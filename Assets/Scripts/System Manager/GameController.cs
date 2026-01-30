using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class GameController : MonoBehaviour
{
    [Header("Scoring (Block Blast-like)")]
    [Tooltip("Points per block placed. Keep 1 if you want the classic 'slow but steady' gain.")]
    [SerializeField] private int scorePerBlockPlaced = 1;

    [Tooltip("Base unit for line clear scoring. Classic baseline is 10.")]
    [SerializeField] private int baseLineUnitScore = 10;

    private const string BestScoreKey = "BB_BestScore";

    public Board Board { get; private set; }

    public int Score { get; private set; }
    public int BestScore { get; private set; }

    /// <summary>
    /// Combo index N (0-based):
    /// - First clear after a miss => N = 0
    /// - Next consecutive clear => N = 1
    /// - Next => N = 2 ...
    /// </summary>
    public int ComboStreak { get; private set; }

    private bool lastTurnHadClear = false;

    public struct TurnResult
    {
        public bool success;
        public PieceDefinition piece;
        public Vector2Int origin;

        public List<Vector2Int> placedCells;
        public List<Vector2Int> clearedCells;

        public int linesCleared;
        public int scoreDelta;
    }

    public event Action OnBoardReset;
    public event Action<PieceDefinition, List<Vector2Int>> OnCellsPlaced;
    public event Action<List<Vector2Int>> OnCellsCleared;

    // score, best, comboIndex(N), delta
    public event Action<int, int, int, int> OnScoreChanged;

    private void Awake()
    {
        Board = new Board();
        BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        ResetGame();
    }

    public void ResetGame()
    {
        Board.ClearAll();
        Score = 0;
        ComboStreak = 0;
        lastTurnHadClear = false;

        OnBoardReset?.Invoke();
        OnScoreChanged?.Invoke(Score, BestScore, ComboStreak, 0);
    }

    public bool CanPlace(PieceDefinition piece, Vector2Int origin)
    {
        if (piece == null || !piece.IsValid()) return false;

        for (int i = 0; i < piece.blocks.Length; i++)
        {
            Vector2Int p = origin + piece.blocks[i];
            if (!Board.IsInBounds(p)) return false;
            if (Board.GetCell(p) != CellState.Empty) return false;
        }
        return true;
    }

    /// <summary>
    /// Returns true if the given piece can be placed anywhere on the current board.
    /// (Used for game-over detection: when no hand pieces fit, the game ends.)
    /// </summary>
    public bool CanPlaceAnywhere(PieceDefinition piece)
    {
        if (piece == null || !piece.IsValid()) return false;

        // Iterate every cell as the potential drag-anchor position.
        for (int y = 0; y < Board.GridHeight; y++)
        {
            for (int x = 0; x < Board.GridWidth; x++)
            {
                Vector2Int anchorCell = new Vector2Int(x, y);
                Vector2Int origin = anchorCell - piece.dragAnchor;
                if (CanPlace(piece, origin))
                    return true;
            }
        }

        return false;
    }

    public TurnResult TryPlace(PieceDefinition piece, Vector2Int origin)
    {
        var result = new TurnResult
        {
            success = false,
            piece = piece,
            origin = origin,
            placedCells = null,
            clearedCells = null,
            linesCleared = 0,
            scoreDelta = 0
        };

        if (!CanPlace(piece, origin))
            return result;

        // 1) Place
        var placed = new List<Vector2Int>(piece.blocks.Length);
        for (int i = 0; i < piece.blocks.Length; i++)
        {
            Vector2Int p = origin + piece.blocks[i];
            Board.SetCell(p, CellState.Filled);
            placed.Add(p);
        }

        result.placedCells = placed;
        OnCellsPlaced?.Invoke(piece, placed);

        // 2) Find full lines
        var fullRows = FindFullRows();
        var fullCols = FindFullCols();
        int linesCleared = fullRows.Count + fullCols.Count;
        result.linesCleared = linesCleared;

        // 3) Clear lines
        List<Vector2Int> cleared = null;
        if (linesCleared > 0)
        {
            cleared = ClearLines(fullRows, fullCols);
            result.clearedCells = cleared;
            OnCellsCleared?.Invoke(cleared);
        }

        // 4) Scoring (Block Blast-like)
        int delta = ApplyScore(placed.Count, linesCleared);
        result.scoreDelta = delta;

        OnScoreChanged?.Invoke(Score, BestScore, ComboStreak, delta);

        result.success = true;
        return result;
    }

    private List<int> FindFullRows()
    {
        var rows = new List<int>();
        for (int y = 0; y < Board.GridHeight; y++)
        {
            bool full = true;
            for (int x = 0; x < Board.GridWidth; x++)
            {
                if (Board.GetCell(new Vector2Int(x, y)) == CellState.Empty)
                {
                    full = false;
                    break;
                }
            }
            if (full) rows.Add(y);
        }
        return rows;
    }

    private List<int> FindFullCols()
    {
        var cols = new List<int>();
        for (int x = 0; x < Board.GridWidth; x++)
        {
            bool full = true;
            for (int y = 0; y < Board.GridHeight; y++)
            {
                if (Board.GetCell(new Vector2Int(x, y)) == CellState.Empty)
                {
                    full = false;
                    break;
                }
            }
            if (full) cols.Add(x);
        }
        return cols;
    }

    private List<Vector2Int> ClearLines(List<int> rows, List<int> cols)
    {
        var toClear = new HashSet<Vector2Int>();
        var cleared = new List<Vector2Int>();

        for (int i = 0; i < rows.Count; i++)
        {
            int y = rows[i];
            for (int x = 0; x < Board.GridWidth; x++)
                toClear.Add(new Vector2Int(x, y));
        }

        for (int i = 0; i < cols.Count; i++)
        {
            int x = cols[i];
            for (int y = 0; y < Board.GridHeight; y++)
                toClear.Add(new Vector2Int(x, y));
        }

        foreach (var p in toClear)
        {
            if (Board.GetCell(p) == CellState.Filled)
            {
                Board.SetCell(p, CellState.Empty);
                cleared.Add(p);
            }
        }

        return cleared;
    }

    /// <summary>
    /// Score = placedBlocks*scorePerBlockPlaced + (BaseClear(lines) * (comboIndex+1))
    /// comboIndex:
    /// - first clear after a miss => 0
    /// - consecutive clears => 1,2,3...
    /// </summary>
    private int ApplyScore(int placedBlocksCount, int linesClearedCount)
    {
        int placeScore = placedBlocksCount * Mathf.Max(0, scorePerBlockPlaced);

        int lineScore = 0;
        if (linesClearedCount > 0)
        {
            if (lastTurnHadClear) ComboStreak++;
            else ComboStreak = 0;

            lastTurnHadClear = true;

            int baseClear = GetBaseClearScore(linesClearedCount);
            lineScore = baseClear * (ComboStreak + 1);
        }
        else
        {
            // miss resets combo chain
            ComboStreak = 0;
            lastTurnHadClear = false;
        }

        int delta = placeScore + lineScore;
        Score += delta;

        if (Score > BestScore)
        {
            BestScore = Score;
            PlayerPrefs.SetInt(BestScoreKey, BestScore);
            PlayerPrefs.Save();
        }

        return delta;
    }


    /// 10
    /// 20
    /// 60
    /// 120
    /// 200
    /// 300

    private int GetBaseClearScore(int lines)
    {
        int unit = Mathf.Max(1, baseLineUnitScore);

        if (lines <= 0) return 0;
        if (lines == 1) return unit;

        return unit * lines * (lines - 1);
    }
}
