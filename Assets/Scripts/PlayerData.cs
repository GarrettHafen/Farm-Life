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
    public int[] plotCellX;
    public int[] plotCellY;
    public string[] activeCropNames;
    public float[] activeTimers;
    public string[] activeCropsStates;
    public bool[] needsPlowing;

    public int treesActive;
    public int[] treeCellX;
    public int[] treeCellY;
    public string[] activeTreeNames;
    public float[] activeTreeTimers;
    public string[] activeTreeStates;

    public int animalsActive;
    public int[] animalCellX;
    public int[] animalCellY;
    public string[] activeAnimalNames;
    public float[] activeAnimalTimers;
    public string[] activeAnimalStates;

    public int debrisActive;
    public int[] debrisCellX;
    public int[] debrisCellY;

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
        data.plotCellX = new int[data.cropsActive];
        data.plotCellY = new int[data.cropsActive];
        data.activeCropNames = new string[data.cropsActive];
        data.activeTimers = new float[data.cropsActive];
        data.activeCropsStates = new string[data.cropsActive];
        data.needsPlowing = new bool[data.cropsActive];

        foreach (GameObject plot in TileSelector.instance.plots)
        {
            Vector3Int cell = grid.WorldToCell(plot.transform.position);
            data.plotCellX[cropsCounter] = cell.x;
            data.plotCellY[cropsCounter] = cell.y;
            DirtTile dirt = plot.GetComponent<DirtTile>();
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
        data.treeCellX = new int[data.treesActive];
        data.treeCellY = new int[data.treesActive];
        data.activeTreeNames = new string[data.treesActive];
        data.activeTreeTimers = new float[data.treesActive];
        data.activeTreeStates = new string[data.treesActive];

        foreach (GameObject treeObj in TileSelector.instance.trees)
        {
            Vector3Int cell = grid.WorldToCell(treeObj.transform.position);
            data.treeCellX[treeCounter] = cell.x;
            data.treeCellY[treeCounter] = cell.y;
            TreeTile treeTile = treeObj.GetComponent<TreeTile>();
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
        data.animalCellX = new int[data.animalsActive];
        data.animalCellY = new int[data.animalsActive];
        data.activeAnimalNames = new string[data.animalsActive];
        data.activeAnimalTimers = new float[data.animalsActive];
        data.activeAnimalStates = new string[data.animalsActive];

        foreach (GameObject animalObj in TileSelector.instance.animals)
        {
            Vector3Int cell = grid.WorldToCell(animalObj.transform.position);
            data.animalCellX[animalCounter] = cell.x;
            data.animalCellY[animalCounter] = cell.y;
            AnimalTile animalTile = animalObj.GetComponent<AnimalTile>();
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
        data.debrisCellX = new int[data.debrisActive];
        data.debrisCellY = new int[data.debrisActive];
        foreach (GameObject d in TileSelector.instance.debris)
        {
            Vector3Int cell = grid.WorldToCell(d.transform.position);
            data.debrisCellX[debrisCounter] = cell.x;
            data.debrisCellY[debrisCounter] = cell.y;
            debrisCounter++;
        }

        return data;
    }
}
