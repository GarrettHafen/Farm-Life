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

	public bool Grow(float amount, DirtTile dirt)
	{
		growthLevel += amount / asset.cropTimer;
		if (growthLevel >= 1f)
		{
			state = CropState.Done;
			return true;
		}else if (growthLevel <= 1f && growthLevel >= .75f)
        {
			state = CropState.Growing;
			if(dirt.overlay.sprite = asset.seedSprite)
            {
				dirt.UpdateSprite();
            }
			return false;
        }
		return false;
	}

	public float GetGrowthLvl()
    {
		return growthLevel;
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

	public Sprite GetCropSprite()
	{
		if (asset == null)
			return null;

		switch (state)
		{
			case CropState.Seed:
				return asset.seedSprite;
			case CropState.Planted:
				return asset.seedSprite;
			case CropState.Growing://displays the sprout sprite
				return asset.sproutSprite;
			case CropState.Dead:
				return asset.deadSprite;
			case CropState.Done:
				return asset.doneSprite;
		}

		Debug.LogError("WHAT?!");
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
