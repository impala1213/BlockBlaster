using System.Collections.Generic;
using UnityEngine;

public sealed class PlaceManager
{
    public bool CanPlace(Board board, PieceDefinition piece, Vector2Int origin)
    {
        for (int i = 0; i < piece.blocks.Length; i++)
        {
            Vector2Int p = origin + piece.blocks[i];
            if (!board.IsInBounds(p)) return false;
            if (board.GetCell(p) != CellState.Empty) return false;
        }
        return true;
    }

    public PlaceResult Place(Board board, PieceDefinition piece, Vector2Int origin)
    {
        var res = new PlaceResult
        {
            success = false,
            placedCells = new List<Vector2Int>(piece.blocks.Length)
        };

        if (!CanPlace(board, piece, origin))
            return res;

        for (int i = 0; i < piece.blocks.Length; i++)
        {
            Vector2Int p = origin + piece.blocks[i];
            board.SetCell(p, CellState.Filled);
            res.placedCells.Add(p);
        }

        res.success = true;
        return res;
    }
}
