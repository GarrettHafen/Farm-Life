using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class QueueProgressThingy : MonoBehaviour
{
    public Slider slider;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            for (int i = 0; i < 10; i++)
            {
                StartCoroutine(ExecuteAfterTime(1));
            }
        }
    }

    public void SetTime(float time)
    {
        if (time > 1.1f)
        {
            slider.value = (time) - 1f;
        }
        else
        {
            slider.value = time;
        }
    }
    public void SetTime()
    {
        slider.value = GetTime() + .1f;
    }
    public float GetTime()
    {
        return slider.value;
    }

    IEnumerator ExecuteAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        SetTime();
    }
}
