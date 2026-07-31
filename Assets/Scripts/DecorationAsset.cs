using UnityEngine;

[CreateAssetMenu(fileName = "NewDecoration", menuName = "Farm/Decoration")]
public class DecorationAsset : ScriptableObject
{
    public string decorationName;
    public Sprite iconSprite;
    public GameObject prefab;
    public int placeCost = 10;
    public int removalCost = 5;
    public int reqLvl = 1;

    // Footprint size, same convention as TreeAsset/AnimalAsset ("1x1", "2x2", "4x4") —
    // consumed via TileSelector.PreviewSizeToCells. A decoration meant to fill a whole
    // grid cell (e.g. a fence segment) should use "4x4" so its footprint matches the
    // coarse cell FenceManager keys connections on.
    public string preview = "1x1";
}
