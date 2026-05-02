using UnityEngine;
using System.IO;
using System;

public static class SaveSystem
{
#if UNITY_WEBGL && !UNITY_EDITOR
    private const int MaxSlots = 4;
    private const string CountKey = "webgl_save_count";

    private static string JsonKey(int slot)      => $"webgl_save_json_{slot}";
    private static string TimeKey(int slot)      => $"webgl_save_time_{slot}";

    public static void SavePlayer()
    {
        PlayerData data = PlayerData.FromCurrentGameState();
        string json = JsonUtility.ToJson(data, prettyPrint: true);
        string now  = DateTime.Now.ToString("O");

        int count = PlayerPrefs.GetInt(CountKey, 0);

        int targetSlot;
        if (count < MaxSlots)
        {
            targetSlot = count;
            PlayerPrefs.SetInt(CountKey, count + 1);
        }
        else
        {
            // Overwrite the oldest slot
            targetSlot = 0;
            DateTime oldest = DateTime.MaxValue;
            for (int i = 0; i < MaxSlots; i++)
            {
                string ts = PlayerPrefs.GetString(TimeKey(i), "");
                if (ts != "" && DateTime.TryParse(ts, out DateTime t) && t < oldest)
                {
                    oldest = t;
                    targetSlot = i;
                }
            }
        }

        PlayerPrefs.SetString(JsonKey(targetSlot), json);
        PlayerPrefs.SetString(TimeKey(targetSlot), now);
        PlayerPrefs.Save(); // flushes to localStorage immediately
    }

    public static PlayerData LoadPlayer()
    {
        int count = PlayerPrefs.GetInt(CountKey, 0);
        if (count == 0)
            return null;

        int newestSlot = -1;
        DateTime newest = DateTime.MinValue;
        for (int i = 0; i < count; i++)
        {
            string ts = PlayerPrefs.GetString(TimeKey(i), "");
            if (ts != "" && DateTime.TryParse(ts, out DateTime t) && t > newest)
            {
                newest = t;
                newestSlot = i;
            }
        }

        if (newestSlot < 0)
            return null;

        string json = PlayerPrefs.GetString(JsonKey(newestSlot), "");
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            return JsonUtility.FromJson<PlayerData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to parse WebGL save: " + e.Message);
            return null;
        }
    }

    public static bool HasSave()
    {
        return PlayerPrefs.GetInt(CountKey, 0) > 0;
    }

    public static void DeleteSave()
    {
        int count = PlayerPrefs.GetInt(CountKey, 0);
        for (int i = 0; i < count; i++)
        {
            PlayerPrefs.DeleteKey(JsonKey(i));
            PlayerPrefs.DeleteKey(TimeKey(i));
        }
        PlayerPrefs.DeleteKey(CountKey);
        PlayerPrefs.Save();
    }

#else
    public static void SavePlayer()
    {
        string folderPath = Application.persistentDataPath + "/saves";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string path;
        string tempPath = "";
        var info = new DirectoryInfo(folderPath);
        FileInfo[] files = info.GetFiles("*.farm");
        DateTime temp2 = new DateTime(3000, 1, 1);

        if (files.Length < 1)
            path = folderPath + "/save1.farm";
        else if (files.Length == 1)
            path = folderPath + "/save2.farm";
        else if (files.Length == 2)
            path = folderPath + "/save3.farm";
        else if (files.Length == 3)
            path = folderPath + "/save4.farm";
        else
        {
            foreach (FileInfo file in files)
            {
                DateTime temp1 = File.GetLastWriteTime(file.FullName);
                if (temp1 < temp2)
                {
                    temp2 = temp1;
                    tempPath = file.FullName;
                }
            }
            path = tempPath;
        }

        PlayerData data = PlayerData.FromCurrentGameState();
        string json = JsonUtility.ToJson(data, prettyPrint: true);

        // Write to a temp file first, then move — prevents a half-written file
        // from becoming the "newest" slot if the game is killed mid-save
        string writePath = path + ".tmp";
        File.WriteAllText(writePath, json);
        if (File.Exists(path))
            File.Delete(path);
        File.Move(writePath, path);
    }

    public static PlayerData LoadPlayer()
    {
        string folderPath = Application.persistentDataPath + "/saves";

        if (!Directory.Exists(folderPath))
            return null;

        var info = new DirectoryInfo(folderPath);
        FileInfo[] files = info.GetFiles("*.farm");

        if (files.Length == 0)
            return null;

        DateTime temp2 = new DateTime(2000, 1, 1);
        string tempPath = "";

        foreach (FileInfo file in files)
        {
            DateTime temp1 = File.GetLastWriteTime(file.FullName);
            if (temp1 > temp2)
            {
                temp2 = temp1;
                tempPath = file.FullName;
            }
        }

        try
        {
            string json = File.ReadAllText(tempPath);
            return JsonUtility.FromJson<PlayerData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to read save file: " + e.Message);
            return null;
        }
    }

    public static bool HasSave()
    {
        string folderPath = Application.persistentDataPath + "/saves";
        if (!Directory.Exists(folderPath)) return false;
        return new DirectoryInfo(folderPath).GetFiles("*.farm").Length > 0;
    }

    public static void DeleteSave()
    {
        string folderPath = Application.persistentDataPath + "/saves";
        if (!Directory.Exists(folderPath)) return;
        foreach (FileInfo file in new DirectoryInfo(folderPath).GetFiles("*.farm"))
            file.Delete();
    }
#endif
}
