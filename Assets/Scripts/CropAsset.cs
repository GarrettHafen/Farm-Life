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
	public float cropTimer;
	public string test = "testCode";

	public bool seedIsOnGround = false;

}
