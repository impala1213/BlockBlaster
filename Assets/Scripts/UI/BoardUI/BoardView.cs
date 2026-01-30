using System.Collections.Generic;
using UnityEngine;

public sealed class BoardView : MonoBehaviour
{
    [SerializeField] private GameController game;
    [SerializeField] private ScreenToGrid screenToGrid;
    [SerializeField] private RectTransform tileLayer; 
    [SerializeField] private TileView tilePrefab;

    private readonly Dictionary<Vector2Int, TileView> active = new Dictionary<Vector2Int, TileView>();
    private readonly Stack<TileView> pool = new Stack<TileView>();

    private void OnEnable()
    {
        if (game == null) return;

        game.OnBoardReset += ClearAll;
        game.OnCellsPlaced += HandlePlaced;
        game.OnCellsCleared += HandleCleared;
    }

    private void OnDisable()
    {
        if (game == null) return;

        game.OnBoardReset -= ClearAll;
        game.OnCellsPlaced -= HandlePlaced;
        game.OnCellsCleared -= HandleCleared;
    }

    private void HandlePlaced(PieceDefinition piece, List<Vector2Int> cells)
    {
        if (tileLayer == null || screenToGrid == null || cells == null) return;

        Vector2 cellSize = screenToGrid.CellSize;

        for (int i = 0; i < cells.Count; i++)
        {
            Vector2Int c = cells[i];

            if (active.TryGetValue(c, out _))
                continue;

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
        if (cells == null) return;

        for (int i = 0; i < cells.Count; i++)
        {
            Vector2Int c = cells[i];
            if (active.TryGetValue(c, out var t))
            {
                active.Remove(c);
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

    /// <summary>
    /// 외부(게임오버 등)에서 보드에 배치된 타일 비주얼을 강제로 지울 때 사용합니다.
    /// 점수/게임 상태는 건드리지 않고 타일 렌더링만 비웁니다.
    /// </summary>
    public void ClearAllVisuals()
    {
        ClearAll();
    }

    private TileView GetTile()
    {
        if (tilePrefab == null) return null;

        if (pool.Count > 0) return pool.Pop();
        return Instantiate(tilePrefab);
    }

    private void ReturnTile(TileView t)
    {
        if (t == null) return;

        t.gameObject.SetActive(false);
        if (tileLayer != null)
            t.transform.SetParent(tileLayer, false);

        pool.Push(t);
    }
}
