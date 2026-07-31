using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DebrisVariant
{
    public List<Sprite> sprites;

    // Optional canopy tier, index-aligned with `sprites` (canopySprites[i] pairs with sprites[i]).
    // Rendered on a sorting layer that always draws in front of ground-level objects. Leave
    // empty, or shorter than `sprites`, to keep unpaired entries rendering as a single sprite.
    public List<Sprite> canopySprites;

    [Range(0f, 1f)] public float threshold;
    public string preview = "1x1"; // footprint size: "1x1", "2x2", or "4x4"
    public int removalCost = 25;
    public bool interactable = true;
    public GameObject prefabOverride;
}

[CreateAssetMenu(fileName = "New Debris Set", menuName = "Farm/Debris Set")]
public class DebrisSetAsset : ScriptableObject
{
    public string setName;
    [Range(0f, 1f)] public float fillChance = 0.7f;
    public float clearRadius = 1.5f;
    public List<DebrisVariant> variants;
}
