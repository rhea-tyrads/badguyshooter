
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DailyReward = FortuneWheels.Scripts.DailyReward;

public class DailyLogin : MonoBehaviour
{
    const string SaveKey = "DailyLoginSave";
    public DailyLoginFile file;
    public List<DailyReward> rewards = new();
    public DailyLoginUI ui;
    public Button openButton;
    [Space(20)]
    public bool test;
    public int testDays;
    bool FirstLogin => string.IsNullOrEmpty(file.weekStartDate);
    public bool IsAnyRewardToCollect => file.logins > file.collected;
    public event Action OnCollect = delegate { };
    bool IsWeekCollected => file.logins >= 7 && file.collected >= 7;

    int DaysPassed
    {
        get
        {
#if UNITY_EDITOR
            if (test) return testDays;
#endif
            string lastLogin = file.weekStartDate;
            var date = DateTime.Parse(lastLogin);
            var span = DateTime.UtcNow - date;
            return span.Days;
        }
    }

    void Awake()
    {
        Load();
        Check();

        ui.Hide();
        ui.Init(file.rewards);
        ui.OnShow += RefreshUI;
        ui.OnCollect += CollectReward;
        openButton.onClick.AddListener(Show);
    }

    void Show() => ui.Show();

    DailyReward GetReward(int id)
        => id < file.rewards.Count
        ? file.rewards[id]
        : file.rewards[^1];

    void CollectReward(int dayID)
    {
        var reward = GetReward(dayID);
        // Game.AddReward(reward);

        file.collected++;
        Save();

        RefreshUI();
        OnCollect();

        if (IsWeekCollected)
        {
            NewWeek();
            RefreshUI();
        }
    }


    void RefreshUI()
        => ui.SetState(file.logins, file.collected, file.week);

    void Check()
    {
        if (FirstLogin)
        {
            NewWeek();
            //  SetLoginYesterday();
            DailyLoginSuccess();

        }
        else
        {
            if (DaysPassed >= 1)
                DailyLoginSuccess();
        }
    }
    void SetLoginDate()
        => file.weekStartDate = DateTime.UtcNow.ToString();

    void SetLoginYesterday()
        => file.weekStartDate = DateTime.UtcNow.AddDays(-1).ToString();

    void NewWeek()
    {
        file.week++;
        file.logins = 0;
        file.collected = 0;
        file.rewards.Clear();
        foreach (var reward in rewards)
        {
            var rew = new DailyReward
            {
                type = reward.type,
                amount = reward.amount
            };
            file.rewards.Add(rew);
        }

        SetLoginDate();
        Save();
    }



    void DailyLoginSuccess()
    {
        if (file.logins >= 7) return;
        file.logins++;
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
      ? JsonUtility.FromJson<DailyLoginFile>(PlayerPrefs.GetString(SaveKey))
      : new DailyLoginFile();
}
