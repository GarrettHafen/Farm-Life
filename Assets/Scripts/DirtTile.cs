using UnityEngine;

public class DirtTile : MonoBehaviour
{
	public static DirtTile instance;
	public Crop crop;

	public SpriteRenderer overlay;

	public bool needsPlowing = false;
	public bool isBusy = false;
	public Sprite fallowed;

	[SerializeField] private string plantSoundName   = "Seed";
	[SerializeField] private string harvestSoundName = "Harvest";
	[SerializeField] private string plowSoundName    = "Plow";
	[SerializeField] private string destroySoundName = "Destroy";

	SpriteRenderer parentSprite;
	SpriteRenderer[] childSprites;

	private void Start()
	{
		instance = this;
	}

	public void Interact (Crop c,PlayerInteraction player, DirtTile dirt)
	{
		if (MenuController.instance.toolState.hasSeed && c.HasCrop())
		{
			//queue PLANTING

			// Can't plant if the tile already has a crop in progress
			if (dirt.crop.HasCrop() && dirt.crop.state != CropState.Seed)
			{
				MenuController.instance.notificationBar.SetActive(false);
				MenuController.instance.AnimateNotifcation("Can't Plant Seed", Color.red, "Error");
				return;
			}

			if (!needsPlowing)
			{
				//check if enough coins to perform task
				if (StatsController.instance.CheckMaster(c.asset.cropCost))
                {
					StatsController.instance.RemoveCoins(c.asset.cropCost);
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
		if (needsPlowing && !MenuController.instance.toolState.fireTool)
		{
			// queue SECOND PLOW

			if (StatsController.instance.CheckMaster(5))
			{
				StatsController.instance.RemoveCoins(5);

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
        else if (!needsPlowing && !dirt.crop.HasCrop() && !MenuController.instance.toolState.fireTool)
        {
			
			MarketController.instance.ActivateMarket();
        }
		else if (dirt.crop.HasCrop() && dirt.crop.state == CropState.Done && !MenuController.instance.toolState.fireTool)
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
        else if (MenuController.instance.toolState.fireTool)
        {
            if (TileSelector.instance.plots.Count == 1)
            {
                MenuController.instance.notificationBar.SetActive(false);
                MenuController.instance.AnimateNotifcation("Can't destroy last plot!", Color.red, "Error");
                return;
            }
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
		AudioManager.instance.PlaySound(plantSoundName);
		dirt.isBusy = false;
		dirt.crop.SetGrowthLvl(0f);
		dirt.crop.StartGrowth(dirt);

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
			AudioManager.instance.PlaySound(harvestSoundName);
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
		StatsController.instance.AddExp(1);
		AudioManager.instance.PlaySound(plowSoundName);
		dirt.isBusy = false;
		
		
	}

	public void UpdateSprite (DirtTile dirtPlot)
	{
		SpriteRenderer[] sprites = dirtPlot.GetComponentsInChildren<SpriteRenderer>();
			sprites[1].sprite = dirtPlot.crop.GetCropSprite(dirtPlot.crop);
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
		AudioManager.instance.PlaySound(destroySoundName);
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
