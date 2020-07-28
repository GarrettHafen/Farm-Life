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
	public float cropTimer; //how long it takes crop to grow
	public int cropCost; //how much it costs per seed
	public int cropReward; //how much money is returned per crop
	public int expReward; //how much xp you get per crop harvested
	public string cropName; //crop name
	public int reqLvl; //the required level to be able to use this crop.


	public bool seedIsOnGround = false;

}
