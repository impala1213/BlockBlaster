using UnityEngine;
using UnityEngine.UI;

public class GameSettingsModal : SettingsModal
{
    [Header("Game Specific Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button homeButton;

    [Header("References")]
    [SerializeField] private GameController gameController;

    protected override void Awake()
    {
        base.Awake();

        // Auto-find GameController if not assigned
        if (gameController == null)
            gameController = FindObjectOfType<GameController>();

        // Setup button listeners
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartButtonClicked);

        if (homeButton != null)
            homeButton.onClick.AddListener(OnHomeButtonClicked);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (restartButton != null)
            restartButton.onClick.RemoveListener(OnRestartButtonClicked);

        if (homeButton != null)
            homeButton.onClick.RemoveListener(OnHomeButtonClicked);
    }

    public void OnRestartButtonClicked()
    {
        // 확인 없이 바로 재시작 (필요시 확인 다이얼로그 추가 가능)
        RestartGame();
    }

    public void OnHomeButtonClicked()
    {
        // 확인 없이 바로 로비로 이동 (필요시 확인 다이얼로그 추가 가능)
        GoToLobby();
    }

    private void RestartGame()
    {
        if (SceneLoader.Instance != null)
        {
            // 현재 게임 모드를 유지하여 재시작
            GameMode currentMode = SceneLoader.Instance.CurrentGameMode;
            
            Hide();
            
            // 모달이 닫힌 후 씬 로드
            SceneLoader.Instance.LoadGameScene(currentMode);
        }
        else
        {
            Debug.LogError("GameSettingsModal: SceneLoader.Instance is null");
        }
    }

    private void GoToLobby()
    {
        if (SceneLoader.Instance != null)
        {
            Hide();
            
            // 모달이 닫힌 후 로비로 이동
            SceneLoader.Instance.LoadLobbyScene();
        }
        else
        {
            Debug.LogError("GameSettingsModal: SceneLoader.Instance is null");
        }
    }
}
