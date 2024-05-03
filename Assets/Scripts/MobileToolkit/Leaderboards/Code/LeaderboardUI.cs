using CoreUI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class LeaderboardUI : MobilePopupUI
{
    public Button openButton;
    public PlayerScoreUI playerScorePrefab;
    public List<PlayerScoreUI> panels = new();
    public Transform scoresContainer;
    public Sprite goldMedal;
    public Sprite silverMedal;
    public Sprite bronzeMedal;
    public TextMeshProUGUI timerRemain;
    public TextMeshProUGUI leagueTitle;
    public Image leagueIcon;
    public TextMeshProUGUI rewardAmount;
    public TextMeshProUGUI rewardGoal;
    public Button collectButton;
    public TextMeshProUGUI notAvailableText;
    public TextMeshProUGUI currentGoal;

    public LeaderboardFinishUI finishUI;
    public event Action OnCollectFinal = delegate { };
    public event Action OnCollect = delegate { };
    public GameObject collectVfx;

    void Awake()
    {
        openButton.onClick.AddListener(Show);
    }

    public void ShowFinalReward(LeadearboardAchievements l)
    {
        finishUI.SetLeagueTitle(l.leagueName, l.leagueNumber);
        finishUI.SetLeagueIcon(l.leagueIcon);
        finishUI.ShowMedal(l.place);
        finishUI.SetPlace(l.placeNumber);
        finishUI.Show();

        finishUI.OnCollect += CollectFinal;
    }

    void CollectFinal()
    {
        finishUI.Hide();
        OnCollectFinal();
    }

    public void RewardNotComplete()
    {
        currentGoal.text = "Current goal:";
        currentGoal.color = Color.white;
        notAvailableText.gameObject.SetActive(true);
        collectButton.gameObject.SetActive(false);
        collectButton.onClick.RemoveListener(CollectReward);
    }

    public void RewardComplete()
    {
        currentGoal.text = "Goal complete";
        currentGoal.color = Color.green;
        notAvailableText.gameObject.SetActive(false);
        collectButton.gameObject.SetActive(true);
        collectButton.onClick.AddListener(CollectReward);
    }

    void CollectReward()
    {
        collectVfx.SetActive(true);
        OnCollect();
    }
    public GameObject goalReward;
    public void SetReward(int forPosition, int amount)
    {
        if (forPosition <= 0)
        {
            goalReward.SetActive(false);
            return;
        }

        goalReward.SetActive(true);
        rewardGoal.text = "Reach position #" + forPosition;
        rewardAmount.text = amount.ToString();
    }

    public void SetLeagueIcon(Sprite icon)
    {
        leagueIcon.sprite = icon;
    }

    public void SetLeagueTitle(string title, int id)
    {
        leagueTitle.text = $"League #{id}\n{title} ";
    }

    public void SetTimeRemain(TimeSpan timeUntilNext)
    {
        timerRemain.text =
            $" {timeUntilNext.Days}d " +
            $" {timeUntilNext.Hours}h";
    }


    public void DisplayScores(List<PlayerScore> topScores)
    {
        for (int i = 0; i < topScores.Count; i++)
        {
            PlayerScore playerScore = topScores[i];
            var ui = panels[i];
            // var ui = Instantiate(playerScorePrefab, scoresContainer);

            ui.SetDetails(playerScore.playerName, i + 1, playerScore.score);

            if (playerScore.isPlayer)
                ui.Highlight();
            else ui.Normal();
        }

        panels[0].ShowMedal(goldMedal);
        panels[1].ShowMedal(silverMedal);
        panels[2].ShowMedal(bronzeMedal);
    }
}

