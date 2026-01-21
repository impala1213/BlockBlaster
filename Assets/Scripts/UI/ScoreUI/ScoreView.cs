using UnityEngine;
using UnityEngine.UI;

public sealed class ScoreView : MonoBehaviour
{
    [SerializeField] private GameController game;
    [SerializeField] private Text text;

    private void OnEnable()
    {
        game.OnScoreChanged += HandleScore;
    }

    private void OnDisable()
    {
        game.OnScoreChanged -= HandleScore;
    }

    private void HandleScore(int score, int best, int combo, int delta)
    {
        text.text = $"Score: {score}\nBest: {best}\nCombo: {combo}\n(+{delta})";
    }
}
