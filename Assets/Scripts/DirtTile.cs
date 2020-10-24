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
		instance = this;
	}

	public void Interact (Crop c,PlayerInteraction player, DirtTile dirt)
	{
		if (MenuController.instance.hasSeed && c.HasCrop())
		{
			//queue PLANTING


			if (!needsPlowing)
			{
				//check if enough coins to perform task
				if (StatsController.instance.CheckMaster(c.asset.cropCost))
                {
					//if enough coins, decrement master
					StatsController.instance.RemoveCoinsMaster(c.asset.cropCost);

					// queue task
					QueueTaskSystem.instance.SetTask(c, player, dirt);

					// after task completes, see PlantSeed(c, player, dirt);
                }
                else
                {
					//error notification and sound
					MenuController.instance.notificationBar.SetActive(false);
					MenuController.instance.AnimateNotifcation("Insufficient Funds", Color.red, "No Money");
				}
			}
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
			// queue SECOND PLOW

			//check if enough oins to perfom task
			if (StatsController.instance.CheckMaster(5))
			{
				//if enough coins, decrement master
				StatsController.instance.RemoveCoinsMaster(5);

				//change overlay
				parentSprite = dirt.GetComponent<SpriteRenderer>();
				childSprites = dirt.GetComponentsInChildren<SpriteRenderer>();
				parentSprite.color = new Color(1f, 1f, 1f, .5f);
				childSprites[1].color = new Color(1f, 1f, 1f, .5f);

				//queue task
				QueueTaskSystem.instance.SetTask("secondPlow", dirt);

				//after task completes see Plow(dirt)
            }
            else
            {
				//error notification and sound
				MenuController.instance.notificationBar.SetActive(false);
				MenuController.instance.AnimateNotifcation("Insufficient Funds", Color.red, "No Money");
			}
		}
        else if (!needsPlowing && !dirt.crop.HasCrop() && !MenuController.instance.fireTool)
        {
			
			MarketController.instance.ActivateMarket();
        }
		else if (dirt.crop.HasCrop() && dirt.crop.state == CropState.Done && !MenuController.instance.fireTool)
		{
			//queue HARVEST

			//set overlay and parent sprite opacity to .5
			parentSprite = dirt.GetComponent<SpriteRenderer>();
            parentSprite.color = new Color(1f, 1f, 1f, .25f);
			childSprites = dirt.GetComponentsInChildren<SpriteRenderer>();
			/*
			 * HARD CODE ALERT!!!!! DON'T KNOW WHY DEPTH FIRST SEARCH ISN'T PICKING UP THE CHILD SPRITE RENDERER
			 * 
			 */
			childSprites[1].color = new Color(1f, 1f, 1f, .5f);
			
			//queue timer
			QueueTaskSystem.instance.SetTask("harvestCrop", dirt);
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

	public void PlantSeed (Crop c, PlayerInteraction player, DirtTile dirt)
	{
		if (dirt.crop.asset != null && dirt.crop.state != CropState.Seed)
		{
			Debug.Log("Crop not seed, can't plan't.");
			MenuController.instance.notificationBar.SetActive(false);
			MenuController.instance.AnimateNotifcation("Can't Plant Seed", Color.red, "Error");
			return;
		}
		crop = c;
		crop.state = CropState.Planted;

		UpdateSprite();

		StatsController.instance.AddExp(1);
		StatsController.instance.RemoveCoinsDisplay(c.asset.cropCost);
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

	public void Plow (DirtTile dirt)
	{
		parentSprite = dirt.GetComponent<SpriteRenderer>();
		childSprites = dirt.GetComponentsInChildren<SpriteRenderer>();
		parentSprite.color = new Color(1f, 1f, 1f, 1f);
		childSprites[1].color = new Color(1f, 1f, 1f, 1f);
		dirt.overlay.sprite = null;
		dirt.needsPlowing = false;
		StatsController.instance.RemoveCoinsDisplay(5);
		StatsController.instance.AddExp(1);
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
