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

	public string onGroundLayer;
	public string normalCropLayer;

	private void Start()
	{
		//for testing if the sprites worked 6/29/2020
		//CheckFallow();

		instance = this;
	}

	public void Interact (Crop c, /*Tool t, */PlayerInteraction player, DirtTile dirt)
	{
		if (MenuController.instance.hasSeed && c.HasCrop())
		{
			if (!needsPlowing)
				PlantSeed(c, player, dirt);
			else
			{
				Debug.Log("Ground needs plowing!");
				MenuController.instance.notificationBar.SetActive(false);
				MenuController.instance.AnimateNotifcation("Ground needs plowing!", Color.red, "Error");
			}
			return;

		}
		if (/*t.toolType == ToolType.Plow &&*/ needsPlowing)
		{
			Plow();

		}
        else if (!needsPlowing && !dirt.crop.HasCrop() && !MenuController.instance.fireTool)
        {
			
			MarketController.instance.ActivateMarket();
        }
		else if (/*t.toolType == ToolType.Harvest &&*/ dirt.crop.HasCrop() && dirt.crop.state == CropState.Done)
		{
			HarvestCrop(player);
		}
        else if (MenuController.instance.fireTool)
        {
            //need confirmation message
			DestroyPlot(dirt);
        }

		return;
	}

	void PlantSeed (Crop c, PlayerInteraction player, DirtTile dirt)
	{
		if (dirt.crop.asset != null && dirt.crop.state != CropState.Seed)
		{
				Debug.Log("Crop not seed, can't plan't.");
			MenuController.instance.notificationBar.SetActive(false);
			MenuController.instance.AnimateNotifcation("Can't Plant Seed", Color.red, "Error");
			return;
		}
		if (!StatsController.instance.RemoveCoins(c.asset.cropCost))
		{
			//poor message
			return;
        }
		//Debug.Log("Planting " + c.GetName());
		crop = c;
		crop.state = CropState.Planted;

		UpdateSprite();
		FindObjectOfType<AudioManager>().PlaySound("Seed");

		player.SetCrop(new Crop(c.asset));
	}

	void HarvestCrop (PlayerInteraction player)
	{
		if (crop.state == CropState.Done || crop.state == CropState.Dead)
		{
			StatsController.instance.AddCoins(crop.asset.cropReward);
			StatsController.instance.AddExp(crop.asset.expReward);
			crop = new Crop(null);
			needsPlowing = true;
			AddDirt();
			FindObjectOfType<AudioManager>().PlaySound("Harvest");
            if (PlayerInteraction.instance.timer.gameObject.activeInHierarchy)
            {
				PlayerInteraction.instance.timer.slider.gameObject.SetActive(false);
            }
		}
	}

	public void AddDirt()
	{
		overlay.sprite = fallowed;
		overlay.sortingLayerName = onGroundLayer;
	}

	void Plow ()
	{
		//Debug.Log("Plowing...");
		if (StatsController.instance.RemoveCoins(5))
		{
			//	if plots should cost money...
			overlay.sprite = null;
			needsPlowing = false;
			FindObjectOfType<AudioManager>().PlaySound("Plow");
		}
		
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
				bool isDone = crop.Grow(Time.deltaTime, this);
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

    public void DestroyPlot(DirtTile plotToDestroy)
    {
		overlay.sprite = null;
		needsPlowing = false;
        crop = new Crop(null);
        foreach(GameObject plot in TileSelector.instance.plots)
        {
            if (plot.Equals(plotToDestroy.gameObject))
            {
				TileSelector.instance.plots.Remove(plot);
				Debug.Log("element removed");
				break;
            }
        }
		Object.Destroy(plotToDestroy.gameObject);
    }
	public void DestroyPlots()
	{
		overlay.sprite = null;
		needsPlowing = false;
		crop = new Crop(null);
		int temp = TileSelector.instance.plots.Count;
		foreach (GameObject plot in TileSelector.instance.plots)
		{
			Object.Destroy(plot);
			
		}
		TileSelector.instance.plots.Clear();

        /* broken for some reason
		for(int i = 0; i < temp; i++)
        {
			TileSelector.instance.plots.Remove(TileSelector.instance.plots[i]);
        }*/
	}

}
