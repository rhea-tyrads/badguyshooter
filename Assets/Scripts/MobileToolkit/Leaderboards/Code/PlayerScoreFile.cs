[System.Serializable]
public class PlayerScoreFile
{
    public int initialScore;
    public int score;
    public int completeLeagues;

    public PlayerScoreFile(int initialScore, int score)
    {
        this.initialScore = initialScore;
        this.score = score;
    }
}
