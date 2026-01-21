using UnityEngine;

public sealed class ScoreManager
{
    public int Score { get; private set; }
    public int BestScore { get; private set; }
    public int ComboStreak { get; private set; }

    private readonly GameSettings settings;
    private readonly SaveManager save;

    public ScoreManager(GameSettings settings, SaveManager save)
    {
        this.settings = settings;
        this.save = save;

        BestScore = save.LoadBestScore();
        Reset();
    }

    public void Reset()
    {
        Score = 0;
        ComboStreak = 0;
    }

    public int ApplyTurnScore(int placedBlockCount, int linesClearedCount)
    {
        int placeScore = placedBlockCount * settings.scorePerBlockPlaced;

        int lineScore = 0;
        if (linesClearedCount > 0)
        {
            ComboStreak++;
            float mult = 1f + ComboStreak * settings.comboMultiplierStep;
            lineScore = Mathf.RoundToInt(linesClearedCount * settings.scorePerLineCleared * mult);
        }
        else
        {
            ComboStreak = 0;
        }

        int delta = placeScore + lineScore;
        Score += delta;

        if (Score > BestScore)
        {
            BestScore = Score;
            save.SaveBestScore(BestScore);
        }

        return delta;
    }
}
