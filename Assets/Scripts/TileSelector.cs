﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TileSelector : MonoBehaviour
{
    public static TileSelector instance;
    public GridLayout grid;

    public Tilemap tilemapGround;
    private List<Vector3> availablePlaces; //transform
    private List<Vector3Int> localPlaces; //grid
    private int intendedPlotPosition; //obsolete
    public List<int> currentPlotPositionsActive = new List<int>(); //obsolete

    public GameObject plot;
    public GameObject baseTree;
    public List<GameObject> plots;
    public List<GameObject> trees;
    public GameObject plotParent;
    public GameObject treeParent;
    public int plotNum = 0;
    public int treeNum = 0;

    public Tree tree;

    public float treeOffset;



    void Start() //-----------------------------------------------------------------------
    {
        instance = this;
        SetupGrid();

        

        //for (int i = 0; i < availablePlaces.Count; i++)
        //{
        //    PlacePlot(availablePlaces[i]);

        //}
        
        //used for writing info to a file
        //WriteGridToFile();
    }
    private void Update()
    {
        
    }

    public void GetPlotPosition()
    {
        /*
         * get mouse position
         * get tile position at mouse position
         * check if plot should go there
        */

        //get mouse position
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 2.0f;//always make on top? maybe?
        //get what is under mouse
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        //get object at mouse position
        Vector3 objectPos = ray.GetPoint(-ray.origin.z / ray.direction.z);
        //change to grid location
        Vector3Int cellIndex = grid.LocalToCell(objectPos);
        //compare grid location to list of available grid locations
        for (int i = 0; i < localPlaces.Count; i++)
        {
            if (localPlaces[i].x == cellIndex.x && localPlaces[i].y == cellIndex.y)
            {
                //this is the array index
                intendedPlotPosition = i;
            }
        }
        CheckPlot(intendedPlotPosition);


    }

    private void CheckPlot(int intendedPlotPosition)
    {
        //check if plot exists by keeping track of a list of ints passed through from the local places array. 
        for (int i = 0; i < currentPlotPositionsActive.Count; i++)
        {
            if (intendedPlotPosition == currentPlotPositionsActive[i])
            {
                //better code needs to go here one day

                return;
            }
        }
        //an array of active plot indexes based on localPlaces array. 
        currentPlotPositionsActive.Add(intendedPlotPosition);

        ActivatePlot(intendedPlotPosition);
    }

    private void ActivatePlot(int intendedPlotPosition)
    {
        // activate plot at plot position

        //if plots should cost money.... still on the fence
        if (StatsController.instance.RemoveCoins(5))
        {
            plots[intendedPlotPosition].SetActive(true);
            FindObjectOfType<AudioManager>().PlaySound("Plow");
        }
        //plots[intendedPlotPosition].SetActive(true);
        //FindObjectOfType<AudioManager>().PlaySound("Plow");
    }

    public void PlacePlot(Vector3 plotPosition, Vector3 offset)
    {
        //this is for creating all the plots and setting them as inactive until the player wants to activate them.
        /*GOOD STUFF DON'T DELETE*/

        //minus .25 for some reason i don't understand
        plotPosition.y -= offset.y;//.25f;
        GameObject tempPlot = (GameObject)Instantiate(plot, plotPosition, transform.rotation); //changed this code while trying to figure out why all the states were being synced, see 7/8/2020 2:00pm ish in trello. new code may not be needed.  
        //GameObject tempPlot = UnityEditor.PrefabUtility.InstantiatePrefab(plot as GameObject) as GameObject; // this code wouldn't build?
        //tempPlot.transform.position = plotPosition; // related to the above line
        tempPlot.name = "Plot: " + plotNum;
        plotNum++;
        tempPlot.SetActive(true);
        tempPlot.transform.SetParent(plotParent.transform);
        plots.Add(tempPlot);
    }

    public void PlantTree(Vector3 mousePosition, Tree t, PlayerInteraction player, Vector3 offset)
    {
        mousePosition.y += offset.y;//0.16f
        mousePosition.x += offset.x; //0.023f;
        GameObject tempTree = (GameObject)Instantiate(baseTree, mousePosition, transform.rotation);
        tempTree.name = t.asset.name + " " + treeNum;
        treeNum++;
        //Debug.Log("Tree Planted: " + t.asset.name);
        tempTree.SetActive(true);
        tempTree.transform.SetParent(treeParent.transform);
        trees.Add(tempTree);
        tree = t;
        //player.SetTree(new Tree(t.asset));
        TreeTile treeTile = tempTree.GetComponent<TreeTile>();
        treeTile.tree = t;
        treeTile.UpdateTreeSprite();

        FindObjectOfType<AudioManager>().PlaySound("Plow");


    }

    private void SetupGrid()
    {
        //two types of grids, the actual grid that goes from -9, 9 or something like that saved in availablePlaces
        //and the actual location of those grid squares, saved in localPlaces.
        availablePlaces = new List<Vector3>();
        localPlaces = new List<Vector3Int>();
        int i = 0;
        for (int xx = 0; xx < tilemapGround.cellBounds.xMax; xx++)
        {
            for (int yy = 0; yy > tilemapGround.cellBounds.yMin; yy--)
            {
                Vector3Int localPlace = (new Vector3Int(xx, yy, (int)tilemapGround.transform.position.y));
                Vector3 place = tilemapGround.CellToLocal(localPlace);
                i++;
                if (tilemapGround.HasTile(localPlace))
                {
                    availablePlaces.Add(place);
                    localPlaces.Add(localPlace);
                }
            }
        }
        Debug.Log("Grid Setup Complete");
    }

    private void WriteGridToFile()
    {
        /*--------------------------------------------------------------------
        *write all the values to a file for easy viewing*/
        string path = "Assets/test3.txt";
        StreamWriter writer = new StreamWriter(path, true);
        int count = 0;
        foreach (Vector3 element in localPlaces)
        {
            count++;
            writer.WriteLine("Local place #" + count + ": " + element);
        }
        count = 0;
        foreach (Vector3 element in availablePlaces)
        {

            count++;
            writer.WriteLine("Available place #" + count + ": " + element);
        }
        //count = 0;
        /* obsolete
         * foreach(GameObject element in plots)
        {
            count++;
            writer.WriteLine("Plot: " + count + " at loctaion: " + element.transform.position);
        }
        */
        writer.Close();
        Debug.Log("file created");
        /*--------------------------------------------------------------------*/
    }

    
}