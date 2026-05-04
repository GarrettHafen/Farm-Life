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
    public List<AnimalAsset> animalList;
    public List<PreviewAsset> previewList;
    public List<GameObject> previewContainerList;

    public List<Tree> loadTreeList;
    public List<Animal> loadAnimalList;
    private List<CropAsset> loadCropList = new List<CropAsset>();

    public bool landingPageOpen = true;
    public GameObject landingPage;
    public GameObject baseTree;

    public bool devMode;



    // Start is called before the first frame update
    // this is a test
    void Start()
    {
        instance = this;
        var mainCanvas = landingPage.transform.parent.gameObject;
        if(!mainCanvas.activeSelf)
        {
            mainCanvas.SetActive(true);    
        }
       

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
        PlayerInteraction.instance.Deselect();
    }
    public void EnableTool()
    {
        overMenu = false;

    }

    public void LoadData(PlayerData data)
    {
        TileSelector.instance.ClearAllFootprints();
        StatsController.instance.SetLvl(data.level);
        StatsController.instance.SetCoins(data.coins);
        StatsController.instance.SetExp(data.exp);
        StatsController.instance.UpdateStats();

        // restore zone unlock flags, then apply saved tile data (which also rebuilds the grid)
        TileSelector.instance.SetZoneUnlocks(data.unlockedZoneNames);
        TileSelector.instance.LoadZoneTileData(data.zoneTileData);

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
        if(TileSelector.instance.animals.Count > 0)
        {
            AnimalTile.instance.DestroyAnimals();
            TileSelector.instance.animalNum = 0;
        }
        if (TileSelector.instance.debris.Count > 0)
        {
            DebrisTile.instance.DestroyAllDebris();
            TileSelector.instance.debrisNum = 0;
        }

        Grid grid = TileSelector.instance.grid;

        //instantiate plots, add to array, set details
        for (int i = 0; i < data.cropsActive; i++)
        {
            Vector3 newPlotPosition = new Vector3(data.plotX[i], data.plotY[i], 9f);
            TileSelector.instance.PlacePlot(newPlotPosition, PlayerInteraction.instance.plotOffset);
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
                            dirt.crop.GetCropSprite(dirt.crop);// ************************i don't know what this line does ************************
                            dirt.UpdateSprite(dirt);
                            if (dirt.crop.state == CropState.Planted || dirt.crop.state == CropState.Growing)
                                dirt.crop.StartGrowth(dirt);

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
            dirt.GetComponent<TimerController>()?.slider.gameObject.SetActive(false);
        }

        //instantiate trees, add to array, set details
        loadTreeList.Clear();
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
            Vector3 newTreePosition = new Vector3(data.treeX[i], data.treeY[i], 9f);
            TileSelector.instance.PlantTree(newTreePosition, loadTreeList[i], PlayerInteraction.instance, PlayerInteraction.instance.treeOffset);
            TreeTile treeTile = TileSelector.instance.trees[i].GetComponent<TreeTile>();
            treeTile.tree.SetGrowthLvl(CalcTimePassed(data.activeTreeTimers[i], data.savedTime, treeTile.tree.asset.treeTimer));
            treeTile.tree.treeState = treeTile.tree.GetState(data.activeTreeStates[i]);
            treeTile.UpdateTreeSprite(treeTile);
            if (treeTile.tree.treeState == TreeState.Planted || treeTile.tree.treeState == TreeState.Growing)
                treeTile.tree.StartGrowth(treeTile);
            treeTile.GetComponent<TimerController>()?.slider.gameObject.SetActive(false);
        }

        //instantiate animals, add to array, set details
        loadAnimalList.Clear();
        for(int i = 0; i < data.activeAnimalNames.Length; i++)
        {
            for(int j = 0; j < animalList.Count; j++)
            {
                if(data.activeAnimalNames[i] == animalList[j].animalName)
                {
                    loadAnimalList.Add(new Animal(animalList[j]));
                    break;
                }
            }
        }
        for(int i = 0; i < data.animalsActive; i++)
        {
            Vector3 newAnimalPosition = new Vector3(data.animalX[i], data.animalY[i], 9f);
            TileSelector.instance.PlaceAnimal(newAnimalPosition, loadAnimalList[i], PlayerInteraction.instance);
            AnimalTile animalTile = TileSelector.instance.animals[i].GetComponent<AnimalTile>();
            animalTile.animal.SetGrowthLvl(CalcTimePassed(data.activeAnimalTimers[i], data.savedTime, animalTile.animal.asset.animalTimer));
            animalTile.animal.animalState = animalTile.animal.GetState(data.activeAnimalStates[i]);
            animalTile.UpdateAnimalSprite(animalTile);
            if (animalTile.animal.animalState == AnimalState.Growing)
                animalTile.animal.StartGrowth(animalTile);
            animalTile.GetComponent<TimerController>()?.slider.gameObject.SetActive(false);
        }

        //instantiate debris, add to array
        for (int i = 0; i < data.debrisActive; i++)
        {
            Vector3 debrisPos = new Vector3(data.debrisX[i], data.debrisY[i], 9f);
            TileSelector.instance.PlaceDebris(debrisPos, Vector3.zero);
        }

        MenuController.instance.ClearHand();
        Debug.Log("Data Loaded");
        MenuController.instance.notificationBar.SetActive(false);
        MenuController.instance.AnimateNotifcation("Load Complete", Color.white, "Null");
    }

    public void NewGame()
    {
        SaveSystem.DeleteSave();
        TileSelector.instance.ClearAllFootprints();

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
        if (TileSelector.instance.animals.Count > 0)
        {
            AnimalTile.instance.DestroyAnimals();
            TileSelector.instance.animalNum = 0;
        }
        if (TileSelector.instance.debris.Count > 0)
        {
            DebrisTile.instance.DestroyAllDebris();
            TileSelector.instance.debrisNum = 0;
        }

        loadTreeList.Clear();
        loadAnimalList.Clear();

        StatsController.instance.SetCoins(50f);
        StatsController.instance.SetLvl(1);
        StatsController.instance.SetExp(0);
        StatsController.instance.UpdateStats();

        TileSelector.instance.GenerateAllZones();
        TileSelector.instance.SpawnDebrisOnMap();
    }

    public void TimeSkip()
    {
        foreach (GameObject plotObj in TileSelector.instance.plots)
        {
            DirtTile dirt = plotObj.GetComponent<DirtTile>();
            if (dirt.crop.HasCrop() && (dirt.crop.state == CropState.Planted || dirt.crop.state == CropState.Growing))
            {
                dirt.crop.CancelGrowth();
                dirt.crop.state = CropState.Done;
                dirt.UpdateSprite(dirt);
            }
        }

        foreach (GameObject treeObj in TileSelector.instance.trees)
        {
            TreeTile treeTile = treeObj.GetComponent<TreeTile>();
            if (treeTile.tree.HasTree() && (treeTile.tree.treeState == TreeState.Planted || treeTile.tree.treeState == TreeState.Growing))
            {
                treeTile.tree.CancelGrowth();
                treeTile.tree.treeState = TreeState.Done;
                treeTile.UpdateTreeSprite(treeTile);
            }
        }

        foreach (GameObject animalObj in TileSelector.instance.animals)
        {
            AnimalTile animalTile = animalObj.GetComponent<AnimalTile>();
            if (animalTile.animal.HasAnimal() && animalTile.animal.animalState == AnimalState.Growing)
            {
                animalTile.animal.CancelGrowth();
                animalTile.animal.animalState = AnimalState.Done;
                animalTile.UpdateAnimalSprite(animalTile);
            }
        }

        Debug.Log("Time skip applied.");
    }

    private float CalcTimePassed(float currentGrowthTime, string oldTimeString, float cropTimeTotal, DirtTile dirt)
    {
        float newTime;
        DateTime oldTime = DateTime.Parse(oldTimeString);
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
    private float CalcTimePassed(float currentGrowthTime, string oldTimeString, float cropTimeTotal)
    {
        float newTime;
        DateTime oldTime = DateTime.Parse(oldTimeString);
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


