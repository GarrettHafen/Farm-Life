using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class TreeTile : MonoBehaviour
{
    public static TreeTile instance;
    public Tree tree;

    public SpriteRenderer overlay;


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
                    UpdateTreeSprite();
                }
            }
        }
    }

    public void Interact(Tree t, TreeTile treeTile, PlayerInteraction player)
    {
        if(treeTile.tree.HasTree() && treeTile.tree.treeState == TreeState.Done && !MenuController.instance.fireTool)
        {
            HarvestTree(player);
        }
        if (MenuController.instance.fireTool)
        {
            MenuController.instance.OpenFireMenu();
            //treeTile.DestroyTree(treeTile);
        }

        //sell
    }

    void HarvestTree(PlayerInteraction player)
    {
        StatsController.instance.AddCoins(tree.asset.treeReward);
        StatsController.instance.AddExp(tree.asset.expReward);
        tree.treeState = TreeState.Growing;
        UpdateTreeSprite();
        tree.SetGrowthLvl(0f);
        FindObjectOfType<AudioManager>().PlaySound("Harvest");
        
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

    public void UpdateTreeSprite()
    {
        overlay.sprite = tree.GetTreeSprite();
    }
}
