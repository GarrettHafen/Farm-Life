using UnityEngine;

// Base type for placed, non-growing props (fences are the first kind — see FenceTile).
// Placement is instant (via TileSelector.PlaceDecoration); removal follows the same
// queued-worker pattern as DebrisTile since a static prop is closer to debris than to a
// growing tree/animal.
public class DecorationTile : MonoBehaviour
{
    public DecorationAsset asset;
    public Vector3 snapPosition;
    public int previewCells = 1;
    public bool isBusy = false;

    public virtual void Interact()
    {
        if (isBusy) return;
        if (!StatsController.instance.CheckMaster(asset.removalCost))
        {
            MenuController.instance.notificationBar.SetActive(false);
            MenuController.instance.AnimateNotifcation("Insufficient Funds", Color.red, "No Money");
            return;
        }
        StatsController.instance.RemoveCoins(asset.removalCost);
        isBusy = true;
        GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
        QueueTaskSystem.instance.SetTask("clearDecoration", this);
    }

    // Named RemoveSelf (not "Destroy") to avoid shadowing UnityEngine.Object.Destroy —
    // called directly on the tile being removed rather than through the legacy
    // "TypeTile.instance.DestroyTypeTile(param)" dispatch pattern used by older tile
    // types, so overriding this in FenceTile dispatches correctly without relying on a
    // static `instance` field shared awkwardly between base and subclass.
    public virtual void RemoveSelf()
    {
        TileSelector.instance.UnregisterFootprint(snapPosition, previewCells);
        TileSelector.instance.decorations.Remove(gameObject);
        AudioManager.instance.PlaySound("Destroy");
        StatsController.instance.AddExp(1);
        Object.Destroy(gameObject);
    }
}
