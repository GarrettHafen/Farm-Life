using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

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
    private float currentCoins = 50;

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
            AddCoins(25);
        }else if (Input.GetKeyDown(KeyCode.V))
        {
            AddCoins(250);
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            AddCoins(1000);
        }else if (Input.GetKeyDown(KeyCode.J))
        {
            AddExp(75);
        }else if (Input.GetKeyDown(KeyCode.K))
        {
            AddExp(500);
        }
    }

    public void AddCoins(int coinsToAdd)
    {
        
        targetCoinsTotal = currentCoins + coinsToAdd;
        
        if (targetCoinsTotal > 999999999f)
        {
            targetCoinsTotal = 999999999f;
        }
        else
        {
            //StartCoroutine(CountUpToTarget(GetSpeed(coinsToAdd)));
            //StartCoroutine(Coroutine2(coinsToAdd));
            int speed = GetSpeed(coinsToAdd);
            queue.EnqueueAction(CountUpToTarget(currentCoins, targetCoinsTotal, speed));
            //queue.EnqueueWait(.5f);

        }
        currentCoins += coinsToAdd;

    }

    /*IEnumerator Coroutine2(int coinsToAdd)
    {
        yield return CountUpToTarget(GetSpeed(coinsToAdd));

        //Coroutine1 is now finished and you can use its result
    }*/

    public bool RemoveCoins(int coinsToRemove)
    {
        targetCoinsTotal = currentCoins - coinsToRemove;
        if(targetCoinsTotal < 0)
        {
            Debug.Log("cant do that you poor fool");
            MenuController.instance.notificationBar.SetActive(false);
            MenuController.instance.AnimateNotifcation("Insufficient Funds", Color.red);
            return false;
        }
        else
        {
            
            StartCoroutine(CountDownToTarget(GetSpeed(coinsToRemove)));
            return true;
        }
    }

    IEnumerator CountUpToTarget(float currentCoinsCU, float targetCoinsTotalCU, int speedCU)
    {

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
    IEnumerator CountDownToTarget(int speed)
    {
        while (currentCoins > targetCoinsTotal)
        {
            currentCoins -= speed;
            currentCoins = Mathf.Clamp(currentCoins, 0f, targetCoinsTotal);
            if (currentCoins < 1000000)
            {
                coinsText.text = currentCoins + " coins";
            }
            else
            {
                coinsText.text = currentCoins.ToString();
            }
        }
        yield return null;
    }

    private int GetSpeed(int coins)
    {
        int speed = 1;
        if (coins < 100)
        {
            speed = 1;
        }
        else if (coins >= 100 && coins < 1000)
        {
            speed = 5;
        }
        else if (coins >= 1000)
        {
            speed = 1000;
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
                //LevelUp(); becuase of using minimum, currentEXP does not need to be adjusted after level up

                playerLevel++;
                levelText.text = playerLevel.ToString();
            }
        }
        if (playerLevel >= maxLevel)
        {
            currentEXP = 0;
        }
        //Debug.Log("------------Passed through------------");
        //Debug.Log("currentEXP: " + currentEXP + " minimum: " + expToNextLevel[playerLevel - 1] + " maximum: " + expToNextLevel[playerLevel]);
        GetCurrentFill(currentEXP, expToNextLevel[playerLevel-1], expToNextLevel[playerLevel]);
    }
    public void GetCurrentFill(float currentEXP, float minimumEXP, float maximumEXP)
    {
        current = currentEXP;
        minimum = minimumEXP;
        maximum = maximumEXP;

        //Debug.Log("------------enter GetCurrentFill------------");
        float currentOffset = current - minimum;
        //Debug.Log("currentOffset " + currentOffset + " = current: " + current + " - minimum: " + minimum);
        float maximumOffset = maximum - minimum;
        //Debug.Log("maximumOffset " + maximumOffset + " = maximum: " + maximum + " - minimum: " + minimum);
        float fillAmount = currentOffset / maximumOffset;
        //Debug.Log("FillAmount " + fillAmount + " = currentOffset: " + currentOffset + " / maximumOffset: " + maximumOffset);
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
        /*for(int i = 0; i < expToNextLevel.Length; i++)
        {
            Debug.Log(i + ": " + expToNextLevel[i]);
        }*/
        
    }

    public float GetCoins()
    {
        return currentCoins;
    }
    public void SetCoins(float coins)
    {
        currentCoins = coins;
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
        coinsText.text = currentCoins + " coins";
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
