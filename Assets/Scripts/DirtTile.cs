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
		if (c.HasCrop())
		{
			if (!needsPlowing)
				PlantSeed(c, player);
			else
				Debug.Log("Ground needs plowing!");

			return;
		}

		if (t != null)
		{
			TileSelector.instance.GetPlotPosition();
			if (t.toolType == ToolType.Plow && needsPlowing)
			{
				Plow();
			} else if (t.toolType == ToolType.Watercan && crop.state == CropState.Planted)
			{
				WaterCrop();
			}

			return;
		}

		if (crop.HasCrop())
		{
			HarvestCrop(player);
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

		player.SetCrop(new Crop(null));
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

	public void BirdEatsCrop()
	{
		crop = new Crop(null);
		needsPlowing = true;
		AddDirt();
	}

	void AddDirt()
	{
		overlay.sprite = fallowed;
		overlay.sortingLayerName = onGroundLayer;
	}

	void Plow ()
	{
		//Debug.Log("Plowing...");
		overlay.sprite = null;
		//needsPlowing = false;
	}

	void WaterCrop ()
	{
		if (crop.GetWaterState() == WaterState.Dry)
		{
			crop.Water();
			UpdateSprite();
			waterIndicator.SetActive(false);
		}
	}

	void UpdateSprite ()
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
        if (Input.GetKeyDown(KeyCode.Q))
        {
			CheckFallow();
        }
        /*Ray ray;
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		Debug.Log(Physics.Raycast(ray, out _));
*/
		if (crop.HasCrop())
		{
			if (crop.state == CropState.Planted)
			{
				bool isDone = crop.Grow(Time.deltaTime);
				if (isDone)
				{
					UpdateSprite();
				} else
				{
					WaterState state = crop.Dry(Time.deltaTime);
					if (state == WaterState.Dry)
					{
						waterIndicator.SetActive(true);
					} else if (state == WaterState.Dead)
					{
						UpdateSprite();
						waterIndicator.SetActive(false);
					}
				}
			}
		}
		//CheckFallow();
	}

	private void CheckFallow()
    {
		Ray ray;
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		Debug.Log(ray);
		Debug.Log(Physics.Raycast(ray, out _, 100f));
		if (needsPlowing)
        {
			AddDirt();
        }else if (!needsPlowing)
        {
			Plow();
        }
    }

}
