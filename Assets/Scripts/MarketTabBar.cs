using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Conveyor-belt tab strip: only the visible window of buttons (2*radius+1 of them) exists at any
// time, keyed by which on-screen slot they're occupying. Clicking a neighbor shifts every active
// button one slot toward it, spawns the newly-revealed tab off-screen on the entering side so it
// slides in, and destroys whichever button slides fully off the trailing edge — looping through
// `tabs` indefinitely in either direction.
public class MarketTabBar : MonoBehaviour
{
    public MarketController marketController;
    public MarketTabButton tabButtonPrefab;
    public RectTransform slotContainer;
    public List<MarketTabConfig> tabs;

    public float slotSpacing = 220f;
    public float slideDuration = 0.25f;
    [Tooltip("How many tabs are visible on each side of the centered/active tab. 1 = 3 tabs visible total.")]
    public int radius = 1;

    [Header("Optional: auto-size the visible window")]
    [Tooltip("The RectMask2D'd RectTransform that clips the tab strip. If set, its width is kept in sync with (radius*2+1) * slotSpacing.")]
    public RectTransform maskRect;

    // Keyed by on-screen slot offset (-radius..+radius): the button currently occupying that slot.
    private readonly Dictionary<int, MarketTabButton> activeButtons = new();
    private int centerTabIndex;
    private Coroutine shiftRoutine;

    void Start()
    {
        BuildTabs();
    }

    public void BuildTabs()
    {
        if (maskRect != null)
            maskRect.sizeDelta = new Vector2((radius * 2 + 1) * slotSpacing, maskRect.sizeDelta.y);

        foreach (MarketTabButton existing in activeButtons.Values)
            if (existing != null) Destroy(existing.gameObject);
        activeButtons.Clear();

        if (tabs.Count == 0) return;

        centerTabIndex = 0;
        for (int offset = -radius; offset <= radius; offset++)
        {
            MarketTabButton btn = CreateButton(WrapIndex(centerTabIndex + offset), offset * slotSpacing);
            btn.SetSelected(offset == 0);
            activeButtons[offset] = btn;
        }

        marketController.SetMarketState(tabs[centerTabIndex].state);
    }

    public void OnTabClicked(MarketTabButton clicked)
    {
        if (shiftRoutine != null) return; // ignore clicks mid-slide

        int clickedOffset = int.MinValue;
        foreach (var kvp in activeButtons)
        {
            if (kvp.Value == clicked)
            {
                clickedOffset = kvp.Key;
                break;
            }
        }
        if (clickedOffset == int.MinValue || clickedOffset == 0) return;

        int direction = clickedOffset > 0 ? 1 : -1;
        shiftRoutine = StartCoroutine(ShiftSteps(direction, Mathf.Abs(clickedOffset)));
    }

    private IEnumerator ShiftSteps(int direction, int steps)
    {
        for (int s = 0; s < steps; s++)
            yield return StartCoroutine(ShiftOneStep(direction));
        shiftRoutine = null;
    }

    // direction +1: clicked a tab to the right of center, so the belt moves left (every offset -1).
    // direction -1: clicked a tab to the left of center, so the belt moves right (every offset +1).
    private IEnumerator ShiftOneStep(int direction)
    {
        centerTabIndex = WrapIndex(centerTabIndex + direction);
        marketController.SetMarketState(tabs[centerTabIndex].state);

        int enteringStartOffset = direction * (radius + 1); // spawn just beyond the visible window
        int enteringRingIndex = WrapIndex(centerTabIndex + direction * radius);
        MarketTabButton entering = CreateButton(enteringRingIndex, enteringStartOffset * slotSpacing);

        int exitingOffset = -direction * radius; // current trailing-edge occupant, about to leave
        activeButtons.TryGetValue(exitingOffset, out MarketTabButton exiting);

        var moving = new List<(MarketTabButton button, int fromOffset, int toOffset)>();
        Dictionary<int, MarketTabButton> newActive = new(activeButtons.Count + 1);
        foreach (var kvp in activeButtons)
        {
            int toOffset = kvp.Key - direction;
            moving.Add((kvp.Value, kvp.Key, toOffset));
            newActive[toOffset] = kvp.Value;
        }
        moving.Add((entering, enteringStartOffset, enteringStartOffset - direction));
        newActive[enteringStartOffset - direction] = entering;

        activeButtons.Clear();
        foreach (var kvp in newActive)
            if (kvp.Key >= -radius && kvp.Key <= radius)
                activeButtons[kvp.Key] = kvp.Value;

        foreach (var kvp in activeButtons)
            kvp.Value.SetSelected(kvp.Key == 0);

        yield return StartCoroutine(AnimateMove(moving));

        if (exiting != null && exiting.gameObject != null)
            Destroy(exiting.gameObject);
    }

    private IEnumerator AnimateMove(List<(MarketTabButton button, int fromOffset, int toOffset)> moving)
    {
        float t = 0f;
        while (t < slideDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / slideDuration);
            foreach (var (button, fromOffset, toOffset) in moving)
            {
                if (button == null) continue;
                float x = Mathf.LerpUnclamped(fromOffset * slotSpacing, toOffset * slotSpacing, p);
                button.RectTransform.anchoredPosition = new Vector2(x, 0f);
            }
            yield return null;
        }

        foreach (var (button, _, toOffset) in moving)
            if (button != null)
                button.RectTransform.anchoredPosition = new Vector2(toOffset * slotSpacing, 0f);
    }

    private MarketTabButton CreateButton(int ringIndex, float initialX)
    {
        MarketTabButton tabButton = Instantiate(tabButtonPrefab, slotContainer);
        tabButton.Configure(tabs[ringIndex], ringIndex, this);
        tabButton.RectTransform.anchoredPosition = new Vector2(initialX, 0f);
        return tabButton;
    }

    private int WrapIndex(int index)
    {
        int n = tabs.Count;
        return ((index % n) + n) % n;
    }
}
