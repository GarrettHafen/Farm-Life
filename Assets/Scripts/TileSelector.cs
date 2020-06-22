using System.Collections;
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
    public Grid grid2;

    public Tilemap tilemapGround;
    private List<Vector3> availablePlaces;
    private List<Vector3Int> localPlaces;
    private int intendedPlotPosition;
    private List<int> currentPlotPositionsActive = new List<int>();

    

    public GameObject plot;
    public List<GameObject> plots;
    public GameObject plotParent;
    public bool plowActive;


    void Start() //-----------------------------------------------------------------------
    {
        //testing


        //testing

        instance = this;
        SetupGrid();

        //used for writing info to a file
        //WriteGridToFile();

        for(int i = 0; i < availablePlaces.Count; i++)
        {
            PlacePlot(availablePlaces[i]);
            
        }
    }

    void Update() 
    {
        if (Input.GetButtonDown("Fire1") && plowActive)
        {
            if (!GameHandler.instance.overMenu)
            {
                GetPlotPosition();
            }
        }
        if(Input.GetKeyDown(KeyCode.J)){
            plowActive = false;
            MenuController.instance.ResetCursor();
        }
    }

    private void GetPlotPosition()
    {
        /*
         * get mouse position
         * get tile position at mouse position
         * check if plot should go there
         * place plot on tile position
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
                Debug.Log("cant do that");
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

        plots[intendedPlotPosition].SetActive(true);
            
        //add EXP
        StatsController.instance.AddExp(10);
    }

    private void PlacePlot(Vector3 plotPosition)
    {

        /*GOOD STUFF DON'T DELETE*/

        //add .5 for some reason i don't understand
        plotPosition.y += .5f;
        GameObject tempPlot = (GameObject)Instantiate(plot, plotPosition, transform.rotation);
        tempPlot.SetActive(false);
        tempPlot.transform.SetParent(plotParent.transform);
        plots.Add(tempPlot);
        
            
            
        

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
            for (int yy = 0; yy >= tilemapGround.cellBounds.yMin; yy--)
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
    }

    private void WriteGridToFile()
    {
        /*--------------------------------------------------------------------
        *write all the values to a file for easy viewing*/
        string path = "Assets/test2.txt";
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
        writer.Close();
        /*--------------------------------------------------------------------*/
    }
}
