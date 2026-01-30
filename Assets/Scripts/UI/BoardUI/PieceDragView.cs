using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class PieceDragView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Refs")]
    [SerializeField] private GameController game;
    [SerializeField] private ScreenToGrid screenToGrid;
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private RectTransform dragLayer; 
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Piece")]
    [SerializeField] private PieceDefinition piece;

    [Header("Visual (optional)")]
    [SerializeField] private Image blockPrefab;

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

    private RectTransform rect;
    private Camera uiCamera;

    private bool isDragging;
    private Vector2 grabOffset;
    private Transform preDragParent;

    private RectTransform homeSlot;
    private Vector2 lastHomeSlotSize;

    private Image[] blocks;
    private Color baseColor = Color.white;
    private Vector2 lastBoardCellSize;

    private RectTransform previewLayer;
    private Image[] previewBlocks;

    private static Sprite s_FallbackSprite;

    private void Awake()
    {
        rect = (RectTransform)transform;

        AutoFillRefsIfMissing();
        EnsureRaycastTargetOnRoot();

        SubscribeGridEvents();

        TryAutoDetectHomeSlot();

        if (piece != null)
        {
            BuildOrRefreshVisual();
            ApplyTintFactor(Color.white);
            ApplyHandLayoutInSlot(force: true);
        }
    }

    private void OnDestroy()
    {
        UnsubscribeGridEvents();
    }

    private void AutoFillRefsIfMissing() // ??????? ?????? ??????? ????
    {
        if (rootCanvas == null) rootCanvas = GetComponentInParent<Canvas>();
        uiCamera = (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? rootCanvas.worldCamera : null;

        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        if (dragLayer == null && rootCanvas != null)
            dragLayer = rootCanvas.transform as RectTransform;
    }

    private void SubscribeGridEvents() // ?? ??? ????? ??????
    {
        if (screenToGrid == null) return;

        screenToGrid.OnRecalculated -= HandleGridRecalculated;
        screenToGrid.OnRecalculated += HandleGridRecalculated;

        Canvas.ForceUpdateCanvases();
        screenToGrid.Recalculate();
    }

    private void UnsubscribeGridEvents()
    {
        if (screenToGrid == null) return;
        screenToGrid.OnRecalculated -= HandleGridRecalculated;
    }

    private void EnsureRaycastTargetOnRoot() // ???? ???? ???? ?????
    {
        var img = GetComponent<Image>();
        if (img == null) img = gameObject.AddComponent<Image>();

        img.color = new Color(1f, 1f, 1f, 0f);
        img.raycastTarget = true;

        var le = GetComponent<LayoutElement>();
        if (le == null) le = gameObject.AddComponent<LayoutElement>();
        le.ignoreLayout = true;

        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    private void TryAutoDetectHomeSlot() // ???? Slot ????? 
    {
        if (homeSlot != null) return;
        if (dragLayer != null && transform.parent == dragLayer) return;

        var parentRt = transform.parent as RectTransform;
        if (parentRt == null) return;

        if (parentRt.name.StartsWith("Slot", StringComparison.OrdinalIgnoreCase))
            SetHomeSlot(parentRt);
    }


    // ??? ???? ?????? 
    public void Initialize(GameController gameRef, ScreenToGrid gridRef, Canvas canvasRef, RectTransform dragLayerRef, RectTransform homeSlotRef)
    {
        if (gameRef != null) game = gameRef;
        if (gridRef != null) screenToGrid = gridRef;
        if (canvasRef != null) rootCanvas = canvasRef;
        if (dragLayerRef != null) dragLayer = dragLayerRef;

        if (rect == null) rect = (RectTransform)transform;
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        uiCamera = (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? rootCanvas.worldCamera : null;

        EnsureRaycastTargetOnRoot();
        SubscribeGridEvents();

        if (homeSlotRef != null)
            SetHomeSlot(homeSlotRef);
    }

    public void SetHomeSlot(RectTransform slot)
    {
        homeSlot = slot;
        if (homeSlot == null) return;

        rect.SetParent(homeSlot, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one;

        ApplyHandLayoutInSlot(force: true);
    }

    public void SetPiece(PieceDefinition newPiece)
    {
        piece = newPiece;

        ClearPreview(hardDestroy: false);

        BuildOrRefreshVisual();
        ApplyTintFactor(Color.white);

        ApplyHandLayoutInSlot(force: true);
    }

    public void OnBeginDrag(PointerEventData eventData) // ???? ???? ???? 
    {
        if (!IsReadyForDrag())
            return;

        isDragging = true;
        ClearPreview(hardDestroy: false);

        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;

        preDragParent = transform.parent;

        // ???? ?????? ??? ????? ???? ????? ????
        if (dragLayer != null && transform.parent != dragLayer)
            rect.SetParent(dragLayer, true);

        transform.SetAsLastSibling();

        ApplyBoardLayout(force: true);

        // ???? ????? ???????
        RectTransformUtility.ScreenPointToLocalPointInRectangle(dragLayer, eventData.position, uiCamera, out var pointerLocal);
        grabOffset = rect.anchoredPosition - pointerLocal;
    }

    public void OnDrag(PointerEventData eventData) // ?????? ???, ???????, ??? ???????? ???? ???
    {
        if (!isDragging || !IsReadyForDrag())
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(dragLayer, eventData.position, uiCamera, out var pointerLocal);

        if (screenToGrid.TryGetGridPos(eventData.position, out var hover)) // ???? ????? ?? ??????? ??? ???? ??????? ???
        {
            if (snapOnBoard && screenToGrid.GridCenterToDragLayerLocal(hover, dragLayer, out var snapLocal))
                rect.anchoredPosition = snapLocal;
            else
                rect.anchoredPosition = pointerLocal + grabOffset;

            Vector2Int origin = hover - piece.dragAnchor;
            bool canPlace = game.CanPlace(piece, origin);

            ApplyTintFactor(canPlace ? validFactor : invalidFactor);

            if (showPreview)
                UpdatePreview(origin, canPlace);
            else
                ClearPreview(hardDestroy: false);
        }
        else
        {
            rect.anchoredPosition = pointerLocal + grabOffset;
            ApplyTintFactor(Color.white);
            ClearPreview(hardDestroy: false);
        }
    }

    public void OnEndDrag(PointerEventData eventData) // ???? ????? ó?? ??? or ???????? ????
    {
        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;

        if (!isDragging)
            return;

        bool placed = false;

        if (IsReadyForDrag() && screenToGrid.TryGetGridPos(eventData.position, out var hover))
        {
            Vector2Int origin = hover - piece.dragAnchor;
            var result = game.TryPlace(piece, origin);
            placed = result.success;

            if (result.success)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.SfxType.Click);
                }

                if (result.linesCleared > 0)
                {
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySFX(AudioManager.SfxType.Success);
                    }

                    if (VibrationManager.Instance != null)
                    {
                        VibrationManager.Instance.VibrateShort();
                    }
                }
            }
            else
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.SfxType.Fail);
                }

                if (VibrationManager.Instance != null)
                {
                    VibrationManager.Instance.VibrateShort();
                }
            }
        }

        isDragging = false;
        ApplyTintFactor(Color.white);
        ClearPreview(hardDestroy: false);

        // Return home
        if (homeSlot != null)
        {
            rect.SetParent(homeSlot, false);
            rect.localScale = Vector3.one;
            ApplyHandLayoutInSlot(force: true);
        }
        else if (preDragParent != null)
        {
            rect.SetParent(preDragParent, true);
        }

        if (placed)
            OnPlaced?.Invoke(this);
    }

    private bool IsReadyForDrag() // ???? ???????? ü? ?????.
    {
        return piece != null &&
               piece.IsValid() &&
               blocks != null &&
               blocks.Length == piece.blocks.Length &&
               screenToGrid != null &&
               game != null &&
               dragLayer != null &&
               screenToGrid.CellSize.x > 0f &&
               screenToGrid.CellSize.y > 0f;
    }

    private void HandleGridRecalculated() // cellsize ???
    {
        if (screenToGrid == null) return;

        if (screenToGrid.CellSize != lastBoardCellSize)
        {
            ApplyBoardLayout(force: true);

            if (!isDragging)
                ApplyHandLayoutInSlot(force: true);
        }
    }

    private void BuildOrRefreshVisual()
    {
        if (piece == null || !piece.IsValid() || screenToGrid == null)
        {
            EnsureBlockCount(0);
            rect.sizeDelta = Vector2.zero;
            return;
        }

        baseColor = piece.tileColor;

        EnsureBlockCount(piece.blocks.Length);

        ApplyBoardLayout(force: true);
    }

    private void EnsureBlockCount(int count)
    {
        if (count <= 0)
        {
            if (blocks != null)
            {
                for (int i = 0; i < blocks.Length; i++)
                {
                    if (blocks[i] != null)
                        blocks[i].gameObject.SetActive(false);
                }
            }
            return;
        }

        if (blocks == null || blocks.Length != count)
        {
            var newArr = new Image[count];

            int reuse = (blocks != null) ? Mathf.Min(blocks.Length, count) : 0;
            for (int i = 0; i < reuse; i++)
                newArr[i] = blocks[i];

            for (int i = reuse; i < count; i++)
            {
                newArr[i] = CreateBlockImage();
                newArr[i].transform.SetParent(transform, false);
            }

            if (blocks != null && blocks.Length > count)
            {
                for (int i = count; i < blocks.Length; i++)
                {
                    if (blocks[i] != null)
                        Destroy(blocks[i].gameObject);
                }
            }

            blocks = newArr;
        }

        Sprite spr = (piece != null && piece.tileSprite != null) ? piece.tileSprite : GetFallbackSprite();

        for (int i = 0; i < blocks.Length; i++)
        {
            var img = blocks[i];
            if (img == null) continue;

            img.gameObject.name = $"Block_{i}";
            img.gameObject.SetActive(true);

            img.raycastTarget = false;
            img.sprite = spr;
            img.color = baseColor;
            img.material = (piece != null && piece.tileMaterial != null) ? piece.tileMaterial : null;

            var r = (RectTransform)img.transform;
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);
        }
    }

    private Image CreateBlockImage()
    {
        if (blockPrefab != null)
            return Instantiate(blockPrefab);

        var go = new GameObject("Block", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        return go.GetComponent<Image>();
    }

    private void ApplyBoardLayout(bool force)
    {
        if (piece == null || !piece.IsValid() || blocks == null || screenToGrid == null)
            return;

        Vector2 cell = screenToGrid.CellSize;
        if (!force && cell == lastBoardCellSize)
            return;

        lastBoardCellSize = cell;

        LayoutBlocks(cell);
        ComputeAndApplyRootSize(cell);

        rect.localScale = Vector3.one;
    }

    private void ApplyHandLayoutInSlot(bool force)
    {
        if (homeSlot == null || piece == null || !piece.IsValid() || blocks == null)
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

        // Center the whole piece inside the slot.
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

    private void ApplyTintFactor(Color factor) // ??? ????? ??? ?????? ????
    {
        if (blocks == null) return;

        Color c = new Color(baseColor.r * factor.r, baseColor.g * factor.g, baseColor.b * factor.b, baseColor.a * factor.a);

        for (int i = 0; i < blocks.Length; i++)
        {
            if (blocks[i] != null)
                blocks[i].color = c;
        }
    }

    private void ResolvePreviewLayer()
    {
        if (previewLayer != null) return;

        if (screenToGrid != null && screenToGrid.BoardRect != null)
        {
            var found = screenToGrid.BoardRect.Find("PreviewLayer") as RectTransform;
            if (found != null)
            {
                previewLayer = found;
                return;
            }
        }

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

    private void EnsurePreviewBlocks() // block?? ???? ??????? block ????
    {
        if (piece == null || !piece.IsValid()) return;

        ResolvePreviewLayer();
        if (previewLayer == null) return;

        if (previewBlocks != null && previewBlocks.Length == piece.blocks.Length)
            return;

        if (previewBlocks != null)
        {
            for (int i = 0; i < previewBlocks.Length; i++)
            {
                if (previewBlocks[i] != null)
                    Destroy(previewBlocks[i].gameObject);
            }
        }

        previewBlocks = new Image[piece.blocks.Length];

        Sprite spr = (piece.tileSprite != null) ? piece.tileSprite : GetFallbackSprite();

        for (int i = 0; i < previewBlocks.Length; i++)
        {
            Image img;
            if (blockPrefab != null)
                img = Instantiate(blockPrefab);
            else
                img = new GameObject($"Preview_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();

            img.transform.SetParent(previewLayer, false);
            img.raycastTarget = false;

            img.sprite = spr;
            img.material = piece.tileMaterial != null ? piece.tileMaterial : null;

            var r = (RectTransform)img.transform;
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);

            img.gameObject.SetActive(false);
            previewBlocks[i] = img;
        }
    }

    private void UpdatePreview(Vector2Int origin, bool canPlace) // ????ø??? ??????? ??? ???
    {
        EnsurePreviewBlocks();
        if (previewBlocks == null || screenToGrid == null || previewLayer == null) return;

        Color factor = canPlace ? validFactor : invalidFactor;
        Color c = new Color(baseColor.r * factor.r, baseColor.g * factor.g, baseColor.b * factor.b, previewAlpha);

        Vector2 cellSize = screenToGrid.CellSize;
        RectTransform boardRect = screenToGrid.BoardRect;

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

            Vector2 boardLocal = screenToGrid.GridToBoardLocalCenter(cell);
            Vector3 world = (boardRect != null) ? boardRect.TransformPoint(boardLocal) : previewLayer.TransformPoint(boardLocal);
            Vector3 local = previewLayer.InverseTransformPoint(world);

            RectTransform r = (RectTransform)img.transform;
            r.sizeDelta = cellSize;
            r.anchoredPosition = (Vector2)local;
        }
    }

    private void ClearPreview(bool hardDestroy)
    {
        if (previewBlocks == null) return;

        if (hardDestroy)
        {
            for (int i = 0; i < previewBlocks.Length; i++)
            {
                if (previewBlocks[i] != null)
                    Destroy(previewBlocks[i].gameObject);
            }
            previewBlocks = null;
            return;
        }

        for (int i = 0; i < previewBlocks.Length; i++)
        {
            if (previewBlocks[i] != null)
                previewBlocks[i].gameObject.SetActive(false);
        }
    }

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
