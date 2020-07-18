using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
	public static PlayerInteraction instance;

	public GameObject target = null;

	public KeyCode interactKey;

	public IconBox iconBox;
	public Image handIndicator;
	public GameObject handIndicatorParent;

	public TimerController timer;

	RaycastHit2D hit;

	[SerializeField]
	private Crop crop;
	[SerializeField]
	private Tool tool;

	private void Start()
    {
		instance = this;
		handIndicatorParent.SetActive(false);
		
    }

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			
			if (!GameHandler.instance.overMenu && MapController.instance.overMap && tool != null)
			{
				if (target == null)
				{
					if (tool.toolType == ToolType.Plow)
					{
						TileSelector.instance.GetPlotPosition();
						return;
					}
					return;
				}
				DirtTile dirt = target.GetComponent<DirtTile>();
				if (dirt != null)
				{					
					dirt.Interact(crop, tool, this, dirt);
				}

				SeedBarrel barrel = target.GetComponent<SeedBarrel>();
				if (barrel != null)
				{
					barrel.Interact(crop, tool, this);
				}
			}
		}
		if (target != null)
		{
			DirtTile temp = target.GetComponent<DirtTile>();
			if (temp.crop.HasCrop())
			{
				timer = temp.GetComponent<TimerController>();
				timer.slider.gameObject.SetActive(true);
				timer.SetTime(temp.crop.GetGrowthLvl());
				timer.SetSprite(temp.crop.asset.iconSprite);

			}
		}
		// define target by what is under mouse
		Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector2 left = new Vector2(mousePosition.x - .01f, mousePosition.y);
		hit = Physics2D.Raycast(mousePosition, left, 0.1f, LayerMask.NameToLayer("Plots"));
		if (hit.collider != null)
		{
			if (hit.collider.gameObject.Equals(target))
			{
				MapController.instance.overMap = true;
			}
			else {
				if (target != null)
				{
					timer = target.GetComponent<TimerController>();
					timer.slider.gameObject.SetActive(false);
				}
				target = hit.collider.gameObject;
				MapController.instance.overMap = true;
			}
        }
        else
        {
			/*------------------------------------------------------------
			 ------------------------------------------------------------
			------------------------------------------------------------
			bade code alert - kicks off every frame, debug deslect and its constantly going, is that bad?
			------------------------------------------------------------
			------------------------------------------------------------
			------------------------------------------------------------*/

			Deselect();
		}
	}

	public void SetCrop(Crop c)
	{
		crop = c;
		DisplayInventory();
	}

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
	public Crop GetCrop()
    {
		return crop;
    }

	void DisplayInventory ()
	{
		handIndicatorParent.SetActive(true);
		if(tool == null)
        {
			handIndicatorParent.SetActive(false);
			return;
        }
		if(tool.toolType == ToolType.Plow || tool.toolType == ToolType.Harvest)
        {
			handIndicator.sprite = tool.sprite;
        }else if(crop.HasCrop())
        {
			handIndicator.sprite = crop.asset.iconSprite;
        }
	}

	void ClearHand()
    {
		handIndicatorParent.SetActive(false);
    }

	void Deselect()
	{
		if (target != null)
		{
			timer = target.GetComponent<TimerController>();
			timer.slider.gameObject.SetActive(false);
			target = null;
        }

	}
	

}
