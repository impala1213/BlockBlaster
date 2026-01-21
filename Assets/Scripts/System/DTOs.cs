using System.Collections.Generic;
using UnityEngine;

public struct PlaceResult
{
    public bool success;
    public List<Vector2Int> placedCells;
}

public struct ClearResult
{
    public List<Vector2Int> clearedCells;
    public int linesClearedCount;
}

public struct TurnResult
{
    public bool success;

    public PieceDefinition piece;
    public Vector2Int origin;

    public List<Vector2Int> placedCells;
    public List<int> clearedRows;
    public List<int> clearedCols;
    public List<Vector2Int> clearedCells;

    public int scoreDelta;
    public int score;
    public int bestScore;
    public int comboStreak;
}
