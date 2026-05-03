using UnityEngine;

public class AnimalTile : MonoBehaviour
{
    public static AnimalTile instance;
    public Animal animal;
    public bool isBusy = false;
    public Vector3 snapPosition;
    public int previewCells = 1;

    public SpriteRenderer overlay;

    [SerializeField] private string harvestSoundName = "Harvest";
    [SerializeField] private string destroySoundName = "Destroy";

    //used to control opacity for task system
    SpriteRenderer parentSprite;
    SpriteRenderer[] childSprites;


    void Start()
    {
        instance = this;
    }

    void Update()
    {
        float randomNumber = Mathf.Round(Random.Range(0.0f, 500.0f));
        if(randomNumber == 18)
        {
            this.gameObject.GetComponent<SpriteRenderer>().flipX = !this.gameObject.GetComponent<SpriteRenderer>().flipX;
        }
    }

    public void Interact(Animal a, AnimalTile animalTile, PlayerInteraction player)
    {
        if(animalTile.animal.HasAnimal() && animalTile.animal.animalState == AnimalState.Done && !MenuController.instance.toolState.fireTool)
        {
            //queue Harvest Animal
            parentSprite = animalTile.GetComponent<SpriteRenderer>();
            childSprites = animalTile.GetComponentsInChildren<SpriteRenderer>();

            parentSprite.color = new Color(1f, 1f, 1f, .5f);
            childSprites[1].color = new Color(1f, 1f, 1f, .5f);

            animalTile.isBusy = true;

            //queue task
            QueueTaskSystem.instance.SetTask("harvestAnimal", animalTile);

            //after task is complete, see HarvestAnimal
        }
        if (MenuController.instance.toolState.fireTool)
        {
            MenuController.instance.OpenFireMenu();
        }
    }

    public void HarvestAnimal(AnimalTile animalThingy)
    {
        parentSprite = animalThingy.GetComponent<SpriteRenderer>();
        childSprites = animalThingy.GetComponentsInChildren<SpriteRenderer>();

        parentSprite.color = new Color(1f, 1f, 1f, 1f);
        childSprites[1].color = new Color(1f, 1f, 1f, 1f);

        StatsController.instance.AddCoins(animalThingy.animal.asset.animalCost);
        StatsController.instance.AddExp(animalThingy.animal.asset.expReward);
        animalThingy.animal.animalState = AnimalState.Growing;
        UpdateAnimalSprite(animalThingy);
        animalThingy.animal.SetGrowthLvl(0f);
        animalThingy.animal.StartGrowth(animalThingy);
        AudioManager.instance.PlaySound(harvestSoundName);
        animalThingy.isBusy = false;
    }

    public void DestroyAnimal(AnimalTile animalToDestroy)
    {
        TileSelector.instance.UnregisterFootprint(animalToDestroy.snapPosition, animalToDestroy.previewCells);
        foreach(GameObject animal in TileSelector.instance.animals)
        {
            if (animal.Equals(animalToDestroy.gameObject))
            {
                TileSelector.instance.animals.Remove(animal);
                break;
            }
        }
        Object.Destroy(animalToDestroy.gameObject);
        AudioManager.instance.PlaySound(destroySoundName);
    }

    public void DestroyAnimals()
    {
        int temp = TileSelector.instance.animals.Count;
        foreach(GameObject animal in TileSelector.instance.animals)
        {
            Object.Destroy(animal);
        }
        TileSelector.instance.animals.Clear();
    }

    public void UpdateAnimalSprite(AnimalTile animalThingy)
    {
        animalThingy.overlay.sprite = animal.GetAnimalSprite(animalThingy.animal);
    }
}
