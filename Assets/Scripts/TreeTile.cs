using UnityEngine;

public class TreeTile : MonoBehaviour
{
    public static TreeTile instance;
    public Tree tree;
    public bool isBusy = false;
    public Vector3 snapPosition;
    public int previewCells = 2;

    public SpriteRenderer overlay;

    [SerializeField] private string harvestSoundName = "Harvest";
    [SerializeField] private string destroySoundName = "Destroy";

    //used to control opacity for task system
    SpriteRenderer parentSprite;
    SpriteRenderer[] childSprites;


    void Start()
    {
        instance = this;
    }

    public void Interact(Tree t, TreeTile treeTile, PlayerInteraction player)
    {
        if(treeTile.tree.HasTree() && treeTile.tree.treeState == TreeState.Done && !MenuController.instance.toolState.fireTool)
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
        if (MenuController.instance.toolState.fireTool)
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
        treeThingy.tree.StartGrowth(treeThingy);
        AudioManager.instance.PlaySound(harvestSoundName);
        treeThingy.isBusy = false;

    }

    public void DestroyTree(TreeTile treeToDestroy)
    {
        TileSelector.instance.UnregisterFootprint(treeToDestroy.snapPosition, treeToDestroy.previewCells);
        foreach(GameObject tree in TileSelector.instance.trees)
        {
            if (tree.Equals(treeToDestroy.gameObject))
            {
                TileSelector.instance.trees.Remove(tree);
                break;
            }
        }
        Object.Destroy(treeToDestroy.gameObject);
        AudioManager.instance.PlaySound(destroySoundName);
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
