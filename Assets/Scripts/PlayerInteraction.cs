using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
	public static PlayerInteraction instance;

	public GameObject target = null;
	private GameObject previousTarget;
	public GameObject tempTarget;

	public TimerController timer;

	RaycastHit2D hit;

	[SerializeField]
	private Crop crop;
	[SerializeField]
	private Tool tool;
	[SerializeField]
	private Tree tree;
	[SerializeField]
	private Animal animal;


	public GameObject mouseyCompanion;
	public SpriteRenderer mouseyCompanionImage;
	public Sprite harvestToolSprite, marketToolSprite, plowToolSprite, fireToolSprite, basketToolSprite;
	public float mouseyOffset;

	public Sprite redPreview4x4, greenPreview4x4, redPreview1x1, greenPreview1x1;


	public Vector3 plotOffset;
	public Vector3 treeOffset;
	public Vector3 animalOffset;

	//sprites to manage opactiy during task execution
	SpriteRenderer parentPlotSprite;
	SpriteRenderer parentTreeSprite;
	SpriteRenderer parentAnimalSprite;

	private bool playAnimalNoise = true;


	private void Start()
    {
		instance = this;

        //not sure if there is a better way to get this. should probably attach a script to mouseyCompanion
		mouseyCompanion.gameObject.SetActive(true);
		mouseyCompanionImage = mouseyCompanion.GetComponent<SpriteRenderer>();
		mouseyCompanion.gameObject.SetActive(false);


	}

	private void Update()
	{

        //when clicking on something, do something based on what is clicked on
		if (Input.GetMouseButtonDown(0))
		{
			if (!GameHandler.instance.overMenu && MapController.instance.overMap)
			{
				if (target == null)
				{
					if (MenuController.instance.plowActive && !MenuController.instance.previewObstructed)
					{
						// queue FIRST PLOW

						//check if enough coins to perform task
						if (StatsController.instance.CheckMaster(5))
						{
							// if enough coins, decrement master
							StatsController.instance.RemoveCoinsMaster(5);

							//instantiate object, set opacity to half
							DirtTile tempPlot = TileSelector.instance.PlacePlot(MenuController.instance.GetMouseyThingyPosition(), plotOffset);
							parentPlotSprite = tempPlot.GetComponent<SpriteRenderer>();
							parentPlotSprite.color = new Color(1f, 1f, 1f, .5f);

							//queue task
							//don't need to include anything else
							tempPlot.isBusy = true;
							QueueTaskSystem.instance.SetTask("firstPlow", tempPlot);
                            //Debug.Log(QueueTaskSystem.instance.GetQueueCount());

                            //after timer finishs, change opacity to full and play sound and update display
                            // see FinishFirstPlot()


                        }
                        else
                        {
							//error notification and sound
							MenuController.instance.notificationBar.SetActive(false);
							MenuController.instance.AnimateNotifcation("Insufficient Funds", Color.red, "No Money");

						}
						return;
					}
					else
					{
						//Debug.Log("cant place plot"); is triggering any time you click on the map
						//error code
						//return;
					}
                    if(MenuController.instance.hasTree && !MenuController.instance.previewObstructed)
                    {

						//queue PLANT TREE

						//check if enough coins to perform task
                        if (StatsController.instance.CheckMaster(tree.GetCost()))
                        {
							// if enough coins, decrement master
							StatsController.instance.RemoveCoinsMaster(tree.GetCost());

							//instantiate object, set opacity to half
							TreeTile tempTree = TileSelector.instance.PlantTree(MenuController.instance.GetMouseyThingyPosition(), tree, this, treeOffset);
							parentTreeSprite = tempTree.GetComponent<SpriteRenderer>();
							parentTreeSprite.color = new Color(1f, 1f, 1f, .5f);

							// queue task
							tempTree.isBusy = true;
							QueueTaskSystem.instance.SetTask("plantTree", tempTree);

							//after task finishes, change opacity to full, update display and play sound, see FinishPlanttree()
							

                        }
                        else
                        {
							//error notification and sound
							MenuController.instance.notificationBar.SetActive(false);
							MenuController.instance.AnimateNotifcation("Insufficient Funds", Color.red, "No Money");
						}
                    }
					if (MenuController.instance.hasAnimal && !MenuController.instance.previewObstructed)
                    {
						//queue PLACE ANIMAL

						// check if enough coins to perform task
						if (StatsController.instance.CheckMaster(animal.GetCost()))
                        {
							//if enough coins, decrement master
							StatsController.instance.RemoveCoinsMaster(animal.GetCost());

							//instantiate object, set opacity to half
							AnimalTile tempAnimal = TileSelector.instance.PlaceAnimal(MenuController.instance.GetMouseyThingyPosition(), animal, this, animalOffset);
							parentAnimalSprite = tempAnimal.GetComponent<SpriteRenderer>();
							parentAnimalSprite.color = new Color(1f, 1f, 1f, .5f);

							tempAnimal.isBusy = true;
							//queue task
							QueueTaskSystem.instance.SetTask("placeAnimal", tempAnimal);

							//after task finishes, change opacity to full, update display and play sound, see FinishPlaceAnimal()
                        }
                        else
                        {
							//error notification and sound
							MenuController.instance.notificationBar.SetActive(false);
							MenuController.instance.AnimateNotifcation("Insufficient Funds", Color.red, "No Money");
						}
                    }
					return;
				}
				DirtTile dirt = target.GetComponent<DirtTile>();
				if (dirt != null && !dirt.isBusy)
				{					
					dirt.Interact(crop, this, dirt);
				}

				//animal code
				AnimalTile animalTile = target.GetComponent<AnimalTile>();
				if(animalTile != null && !animalTile.isBusy)
                {
					animalTile.Interact(animal, animalTile, this);
                }


				//tree code
				TreeTile treeTile = target.GetComponent<TreeTile>();
				//Debug.Log("tree" + treeTile);
                if(treeTile != null && !treeTile.isBusy)
                {
					treeTile.Interact(tree, treeTile, this);
                }
                //decor code
			}
		}else if (Input.GetMouseButton(1))
        {
			MenuController.instance.ClearHand();
			MenuController.instance.DestroyPreview();
        }

		/*
		 * 
		 * can this all be replaced by a single function that pulls the right preview from the new list i just made?
		 * 
		 * 
		 */
        //if plow tool active, display 4x4 preview
        if (MenuController.instance.plowActive)
		{
			GameObject preview = MenuController.instance.preview4x4;
            if (!MenuController.instance.GetMouseyThingy())
            {
				MenuController.instance.ActivatePreview(preview);
            }
            if (MenuController.instance.previewObstructed)
            {
				MenuController.instance.SetPreviewColor(GameHandler.instance.previewList[8].previewRedSprite, preview);
            }
            else
            {
				MenuController.instance.SetPreviewColor(GameHandler.instance.previewList[8].previewGreenSprite, preview);
            }
		}
        //display tree and preview
        if(MarketController.instance.marketState == MarketState.Tree && MenuController.instance.hasTree)
        {
			GameObject preview = MenuController.instance.preview1x1;
			if (!MenuController.instance.GetMouseyThingy())
			{
				MenuController.instance.ActivatePreview(preview);
				MenuController.instance.previewObstructed = false;
			}
			if (MenuController.instance.previewObstructed)
			{
				MenuController.instance.SetPreviewColor(GameHandler.instance.previewList[0].previewRedSprite, preview);
			}
			else
			{
				MenuController.instance.SetPreviewColor(GameHandler.instance.previewList[0].previewGreenSprite, preview);
			}
		}

		//display animal and preview
		if (MarketController.instance.marketState == MarketState.Animal && MenuController.instance.hasAnimal)
		{
			GameObject preview = GetPreviewContainer(animal.asset.preview);
			int previewPosition = GetPreviewPosition(animal.asset.preview);
			if (!MenuController.instance.GetMouseyThingy())
			{
				MenuController.instance.ActivatePreview(preview);
				MenuController.instance.previewObstructed = false;
			}
			if (MenuController.instance.previewObstructed)
			{
				MenuController.instance.SetPreviewColor(GameHandler.instance.previewList[previewPosition].previewRedSprite, preview);
			}
			else
			{
				MenuController.instance.SetPreviewColor(GameHandler.instance.previewList[previewPosition].previewGreenSprite, preview);
			}
		}
		//display decoration and preview
		//remove to comment out entire section*/

		//display timer if hovered over target
		if (target != null && !MenuController.instance.previewActive)
		{
			
			DirtTile tempDirt = target.GetComponent<DirtTile>();
			if (tempDirt)//code to prevent random errors when game starts
			{
				if (tempDirt.crop.HasCrop())
				{
					timer = tempDirt.GetComponent<TimerController>();
					timer.slider.gameObject.SetActive(true);
					timer.SetTime(tempDirt.crop.GetGrowthLvl());
					timer.SetSprite(tempDirt.crop.asset.iconSprite);

				}
				if (!tempDirt.isBusy)
				{
					if (tempDirt.crop.state == CropState.Done)
					{
						//display harvest tool
						DisplayMouseyCompanion(harvestToolSprite);
					}
					if (tempDirt.needsPlowing)
					{
						//display plow tool
						DisplayMouseyCompanion(plowToolSprite);


					}
					if (!tempDirt.needsPlowing && !MenuController.instance.hasSeed && tempDirt.crop.state == CropState.Seed && !MenuController.instance.fireTool)
					{
						//display market tool
						DisplayMouseyCompanion(marketToolSprite);
					}
					if (MenuController.instance.hasSeed)
					{
						//display seed to be planted
						DisplayMouseyCompanion(crop.asset.iconSprite);
					}
					if (MenuController.instance.fireTool)
					{
						//display destroy icon
						DisplayMouseyCompanion(fireToolSprite);
					}
				}
			}
			
			TreeTile tempTree = target.GetComponent<TreeTile>();
			if (tempTree)//code to prevent random errors when game starts
			{
				if (tempTree.tree.HasTree())
				{
					timer = tempTree.GetComponent<TimerController>();
					timer.slider.gameObject.SetActive(true);
					timer.SetTime(tempTree.tree.GetGrowthLvl());
					timer.SetSprite(tempTree.tree.asset.treeIconSprite);

				}
                if (tempTree.tree.treeState == TreeState.Done)
				{
					//display harvest tool
					DisplayMouseyCompanion(basketToolSprite);
				}
                if (MenuController.instance.fireTool)
				{
					//display destroy icon
					DisplayMouseyCompanion(fireToolSprite);
                }
                if(tempTree.tree.treeState == TreeState.Growing)
                {
					DisplayMouseyCompanion(null);
                }
			}

			AnimalTile tempAnimal = target.GetComponent<AnimalTile>();
            if (tempAnimal)
            {
                if (tempAnimal.animal.HasAnimal())
                {
					timer = tempAnimal.GetComponent<TimerController>();
					timer.slider.gameObject.SetActive(true);
					timer.SetTime(tempAnimal.animal.GetGrowthLvl());
					timer.SetSprite(tempAnimal.animal.asset.animalIconSprite);
                }
				if(tempAnimal.animal.animalState == AnimalState.Done)
                {
					//display harvest tool
					DisplayMouseyCompanion(basketToolSprite);
                }
				if (MenuController.instance.fireTool)
				{
					//display destroy icon
					DisplayMouseyCompanion(fireToolSprite);
				}
				if (tempAnimal.animal.animalState == AnimalState.Growing)
				{
					DisplayMouseyCompanion(null);
				}
			}
		}
        else
        {
			//default cursor. maybe
			mouseyCompanionImage.sprite = null;
			mouseyCompanion.gameObject.SetActive(false);
		}
		// define target by what is under mouse
		if (!GameHandler.instance.overMenu && !MenuController.instance.previewActive)
		{
			previousTarget = target;
			Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Vector2 left = new Vector2(mousePosition.x - .01f, mousePosition.y);
			hit = Physics2D.Raycast(mousePosition, left, 0.1f, LayerMask.NameToLayer("Plots"));
			if (hit.collider != null && !hit.collider.CompareTag("Bound"))
			{
				if (hit.collider.gameObject == target || hit.collider.gameObject.transform.parent.gameObject == target)
				{
					//this code fixed a bug where when hovering over a plot, it was considered "not the map"
					MapController.instance.overMap = true;
				}
				else
                {
					Deselect();
					if (hit.collider.name.Equals("OverlaySprite")) 
					{
						target = hit.collider.gameObject.transform.parent.gameObject;
					}
					else
					{
						target = hit.collider.gameObject;
					}
					mouseyCompanionImage.sprite = null;
					MapController.instance.overMap = true;
				}
            }
            else
            {
				if(target != null)
                {
					Deselect();
                }
            }
		}


        
	}

	public void SetCrop(Crop c)
	{
		crop = c;
		MenuController.instance.DisplayInventory();
	}
    public void SetTree(Tree t)
    {
		tree = t;
		MenuController.instance.DisplayInventory();
    }
	public void SetAnimal(Animal a)
    {
		animal = a;
		MenuController.instance.DisplayInventory();
    }
	public Crop GetCrop()
    {
		return crop;
    }

    public Tree GetTree()
    {
		return tree;
    }

	public Animal GetAnimal()
    {
		return animal;
    }
    private void DisplayMouseyCompanion(Sprite sprite)
    {
        if(sprite == null)
        {
			mouseyCompanion.gameObject.SetActive(false);
        }

        Vector3 m = Input.mousePosition;
		Vector3 p = Camera.main.ScreenToWorldPoint(m);
		mouseyCompanion.transform.position = new Vector3(p.x, p.y + mouseyOffset, 10);
		mouseyCompanionImage.sprite = sprite;
		mouseyCompanion.gameObject.SetActive(true);
	}

	public void Deselect()
	{
		if (previousTarget != null)
		{
			//Debug.Log("deselect code: " + target.name);
			if (timer != null)
			{
				timer = previousTarget.GetComponent<TimerController>();
				timer.slider.gameObject.SetActive(false);
			}
			previousTarget = null;
			target = null;
            timer = null;

		}
	}

	public void DestroyStuff()
    {
		DirtTile dirt = tempTarget.GetComponent<DirtTile>();
		if(dirt != null)
		{ 
			QueueTaskSystem.instance.SetTask("burn", dirt);
		}
		TreeTile treeTile = tempTarget.GetComponent<TreeTile>();
		if(treeTile != null)
        {
			QueueTaskSystem.instance.SetTask("burn", treeTile);
		}
		AnimalTile animalTile = tempTarget.GetComponent<AnimalTile>();
		if(animalTile != null)
        {
			
			QueueTaskSystem.instance.SetTask("burn", animalTile);
		}
    }

	public void SetTempTarget()
    {
		tempTarget = target;
    }
	public void ClearTemptTarget()
    {
		tempTarget = null;
    }

	public void FinishFirstPlow(DirtTile dirt)
    {
		parentPlotSprite = dirt.GetComponent<SpriteRenderer>();
		parentPlotSprite.color = new Color(1f, 1f, 1f, 1f);
		StatsController.instance.RemoveCoinsDisplay(5);
		FindObjectOfType<AudioManager>().PlaySound("Plow");
		StatsController.instance.AddExp(1);
		dirt.isBusy = false;
	}

	public void FinishPlantTree(TreeTile tree)
    {
		parentTreeSprite = tree.GetComponent<SpriteRenderer>();
		parentTreeSprite.color = new Color(1f, 1f, 1f, 1f);
		StatsController.instance.RemoveCoinsDisplay(tree.tree.GetCost());
		FindObjectOfType<AudioManager>().PlaySound("Plow");
		StatsController.instance.AddExp(1);
		tree.isBusy = false;
	}

	public void FinishPlaceAnimal(AnimalTile animal)
    {
		parentAnimalSprite = animal.GetComponent<SpriteRenderer>();
		parentAnimalSprite.color = new Color(1f, 1f, 1f, 1f);
		StatsController.instance.RemoveCoinsDisplay(animal.animal.GetCost());
        if (playAnimalNoise)
        {
			FindObjectOfType<AudioManager>().PlaySound(animal.animal.asset.animalSound);
			
        }
		playAnimalNoise = !playAnimalNoise;

		StatsController.instance.AddExp(1);

		animal.isBusy = false;
	}

	public GameObject GetPreviewContainer(string intendedPreview)
    {
		switch (intendedPreview)
		{
			case "1x1":
				{
					return GameHandler.instance.previewContainerList[0];
				}
			case "4x4":
                {
					return GameHandler.instance.previewContainerList[1];
                }
			case "2x2":
                {
					return GameHandler.instance.previewContainerList[6];
                }
        }

		return null;
    }
	public int GetPreviewPosition(string intendedPreview)
	{
		switch (intendedPreview)
		{
			case "1x1":
				{
					return 0;
				}
			case "4x4":
				{
					return 8;	
				}
			case "2x2":
                {
					return 5;
                }
		}
		Debug.Log("error in preview position");
		return 0;
	}
}
