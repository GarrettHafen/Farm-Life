using UnityEngine;

[System.Serializable]
public class Tree
{
    public TreeAsset asset;
    public TreeState treeState;
    private float growthLevel;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool TreeGrow(float amount, TreeTile tree)
    {
        growthLevel += amount / asset.treeTimer;
        if (growthLevel >= 1f)
        {
            treeState = TreeState.Done;
            Debug.Log("tree is done");
            return true;
        }
        /*else if (growthLevel <= 1f && growthLevel >= .5f)
        {
            treeState = TreeState.Growing;
            if (tree.overlay.sprite = asset.treePlantedSprite)
            {
                tree.UpdateTreeSprite();
            }
            return false;
        }*/
        return false;
    }

    public float GetGrowthLvl()
    {
        return growthLevel;
    }
    public void SetGrowthLvl(float time)
    {
        growthLevel = time;
    }

    public Tree(TreeAsset a)
    {
        asset = a;
        treeState = TreeState.Growing;
        growthLevel = 0f;
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

    public Sprite GetTreeSprite()
    {
        if (asset == null)
            return null;

        switch (treeState)
        {
            case TreeState.Planted:
                return asset.treePlantedSprite;
            case TreeState.Growing://displays the flowering sprite
                return asset.treeGrowingSprite;
            case TreeState.Done:
                return asset.treeDoneSprite;
        }

        Debug.LogError("WHAT?!");
        MenuController.instance.notificationBar.SetActive(false);
        MenuController.instance.AnimateNotifcation("Tree State Error", Color.red, "Error");

        return asset.treePlantedSprite;
    }

    
}

public enum TreeState
    {
        Planted,
        Growing,
        Done
    }
