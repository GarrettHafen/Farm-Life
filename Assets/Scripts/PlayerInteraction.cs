using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

public class PlayerInteraction : MonoBehaviour
{
	public static PlayerInteraction instance;

	public GameObject target = null;

	public KeyCode interactKey;

	public IconBox iconBox;
	public SpriteRenderer handIndicator;

	RaycastHit2D hit;

	[SerializeField]
	private Crop crop;
	[SerializeField]
	private Tool tool;

	private void Start()
    {
		instance = this;
		
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
		// define target by what is under mouse
		Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector2 left = new Vector2(mousePosition.x - .01f, mousePosition.y);
		hit = Physics2D.Raycast(mousePosition, left, Mathf.Infinity, LayerMask.NameToLayer("Plots"));
		if (hit.collider != null)
		{
			if (hit.collider.gameObject.Equals(target))
			{
				MapController.instance.overMap = true;
			}
			else {
				target = hit.collider.gameObject;
				MapController.instance.overMap = true;
			}
        }
        else
        {
			Deselect();
		}
	}

	public void SetCrop(Crop c)
	{
		crop = c;
		//DisplayInventory();
	}

	public void SetTool(Tool t)
	{
		tool = t;
		//DisplayInventory();
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
		if(tool.toolType == ToolType.Plow || tool.toolType == ToolType.Harvest)
        {
			handIndicator.sprite = tool.sprite;
        }else if(tool.toolType == ToolType.Market)
        {
			handIndicator.sprite = crop.asset.iconSprite;

		}
	}
	void Deselect()
	{
		target = null;

	}
	

}
