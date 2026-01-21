using UnityEngine;

public sealed class SaveManager
{
    private const string BestScoreKey = "BB_BestScore";

    public int LoadBestScore()
    {
        return PlayerPrefs.GetInt(BestScoreKey, 0);
    }

    public void SaveBestScore(int bestScore)
    {
        PlayerPrefs.SetInt(BestScoreKey, bestScore);
        PlayerPrefs.Save();
    }
}
