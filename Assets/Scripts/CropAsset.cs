using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop", menuName = "Crop")]
public class CropAsset : ScriptableObject
{
	public Sprite seedSprite;
	public Sprite sproutSprite;
	public Sprite deadSprite;
	public Sprite doneSprite;
	public Sprite iconSprite;
	public float cropTimer;
	public int cropCost;
	public int cropReward;
	public int expReward;
	public string cropName;

	public bool seedIsOnGround = false;

}
