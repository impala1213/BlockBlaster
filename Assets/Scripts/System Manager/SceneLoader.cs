using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public enum GameMode
{
    Classic,
    Adventure
}

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("Scene Names")]

    [SerializeField] private string lobbySceneName = "Lobby";
    [SerializeField] private string gameSceneName = "Game";

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private Color fadeColor = Color.black;

    [Header("Loading Settings")]
    [SerializeField] private float minLoadingTime = 0.5f;

    private bool isLoading = false;
    private Canvas fadeCanvas;
    private Image fadeImage;
    private CanvasGroup fadeCanvasGroup;
    private GameMode currentGameMode = GameMode.Classic;

    public bool IsLoading => isLoading;
    public GameMode CurrentGameMode => currentGameMode;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreateFadeUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void CreateFadeUI()
    {
        // Create Canvas
        GameObject canvasGO = new GameObject("FadeCanvas");
        DontDestroyOnLoad(canvasGO);
        canvasGO.transform.SetParent(transform);

        fadeCanvas = canvasGO.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 9999;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();

        // Create CanvasGroup
        fadeCanvasGroup = canvasGO.AddComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
        fadeCanvasGroup.interactable = false;

        // Create Image
        GameObject imageGO = new GameObject("FadeImage");
        imageGO.transform.SetParent(canvasGO.transform, false);

        fadeImage = imageGO.AddComponent<Image>();
        fadeImage.color = fadeColor;
        fadeImage.raycastTarget = false;

        RectTransform rect = imageGO.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        fadeCanvasGroup.blocksRaycasts = true;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
    }

    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        isLoading = true;

        // Fade Out
        yield return FadeOut();

        // Load Scene
        float startTime = Time.time;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            // Ensure minimum loading time
            if (asyncLoad.progress >= 0.9f)
            {
                float elapsed = Time.time - startTime;
                if (elapsed < minLoadingTime)
                {
                    yield return null;
                    continue;
                }
            }
            yield return null;
        }

        // Fade In
        yield return FadeIn();

        isLoading = false;
    }

    public void LoadLobbyScene()
    {
        if (!isLoading)
        {
            StartCoroutine(LoadSceneWithFade(lobbySceneName));
        }
    }

    public void LoadGameScene(GameMode mode)
    {
        if (!isLoading)
        {
            currentGameMode = mode;
            StartCoroutine(LoadSceneWithFade(gameSceneName));
        }
    }

    public void LoadClassicMode()
    {
        LoadGameScene(GameMode.Classic);
    }

    public void LoadAdventureMode()
    {
        LoadGameScene(GameMode.Adventure);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
