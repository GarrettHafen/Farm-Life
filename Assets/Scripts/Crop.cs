using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Crop 
{
	public CropAsset asset;

	public CropState state;

	private float growthLevel;
    private bool isDead;

	private float witherTimer;

	public bool Grow(float amount, DirtTile dirt)
	{
		growthLevel += amount / asset.cropTimer;
		if (growthLevel >= 1f)
		{
			state = CropState.Done;
			return true;
		}else if (growthLevel <= 1f && growthLevel >= .5f)
        {
			state = CropState.Growing;
			if(dirt.overlay.sprite = asset.seedSprite)
            {
				dirt.UpdateSprite(dirt);
            }
			return false;
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

	public Crop (CropAsset a) {
		asset = a;
		state = CropState.Seed;
		growthLevel = 0f;
		isDead = false;
	}

	public bool HasCrop()
	{
		if (asset == null)
			return false;
		else
			return true;
	}

	public Sprite GetCropSprite(Crop c)
	{
		if (c.asset == null)
			return null;

		switch (state)
		{
			case CropState.Seed:
				return c.asset.seedSprite;
			case CropState.Planted:
				return c.asset.seedSprite;
			case CropState.Growing://displays the sprout sprite
				return c.asset.sproutSprite;
			case CropState.Dead:
				return c.asset.deadSprite;
			case CropState.Done:
				return c.asset.doneSprite;
		}

		Debug.LogError("WHAT?!");
        MenuController.instance.notificationBar.SetActive(false);
		MenuController.instance.AnimateNotifcation("Seed State Error", Color.red, "Error");

		return asset.seedSprite;
	}

	public bool IsOnGround()
	{
		if (state == CropState.Planted && asset.seedIsOnGround)
			return true;
		else
			return false;
	}

	public Sprite GetDoneSprite()
	{
		return asset.doneSprite;
	}

	public string GetName()
	{
		if (asset == null)
			return null;

		return asset.name;
	}
	public void SetWitherTimer(float pastGrown)
    {
		witherTimer = pastGrown;
    }
	public string GetState()
	{
		string saveState = "Seed";
		switch (state)
		{
			case CropState.Seed:
				saveState = "Seed";
				break;
			case CropState.Planted:
				saveState = "Planted";
				break;
			case CropState.Growing:
				saveState = "Growing";
				break;
			case CropState.Dead:
				saveState = "Dead";
				break;
			case CropState.Done:
				saveState = "Done";
				break;
		}
		return saveState;
	}
	public CropState GetState(string state)
    {
		CropState loadedState = CropState.Seed;
        switch (state)
        {
			case "Seed":
				loadedState = CropState.Seed;
				break;
			case "Planted":
				loadedState = CropState.Planted;
				break;
			case "Growing":
				loadedState = CropState.Growing;
				break;
			case "Dead":
				loadedState = CropState.Dead;
				break;
			case "Done":
				loadedState = CropState.Done;
				break;
		}
		return loadedState;
	}
}

public enum CropState
{
	Seed, //may not be needed
	Planted,
	Growing,
	Dead,
	Done
		//plowed = DirtTile !needsPlowing
		//fallowed = DirtTile needsPlowing
}
