using UnityEngine;

[System.Serializable]
public class Animal
{
    public AnimalAsset asset;
    public AnimalState animalState;
    private float growthLevel;

    public bool AnimalGrow(float amount, AnimalTile animal)
    {
        if (!animal.isBusy)
        {
            growthLevel += amount / asset.animalTimer;
            if (growthLevel >= 1f)
            {
                animalState = AnimalState.Done;
                return true;
            }
        }
        return false;
    }

    public float GetGrowthLvl()
    {
        return growthLevel;
    }

    public void SetGrowthLvl(float time)
    {
        growthLevel = time;
    }

    public Animal(AnimalAsset a)
    {
        asset = a;
        animalState = AnimalState.Growing;
        growthLevel = 0;
    }

    public bool HasAnimal()
    {
        if(asset == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public int GetCost()
    {
        return asset.animalCost;
    }

    public Sprite GetAnimalSprite(Animal a)
    {
        if (a.asset == null)
            return null;

        switch (a.animalState)
        {
            case AnimalState.Growing:
                return a.asset.animalGrowingSprite;
            case AnimalState.Done:
                return a.asset.animalDoneSprite;
        }

        MenuController.instance.notificationBar.SetActive(false);
        MenuController.instance.AnimateNotifcation("Animal State Error", Color.red, "Error");

        return asset.animalGrowingSprite;
    }

    public string GetState()
    {
        string saveState = "Growing";
        switch (animalState)
        {
            case AnimalState.Growing:
                saveState = "Growing";
                break;
            case AnimalState.Done:
                saveState = "Done";
                break;
        }
        return saveState;
    }

    public AnimalState GetState(string state)
    {
        AnimalState loadedState = AnimalState.Growing;
        switch (state)
        {
            case "Growing":
                loadedState = AnimalState.Growing;
                break;
            case "Done":
                loadedState = AnimalState.Done;
                break;
        }
        return loadedState;
    }

    public string GetName()
    {
        if (asset == null)
            return null;

        return asset.name;
    }
        

    
}

public enum AnimalState
    {
        Growing,
        Done
    }