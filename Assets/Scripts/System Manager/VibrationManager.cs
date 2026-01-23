using UnityEngine;

public class VibrationManager : MonoBehaviour
{
    public static VibrationManager Instance { get; private set; }

    private bool isVibrationEnabled = true;

    public bool IsVibrationEnabled => isVibrationEnabled;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadSettings()
    {
        SaveManager saveManager = new SaveManager();
        isVibrationEnabled = saveManager.LoadVibrationSetting();
    }

    public void Vibrate()
    {
        if (!isVibrationEnabled)
            return;

#if UNITY_ANDROID || UNITY_IOS
        if (Application.isMobilePlatform)
        {
            Handheld.Vibrate();
        }
#else
        // PC나 다른 플랫폼에서는 로그만 출력
        Debug.Log("VibrationManager: Vibrate called (PC - no vibration)");
#endif
    }

    public void VibrateShort()
    {
        // 짧은 진동 (약 50ms)
        Vibrate();
    }

    public void VibrateLong()
    {
        // 긴 진동 (약 400ms) - Unity의 Handheld.Vibrate는 고정 시간
        Vibrate();
    }

    public void SetVibrationEnabled(bool enabled)
    {
        isVibrationEnabled = enabled;

        // 설정 저장
        SaveManager saveManager = new SaveManager();
        saveManager.SaveVibrationSetting(isVibrationEnabled);

        // 진동을 켤 때 피드백
        if (enabled)
        {
            Vibrate();
        }
    }
}
