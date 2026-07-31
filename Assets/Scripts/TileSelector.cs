using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class WeightedTile
{
    public TileBase tile;
    [Range(0f, 1f)] public float threshold;
}

[System.Serializable]
public class ZoneData
{
    public Tilemap tilemap;
    public FarmZoneAsset zoneAsset;
    [HideInInspector] public bool isUnlocked;
    [HideInInspector] public BoundsInt cachedBounds;
}

public class TileSelector : MonoBehaviour
{
    public static TileSelector instance;
    public Grid grid;
    public List<ZoneData> zones;

    public List<WeightedTile> weightedTiles;
    public Vector3 tileOffset;
    public float noiseScale = 0.15f;

    private ZoneTileData[] zoneTileData;
    private List<Vector3> availablePlaces;
    private List<Vector3Int> localPlaces;

    public GameObject plot;
    public GameObject baseTree;
    public GameObject baseAnimal;
    public List<GameObject> plots;
    public List<GameObject> trees;
    public List<GameObject> animals;
    public GameObject plotParent;
    public GameObject treeParent;
    public GameObject animalParent;
    public int plotNum = 0;
    public int treeNum = 0;
    public int animalNum = 0;

    public List<GameObject> debris;
    public GameObject baseDebris;
    public GameObject baseBoundaryDebris;
    public GameObject debrisParent;
    public int debrisNum = 0;
    public Vector3 debrisOffset;

    // Decorations — player-placed static props (fences are the first kind). Kept in a
    // separate list from `debris` since removal/placement flow differs (instant place,
    // queued remove) and only these are persisted by SaveSystem.
    public List<GameObject> decorations = new List<GameObject>();
    public GameObject decorationParent;
    public int decorationNum = 0;

    // Fence asset used for the auto-generated border ring around farmland (see
    // GenerateBorderFence). Player-placed fences use whatever FenceAsset is selected in
    // the Decoration market instead.
    public FenceAsset borderFenceAsset;
    private readonly HashSet<Vector3Int> borderFenceCells = new HashSet<Vector3Int>();

    public List<GameObject> boundaryDebris = new List<GameObject>();
    private int boundaryDebrisNum = 0;
    public int boundaryRingWidthCells = 140;      // how far the ring TILES extend past farmland — keeps the outer edge off-screen
    public int boundaryDebrisFillDepth = 30;      // how far DEBRIS actually spawns past farmland — keep well under boundaryRingWidthCells, the camera never sees the rest
    public float boundaryClearMargin = 3f;        // world-space buffer, in cell widths, between the farmland edge and the first boundary debris
    public int boundarySampleStride = 1;

    // Boundary regeneration is incremental — only the delta between the previous and current
    // farmland extent gets repainted/respawned, so unrelated/unexplored parts of the ring never
    // visibly change when a new zone is unlocked. The noise offset is seeded once and reused so
    // repainting any region always produces the same tiles (no seams, no drift).
    private bool boundaryNoiseSeeded;
    private float boundaryNoiseOffsetX;
    private float boundaryNoiseOffsetY;
    private bool hasPreviousFarmland;
    private BoundsInt previousFarmlandBounds;

    public Tree tree;
    public Animal animal;

    public HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>();

    private Matrix4x4 TileMatrix => Matrix4x4.TRS(tileOffset, Quaternion.identity, Vector3.one);

    void Start()
    {
        instance = this;

        foreach (ZoneData zone in zones)
        {
            if (zone.tilemap != null)
            {
                // CompressBounds recalculates to the minimum rectangle containing painted tiles.
                // Without this, removed tiles leave the bounds stale at their old larger size.
                zone.tilemap.CompressBounds();
                zone.cachedBounds = zone.tilemap.cellBounds;
                string name = zone.zoneAsset != null ? zone.zoneAsset.zoneName : "unnamed";
            }

            zone.isUnlocked = zone.zoneAsset != null && zone.zoneAsset.unlockedByDefault;

            // Clear locked zones so they aren't visible until unlocked.
            if (!zone.isUnlocked)
                zone.tilemap?.ClearAllTiles();
        }

        SetupGrid();
    }

    public DirtTile PlacePlot(Vector3 plotPosition)
    {
        SetGroundTileAtWorldPos(plotPosition, weightedTiles[weightedTiles.Count - 1].tile);
        GameObject tempPlot = (GameObject)Instantiate(plot, plotPosition, transform.rotation);
        tempPlot.name = "Plot: " + plotNum;
        plotNum++;
        tempPlot.SetActive(true);
        tempPlot.transform.SetParent(plotParent.transform);
        plots.Add(tempPlot);
        DirtTile dirtTile = tempPlot.GetComponent<DirtTile>();
        dirtTile.snapPosition = plotPosition;
        dirtTile.previewCells = 4;
        RegisterFootprint(plotPosition, 4);
        return dirtTile;
    }

    private void SetGroundTileAtWorldPos(Vector3 worldPos, TileBase tile)
    {
        Vector3Int cell = new Vector3Int(grid.WorldToCell(worldPos).x, grid.WorldToCell(worldPos).y, 0);
        foreach (ZoneData zone in zones)
        {
            if (!IsFarmableZone(zone)) continue;
            if (zone.tilemap.HasTile(cell))
            {
                zone.tilemap.SetTile(cell, tile);
                return;
            }
        }
    }

    public TreeTile PlantTree(Vector3 mousePosition, Tree t, PlayerInteraction player)
    {
        GameObject tempTree = (GameObject)Instantiate(baseTree, mousePosition, transform.rotation);
        tempTree.name = t.asset.name + " " + treeNum;
        treeNum++;
        tempTree.SetActive(true);
        tempTree.transform.SetParent(treeParent.transform);
        trees.Add(tempTree);
        tree = t;
        player.SetTree(new Tree(t.asset));
        TreeTile treeTile = tempTree.GetComponent<TreeTile>();
        treeTile.tree = t;
        treeTile.UpdateTreeSprite(treeTile);
        int treeCells = PreviewSizeToCells(string.IsNullOrEmpty(t.asset.preview) ? "2x2" : t.asset.preview);
        treeTile.snapPosition = mousePosition;
        treeTile.previewCells = treeCells;
        RegisterFootprint(mousePosition, treeCells);
        return treeTile;
    }

    public AnimalTile PlaceAnimal(Vector3 mousePosition, Animal a, PlayerInteraction player)
    {
        GameObject tempAnimal = (GameObject)Instantiate(a.asset.animalPrefab, mousePosition, transform.rotation);
        tempAnimal.name = a.asset.name + " " + animalNum;
        animalNum++;
        tempAnimal.SetActive(true);
        tempAnimal.transform.SetParent(animalParent.transform);
        animals.Add(tempAnimal);
        animal = a;
        player.SetAnimal(new Animal(a.asset));
        AnimalTile animalTile = tempAnimal.GetComponent<AnimalTile>();
        animalTile.animal = a;
        animalTile.UpdateAnimalSprite(animalTile);
        int animalCells = PreviewSizeToCells(a.asset.preview);
        animalTile.snapPosition = mousePosition;
        animalTile.previewCells = animalCells;
        RegisterFootprint(mousePosition, animalCells);
        return animalTile;
    }

    // Resolves the owning zone's DebrisSetAsset from world position. Used when only a position is
    // known (e.g. restoring saved debris, where no variant/theme is persisted).
    public DebrisTile PlaceDebris(Vector3 position)
    {
        ZoneData zone = FindZoneAtWorldPos(position);
        return PlaceDebris(position, zone?.zoneAsset?.debrisSet);
    }

    public DebrisTile PlaceDebris(Vector3 position, DebrisSetAsset set)
    {
        if (set == null || set.variants == null || set.variants.Count == 0) return null;
        DebrisVariant variant = PickDebrisVariant(set.variants);
        if (variant == null || variant.sprites == null || variant.sprites.Count == 0) return null;

        GameObject prefab = variant.prefabOverride != null ? variant.prefabOverride : baseDebris;
        GameObject tempDebris = (GameObject)Instantiate(prefab, position, transform.rotation);
        DebrisTile debrisTile = tempDebris.GetComponent<DebrisTile>();
        AssignVariantSprites(tempDebris.GetComponent<SpriteRenderer>(), debrisTile.canopyOverlay, variant);
        tempDebris.name = "Debris: " + debrisNum;
        debrisNum++;
        tempDebris.SetActive(true);
        tempDebris.transform.SetParent(debrisParent.transform);
        debris.Add(tempDebris);
        int cells = PreviewSizeToCells(variant.preview);
        debrisTile.snapPosition = position;
        debrisTile.previewCells = cells;
        debrisTile.removalCost = variant.removalCost;
        RegisterFootprint(position, cells);
        return debrisTile;
    }

    private static DebrisVariant PickDebrisVariant(List<DebrisVariant> variants)
    {
        float roll = Random.value;
        for (int i = 0; i < variants.Count; i++)
            if (roll <= variants[i].threshold)
                return variants[i];
        return variants[variants.Count - 1];
    }

    // Picks one trunk sprite and, if a canopy sprite exists at the same index, its paired
    // canopy sprite too. canopy may be null (prefab not yet wired for the split) or the variant
    // may not have a canopy entry at that index (art not split yet) — both fall back gracefully.
    private static void AssignVariantSprites(SpriteRenderer main, SpriteRenderer canopy, DebrisVariant variant)
    {
        int index = Random.Range(0, variant.sprites.Count);
        main.sprite = variant.sprites[index];
        if (canopy != null)
            canopy.sprite = (variant.canopySprites != null && index < variant.canopySprites.Count)
                ? variant.canopySprites[index]
                : null;
    }

    // Places a player-purchased decoration (e.g. a fence) instantly — no worker queue,
    // matching how debris is spawned rather than how trees/animals are planted, since a
    // decoration is a static prop with no growth state.
    public DecorationTile PlaceDecoration(Vector3 position, DecorationAsset asset)
    {
        GameObject tempDecoration = (GameObject)Instantiate(asset.prefab, position, transform.rotation);
        tempDecoration.name = asset.decorationName + ": " + decorationNum;
        decorationNum++;
        tempDecoration.SetActive(true);
        tempDecoration.transform.SetParent(decorationParent.transform);

        DecorationTile decorationTile = tempDecoration.GetComponent<DecorationTile>();
        decorationTile.asset = asset;
        decorationTile.snapPosition = position;
        decorationTile.previewCells = PreviewSizeToCells(asset.preview);
        decorations.Add(tempDecoration);
        RegisterFootprint(position, decorationTile.previewCells);

        if (decorationTile is FenceTile fenceTile)
            FenceManager.instance.RegisterFence(position, fenceTile);

        return decorationTile;
    }

    public void DestroyAllDecorations()
    {
        foreach (GameObject d in decorations)
        {
            if (d == null) continue;
            if (d.TryGetComponent(out FenceTile fenceTile))
                FenceManager.instance.UnregisterFence(fenceTile.cellCoord);
            Destroy(d);
        }
        decorations.Clear();
        decorationNum = 0;
    }

    public void SpawnDebrisOnMap()
    {
        foreach (ZoneData zone in zones)
        {
            if (!IsFarmableZone(zone)) continue;
            SpawnDebrisOnZone(zone.zoneAsset.zoneName);
        }
    }

    public void SpawnDebrisOnZone(string zoneName)
    {
        ZoneData zone = FindZone(zoneName);
        if (zone == null || zone.tilemap == null || zone.zoneAsset == null) return;

        DebrisSetAsset set = zone.zoneAsset.debrisSet;
        if (set == null || set.variants == null || set.variants.Count == 0) return;

        ZoneTileData zd = FindZoneTileData(zoneName);
        if (zd == null || zd.tileIndices.Length == 0) return;

        var centers = new List<Vector3>();
        for (int i = 0; i < zd.cellX.Length; i++)
            centers.Add(zone.tilemap.GetCellCenterWorld(new Vector3Int(zd.cellX[i], zd.cellY[i], 0)));

        Vector3 zoneCenter = Vector3.zero;
        foreach (Vector3 p in centers) zoneCenter += p;
        zoneCenter /= centers.Count;

        foreach (Vector3 pos in centers)
        {
            if (Vector3.Distance(pos, zoneCenter) < set.clearRadius) continue;
            if (Random.value > set.fillChance) continue;
            PlaceDebris(pos, set);
        }
    }

    // Called by GameHandler.NewGame — generates all zones and applies to unlocked ones.
    public void GenerateAllZones()
    {
        ResetBoundaryState();

        float offsetX = Random.Range(0f, 9999f);
        float offsetY = Random.Range(0f, 9999f);

        zoneTileData = new ZoneTileData[zones.Count];
        for (int i = 0; i < zones.Count; i++)
        {
            ZoneData zone = zones[i];
            string name = zone.zoneAsset != null ? zone.zoneAsset.zoneName : "NULL";

            if (zone.zoneAsset == null || zone.zoneAsset.isBoundary || zone.cachedBounds.size.x == 0)
            {
                continue;
            }

            zoneTileData[i] = MapGenerator.Generate(
                zone.zoneAsset.zoneName,
                zone.cachedBounds,
                weightedTiles,
                noiseScale,
                offsetX,
                offsetY
            );

            if (zone.isUnlocked)
                MapGenerator.Apply(zone.tilemap, zoneTileData[i], weightedTiles, TileMatrix, GameHandler.instance.randomTileRotation);
        }

        SetupGrid();
        MapController.instance.RefreshBounds(zones);
        RepositionBoundary();
    }

    // Called by GameHandler.LoadData — restores saved tile data to all unlocked zones.
    public void LoadZoneTileData(ZoneTileData[] data)
    {
        ResetBoundaryState();
        zoneTileData = data;

        foreach (ZoneData zone in zones)
            zone.tilemap?.ClearAllTiles();

        if (zoneTileData != null)
        {
            foreach (ZoneData zone in zones)
            {
                if (!zone.isUnlocked || (zone.zoneAsset != null && zone.zoneAsset.isBoundary)) continue;
                ZoneTileData zd = FindZoneTileData(zone.zoneAsset?.zoneName);
                if (zd != null)
                    MapGenerator.Apply(zone.tilemap, zd, weightedTiles, TileMatrix, GameHandler.instance.randomTileRotation);
            }
        }

        SetupGrid();
        MapController.instance.RefreshBounds(zones);
        RepositionBoundary();
    }

    public bool HasTileAtWorldPos(Vector3 worldPos)
    {
        // Use z=0 and each tilemap's own WorldToCell so the lookup matches
        // the coordinate system the tiles were stored in (avoids z=9 skew and
        // any local offset between the Grid object and individual Tilemaps).
        Vector3 flatPos = new Vector3(worldPos.x, worldPos.y, 0f);
        foreach (ZoneData zone in zones)
        {
            if (!IsFarmableZone(zone)) continue;
            Vector3Int cell = zone.tilemap.WorldToCell(flatPos);
            if (zone.tilemap.HasTile(cell)) return true;
        }
        return false;
    }

    public ZoneTileData[] GetZoneTileData() => zoneTileData;

    // Sets unlock flags from save data. Call before LoadZoneTileData so flags are ready.
    public void SetZoneUnlocks(string[] unlockedZoneNames)
    {
        foreach (ZoneData zone in zones)
            zone.isUnlocked = zone.zoneAsset != null && zone.zoneAsset.unlockedByDefault;

        if (unlockedZoneNames != null)
            foreach (string name in unlockedZoneNames)
                foreach (ZoneData zone in zones)
                    if (zone.zoneAsset != null && zone.zoneAsset.zoneName == name)
                        zone.isUnlocked = true;
    }

    // Unlocks a zone mid-game and paints its pre-generated tiles.
    public void UnlockZone(string zoneName)
    {
        ZoneData zone = FindZone(zoneName);
        if (zone == null) return;

        zone.isUnlocked = true;
        ZoneTileData zd = FindZoneTileData(zoneName);
        if (zd != null)
            MapGenerator.Apply(zone.tilemap, zd, weightedTiles, TileMatrix, GameHandler.instance.randomTileRotation);
        SetupGrid();
        SpawnDebrisOnZone(zoneName);
        MapController.instance.RefreshBounds(zones);
        RepositionBoundary();
    }

    public string[] GetUnlockedZoneNames()
    {
        var names = new List<string>();
        foreach (ZoneData zone in zones)
            if (zone.isUnlocked && zone.zoneAsset != null)
                names.Add(zone.zoneAsset.zoneName);
        return names.ToArray();
    }

    private ZoneTileData FindZoneTileData(string zoneName)
    {
        if (zoneTileData == null || zoneName == null) return null;
        foreach (ZoneTileData zd in zoneTileData)
            if (zd != null && zd.zoneName == zoneName)
                return zd;
        return null;
    }

    private static bool IsFarmableZone(ZoneData zone) =>
        zone.isUnlocked && zone.tilemap != null && zone.zoneAsset != null && !zone.zoneAsset.isBoundary;

    private ZoneData FindZone(string zoneName)
    {
        foreach (ZoneData z in zones)
            if (z.zoneAsset != null && z.zoneAsset.zoneName == zoneName) return z;
        return null;
    }

    private ZoneData FindZoneAtWorldPos(Vector3 worldPos)
    {
        Vector3 flatPos = new Vector3(worldPos.x, worldPos.y, 0f);
        foreach (ZoneData zone in zones)
        {
            if (!IsFarmableZone(zone)) continue;
            if (zone.tilemap.HasTile(zone.tilemap.WorldToCell(flatPos))) return zone;
        }
        return null;
    }

    // Re-anchors and repaints the boundary ring so it hugs just outside the current farmland
    // frontier. Called whenever zone generation/unlocking changes the farmland extent.
    // Only the delta between the previous and current farmland extent is touched — tiles and
    // debris in unrelated/unexplored parts of the ring are left completely alone, both so nothing
    // visibly pops when a distant zone is unlocked, and so a future "whisk away" animation has a
    // stable, addressable set of objects to work with instead of a full destroy-and-rebuild.
    public void RepositionBoundary()
    {
        ZoneData boundary = null;
        foreach (ZoneData z in zones)
            if (z.zoneAsset != null && z.zoneAsset.isBoundary) { boundary = z; break; }
        if (boundary == null || boundary.tilemap == null) return;

        BoundsInt farmland = GetFarmlandCellBounds();
        if (farmland.size.x == 0) return;

        if (!boundaryNoiseSeeded)
        {
            boundaryNoiseOffsetX = Random.Range(0f, 9999f);
            boundaryNoiseOffsetY = Random.Range(0f, 9999f);
            boundaryNoiseSeeded = true;
        }

        BoundsInt ring = ExpandBounds(farmland, boundaryRingWidthCells);
        boundary.cachedBounds = ring;

        // Paint only cells outside the previously-painted ring (empty on the first call, so that
        // one paints everything). Cells inside it already have the right tiles — don't touch them.
        BoundsInt tileExclude = hasPreviousFarmland ? ExpandBounds(previousFarmlandBounds, boundaryRingWidthCells) : default;
        ZoneTileData deltaTiles = GenerateBoundaryDelta(boundary.zoneAsset.zoneName, ring, tileExclude);
        MapGenerator.Apply(boundary.tilemap, deltaTiles, weightedTiles, TileMatrix, GameHandler.instance.randomTileRotation);

        // Any existing boundary debris that now falls inside the (possibly larger) farmland clear
        // zone belongs to a region that just became farmland — remove it. (Future hook: replace
        // this instant destroy with an animated "whisk away" pass.)
        RemoveBoundaryDebrisInsideClearZone(boundary, farmland);

        // Debris only needs to cover a shallow band near the visible frontier, not the full ring,
        // and only the newly-exposed part of that band — already-populated area is left alone.
        BoundsInt fillBounds = ClampBounds(ExpandBounds(farmland, boundaryDebrisFillDepth), ring);
        BoundsInt fillExclude = hasPreviousFarmland ? ClampBounds(ExpandBounds(previousFarmlandBounds, boundaryDebrisFillDepth), ring) : default;
        SpawnBoundaryDebris(boundary, fillBounds, fillExclude, farmland);

        GenerateBorderFence(boundary);

        previousFarmlandBounds = farmland;
        hasPreviousFarmland = true;
    }

    // Stamps a connected fence line immediately outside the farmland footprint, using the
    // same FenceManager registry/masking as manually placed decorative fences — a border
    // segment and a player-placed one sitting next to each other connect for free. Recomputed
    // in full from current farmland shape each call (farmland only changes on zone unlock/load,
    // never per-frame, so no incremental delta tracking is needed here unlike the tile/debris
    // painting above).
    private void GenerateBorderFence(ZoneData boundary)
    {
        if (borderFenceAsset == null || borderFenceAsset.prefab == null) return;

        var farmlandCells = new HashSet<Vector3Int>();
        foreach (ZoneData zone in zones)
        {
            if (!IsFarmableZone(zone)) continue;
            for (int x = zone.cachedBounds.xMin; x < zone.cachedBounds.xMax; x++)
            {
                for (int y = zone.cachedBounds.yMin; y < zone.cachedBounds.yMax; y++)
                {
                    Vector3Int cell = new Vector3Int(x, y, 0);
                    if (zone.tilemap.HasTile(cell)) farmlandCells.Add(cell);
                }
            }
        }

        var required = new HashSet<Vector3Int>();
        foreach (Vector3Int cell in farmlandCells)
        {
            bool missingUp = !farmlandCells.Contains(cell + Vector3Int.up);
            bool missingRight = !farmlandCells.Contains(cell + Vector3Int.right);
            bool missingDown = !farmlandCells.Contains(cell + Vector3Int.down);
            bool missingLeft = !farmlandCells.Contains(cell + Vector3Int.left);

            if (missingUp) required.Add(cell + Vector3Int.up);
            if (missingRight) required.Add(cell + Vector3Int.right);
            if (missingDown) required.Add(cell + Vector3Int.down);
            if (missingLeft) required.Add(cell + Vector3Int.left);

            // Corner fill: an outer convex corner of the farmland shape is missing two
            // *adjacent* orthogonal neighbors. The two border fences placed above end up
            // diagonal to each other and won't auto-connect through the N/E/S/W mask,
            // leaving a one-cell gap — fence the diagonal cell too to close it.
            if (missingUp && missingRight) required.Add(cell + Vector3Int.up + Vector3Int.right);
            if (missingRight && missingDown) required.Add(cell + Vector3Int.right + Vector3Int.down);
            if (missingDown && missingLeft) required.Add(cell + Vector3Int.down + Vector3Int.left);
            if (missingLeft && missingUp) required.Add(cell + Vector3Int.left + Vector3Int.up);
        }

        // Remove border fences that are no longer required (a zone unlock can turn a former edge into interior).
        foreach (Vector3Int cell in new List<Vector3Int>(borderFenceCells))
        {
            if (required.Contains(cell)) continue;
            FenceTile existing = FenceManager.instance.GetFenceAt(cell);
            FenceManager.instance.UnregisterFence(cell);
            if (existing != null)
            {
                UnregisterFootprint(existing.snapPosition, 4);
                Destroy(existing.gameObject);
            }
            borderFenceCells.Remove(cell);
        }

        // Place newly required border fences.
        foreach (Vector3Int cell in required)
        {
            if (borderFenceCells.Contains(cell)) continue;         // already placed
            if (farmlandCells.Contains(cell)) continue;             // safety: never fence over farmland
            if (FenceManager.instance.HasFenceAt(cell))             // a player fence already sits here
            {
                borderFenceCells.Add(cell);
                continue;
            }

            Vector3 worldPos = boundary.tilemap.GetCellCenterWorld(cell);
            GameObject go = Instantiate(borderFenceAsset.prefab, worldPos, transform.rotation);
            go.name = "BorderFence: " + cell;
            go.transform.SetParent(decorationParent.transform);

            FenceTile tile = go.GetComponent<FenceTile>();
            tile.asset = borderFenceAsset;
            tile.snapPosition = worldPos;
            tile.previewCells = 4;
            tile.isBorderFence = true;
            FenceManager.instance.RegisterFenceAtCell(cell, tile);
            // Border ring sits outside farmland, but WorkerCharacter still paths through the
            // boundary zone (e.g. to clear boundary debris) — block it here the same way a
            // player-placed fence blocks it via PlaceDecoration's RegisterFootprint.
            RegisterFootprint(worldPos, 4);
            borderFenceCells.Add(cell);
        }
    }

    // Destroys all boundary debris and resets incremental tracking. Call whenever the boundary
    // tilemap itself gets wiped out from under that tracking (new game, load) so the next
    // RepositionBoundary() call does a full fresh paint instead of assuming stale prior state.
    private void ResetBoundaryState()
    {
        foreach (GameObject d in boundaryDebris)
            if (d != null) Destroy(d);
        boundaryDebris.Clear();

        foreach (Vector3Int cell in borderFenceCells)
        {
            FenceTile tile = FenceManager.instance?.GetFenceAt(cell);
            if (tile != null)
            {
                UnregisterFootprint(tile.snapPosition, 4);
                Destroy(tile.gameObject);
            }
            FenceManager.instance?.UnregisterFence(cell);
        }
        borderFenceCells.Clear();

        hasPreviousFarmland = false;
        boundaryNoiseSeeded = false;
    }

    private static BoundsInt ExpandBounds(BoundsInt b, int margin) =>
        new BoundsInt(b.xMin - margin, b.yMin - margin, 0, b.size.x + margin * 2, b.size.y + margin * 2, 1);

    private static BoundsInt ClampBounds(BoundsInt b, BoundsInt clampTo)
    {
        int xMin = Mathf.Max(b.xMin, clampTo.xMin);
        int yMin = Mathf.Max(b.yMin, clampTo.yMin);
        int xMax = Mathf.Min(b.xMax, clampTo.xMax);
        int yMax = Mathf.Min(b.yMax, clampTo.yMax);
        return new BoundsInt(xMin, yMin, 0, Mathf.Max(0, xMax - xMin), Mathf.Max(0, yMax - yMin), 1);
    }

    private BoundsInt GetFarmlandCellBounds()
    {
        bool first = true;
        int xMin = 0, xMax = 0, yMin = 0, yMax = 0;
        foreach (ZoneData zone in zones)
        {
            if (!IsFarmableZone(zone)) continue;
            BoundsInt b = zone.cachedBounds;
            if (first)
            {
                xMin = b.xMin; xMax = b.xMax; yMin = b.yMin; yMax = b.yMax;
                first = false;
            }
            else
            {
                xMin = Mathf.Min(xMin, b.xMin);
                xMax = Mathf.Max(xMax, b.xMax);
                yMin = Mathf.Min(yMin, b.yMin);
                yMax = Mathf.Max(yMax, b.yMax);
            }
        }
        return first ? new BoundsInt() : new BoundsInt(xMin, yMin, 0, xMax - xMin, yMax - yMin, 1);
    }

    // Buffers the farmland exclusion outward in world space (not whole cells) so fractional
    // margins like 0.5 are meaningful — tree sprites are visually taller/wider than one cell and
    // would otherwise overhang into the playable area right at the edge.
    private void GetClearZoneWorldBounds(ZoneData boundary, BoundsInt farmland, out float minX, out float minY, out float maxX, out float maxY)
    {
        Vector3 farmlandMin = boundary.tilemap.CellToWorld(new Vector3Int(farmland.xMin, farmland.yMin, 0));
        Vector3 farmlandMax = boundary.tilemap.CellToWorld(new Vector3Int(farmland.xMax, farmland.yMax, 0));
        float marginWorld = boundaryClearMargin * grid.cellSize.x;
        minX = farmlandMin.x - marginWorld;
        minY = farmlandMin.y - marginWorld;
        maxX = farmlandMax.x + marginWorld;
        maxY = farmlandMax.y + marginWorld;
    }

    // Builds tile data for cells in `area` that aren't inside `exclude`, using the stable boundary
    // noise offset so repainting any region always produces the same tiles as before (no seams).
    private ZoneTileData GenerateBoundaryDelta(string zoneName, BoundsInt area, BoundsInt exclude)
    {
        var cellXList = new List<int>();
        var cellYList = new List<int>();
        var indexList = new List<int>();

        for (int y = area.yMin; y < area.yMax; y++)
        {
            for (int x = area.xMin; x < area.xMax; x++)
            {
                if (x >= exclude.xMin && x < exclude.xMax && y >= exclude.yMin && y < exclude.yMax) continue;
                float noise = Mathf.PerlinNoise(x * noiseScale + boundaryNoiseOffsetX, y * noiseScale + boundaryNoiseOffsetY);
                cellXList.Add(x);
                cellYList.Add(y);
                indexList.Add(MapGenerator.PickTile(noise, weightedTiles));
            }
        }

        return new ZoneTileData
        {
            zoneName = zoneName,
            cellX = cellXList.ToArray(),
            cellY = cellYList.ToArray(),
            tileIndices = indexList.ToArray()
        };
    }

    // Removes any boundary debris whose position now falls inside the farmland clear zone —
    // it just became farmland and should no longer be dressed as unreachable boundary forest.
    private void RemoveBoundaryDebrisInsideClearZone(ZoneData boundary, BoundsInt farmland)
    {
        GetClearZoneWorldBounds(boundary, farmland, out float clearMinX, out float clearMinY, out float clearMaxX, out float clearMaxY);

        for (int i = boundaryDebris.Count - 1; i >= 0; i--)
        {
            GameObject go = boundaryDebris[i];
            if (go == null) { boundaryDebris.RemoveAt(i); continue; }

            Vector3 p = go.transform.position;
            if (p.x >= clearMinX && p.x < clearMaxX && p.y >= clearMinY && p.y < clearMaxY)
            {
                Destroy(go);
                boundaryDebris.RemoveAt(i);
            }
        }
    }

    // Samples the given fill area on a coarse grid (skipping `fillExclude`, already-populated area,
    // and a padded farmland hole) so instance count stays bounded regardless of area size. Each
    // candidate lands on a random cell within its stride block plus a sub-cell jitter, so the
    // result doesn't read as a rigid lattice. Purely decorative — no footprint registration.
    private void SpawnBoundaryDebris(ZoneData boundary, BoundsInt fillBounds, BoundsInt fillExclude, BoundsInt farmland)
    {
        DebrisSetAsset set = boundary.zoneAsset.debrisSet;
        if (set == null || set.variants == null || set.variants.Count == 0) return;

        GetClearZoneWorldBounds(boundary, farmland, out float clearMinX, out float clearMinY, out float clearMaxX, out float clearMaxY);

        for (int x = fillBounds.xMin; x < fillBounds.xMax; x += boundarySampleStride)
        {
            for (int y = fillBounds.yMin; y < fillBounds.yMax; y += boundarySampleStride)
            {
                if (x >= fillExclude.xMin && x < fillExclude.xMax && y >= fillExclude.yMin && y < fillExclude.yMax) continue;

                int cellX = x + Random.Range(0, boundarySampleStride);
                int cellY = y + Random.Range(0, boundarySampleStride);
                Vector3 cellCenter = boundary.tilemap.GetCellCenterWorld(new Vector3Int(cellX, cellY, 0));
                Vector2 jitter = Random.insideUnitCircle * grid.cellSize.x * 0.5f;
                Vector3 pos = new Vector3(cellCenter.x + jitter.x, cellCenter.y + jitter.y, 0f);

                if (pos.x >= clearMinX && pos.x < clearMaxX && pos.y >= clearMinY && pos.y < clearMaxY) continue;
                if (Random.value > set.fillChance) continue;

                PlaceBoundaryDebris(pos, set);
            }
        }
    }

    private void PlaceBoundaryDebris(Vector3 position, DebrisSetAsset set)
    {
        DebrisVariant variant = PickDebrisVariant(set.variants);
        if (variant == null || variant.sprites == null || variant.sprites.Count == 0) return;

        GameObject prefab = variant.prefabOverride != null ? variant.prefabOverride : baseBoundaryDebris;
        if (prefab == null) return;

        GameObject go = Instantiate(prefab, position, transform.rotation);
        AssignVariantSprites(go.GetComponent<SpriteRenderer>(), go.GetComponent<DebrisTile>()?.canopyOverlay, variant);
        go.name = "BoundaryDebris: " + boundaryDebrisNum;
        boundaryDebrisNum++;
        go.transform.SetParent(debrisParent.transform);
        boundaryDebris.Add(go);
    }

    public void RegisterFootprint(Vector3 snapPos, int objCells)
    {
        Vector3Int origin = GetFineCellOrigin(snapPos, objCells);
        for (int dx = 0; dx < objCells; dx++)
            for (int dy = 0; dy < objCells; dy++)
                occupiedCells.Add(new Vector3Int(origin.x + dx, origin.y + dy, 0));
    }

    public void UnregisterFootprint(Vector3 snapPos, int objCells)
    {
        Vector3Int origin = GetFineCellOrigin(snapPos, objCells);
        for (int dx = 0; dx < objCells; dx++)
            for (int dy = 0; dy < objCells; dy++)
                occupiedCells.Remove(new Vector3Int(origin.x + dx, origin.y + dy, 0));
    }

    public bool IsFootprintClear(Vector3 snapPos, int objCells)
    {
        Vector3Int origin = GetFineCellOrigin(snapPos, objCells);
        for (int dx = 0; dx < objCells; dx++)
            for (int dy = 0; dy < objCells; dy++)
                if (occupiedCells.Contains(new Vector3Int(origin.x + dx, origin.y + dy, 0)))
                    return false;
        return true;
    }

    public void ClearAllFootprints() => occupiedCells.Clear();

    public static int PreviewSizeToCells(string previewSize) =>
        previewSize == "4x4" ? 4 : previewSize == "2x2" ? 2 : 1;

    // World position -> fine cell coordinate (finest granularity, i.e. what a 1x1 occupant like a character occupies).
    public Vector3Int WorldToFineCell(Vector3 worldPos) => GetFineCellOrigin(worldPos, 1);

    // Fine cell coordinate -> world center position (inverse of GetFineCellOrigin with objCells=1).
    public Vector3 FineCellToWorld(Vector3Int fineCell)
    {
        const int BASE = 4;
        int cellX = Mathf.FloorToInt(fineCell.x / (float)BASE);
        int cellY = Mathf.FloorToInt(fineCell.y / (float)BASE);
        int kx = fineCell.x - cellX * BASE;
        int ky = fineCell.y - cellY * BASE;
        Vector3 cellCenter = grid.GetCellCenterWorld(new Vector3Int(cellX, cellY, 0));
        float subX = (kx + 0.5f) / BASE - 0.5f;
        float subY = (ky + 0.5f) / BASE - 0.5f;
        return new Vector3(cellCenter.x + subX * grid.cellSize.x, cellCenter.y + subY * grid.cellSize.y, 0f);
    }

    // A fine cell is walkable if it isn't occupied by a placed object and sits on real ground.
    public bool IsFineCellWalkable(Vector3Int fineCell)
    {
        if (occupiedCells.Contains(fineCell)) return false;
        return HasTileAtWorldPos(FineCellToWorld(fineCell));
    }

    // Converts a world snap position to the fine-cell origin of the object's footprint.
    // Each grid cell is divided into a 4x4 grid of fine cells (BASE=4).
    // objCells=4 → 1 position/cell (4x4 object), objCells=2 → 4 positions, objCells=1 → 16 positions.
    public Vector3Int GetFineCellOrigin(Vector3 worldPos, int objCells)
    {
        const int BASE = 4;
        int N_sub = BASE / objCells;

        Vector3Int oldCell = grid.WorldToCell(new Vector3(worldPos.x, worldPos.y, 0f));
        Vector3 oldCenter = grid.GetCellCenterWorld(oldCell);

        float offsetX = worldPos.x - oldCenter.x;
        float offsetY = worldPos.y - oldCenter.y;

        // Quantize X and Y independently into N_sub subdivisions per cell
        float normX = offsetX / grid.cellSize.x;
        float normY = offsetY / grid.cellSize.y;
        int kx = Mathf.Clamp(Mathf.FloorToInt((normX + 0.5f) * N_sub), 0, N_sub - 1);
        int ky = Mathf.Clamp(Mathf.FloorToInt((normY + 0.5f) * N_sub), 0, N_sub - 1);

        return new Vector3Int(
            oldCell.x * BASE + kx * objCells,
            oldCell.y * BASE + ky * objCells,
            0);
    }

    private void SetupGrid()
    {
        availablePlaces = new List<Vector3>();
        localPlaces = new List<Vector3Int>();

        foreach (ZoneData zone in zones)
        {
            if (!zone.isUnlocked || zone.tilemap == null) continue;
            Tilemap tilemap = zone.tilemap;
            for (int xx = tilemap.cellBounds.xMin; xx < tilemap.cellBounds.xMax; xx++)
            {
                for (int yy = tilemap.cellBounds.yMin; yy < tilemap.cellBounds.yMax; yy++)
                {
                    Vector3Int cellCoord = new Vector3Int(xx, yy, 0);
                    if (tilemap.HasTile(cellCoord))
                    {
                        availablePlaces.Add(tilemap.CellToWorld(cellCoord));
                        localPlaces.Add(cellCoord);
                    }
                }
            }
        }
    }
}
