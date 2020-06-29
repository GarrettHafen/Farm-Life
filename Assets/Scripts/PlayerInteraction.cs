using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
	public static PlayerInteraction instance;

	public GameObject target;

	public KeyCode interactKey;

	public IconBox iconBox;

	[SerializeField]
	private Crop crop;
	[SerializeField]
	private Tool tool;

	Ray ray;
	RaycastHit hit;

	private void Start()
    {
		instance = this;
    }

    private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			if(!GameHandler.instance.overMenu && MapController.instance.overMap)
			{
				if (target == null)
				{
                    if (TileSelector.instance.plowActive)
                    {
						TileSelector.instance.GetPlotPosition();
                    }
					return;
				}
				DirtTile dirt = target.GetComponent<DirtTile>();
				if (dirt != null)
				{
					Debug.Log("inside if dirt != null");
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
		/*ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		
		if (Physics.Raycast(ray, out hit, maxDistance, layerMask, QueryTriggerInteraction.Collide))
		{
			target = hit.transform.gameObject;
			Debug.Log(target);
		}*/
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

	void Deselect()
	{
		SeedBarrel barrel = target.GetComponent<SeedBarrel>();
		if (barrel != null)
		{
			barrel.DeSelect();
		}

		SpriteRenderer[] srs = target.GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer sr in srs)
		{
			sr.color = Color.white;
		}
	}
	private void OnMouseEnter()
	{
		target = gameObject;
	}
	private void OnMouseExit()
	{
		target = null;
	}

}
