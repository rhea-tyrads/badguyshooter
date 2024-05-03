using System;
 
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LeaderboardSO settings", menuName = "Config/LeaderboardSO  ")]
public class LeaderboardSO : ScriptableObject
{
    [Space(20)]
    public AnimationCurve aiTableCurve;
    [Space(20)]
    public SelectedDays refreshEvery;
    public bool ShouldRefreshDataToday
    {
        get
        {
            DayOfWeek today = DateTime.Now.DayOfWeek;
            switch (today)
            {
                case DayOfWeek.Monday: return refreshEvery.Monday;
                case DayOfWeek.Tuesday: return refreshEvery.Tuesday;
                case DayOfWeek.Wednesday: return refreshEvery.Wednesday;
                case DayOfWeek.Thursday: return refreshEvery.Thursday;
                case DayOfWeek.Friday: return refreshEvery.Friday;
                case DayOfWeek.Saturday: return refreshEvery.Saturday;
                case DayOfWeek.Sunday: return refreshEvery.Sunday;
                default: return false;
            }
        }
    }
    [Space(20)]
    public List<LeagueRank> ranks = new();

    [Space(20)]
    public List<int> goldPerStepInBoard = new();

    [Space(20)]
    public List<int> goldForWinning = new();

    public LeagueRank GetLeagueByComplete(int completed)
    {
        return completed < ranks.Count ? ranks[completed] : ranks[^1];
    }
    public Sprite GetLeagueIcon(string leagueName)=> ranks.Find(r=>r.leagueName.Equals(leagueName)).icon;
    public LeagueRank GetLeague(int lvl)
    {
        foreach (LeagueRank r in ranks)
            if(lvl>= r.levelRequire) return r; 
        return ranks[^1];
    }

    public TimeSpan GetTimeUntilNextSelectedDay => refreshEvery.GetTimeUntilNextSelectedDay();

    private void OnValidate()
    {
        int k = ranks.Count;  
        for (int i = 0; i < ranks.Count; i++)
        {
          
            ranks[i].leagueNumber = k;
            k--;
        }
     
        
    }
}
