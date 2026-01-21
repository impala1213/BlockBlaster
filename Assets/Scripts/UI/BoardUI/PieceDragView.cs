using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Fundamental fix:
/// - No "home anchoredPosition" math from screen/world conversions.
/// - The piece lives INSIDE its slot (homeSlot) while idle.
/// - During drag: reparent to DragLayer (worldPositionStays = true), resize to board cell size, and follow pointer.
/// - Returning: reparent back to homeSlot and recompute hand layout from homeSlot.rect.size.
/// This removes the root cause of "hand slots drop / piece invisible" caused by layout timing and mixed coordinate spaces.
/// </summary>
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

    [Header("Visual")]
    [SerializeField] private Image blockPrefab; // BlockUnit.prefab (Image, raycast OFF)

    [Header("Tint Factors")]
    [SerializeField] private Color validFactor = new Color(0.6f, 1f, 0.6f, 1f);
    [SerializeField] private Color invalidFactor = new Color(1f, 0.6f, 0.6f, 1f);

    [Header("Hand Fit")]
    [Tooltip("Padding factor so the piece doesn't fully touch slot borders (0.85~0.95 recommended).")]
    [Range(0.6f, 1f)]
    [SerializeField] private float handPaddingFactor = 0.9f;

    public event Action<PieceDragView> OnPlaced;

    public bool IsDragging => isDragging;

    private RectTransform rect;
    private Camera uiCamera;

    // Home (slot) reference
    private RectTransform homeSlot;
    private bool hasHomeSlot;
    private Vector2 lastHomeSlotSize;

    // Drag
    private bool isDragging;
    private Vector2 grabOffset;

    // Visual cache
    private Image[] blocks;
    private Color[] baseColors;
    private Vector2 lastBoardCellSize;

    // Defaults from prefab (for deterministic visuals)
    private Sprite defaultBlockSprite;
    private Material defaultBlockMaterial;

    private void Awake()
    {
        rect = (RectTransform)transform;

        if (rootCanvas == null)
        {
            uiCamera = null;
        }
        else
        {
            uiCamera = (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : rootCanvas.worldCamera;
        }

        if (blockPrefab != null)
        {
            blockPrefab.raycastTarget = false;
            defaultBlockSprite = blockPrefab.sprite;
            defaultBlockMaterial = blockPrefab.material;
        }

        // Make the piece root centered by default.
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    private void Update()
    {
        // While idle in slot: if slot resizes (safe area/layout), recompute hand layout.
        if (!isDragging && hasHomeSlot && piece != null && piece.IsValid())
        {
            Vector2 size = homeSlot.rect.size;
            if (size != lastHomeSlotSize)
            {
                lastHomeSlotSize = size;
                ApplyHandLayout(force: true);
            }
        }

        // While dragging: if board cell size changes (resolution/layout), rebuild drag layout.
        if (isDragging && screenToGrid != null)
        {
            Vector2 cell = screenToGrid.CellSize;
            if (cell != lastBoardCellSize)
            {
                lastBoardCellSize = cell;
                ApplyBoardLayout();
            }
        }
    }

    /// <summary>
    /// Bind the slot that owns this piece while idle.
    /// </summary>
    public void SetHomeSlot(RectTransform slot)
    {
        homeSlot = slot;
        hasHomeSlot = homeSlot != null;
        lastHomeSlotSize = Vector2.zero;

        if (hasHomeSlot)
        {
            // Keep it as a child of the slot so it always follows layout.
            rect.SetParent(homeSlot, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;

            ApplyHandLayout(force: true);
        }
    }

    public void SetPiece(PieceDefinition def)
    {
        piece = def;

        // Destroy old visuals if any
        DestroyVisual();

        if (piece == null || !piece.IsValid() || blockPrefab == null)
            return;

        // Build child block images once
        blocks = new Image[piece.blocks.Length];
        baseColors = new Color[piece.blocks.Length];

        for (int i = 0; i < piece.blocks.Length; i++)
        {
            Image img = Instantiate(blockPrefab, transform);
            img.raycastTarget = false;

            // Deterministic visuals even when piece has null sprite/material.
            img.sprite = (piece.tileSprite != null) ? piece.tileSprite : defaultBlockSprite;
            img.material = (piece.tileMaterial != null) ? piece.tileMaterial : defaultBlockMaterial;
            img.color = piece.tileColor;

            blocks[i] = img;
            baseColors[i] = img.color;

            RectTransform r = (RectTransform)img.transform;
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);
        }

        // Apply correct layout depending on state
        if (isDragging)
            ApplyBoardLayout();
        else
            ApplyHandLayout(force: true);

        ApplyTintFactor(Color.white);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (piece == null || !piece.IsValid() || blocks == null || dragLayer == null || screenToGrid == null)
            return;

        isDragging = true;

        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;

        // Reparent to drag layer while keeping current world position to prevent jumps.
        rect.SetParent(dragLayer, true);
        transform.SetAsLastSibling();

        // Resize to board cell size (so it matches placed tiles).
        lastBoardCellSize = screenToGrid.CellSize;
        ApplyBoardLayout();

        // Compute grab offset in dragLayer local space AFTER reparent.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(dragLayer, eventData.position, uiCamera, out var pointerLocal);
        grabOffset = rect.anchoredPosition - pointerLocal;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || piece == null || !piece.IsValid() || blocks == null || dragLayer == null || screenToGrid == null || game == null)
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

        ApplyTintFactor(Color.white);

        if (placed)
        {
            // Snap back first, then notify hand to refill.
            ReturnHome(rebuildHandLayout: false);
            OnPlaced?.Invoke(this);
        }
        else
        {
            // Failed placement: return and rebuild hand layout.
            ReturnHome(rebuildHandLayout: true);
        }
    }

    private void ReturnHome(bool rebuildHandLayout)
    {
        isDragging = false;

        if (hasHomeSlot)
        {
            rect.SetParent(homeSlot, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;

            if (rebuildHandLayout)
                ApplyHandLayout(force: true);
        }
    }

    private void ApplyHandLayout(bool force)
    {
        if (!hasHomeSlot || homeSlot == null || piece == null || !piece.IsValid() || blocks == null)
            return;

        // Compute a hand cell size from slot size and piece bounds (in cells).
        Vector2 slotSize = homeSlot.rect.size;
        lastHomeSlotSize = slotSize;

        GetPieceBoundsInCells(out int widthCells, out int heightCells);

        if (widthCells <= 0 || heightCells <= 0)
            return;

        float cell = Mathf.Min(slotSize.x / widthCells, slotSize.y / heightCells) * handPaddingFactor;
        cell = Mathf.Max(1f, cell); // avoid invisible zero-size due to weird layout
        Vector2 cellSize = new Vector2(cell, cell);

        LayoutBlocks(cellSize);

        // Keep centered in slot.
        rect.anchoredPosition = Vector2.zero;
    }

    private void ApplyBoardLayout()
    {
        if (piece == null || !piece.IsValid() || blocks == null || screenToGrid == null)
            return;

        Vector2 cell = screenToGrid.CellSize;
        lastBoardCellSize = cell;

        LayoutBlocks(cell);
    }

    private void LayoutBlocks(Vector2 cellSize)
    {
        // Position each block relative to dragAnchor
        for (int i = 0; i < piece.blocks.Length; i++)
        {
            Vector2Int rel = piece.blocks[i] - piece.dragAnchor;

            RectTransform r = (RectTransform)blocks[i].transform;
            r.sizeDelta = cellSize;
            r.anchoredPosition = new Vector2(rel.x * cellSize.x, rel.y * cellSize.y);
        }

        // Root size = bounds size in cells
        GetPieceBoundsRel(out int minX, out int maxX, out int minY, out int maxY);
        float w = (maxX - minX + 1) * cellSize.x;
        float h = (maxY - minY + 1) * cellSize.y;
        rect.sizeDelta = new Vector2(w, h);
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    private void GetPieceBoundsInCells(out int widthCells, out int heightCells)
    {
        GetPieceBoundsRel(out int minX, out int maxX, out int minY, out int maxY);
        widthCells = (maxX - minX + 1);
        heightCells = (maxY - minY + 1);
    }

    private void GetPieceBoundsRel(out int minX, out int maxX, out int minY, out int maxY)
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
}
