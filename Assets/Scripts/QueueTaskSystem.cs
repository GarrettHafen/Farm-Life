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
        WorkerCharacter.instance.NotifyTaskQueued();
        queue.EnqueueAction(WorkerCharacter.instance.MoveTo(dirt.snapPosition));
        queue.EnqueueAction(TaskTimer(task, dirt));
        queue.EnqueueWait(waitTime);
    }

    public void SetTask(string task, TreeTile tree)
    {
        //tree
        WorkerCharacter.instance.NotifyTaskQueued();
        queue.EnqueueAction(WorkerCharacter.instance.MoveTo(tree.snapPosition));
        queue.EnqueueAction(TaskTimer(task, tree));
        queue.EnqueueWait(waitTime);
    }

    public void SetTask(string task, AnimalTile animal)
    {
        //animal
        WorkerCharacter.instance.NotifyTaskQueued();
        queue.EnqueueAction(WorkerCharacter.instance.MoveTo(animal.snapPosition));
        queue.EnqueueAction(TaskTimer(task, animal));
        queue.EnqueueWait(waitTime);
    }

    public void SetTask(string task, DebrisTile debris)
    {
        WorkerCharacter.instance.NotifyTaskQueued();
        queue.EnqueueAction(WorkerCharacter.instance.MoveTo(debris.snapPosition));
        queue.EnqueueAction(TaskTimer(task, debris));
        queue.EnqueueWait(waitTime);
    }

    public void SetTask(string task, DecorationTile decoration)
    {
        WorkerCharacter.instance.NotifyTaskQueued();
        queue.EnqueueAction(WorkerCharacter.instance.MoveTo(decoration.snapPosition));
        queue.EnqueueAction(TaskTimer(task, decoration));
        queue.EnqueueWait(waitTime);
    }

    public void SetTask(Crop c, PlayerInteraction player, DirtTile dirt)
    {
        //plant seed
        WorkerCharacter.instance.NotifyTaskQueued();
        queue.EnqueueAction(WorkerCharacter.instance.MoveTo(dirt.snapPosition));
        queue.EnqueueAction(TaskTimer(c, player, dirt));
        queue.EnqueueWait(waitTime);
    }

    IEnumerator TaskTimer(string task, DirtTile dirt)
    {
        //dirt
        Slider[] sliders = dirt.GetComponentsInChildren<Slider>(true);
        sliders[1].gameObject.SetActive(true);
        sliders[1].value = sliders[1].minValue;
        WorkerCharacter.instance.StartWorking(task);
        while (sliders[1].value < sliders[1].maxValue)
        {
            sliders[1].value += Time.deltaTime;
            yield return null;
        }
        WorkerCharacter.instance.StopWorking();
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
        WorkerCharacter.instance.StartWorking(task);
        while (sliders[1].value < sliders[1].maxValue)
        {
            sliders[1].value += Time.deltaTime;
            yield return null;
        }
        WorkerCharacter.instance.StopWorking();
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
        WorkerCharacter.instance.StartWorking(task);
        while (sliders[1].value < sliders[1].maxValue)
        {
            sliders[1].value += Time.deltaTime;
            yield return null;
        }
        WorkerCharacter.instance.StopWorking();
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

    // Debris prefab uses a single Slider at index [0] (no growth hover timer needed)
    IEnumerator TaskTimer(string task, DebrisTile debris)
    {
        Slider[] sliders = debris.GetComponentsInChildren<Slider>(true);
        sliders[0].gameObject.SetActive(true);
        sliders[0].value = sliders[0].minValue;
        WorkerCharacter.instance.StartWorking(task);
        while (sliders[0].value < sliders[0].maxValue)
        {
            sliders[0].value += Time.deltaTime;
            yield return null;
        }
        WorkerCharacter.instance.StopWorking();
        sliders[0].gameObject.SetActive(false);
        switch (task)
        {
            case "clearDebris":
                DebrisTile.instance.DestroyDebris(debris);
                break;
        }
    }

    // Decoration prefab uses a single Slider at index [0], same as DebrisTile.
    // FenceTile : DecorationTile, so this same overload handles fence removal too.
    IEnumerator TaskTimer(string task, DecorationTile decoration)
    {
        Slider[] sliders = decoration.GetComponentsInChildren<Slider>(true);
        sliders[0].gameObject.SetActive(true);
        sliders[0].value = sliders[0].minValue;
        WorkerCharacter.instance.StartWorking(task);
        while (sliders[0].value < sliders[0].maxValue)
        {
            sliders[0].value += Time.deltaTime;
            yield return null;
        }
        WorkerCharacter.instance.StopWorking();
        sliders[0].gameObject.SetActive(false);
        switch (task)
        {
            case "placeDecoration":
                PlayerInteraction.instance.FinishPlaceDecoration(decoration);
                break;
            case "clearDecoration":
                decoration.RemoveSelf();
                break;
        }
    }

    IEnumerator TaskTimer(Crop c, PlayerInteraction player, DirtTile dirt)
    {
        //plant seed
        Slider[] sliders = dirt.GetComponentsInChildren<Slider>(true);
        sliders[1].gameObject.SetActive(true);
        sliders[1].value = sliders[1].minValue;
        WorkerCharacter.instance.StartWorking("plantSeed");
        while (sliders[1].value < sliders[1].maxValue)
        {
            sliders[1].value += Time.deltaTime;
            yield return null;
        }
        WorkerCharacter.instance.StopWorking();
        sliders[1].gameObject.SetActive(false);

        DirtTile.instance.PlantSeed(c, player, dirt);
    }

    public int GetQueueCount()
    {
        return queue.GetQueueCount();
    }
}