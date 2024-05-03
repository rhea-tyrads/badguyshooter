using System;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardHandler : MonoBehaviour
{
    public LeaderboardFile file => save.file;
    public LeaderboardSave save;
    public LeaderboardUI ui;
    public Leaderboard board;
    public LeaderboardSO so;
    public LeagueRank rank => file.rank;
    public List<PlayerScore> topScore => board.topScore;
    PlayerScore Score => topScore.Find(s => s.isPlayer);
    int Place => topScore.IndexOf(Score) + 1;
    int LastPlace => file.lastPlayerPlace;

    bool IsRewardComplete => Place < LastPlace;
    const int TotalPlaces = 7;
    bool RewardNotCollected => file.collectedRewards < (TotalPlaces - Place);
    public bool AnyRewardToCollect => IsRewardComplete && RewardNotCollected;


    public void UpdateScore(DateTime currentVisit)
    {
        var minSinceLastVisit = (currentVisit - file.lastVisit).TotalMinutes;
        if (minSinceLastVisit < 60) return;

        foreach (var item in file.scores)
        {
            var addScore = (int)(item.earnPerMin * minSinceLastVisit);
            var newScore = (int)(item.score + addScore);
            item.SetScore(newScore);
        }

        file.lastVisit = currentVisit;
        Save();
    }

    public void GetPlayerPlace()
    {
        ui.RewardNotComplete();
        file.realPlayerPlace = Place;

        if (AnyRewardToCollect)
            ui.RewardComplete();
        else
            file.lastPlayerPlace = Place;

        Save();
        RefreshUI();
    }

    public void RefreshPlayerExp(float exp)
    {
        var p = file.scores.Find(s => s.isPlayer);
        p.SetScore((int)exp);
        board.topScore = board.GetTopScores(7);
        GetPlayerPlace();
        RefreshUI();
        Save();
    }

    public void RefreshUI()
    {
        topScore.Find(s => s.isPlayer).score=board.player.Score;
        var place = file.lastPlayerPlace;
        var totalPlaces = 7;
        var next = totalPlaces - place;
        //  Debug.LogError("next: " + next + ", goldPerStepInBoard: " + so.goldPerStepInBoard.Count);
        next = Mathf.Clamp(next, 0, so.goldPerStepInBoard.Count - 1);
        var reward = so.goldPerStepInBoard[next];

        ui.SetReward(totalPlaces - file.collectedRewards - 1, reward);
        ui.DisplayScores(topScore);
        ui.SetTimeRemain(so.refreshEvery.GetTimeUntilNextSelectedDay());
        ui.SetLeagueTitle(rank.leagueName, rank.leagueNumber);
        ui.SetLeagueIcon(rank.icon);
    }

    public void Save() => save.Save();

}
