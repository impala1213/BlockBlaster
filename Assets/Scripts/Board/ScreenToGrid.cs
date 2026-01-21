using System;
using UnityEngine;

public sealed class ScreenToGrid : MonoBehaviour
{
    [SerializeField] private RectTransform boardRect; // MUST be GridArea
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private bool forceSquareCells = true;

    private Camera uiCamera;
    private Vector2 boardSize;
    private Vector2 cellSize;
    private Vector2 lastBoardSize;

    public event Action OnRecalculated;
    public Vector2 CellSize => cellSize;

    private void Awake()
    {
        uiCamera = (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : rootCanvas.worldCamera;

        Canvas.ForceUpdateCanvases();
        Recalculate();
        lastBoardSize = boardRect.rect.size;
    }

    private void LateUpdate()
    {
        var size = boardRect.rect.size;
        if (size != lastBoardSize)
        {
            lastBoardSize = size;
            Recalculate();
        }
    }

    public void Recalculate()
    {
        boardSize = boardRect.rect.size;

        float cx = boardSize.x / Board.GridWidth;
        float cy = boardSize.y / Board.GridHeight;

        if (forceSquareCells)
        {
            float s = Mathf.Min(cx, cy);
            cellSize = new Vector2(s, s);
        }
        else
        {
            cellSize = new Vector2(cx, cy);
        }

        OnRecalculated?.Invoke();
    }

    public bool TryGetGridPos(Vector2 screenPos, out Vector2Int gridPos)
    {
        gridPos = default;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(boardRect, screenPos, uiCamera, out var local))
            return false;

        Vector2 bottomLeft = local + boardSize * 0.5f;

        int x = Mathf.FloorToInt(bottomLeft.x / cellSize.x);
        int y = Mathf.FloorToInt(bottomLeft.y / cellSize.y);

        if (x < 0 || x >= Board.GridWidth || y < 0 || y >= Board.GridHeight)
            return false;

        gridPos = new Vector2Int(x, y);
        return true;
    }

    public Vector2 GridToBoardLocalCenter(Vector2Int gridPos)
    {
        float x = (gridPos.x + 0.5f) * cellSize.x;
        float y = (gridPos.y + 0.5f) * cellSize.y;
        return new Vector2(x, y) - boardSize * 0.5f;
    }
}
