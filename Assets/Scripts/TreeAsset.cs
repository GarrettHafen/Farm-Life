using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tree", menuName = "Tree")]
public class TreeAsset : ScriptableObject
{
	public Sprite treePlantedSprite;
    public Sprite treeGrowingSprite;
	public Sprite treeDoneSprite;
	public Sprite treeIconSprite;
	public float treeTimer; //how long it takes tree to grow
	public int treeCost; //how much it costs per tree
	public int treeReward; //how much money is returned per tree
	public int expReward; //how much xp you get per crop harvested
	public string treeName; //crop name
	public int reqLvl; //the required level to be able to use this crop.
}