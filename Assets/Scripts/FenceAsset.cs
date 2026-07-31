using UnityEngine;

// One hand-drawn sprite per N/E/S/W connection combination — see
// Assets/Art/Decorations/FenceDirections. No runtime rotation: each combination is its
// own baked image, matched by FenceTile.ApplyMask.
[CreateAssetMenu(fileName = "NewFenceSet", menuName = "Farm/Decoration/Fence Set")]
public class FenceAsset : DecorationAsset
{
    [Header("No connections")]
    public Sprite spriteIsolated;

    [Header("One connection")]
    public Sprite spriteN;
    public Sprite spriteE;
    public Sprite spriteS;
    public Sprite spriteW;

    [Header("Two connections - straight")]
    public Sprite spriteNS;
    public Sprite spriteEW;

    [Header("Two connections - corner")]
    public Sprite spriteNE;
    public Sprite spriteSE;
    public Sprite spriteSW;
    public Sprite spriteNW;

    [Header("Three connections")]
    public Sprite spriteNES;
    public Sprite spriteNEW;
    public Sprite spriteNSW;
    public Sprite spriteESW;

    [Header("Four connections")]
    public Sprite spriteCross;
}
