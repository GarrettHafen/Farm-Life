using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

public class MarketController : MonoBehaviour
{
    public static MarketController instance;
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

    public List<TreeAsset> treeAssetList;

    public GameObject cropCell;
    private int pageNumber;
    private int cellNumber = 0;
    public GameObject cropCellParent;
    
    public GameObject backButton;
    public GameObject forwardButton;

    public MarketState marketState;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
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

    public void Test()
    {
        Debug.Log("test");
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
        FindObjectOfType<AudioManager>().PlaySound("Click");
    }

    public void DeactivateMarket()
    {
        marketOpen = false;
        market.SetActive(false);
        FindObjectOfType<AudioManager>().PlaySound("Click");
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
        PopulateMarket();
        FindObjectOfType<AudioManager>().PlaySound("Page Turn");
    }

    public void LowerPageNumber()
    {
        //same as the raise, but lower. 
        if(pageNumber > 1)
        { 
            pageNumber -= 9;
            PopulateMarket();
            FindObjectOfType<AudioManager>().PlaySound("Page Turn");
        }
        forwardButton.SetActive(true);
    }

    public void PopulateMarket()
    {
        cropAssetList.Clear();
        treeAssetList.Clear();
        cellNumber = 0;
        forwardButton.SetActive(true);
        //based on page number, populate those crops on the market page.
        //should always start with one
        //------------------------need to eventually handle unavialable crops due to level------------------------
        for (int i = pageNumber; i < pageNumber + 9; i++)
        {
            switch (marketState) {
                case MarketState.Crop:
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
                            
                            if(GameHandler.instance.cropsList[i] == null)// this code probably only applies to an incomplete croplist
                            {
                                cropNameList[cellNumber].gameObject.SetActive(false);
                            }
                            else
                            {
                                cropNameList[cellNumber].gameObject.SetActive(true);
                                cropNameList[cellNumber].text = "Unlocked at lvl: " + GameHandler.instance.cropsList[i].reqLvl.ToString();
                            }
                            
                            cropCostList[cellNumber].gameObject.SetActive(false);
                            cropYieldList[cellNumber].gameObject.SetActive(false);
                            cropTimeList[cellNumber].gameObject.SetActive(false);
                            cropExpList[cellNumber].gameObject.SetActive(false);
                            cropExpList[cellNumber].transform.parent.gameObject.SetActive(false);
                            cropImageList[cellNumber].gameObject.SetActive(false);
                            cellNumber++;
                        }
                        if (GameHandler.instance.cropsList[pageNumber + 9] == null || GameHandler.instance.cropsList[pageNumber + 9].reqLvl > StatsController.instance.GetLvl())
                        {
                            forwardButton.SetActive(false);
                        }
                        break;
                    }
                case MarketState.Tree:
                    {
                        //populate tree stuff
                        if (GameHandler.instance.treeList[i] != null && GameHandler.instance.treeList[i].reqLvl <= StatsController.instance.GetLvl())
                        {
                            cropLockedList[cellNumber].gameObject.SetActive(false);
                            cropNameList[cellNumber].text = GameHandler.instance.treeList[i].name;
                            cropNameList[cellNumber].gameObject.SetActive(true);
                            cropCostList[cellNumber].text = GameHandler.instance.treeList[i].treeCost.ToString();
                            cropCostList[cellNumber].gameObject.SetActive(true);
                            cropYieldList[cellNumber].text = GameHandler.instance.treeList[i].treeReward.ToString();
                            cropYieldList[cellNumber].gameObject.SetActive(true);
                            cropTimeList[cellNumber].text = GetTimeString(GameHandler.instance.treeList[i].treeTimer);
                            cropTimeList[cellNumber].gameObject.SetActive(true);
                            cropExpList[cellNumber].text = GameHandler.instance.treeList[i].expReward.ToString();
                            cropExpList[cellNumber].gameObject.SetActive(true);
                            cropExpList[cellNumber].transform.parent.gameObject.SetActive(true);
                            cropImageList[cellNumber].sprite = GameHandler.instance.treeList[i].treeIconSprite;
                            cropImageList[cellNumber].gameObject.SetActive(true);
                            treeAssetList.Add(GameHandler.instance.treeList[i]);
                            cellNumber++;
                        }
                        else
                        {
                            cropLockedList[cellNumber].gameObject.SetActive(true);
                            cropNameList[cellNumber].gameObject.SetActive(true);
                            cropNameList[cellNumber].text = "Unlocked at lvl: " + GameHandler.instance.treeList[i].reqLvl.ToString();
                            cropCostList[cellNumber].gameObject.SetActive(false);
                            cropYieldList[cellNumber].gameObject.SetActive(false);
                            cropTimeList[cellNumber].gameObject.SetActive(false);
                            cropExpList[cellNumber].gameObject.SetActive(false);
                            cropExpList[cellNumber].transform.parent.gameObject.SetActive(false);
                            cropImageList[cellNumber].gameObject.SetActive(false);
                            cellNumber++;
                        }
                        if (GameHandler.instance.treeList[pageNumber + 9] == null || GameHandler.instance.treeList[pageNumber + 9].reqLvl > StatsController.instance.GetLvl())
                        {
                            forwardButton.SetActive(false);
                        }
                        break;
                    }
            }
        }
        
    }

    public void SetSeed(int cropNumber)//sets trees and animals etc also
    {
        switch (marketState)
        {
            case MarketState.Crop:
                {
                    PlayerInteraction.instance.SetCrop(new Crop(cropAssetList[cropNumber]));
                    MenuController.instance.hasSeed = true;
                    break;
                }
            case MarketState.Tree:
                {
                    PlayerInteraction.instance.SetTree(new Tree(treeAssetList[cropNumber]));
                    MenuController.instance.hasTree = true;
                    break;
                }
        }
        FindObjectOfType<AudioManager>().PlaySound("Buy Button");

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

    public void SetMarketState(string input)
    {
        switch (input)
        {
            case "Crop":
                marketState = MarketState.Crop;
                pageNumber = 1;
                break;
            case "Tree":
                marketState = MarketState.Tree;
                pageNumber = 1;
                break;
        }
        FindObjectOfType<AudioManager>().PlaySound("Click");
    }

    public void ComingSoon()
    {
        MenuController.instance.notificationBar.SetActive(false);
        MenuController.instance.AnimateNotifcation("Coming Soon", Color.white, "Manual Save");
        FindObjectOfType<AudioManager>().PlaySound("Click");
    }
}

public enum MarketState
{
    Crop,
    Tree,
    Animal,
    Decoration,
    Expansion
}
