using Firebase.Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities.Extensions;

public class FirebaseDailyPlaytime : MonoBehaviour
{
    const string SaveKey = "DailyPlaytimeSave";
    public DailyPlaytimeFile file;
    float timer;
    public int refershTime = 5;
    void Start()
    {
        Load();
        SendEvents();
    }


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
            if (DaysPassed(item.date) < 1) continue;
            if (item.eventHasBeenSend) continue;
            item.eventHasBeenSend = true;
            Sent(item.totalSecondsPlayed);
        }

        Save();
    }

    void Sent(int sec)
    {
        FirebaseAnalytics.LogEvent(Events.DAILY_PLAYTIME, Params.SECONDS, sec);
    }

    int DaysPassed(string from)
    {
        var date = DateTime.Parse(from);
        var span = DateTime.UtcNow - date;
        return span.Days;
    }

    DailyPlaytime Get(string date) => file.Get(date);

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
      ? JsonUtility.FromJson<DailyPlaytimeFile>(PlayerPrefs.GetString(SaveKey))
      : new DailyPlaytimeFile();
}


[Serializable]
public class DailyPlaytimeFile
{
    public List<DailyPlaytime> days = new();

    public DailyPlaytime Get(string date)
        => days.Any(d => d.date.Equals(date))
        ? days.Find(d => d.date.Equals(date))
        : new DailyPlaytime() { date = date }.Please(days.Add);
}


[Serializable]
public class DailyPlaytime
{
    public string date;
    public int totalSecondsPlayed;
    public bool eventHasBeenSend;


}