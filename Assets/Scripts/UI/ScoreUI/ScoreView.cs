using TMPro;
using UnityEngine;

public sealed class ScoreView : MonoBehaviour
{
    [SerializeField] private GameController game;
    [SerializeField] private TMP_Text text;

    private void Awake()
    {
        if (text == null) text = GetComponentInChildren<TMP_Text>(true);
    }

    private void OnEnable()
    {
        if (game != null)
            game.OnScoreChanged += HandleScore;
    }

    private void OnDisable()
    {
        if (game != null)
            game.OnScoreChanged -= HandleScore;
    }

    private void HandleScore(int score, int best, int combo, int delta)
    {
        if (text == null) return;
        text.text = $"Score: {score}\nBest: {best}\nCombo: {combo}\n(+{delta})";
    }
}
