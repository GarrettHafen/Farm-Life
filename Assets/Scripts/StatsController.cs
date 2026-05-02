using System.Collections;
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

    // Single authoritative coin balance. displayedCoins chases this in Update()
    // so the UI always animates smoothly without any risk of desync.
    private float coins = 50f;
    private float displayedCoins = 50f;

    //radial fill
    public float minimum;
    public float maximum;
    public float current;
    public Image mask;
    public Image fill;
    public Color color;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        SetupExp();
        UpdateStats();
        GetCurrentFill(currentEXP, 0f, expToNextLevel[playerLevel]);
    }

    // Update is called once per frame
    void Update()
    {
        // Smoothly animate the displayed coin count toward the real balance
        float speed = Mathf.Max(10f, Mathf.Abs(coins - displayedCoins) * 5f);
        displayedCoins = Mathf.MoveTowards(displayedCoins, coins, speed * Time.deltaTime);
        int displayed = Mathf.RoundToInt(displayedCoins);
        coinsText.text = displayed < 1000000 ? displayed + " coins" : displayed.ToString();

        // Dev cheat keys
        if (Input.GetKeyDown(KeyCode.C))
        {
            AddCoins(25);
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            AddCoins(10000);
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            AddExp(75);
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            AddExp(500);
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            GameHandler.instance.TimeSkip();
        }
        else if (Input.GetKeyDown(KeyCode.L) && GameHandler.instance.devMode)
        {
            SetLvl(60);
            SetExp(45318);
            SetCoins(5000);
            UpdateStats();
        }
    }

    public void AddCoins(int coinsToAdd)
    {
        coins += coinsToAdd;
    }

    public bool CheckMaster(int coinsToRemove)
    {
        return coins >= coinsToRemove;
    }

    public void RemoveCoins(int coinsToRemove)
    {
        coins -= coinsToRemove;
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
                AudioManager.instance.PlaySound("Level Up");
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
        return coins;
    }
    public void SetCoins(float amount)
    {
        coins = amount;
        displayedCoins = amount; // snap on load so it doesn't animate from 0
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
        int displayed = Mathf.RoundToInt(displayedCoins);
        coinsText.text = displayed < 1000000 ? displayed + " coins" : displayed.ToString();
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
