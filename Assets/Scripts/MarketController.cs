using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarketController : MonoBehaviour
{
    public GameObject market;
    private bool marketOpen;
    public Text coinText;
    public Text lvlText;
    public Text cropName;
    public Text cropCost;
    public Text cropYield;
    public Text cropExp;
    public Text cropTime;
    public Image cropImage;
    public CropAsset t;

    // Start is called before the first frame update
    void Start()
    {   
    }

    // Update is called once per frame
    void Update()
    {
        if (marketOpen)
        {
            coinText.text = StatsController.instance.GetCoins();
            lvlText.text = StatsController.instance.GetLvl();
            cropName.text = t.cropName;
            cropCost.text = t.cropCost.ToString();
            cropYield.text = t.cropReward.ToString();
            cropExp.text = t.expReward.ToString();
            cropTime.text = GetTimeString(t.cropTimer);
            cropImage.sprite = t.iconSprite;
        }
    }

    public void ActivateMarket()
    {
        marketOpen = true;
        market.SetActive(true);
        
    }

    public void DeactivateMarket()
    {
        marketOpen = false;
        market.SetActive(false);
    }

    public string GetTimeString(float timeInSeconds)
    {
        float minutes;
        float hours;
        float days;

        if (timeInSeconds >= 60)
        {
            minutes = timeInSeconds / 60;
            if (minutes >= 60)
            {
                hours = minutes / 60;
                if (hours >= 24)
                {
                    days = hours / 24;
                    return( days + " days");
                }
                else
                {
                    return (hours + " Hours");
                }
            }
            else
            {
                return (minutes + " Minutes");
            }
        }
        else
        { 
            return timeInSeconds + " Broke";
            Debug.Log("What went Wrong?");
        }

        
        

    }
}
