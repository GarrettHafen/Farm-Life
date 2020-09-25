using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerController : MonoBehaviour
{
    public Slider slider;
    public Image timerSprite;

    public void SetTime(float time)
    {
        slider.value = time;
    }

    public void SetSprite(Sprite sprite)
    {
        timerSprite.sprite = sprite;
    }
}
