using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MarketController : MonoBehaviour
{
    public static MarketController instance;
    public GameObject market;
    private bool marketOpen;
    public TMP_Text coinText;
    public Text lvlText;

    [Header("Icon Grid")]
    public List<Image> slotIconList;
    public List<Button> slotButtonList;

    [Header("Universal Detail Panel")]
    public GameObject detailPanel;
    public TMP_Text detailNameText;
    public GameObject detailStatsPanel;
    public TMP_Text detailYieldAmountText;
    public TMP_Text detailExpAmountText;
    public TMP_Text detailTimeAmountText;
    public TMP_Text detailTimeUnitText;
    public Button actionButton;
    public TMP_Text actionButtonText;

    public List<CropAsset> cropAssetList;
    public List<TreeAsset> treeAssetList;
    public List<AnimalAsset> animalAssetList;
    public List<DecorationAsset> decorationAssetList;
    public List<FarmZoneAsset> expansionAssetList;
    private readonly List<bool> slotLockedList = new();
    private static readonly Color lockedColor = new Color(0.4f, 0.4f, 0.4f, 1f);

    private int pageNumber = 1;
    private int cellNumber = 0;
    private int selectedSlot = -1;

    public GameObject backButton;
    public GameObject forwardButton;

    [Header("HUD Elements")]
    public GameObject longMenu;
    public GameObject statsPanel;

    public MarketState marketState;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        for (int i = 0; i < slotButtonList.Count; i++)
        {
            int slotIndex = i;
            slotButtonList[i].onClick.AddListener(() => SelectSlot(slotIndex));
        }

        actionButton.onClick.AddListener(() =>
        {
            SetSeed(selectedSlot);
            DeactivateMarket();
        });
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
        longMenu.SetActive(false);
        statsPanel.SetActive(false);
        cellNumber = 0;
        coinText.text = $"{StatsController.instance.GetCoins().ToString()} C";
        lvlText.text = StatsController.instance.GetLvl().ToString();
        PopulateMarket();
        AudioManager.instance.PlaySound("Click");
    }

    public void DeactivateMarket()
    {
        marketOpen = false;
        market.SetActive(false);
        longMenu.SetActive(true);
        statsPanel.SetActive(true);
        AudioManager.instance.PlaySound("Click");
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

        pageNumber += slotIconList.Count;
        PopulateMarket();
        AudioManager.instance.PlaySound("Page Turn");
    }

    public void LowerPageNumber()
    {
        //same as the raise, but lower.
        if(pageNumber > 1)
        {
            pageNumber -= slotIconList.Count;
            PopulateMarket();
            AudioManager.instance.PlaySound("Page Turn");
        }
        forwardButton.SetActive(true);
    }

    public void PopulateMarket()
    {
        cropAssetList.Clear();
        treeAssetList.Clear();
        animalAssetList.Clear();
        decorationAssetList.Clear();
        expansionAssetList.Clear();
        slotLockedList.Clear();
        cellNumber = 0;
        forwardButton.SetActive(true);
        selectedSlot = -1;
        detailPanel.SetActive(false);

        int slotCount = slotIconList.Count;

        // Clear all slots so previous tab content doesn't bleed through on shorter lists.
        for (int c = 0; c < slotCount; c++)
        {
            if (slotIconList[c] == null || slotButtonList[c] == null)
            {
                Debug.LogError($"MarketController: slotIconList/slotButtonList index {c} is unassigned or points at a deleted object — check the Inspector wiring.");
                continue;
            }
            slotIconList[c].gameObject.SetActive(false);
            slotButtonList[c].interactable = false;
        }

        // Pre-compute list size so the loop and forward button check can't go out of bounds
        int listCount = marketState switch
        {
            MarketState.Crop       => GameHandler.instance.cropsList.Count,
            MarketState.Animal     => GameHandler.instance.animalList.Count,
            MarketState.Tree       => GameHandler.instance.treeList.Count,
            MarketState.Decoration => GameHandler.instance.decorationList.Count + 1,
            MarketState.Expansion  => GetPurchasableZones().Count + 1,
            _                      => 0
        };

        int upperBound = Mathf.Min(pageNumber + slotCount, listCount);

        //based on page number, populate those crops on the market page.
        //should always start with one
        //------------------------need to eventually handle unavialable crops due to level------------------------
        for (int i = pageNumber; i < upperBound; i++)
        {
            switch (marketState)
            {
                case MarketState.Crop:
                    PopulateCropSlot(i);
                    break;
                case MarketState.Animal:
                    PopulateAnimalSlot(i);
                    break;
                case MarketState.Tree:
                    PopulateTreeSlot(i);
                    break;
                case MarketState.Decoration:
                    PopulateDecorationSlot(i);
                    break;
                case MarketState.Expansion:
                    PopulateExpansionSlot(i);
                    break;
            }
        }

        // Hide forward button if there's no next page or it's all locked content
        int nextPage = pageNumber + slotCount;
        if (nextPage >= listCount)
        {
            forwardButton.SetActive(false);
        }
        else
        {
            switch (marketState)
            {
                case MarketState.Crop:
                    if (GameHandler.instance.cropsList[nextPage] == null || GameHandler.instance.cropsList[nextPage].reqLvl > StatsController.instance.GetLvl())
                        forwardButton.SetActive(false);
                    break;
                case MarketState.Animal:
                    if (GameHandler.instance.animalList[nextPage] == null || GameHandler.instance.animalList[nextPage].reqLvl > StatsController.instance.GetLvl())
                        forwardButton.SetActive(false);
                    break;
                case MarketState.Tree:
                    if (GameHandler.instance.treeList[nextPage] == null || GameHandler.instance.treeList[nextPage].reqLvl > StatsController.instance.GetLvl())
                        forwardButton.SetActive(false);
                    break;
                case MarketState.Decoration:
                    if (GameHandler.instance.decorationList[nextPage - 1] == null || GameHandler.instance.decorationList[nextPage - 1].reqLvl > StatsController.instance.GetLvl())
                        forwardButton.SetActive(false);
                    break;
                case MarketState.Expansion:
                    // expansions are always shown (owned or locked), so always allow paging if more exist
                    break;
            }
        }
    }

    private void PopulateCropSlot(int i)
    {
        CropAsset crop = GameHandler.instance.cropsList[i];
        cropAssetList.Add(crop);

        if (crop == null) // this code probably only applies to an incomplete croplist
        {
            slotLockedList.Add(false);
            cellNumber++;
            return;
        }

        bool unlocked = crop.reqLvl <= StatsController.instance.GetLvl();
        slotIconList[cellNumber].sprite = crop.iconSprite;
        slotIconList[cellNumber].gameObject.SetActive(true);
        slotIconList[cellNumber].color = unlocked ? Color.white : lockedColor;
        slotButtonList[cellNumber].interactable = true;

        slotLockedList.Add(!unlocked);
        cellNumber++;
    }

    private void PopulateAnimalSlot(int i)
    {
        AnimalAsset animal = GameHandler.instance.animalList[i];
        animalAssetList.Add(animal);

        if (animal == null)
        {
            slotLockedList.Add(false);
            cellNumber++;
            return;
        }

        bool unlocked = animal.reqLvl <= StatsController.instance.GetLvl();
        slotIconList[cellNumber].sprite = animal.animalIconSprite;
        slotIconList[cellNumber].gameObject.SetActive(true);
        slotIconList[cellNumber].color = unlocked ? Color.white : lockedColor;
        slotButtonList[cellNumber].interactable = true;

        slotLockedList.Add(!unlocked);
        cellNumber++;
    }

    private void PopulateTreeSlot(int i)
    {
        TreeAsset tree = GameHandler.instance.treeList[i];
        treeAssetList.Add(tree);

        if (tree == null)
        {
            slotLockedList.Add(false);
            cellNumber++;
            return;
        }

        bool unlocked = tree.reqLvl <= StatsController.instance.GetLvl();
        slotIconList[cellNumber].sprite = tree.treeIconSprite;
        slotIconList[cellNumber].gameObject.SetActive(true);
        slotIconList[cellNumber].color = unlocked ? Color.white : lockedColor;
        slotButtonList[cellNumber].interactable = true;

        slotLockedList.Add(!unlocked);
        cellNumber++;
    }

    private void PopulateDecorationSlot(int i)
    {
        DecorationAsset decoration = GameHandler.instance.decorationList[i - 1];
        decorationAssetList.Add(decoration);

        if (decoration == null)
        {
            slotLockedList.Add(false);
            cellNumber++;
            return;
        }

        bool unlocked = decoration.reqLvl <= StatsController.instance.GetLvl();
        slotIconList[cellNumber].sprite = decoration.iconSprite;
        slotIconList[cellNumber].gameObject.SetActive(true);
        slotIconList[cellNumber].color = unlocked ? Color.white : lockedColor;
        slotButtonList[cellNumber].interactable = true;

        slotLockedList.Add(!unlocked);
        cellNumber++;
    }

    private void PopulateExpansionSlot(int i)
    {
        List<ZoneData> purchasable = GetPurchasableZones();
        FarmZoneAsset zone = purchasable[i - 1].zoneAsset;
        bool levelMet = zone.unlockLevel <= StatsController.instance.GetLvl();

        expansionAssetList.Add(zone);
        slotIconList[cellNumber].sprite = zone.iconSprite;
        slotIconList[cellNumber].gameObject.SetActive(zone.iconSprite != null);
        slotIconList[cellNumber].color = levelMet ? Color.white : lockedColor;
        slotButtonList[cellNumber].interactable = true;

        slotLockedList.Add(!levelMet);
        cellNumber++;
    }

    public void SelectSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotLockedList.Count)
            return;

        selectedSlot = slotIndex;
        AudioManager.instance.PlaySound("Click");
        detailPanel.SetActive(true);

        bool locked = slotLockedList[slotIndex];

        switch (marketState)
        {
            case MarketState.Crop:
                ShowCropDetails(cropAssetList[slotIndex], locked);
                break;
            case MarketState.Animal:
                ShowAnimalDetails(animalAssetList[slotIndex], locked);
                break;
            case MarketState.Tree:
                ShowTreeDetails(treeAssetList[slotIndex], locked);
                break;
            case MarketState.Decoration:
                ShowDecorationDetails(decorationAssetList[slotIndex], locked);
                break;
            case MarketState.Expansion:
                ShowExpansionDetails(expansionAssetList[slotIndex], locked);
                break;
        }
    }

    private void ShowCropDetails(CropAsset crop, bool locked)
    {
        if (locked)
        {
            detailNameText.text = "Unlocked at lvl: " + crop.reqLvl;
            detailStatsPanel.SetActive(false);
            actionButton.gameObject.SetActive(false);
            return;
        }

        detailNameText.text = crop.cropName;
        detailYieldAmountText.text = crop.cropReward.ToString();
        detailExpAmountText.text = crop.expReward.ToString();
        SetTimeText(crop.cropTimer);
        detailStatsPanel.SetActive(true);
        SetActionButtonCost(crop.cropCost);
    }

    private void ShowAnimalDetails(AnimalAsset animal, bool locked)
    {
        if (locked)
        {
            detailNameText.text = "Unlocked at lvl: " + animal.reqLvl;
            detailStatsPanel.SetActive(false);
            actionButton.gameObject.SetActive(false);
            return;
        }

        detailNameText.text = animal.animalName;
        detailYieldAmountText.text = animal.animalReward.ToString();
        detailExpAmountText.text = animal.expReward.ToString();
        SetTimeText(animal.animalTimer);
        detailStatsPanel.SetActive(true);
        SetActionButtonCost(animal.animalCost);
    }

    private void ShowTreeDetails(TreeAsset tree, bool locked)
    {
        if (locked)
        {
            detailNameText.text = "Unlocked at lvl: " + tree.reqLvl;
            detailStatsPanel.SetActive(false);
            actionButton.gameObject.SetActive(false);
            return;
        }

        detailNameText.text = tree.treeName;
        detailYieldAmountText.text = tree.treeReward.ToString();
        detailExpAmountText.text = tree.expReward.ToString();
        SetTimeText(tree.treeTimer);
        detailStatsPanel.SetActive(true);
        SetActionButtonCost(tree.treeCost);
    }

    private void ShowDecorationDetails(DecorationAsset decoration, bool locked)
    {
        // No growth/yield/exp stats for a static prop — just name + cost, same shape as ShowExpansionDetails.
        detailStatsPanel.SetActive(false);

        if (locked)
        {
            detailNameText.text = "Unlocked at lvl: " + decoration.reqLvl;
            actionButton.gameObject.SetActive(false);
            return;
        }

        detailNameText.text = decoration.decorationName;
        SetActionButtonCost(decoration.placeCost);
    }

    private void ShowExpansionDetails(FarmZoneAsset zone, bool locked)
    {
        detailStatsPanel.SetActive(false);

        if (locked)
        {
            detailNameText.text = "Unlocked at lvl: " + zone.unlockLevel;
            actionButton.gameObject.SetActive(false);
            return;
        }

        detailNameText.text = zone.zoneName;
        actionButton.gameObject.SetActive(true);

        bool alreadyOwned = TileSelector.instance.zones.Find(z => z.zoneAsset == zone).isUnlocked;
        if (alreadyOwned)
        {
            actionButtonText.text = "Owned";
            actionButton.interactable = false;
        }
        else
        {
            SetActionButtonCost(zone.unlockCost);
        }
    }

    private void SetActionButtonCost(int cost)
    {
        actionButton.gameObject.SetActive(true);
        actionButton.interactable = true;
        actionButtonText.text = cost > 0 ? cost + " C" : "Free";
    }

    public void SetSeed(int cropNumber)//sets trees and animals etc also
    {
        switch (marketState)
        {
            case MarketState.Crop:
                {
                    MenuController.instance.toolState.SetSeed();
                    PlayerInteraction.instance.SetCrop(new Crop(cropAssetList[cropNumber]));

                    break;
                }
            case MarketState.Tree:
                {
                    MenuController.instance.toolState.SetTree();
                    PlayerInteraction.instance.SetTree(new Tree(treeAssetList[cropNumber]));

                    break;
                }
            case MarketState.Animal:
                {
                    MenuController.instance.toolState.SetAnimal();
                    PlayerInteraction.instance.SetAnimal(new Animal(animalAssetList[cropNumber]));

                    break;
                }
            case MarketState.Decoration:
                {
                    MenuController.instance.toolState.SetDecoration();
                    PlayerInteraction.instance.SetDecoration(decorationAssetList[cropNumber]);

                    break;
                }
            case MarketState.Expansion:
                {
                    BuyExpansion(cropNumber);
                    return;
                }
        }
        AudioManager.instance.PlaySound("Buy Button");

    }

    private void SetTimeText(float timeInSeconds)
    {
        //split into a whole number amount and a unit, escalating from seconds to days
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
                    detailTimeAmountText.text = days.ToString();
                    detailTimeUnitText.text = "Days";
                }
                else
                {
                    detailTimeAmountText.text = hours.ToString();
                    detailTimeUnitText.text = "Hours";
                }
            }
            else
            {
                detailTimeAmountText.text = minutes.ToString();
                detailTimeUnitText.text = "Minutes";
            }
        }
        else
        {
            detailTimeAmountText.text = timeInSeconds.ToString();
            detailTimeUnitText.text = "Seconds";
        }
    }

    // Kept for existing Inspector-wired OnClick calls (UnityEvent persistent calls only support string/int/float/bool/Object arguments).
    public void SetMarketState(string input)
    {
        if (Enum.TryParse(input, out MarketState state))
            SetMarketState(state);
    }

    // Preferred entry point for code-driven callers (e.g. MarketTabBar), since it can't typo a category name.
    public void SetMarketState(MarketState state)
    {
        marketState = state;
        pageNumber = 1;
        PopulateMarket();
        AudioManager.instance.PlaySound("Click");
    }

    public void BuyExpansion(int index)
    {
        FarmZoneAsset zone = expansionAssetList[index];

        if (TileSelector.instance.zones.Find(z => z.zoneAsset == zone).isUnlocked)
        {
            MenuController.instance.AnimateNotifcation("Already owned!", Color.yellow, "Null");
            return;
        }

        if (zone.unlockLevel > StatsController.instance.GetLvl())
        {
            MenuController.instance.AnimateNotifcation("Level too low!", Color.red, "Error");
            return;
        }

        if (!StatsController.instance.CheckMaster(zone.unlockCost))
        {
            MenuController.instance.notificationBar.SetActive(false);
            MenuController.instance.AnimateNotifcation("Insufficient Funds", Color.red, "No Money");
            return;
        }

        StatsController.instance.RemoveCoins(zone.unlockCost);
        TileSelector.instance.UnlockZone(zone.zoneName);
        AudioManager.instance.PlaySound("Buy Button");
        MenuController.instance.notificationBar.SetActive(false);
        MenuController.instance.AnimateNotifcation(zone.zoneName + " Unlocked!", Color.green, "Null");
        PopulateMarket();
    }

    private List<ZoneData> GetPurchasableZones()
    {
        var list = new List<ZoneData>();
        foreach (ZoneData zone in TileSelector.instance.zones)
            if (zone.zoneAsset != null && !zone.zoneAsset.unlockedByDefault)
                list.Add(zone);
        return list;
    }

    public void ComingSoon()
    {
        MenuController.instance.notificationBar.SetActive(false);
        MenuController.instance.AnimateNotifcation("Coming Soon", Color.white, "Manual Save");
        AudioManager.instance.PlaySound("Click");
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
