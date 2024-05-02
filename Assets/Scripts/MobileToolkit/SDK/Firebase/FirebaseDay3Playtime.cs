using Firebase.Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities.Extensions;


public class FirebaseDay3Playtime : MonoBehaviour
{
    const string SaveKey = "Day3PlaytimeSave";
    public Day3PlaytimeFile file;
    float timer;

    void Start()
    {
        Load();
        SendEvents();
    }
    public int refershTime = 5;

    void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;
        if (timer > refershTime)
        {
            timer -= refershTime;
            Record(DateTime.UtcNow.ToString(), refershTime);
        }
    }

    void SendEvents()
    {
        foreach (var item in file.days)
        {
            if (DaysPassed(item.date) < 3) continue;
            if (item.eventHasBeenSend) continue;
            item.eventHasBeenSend = true;
            Sent(item.totalSecondsPlayed);
        }

        Save();
    }

    void Sent(int sec)
    {
        FirebaseAnalytics.LogEvent(Events.DAY_3_PLAYTIME, Params.SECONDS, sec);
    }

    int DaysPassed(string from)
    {
        var date = DateTime.Parse(from);
        var span = DateTime.UtcNow - date;
        return span.Days;
    }

    Day3Playtime Get(string date) => file.Get(date);

    public void Record(string date, int seconds)
    {
        var d = Get(date);
        d.totalSecondsPlayed += seconds;
        Save();
    }

    void Save()
    {
        string jsonData = JsonUtility.ToJson(file);
        PlayerPrefs.SetString(SaveKey, jsonData);
        PlayerPrefs.Save();
    }

    void Load()
      => file = PlayerPrefs.HasKey(SaveKey)
      ? JsonUtility.FromJson<Day3PlaytimeFile>(PlayerPrefs.GetString(SaveKey))
      : new Day3PlaytimeFile();
}



[Serializable]
public class Day3PlaytimeFile
{
    public List<Day3Playtime> days = new();

    public Day3Playtime Get(string date)
        => days.Any(d => d.date.Equals(date))
        ? days.Find(d => d.date.Equals(date))
        : new Day3Playtime() { date = date }.Please(days.Add);
}


[Serializable]
public class Day3Playtime
{
    public string date;
    public int totalSecondsPlayed;
    public bool eventHasBeenSend;


}