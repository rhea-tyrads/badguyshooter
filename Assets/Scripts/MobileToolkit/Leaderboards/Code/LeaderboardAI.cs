using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LeaderboardAI : MonoBehaviour
{
    public LeaderboardSO so;
    public LeagueRank rank => file.rank;
    public List<PlayerScore> scores => file.scores;
    public LeaderboardFile file => save.file;
    public LeaderboardSave save;
    public Leaderboard  board;
    public float firstPlaceScore=> board.firstPlaceScore;

    public  string[] playerNames = new string[]
{
    "Alex Taylor", "Jordan Brown", "Jordan Wilson", "Skyler Moore", "Avery Smith",
    "Quinn Davis", "Morgan Jones", "Taylor Williams", "Casey Miller", "Robin Wilson",
    "Jamie Smith", "Avery Johnson", "Skyler Brown", "Quinn Davis", "Morgan Miller",
    "Taylor Wilson", "Casey Moore", "Robin Taylor", "Jamie Johnson", "Avery Smith",
    "Alex Brown", "Jordan Davis", "Taylor Miller", "Casey Wilson", "Robin Moore",
    "Jamie Taylor", "Avery Johnson", "Skyler Brown", "Quinn Davis", "Morgan Miller",
    "Taylor Wilson", "Casey Moore", "Robin Taylor", "Jamie Johnson", "Avery Smith",
    "Alex Brown", "Jordan Davis", "Skyler Miller", "Quinn Wilson", "Morgan Moore",
    "Taylor Taylor", "Casey Johnson", "Robin Brown", "Jamie Davis", "Avery Miller",
    "Alex Wilson", "Jordan Moore", "Skyler Taylor", "Quinn Johnson", "Morgan Brown",
    "Taylor Davis", "Casey Miller", "Robin Wilson", "Jamie Moore", "Avery Taylor",
    "Alex Johnson", "Jordan Brown", "Skyler Davis", "Quinn Miller", "Morgan Wilson",
    "Taylor Moore", "Casey Taylor", "Robin Johnson", "Jamie Brown", "Avery Davis",
    "Alex Miller", "Jordan Wilson", "Skyler Moore", "Quinn Taylor", "Morgan Johnson",
    "Taylor Brown", "Casey Davis", "Robin Miller", "Jamie Wilson", "Avery Moore",
    "Alex Taylor", "Jordan Johnson", "Skyler Brown", "Quinn Davis", "Morgan Miller",
    "Taylor Wilson", "Casey Moore", "Robin Taylor", "Jamie Johnson", "Avery Smith",
    "Alex Brown", "Jordan Davis", "Skyler Miller", "Quinn Wilson", "Morgan Moore",
    "Taylor Taylor", "Casey Johnson", "Robin Brown", "Jamie Davis", "Avery Miller",
    "Alex Wilson", "Jordan Moore", "Skyler Taylor", "Quinn Johnson", "Morgan Brown"
};
    public void CreateAI(float playerScore)
    {
        System.Random rnd = new System.Random();
        var selectedNames = playerNames.OrderBy(x => rnd.Next()).Take(6).ToArray();

        for (int i = 0; i < 6; i++)
        {
            var value = so.aiTableCurve.Evaluate((float)i / 7);
            var score = value * firstPlaceScore;
            score += playerScore;
            var aiName = selectedNames[i];
            var earnPerMin = UnityEngine.Random.Range(0.02f, 0.15f) * score / 1440;
            AddScore(aiName, (int)score, earnPerMin);
        }
    }
    public void AddScore(string playerName, int score, float earnPerDay, bool isPlayer = false)
    {
        scores.Add(new PlayerScore(playerName, score, earnPerDay, isPlayer));
    }
}
