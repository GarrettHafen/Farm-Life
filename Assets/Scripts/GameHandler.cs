using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameHandler : MonoBehaviour
{
    public static GameHandler instance;

    public CameraFollow cameraFollow;
    public Transform playerTransform;
    public Transform manualMovementTransform;

    private float autoSaveTimer;
    private int autoSaveInterval = 180;

    private Vector3 cameraFollowPostion;

    private float zoom = 1.25f;

    private bool edgeScrolling;

    public bool overMenu = false;

    public Tool PlowTool;

    public List<CropAsset> cropsList;
    public List<TreeAsset> treeList;
    public List<Tree> loadTreeList;

    private List<CropAsset> loadCropList = new List<CropAsset>();

    public bool landingPageOpen = true;
    public GameObject landingPage;
    public GameObject baseTree;



    // Start is called before the first frame update
    // this is a test
    void Start()
    {
        instance = this;
       

    }

    // Update is called once per frame
    void Update()
    {

        if (!landingPageOpen)
        {
            if (autoSaveTimer < autoSaveInterval)
            {
                autoSaveTimer += Time.deltaTime;
            }
            else
            {
                SaveSystem.SavePlayer();
                Debug.Log("Auto Save Complete");
                autoSaveTimer = 0;
                MenuController.instance.notificationBar.SetActive(false);
                MenuController.instance.AnimateNotifcation("Auto Save Complete", Color.white, "Null");
            }
        }

        



    }

    

    //disable and enable gameplay if mouse is hovered over menu
    public void DisableTool()
    {
        overMenu = true;
    }
    public void EnableTool()
    {
        overMenu = false;
    }

    public void LoadData(PlayerData data)
    {

        StatsController.instance.SetLvl(data.level);
        StatsController.instance.SetCoins(data.coins);
        StatsController.instance.SetExp(data.exp);
        StatsController.instance.UpdateStats();

        //destroy all objects, clear plots array
        if (TileSelector.instance.plots.Count > 0)
        {
            DirtTile.instance.DestroyPlots();
            TileSelector.instance.plotNum = 0;
        }
        if (TileSelector.instance.trees.Count > 0)
        {
            TreeTile.instance.DestroyTrees();
            TileSelector.instance.treeNum = 0;
        }
        

        //instantiate plots, add to array, set details
        for (int i = 0; i < data.cropsActive; i++)
        {
            Vector3 newPlotPosition = new Vector3(data.plotPositionX[i], data.plotPositionY[i], 9);
            TileSelector.instance.PlacePlot(newPlotPosition, new Vector3(0,0,0));
            //PlacePlot should add to new array.

            //get dirt object of newly instantiated plot
            DirtTile dirt = TileSelector.instance.plots[i].GetComponent<DirtTile>();
            if (data.activeCropNames[i] != null) //if activeCropNames is null, add blank crop
            {
                for (int j = 0; j < cropsList.Count; j++) // loop through entire crops list
                {
                    if (cropsList[j] != null) // if crops list item is null, might could remove when list = 100
                    {
                        if (data.activeCropNames[i] == cropsList[j].cropName) // if strings match, add as new crop
                        {
                            dirt.crop = new Crop(cropsList[j]);

                            //calc time that has passed since save and apply to growth timer
                            dirt.crop.SetGrowthLvl(CalcTimePassed(data.activeTimers[i], data.savedTime, dirt.crop.asset.cropTimer, dirt));
                            //set crop state and update sprites
                            dirt.crop.state = dirt.crop.GetState(data.activeCropsStates[i]);
                            dirt.crop.GetCropSprite();// ************************i don't know what this line does ************************
                            dirt.UpdateSprite();

                            //set needs plowing
                            dirt.needsPlowing = data.needsPlowing[i];
                            break; // this might be needed for performance because the list
                        }
                    }
                }
            }
            else
            {
                    
                dirt.crop = new Crop(null);
                dirt.crop.SetGrowthLvl(data.activeTimers[i]);
                dirt.crop.state = dirt.crop.GetState(data.activeCropsStates[i]);
                dirt.needsPlowing = data.needsPlowing[i];
                if (dirt.needsPlowing)// if needs plowing is true, then show fallow
                {
                    dirt.AddDirt();
                }
            }
        }

        //instantiate trees, add to array, set details
        
        for (int i = 0; i < data.activeTreeNames.Length; i++) {
            for (int j = 0; j < treeList.Count; j++)
            {
                if (data.activeTreeNames[i] == treeList[j].treeName)
                {
                    loadTreeList.Add(new Tree(treeList[j]));
                    break;
                }
            }
        }
        for(int i = 0; i < data.treesActive; i++)
        {
            Vector3 newTreePosition = new Vector3(data.treePositionX[i], data.treePositionY[i], 9);
            TileSelector.instance.PlantTree(newTreePosition, loadTreeList[i], PlayerInteraction.instance, new Vector3(0, 0, 0));
            TreeTile treeTile = TileSelector.instance.trees[i].GetComponent<TreeTile>();
            treeTile.tree.SetGrowthLvl(CalcTimePassed(data.activeTreeTimers[i], data.savedTime, treeTile.tree.asset.treeTimer));
            treeTile.tree.treeState = treeTile.tree.GetState(data.activeTreeStates[i]);
            treeTile.UpdateTreeSprite();

        }

        //instantiate animals, add to array, set details

        //instantiate decor, add to array, set details


        Debug.Log("Data Loaded");
        MenuController.instance.notificationBar.SetActive(false);
        MenuController.instance.AnimateNotifcation("Load Complete", Color.white, "Null");
    }

    private float CalcTimePassed(float currentGrowthTime, DateTime oldTime, float cropTimeTotal, DirtTile dirt)
    {
        float newTime;
        TimeSpan difference = System.DateTime.Now.Subtract(oldTime);
        newTime = (float)difference.TotalSeconds; // number of seconds that have passed since the save to the load.
        //need to determine how many seconds from when the crop was planted, to the save, then add that to the difference
        // from there need to figure out based on the growth time total, where its at. if its greater than 1, then its full grown. 
        //and to future proof, need to also save the difference between new time and growth time to factor in weathering in the future. 
        if(newTime >= cropTimeTotal)
        {
            dirt.crop.SetWitherTimer(newTime - cropTimeTotal);
            return 1f;
        }
        else
        {
            float temp = newTime / cropTimeTotal;
            return temp + currentGrowthTime;
        }
    }
    private float CalcTimePassed(float currentGrowthTime, DateTime oldTime, float cropTimeTotal)
    {
        float newTime;
        TimeSpan difference = System.DateTime.Now.Subtract(oldTime);
        newTime = (float)difference.TotalSeconds; // number of seconds that have passed since the save to the load.
        //need to determine how many seconds from when the crop was planted, to the save, then add that to the difference
        // from there need to figure out based on the growth time total, where its at. if its greater than 1, then its full grown. 
        //and to future proof, need to also save the difference between new time and growth time to factor in weathering in the future. 
        if (newTime >= cropTimeTotal)
        {
            return 1f;
        }
        else
        {
            float temp = newTime / cropTimeTotal;
            return temp + currentGrowthTime;
        }
    }






}


