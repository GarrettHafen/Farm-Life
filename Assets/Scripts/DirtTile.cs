using UnityEngine;

public class DirtTile : MonoBehaviour
{
	public static DirtTile instance;
	public Crop crop;

	public SpriteRenderer overlay;

	public bool needsPlowing = false;
	public bool isBusy = false;
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
					//change sprite opacity
					parentSprite = this.GetComponent<SpriteRenderer>();
					childSprites = this.GetComponentsInChildren<SpriteRenderer>();
					parentSprite.color = new Color(1f, 1f, 1f, .5f);
					childSprites[1].color = new Color(1f, 1f, 1f, .5f);

					// queue task
					dirt.isBusy = true;
					QueueTaskSystem.instance.SetTask(new Crop(c.asset), player, this);

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
				parentSprite = this.GetComponent<SpriteRenderer>();
				childSprites = this.GetComponentsInChildren<SpriteRenderer>();
				parentSprite.color = new Color(1f, 1f, 1f, .5f);
				childSprites[1].color = new Color(1f, 1f, 1f, .5f);

				dirt.isBusy = true;
				//queue task
				QueueTaskSystem.instance.SetTask("secondPlow", this);

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
			parentSprite = this.GetComponent<SpriteRenderer>();
            parentSprite.color = new Color(1f, 1f, 1f, .25f);
			childSprites = this.GetComponentsInChildren<SpriteRenderer>();
			/*
			 * HARD CODE ALERT!!!!! DON'T KNOW WHY DEPTH FIRST SEARCH ISN'T PICKING UP THE CHILD SPRITE RENDERER
			 * 
			 */
			childSprites[1].color = new Color(1f, 1f, 1f, .5f);

			dirt.isBusy = true;

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

			//need to assign to task queue
			MenuController.instance.OpenFireMenu();
        }

		return;
	}

	public void PlantSeed (Crop c, PlayerInteraction player, DirtTile dirt)
	{
		parentSprite = dirt.GetComponent<SpriteRenderer>();
		childSprites = dirt.GetComponentsInChildren<SpriteRenderer>();

		
		if (dirt.crop.asset != null && dirt.crop.state != CropState.Seed)
		{
			Debug.Log("Crop not seed, can't plan't.");
			MenuController.instance.notificationBar.SetActive(false);
			MenuController.instance.AnimateNotifcation("Can't Plant Seed", Color.red, "Error");
			return;
		}
		dirt.crop = c;
		dirt.crop.state = CropState.Planted;
		UpdateSprite(dirt);
		parentSprite.color = new Color(1f, 1f, 1f, 1f);
		childSprites[1].color = new Color(1f, 1f, 1f, 1f);

		StatsController.instance.AddExp(1);
		StatsController.instance.RemoveCoinsDisplay(c.asset.cropCost);
		FindObjectOfType<AudioManager>().PlaySound("Seed");
		dirt.isBusy = false;

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
			dirt.isBusy = false;
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
		dirt.isBusy = false;
		
		
	}

	public void UpdateSprite (DirtTile dirtPlot)
	{
		SpriteRenderer[] sprites = dirtPlot.GetComponentsInChildren<SpriteRenderer>();
			sprites[1].sprite = dirtPlot.crop.GetCropSprite(dirtPlot.crop);
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
					UpdateSprite(this);
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
		foreach (GameObject plot in TileSelector.instance.plots)
		{
			Object.Destroy(plot);
			
		}
		TileSelector.instance.plots.Clear();

	}

}
