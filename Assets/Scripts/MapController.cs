using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapController : MonoBehaviour
{
    public static MapController instance;
    public bool overMap = false;

    private PolygonCollider2D polyCollider;

    // Optional — assign the single "Bound" edge absorber here if you need one.
    // Leave unassigned if the Bound objects were removed entirely.
    public PolygonCollider2D boundCollider;

    private void Start()
    {
        instance = this;
        polyCollider = GetComponent<PolygonCollider2D>();
    }

    private void Update()
    {
        if (!overMap)
            MenuController.instance.previewObstructed = true;
    }

    private void OnMouseOver()
    {
        overMap = true;
    }

    private void OnMouseExit()
    {
        overMap = false;
    }

    // Call this after unlocking a zone or generating the map to resize the collider.
    public void RefreshBounds(List<ZoneData> unlockedZones)
    {
        if (polyCollider == null) return;

        // Find the combined world-space bounds of all unlocked tilemaps.
        bool first = true;
        float xMin = 0, xMax = 0, yMin = 0, yMax = 0;

        foreach (ZoneData zone in unlockedZones)
        {
            if (!zone.isUnlocked || zone.tilemap == null) continue;

            Bounds b = zone.tilemap.localBounds;
            Vector3 worldMin = zone.tilemap.transform.TransformPoint(b.min);
            Vector3 worldMax = zone.tilemap.transform.TransformPoint(b.max);

            if (first)
            {
                xMin = worldMin.x; xMax = worldMax.x;
                yMin = worldMin.y; yMax = worldMax.y;
                first = false;
            }
            else
            {
                xMin = Mathf.Min(xMin, worldMin.x);
                xMax = Mathf.Max(xMax, worldMax.x);
                yMin = Mathf.Min(yMin, worldMin.y);
                yMax = Mathf.Max(yMax, worldMax.y);
            }
        }

        if (first) return; // no unlocked zones

        // Use the full bounding rectangle with a small padding so all edge tiles
        // are inside the overMap collider. Exact out-of-bounds enforcement is
        // handled by the tile-existence check in PreviewCollisionController.
        float pad = 0.5f;
        Vector2[] rect = new Vector2[]
        {
            new Vector2(xMin - pad, yMin - pad),
            new Vector2(xMax + pad, yMin - pad),
            new Vector2(xMax + pad, yMax + pad),
            new Vector2(xMin - pad, yMax + pad),
        };

        polyCollider.SetPath(0, rect);

        // Keep the Bound absorber collider in sync if one is assigned.
        if (boundCollider != null)
            boundCollider.SetPath(0, rect);
    }
}
