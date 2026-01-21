using System.Collections.Generic;
using UnityEngine;

public sealed class BoardView : MonoBehaviour
{
    [SerializeField] private GameController game;
    [SerializeField] private ScreenToGrid screenToGrid;
    [SerializeField] private RectTransform tileLayer; // MUST be GridArea/TileLayer
    [SerializeField] private TileView tilePrefab;

    private readonly Dictionary<Vector2Int, TileView> active = new Dictionary<Vector2Int, TileView>();
    private readonly Stack<TileView> pool = new Stack<TileView>();

    private void OnEnable()
    {
        game.OnBoardReset += ClearAll;
        game.OnCellsPlaced += HandlePlaced;
        game.OnCellsCleared += HandleCleared;
    }

    private void OnDisable()
    {
        game.OnBoardReset -= ClearAll;
        game.OnCellsPlaced -= HandlePlaced;
        game.OnCellsCleared -= HandleCleared;
    }

    private void HandlePlaced(PieceDefinition piece, List<Vector2Int> cells)
    {
        Vector2 cellSize = screenToGrid.CellSize;

        for (int i = 0; i < cells.Count; i++)
        {
            Vector2Int c = cells[i];
            if (active.ContainsKey(c)) continue;

            TileView t = GetTile();
            t.gameObject.SetActive(true);
            t.transform.SetParent(tileLayer, false);

            t.ApplyLayout(screenToGrid.GridToBoardLocalCenter(c), cellSize);
            t.ApplyVisual(piece);

            active[c] = t;
        }
    }

    private void HandleCleared(List<Vector2Int> cells)
    {
        for (int i = 0; i < cells.Count; i++)
        {
            if (active.TryGetValue(cells[i], out var t))
            {
                active.Remove(cells[i]);
                ReturnTile(t);
            }
        }
    }

    private void ClearAll()
    {
        foreach (var kv in active)
            ReturnTile(kv.Value);

        active.Clear();
    }

    private TileView GetTile()
    {
        if (pool.Count > 0) return pool.Pop();
        return Instantiate(tilePrefab);
    }

    private void ReturnTile(TileView t)
    {
        t.gameObject.SetActive(false);
        t.transform.SetParent(tileLayer, false);
        pool.Push(t);
    }
}
