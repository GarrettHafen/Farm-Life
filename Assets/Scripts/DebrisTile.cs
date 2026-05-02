using UnityEngine;

public class DebrisTile : MonoBehaviour
{
    public static DebrisTile instance;
    public int removalCost = 25;
    public bool isBusy = false;

    private void Start()
    {
        instance = this;
    }

    public void Interact()
    {
        if (isBusy) return;
        if (!StatsController.instance.CheckMaster(removalCost))
        {
            MenuController.instance.notificationBar.SetActive(false);
            MenuController.instance.AnimateNotifcation("Insufficient Funds", Color.red, "No Money");
            return;
        }
        StatsController.instance.RemoveCoins(removalCost);
        isBusy = true;
        GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
        QueueTaskSystem.instance.SetTask("clearDebris", this);
    }

    public void DestroyDebris(DebrisTile debris)
    {
        TileSelector.instance.debris.Remove(debris.gameObject);
        AudioManager.instance.PlaySound("Destroy");
        StatsController.instance.AddExp(1);
        Object.Destroy(debris.gameObject);
    }

    public void DestroyAllDebris()
    {
        foreach (GameObject d in TileSelector.instance.debris)
            Object.Destroy(d);
        TileSelector.instance.debris.Clear();
    }
}
