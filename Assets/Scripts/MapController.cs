using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapController : MonoBehaviour
{
    public static MapController instance;
    public bool overMap = false;

    private BoxCollider2D polyCollider;

    // Optional — assign the single "Bound" edge absorber here if you need one.
    // Leave unassigned if the Bound objects were removed entirely.
    public BoxCollider2D boundCollider;

    private void Start()
    {
        instance = this;
        polyCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        overMap = polyCollider != null && polyCollider.OverlapPoint(mousePos);

        if (!overMap)
            MenuController.instance.previewObstructed = true;
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
            if (!zone.isUnlocked || zone.tilemap == null || (zone.zoneAsset != null && zone.zoneAsset.isBoundary)) continue;

            zone.tilemap.CompressBounds();
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
        Vector2 center = new Vector2((xMin + xMax) / 2f, (yMin + yMax) / 2f);
        Vector2 size = new Vector2(xMax - xMin + pad * 2f, yMax - yMin + pad * 2f);

        polyCollider.offset = center;
        polyCollider.size = size;

        // Keep the Bound absorber collider in sync if one is assigned.
        if (boundCollider != null)
        {
            boundCollider.offset = center;
            boundCollider.size = size;
        }
    }

    // World-space playable bounds, used to constrain idle-wander targets.
    public Bounds GetBounds()
    {
        return polyCollider != null ? polyCollider.bounds : new Bounds(Vector3.zero, Vector3.zero);
    }
}
