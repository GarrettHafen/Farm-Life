﻿using UnityEngine;

public class TreeTile : MonoBehaviour
{
    public static TreeTile instance;
    public Tree tree;
    public bool isBusy = false;

    public SpriteRenderer overlay;


    //used to control opacity for task system
    SpriteRenderer parentSprite;
    SpriteRenderer[] childSprites;


    void Start()
    {
        instance = this;
    }

    void Update()
    {
        if (tree.HasTree())
        {
            if (tree.treeState == TreeState.Planted || tree.treeState == TreeState.Growing)
            {
                bool treeIsDone = tree.TreeGrow(Time.deltaTime, this);
                if (treeIsDone)
                {
                    UpdateTreeSprite(this);
                }
            }
        }
    }

    public void Interact(Tree t, TreeTile treeTile, PlayerInteraction player)
    {
        if(treeTile.tree.HasTree() && treeTile.tree.treeState == TreeState.Done && !MenuController.instance.fireTool)
        {
            //queue HARVEST TREE

            //set overlay and parent sprite opacity to .5
            parentSprite = treeTile.GetComponent<SpriteRenderer>();
            childSprites = treeTile.GetComponentsInChildren<SpriteRenderer>();

            parentSprite.color = new Color(1f, 1f, 1f, .5f);
            childSprites[1].color = new Color(1f, 1f, 1f, .5f);

            // queue task
            treeTile.isBusy = true;
            QueueTaskSystem.instance.SetTask("harvestTree", treeTile);

            //after task is compolete, see HarvestTree
        }
        if (MenuController.instance.fireTool)
        {
            MenuController.instance.OpenFireMenu();
        }

        //sell
        //move??
    }

    public void HarvestTree(TreeTile treeThingy)
    {
        parentSprite = treeThingy.GetComponent<SpriteRenderer>();
        childSprites = treeThingy.GetComponentsInChildren<SpriteRenderer>();

        parentSprite.color = new Color(1f, 1f, 1f, 1f);
        childSprites[1].color = new Color(1f, 1f, 1f, 1f);

        StatsController.instance.AddCoins(treeThingy.tree.asset.treeReward);
        StatsController.instance.AddExp(treeThingy.tree.asset.expReward);
        treeThingy.tree.treeState = TreeState.Growing;
        UpdateTreeSprite(treeThingy);
        treeThingy.tree.SetGrowthLvl(0f);
        FindObjectOfType<AudioManager>().PlaySound("Harvest");
        treeThingy.isBusy = false;

    }

    public void DestroyTree(TreeTile treeToDestroy)
    {
        foreach(GameObject tree in TileSelector.instance.trees)
        {
            if (tree.Equals(treeToDestroy.gameObject))
            {
                TileSelector.instance.trees.Remove(tree);
                break;
            }
        }
        Object.Destroy(treeToDestroy.gameObject);

        FindObjectOfType<AudioManager>().PlaySound("Destroy");
    }

    public void DestroyTrees()
    {
        int temp = TileSelector.instance.trees.Count;
        foreach (GameObject tree in TileSelector.instance.trees)
        {
            Object.Destroy(tree);
        }
        TileSelector.instance.trees.Clear();
        
    }

    public void UpdateTreeSprite(TreeTile treeThingy)
    {
        treeThingy.overlay.sprite = tree.GetTreeSprite(treeThingy.tree);
    }
}
