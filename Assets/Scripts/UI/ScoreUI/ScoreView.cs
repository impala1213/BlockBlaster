using TMPro;
using UnityEngine;

public sealed class ScoreView : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameController game;

    [Header("UI Texts")]
    [Tooltip("현재 점수 표시 텍스트")]
    [SerializeField] private TMP_Text scoreText;

    [Tooltip("역대 최고 점수(BEST) 표시 텍스트")]
    [SerializeField] private TMP_Text bestText;

    private void Awake()
    {
        if (scoreText == null)
            scoreText = GetComponentInChildren<TMP_Text>(true);
    }

    private void OnEnable()
    {
        if (game == null) return;

        game.OnScoreChanged += HandleScoreChanged;

        HandleScoreChanged(game.Score, game.BestScore, game.ComboStreak, 0);
    }

    private void OnDisable()
    {
        if (game == null) return;
        game.OnScoreChanged -= HandleScoreChanged;
    }

    private void HandleScoreChanged(int score, int best, int comboIndex, int delta)
    {
        if (bestText != null)
            bestText.text = $"{best}";
        if (scoreText != null)
            scoreText.text = $"{score}";
        
    }
}
