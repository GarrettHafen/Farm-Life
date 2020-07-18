using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchMarket : MonoBehaviour
{
    public List<CropAsset> cropList = GameHandler.instance.cropsList;
    public List<CropAsset> filteredCropList;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SearchTheMarket(string input)
    {
        input = ToLower(input);
        string[] inputCharArray = StringToArray(input);
        for(int i = 0; i< cropList.Count; i++)
        {
            string[] cropNameArray = StringToArray(cropList[i].cropName);
            if (Compare(inputCharArray, inputCharArray.Length, cropNameArray))
            {
                filteredCropList.Add(cropList[i]);
            }
        }
    }

    private string ToLower(string input)
    {
        return input.ToLower();
    }

    private string[] StringToArray(string input)
    {
        string[] tempArray = input.Split();
        return tempArray;
    }

    private bool Compare(string[] inputCharArray, int inputCharArrayCount, string[] cropNameArray)
    {
        int counter = 0;
        for(int i =0; i < cropNameArray.Length; i++)
        {
            if(counter != inputCharArrayCount)
            {
                if(cropNameArray[i] == inputCharArray[counter])
                {
                    counter++;
                }
                else
                {
                    counter = 0;
                }
            }
            else
            {
                return true;
            }
        }
        return false;
    }
}
