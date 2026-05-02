using UnityEngine;

[System.Serializable]
public class Tree
{
    public TreeAsset asset;
    public TreeState treeState;
    private float growthStartTime;

    private System.Action _doneCallback;

    public void StartGrowth(TreeTile tile)
    {
        CancelGrowth();

        float doneTime = growthStartTime + asset.treeTimer;

        if (doneTime > Time.time)
        {
            _doneCallback = () =>
            {
                treeState = TreeState.Done;
                tile.UpdateTreeSprite(tile);
            };
            GrowthManager.instance.Register(doneTime, _doneCallback);
        }
        else if (treeState != TreeState.Done)
        {
            treeState = TreeState.Done;
            tile.UpdateTreeSprite(tile);
        }
    }

    public void CancelGrowth()
    {
        if (_doneCallback != null)
        {
            GrowthManager.instance.Cancel(_doneCallback);
            _doneCallback = null;
        }
    }

    public float GetGrowthLvl()
    {
        if (treeState == TreeState.Done)
            return 1f;
        if (asset == null)
            return 0f;
        return Mathf.Clamp01((Time.time - growthStartTime) / asset.treeTimer);
    }

    public void SetGrowthLvl(float level)
    {
        if (asset != null)
            growthStartTime = Time.time - level * asset.treeTimer;
    }

    public Tree(TreeAsset a)
    {
        asset = a;
        treeState = TreeState.Growing;
        growthStartTime = 0f;
    }

    public bool HasTree()
    {
        if(asset == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public int GetCost()
    {
        return asset.treeCost;
    }

    public Sprite GetTreeSprite(Tree t)
    {
        if (t.asset == null)
            return null;

        switch (t.treeState)
        {
            case TreeState.Planted:
                return t.asset.treePlantedSprite;
            case TreeState.Growing://displays the flowering sprite
                return t.asset.treeGrowingSprite;
            case TreeState.Done:
                return t.asset.treeDoneSprite;
        }

        Debug.LogError("WHAT?!");
        MenuController.instance.notificationBar.SetActive(false);
        MenuController.instance.AnimateNotifcation("Tree State Error", Color.red, "Error");

        return asset.treePlantedSprite;
    }

    public string GetState()
    {
        string saveState = "Growing";
        switch (treeState)
        {
            case TreeState.Planted:
                saveState = "Planted";
                break;
            case TreeState.Growing:
                saveState = "Growing";
                break;
            case TreeState.Done:
                saveState = "Done";
                break;
        }
        return saveState;
    }
    public TreeState GetState(string state)
    {
        TreeState loadedState = TreeState.Growing;
        switch (state)
        {
            case "Planted":
                loadedState = TreeState.Planted;
                break;
            case "Growing":
                loadedState = TreeState.Growing;
                break;
            case "Done":
                loadedState = TreeState.Done;
                break;
        }


        return loadedState;
    }
    public string GetName()
    {
        if (asset == null)
            return null;

        return asset.name;
    }


}

public enum TreeState
    {
        Planted,
        Growing,
        Done
    }
