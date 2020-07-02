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
    private float currentCoins = 500;

    private float targetCoinsTotal;

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
        coinsText.text = currentCoins + " coins";
        levelText.text = "" + playerLevel;
        SetupExp();

        //used for writing info to a file
        //WriteExpToFile();


        GetCurrentFill(currentEXP, 0f, expToNextLevel[playerLevel]);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            AddCoins(25, 1);
        }else if (Input.GetKeyDown(KeyCode.V))
        {
            AddCoins(250, 3);
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            AddCoins(1000, 10);
        }else if (Input.GetKeyDown(KeyCode.J))
        {
            AddExp(75);
        }
    }

    private void AddCoins(int coinsToAdd, int speed)
    {
        targetCoinsTotal = currentCoins + coinsToAdd;
        if(targetCoinsTotal > 999999999f)
        {
            targetCoinsTotal = 999999999f;
        }
        else
        {
            StartCoroutine(CountUpToTarget(speed));
        }
        
    }

    public void RemoveCoins(int coinsToRemove, int Speed)
    {
        targetCoinsTotal = currentCoins - coinsToRemove;
        if(targetCoinsTotal < coinsToRemove)
        {
            Debug.Log("cant do that you poor fool");
        }
        else
        {
            StartCoroutine(CountDownToTarget(Speed));
        }
    }

    IEnumerator CountUpToTarget(int speed)
    {
        while (currentCoins < targetCoinsTotal)
        {
            currentCoins += speed;
            currentCoins = Mathf.Clamp(currentCoins, 0f, targetCoinsTotal);
            if(currentCoins < 1000000)
            {
                coinsText.text = currentCoins + " coins";
            }
            else
            {
                coinsText.text = currentCoins.ToString();
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
        mask.fillAmount = fillAmount;

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
