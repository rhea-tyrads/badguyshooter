using System;
using UnityEngine;

namespace MobileTools.Leaderboards.Code
{
    [Serializable]
    public class LeagueRank
    {
        public string leagueName;
        public int topScore;
        public int levelRequire;
        public int leagueNumber;
        public Sprite icon;
    }
}
