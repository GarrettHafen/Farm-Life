using UnityEngine;

// One entry per market category shown in the tab strip. Add a new tab (Machines, Decorations, Friends, Shops...)
// by adding one entry to MarketTabBar's list instead of duplicating header + grid UI.
[System.Serializable]
public class MarketTabConfig
{
    public MarketState state;
    public string label;
    public Sprite icon;
    public Color tint = Color.white;
}
