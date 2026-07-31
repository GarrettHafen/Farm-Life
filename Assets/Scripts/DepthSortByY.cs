using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(SpriteRenderer))]
public class DepthSortByY : MonoBehaviour
{
    [SerializeField] private int precision = 10;
    [SerializeField] private int baseOffset = 0;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        // SpriteRenderer.sortingOrder is a 16-bit value (-32768..32767) internally;
        // clamp so far-off-origin objects can't silently wrap and invert draw order.
        int order = baseOffset - Mathf.RoundToInt(transform.position.y * precision);
        sr.sortingOrder = Mathf.Clamp(order, -32000, 32000);
    }
}
