using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
	public static PlayerInteraction instance;

	public GameObject target = null;

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
	public Sprite harvestToolSprite, marketToolSprite, plowToolSprite, fireToolSprite;
	public float mouseyOffset;

	public Sprite redPreview4x4, greenPreview4x4, redPreview1x1, greenPreview1x1;


	//public GameObject plot;



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
			if (!GameHandler.instance.overMenu && MapController.instance.overMap /*&& tool != null*/)
			{
				if (target == null)
				{
					if (MenuController.instance.plowActive && !MenuController.instance.previewObstructed)
					{
						//old code
						//TileSelector.instance.GetPlotPosition();
						if (StatsController.instance.RemoveCoins(5))
						{
							TileSelector.instance.PlacePlot(MenuController.instance.GetMouseyThingyPosition());
							FindObjectOfType<AudioManager>().PlaySound("Plow");
							StatsController.instance.AddExp(1);
						}
						return;
					}
					else
					{
						Debug.Log("cant place plot");
						//error code
						//return;
					}
                    if(MenuController.instance.hasTree && !MenuController.instance.previewObstructed)
                    {
                        if (StatsController.instance.RemoveCoins(tree.GetCost()))
                        {
							TileSelector.instance.PlantTree(MenuController.instance.GetMouseyThingyPosition(), tree, this);
                        }
                    }
					return;
				}
				DirtTile dirt = target.GetComponent<DirtTile>();
				Debug.Log("dirt: " + dirt);
				if (dirt != null)
				{					
					dirt.Interact(crop, /*tool,*/ this, dirt);
				}
				//animal code
				//tree code
				TreeTile treeTile = target.GetComponent<TreeTile>();
				Debug.Log("tree" + treeTile);
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
					DisplayMouseyCompanion(harvestToolSprite);
				}
				if (MenuController.instance.fireTool)
				{
					//display destroy icon
					DisplayMouseyCompanion(fireToolSprite);
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
			if (hit.collider != null && hit.collider.tag != "Bound")
			{
				if (hit.collider.gameObject.Equals(target))
				{
                    //this code fixed a bug where when hovering over a plot, it was considered "not the map"
					MapController.instance.overMap = true;
				}
				else
				{
					if (target != null)
					{
                        //if hovered over a plot or animal or tree, need detection later
						timer = target.GetComponent<TimerController>();
						timer.slider.gameObject.SetActive(false);
                    }
					target = hit.collider.gameObject;
					mouseyCompanionImage.sprite = null;
					MapController.instance.overMap = true;
				}
			}
			else
			{
				/*------------------------------------------------------------
				 ------------------------------------------------------------
				------------------------------------------------------------
				bad code alert - kicks off every frame, debug deslect and its constantly going, is that bad?
				------------------------------------------------------------
				------------------------------------------------------------
				------------------------------------------------------------*/

				Deselect();
			}
		}else
            {
				Deselect();
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
		Vector3 m = Input.mousePosition;
		Vector3 p = Camera.main.ScreenToWorldPoint(m);
		mouseyCompanion.transform.position = new Vector3(p.x, p.y + mouseyOffset, 10);
		mouseyCompanionImage.sprite = sprite;
		mouseyCompanion.gameObject.SetActive(true);
	}

	

	void Deselect()
	{
		if (target != null)
		{
			Debug.Log("deselect code: s" + target);
			if (timer != null)
			{
				timer = target.GetComponent<TimerController>();
				timer.slider.gameObject.SetActive(false);
			}
			target = null;
            timer = null;
        }

	}
	

}
