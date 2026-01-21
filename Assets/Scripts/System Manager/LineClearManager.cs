using System.Collections.Generic;
using UnityEngine;

public sealed class LineClearManager
{
    public List<int> FindFullRows(Board board)
    {
        var rows = new List<int>();

        for (int y = 0; y < Board.GridHeight; y++)
        {
            bool full = true;
            for (int x = 0; x < Board.GridWidth; x++)
            {
                if (board.GetCell(new Vector2Int(x, y)) == CellState.Empty)
                {
                    full = false;
                    break;
                }
            }
            if (full) rows.Add(y);
        }

        return rows;
    }

    public List<int> FindFullCols(Board board)
    {
        var cols = new List<int>();

        for (int x = 0; x < Board.GridWidth; x++)
        {
            bool full = true;
            for (int y = 0; y < Board.GridHeight; y++)
            {
                if (board.GetCell(new Vector2Int(x, y)) == CellState.Empty)
                {
                    full = false;
                    break;
                }
            }
            if (full) cols.Add(x);
        }

        return cols;
    }

    public ClearResult ClearLines(Board board, List<int> rows, List<int> cols)
    {
        var toClear = new HashSet<Vector2Int>();
        var clearedCells = new List<Vector2Int>();

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
            if (board.GetCell(p) == CellState.Filled)
            {
                board.SetCell(p, CellState.Empty);
                clearedCells.Add(p);
            }
        }

        return new ClearResult
        {
            clearedCells = clearedCells,
            linesClearedCount = rows.Count + cols.Count
        };
    }
}
