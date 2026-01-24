using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SettingsModal : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] protected CanvasGroup canvasGroup;
    [SerializeField] protected RectTransform modalPanel;
    [SerializeField] protected Toggle bgmToggle;
    [SerializeField] protected Toggle sfxToggle;
    [SerializeField] protected Toggle vibrationToggle;
    [SerializeField] protected Button closeButton;

    [Header("Animation Settings")]
    [SerializeField] protected float showDuration = 0.2f;
    [SerializeField] protected float hideDuration = 0.15f;
    [SerializeField] protected float startScale = 0.8f;

    protected bool isAnimating = false;

    protected virtual void Awake()
    {
        // Auto-find components if not assigned
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (modalPanel == null)
            modalPanel = transform.Find("ModalPanel") as RectTransform;

        // Setup button listeners
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseButtonClicked);

        // Setup toggle listeners
        if (bgmToggle != null)
            bgmToggle.onValueChanged.AddListener(OnBGMToggleChanged);

        if (sfxToggle != null)
            sfxToggle.onValueChanged.AddListener(OnSFXToggleChanged);

        if (vibrationToggle != null)
            vibrationToggle.onValueChanged.AddListener(OnVibrationToggleChanged);

        // Initialize hidden
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        if (modalPanel != null)
        {
            modalPanel.localScale = Vector3.one * startScale;
        }
    }

    protected virtual void Start()
    {
        LoadSettings();
    }

    protected virtual void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(OnCloseButtonClicked);

        if (bgmToggle != null)
            bgmToggle.onValueChanged.RemoveListener(OnBGMToggleChanged);

        if (sfxToggle != null)
            sfxToggle.onValueChanged.RemoveListener(OnSFXToggleChanged);

        if (vibrationToggle != null)
            vibrationToggle.onValueChanged.RemoveListener(OnVibrationToggleChanged);
    }

    public virtual void Show()
    {
        if (isAnimating)
            return;

        gameObject.SetActive(true);
        LoadSettings();
        StartCoroutine(ShowAnimation());
    }

    public virtual void Hide()
    {
        if (isAnimating)
            return;

        StartCoroutine(HideAnimation());
    }

    protected virtual IEnumerator ShowAnimation()
    {
        isAnimating = true;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        float elapsed = 0f;

        while (elapsed < showDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / showDuration;

            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            if (modalPanel != null)
                modalPanel.localScale = Vector3.one * Mathf.Lerp(startScale, 1f, t);

            yield return null;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        if (modalPanel != null)
            modalPanel.localScale = Vector3.one;

        isAnimating = false;
    }

    protected virtual IEnumerator HideAnimation()
    {
        isAnimating = true;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        float elapsed = 0f;

        while (elapsed < hideDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / hideDuration;

            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            if (modalPanel != null)
                modalPanel.localScale = Vector3.one * Mathf.Lerp(1f, startScale, t);

            yield return null;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        if (modalPanel != null)
            modalPanel.localScale = Vector3.one * startScale;

        isAnimating = false;

        // 재사용을 위해 삭제 대신 비활성화
        gameObject.SetActive(false);
    }

    protected virtual void LoadSettings()
    {
        // Temporarily remove listeners to prevent triggering events
        if (bgmToggle != null)
            bgmToggle.onValueChanged.RemoveListener(OnBGMToggleChanged);
        if (sfxToggle != null)
            sfxToggle.onValueChanged.RemoveListener(OnSFXToggleChanged);
        if (vibrationToggle != null)
            vibrationToggle.onValueChanged.RemoveListener(OnVibrationToggleChanged);

        // Load settings
        if (AudioManager.Instance != null)
        {
            if (bgmToggle != null)
                bgmToggle.isOn = AudioManager.Instance.IsBGMEnabled;
            if (sfxToggle != null)
                sfxToggle.isOn = AudioManager.Instance.IsSFXEnabled;
        }

        if (VibrationManager.Instance != null)
        {
            if (vibrationToggle != null)
                vibrationToggle.isOn = VibrationManager.Instance.IsVibrationEnabled;
        }

        // Re-add listeners
        if (bgmToggle != null)
            bgmToggle.onValueChanged.AddListener(OnBGMToggleChanged);
        if (sfxToggle != null)
            sfxToggle.onValueChanged.AddListener(OnSFXToggleChanged);
        if (vibrationToggle != null)
            vibrationToggle.onValueChanged.AddListener(OnVibrationToggleChanged);
    }

    protected virtual void OnBGMToggleChanged(bool value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMEnabled(value);
        }
        else
        {
            Debug.LogWarning("SettingsModal: AudioManager.Instance is null");
        }
    }

    protected virtual void OnSFXToggleChanged(bool value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXEnabled(value);
        }
        else
        {
            Debug.LogWarning("SettingsModal: AudioManager.Instance is null");
        }
    }

    protected virtual void OnVibrationToggleChanged(bool value)
    {
        if (VibrationManager.Instance != null)
        {
            VibrationManager.Instance.SetVibrationEnabled(value);
        }
        else
        {
            Debug.LogWarning("SettingsModal: VibrationManager.Instance is null");
        }
    }

    protected virtual void OnCloseButtonClicked()
    {
        Hide();
    }
}
