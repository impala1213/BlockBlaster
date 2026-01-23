using UnityEngine;

public sealed class SaveManager
{
    private const string BestScoreKey = "BB_BestScore";
    private const string BGMEnabledKey = "BB_BGMEnabled";
    private const string SFXEnabledKey = "BB_SFXEnabled";
    private const string VibrationEnabledKey = "BB_VibrationEnabled";

    public int LoadBestScore()
    {
        return PlayerPrefs.GetInt(BestScoreKey, 0);
    }

    public void SaveBestScore(int bestScore)
    {
        PlayerPrefs.SetInt(BestScoreKey, bestScore);
        PlayerPrefs.Save();
    }

    public void SaveAudioSettings(bool bgm, bool sfx)
    {
        PlayerPrefs.SetInt(BGMEnabledKey, bgm ? 1 : 0);
        PlayerPrefs.SetInt(SFXEnabledKey, sfx ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void LoadAudioSettings(out bool bgm, out bool sfx)
    {
        bgm = PlayerPrefs.GetInt(BGMEnabledKey, 1) == 1;
        sfx = PlayerPrefs.GetInt(SFXEnabledKey, 1) == 1;
    }

    public void SaveVibrationSetting(bool enabled)
    {
        PlayerPrefs.SetInt(VibrationEnabledKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public bool LoadVibrationSetting()
    {
        return PlayerPrefs.GetInt(VibrationEnabledKey, 1) == 1;
    }
}
