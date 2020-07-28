using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class GameHandler : MonoBehaviour
{
    public static GameHandler instance;

    public CameraFollow cameraFollow;
    public Transform playerTransform;
    public Transform manualMovementTransform;

    private float autoSaveTimer;
    private int autoSaveInterval = 70;

    private Vector3 cameraFollowPostion;

    private float zoom = 1.25f;

    private bool edgeScrolling;

    public bool overMenu = false;

    public Tool PlowTool;

    public List<CropAsset> cropsList;

    private List<CropAsset> loadCropList = new List<CropAsset>();

    //plow pointer will be a hoe, default will be a gloved hand, harvest will be a scythe
    //the planting pointer will be dependant on which seed is selected
    public Texture2D plowPointer, defaultPointer, harvestPointer, plantingPointer;
    //might not need all of these

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        cameraFollowPostion = new Vector3(cameraFollowPostion.x + 4.5f, cameraFollowPostion.y + .5f, cameraFollowPostion.z);
        cameraFollow.Setup(() => cameraFollowPostion, () => zoom);

    }

    // Update is called once per frame
    void Update()
    {
        float moveAmount = 5f;
        float edgeSize = 10f;

        HandleManualMovement(moveAmount);
        HandleScreenEdges(edgeSize, moveAmount);
        HandleZoom();

        if (Input.GetKeyDown(KeyCode.U))
        {
            SaveSystem.SavePlayer();
            Debug.Log("Save Complete");
        }else if (Input.GetKeyDown(KeyCode.L))
        {
            LoadData(SaveSystem.LoadPlayer());
        }

        if(autoSaveTimer < autoSaveInterval)
        {
            autoSaveTimer += Time.deltaTime;
        }
        else
        {
            SaveSystem.SavePlayer();
            Debug.Log("Save Complete");
            autoSaveTimer = 0;
        }



    }

    private void HandleZoom()
    {
        float zoomChangeAmount = .25f;
        if (Input.GetKey(KeyCode.KeypadPlus))
        {
            zoom -= zoomChangeAmount;
        }
        if (Input.GetKey(KeyCode.KeypadMinus))
        {
            zoom += zoomChangeAmount;
        }

        if(Input.mouseScrollDelta.y > 0){
            zoom -= zoomChangeAmount;
        }
        if(Input.mouseScrollDelta.y < 0)
        {
            zoom += zoomChangeAmount;
        }
        zoom = Mathf.Clamp(zoom, .5f, 5f);
    }

    private void HandleScreenEdges(float edgeSize, float moveAmount)
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            edgeScrolling = !edgeScrolling;
            Debug.Log("Edge Scroll enabled: " + edgeScrolling);
        }
        if (edgeScrolling)
        {
            if (Input.mousePosition.x > Screen.width - edgeSize)
            {
                cameraFollowPostion.x += moveAmount * Time.deltaTime;
            }
            if (Input.mousePosition.x < edgeSize)
            {
                cameraFollowPostion.x -= moveAmount * Time.deltaTime;
            }
            if (Input.mousePosition.y > Screen.height - edgeSize)
            {
                cameraFollowPostion.y += moveAmount * Time.deltaTime;
            }
            if (Input.mousePosition.y < edgeSize)
            {
                cameraFollowPostion.y -= moveAmount * Time.deltaTime;
            }
        }
    }

    private void HandleManualMovement(float moveAmount)
    {
        if (Input.GetKey(KeyCode.W))
        {
            if (!CameraController.instance.atTopEdge) { 
            cameraFollowPostion.y += moveAmount * Time.deltaTime;
            }
        }
        if (Input.GetKey(KeyCode.S))
        {
            if (!CameraController.instance.atBottomEdge)
            {
                cameraFollowPostion.y -= moveAmount * Time.deltaTime;
            }
        }
        if (Input.GetKey(KeyCode.D))
        {
            if (!CameraController.instance.atRightEdge)
            {
                cameraFollowPostion.x += moveAmount * Time.deltaTime;
            }
        }
        if (Input.GetKey(KeyCode.A))
        {
            if (!CameraController.instance.atLeftEdge)
            {
                cameraFollowPostion.x -= moveAmount * Time.deltaTime;
            }
        }
    }

    // for zoom buttons (obsolete)
    public void ZoomIn()
    {
        zoom -= 1f;
        if (zoom < .5f)
        {
            zoom = .5f;
        }
    }

    public void ZoomOut()
    {
        zoom += 1f;
        if (zoom > 5f)
        {
            zoom = 5f;
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
        //deactivate all current plots
        for (int j = 0; j < TileSelector.instance.currentPlotPositionsActive.Count; j++)
        {
            DirtTile dirt = TileSelector.instance.plots[TileSelector.instance.currentPlotPositionsActive[j]].GetComponent<DirtTile>();
            dirt.crop = null;
            TileSelector.instance.plots[TileSelector.instance.currentPlotPositionsActive[j]].SetActive(false);

        }

        TileSelector.instance.currentPlotPositionsActive.Clear();
        //----------------plots Array----------------
        for (int i = 0; i < data.activePlots.Length; i++)
        {
            //activate plots based on loadData
            TileSelector.instance.currentPlotPositionsActive.Add(data.activePlots[i]); // will this work if list is empty?
            TileSelector.instance.plots[TileSelector.instance.currentPlotPositionsActive[i]].SetActive(true);
        }

        //----------------crops/timers Array----------------
        for(int i = 0; i < data.activeCrops.Length; i++)
        {
            if(data.activeCrops[i] != null)
            {
                for(int j = 0; j < cropsList.Count; j++)
                {
                    if(data.activeCrops[i] == cropsList[j].cropName)
                    {
                        loadCropList.Add(cropsList[j]);
                    }
                }
            }
            else
            {
                loadCropList.Add(cropsList[0]);
            }
        }
        //attach crops to the plots
        for(int i = 0; i < TileSelector.instance.currentPlotPositionsActive.Count; i++)
        {
            DirtTile dirt = TileSelector.instance.plots[TileSelector.instance.currentPlotPositionsActive[i]].GetComponent<DirtTile>();
            if (loadCropList[i].name != "Blank")
            {
                dirt.crop = new Crop(loadCropList[i]);
                dirt.crop.SetGrowthLvl(CalcTimePassed(data.activeTimers[i], data.savedTime, dirt.crop.asset.cropTimer, dirt));
                dirt.crop.state = dirt.crop.GetState(data.activeCropsStates[i]);
                dirt.crop.GetCropSprite();
                dirt.UpdateSprite();
            }
            else
            {
                dirt.crop = new Crop(null);
                dirt.crop.state = CropState.Seed;
                dirt.crop.GetCropSprite();
                dirt.UpdateSprite();
            }
        }
        Debug.Log("Data Loaded");
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


    //do we want to save on quit just yet?
    /*void OnApplicationQuit()
    {
        SaveSystem.SavePlayer();
        Debug.Log("Save Complete");
    }*/



}


