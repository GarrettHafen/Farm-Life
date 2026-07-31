using System.Collections.Generic;
using UnityEngine;

// Owns fence adjacency: which coarse grid cells have a fence, and keeping each fence's
// connection sprite in sync with its N/E/S/W neighbors. Doesn't spawn or destroy fence
// GameObjects itself — TileSelector does that (both for player-placed decorative fences
// and for the auto-generated border ring) and calls Register/UnregisterFence around it.
public class FenceManager : MonoBehaviour
{
    public static FenceManager instance;

    // Optional explicit wire-up — if left unassigned, falls back to TileSelector's grid
    // so this doesn't need a second manual Inspector reference to the same Grid object.
    [SerializeField] private Grid grid;
    private Grid Grid => grid != null ? grid : TileSelector.instance.grid;

    private readonly Dictionary<Vector3Int, FenceTile> fences = new Dictionary<Vector3Int, FenceTile>();

    private static readonly Vector3Int[] Neighbors4 =
    {
        Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left
    };

    private void Awake()
    {
        instance = this;
    }

    public bool HasFenceAt(Vector3Int cell) => fences.ContainsKey(cell);

    public FenceTile GetFenceAt(Vector3Int cell) => fences.TryGetValue(cell, out FenceTile tile) ? tile : null;

    public void RegisterFence(Vector3 worldPos, FenceTile tile)
    {
        RegisterFenceAtCell(Grid.WorldToCell(worldPos), tile);
    }

    public void RegisterFenceAtCell(Vector3Int cell, FenceTile tile)
    {
        tile.cellCoord = cell;
        fences[cell] = tile;
        RefreshConnections(cell);
        foreach (Vector3Int dir in Neighbors4)
            RefreshConnections(cell + dir);
    }

    public void UnregisterFence(Vector3Int cell)
    {
        fences.Remove(cell);
        foreach (Vector3Int dir in Neighbors4)
            RefreshConnections(cell + dir);
    }

    private void RefreshConnections(Vector3Int cell)
    {
        if (!fences.TryGetValue(cell, out FenceTile tile)) return;

        int mask = 0;
        if (fences.ContainsKey(cell + Vector3Int.up))    mask |= 1; // N
        if (fences.ContainsKey(cell + Vector3Int.right)) mask |= 2; // E
        if (fences.ContainsKey(cell + Vector3Int.down))  mask |= 4; // S
        if (fences.ContainsKey(cell + Vector3Int.left))  mask |= 8; // W
        tile.ApplyMask(mask);
    }
}
