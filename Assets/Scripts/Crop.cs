using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Crop 
{
	public CropAsset asset;

	public CropState state;

	private float growthStartTime;
    private bool isDead;

	private float witherTimer;

	private System.Action _halfGrownCallback;
	private System.Action _doneCallback;

	public void StartGrowth(DirtTile dirt)
	{
		CancelGrowth();

		float halfTime = growthStartTime + asset.cropTimer * 0.5f;
		float doneTime = growthStartTime + asset.cropTimer;

		if (halfTime > Time.time)
		{
			_halfGrownCallback = () =>
			{
				if (state == CropState.Planted)
				{
					state = CropState.Growing;
					dirt.UpdateSprite(dirt);
				}
			};
			GrowthManager.instance.Register(halfTime, _halfGrownCallback);
		}
		else if (state == CropState.Planted)
		{
			state = CropState.Growing;
			dirt.UpdateSprite(dirt);
		}

		if (doneTime > Time.time)
		{
			_doneCallback = () =>
			{
				state = CropState.Done;
				dirt.UpdateSprite(dirt);
			};
			GrowthManager.instance.Register(doneTime, _doneCallback);
		}
		else if (state != CropState.Done)
		{
			state = CropState.Done;
			dirt.UpdateSprite(dirt);
		}
	}

	public void CancelGrowth()
	{
		if (_halfGrownCallback != null)
		{
			GrowthManager.instance.Cancel(_halfGrownCallback);
			_halfGrownCallback = null;
		}
		if (_doneCallback != null)
		{
			GrowthManager.instance.Cancel(_doneCallback);
			_doneCallback = null;
		}
	}

	public float GetGrowthLvl()
    {
		if (state == CropState.Done || state == CropState.Dead)
			return 1f;
		if (asset == null || state == CropState.Seed)
			return 0f;
		return Mathf.Clamp01((Time.time - growthStartTime) / asset.cropTimer);
    }

	public void SetGrowthLvl(float level)
    {
		if (asset != null)
			growthStartTime = Time.time - level * asset.cropTimer;
    }

	public Crop (CropAsset a) {
		asset = a;
		state = CropState.Seed;
		growthStartTime = 0f;
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
