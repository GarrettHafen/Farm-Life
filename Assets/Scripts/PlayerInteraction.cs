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

    public GameObject mouseyCompanion;
	public SpriteRenderer mouseyCompanionImage;
	public Sprite harvestToolSprite, marketToolSprite, plowToolSprite, fireToolSprite;
	public float mouseyOffset;

	public Sprite redPreview;
	public Sprite greenPreview;

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
						return;
					}
				}
				DirtTile dirt = target.GetComponent<DirtTile>();
				if (dirt != null)
				{					
					dirt.Interact(crop, /*tool,*/ this, dirt);
				}
                //animal code
                //tree code
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
            if (!MenuController.instance.GetMouseyThingy())
            {
				MenuController.instance.ActivatePreview();
            }
            if (MenuController.instance.previewObstructed)
            {
				MenuController.instance.SetPreviewColor(redPreview);
            }
            else
            {
				MenuController.instance.SetPreviewColor(greenPreview);
            }
		}

		//display timer if hovered over target
		if (target != null && !MenuController.instance.previewActive)
		{
			DirtTile temp = target.GetComponent<DirtTile>();
			if (temp)//code to prevent random errors when game starts
			{
				if (temp.crop.HasCrop())
				{
					timer = temp.GetComponent<TimerController>();
					timer.slider.gameObject.SetActive(true);
					timer.SetTime(temp.crop.GetGrowthLvl());
					timer.SetSprite(temp.crop.asset.iconSprite);

				}
                if(temp.crop.state == CropState.Done)
                {
					//display harvest tool
					Vector3 m = Input.mousePosition;
					Vector3 p = Camera.main.ScreenToWorldPoint(m);
					mouseyCompanion.transform.position = new Vector3(p.x, p.y + mouseyOffset, 10);
					mouseyCompanionImage.sprite = harvestToolSprite;
					mouseyCompanion.gameObject.SetActive(true);
				}
                if (temp.needsPlowing)
                {
					//display plow tool
					Vector3 m = Input.mousePosition;
					Vector3 p = Camera.main.ScreenToWorldPoint(m);
					mouseyCompanion.transform.position = new Vector3(p.x, p.y + mouseyOffset, 10);
					mouseyCompanionImage.sprite = plowToolSprite;
					mouseyCompanion.gameObject.SetActive(true);
				}
                if (!temp.needsPlowing && !MenuController.instance.hasSeed && temp.crop.state == CropState.Seed && !MenuController.instance.fireTool)
                {
					//display market tool
					Vector3 m = Input.mousePosition;
					Vector3 p = Camera.main.ScreenToWorldPoint(m);
					mouseyCompanion.transform.position = new Vector3(p.x, p.y + mouseyOffset, 10);
					mouseyCompanionImage.sprite = marketToolSprite;
					mouseyCompanion.gameObject.SetActive(true);
				}
                if (MenuController.instance.hasSeed)
                {
                    //display seed to be planted
					Vector3 m = Input.mousePosition;
					Vector3 p = Camera.main.ScreenToWorldPoint(m);
					mouseyCompanion.transform.position = new Vector3(p.x, p.y + mouseyOffset, 10);
					mouseyCompanionImage.sprite = crop.asset.iconSprite;
					mouseyCompanion.gameObject.SetActive(true);
				}
                if (MenuController.instance.fireTool)
                {
                    //display destroy icon
					Vector3 m = Input.mousePosition;
					Vector3 p = Camera.main.ScreenToWorldPoint(m);
					mouseyCompanion.transform.position = new Vector3(p.x, p.y + mouseyOffset, 10);
					mouseyCompanion.gameObject.SetActive(true);
					mouseyCompanionImage.sprite = fireToolSprite;
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

	/*
    public void SetTool(Tool t)
	{
		tool = t;
		DisplayInventory();
		SetCrop(new Crop(null));
		
	}
	public Tool GetTool()
    {
		return tool;
    }
    */
	public Crop GetCrop()
    {
		return crop;
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
        }

	}
	

}
