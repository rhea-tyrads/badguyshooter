

using UnityEngine;

[System.Serializable]
public class PlayerScore
{
    public bool isPlayer;
    public string playerName;
    public int score;
    public float earnPerMin;
    public void SetScore(int a)
    {
        score = a;
    }

    public PlayerScore(string name, int score, float earnPerMin = 1, bool isPlayer = false)
    {
        this.playerName = name;
        this.score = score;
        this.earnPerMin = earnPerMin;
        this.isPlayer = isPlayer;
    }
}
