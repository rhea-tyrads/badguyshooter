using CoreUI;
using System;

using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class LeaderboardFinishUI : ScreenUI
{

    public TextMeshProUGUI leagueTitle;
    public Image leagueIcon;
    public TextMeshProUGUI rewardAmount;
    public TextMeshProUGUI yourPlace;
    public Button collectButton;
    public Sprite goldMedal;
    public Sprite silverMedal;
    public Sprite bronzeMedal;
    public Image medalIcon;
    public event Action OnCollect = delegate { };
    void Awake()
    {
        Hide();
        collectButton.onClick.AddListener(Collect);
    }
    public void SetPlace(int place)
    {
        yourPlace.text = "Your placed: " + "<color=green>" + place;
    }

    public void ShowMedal(LeadearBoardPlace place)
    {
        medalIcon.enabled = true;

        if (place == LeadearBoardPlace.Gold)
            medalIcon.sprite = goldMedal;
        else if (place == LeadearBoardPlace.Silver)
            medalIcon.sprite = silverMedal;
        else if (place == LeadearBoardPlace.Bronze)
            medalIcon.sprite = bronzeMedal;
        else HideMedal();

    }

    public void HideMedal()
    {
        medalIcon.enabled = false;
    }

    public void SetLeagueIcon(Sprite icon)
    {
        leagueIcon.sprite = icon;
    }

    public void SetLeagueTitle(string title, int id)
    {
        leagueTitle.text = $"League #{id}\n{title} ";
    }


    void Collect() => OnCollect();
}
