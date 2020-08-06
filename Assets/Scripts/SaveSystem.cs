using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

public static class SaveSystem 
{
    public static void SavePlayer()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        if (!Directory.Exists(Application.persistentDataPath + "/saves"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/saves");
        }
        string folderPath = Application.persistentDataPath + "/saves";
        string path;
        string tempPath = "";
        var info = new DirectoryInfo(folderPath);
        FileInfo[] files = info.GetFiles();
        DateTime temp2 = new DateTime(3000, 1, 1);
        // create rotating save system
        if(files.Length < 1)
        {
            path = Application.persistentDataPath + "/saves/save1.farm";
        }else if(files.Length == 1){
            path = Application.persistentDataPath + "/saves/save2.farm";
        }
        else if (files.Length == 2)
        {
            path = Application.persistentDataPath + "/saves/save3.farm";
        }
        else if (files.Length == 3)
        {
            path = Application.persistentDataPath + "/saves/save4.farm";
        }
        else
        {
            foreach (System.IO.FileInfo thingy in files)
            {
                DateTime temp1 = System.IO.File.GetLastWriteTime(thingy.ToString());
                if (temp1 < temp2)
                {
                    temp2 = temp1;
                    tempPath = thingy.ToString();
                }
            }
            path = tempPath.ToString();
        }
        //path = C:/Users/garre/AppData/LocalLow/DefaultCompany/FarmLife/player.farm
        
        FileStream stream = new FileStream(path, FileMode.Create);

        PlayerData data = new PlayerData();

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static PlayerData LoadPlayer()
    {
        
        string folderPath = Application.persistentDataPath + "/saves";
        string path;
        DateTime temp2 = new DateTime(2000, 1, 1);
        string tempPath = "";
        var info = new DirectoryInfo(folderPath);
        FileInfo[] files = info.GetFiles();
        //get most recent save
        foreach (System.IO.FileInfo thingy in files)
        {
            DateTime temp1 = System.IO.File.GetLastWriteTime(thingy.ToString());
            if (temp1 > temp2)
            {
                temp2 = temp1;
                tempPath = thingy.ToString();
            }
        }
        path = tempPath.ToString();
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            PlayerData data = formatter.Deserialize(stream) as PlayerData;
            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }
}
