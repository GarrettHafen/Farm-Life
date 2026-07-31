using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MarketTabButton : MonoBehaviour
{
    public Button button;
    public Image background;
    public Image icon;
    public TMP_Text label;

    public float selectedScale = 1.08f;
    public float sideScale = 0.85f;

    public MarketState State { get; private set; }
    public int RingIndex { get; private set; }
    public RectTransform RectTransform => (RectTransform)transform;

    public void Configure(MarketTabConfig config, int ringIndex, MarketTabBar bar)
    {
        State = config.state;
        RingIndex = ringIndex;

        // MarketTabBar positions buttons via anchoredPosition relative to slotContainer's center,
        // which only means "center" if the anchors/pivot are centered too — force that here so a
        // corner-anchored prefab (Unity's UI default) can't throw the carousel off to one side.
        RectTransform.anchorMin = RectTransform.anchorMax = RectTransform.pivot = new Vector2(0.5f, 0.5f);

        if (label != null) label.text = config.label;
        else Debug.LogWarning($"MarketTabButton on '{name}': label reference is unassigned, so its text won't update.", this);

        if (icon != null) icon.sprite = config.icon;
        else Debug.LogWarning($"MarketTabButton on '{name}': icon reference is unassigned, so its sprite won't update.", this);

        if (background != null) background.color = config.tint;
        else Debug.LogWarning($"MarketTabButton on '{name}': background reference is unassigned, so its tint won't update.", this);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => bar.OnTabClicked(this));
    }

    public void SetSelected(bool selected)
    {
        transform.localScale = Vector3.one * (selected ? selectedScale : sideScale);
    }
}
