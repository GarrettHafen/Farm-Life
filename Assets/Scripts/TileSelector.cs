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

    public GameObject plot;
    public GameObject baseTree;
    public GameObject baseAnimal;
    public List<GameObject> plots;
    public List<GameObject> trees;
    public List<GameObject> animals;
    public GameObject plotParent;
    public GameObject treeParent;
    public GameObject animalParent;
    public int plotNum = 0;
    public int treeNum = 0;
    public int animalNum = 0;

    public Tree tree;
    public Animal animal;



    void Start() 
    {
        instance = this;
        SetupGrid();

        
        //used for writing info to a file
        //WriteGridToFile();
    }
    
    public DirtTile PlacePlot(Vector3 plotPosition, Vector3 offset)
    {
        plotPosition.y -= offset.y;//.25f;
        GameObject tempPlot = (GameObject)Instantiate(plot, plotPosition, transform.rotation); 
        tempPlot.name = "Plot: " + plotNum;
        plotNum++;
        tempPlot.SetActive(true);
        tempPlot.transform.SetParent(plotParent.transform);
        plots.Add(tempPlot);
        return tempPlot.GetComponent<DirtTile>();
    }

    public TreeTile PlantTree(Vector3 mousePosition, Tree t, PlayerInteraction player, Vector3 offset)
    {
        mousePosition.y += offset.y;//0.16f
        mousePosition.x += offset.x; //0.023f;
        GameObject tempTree = (GameObject)Instantiate(baseTree, mousePosition, transform.rotation);
        tempTree.name = t.asset.name + " " + treeNum;
        treeNum++;
        tempTree.SetActive(true);
        tempTree.transform.SetParent(treeParent.transform);
        trees.Add(tempTree);
        tree = t;
        player.SetTree(new Tree(t.asset));
        TreeTile treeTile = tempTree.GetComponent<TreeTile>();
        treeTile.tree = t;
        treeTile.UpdateTreeSprite(treeTile);

        return treeTile;

    }

    public AnimalTile PlaceAnimal(Vector3 mousePosition, Animal a, PlayerInteraction player, Vector3 offset)
    {

        mousePosition.y += offset.y;
        mousePosition.x += offset.x;
        GameObject tempAnimal = (GameObject)Instantiate(baseAnimal, mousePosition, transform.rotation);
        tempAnimal.name = a.asset.name + " " + animalNum;
        animalNum++;
        tempAnimal.SetActive(true);
        tempAnimal.transform.SetParent(animalParent.transform);
        animals.Add(tempAnimal);
        animal = a;
        player.SetAnimal(new Animal(a.asset));
        AnimalTile animalTile = tempAnimal.GetComponent<AnimalTile>();
        animalTile.animal = a;
        animalTile.UpdateAnimalSprite(animalTile);

        return animalTile;
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