using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class MapGenerator
{
    // Generates tile data for a zone using Perlin noise. Does not modify the tilemap.
    public static ZoneTileData Generate(string zoneName, BoundsInt bounds, List<WeightedTile> tiles, float noiseScale, float offsetX, float offsetY)
    {
        var cellXList   = new List<int>();
        var cellYList   = new List<int>();
        var indexList   = new List<int>();

        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                float noise = Mathf.PerlinNoise(x * noiseScale + offsetX, y * noiseScale + offsetY);
                cellXList.Add(x);
                cellYList.Add(y);
                indexList.Add(PickTile(noise, tiles));
            }
        }

        return new ZoneTileData
        {
            zoneName    = zoneName,
            cellX       = cellXList.ToArray(),
            cellY       = cellYList.ToArray(),
            tileIndices = indexList.ToArray()
        };
    }

    // Paints a previously generated ZoneTileData onto a tilemap.
    // tileMatrix is optional — when provided it overrides the per-cell transform on every tile
    // so that offsets set via Grid Selection Properties survive ClearAllTiles + re-apply cycles.
    public static void Apply(Tilemap tilemap, ZoneTileData data, List<WeightedTile> tiles, Matrix4x4? tileMatrix = null)
    {
        for (int i = 0; i < data.tileIndices.Length; i++)
        {
            int idx = data.tileIndices[i];
            if (idx >= 0 && idx < tiles.Count)
            {
                Vector3Int cell = new Vector3Int(data.cellX[i], data.cellY[i], 0);
                tilemap.SetTile(cell, tiles[idx].tile);
                if (tileMatrix.HasValue)
                    tilemap.SetTransformMatrix(cell, tileMatrix.Value);
            }
        }
    }

    // Returns the index of the first tile whose threshold >= noise value.
    // Tiles should be ordered by threshold ascending in the inspector.
    private static int PickTile(float noise, List<WeightedTile> tiles)
    {
        for (int i = 0; i < tiles.Count; i++)
            if (noise <= tiles[i].threshold)
                return i;
        return tiles.Count - 1;
    }
}
