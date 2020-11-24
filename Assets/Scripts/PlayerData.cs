using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class PlayerData
{
    public int level;                   // player level
    public int exp;                     // player exp to fill up the xp bar    
    public float coins;                 // player coins
    public int[] activePlots;           // **old code** becomes array of active plots with a size of numActive      
    public float[] plotPositionX;       // x position of a plot
    public float[] plotPositionY;       // y position of a plot
    public string[] activeCropNames;    // current crop name attached to plot
    public float[] activeTimers;        // current growth timer of the crop attached to a plot
    public string[] activeCropsStates;  // current crop state of the crop attached to a plot
    public bool[] needsPlowing;         // list of bools for if a plot needs plowing, to fix a bug where fallow plots were plowed on load
    public DateTime savedTime;          // time when the game is saved, used to calculate time passed when game is loaded

    public int[] activeTrees;
    public float[] treePositionX;
    public float[] treePositionY;
    public string[] activeTreeNames;
    public float[] activeTreeTimers;
    public string[] activeTreeStates;

    public int[] activeAnimals;
    public float[] animalPositionX;
    public float[] animalPositionY;
    public string[] activeAnimalNames;
    public float[] activeAnimalTimers;
    public string[] activeAnimalStates;

    public int cropsActive;
    public int treesActive;
    public int animalsActive;
    

    public PlayerData ()
    {

        savedTime = DateTime.Now;
        level = StatsController.instance.GetLvl();
        exp = StatsController.instance.GetExp();
        coins = StatsController.instance.GetCoins();


        //save data for crops
        int cropsCounter = 0; //needed for the for each loop, seems dumb....
        cropsActive = TileSelector.instance.plots.Count;
        
        //set size of arrays
        plotPositionX = new float[cropsActive];
        plotPositionY = new float[cropsActive];
        activeCropNames = new string[cropsActive];
        activeTimers = new float[cropsActive];
        activeCropsStates = new string[cropsActive];
        needsPlowing = new bool[cropsActive];

        //----------------plots Array----------------
        foreach (GameObject plot in TileSelector.instance.plots)
        {
            plotPositionX[cropsCounter] = plot.transform.position.x;
            plotPositionY[cropsCounter] = plot.transform.position.y;
            DirtTile dirt = plot.GetComponent<DirtTile>();
            if (dirt.crop.HasCrop())
            {
                activeCropNames[cropsCounter] = dirt.crop.GetName();
                activeTimers[cropsCounter] = dirt.crop.GetGrowthLvl();
            }
            else
            {
                activeCropNames[cropsCounter] = null;
                activeTimers[cropsCounter] = -1f;
            }
            activeCropsStates[cropsCounter] = dirt.crop.GetState();
            needsPlowing[cropsCounter] = dirt.needsPlowing;
            cropsCounter++;
        }

        //save data for trees
        int treeCounter = 0;
        treesActive = TileSelector.instance.trees.Count;
        treePositionX = new float[treesActive];
        treePositionY = new float[treesActive];
        activeTreeNames = new string[treesActive];
        activeTreeTimers = new float[treesActive];
        activeTreeStates = new string[treesActive];

        foreach(GameObject tree in TileSelector.instance.trees)
        {
            treePositionX[treeCounter] = tree.transform.position.x;
            treePositionY[treeCounter] = tree.transform.position.y;
            TreeTile treeTile = tree.GetComponent<TreeTile>();
            if (treeTile.tree.HasTree())
            {
                activeTreeNames[treeCounter] = treeTile.tree.GetName();
                activeTreeTimers[treeCounter] = treeTile.tree.GetGrowthLvl();
            }
            else
            {
                activeTreeNames[treeCounter] = null;
                activeTreeTimers[treeCounter] = -1f;
            }
            activeTreeStates[treeCounter] = treeTile.tree.GetState();
            treeCounter++;
        }

        //save data for animals
        int animalCounter = 0;
        animalsActive = TileSelector.instance.animals.Count;
        animalPositionX = new float[animalsActive];
        animalPositionY = new float[animalsActive];
        activeAnimalNames = new string[animalsActive];
        activeAnimalTimers = new float[animalsActive];
        activeAnimalStates = new string[animalsActive];

        foreach(GameObject animal in TileSelector.instance.animals)
        {
            animalPositionX[animalCounter] = animal.transform.position.x;
            animalPositionY[animalCounter] = animal.transform.position.y;
            AnimalTile animalTile = animal.GetComponent<AnimalTile>();
            if (animalTile.animal.HasAnimal())
            {
                activeAnimalNames[animalCounter] = animalTile.animal.GetName();
                activeAnimalTimers[animalCounter] = animalTile.animal.GetGrowthLvl();

            }
            else
            {
                activeAnimalNames[animalCounter] = null;
                activeAnimalTimers[animalCounter] = -1f;
            }
            activeAnimalStates[animalCounter] = animalTile.animal.GetState();
            animalCounter++;
        }
    }
}
