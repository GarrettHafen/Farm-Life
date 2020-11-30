using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class QueueTaskSystem : MonoBehaviour
{
    public static QueueTaskSystem instance;
    //public Slider slider;
    QueueSystem queue;

    float waitTime = .15f;

    private void Start()
    {
        instance = this;
        queue = new QueueSystem(this);
        queue.StartLoop();
    }

    public void SetTask(string task, DirtTile dirt)
    {
        //dirt
        queue.EnqueueAction(TaskTimer(task, dirt));
        queue.EnqueueWait(waitTime);
    }

    public void SetTask(string task, TreeTile tree)
    {
        //tree
        queue.EnqueueAction(TaskTimer(task, tree));
        queue.EnqueueWait(waitTime);
    }

    public void SetTask(string task, AnimalTile animal)
    {
        //animal
        queue.EnqueueAction(TaskTimer(task, animal));
        queue.EnqueueWait(waitTime);
    }

    public void SetTask(Crop c, PlayerInteraction player, DirtTile dirt)
    {
        //plant seed
        queue.EnqueueAction(TaskTimer(c, player, dirt));
        queue.EnqueueWait(waitTime);
    }

    IEnumerator TaskTimer(string task, DirtTile dirt)
    {
        //dirt
        Slider[] sliders = dirt.GetComponentsInChildren<Slider>(true);
        sliders[1].gameObject.SetActive(true);
        sliders[1].value = sliders[1].minValue;
        while (sliders[1].value < sliders[1].maxValue)
        {
            sliders[1].value += Time.deltaTime;
            yield return null;
        }
        sliders[1].gameObject.SetActive(false);
        switch(task)
        {
            case "firstPlow":
                PlayerInteraction.instance.FinishFirstPlow(dirt);
                break;
            case "secondPlow":
                DirtTile.instance.Plow(dirt);

                break;
            case "harvestCrop":
                DirtTile.instance.HarvestCrop(dirt);
            break;
            case "burn":
                DirtTile.instance.DestroyPlot(dirt);
                break;
        }
    }

    IEnumerator TaskTimer(string task, TreeTile tree)
    {
        //tree
        Slider[] sliders = tree.GetComponentsInChildren<Slider>(true);
        sliders[1].gameObject.SetActive(true);
        sliders[1].value = sliders[1].minValue;
        while (sliders[1].value < sliders[1].maxValue)
        {
            sliders[1].value += Time.deltaTime;
            yield return null;
        }
        sliders[1].gameObject.SetActive(false);
        switch (task)
        {
            case "plantTree":
                PlayerInteraction.instance.FinishPlantTree(tree);
                break; 
            case "harvestTree":
                TreeTile.instance.HarvestTree(tree);
                break;
            case "burn":
                TreeTile.instance.DestroyTree(tree);
                break;
        }
    }

    IEnumerator TaskTimer(string task, AnimalTile animal)
    {
        //animal
        Slider[] sliders = animal.GetComponentsInChildren<Slider>(true);
        sliders[1].gameObject.SetActive(true);
        sliders[1].value = sliders[1].minValue;
        while (sliders[1].value < sliders[1].maxValue)
        {
            sliders[1].value += Time.deltaTime;
            yield return null;
        }
        sliders[1].gameObject.SetActive(false);
        switch (task)
        {
            case "placeAnimal":
                PlayerInteraction.instance.FinishPlaceAnimal(animal);
                break;
            case "harvestAnimal":
                AnimalTile.instance.HarvestAnimal(animal);
                break;
            case "burn":
                AnimalTile.instance.DestroyAnimal(animal);
                break;
        }
    }

    IEnumerator TaskTimer(Crop c, PlayerInteraction player, DirtTile dirt)
    {
        //plant seed
        Slider[] sliders = dirt.GetComponentsInChildren<Slider>(true);
        sliders[1].gameObject.SetActive(true);
        sliders[1].value = sliders[1].minValue;
        while (sliders[1].value < sliders[1].maxValue)
        {
            sliders[1].value += Time.deltaTime;
            yield return null;
        }
        sliders[1].gameObject.SetActive(false);

        DirtTile.instance.PlantSeed(c, player, dirt);
    }

    public int GetQueueCount()
    {
        return queue.GetQueueCount();
    }
}