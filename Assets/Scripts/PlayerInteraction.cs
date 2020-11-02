using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
	public static PlayerInteraction instance;

	public GameObject target = null;
	public GameObject tempTarget;

	public TimerController timer;

	RaycastHit2D hit;

	[SerializeField]
	private Crop crop;
	[SerializeField]
	private Tool tool;
	[SerializeField]
	private Tree tree;


	public GameObject mouseyCompanion;
	public SpriteRenderer mouseyCompanionImage;
	public Sprite harvestToolSprite, marketToolSprite, plowToolSprite, fireToolSprite, basketToolSprite;
	public float mouseyOffset;

	public Sprite redPreview4x4, greenPreview4x4, redPreview1x1, greenPreview1x1;


	public Vector3 plotOffset;
	public Vector3 treeOffset;

	//sprites to manage opactiy during task execution
	SpriteRenderer parentPlotSprite;
	SpriteRenderer parentTreeSprite;


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
							// nothing else needs passed

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
					return;
				}
				DirtTile dirt = target.GetComponent<DirtTile>();
				if (dirt != null)
				{					
					dirt.Interact(crop, this, dirt);
				}

				//animal code


				//tree code
				TreeTile treeTile = target.GetComponent<TreeTile>();
				//Debug.Log("tree" + treeTile);
                if(treeTile != null)
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
				MenuController.instance.SetPreviewColor(redPreview4x4, preview);
            }
            else
            {
				MenuController.instance.SetPreviewColor(greenPreview4x4, preview);
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
				MenuController.instance.SetPreviewColor(redPreview1x1, preview);
			}
			else
			{
				MenuController.instance.SetPreviewColor(greenPreview1x1, preview);
			}
		}

        //display animal and preview

        //display decoration and preview

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
                if(tempDirt.crop.state == CropState.Done)
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
			Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Vector2 left = new Vector2(mousePosition.x - .01f, mousePosition.y);
			hit = Physics2D.Raycast(mousePosition, left, 0.1f, LayerMask.NameToLayer("Plots"));
			if (hit.collider != null && !hit.collider.CompareTag("Bound"))
			{
				if (hit.collider.gameObject == target || hit.collider.name == "OverlaySprite")
				{
					//this code fixed a bug where when hovering over a plot, it was considered "not the map"
					MapController.instance.overMap = true;
				}
				else
                {
					Deselect();
					if (hit.collider.name == "OverlaySprite")
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
	public Crop GetCrop()
    {
		return crop;
    }

    public Tree GetTree()
    {
		return tree;
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
		if (target != null)
		{
			//Debug.Log("deselect code: " + target.name);
			if (timer != null)
			{
				timer = target.GetComponent<TimerController>();
				timer.slider.gameObject.SetActive(false);
			}
			target = null;
            timer = null;
			
        }
	}

	public void DestroyStuff()
    {
		DirtTile dirt = tempTarget.GetComponent<DirtTile>();
		if(dirt != null)
        {
			DirtTile.instance.DestroyPlot(dirt);
        }
		TreeTile treeTile = tempTarget.GetComponent<TreeTile>();
		if(treeTile != null)
        {
			TreeTile.instance.DestroyTree(treeTile);
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
	}

	public void FinishPlantTree(TreeTile tree)
    {
		parentTreeSprite = tree.GetComponent<SpriteRenderer>();
		parentTreeSprite.color = new Color(1f, 1f, 1f, 1f);
		StatsController.instance.RemoveCoinsDisplay(tree.tree.GetCost());
		FindObjectOfType<AudioManager>().PlaySound("Plow");
		StatsController.instance.AddExp(1);
	}
}
