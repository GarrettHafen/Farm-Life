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

	private List<System.Action> _growthCallbacks = new List<System.Action>();
	private System.Action _doneCallback;

	public void StartGrowth(DirtTile dirt)
	{
		CancelGrowth();

		int n = (asset.growthSprites != null) ? asset.growthSprites.Count : 0;
		int halfIdx = n / 2;

		// Schedule a sprite update at each growth sprite boundary
		for (int i = 1; i < n; i++)
		{
			float t = growthStartTime + asset.cropTimer * ((float)i / n);
			int capturedI = i;
			if (t > Time.time)
			{
				System.Action cb = () =>
				{
					if (state == CropState.Done || state == CropState.Dead) return;
					if (capturedI >= halfIdx && state == CropState.Planted)
						state = CropState.Growing;
					dirt.UpdateSprite(dirt);
				};
				_growthCallbacks.Add(cb);
				GrowthManager.instance.Register(t, cb);
			}
			else
			{
				if (capturedI >= halfIdx && state == CropState.Planted)
					state = CropState.Growing;
			}
		}

		float doneTime = growthStartTime + asset.cropTimer;
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
		foreach (var cb in _growthCallbacks)
			GrowthManager.instance.Cancel(cb);
		_growthCallbacks.Clear();

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
			case CropState.Dead:
				return c.asset.deadSprite;
			case CropState.Done:
				return c.asset.doneSprite;
			default:
				var sprites = c.asset.growthSprites;
				if (sprites == null || sprites.Count == 0) return null;
				float lvl = state == CropState.Seed ? 0f : GetGrowthLvl();
				int idx = Mathf.Clamp(Mathf.FloorToInt(lvl * sprites.Count), 0, sprites.Count - 1);
				return sprites[idx];
		}
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
