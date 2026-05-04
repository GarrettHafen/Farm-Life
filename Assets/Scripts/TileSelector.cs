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
    public List<Sprite> debrisImages;
    public GameObject baseDebris;
    public GameObject debrisParent;
    public int debrisNum = 0;
    [Range(0f, 1f)] public float debrisFillChance = 0.7f;
    public float debrisClearRadius = 1.5f;
    public Vector3 debrisOffset;

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

    public DirtTile PlacePlot(Vector3 plotPosition, Vector3 offset)
    {
        Vector3 snapPos = plotPosition;
        plotPosition.y -= offset.y;
        SetGroundTileAtWorldPos(plotPosition, weightedTiles[weightedTiles.Count - 1].tile);
        GameObject tempPlot = (GameObject)Instantiate(plot, plotPosition, transform.rotation);
        tempPlot.name = "Plot: " + plotNum;
        plotNum++;
        tempPlot.SetActive(true);
        tempPlot.transform.SetParent(plotParent.transform);
        plots.Add(tempPlot);
        DirtTile dirtTile = tempPlot.GetComponent<DirtTile>();
        dirtTile.snapPosition = snapPos;
        dirtTile.previewCells = 4;
        RegisterFootprint(snapPos, 4);
        return dirtTile;
    }

    private void SetGroundTileAtWorldPos(Vector3 worldPos, TileBase tile)
    {
        Vector3Int cell = new Vector3Int(grid.WorldToCell(worldPos).x, grid.WorldToCell(worldPos).y, 0);
        foreach (ZoneData zone in zones)
        {
            if (!zone.isUnlocked || zone.tilemap == null) continue;
            if (zone.tilemap.HasTile(cell))
            {
                zone.tilemap.SetTile(cell, tile);
                return;
            }
        }
    }

    public TreeTile PlantTree(Vector3 mousePosition, Tree t, PlayerInteraction player, Vector3 offset)
    {
        Vector3 snapPos = mousePosition;
        mousePosition.y += offset.y;
        mousePosition.x += offset.x;
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
        treeTile.snapPosition = snapPos;
        treeTile.previewCells = treeCells;
        RegisterFootprint(snapPos, treeCells);
        return treeTile;
    }

    public AnimalTile PlaceAnimal(Vector3 mousePosition, Animal a, PlayerInteraction player)
    {
        Vector3 snapPos = mousePosition;
        mousePosition.y += a.asset.placementOffset.y;
        mousePosition.x += a.asset.placementOffset.x;
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
        animalTile.snapPosition = snapPos;
        animalTile.previewCells = animalCells;
        RegisterFootprint(snapPos, animalCells);
        return animalTile;
    }

    public DebrisTile PlaceDebris(Vector3 position, Vector3 offset)
    {
        position.y -= offset.y;
        Vector3 snapPos = position;  // footprint must match the actual instantiation position
        GameObject tempDebris = (GameObject)Instantiate(baseDebris, position, transform.rotation);
        tempDebris.GetComponent<SpriteRenderer>().sprite = debrisImages[Random.Range(0, debrisImages.Count)];
        tempDebris.name = "Debris: " + debrisNum;
        debrisNum++;
        tempDebris.SetActive(true);
        tempDebris.transform.SetParent(debrisParent.transform);
        debris.Add(tempDebris);
        DebrisTile debrisTile = tempDebris.GetComponent<DebrisTile>();
        debrisTile.snapPosition = snapPos;
        debrisTile.previewCells = 1;
        RegisterFootprint(snapPos, 1);
        return debrisTile;
    }

    public void SpawnDebrisOnMap()
    {
        // Spawn at exact cell centers (no plotOffset) so the debris collider lines up
        // with the preview ghost, which also sits at the unmodified cell center.
        // Visual alignment is handled by the debris sprite pivot instead.

        var centers = CollectTileCenters();
        if (centers.Count == 0) return;

                Vector3 mapCenter = Vector3.zero;
        foreach (Vector3 p in centers) mapCenter += p;
        mapCenter /= centers.Count;

        foreach (Vector3 pos in centers)
        {
            if (Vector3.Distance(pos, mapCenter) < debrisClearRadius) continue;
            if (Random.value > debrisFillChance) continue;
            PlaceDebris(new Vector3(pos.x, pos.y, 9f), debrisOffset);
        }

        // DEBUG: spawn one debris on the bottom-most tile only
        /* Vector3 bottom = centers[0];
        foreach (Vector3 p in centers)
            if (p.y < bottom.y) bottom = p;
        PlaceDebris(new Vector3(bottom.x, bottom.y, 9f), debrisOffset); */
    }

    public void SpawnDebrisOnZone(string zoneName)
    {

        ZoneTileData zd = FindZoneTileData(zoneName);
        if (zd == null || zd.tileIndices.Length == 0) return;

        ZoneData zone = null;
        foreach (ZoneData z in zones)
            if (z.zoneAsset != null && z.zoneAsset.zoneName == zoneName) { zone = z; break; }
        if (zone == null || zone.tilemap == null) return;

        var centers = new List<Vector3>();
        for (int i = 0; i < zd.cellX.Length; i++)
            centers.Add(zone.tilemap.GetCellCenterWorld(new Vector3Int(zd.cellX[i], zd.cellY[i], 0)));

        Vector3 mapCenter = Vector3.zero;
        foreach (Vector3 p in centers) mapCenter += p;
        mapCenter /= centers.Count;

        foreach (Vector3 pos in centers)
        {
            if (Vector3.Distance(pos, mapCenter) < debrisClearRadius) continue;
            if (Random.value > debrisFillChance) continue;
            PlaceDebris(new Vector3(pos.x, pos.y, 9f), debrisOffset);
        }
    }

    private List<Vector3> CollectTileCenters()
    {
        var result = new List<Vector3>();
        foreach (ZoneData zone in zones)
        {
            if (!zone.isUnlocked || zone.tilemap == null) continue;
            Tilemap tm = zone.tilemap;
            for (int xx = tm.cellBounds.xMin; xx < tm.cellBounds.xMax; xx++)
                for (int yy = tm.cellBounds.yMin; yy < tm.cellBounds.yMax; yy++)
                {
                    var cell = new Vector3Int(xx, yy, 0);
                    if (tm.HasTile(cell))
                        result.Add(tm.GetCellCenterWorld(cell));
                }
        }
        return result;
    }

    // Called by GameHandler.NewGame — generates all zones and applies to unlocked ones.
    public void GenerateAllZones()
    {
        float offsetX = Random.Range(0f, 9999f);
        float offsetY = Random.Range(0f, 9999f);

        zoneTileData = new ZoneTileData[zones.Count];
        for (int i = 0; i < zones.Count; i++)
        {
            ZoneData zone = zones[i];
            string name = zone.zoneAsset != null ? zone.zoneAsset.zoneName : "NULL";

            if (zone.zoneAsset == null || zone.cachedBounds.size.x == 0)
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
                MapGenerator.Apply(zone.tilemap, zoneTileData[i], weightedTiles, TileMatrix);
        }

        SetupGrid();
        MapController.instance.RefreshBounds(zones);
    }

    // Called by GameHandler.LoadData — restores saved tile data to all unlocked zones.
    public void LoadZoneTileData(ZoneTileData[] data)
    {
        zoneTileData = data;

        foreach (ZoneData zone in zones)
            zone.tilemap?.ClearAllTiles();

        if (zoneTileData != null)
        {
            foreach (ZoneData zone in zones)
            {
                if (!zone.isUnlocked) continue;
                ZoneTileData zd = FindZoneTileData(zone.zoneAsset?.zoneName);
                if (zd != null)
                    MapGenerator.Apply(zone.tilemap, zd, weightedTiles, TileMatrix);
            }
        }

        SetupGrid();
        MapController.instance.RefreshBounds(zones);
    }

    public bool HasTileAtWorldPos(Vector3 worldPos)
    {
        // Use z=0 and each tilemap's own WorldToCell so the lookup matches
        // the coordinate system the tiles were stored in (avoids z=9 skew and
        // any local offset between the Grid object and individual Tilemaps).
        Vector3 flatPos = new Vector3(worldPos.x, worldPos.y, 0f);
        foreach (ZoneData zone in zones)
        {
            if (!zone.isUnlocked || zone.tilemap == null) continue;
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
        foreach (ZoneData zone in zones)
        {
            if (zone.zoneAsset == null || zone.zoneAsset.zoneName != zoneName) continue;

            zone.isUnlocked = true;
            ZoneTileData zd = FindZoneTileData(zoneName);
            if (zd != null)
                MapGenerator.Apply(zone.tilemap, zd, weightedTiles, TileMatrix);
            SetupGrid();
            SpawnDebrisOnZone(zoneName);
            MapController.instance.RefreshBounds(zones);
            return;
        }
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

    // Converts a world snap position to the top-left fine-cell index of the object's footprint.
    // Each old cell is divided into a 4x4 grid of fine cells (BASE=4).
    // objCells=4 → 1 position/cell (4x4 object), objCells=2 → 4 positions, objCells=1 → 16 positions.
    private Vector3Int GetFineCellOrigin(Vector3 worldPos, int objCells)
    {
        const int BASE = 4;
        int N_sub = BASE / objCells;

        Vector3Int oldCell = grid.WorldToCell(new Vector3(worldPos.x, worldPos.y, 0f));
        Vector3 oldCenter = grid.GetCellCenterWorld(oldCell);

        float bx = grid.cellSize.x * 0.5f;
        float by = grid.cellSize.y * 0.5f;
        float offsetX = worldPos.x - oldCenter.x;
        float offsetY = worldPos.y - oldCenter.y;

        // Decompose world offset into isometric basis coordinates (alpha, beta ∈ [-0.5, 0.5])
        float alpha = (offsetX / bx + offsetY / by) * 0.5f;
        float beta  = (offsetY / by - offsetX / bx) * 0.5f;

        int k_alpha = Mathf.Clamp(Mathf.FloorToInt((alpha + 0.5f) * N_sub), 0, N_sub - 1);
        int k_beta  = Mathf.Clamp(Mathf.FloorToInt((beta  + 0.5f) * N_sub), 0, N_sub - 1);

        return new Vector3Int(
            oldCell.x * BASE + k_alpha * objCells,
            oldCell.y * BASE + k_beta  * objCells,
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
