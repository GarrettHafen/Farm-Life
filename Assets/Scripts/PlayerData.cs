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
    public string[] activeCropNames;        // current crop name attached to plot
    public float[] activeTimers;        // current growth timer of the crop attached to a plot
    public string[] activeCropsStates;  // current crop state of the crop attached to a plot
    public bool[] needsPlowing;         // list of bools for if a plot needs plowing, to fix a bug where fallow plots were plowed on load
    public DateTime savedTime;          // time when the game is saved, used to calculate time passed when game is loaded


    //private int numActive = TileSelector.instance.currentPlotPositionsActive.Count; // old count of plots currently existing, based on TileSelector.plots[] 
    public int numActive; 
    

    public PlayerData ()
    {
        int counter = 0; //needed for the for each loop, seems dumb....
        numActive = TileSelector.instance.plots.Count;
        savedTime = DateTime.Now;
        level = StatsController.instance.GetLvl();
        exp = StatsController.instance.GetExp();
        coins = StatsController.instance.GetCoins();

        //set size of arrays
        plotPositionX = new float[numActive];
        plotPositionY = new float[numActive];
        activeCropNames = new string[numActive];
        activeTimers = new float[numActive];
        activeCropsStates = new string[numActive];
        needsPlowing = new bool[numActive];

        //----------------plots Array----------------
        foreach (GameObject plot in TileSelector.instance.plots)
        {
            plotPositionX[counter] = plot.transform.position.x;
            plotPositionY[counter] = plot.transform.position.y;
            DirtTile dirt = plot.GetComponent<DirtTile>();
            if (dirt.crop.HasCrop())
            {
                activeCropNames[counter] = dirt.crop.GetName();
                activeTimers[counter] = dirt.crop.GetGrowthLvl();
            }
            else
            {
                activeCropNames[counter] = null;
                activeTimers[counter] = -1f;
            }
            activeCropsStates[counter] = dirt.crop.GetState();
            needsPlowing[counter] = dirt.needsPlowing;
            counter++;
        }

        /* old code based on all plots intialized at start
        activePlots = new int[numActive];
        
        for(int i = 0; i < numActive; i++)
        {
            activePlots[i] = TileSelector.instance.currentPlotPositionsActive[i];
        }
        */

        //new code based on player intializing plots

        //----------------crops/timers Array----------------
        /* old code based on all plots intialized at start
        activeCrops = new string[numActive];
        activeTimers = new float[numActive];
        activeCropsStates = new string[numActive];
        for(int i = 0; i < numActive; i++)
        {
            DirtTile dirt = TileSelector.instance.plots[TileSelector.instance.currentPlotPositionsActive[i]].GetComponent<DirtTile>();
            if (dirt.crop.HasCrop())
            {
                activeCrops[i] = dirt.crop.GetName();
                activeTimers[i] = dirt.crop.GetGrowthLvl();
                activeCropsStates[i] = dirt.crop.GetState();
            }
            else
            {
                activeCrops[i] = null;
                activeTimers[i] = -1f;
            }
        }
        */

    }
}
