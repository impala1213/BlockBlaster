using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class PieceDragView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Refs (optional if initialized by HandController)")]
    [SerializeField] private GameController game;
    [SerializeField] private ScreenToGrid screenToGrid;
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private RectTransform dragLayer; // Full-stretch DragLayer
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Piece")]
    [SerializeField] private PieceDefinition piece;

    [Header("Tint Factors")]
    [SerializeField] private Color validFactor = new Color(0.6f, 1f, 0.6f, 1f);
    [SerializeField] private Color invalidFactor = new Color(1f, 0.6f, 0.6f, 1f);

    [Header("Hand Fit")]
    [Tooltip("Slot 안에서 너무 꽉 차지 않게 여백 비율(0.85~0.95 추천)")]
    [Range(0.6f, 1f)]
    [SerializeField] private float handPaddingFactor = 0.9f;

    public event Action<PieceDragView> OnPlaced;

    public bool IsDragging => isDragging;

    private RectTransform rect;
    private Camera uiCamera;

    // Home slot (idle 상태일 때 slot의 자식으로 존재)
    private RectTransform homeSlot;
    private bool hasHomeSlot;

    // Drag
    private bool isDragging;
    private Vector2 grabOffset;

    // Visual blocks
    private Image[] blocks;
    private Color[] baseColors;

    private Vector2 lastHomeSlotSize;

    private static Sprite s_FallbackSprite;

    public void Initialize(GameController gameRef, ScreenToGrid gridRef, Canvas canvasRef, RectTransform dragLayerRef, RectTransform homeSlotRef)
    {
        game = gameRef != null ? gameRef : game;
        screenToGrid = gridRef != null ? gridRef : screenToGrid;
        rootCanvas = canvasRef != null ? canvasRef : rootCanvas;
        dragLayer = dragLayerRef != null ? dragLayerRef : dragLayer;

        EnsureSetup();

        SetHomeSlot(homeSlotRef);

        if (screenToGrid != null)
        {
            screenToGrid.OnRecalculated -= HandleGridRecalculated;
            screenToGrid.OnRecalculated += HandleGridRecalculated;
            screenToGrid.Recalculate();
        }
    }

    private void Awake()
    {
        EnsureSetup();

        // Initialize 안 했을 때도 최소한 돌아가게 자동 탐색
        if (game == null) game = FindObjectOfType<GameController>();
        if (screenToGrid == null) screenToGrid = FindObjectOfType<ScreenToGrid>();
        if (rootCanvas == null) rootCanvas = GetComponentInParent<Canvas>();
        if (dragLayer == null && rootCanvas != null) dragLayer = rootCanvas.transform as RectTransform;

        if (screenToGrid != null)
        {
            screenToGrid.OnRecalculated -= HandleGridRecalculated;
            screenToGrid.OnRecalculated += HandleGridRecalculated;
        }
    }

    private void OnDestroy()
    {
        if (screenToGrid != null)
            screenToGrid.OnRecalculated -= HandleGridRecalculated;
    }

    private void EnsureSetup()
    {
        if (rect == null) rect = (RectTransform)transform;

        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (rootCanvas != null)
            uiCamera = (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : rootCanvas.worldCamera;

        // LayoutGroup 영향 차단
        var le = GetComponent<LayoutElement>();
        if (le == null) le = gameObject.AddComponent<LayoutElement>();
        le.ignoreLayout = true;

        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    public void SetHomeSlot(RectTransform slot)
    {
        homeSlot = slot;
        hasHomeSlot = homeSlot != null;

        if (!hasHomeSlot) return;

        rect.SetParent(homeSlot, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one;

        ApplyHandLayout(force: true);
    }

    public void SetPiece(PieceDefinition newPiece)
    {
        piece = newPiece;

        DestroyVisual();

        if (piece == null || !piece.IsValid())
            return;

        BuildVisual();

        if (isDragging) ApplyBoardLayout();
        else ApplyHandLayout(force: true);

        ApplyTintFactor(Color.white);
    }

    private void Update()
    {
        if (!isDragging && hasHomeSlot && piece != null && piece.IsValid())
        {
            Vector2 s = homeSlot.rect.size;
            if (s != lastHomeSlotSize)
            {
                lastHomeSlotSize = s;
                ApplyHandLayout(force: true);
            }
        }
    }

    private void HandleGridRecalculated()
    {
        if (!isDragging) return;
        ApplyBoardLayout();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (piece == null || !piece.IsValid() || dragLayer == null || screenToGrid == null || game == null)
            return;

        isDragging = true;
        canvasGroup.blocksRaycasts = false;

        // dragLayer로 이동 (점프 방지)
        rect.SetParent(dragLayer, true);
        transform.SetAsLastSibling();

        ApplyBoardLayout();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(dragLayer, eventData.position, uiCamera, out var pointerLocal);
        grabOffset = rect.anchoredPosition - pointerLocal;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || piece == null || !piece.IsValid() || dragLayer == null || screenToGrid == null || game == null)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(dragLayer, eventData.position, uiCamera, out var pointerLocal);
        rect.anchoredPosition = pointerLocal + grabOffset;

        if (screenToGrid.TryGetGridPos(eventData.position, out var hover))
        {
            Vector2Int origin = hover - piece.dragAnchor;
            bool canPlace = game.CanPlace(piece, origin);
            ApplyTintFactor(canPlace ? validFactor : invalidFactor);
        }
        else
        {
            ApplyTintFactor(Color.white);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (!isDragging) return;

        bool placed = false;

        if (piece != null && piece.IsValid() && screenToGrid != null && game != null)
        {
            if (screenToGrid.TryGetGridPos(eventData.position, out var hover))
            {
                Vector2Int origin = hover - piece.dragAnchor;
                var turn = game.TryPlace(piece, origin);
                placed = turn.success;
            }
        }

        ApplyTintFactor(Color.white);

        isDragging = false;

        // home으로 복귀
        if (hasHomeSlot)
        {
            rect.SetParent(homeSlot, false);
            rect.localScale = Vector3.one;
            ApplyHandLayout(force: true);
        }

        if (placed)
            OnPlaced?.Invoke(this);
    }

    private void BuildVisual()
    {
        blocks = new Image[piece.blocks.Length];
        baseColors = new Color[piece.blocks.Length];

        Sprite sprite = (piece.tileSprite != null) ? piece.tileSprite : GetFallbackSprite();

        for (int i = 0; i < piece.blocks.Length; i++)
        {
            var go = new GameObject($"Block_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            go.transform.SetParent(transform, false);

            // LayoutGroup 영향 차단
            go.GetComponent<LayoutElement>().ignoreLayout = true;

            var img = go.GetComponent<Image>();
            img.raycastTarget = true;

            img.sprite = sprite;
            img.color = piece.tileColor;
            img.material = piece.tileMaterial; // null이면 기본 UI material

            blocks[i] = img;
            baseColors[i] = img.color;

            RectTransform r = (RectTransform)go.transform;
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);
        }
    }

    private void DestroyVisual()
    {
        if (blocks == null) return;

        for (int i = 0; i < blocks.Length; i++)
        {
            if (blocks[i] != null)
                Destroy(blocks[i].gameObject);
        }

        blocks = null;
        baseColors = null;
    }

    private void ApplyHandLayout(bool force)
    {
        if (!hasHomeSlot || homeSlot == null || piece == null || !piece.IsValid() || blocks == null)
            return;

        Vector2 slotSize = homeSlot.rect.size;
        lastHomeSlotSize = slotSize;

        GetBoundsRel(out int minX, out int maxX, out int minY, out int maxY);
        int wCells = (maxX - minX + 1);
        int hCells = (maxY - minY + 1);
        if (wCells <= 0 || hCells <= 0) return;

        float cell = Mathf.Min(slotSize.x / wCells, slotSize.y / hCells) * handPaddingFactor;
        cell = Mathf.Max(4f, cell); // tiny frame에서도 안 사라지게
        Vector2 cellSize = new Vector2(cell, cell);

        LayoutBlocks(cellSize);

        // dragAnchor가 중앙이 아닐 수 있으니 bounds 기준으로 slot 중앙 정렬
        Vector2 centerOffset = new Vector2((minX + maxX) * 0.5f * cellSize.x, (minY + maxY) * 0.5f * cellSize.y);
        rect.anchoredPosition = -centerOffset;
    }

    private void ApplyBoardLayout()
    {
        if (piece == null || !piece.IsValid() || blocks == null || screenToGrid == null)
            return;

        Vector2 cell = screenToGrid.CellSize;
        LayoutBlocks(cell);
    }

    private void LayoutBlocks(Vector2 cellSize)
    {
        for (int i = 0; i < piece.blocks.Length; i++)
        {
            Vector2Int rel = piece.blocks[i] - piece.dragAnchor;
            RectTransform r = (RectTransform)blocks[i].transform;
            r.sizeDelta = cellSize;
            r.anchoredPosition = new Vector2(rel.x * cellSize.x, rel.y * cellSize.y);
        }

        GetBoundsRel(out int minX, out int maxX, out int minY, out int maxY);
        float w = (maxX - minX + 1) * cellSize.x;
        float h = (maxY - minY + 1) * cellSize.y;
        rect.sizeDelta = new Vector2(w, h);
    }

    private void GetBoundsRel(out int minX, out int maxX, out int minY, out int maxY)
    {
        minX = int.MaxValue; maxX = int.MinValue;
        minY = int.MaxValue; maxY = int.MinValue;

        for (int i = 0; i < piece.blocks.Length; i++)
        {
            Vector2Int rel = piece.blocks[i] - piece.dragAnchor;
            minX = Mathf.Min(minX, rel.x);
            maxX = Mathf.Max(maxX, rel.x);
            minY = Mathf.Min(minY, rel.y);
            maxY = Mathf.Max(maxY, rel.y);
        }
    }

    private void ApplyTintFactor(Color factor)
    {
        if (blocks == null || baseColors == null) return;

        for (int i = 0; i < blocks.Length; i++)
        {
            Color b = baseColors[i];
            blocks[i].color = new Color(b.r * factor.r, b.g * factor.g, b.b * factor.b, b.a * factor.a);
        }
    }

    private static Sprite GetFallbackSprite()
    {
        if (s_FallbackSprite != null) return s_FallbackSprite;

        // 프리팹 없이도 무조건 보이게 런타임 흰색 스프라이트 생성
        var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
        tex.hideFlags = HideFlags.HideAndDontSave;
        tex.filterMode = FilterMode.Point;

        var pixels = new Color32[8 * 8];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, 255, 255, 255);
        tex.SetPixels32(pixels);
        tex.Apply(false, true);

        s_FallbackSprite = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 100f);
        s_FallbackSprite.name = "RuntimeWhiteSprite";
        return s_FallbackSprite;
    }
}
