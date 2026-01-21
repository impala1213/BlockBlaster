using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class PieceDragView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Refs")]
    [SerializeField] private GameController game;
    [SerializeField] private ScreenToGrid screenToGrid;
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private RectTransform dragLayer; // Full-stretch DragLayer
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Piece")]
    [SerializeField] private PieceDefinition piece;

    [Header("Visual (optional)")]
    [SerializeField] private Image blockPrefab; // Optional. If null, runtime Image blocks are created.

    [Header("Tint Factors")]
    [SerializeField] private Color validFactor = new Color(0.6f, 1f, 0.6f, 1f);
    [SerializeField] private Color invalidFactor = new Color(1f, 0.6f, 0.6f, 1f);

    [Header("Hand Fit")]
    [Range(0.6f, 1f)]
    [SerializeField] private float handPaddingFactor = 0.9f;

    [Header("Snap + Preview")]
    [SerializeField] private bool snapOnBoard = true;
    [SerializeField] private bool showPreview = true;
    [Range(0.05f, 1f)]
    [SerializeField] private float previewAlpha = 0.35f;

    public event Action<PieceDragView> OnPlaced;

    public bool IsDragging => isDragging;

    // Core
    private RectTransform rect;
    private Camera uiCamera;

    // Drag state
    private bool isDragging;
    private Vector2 grabOffset;
    private Transform preDragParent;
    private bool reparentedToDragLayer;

    // Visual blocks
    private Image[] blocks;
    private Color[] baseColors;
    private Vector2 lastCellSize;

    // Home mode A (legacy): anchored pos + slot size in dragLayer space
    private bool hasLegacyHome;
    private Vector2 legacyHomeAnchoredPos;
    private Vector2 legacyHomeSlotSize;
    private float legacyHandScale = 1f;

    // Home mode B (recommended): the piece lives under a slot RectTransform while idle
    private bool hasHomeSlot;
    private RectTransform homeSlot;
    private Vector2 lastHomeSlotSize;

    // Preview
    private RectTransform previewLayer;
    private Image[] previewBlocks;

    // Cached board rect (private field in ScreenToGrid)
    private RectTransform cachedBoardRect;
    private FieldInfo boardRectField;

    // Runtime fallback sprite (when no sprite/prefab is provided)
    private static Sprite s_FallbackSprite;

    private void Awake()
    {
        rect = (RectTransform)transform;

        AutoFillRefsIfMissing();
        EnsureRaycastTargetOnRoot();

        if (screenToGrid != null)
            screenToGrid.OnRecalculated += HandleGridRecalculated;

        Canvas.ForceUpdateCanvases();
        if (screenToGrid != null) screenToGrid.Recalculate();

        // If this object is already parented under a Slot*, treat it as home slot mode automatically.
        TryAutoDetectHomeSlot();

        BuildVisual();
        ApplyTintFactor(Color.white);

        if (hasHomeSlot)
            ApplyHandLayoutInSlot(force: true);
        else if (hasLegacyHome)
        {
            RecomputeLegacyHandScale();
            SnapToLegacyHomePose();
        }
    }

    private void OnDestroy()
    {
        if (screenToGrid != null)
            screenToGrid.OnRecalculated -= HandleGridRecalculated;
    }

    private void AutoFillRefsIfMissing()
    {
        if (rootCanvas == null) rootCanvas = GetComponentInParent<Canvas>();
        uiCamera = (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? rootCanvas.worldCamera : null;

        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        if (dragLayer == null && rootCanvas != null)
            dragLayer = rootCanvas.transform as RectTransform;

        if (game == null) game = FindObjectOfType<GameController>();
        if (screenToGrid == null) screenToGrid = FindObjectOfType<ScreenToGrid>();
    }

    private void EnsureRaycastTargetOnRoot()
    {
        // Drag events start from a raycast-hit UI Graphic.
        // If blocks have raycastTarget = false (recommended), the root must catch raycasts.
        var img = GetComponent<Image>();
        if (img == null) img = gameObject.AddComponent<Image>();

        img.color = new Color(1f, 1f, 1f, 0f); // fully transparent
        img.raycastTarget = true;

        // Prevent LayoutGroup from moving/resizing this piece unexpectedly.
        var le = GetComponent<LayoutElement>();
        if (le == null) le = gameObject.AddComponent<LayoutElement>();
        le.ignoreLayout = true;

        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    private void TryAutoDetectHomeSlot()
    {
        if (hasHomeSlot) return;
        if (dragLayer != null && transform.parent == dragLayer) return;

        var parentRt = transform.parent as RectTransform;
        if (parentRt == null) return;

        // Heuristic: Slot1 / Slot2 / Slot3...
        if (parentRt.name.StartsWith("Slot", StringComparison.OrdinalIgnoreCase))
        {
            SetHomeSlot(parentRt);
        }
    }

    // -------------------------
    // Public API (keep compatible)
    // -------------------------

    // Legacy: home anchored pos + home slot size (in dragLayer local space)
    public void SetHome(Vector2 anchoredPos, Vector2 slotSizeInDragLayer)
    {
        hasLegacyHome = true;
        legacyHomeAnchoredPos = anchoredPos;
        legacyHomeSlotSize = slotSizeInDragLayer;

        // If the object is under a Slot parent, prefer slot mode.
        TryAutoDetectHomeSlot();

        RecomputeLegacyHandScale();
        SnapToLegacyHomePose();
    }

    // Recommended: the piece lives under slot while idle; drag reparents to dragLayer
    public void SetHomeSlot(RectTransform slot)
    {
        homeSlot = slot;
        hasHomeSlot = homeSlot != null;

        if (!hasHomeSlot) return;

        // While idle, live under the slot so layout changes never break the hand.
        rect.SetParent(homeSlot, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one;

        ApplyHandLayoutInSlot(force: true);
    }

    public void SetPiece(PieceDefinition newPiece)
    {
        piece = newPiece;

        BuildVisual();
        ApplyTintFactor(Color.white);

        ClearPreview();

        if (hasHomeSlot)
            ApplyHandLayoutInSlot(force: true);
        else if (hasLegacyHome)
        {
            RecomputeLegacyHandScale();
            SnapToLegacyHomePose();
        }
    }

    // -------------------------
    // Drag handlers
    // -------------------------

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (piece == null || !piece.IsValid() || blocks == null || screenToGrid == null || game == null || dragLayer == null)
            return;

        isDragging = true;
        ClearPreview();

        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;

        preDragParent = transform.parent;
        reparentedToDragLayer = false;

        // If we were under a slot, move to dragLayer for free dragging over the board.
        if (hasHomeSlot && transform.parent != dragLayer)
        {
            rect.SetParent(dragLayer, true);
            reparentedToDragLayer = true;
        }

        transform.SetAsLastSibling();

        ApplyBoardLayout();

        // Compute grab offset in dragLayer local space
        RectTransformUtility.ScreenPointToLocalPointInRectangle(dragLayer, eventData.position, uiCamera, out var pointerLocal);
        grabOffset = rect.anchoredPosition - pointerLocal;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || piece == null || !piece.IsValid() || blocks == null || screenToGrid == null || game == null || dragLayer == null)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(dragLayer, eventData.position, uiCamera, out var pointerLocal);

        if (screenToGrid.TryGetGridPos(eventData.position, out var hover))
        {
            // Snap the dragAnchor cell center to the hovered cell center (feels "chunky" / grid-locked).
            if (snapOnBoard && TryGetDragLocalForCellCenter(hover, out var snapLocal))
            {
                rect.anchoredPosition = snapLocal;
            }
            else
            {
                rect.anchoredPosition = pointerLocal + grabOffset;
            }

            Vector2Int origin = hover - piece.dragAnchor;
            bool canPlace = game.CanPlace(piece, origin);

            ApplyTintFactor(canPlace ? validFactor : invalidFactor);

            if (showPreview)
                UpdatePreview(origin, canPlace);
            else
                ClearPreview();
        }
        else
        {
            // Outside the board: free movement, no preview
            rect.anchoredPosition = pointerLocal + grabOffset;
            ApplyTintFactor(Color.white);
            ClearPreview();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;

        if (!isDragging)
            return;

        bool placed = false;

        if (piece != null && piece.IsValid() && blocks != null && screenToGrid != null && game != null)
        {
            if (screenToGrid.TryGetGridPos(eventData.position, out var hover))
            {
                Vector2Int origin = hover - piece.dragAnchor;
                var turn = game.TryPlace(piece, origin);
                placed = turn.success;
            }
        }

        isDragging = false;
        ApplyTintFactor(Color.white);
        ClearPreview();

        // Return home
        if (hasHomeSlot)
        {
            if (homeSlot != null)
            {
                rect.SetParent(homeSlot, false);
                rect.localScale = Vector3.one;
                ApplyHandLayoutInSlot(force: true);
            }
        }
        else if (hasLegacyHome)
        {
            RecomputeLegacyHandScale();
            SnapToLegacyHomePose();
        }
        else
        {
            // If no home set, restore parent if we reparented
            if (reparentedToDragLayer && preDragParent != null)
                rect.SetParent(preDragParent, true);
        }

        if (placed)
            OnPlaced?.Invoke(this);
    }

    // -------------------------
    // Recalc hooks
    // -------------------------

    private void HandleGridRecalculated()
    {
        if (screenToGrid == null) return;

        if (screenToGrid.CellSize != lastCellSize)
        {
            ApplyBoardLayout();

            if (hasHomeSlot)
                ApplyHandLayoutInSlot(force: true);
            else if (hasLegacyHome)
            {
                RecomputeLegacyHandScale();
                SnapToLegacyHomePose();
            }
        }
    }

    // -------------------------
    // Visual build/layout
    // -------------------------

    private void BuildVisual()
    {
        // Destroy old blocks
        if (blocks != null)
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                if (blocks[i] != null)
                    Destroy(blocks[i].gameObject);
            }
        }

        blocks = null;
        baseColors = null;

        if (piece == null || !piece.IsValid() || screenToGrid == null)
            return;

        Vector2 cell = screenToGrid.CellSize;
        lastCellSize = cell;

        blocks = new Image[piece.blocks.Length];
        baseColors = new Color[piece.blocks.Length];

        for (int i = 0; i < piece.blocks.Length; i++)
        {
            Image img = CreateBlockImage();
            img.transform.SetParent(transform, false);

            // Always disable block raycast: root receives raycasts (stable drag target).
            img.raycastTarget = false;

            // Deterministic visuals
            img.sprite = (piece.tileSprite != null) ? piece.tileSprite : GetFallbackSprite();
            img.color = piece.tileColor;
            img.material = piece.tileMaterial != null ? piece.tileMaterial : null;

            RectTransform r = (RectTransform)img.transform;
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.sizeDelta = cell;

            Vector2Int rel = piece.blocks[i] - piece.dragAnchor;
            r.anchoredPosition = new Vector2(rel.x * cell.x, rel.y * cell.y);

            blocks[i] = img;
            baseColors[i] = img.color;
        }

        ComputeAndApplyRootSize(cell);
    }

    private Image CreateBlockImage()
    {
        if (blockPrefab != null)
            return Instantiate(blockPrefab);

        var go = new GameObject("Block", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        return go.GetComponent<Image>();
    }

    private void ApplyBoardLayout()
    {
        if (piece == null || !piece.IsValid() || blocks == null || screenToGrid == null)
            return;

        Vector2 cell = screenToGrid.CellSize;
        lastCellSize = cell;

        LayoutBlocks(cell);
        ComputeAndApplyRootSize(cell);

        rect.localScale = Vector3.one;
    }

    private void ApplyHandLayoutInSlot(bool force)
    {
        if (!hasHomeSlot || homeSlot == null || piece == null || !piece.IsValid() || blocks == null)
            return;

        Vector2 slotSize = homeSlot.rect.size;
        if (!force && slotSize == lastHomeSlotSize)
            return;

        lastHomeSlotSize = slotSize;

        GetBoundsRel(out int minX, out int maxX, out int minY, out int maxY);
        int wCells = (maxX - minX + 1);
        int hCells = (maxY - minY + 1);
        if (wCells <= 0 || hCells <= 0) return;

        float cell = Mathf.Min(slotSize.x / wCells, slotSize.y / hCells) * handPaddingFactor;
        cell = Mathf.Max(4f, cell);
        Vector2 cellSize = new Vector2(cell, cell);

        LayoutBlocks(cellSize);
        ComputeAndApplyRootSize(cellSize);

        // Center the whole piece inside the slot (dragAnchor may not be geometric center).
        Vector2 centerOffset = new Vector2((minX + maxX) * 0.5f * cellSize.x, (minY + maxY) * 0.5f * cellSize.y);
        rect.anchoredPosition = -centerOffset;
        rect.localScale = Vector3.one;
    }

    private void LayoutBlocks(Vector2 cell)
    {
        for (int i = 0; i < piece.blocks.Length; i++)
        {
            RectTransform r = (RectTransform)blocks[i].transform;
            r.sizeDelta = cell;

            Vector2Int rel = piece.blocks[i] - piece.dragAnchor;
            r.anchoredPosition = new Vector2(rel.x * cell.x, rel.y * cell.y);
        }
    }

    private void ComputeAndApplyRootSize(Vector2 cell)
    {
        GetBoundsRel(out int minX, out int maxX, out int minY, out int maxY);

        float w = (maxX - minX + 1) * cell.x;
        float h = (maxY - minY + 1) * cell.y;

        rect.sizeDelta = new Vector2(w, h);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
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

    // -------------------------
    // Legacy home pose (for old HandController logic)
    // -------------------------

    private void RecomputeLegacyHandScale()
    {
        if (!hasLegacyHome || piece == null || !piece.IsValid() || screenToGrid == null)
        {
            legacyHandScale = 1f;
            return;
        }

        Vector2 pieceSize = rect.sizeDelta;

        float sx = (pieceSize.x <= 0.01f) ? 1f : (legacyHomeSlotSize.x * handPaddingFactor) / pieceSize.x;
        float sy = (pieceSize.y <= 0.01f) ? 1f : (legacyHomeSlotSize.y * handPaddingFactor) / pieceSize.y;

        legacyHandScale = Mathf.Clamp01(Mathf.Min(sx, sy));
        if (legacyHandScale <= 0f) legacyHandScale = 1f;
    }

    private void SnapToLegacyHomePose()
    {
        if (!hasLegacyHome) return;
        rect.anchoredPosition = legacyHomeAnchoredPos;
        rect.localScale = Vector3.one * legacyHandScale;
    }

    // -------------------------
    // Tint
    // -------------------------

    private void ApplyTintFactor(Color factor)
    {
        if (blocks == null || baseColors == null) return;

        for (int i = 0; i < blocks.Length; i++)
        {
            Color b = baseColors[i];
            blocks[i].color = new Color(b.r * factor.r, b.g * factor.g, b.b * factor.b, b.a * factor.a);
        }
    }

    // -------------------------
    // Snap helpers
    // -------------------------

    private bool TryGetDragLocalForCellCenter(Vector2Int cell, out Vector2 dragLocal)
    {
        dragLocal = default;

        RectTransform boardRect = ResolveBoardRect();
        if (boardRect == null || dragLayer == null || screenToGrid == null) return false;

        Vector2 boardLocal = screenToGrid.GridToBoardLocalCenter(cell);
        Vector3 world = boardRect.TransformPoint(boardLocal);
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(uiCamera, world);

        return RectTransformUtility.ScreenPointToLocalPointInRectangle(dragLayer, screen, uiCamera, out dragLocal);
    }

    private RectTransform ResolveBoardRect()
    {
        if (cachedBoardRect != null) return cachedBoardRect;
        if (screenToGrid == null) return null;

        // Try reflection: ScreenToGrid has a private [SerializeField] RectTransform boardRect
        if (boardRectField == null)
        {
            boardRectField = typeof(ScreenToGrid).GetField("boardRect", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        cachedBoardRect = boardRectField != null ? boardRectField.GetValue(screenToGrid) as RectTransform : null;

        // Fallback: search by name in canvas
        if (cachedBoardRect == null && rootCanvas != null)
        {
            var all = rootCanvas.GetComponentsInChildren<RectTransform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name == "GridArea")
                {
                    cachedBoardRect = all[i];
                    break;
                }
            }
        }

        return cachedBoardRect;
    }
    public void Initialize(GameController gameRef, ScreenToGrid gridRef, Canvas canvasRef, RectTransform dragLayerRef, RectTransform homeSlotRef)
    {
        if (gameRef != null) game = gameRef;
        if (gridRef != null) screenToGrid = gridRef;
        if (canvasRef != null) rootCanvas = canvasRef;
        if (dragLayerRef != null) dragLayer = dragLayerRef;

        // 내부 세팅 다시 계산
        if (rect == null) rect = (RectTransform)transform;
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        uiCamera = (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? rootCanvas.worldCamera : null;

        EnsureRaycastTargetOnRoot();

        if (screenToGrid != null)
        {
            screenToGrid.OnRecalculated -= HandleGridRecalculated;
            screenToGrid.OnRecalculated += HandleGridRecalculated;
            screenToGrid.Recalculate();
        }

        if (homeSlotRef != null)
            SetHomeSlot(homeSlotRef);
    }

    private void ResolvePreviewLayer()
    {
        if (previewLayer != null) return;

        RectTransform boardRect = ResolveBoardRect();
        if (boardRect != null)
        {
            // Typical hierarchy: GridArea/PreviewLayer
            var found = boardRect.Find("PreviewLayer") as RectTransform;
            if (found != null)
            {
                previewLayer = found;
                return;
            }
        }

        // Fallback: search by name
        if (rootCanvas != null)
        {
            var all = rootCanvas.GetComponentsInChildren<RectTransform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name == "PreviewLayer")
                {
                    previewLayer = all[i];
                    return;
                }
            }
        }
    }

    private void EnsurePreviewBlocks()
    {
        if (piece == null || !piece.IsValid()) return;
        ResolvePreviewLayer();
        if (previewLayer == null) return;

        if (previewBlocks != null && previewBlocks.Length == piece.blocks.Length)
            return;

        ClearPreview();

        previewBlocks = new Image[piece.blocks.Length];
        Sprite spr = (piece.tileSprite != null) ? piece.tileSprite : GetFallbackSprite();

        for (int i = 0; i < previewBlocks.Length; i++)
        {
            Image img;
            if (blockPrefab != null)
            {
                img = Instantiate(blockPrefab);
            }
            else
            {
                var go = new GameObject($"Preview_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                img = go.GetComponent<Image>();
            }

            img.transform.SetParent(previewLayer, false);
            img.raycastTarget = false;

            img.sprite = spr;
            img.material = piece.tileMaterial != null ? piece.tileMaterial : null;

            RectTransform r = (RectTransform)img.transform;
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);

            previewBlocks[i] = img;
        }
    }

    private void UpdatePreview(Vector2Int origin, bool canPlace)
    {
        EnsurePreviewBlocks();
        if (previewBlocks == null || screenToGrid == null) return;

        Color factor = canPlace ? validFactor : invalidFactor;
        Color baseC = piece.tileColor;
        Color c = new Color(baseC.r * factor.r, baseC.g * factor.g, baseC.b * factor.b, previewAlpha);

        Vector2 cellSize = screenToGrid.CellSize;

        for (int i = 0; i < previewBlocks.Length; i++)
        {
            Vector2Int cell = origin + piece.blocks[i];

            bool inBounds =
                cell.x >= 0 && cell.x < Board.GridWidth &&
                cell.y >= 0 && cell.y < Board.GridHeight;

            Image img = previewBlocks[i];
            img.gameObject.SetActive(inBounds);
            if (!inBounds) continue;

            img.color = c;

            // Place in preview layer using board local -> preview local conversion
            RectTransform boardRect = ResolveBoardRect();
            if (boardRect != null && previewLayer != null)
            {
                Vector2 boardLocal = screenToGrid.GridToBoardLocalCenter(cell);
                Vector3 world = boardRect.TransformPoint(boardLocal);
                Vector3 local = previewLayer.InverseTransformPoint(world);

                RectTransform r = (RectTransform)img.transform;
                r.sizeDelta = cellSize;
                r.anchoredPosition = (Vector2)local;
            }
            else
            {
                // Fallback (works if PreviewLayer shares board local origin)
                RectTransform r = (RectTransform)img.transform;
                r.sizeDelta = cellSize;
                r.anchoredPosition = screenToGrid.GridToBoardLocalCenter(cell);
            }
        }
    }

    private void ClearPreview()
    {
        if (previewBlocks == null) return;

        for (int i = 0; i < previewBlocks.Length; i++)
        {
            if (previewBlocks[i] != null)
                Destroy(previewBlocks[i].gameObject);
        }

        previewBlocks = null;
    }

    // -------------------------
    // Fallback sprite
    // -------------------------

    private static Sprite GetFallbackSprite()
    {
        if (s_FallbackSprite != null) return s_FallbackSprite;

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
