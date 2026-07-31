using UnityEngine;

// A fence segment that auto-connects to its N/E/S/W neighbors. Sprite is driven entirely
// by FenceManager, which recomputes this tile's mask (and its neighbors') whenever a
// fence is placed/removed nearby. Each of the 16 N/E/S/W combinations is its own
// hand-drawn sprite (Assets/Art/Decorations/FenceDirections) — no runtime rotation.
public class FenceTile : DecorationTile
{
    // Coarse grid cell this fence occupies — set by FenceManager on registration.
    public Vector3Int cellCoord;

    // True for fences stamped by TileSelector.GenerateBorderFence around the farmland
    // perimeter, as opposed to ones a player placed through the Decoration market.
    // Border fences aren't player-removable (yet) — removing one would desync
    // TileSelector's borderFenceCells tracking from what's actually on screen.
    public bool isBorderFence = false;

    [SerializeField] private SpriteRenderer sr;

    private FenceAsset Fence => (FenceAsset)asset;

    private void Awake()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
    }

    // mask: bit0=N bit1=E bit2=S bit3=W (0-15).
    public void ApplyMask(int mask)
    {
        sr.sprite = mask switch
        {
            0  => Fence.spriteIsolated,
            1  => Fence.spriteN,
            2  => Fence.spriteE,
            3  => Fence.spriteNE,
            4  => Fence.spriteS,
            5  => Fence.spriteNS,
            6  => Fence.spriteSE,
            7  => Fence.spriteNES,
            8  => Fence.spriteW,
            9  => Fence.spriteNW,
            10 => Fence.spriteEW,
            11 => Fence.spriteNEW,
            12 => Fence.spriteSW,
            13 => Fence.spriteNSW,
            14 => Fence.spriteESW,
            _  => Fence.spriteCross, // 15 (N+E+S+W)
        };
    }

    public override void Interact()
    {
        if (isBorderFence) return;
        base.Interact();
    }

    public override void RemoveSelf()
    {
        FenceManager.instance.UnregisterFence(cellCoord);
        base.RemoveSelf();
    }
}
