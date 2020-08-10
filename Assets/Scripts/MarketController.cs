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
    public List<Text> cropNameList;
    public List<Text> cropCostList;
    public List<Text> cropYieldList;
    public List<Text> cropExpList;
    public List<Text> cropTimeList;
    public List<Image> cropImageList;
    public List<CropAsset> cropAssetList;
    public List<Image> cropLockedList;

    public GameObject cropCell;
    private int pageNumber;
    private int cellNumber = 0;
    public GameObject cropCellParent;
    
    public GameObject backButton;
    public GameObject forwardButton;

    // Start is called before the first frame update
    void Start()
    {   
    }

    // Update is called once per frame
    void Update()
    {
        if(pageNumber == 1)
        {
            backButton.SetActive(false);
        }
        else
        {
            backButton.SetActive(true);
        }
    }

    public void ActivateMarket()
    {
        marketOpen = true;
        market.SetActive(true);
        pageNumber = 1;
        cellNumber = 0;
        coinText.text = StatsController.instance.GetCoins().ToString();
        lvlText.text = StatsController.instance.GetLvl().ToString();
        PopulateMarket();
    }

    public void DeactivateMarket()
    {
        marketOpen = false;
        market.SetActive(false);
    }

    public void RaisePageNumber()
    {
        //this will increment a number and control which part of the crop array is displayed
        // for example, page 1 will be 0-8, then the PageNumber variable will increment and
        // 9-17 will show, etc
        //maybe increment pageNumber by 9? 1, 10, 19, 28....
        /*	    1	2	3	4	5	6	7	8	9
            1   1   2   3   4   5   6   7   8   9
            2   10  11  12  13  14  15  16  17  18
            3   19  20  21  22  23  24  25  26  27
            4   28  29  30  31  32  33  34  35  36
            5   37  38  39  40  41  42  43  44  45
            6   46  47  48  49  50  51  52  53  54
            7   55  56  57  58  59  60  61  62  63
            8   64  65  66  67  68  69  70  71  72
            9   73  74  75  76  77  78  79  80  81
            10  82  83  84  85  86  87  88  89  90*/

        pageNumber += 9;
        cellNumber = 0;
        PopulateMarket();
    }

    public void LowerPageNumber()
    {
        //same as the raise, but lower. 
        if(pageNumber > 1)
        { 
            pageNumber -= 9;
            cellNumber = 0;
            PopulateMarket();
        }
        forwardButton.SetActive(true);
    }

    public void PopulateMarket()
    {
        cropAssetList.Clear();
        forwardButton.SetActive(true);
        //based on page number, populate those crops on the market page.
        //should always start with one
        //------------------------need to eventually handle unavialable crops due to level------------------------
        for (int i = pageNumber; i < pageNumber + 9; i++)
        {

            if (GameHandler.instance.cropsList[i] != null && GameHandler.instance.cropsList[i].reqLvl <= StatsController.instance.GetLvl())
            {
                cropLockedList[cellNumber].gameObject.SetActive(false);
                cropNameList[cellNumber].text = GameHandler.instance.cropsList[i].cropName;
                cropNameList[cellNumber].gameObject.SetActive(true);
                cropCostList[cellNumber].text = GameHandler.instance.cropsList[i].cropCost.ToString();
                cropCostList[cellNumber].gameObject.SetActive(true);
                cropYieldList[cellNumber].text = GameHandler.instance.cropsList[i].cropReward.ToString();
                cropYieldList[cellNumber].gameObject.SetActive(true);
                cropTimeList[cellNumber].text = GetTimeString(GameHandler.instance.cropsList[i].cropTimer);
                cropTimeList[cellNumber].gameObject.SetActive(true);
                cropExpList[cellNumber].text = GameHandler.instance.cropsList[i].expReward.ToString();
                cropExpList[cellNumber].gameObject.SetActive(true);
                cropExpList[cellNumber].transform.parent.gameObject.SetActive(true);
                cropImageList[cellNumber].sprite = GameHandler.instance.cropsList[i].iconSprite;
                cropImageList[cellNumber].gameObject.SetActive(true);
                cropAssetList.Add(GameHandler.instance.cropsList[i]);
                cellNumber++;
            }
            else
            {
                cropLockedList[cellNumber].gameObject.SetActive(true);
                cropNameList[cellNumber].gameObject.SetActive(false);
                cropCostList[cellNumber].gameObject.SetActive(false);
                cropYieldList[cellNumber].gameObject.SetActive(false);
                cropTimeList[cellNumber].gameObject.SetActive(false);
                cropExpList[cellNumber].gameObject.SetActive(false);
                cropExpList[cellNumber].transform.parent.gameObject.SetActive(false);
                cropImageList[cellNumber].gameObject.SetActive(false);
                cellNumber++;
            }
        }
        if (GameHandler.instance.cropsList[pageNumber+9] == null || GameHandler.instance.cropsList[pageNumber + 9].reqLvl > StatsController.instance.GetLvl())
        {
            forwardButton.SetActive(false);
        }
    }

    public void SetSeed(int cropNumber)
    {
        PlayerInteraction.instance.SetCrop(new Crop(cropAssetList[cropNumber]));

    }

    public string GetTimeString(float timeInSeconds)
    {
        //return whole number based on number of seconds, if more than 60 seconds = 1 minute
        // if more than 60 minutes = 1 hour etc.
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
            return timeInSeconds + " Seconds";
            
        }

        
        

    }
}
