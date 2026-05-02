using UnityEngine;

[System.Serializable]
public class Animal
{
    public AnimalAsset asset;
    public AnimalState animalState;
    private float growthStartTime;

    private System.Action _doneCallback;

    public void StartGrowth(AnimalTile tile)
    {
        CancelGrowth();

        float doneTime = growthStartTime + asset.animalTimer;

        if (doneTime > Time.time)
        {
            _doneCallback = () =>
            {
                animalState = AnimalState.Done;
                tile.UpdateAnimalSprite(tile);
            };
            GrowthManager.instance.Register(doneTime, _doneCallback);
        }
        else if (animalState != AnimalState.Done)
        {
            animalState = AnimalState.Done;
            tile.UpdateAnimalSprite(tile);
        }
    }

    public void CancelGrowth()
    {
        if (_doneCallback != null)
        {
            GrowthManager.instance.Cancel(_doneCallback);
            _doneCallback = null;
        }
    }

    public float GetGrowthLvl()
    {
        if (animalState == AnimalState.Done)
            return 1f;
        if (asset == null)
            return 0f;
        return Mathf.Clamp01((Time.time - growthStartTime) / asset.animalTimer);
    }

    public void SetGrowthLvl(float level)
    {
        if (asset != null)
            growthStartTime = Time.time - level * asset.animalTimer;
    }

    public Animal(AnimalAsset a)
    {
        asset = a;
        animalState = AnimalState.Growing;
        growthStartTime = 0f;
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