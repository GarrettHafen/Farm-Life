using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DirtTile : MonoBehaviour
{
	public static DirtTile instance;
	public Crop crop;

	public SpriteRenderer overlay;

	public bool needsPlowing = false;
	public Sprite fallowed;

	SpriteRenderer parentSprite;
	SpriteRenderer[] childSprites;

	private void Start()
	{
		//for testing if the sprites worked 6/29/2020
		//CheckFallow();

		instance = this;
	}

	public void Interact (Crop c,PlayerInteraction player, DirtTile dirt)
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
		if (needsPlowing && !MenuController.instance.fireTool)
		{
			if (StatsController.instance.RemoveCoins(5))
			{
				
				Plow();
			}
		}
        else if (!needsPlowing && !dirt.crop.HasCrop() && !MenuController.instance.fireTool)
        {
			
			MarketController.instance.ActivateMarket();
        }
		else if (dirt.crop.HasCrop() && dirt.crop.state == CropState.Done && !MenuController.instance.fireTool)
		{
			//set overlay and parent sprite opacity to .5
			parentSprite = dirt.GetComponent<SpriteRenderer>();
            parentSprite.color = new Color(1f, 1f, 1f, .5f);
			childSprites = dirt.GetComponentsInChildren<SpriteRenderer>();
			/*
			 * HARD CODE ALERT!!!!! DON'T KNOW WHY DEPTH FIRST SEARCH ISN'T PICKING UP THE CHILD SPRITE RENDERER
			 * 
			 */
			childSprites[1].color = new Color(1f, 1f, 1f, .5f);

			//queue timer
			QueueTaskSystem.instance.SetTask("harvestCrop", this);
				//slider set to active
				//while loop
				//slider set to inactive
				//call Harvest Crop
					//add money
					//change overlay and parent opacity to 1
					//change overlay sprite to fallow
		}
        else if (MenuController.instance.fireTool)
        {
			MenuController.instance.OpenFireMenu();
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

	public void HarvestCrop(DirtTile dirt)
	{
		if (dirt.crop.state == CropState.Done || dirt.crop.state == CropState.Dead)
		{
			StatsController.instance.AddCoins(dirt.crop.asset.cropReward);
			StatsController.instance.AddExp(dirt.crop.asset.expReward);
			dirt.crop = new Crop(null);
			dirt.needsPlowing = true;
			parentSprite = dirt.GetComponent<SpriteRenderer>();
			parentSprite.color = new Color(1f, 1f, 1f, 1f);
			childSprites = dirt.GetComponentsInChildren<SpriteRenderer>();
			childSprites[1].color = new Color(1f, 1f, 1f, 1f);
			dirt.AddDirt();
			FindObjectOfType<AudioManager>().PlaySound("Harvest");
		}
	}

	public void AddDirt()
	{
		overlay.sprite = fallowed;
	}

	public void Plow ()
	{
		//Debug.Log("Plowing...");
		
			//	if plots should cost money...
			overlay.sprite = null;
			needsPlowing = false;
			FindObjectOfType<AudioManager>().PlaySound("Plow");
		
		
	}

	public void UpdateSprite ()
	{
		overlay.sprite = crop.GetCropSprite();
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
        foreach(GameObject plot in TileSelector.instance.plots)
        {
            if (plot.Equals(plotToDestroy.gameObject))
            {
				TileSelector.instance.plots.Remove(plot);
				break;
            }
        }
		Object.Destroy(plotToDestroy.gameObject);
		FindObjectOfType<AudioManager>().PlaySound("Destroy");

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
