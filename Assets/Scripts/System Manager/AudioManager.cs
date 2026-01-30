using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public enum SfxType
    {
        Click = 0,
        Success = 1,
        Fail = 2,
        GameEnd = 3
    }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Volume Settings")]
    [SerializeField][Range(0f, 1f)] private float bgmVolume = 0.7f;
    [SerializeField][Range(0f, 1f)] private float sfxVolume = 1f;

    [Header("Default Clips")]
    [SerializeField] private AudioClip defaultBgm;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip clickSfx;
    [SerializeField] private AudioClip successSfx;
    [SerializeField] private AudioClip failSfx;
    [SerializeField] private AudioClip gameEndSfx;

    private bool isBGMEnabled = true;
    private bool isSFXEnabled = true;

    public bool IsBGMEnabled => isBGMEnabled;
    public bool IsSFXEnabled => isSFXEnabled;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
            LoadSettings();
            if (defaultBgm != null)
            {
                PlayBGM(defaultBgm);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioSources()
    {
        // BGM Source
        if (bgmSource == null)
        {
            GameObject bgmGO = new GameObject("BGM_Source");
            bgmGO.transform.SetParent(transform);
            bgmSource = bgmGO.AddComponent<AudioSource>();
        }

        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = bgmVolume;

        // SFX Source
        if (sfxSource == null)
        {
            GameObject sfxGO = new GameObject("SFX_Source");
            sfxGO.transform.SetParent(transform);
            sfxSource = sfxGO.AddComponent<AudioSource>();
        }

        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;
    }

    private void LoadSettings()
    {
        SaveManager saveManager = new SaveManager();
        saveManager.LoadAudioSettings(out isBGMEnabled, out isSFXEnabled);

        UpdateBGMState();
        UpdateSFXState();
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioManager: BGM clip is null");
            return;
        }

        if (bgmSource.clip == clip && bgmSource.isPlaying)
            return;

        bgmSource.clip = clip;

        if (isBGMEnabled)
        {
            bgmSource.Play();
        }
    }

    public void StopBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioManager: SFX clip is null");
            return;
        }

        if (isSFXEnabled && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    public void PlaySFX(SfxType type)
    {
        AudioClip clip = GetSfxClip(type);
        if (clip == null)
        {
            Debug.LogWarning($"AudioManager: SFX clip is missing for {type}");
            return;
        }

        PlaySFX(clip);
    }

    public void SetBGMEnabled(bool enabled)
    {
        isBGMEnabled = enabled;
        UpdateBGMState();

        SaveManager saveManager = new SaveManager();
        saveManager.SaveAudioSettings(isBGMEnabled, isSFXEnabled);
    }

    public void SetSFXEnabled(bool enabled)
    {
        isSFXEnabled = enabled;
        UpdateSFXState();

        SaveManager saveManager = new SaveManager();
        saveManager.SaveAudioSettings(isBGMEnabled, isSFXEnabled);
    }

    private void UpdateBGMState()
    {
        if (bgmSource != null)
        {
            if (isBGMEnabled)
            {
                if (!bgmSource.isPlaying && bgmSource.clip != null)
                {
                    bgmSource.Play();
                }
            }
            else
            {
                if (bgmSource.isPlaying)
                {
                    bgmSource.Pause();
                }
            }
        }
    }

    private void UpdateSFXState()
    {
        // SFX는 즉시 반영됨 (PlayOneShot에서 isSFXEnabled 체크)
    }

    private AudioClip GetSfxClip(SfxType type)
    {
        switch (type)
        {
            case SfxType.Click:
                return clickSfx;
            case SfxType.Success:
                return successSfx;
            case SfxType.Fail:
                return failSfx;
            case SfxType.GameEnd:
                return gameEndSfx;
            default:
                return null;
        }
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }
}
