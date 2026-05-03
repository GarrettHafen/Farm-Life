using UnityEngine;

public class PreviewCollisionController : MonoBehaviour
{
    private Collider2D previewCollider;
    private readonly Collider2D[] buffer = new Collider2D[16];
    private readonly ContactFilter2D noFilter = new ContactFilter2D().NoFilter();

    [Header("Debug")]
    public bool debugLog = false;

    private void Awake()
    {
        previewCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (previewCollider == null) return;

        // Block placement where no tilemap tile exists (keeps objects inside map bounds)
        bool noTile = false;
        Vector3 placementPos = Vector3.zero;
        bool obstructed = false;
        if (TileSelector.instance != null)
        {
            placementPos = MenuController.instance.GetPlacementPosition();
            if (!TileSelector.instance.HasTileAtWorldPos(placementPos))
            {
                obstructed = true;
                noTile = true;
            }
        }

        // Block placement when any fine cell in the footprint is already occupied
        if (!obstructed && TileSelector.instance != null)
        {
            if (!TileSelector.instance.IsFootprintClear(placementPos, MenuController.instance.activePreviewCells))
                obstructed = true;
        }

        if (debugLog)
        {
            Bounds b = previewCollider.bounds;
            Vector2 center = b.center;
            Vector2 half   = b.extents;

            // Sample the four corners of the preview collider bounds
            Vector2 tl = new Vector2(center.x - half.x, center.y + half.y);
            Vector2 tr = new Vector2(center.x + half.x, center.y + half.y);
            Vector2 bl = new Vector2(center.x - half.x, center.y - half.y);
            Vector2 br = new Vector2(center.x + half.x, center.y - half.y);

            Collider2D cTL = Physics2D.OverlapPoint(tl);
            Collider2D cTR = Physics2D.OverlapPoint(tr);
            Collider2D cBL = Physics2D.OverlapPoint(bl);
            Collider2D cBR = Physics2D.OverlapPoint(br);

            System.Text.StringBuilder sb = new();
            sb.AppendLine($"[Preview] pos={placementPos:F2}  obstructed={obstructed}  noTile={noTile}");
            sb.AppendLine($"  collider bounds: center={center:F2}  size={b.size:F2}");
            sb.AppendLine($"  TL={tl:F2} → {(cTL ? cTL.gameObject.name : "none")}");
            sb.AppendLine($"  TR={tr:F2} → {(cTR ? cTR.gameObject.name : "none")}");
            sb.AppendLine($"  BL={bl:F2} → {(cBL ? cBL.gameObject.name : "none")}");
            sb.AppendLine($"  BR={br:F2} → {(cBR ? cBR.gameObject.name : "none")}");
            //sb.Append($"  OverlapCollider hits ({count}): ");
            //for (int i = 0; i < count; i++)
            //    sb.Append($"'{buffer[i].gameObject.name}' ");

            Debug.Log(sb.ToString());
        }

        MenuController.instance.previewObstructed = obstructed;
    }
}
