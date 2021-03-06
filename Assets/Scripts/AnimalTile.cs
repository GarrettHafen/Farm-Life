﻿using UnityEngine;

public class AnimalTile : MonoBehaviour
{
    public static AnimalTile instance;
    public Animal animal;
    public bool isBusy = false;

    public SpriteRenderer overlay;

    //used to control opacity for task system
    SpriteRenderer parentSprite;
    SpriteRenderer[] childSprites;


    void Start()
    {
        instance = this;
    }

    void Update()
    {
        if (animal.HasAnimal())
        {
            if(animal.animalState == AnimalState.Growing)
            {
                bool animalIsDone = animal.AnimalGrow(Time.deltaTime, this);
                if (animalIsDone)
                {
                    UpdateAnimalSprite(this);
                }
            }
        }

        //code to randomly flip the sprite, happens pretty quick, 500 might not be high enough. 18 is an arbitrary number, just like that number. 
        float randomNumber = Mathf.Round(Random.Range(0.0f, 500.0f));
        if(randomNumber == 18)
        {
            this.gameObject.GetComponent<SpriteRenderer>().flipX = !this.gameObject.GetComponent<SpriteRenderer>().flipX;
        }
    }

    public void Interact(Animal a, AnimalTile animalTile, PlayerInteraction player)
    {
        if(animalTile.animal.HasAnimal() && animalTile.animal.animalState == AnimalState.Done && !MenuController.instance.fireTool)
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
        if (MenuController.instance.fireTool)
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
        FindObjectOfType<AudioManager>().PlaySound("Harvest");
        animalThingy.isBusy = false;
    }

    public void DestroyAnimal(AnimalTile animalToDestroy)
    {
        foreach(GameObject animal in TileSelector.instance.animals)
        {
            if (animal.Equals(animalToDestroy.gameObject))
            {
                TileSelector.instance.animals.Remove(animal);
                break;
            }
        }
        Object.Destroy(animalToDestroy.gameObject);
        FindObjectOfType<AudioManager>().PlaySound("Destroy");
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
