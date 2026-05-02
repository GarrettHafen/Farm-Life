using UnityEngine;

[CreateAssetMenu(fileName = "NewFarmZone", menuName = "Farm/Farm Zone")]
public class FarmZoneAsset : ScriptableObject
{
    public string zoneName;
    public int unlockLevel;
    public int unlockCost;
    public bool unlockedByDefault;
    public Sprite iconSprite;
}
