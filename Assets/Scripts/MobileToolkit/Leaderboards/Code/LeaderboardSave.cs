using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardSave : MonoBehaviour
{
    public LeaderboardFile file;
    const string SaveKey = "LeaderboardSave";
    public void Save()
    {
        string jsonData = JsonUtility.ToJson(file);
        PlayerPrefs.SetString(SaveKey, jsonData);
        PlayerPrefs.Save();
    }


    public void Load()
    {
        file = PlayerPrefs.HasKey(SaveKey)
            ? JsonUtility.FromJson<LeaderboardFile>(PlayerPrefs.GetString(SaveKey))
            : new LeaderboardFile();
    }
}
