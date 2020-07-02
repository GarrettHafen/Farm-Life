using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
	public static PlayerInteraction instance;

	public GameObject target = null;

	public KeyCode interactKey;

	public IconBox iconBox;

	RaycastHit2D hit;

	[SerializeField]
	private Crop crop;
	[SerializeField]
	private Tool tool;

	private void Start()
    {
		instance = this;
		Debug.Log(target);
		
    }

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			
			if (!GameHandler.instance.overMenu && MapController.instance.overMap)
			{Debug.Log("not menu or map");
				if (target == null)
				{
					if (TileSelector.instance.plowActive)
					{
						TileSelector.instance.GetPlotPosition();
					}
					return;
				}
				DirtTile dirt = target.GetComponent<DirtTile>();
				Debug.Log(dirt);
				if (dirt != null)
				{
					dirt.Interact(crop, tool, this);
				}

				TableTile table = target.GetComponent<TableTile>();
				if (table != null)
				{
					table.Interact(crop, tool, this);
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
		DisplayInventory();
	}

	public void SetTool(Tool t)
	{
		tool = t;
		//don't need until implement IconBox
		//DisplayInventory();
	}

	void DisplayInventory ()
	{
		if (crop.HasCrop())
		{
			iconBox.SetIcon(crop.GetCropSprite());
		} else if (tool != null)
		{
			iconBox.SetIcon(tool.sprite);
		} else
		{
			iconBox.Close();
		}
	}
	/*
	private void OnTriggerEnter2D(Collider2D col)
	{
		if (target != col.gameObject && target != null)
		{
			Deselect();
		}

		target = col.gameObject;

		SeedBarrel barrel = target.GetComponent<SeedBarrel>();
		if (barrel != null)
		{
			barrel.Select();
		}

		SpriteRenderer[] srs = target.GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer sr in srs)
		{
			sr.color = new Color(1f, 1f, 1f, 0.9f);
		}
	}

	private void OnTriggerExit2D(Collider2D col)
	{
		if (col.gameObject == target)
		{
			Deselect();
			target = null;
		}
	}
	*/
	void Deselect()
	{
		target = null;

		/*

		SpriteRenderer[] srs = target.GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer sr in srs)
		{
			sr.color = Color.white;
		}*/
	}
	

}
