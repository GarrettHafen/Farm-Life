using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Packages.Rider.Editor.UnitTesting;

public class StatsController : MonoBehaviour
{
    public static StatsController instance;

    public Text coinsText;
    public Text levelText;

    private int playerLevel = 1;
    private int maxLevel = 99;
    private int baseEXP = 100;
    private int currentEXP = 0;
    private int[] expToNextLevel;

    /*
     * had to split the coins into two types for new queue system. 
     * need to make sure the player can execute a queued system before
     * adding it to the task list. FarmVille cancels the queue if a task 
     * cant complete due to insufficient funds, which seems dumb to me. 
     * so i created the master coins list, and when a task completes, the 
     * display will update to with the same change as master. master shouldn't
     * get out of sync with display...but its a concern.
     */
    private float masterCoins = 50;
    private float displayCoins = 50;

    private float targetCoinsTotal;

    //radial fill 
    public float minimum;
    public float maximum;
    public float current;
    public Image mask;
    public Image fill;
    public Color color;

    private QueueSystem queue;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        SetupExp();
        UpdateStats();

        //used for writing info to a file
        //WriteExpToFile();


        GetCurrentFill(currentEXP, 0f, expToNextLevel[playerLevel]);

        queue = new QueueSystem(this);
        queue.StartLoop();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            AddCoinsMaster(25);
            AddCoinsDisplay(25);
        }/*else if (Input.GetKeyDown(KeyCode.V))
        {
            AddCoins(250);
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            AddCoins(1000);
        }*/else if (Input.GetKeyDown(KeyCode.J))
        {
            AddExp(75);
        }else if (Input.GetKeyDown(KeyCode.K))
        {
            AddExp(500);
        }
        else if (Input.GetKeyDown(KeyCode.L) && GameHandler.instance.devMode)
        {
            SetLvl(60);
            SetExp(45318);
            SetCoins(5000);
            UpdateStats();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Master: " + masterCoins);
            Debug.Log("Display: " + displayCoins);
        }
    }

    public void AddCoins(int coinsToAdd)
    {
        AddCoinsMaster(coinsToAdd);
        AddCoinsDisplay(coinsToAdd);
    }

    void AddCoinsMaster(int coinsToAdd)
    {
        /*
         * doesn't need as much logic because its not going to be displayed
         */
        masterCoins += coinsToAdd;
    }

    void AddCoinsDisplay(int coinsToAdd)
    {
        /*
         * - displayed in UI
         * - if the coins get to high, i don't want 'coins' appended, ***pretty bad code, coins aren't being updated after this point, stupid***
         * - i want coins to count up slowly but it needs to queue
         * depending on how many coins are being added, it should count up 
         * faster or slower.
         * - queue actually updates display in UI
         */
        
        targetCoinsTotal = displayCoins + coinsToAdd;
        
        if (targetCoinsTotal > 999999999f)
        {
            targetCoinsTotal = 999999999f;
        }
        else
        {
            int speed = GetSpeed(coinsToAdd);
            queue.EnqueueAction(CountUpToTarget(displayCoins, targetCoinsTotal, speed));

        }
        displayCoins += coinsToAdd;

    }

    public bool CheckMaster(int coinsToRemove)
    {
        /*
         * if master has enough coins, then the task can proceed 
         */

        if(masterCoins >= coinsToRemove)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void RemoveCoinsMaster(int coinsToRemove)
    {
        /*
         * doesn't need as much logic because its not going to be displayed
         */

        masterCoins -= coinsToRemove;
    }

    public void RemoveCoinsDisplay(int coinsToRemove)
    {
        /*
         * needs to same logic as AddCoinsDisplay just in reverse
         * 
         */
        targetCoinsTotal = displayCoins - coinsToRemove;
        int speed = GetSpeed(coinsToRemove);
        queue.EnqueueAction(CountDownToTarget(displayCoins, targetCoinsTotal, speed));
        displayCoins -= coinsToRemove;
    }

    IEnumerator CountUpToTarget(float currentCoinsCU, float targetCoinsTotalCU, int speedCU)
    {
        /*
         * if coins are greater than x amount, remove Coins from display
         * count up coins at the count of speed
         */
        while (currentCoinsCU < targetCoinsTotalCU)
        {
            currentCoinsCU += speedCU;
            //currentCoinsCU = Mathf.Clamp(currentCoins, 0f, targetCoinsTotal); dont remember why we needed this clamp.....
            if(currentCoinsCU < 1000000)
            {
                coinsText.text = currentCoinsCU + " coins";
            }
            else
            {
                coinsText.text = currentCoinsCU.ToString();
            }
            yield return null;
        }
        
    }
    IEnumerator CountDownToTarget(float currentCoinsCD, float targetCoinsTotalCD, int speedCD)
    {
        /*
         * if coins are greater than x amount, remove Coins from display
         * count up coins at the count of speed
         */
        while (currentCoinsCD > targetCoinsTotalCD)
        { 
            currentCoinsCD -= speedCD;
            //currentCoinsCD = Mathf.Clamp(currentCoinsCD, 0f, targetCoinsTotal);
            if (currentCoinsCD < 1000000)
            {
                coinsText.text = currentCoinsCD + " coins";
            }
            else
            {
                coinsText.text = currentCoinsCD.ToString();
            }
        }
        yield return null;
    }

    private int GetSpeed(int coins)
    {
        /*
         * bad code ----- might not matter with new queue task system
         * 
         * 
         * not very elegant, if coins is small, speed is low.
         * code is to prevent coins from counting up for 
         * forever when doing things in mass 
         */
        int speed = 1;
        if (coins < 25)
        {
            speed = 1;
        }
        else if (coins >= 25 && coins < 100)
        {
            speed = 5;
        }
        else if (coins >= 100)
        {
            speed = 100;
        }
        return speed;
    }

    public void AddExp(int expToAdd)
    {
        currentEXP += expToAdd;
        if (playerLevel < maxLevel)
        {
            if (currentEXP >= expToNextLevel[playerLevel])
            {
                playerLevel++;
                levelText.text = playerLevel.ToString();
                FindObjectOfType<AudioManager>().PlaySound("Level Up");
            }
        }
        if (playerLevel >= maxLevel)
        {
            currentEXP = 0;
        }
        GetCurrentFill(currentEXP, expToNextLevel[playerLevel-1], expToNextLevel[playerLevel]);
    }

    public void GetCurrentFill(float currentEXP, float minimumEXP, float maximumEXP)
    {
        current = currentEXP;
        minimum = minimumEXP;
        maximum = maximumEXP;

        float currentOffset = current - minimum;
        float maximumOffset = maximum - minimum;
        float fillAmount = currentOffset / maximumOffset;
        fill.fillAmount = fillAmount;

        fill.color = color;
    }

    public void SetupExp()
    {
        expToNextLevel = new int[maxLevel];
        expToNextLevel[1] = baseEXP;
        for (int i = 2; i < expToNextLevel.Length; i++)
        {
            expToNextLevel[i] = (int)(Mathf.Floor(baseEXP * (Mathf.Pow(i, 1.5f))));   
        }
    }

    public float GetCoins()
    {
        return masterCoins;
    }
    public void SetCoins(float coins)
    {
        masterCoins = coins;
        displayCoins = coins;
    }
    public int GetLvl()
    {
        return playerLevel;
    }
    public void SetLvl(int lvl)
    {
        playerLevel = lvl;
    }
    public int GetExp()
    {
        return currentEXP;
    }
    public void SetExp(int exp)
    {
        currentEXP = exp;
    }
    public void UpdateStats()
    {
        coinsText.text = displayCoins + " coins";
        levelText.text = "" + playerLevel;
        GetCurrentFill(currentEXP, expToNextLevel[playerLevel - 1], expToNextLevel[playerLevel]);
    }
    private void WriteExpToFile()
    {
        /*--------------------------------------------------------------------
        *write all the values to a file for easy viewing*/
        string path = "Assets/test2.txt";
        StreamWriter writer = new StreamWriter(path, true);
         int count = 0; 
        foreach (int number in expToNextLevel)
        {
            count++;
            writer.WriteLine("To get from level " + (count-1) + " to level " + count + ", you need " + number + " experience.");
        }
        count = 0;
        foreach( int number in expToNextLevel)
        {
            writer.WriteLine("Level " + (count) + ": " + number);
            count++;
        }
        writer.Close();
        /*--------------------------------------------------------------------*/
    }
}
