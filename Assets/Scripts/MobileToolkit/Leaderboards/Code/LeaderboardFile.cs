using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class LeaderboardFile
{
    public string lastVisitString;

    private DateTime? _lastVisit;
    public LeagueRank rank;
    public int lastPlayerPlace=999;
    public int realPlayerPlace=10;
    public int collectedRewards;
    public int completeLeagues;
    public int endedLeagues;
    public int collectedLeagues;
    public int lastRefreshDay;

    public List<LeadearboardAchievements> achievements = new();
    public List<PlayerScore> scores = new();

    public DateTime lastVisit
    {
        get
        {
            // Lazy load the DateTime from the string
            if (!_lastVisit.HasValue && !string.IsNullOrEmpty(lastVisitString))
            {
                if (DateTime.TryParse(lastVisitString, out DateTime result))
                {
                    _lastVisit = result;
                }
            }
            return _lastVisit ?? DateTime.MinValue; // Return a default value if parsing fails or _lastVisit is null
        }
        set
        {
            _lastVisit = value;
            lastVisitString = value.ToString("o"); // Convert the DateTime to a string using a round-trip format
        }
    }
}

[Serializable]

public class LeadearboardAchievements
{
    public Sprite leagueIcon;
    public string leagueName;
    public LeadearBoardPlace place;
    public int goldReward;
    public int leagueNumber;
    public int placeNumber;
}

public enum LeadearBoardPlace
{
    Gold,
    Silver,
    Bronze,
    None
}
