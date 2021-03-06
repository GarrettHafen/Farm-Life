﻿using UnityEngine;

[System.Serializable]
public class Tree
{
    public TreeAsset asset;
    public TreeState treeState;
    private float growthLevel;

    public bool TreeGrow(float amount, TreeTile tree)
    {
        if (!tree.isBusy)
        {
            growthLevel += amount / asset.treeTimer;
            if (growthLevel >= 1f)
            {
                treeState = TreeState.Done;
                //Debug.Log("tree is done");
                return true;
            }
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
