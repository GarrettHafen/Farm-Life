using UnityEngine;

[CreateAssetMenu(fileName = "New Animal", menuName = "Animal")]
public class AnimalAsset : ScriptableObject
{
    public Sprite animalGrowingSprite;
    public Sprite animalDoneSprite;
    public Sprite animalIconSprite;
    public float animalTimer; //how long it takes the animal to grow
    public int animalCost; //how much it costs per animal
    public int animalReward; //how much money is returned per animal harvested
    public int expReward; //how much xp you get per animal harvested
    public string animalName; //animal name
    public int reqLvl; //the required lvl to be able to use this animal
    public string preview; //for when we have multiple different previews, long and skinny, 2x2, 4x4 etc
    public GameObject animalPrefab; // prefab for each animal type to maintain the collider
    //happiness modifier? to be used at time of harvest, but would need another interact which means more code


    /*
     * Adding new Animals
     * each animal type (cows, dogs, chickens) needs its own prefab, 
     * needs to be attached to a preview and a preview container
     * cases for all of these need to be set up in PlayerInteration
     */

}
