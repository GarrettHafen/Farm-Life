using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirtTile : MonoBehaviour
{
	public static DirtTile instance;
	public Crop crop;

	public SpriteRenderer overlay;

	public bool needsPlowing = false;
	public Sprite fallowed;
	public GameObject waterIndicator;

	public string onGroundLayer;
	public string normalCropLayer;

	public bool hasCrow = false;

	private void Start()
	{
		//for testing if the sprites worked 6/29/2020
		//CheckFallow();

		instance = this;
	}

	public void Interact (Crop c, Tool t, PlayerInteraction player)
	{
		if (t != null)
		{
			if (t.toolType == ToolType.Plow && needsPlowing)
			{
				Plow();

			}else if(t.toolType == ToolType.Market && c.HasCrop())
            {
				if (!needsPlowing)
					PlantSeed(c, player);
				else
					Debug.Log("Ground needs plowing!");

				return;

			}else if(t.toolType == ToolType.Harvest && c.HasCrop())
            {
				HarvestCrop(player);
            }

			return;
		}
	}

	void PlantSeed (Crop c, PlayerInteraction player)
	{
		if (c.state != CropState.Seed)
		{
			Debug.Log("Crop not seed, can't plan't.");
			return;
		}
		Debug.Log("Planting " + c.GetName());
		crop = c;
		crop.state = CropState.Planted;

		UpdateSprite();

		//player.SetCrop(new Crop(null));
	}

	void HarvestCrop (PlayerInteraction player)
	{
		if (crop.state == CropState.Done || crop.state == CropState.Dead)
		{
			player.SetCrop(crop);
			crop = new Crop(null);
			needsPlowing = true;
			AddDirt();
		}
	}

	void AddDirt()
	{
		overlay.sprite = fallowed;
		overlay.sortingLayerName = onGroundLayer;
	}

	void Plow ()
	{
		Debug.Log("Plowing...");
		overlay.sprite = null;
		needsPlowing = false;
	}

	public void UpdateSprite ()
	{
		overlay.sprite = crop.GetCropSprite();
		if (crop.IsOnGround())
		{
			overlay.sortingLayerName = onGroundLayer;
		} else
		{
			overlay.sortingLayerName = normalCropLayer;
		}
	}

	private void Update()
	{
		if (crop.HasCrop())
		{
			if (crop.state == CropState.Planted || crop.state == CropState.Growing)
			{
				bool isDone = crop.Grow(Time.deltaTime);
				if (isDone)
				{
					UpdateSprite();
				}
			}
		}
		//CheckFallow();
	}

	private void CheckFallow()
    {
		
		if (needsPlowing)
        {
			AddDirt();
        }else if (!needsPlowing)
        {
			Plow();
        }
    }

}
