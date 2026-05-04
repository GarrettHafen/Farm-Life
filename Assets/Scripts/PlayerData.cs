using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class PlayerData
{
    public int level;
    public int exp;
    public float coins;
    public string savedTime;

    public string[] unlockedZoneNames;
    public ZoneTileData[] zoneTileData;

    public int cropsActive;
    public float[] plotX;
    public float[] plotY;
    public string[] activeCropNames;
    public float[] activeTimers;
    public string[] activeCropsStates;
    public bool[] needsPlowing;

    public int treesActive;
    public float[] treeX;
    public float[] treeY;
    public string[] activeTreeNames;
    public float[] activeTreeTimers;
    public string[] activeTreeStates;

    public int animalsActive;
    public float[] animalX;
    public float[] animalY;
    public string[] activeAnimalNames;
    public float[] activeAnimalTimers;
    public string[] activeAnimalStates;

    public int debrisActive;
    public float[] debrisX;
    public float[] debrisY;

    public PlayerData() { }

    public static PlayerData FromCurrentGameState()
    {
        var data = new PlayerData();
        Grid grid = TileSelector.instance.grid;

        data.savedTime = DateTime.Now.ToString("O");
        data.level = StatsController.instance.GetLvl();
        data.exp = StatsController.instance.GetExp();
        data.coins = StatsController.instance.GetCoins();
        data.unlockedZoneNames = TileSelector.instance.GetUnlockedZoneNames();
        data.zoneTileData = TileSelector.instance.GetZoneTileData();

        // crops
        int cropsCounter = 0;
        data.cropsActive = TileSelector.instance.plots.Count;
        data.plotX = new float[data.cropsActive];
        data.plotY = new float[data.cropsActive];
        data.activeCropNames = new string[data.cropsActive];
        data.activeTimers = new float[data.cropsActive];
        data.activeCropsStates = new string[data.cropsActive];
        data.needsPlowing = new bool[data.cropsActive];

        foreach (GameObject plot in TileSelector.instance.plots)
        {
            DirtTile dirt = plot.GetComponent<DirtTile>();
            data.plotX[cropsCounter] = dirt.snapPosition.x;
            data.plotY[cropsCounter] = dirt.snapPosition.y;
            if (dirt.crop.HasCrop())
            {
                data.activeCropNames[cropsCounter] = dirt.crop.GetName();
                data.activeTimers[cropsCounter] = dirt.crop.GetGrowthLvl();
            }
            else
            {
                data.activeCropNames[cropsCounter] = null;
                data.activeTimers[cropsCounter] = -1f;
            }
            data.activeCropsStates[cropsCounter] = dirt.crop.GetState();
            data.needsPlowing[cropsCounter] = dirt.needsPlowing;
            cropsCounter++;
        }

        // trees
        int treeCounter = 0;
        data.treesActive = TileSelector.instance.trees.Count;
        data.treeX = new float[data.treesActive];
        data.treeY = new float[data.treesActive];
        data.activeTreeNames = new string[data.treesActive];
        data.activeTreeTimers = new float[data.treesActive];
        data.activeTreeStates = new string[data.treesActive];

        foreach (GameObject treeObj in TileSelector.instance.trees)
        {
            TreeTile treeTile = treeObj.GetComponent<TreeTile>();
            data.treeX[treeCounter] = treeTile.snapPosition.x;
            data.treeY[treeCounter] = treeTile.snapPosition.y;
            if (treeTile.tree.HasTree())
            {
                data.activeTreeNames[treeCounter] = treeTile.tree.GetName();
                data.activeTreeTimers[treeCounter] = treeTile.tree.GetGrowthLvl();
            }
            else
            {
                data.activeTreeNames[treeCounter] = null;
                data.activeTreeTimers[treeCounter] = -1f;
            }
            data.activeTreeStates[treeCounter] = treeTile.tree.GetState();
            treeCounter++;
        }

        // animals
        int animalCounter = 0;
        data.animalsActive = TileSelector.instance.animals.Count;
        data.animalX = new float[data.animalsActive];
        data.animalY = new float[data.animalsActive];
        data.activeAnimalNames = new string[data.animalsActive];
        data.activeAnimalTimers = new float[data.animalsActive];
        data.activeAnimalStates = new string[data.animalsActive];

        foreach (GameObject animalObj in TileSelector.instance.animals)
        {
            AnimalTile animalTile = animalObj.GetComponent<AnimalTile>();
            data.animalX[animalCounter] = animalTile.snapPosition.x;
            data.animalY[animalCounter] = animalTile.snapPosition.y;
            if (animalTile.animal.HasAnimal())
            {
                data.activeAnimalNames[animalCounter] = animalTile.animal.GetName();
                data.activeAnimalTimers[animalCounter] = animalTile.animal.GetGrowthLvl();
            }
            else
            {
                data.activeAnimalNames[animalCounter] = null;
                data.activeAnimalTimers[animalCounter] = -1f;
            }
            data.activeAnimalStates[animalCounter] = animalTile.animal.GetState();
            animalCounter++;
        }

        // debris
        int debrisCounter = 0;
        data.debrisActive = TileSelector.instance.debris.Count;
        data.debrisX = new float[data.debrisActive];
        data.debrisY = new float[data.debrisActive];
        foreach (GameObject d in TileSelector.instance.debris)
        {
            DebrisTile dt = d.GetComponent<DebrisTile>();
            data.debrisX[debrisCounter] = dt.snapPosition.x;
            data.debrisY[debrisCounter] = dt.snapPosition.y;
            debrisCounter++;
        }

        return data;
    }
}
