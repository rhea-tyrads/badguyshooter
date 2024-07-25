using System;
using MobileTools.SDK;
using MobileTools.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.LevelSystem;

public class SpecialOffer : MonoBehaviour
{
    public SpecialOfferUI ui;
    public Button showButton;

    TimeSpan offerDuration = new(3, 0, 0);
    DateTime offerEndTime;
    const string LastShowTimeKey = "LastShowTimeSpecialOffer";

    void Start()
    {
        Hide();
        SDKEvents.Instance.OnLevelComplete += LevelComplete;

        if (ShouldShowOffer())
        {
            ResetOffer();
            Show();
        }
        
        showButton.onClick.AddListener(Show);
        showButton.gameObject.SetActive(!Keys.IsSpecialOfferPurchased);
    }

    void LevelComplete(int arg1, int arg2)
    {
        var world = ActiveRoom.World;
        var level = ActiveRoom.Level + 1;

        if (level % 4 == 0)
            Show();
    }

   public void Show()
    {
        ui.Show();
    }

    void Hide()
    {
        ui.Hide();
    }

    void Update()
    {
        if (ShouldShowOffer())
        {
            UpdateTimer();
        }
    }

    bool ShouldShowOffer()
    {
        if (PlayerPrefs.HasKey("SpecialOfferFirstTime")) return true;
        PlayerPrefs.SetInt("SpecialOfferFirstTime", 1);
        return false;

        if (!PlayerPrefs.HasKey(LastShowTimeKey)) return true;
        var lastShowTime = DateTime.Parse(PlayerPrefs.GetString(LastShowTimeKey));
        return DateTime.Now.Date > lastShowTime.Date;
    }

    void ResetOffer()
    {
        offerEndTime = DateTime.Now.Add(offerDuration);
        PlayerPrefs.SetString(LastShowTimeKey, DateTime.Now.ToString());
        PlayerPrefs.Save();
    }

    void UpdateTimer()
    {
        var remainingTime = offerEndTime - DateTime.Now;
        ui.RefreshText(remainingTime);
    }
}