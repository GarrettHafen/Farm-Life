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

    private void Start()
    {
        instance = this;
        queue = new QueueSystem(this);
        queue.StartLoop();
    }

    private void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.T))
        {
            //StartCoroutine(SliderTest());
            queue.EnqueueAction(TaskTimer());
            queue.EnqueueWait(3f);
        }
        */
    }

    public void SetTask(string task, DirtTile dirt)
    {
        queue.EnqueueAction(TaskTimer(task, dirt));
        queue.EnqueueWait(.5f);
    }

    IEnumerator TaskTimer(string task, DirtTile dirt)
    {
        Slider[] sliders = dirt.GetComponentsInChildren<Slider>(true);
        Debug.Log(sliders.Length);
        sliders[1].gameObject.SetActive(true);
        sliders[1].value = sliders[1].minValue;
        while (sliders[1].value < sliders[1].maxValue)
        {
            sliders[1].value += Time.deltaTime;
            yield return null;
        }
        sliders[1].gameObject.SetActive(false);
        CompleteTask(task, dirt);
    }

    void CompleteTask(string task, DirtTile dirt)
    {
        switch (task)
        {
            case "":

                break;
            case "firstPlow":

                break;
            case "secondPlow":
                //DirtTile.instance.Plow();

                break;
            case "plantCrop":

                break;
            case "plantTree":

                break;
            case "harvestCrop":
                DirtTile.instance.HarvestCrop(dirt);
                break;
            case "harvestTree":

                break;
            case "burn":

                break;
        }
    }
}
