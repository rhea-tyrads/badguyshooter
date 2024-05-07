using System;
using System.Collections.Generic;
using System.Linq;

namespace MobileTools.Leaderboards.Code
{
    public class Leaderboard : MobileTool
    {
        public LeaderboardSO so;
        public LeaderboardPlayer player;
        public LeaderboardUI ui;
        public LeaderboardSave save;
        public LeaderboardAI ai;
        public LeaderboardHandler handler;
        public List<PlayerScore> topScore = new();
        public LeaderboardFile file => save.file;
        public LeagueRank rank => file.rank;
        public List<PlayerScore> scores => file.scores;

        public bool ShouldRefreshDataToday => testShouldRefreshToday ? true : so.ShouldRefreshDataToday && NotRefreshedYet;
        public bool testShouldRefreshToday;
        public float firstPlaceScore { get; set; }
        public bool AnyRewardToCollect => handler.AnyRewardToCollect;
        bool NotRefreshedYet => DateTime.Now.Day != file.lastRefreshDay;
        public event Action OnChange = delegate { };


        void Start()
        {
            ui.OnCollect += CollectPlaceReward;
            player.OnChange += RefreshPlayerExp;
            Load();

            ui.Hide();
            ui.OnShow += ShowUI;

            if (file.scores.Count == 0)
            {
                GenerateNewBoard();
            }
            else
            {
                if (ShouldRefreshDataToday)
                {
                    GiveFinalRewards();
                    GenerateNewBoard();
                }

                UpdateScore(DateTime.Now);
            }

            topScore = GetTopScores(7);
            GetPlayerPlace();
        }
        void ShowUI()
        {
            if (file.endedLeagues > file.collectedLeagues)
            {
                ShowFinalReward();
            }
            RefreshUI();
        }

        void RefreshPlayerExp(float exp)
        {
            handler.RefreshPlayerExp(exp);
            OnChange();
        }

        void GiveFinalRewards()
        {
            var achievement = new LeadearboardAchievements
            {
                leagueName = file.rank.leagueName,
                leagueIcon = file.rank.icon,
                place = LeadearBoardPlace.None,
                placeNumber = file.realPlayerPlace
            };

            var gold = 0;

            if (achievement.placeNumber == 3)
            {
                gold = so.goldForWinning[^3];
                achievement.place = LeadearBoardPlace.Bronze;
                file.completeLeagues++;
            }

            if (achievement.placeNumber == 2)
            {
                gold = so.goldForWinning[^2];
                achievement.place = LeadearBoardPlace.Silver;
                file.completeLeagues++;
            }

            if (achievement.placeNumber == 1)
            {
                gold = so.goldForWinning[^1];
                achievement.place = LeadearBoardPlace.Gold;
                file.completeLeagues++;
            }

            file.endedLeagues++;
            achievement.goldReward = gold;
            achievement.leagueNumber = file.rank.leagueNumber;
            file.achievements.Add(achievement);
            Save();
        }

        public void UpdateScore(DateTime currentVisit)
            => handler.UpdateScore(currentVisit);

        void RefreshUI() => handler.RefreshUI();

        void GetPlayerPlace() => handler.GetPlayerPlace();

        void ShowFinalReward()
        {
            var l = file.achievements[file.collectedLeagues];
            ui.ShowFinalReward(l);
            ui.OnCollectFinal += CollectFinalReward;
        }

        void CollectFinalReward()
        {
            var achievement = file.achievements[file.collectedLeagues];
            // Bank.Instance.Add(CoreGame.ResourceEnum.Gold, achievement.goldReward);
            file.collectedLeagues++;
            Save();
        }

        void GenerateNewBoard()
        {
            file.lastVisit = DateTime.Now;
            file.lastRefreshDay = DateTime.Now.Day;
            file.lastPlayerPlace = 7;
            file.scores = new();
            file.collectedRewards = 0;

            CreateLeague();
            CreatePlayer();
            CreateAI();
            Save();
        }

        void CollectPlaceReward()
        {
            var gold = so.goldPerStepInBoard[file.collectedRewards];
            //  Bank.Instance.Add(CoreGame.ResourceEnum.Gold, gold);

            file.collectedRewards++;
            Save();
            Invoke(nameof(GetPlayerPlace), 0.1f);
            OnChange();
        }

        void CreateLeague()
        {
            var complete = file.completeLeagues;
            file.rank = so.GetLeagueByComplete(complete);
            firstPlaceScore = rank.topScore;
        }

        void CreateAI() => ai.CreateAI(player.file.score);

        void CreatePlayer()
        {
            var playerScore = player.Score;
            var playerName = "You ";
            scores.Add(new PlayerScore(playerName, playerScore, 0, true));
        }

        public List<PlayerScore> GetTopScores(int topN)
        {
            var sortedScores = scores.OrderByDescending(s => s.score).ToList();
            return sortedScores.Take(topN).ToList();
        }

        public void Save() => save.Save();
        public void Load() => save.Load();


    }
}
