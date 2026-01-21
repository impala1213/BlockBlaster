using System;
using UnityEngine;

public sealed class ScreenToGrid : MonoBehaviour
{
    [SerializeField] private RectTransform boardRect; 
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private bool forceSquareCells = true;

    private Camera uiCamera;
    private Vector2 boardSize;
    private Vector2 cellSize;
    private Vector2 lastBoardSize;

    public event Action OnRecalculated;
    public Vector2 CellSize => cellSize;
    public RectTransform BoardRect => boardRect;

    private void Awake() // 미할당시 자동 할당용
    {
        if (rootCanvas == null) rootCanvas = GetComponentInParent<Canvas>();

        if (boardRect == null && rootCanvas != null)
        {
            var all = rootCanvas.GetComponentsInChildren<RectTransform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name == "GridArea")
                {
                    boardRect = all[i];
                    break;
                }
            }
        }

        uiCamera = (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? rootCanvas.worldCamera : null;

        Canvas.ForceUpdateCanvases();
        Recalculate();

        if (boardRect != null)
            lastBoardSize = boardRect.rect.size;
    }

    private void LateUpdate()
    {
        if (boardRect == null) return;

        var size = boardRect.rect.size;
        if (size != lastBoardSize)
        {
            lastBoardSize = size;
            Recalculate();
        }
    }

    public void Recalculate() // board사이즈 변화시 재계산
    {
        if (boardRect == null)
        {
            cellSize = Vector2.zero;
            return;
        }

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

    public bool TryGetGridPos(Vector2 screenPos, out Vector2Int gridPos) // 마우스의 위치가 보드의 어떤 좌표인지 구해주는 함수
    {
        gridPos = default;

        if (boardRect == null || cellSize.x <= 0f || cellSize.y <= 0f)
            return false;

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

    public Vector2 GridToBoardLocalCenter(Vector2Int gridPos) // 구한 좌표를 보드 칸 중심으로 변환 (칸 사이에 올려지는일 없게)
    {
        float x = (gridPos.x + 0.5f) * cellSize.x;
        float y = (gridPos.y + 0.5f) * cellSize.y;
        return new Vector2(x, y) - boardSize * 0.5f;
    }

    public bool GridCenterToDragLayerLocal(Vector2Int gridPos, RectTransform dragLayer, out Vector2 dragLocal) // 특정 셀의 중심을 gridlayer의 좌표로 변환
    {
        dragLocal = default;
        if (dragLayer == null || boardRect == null) return false;

        Vector2 boardLocal = GridToBoardLocalCenter(gridPos);
        Vector3 world = boardRect.TransformPoint(boardLocal);
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(uiCamera, world);

        return RectTransformUtility.ScreenPointToLocalPointInRectangle(dragLayer, screen, uiCamera, out dragLocal);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (boardRect != null)
            lastBoardSize = boardRect.rect.size;
    }
#endif
}
