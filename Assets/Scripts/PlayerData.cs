using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class PlayerData
{
    public int level;
    public int exp;
    public float coins;
    public int[] activePlots;
    public string[] activeCrops;
    public float[] activeTimers;
    public string[] activeCropsStates;
    
    private int numActive = TileSelector.instance.currentPlotPositionsActive.Count;

    
    public PlayerData ()
    {
        level = StatsController.instance.GetLvl();
        exp = StatsController.instance.GetExp();
        coins = StatsController.instance.GetCoins();
        //----------------plots Array----------------
        activePlots = new int[numActive];
        for(int i = 0; i < numActive; i++)
        {
            activePlots[i] = TileSelector.instance.currentPlotPositionsActive[i];
        }

        //----------------crops/timers Array----------------
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

    }
}
