using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Line clear VFX:
/// - When rows/cols are cleared, spawn horizontal/vertical VFX prefabs.
/// - VFX color MUST be the color of the last placed piece in that turn.
///   Example: if player placed a blue piece, any line cleared in that resolution uses blue.
/// 
/// Notes:
/// - This script does NOT scale/resize your prefab (keeps prefab as-is).
/// - It force-tints ParticleSystems + Sprite/Line/Trail renderers + MaterialPropertyBlock colors.
/// - It detects cleared rows/cols strictly from clearedCells to avoid missing lines.
/// </summary>
[DisallowMultipleComponent]
public sealed class LineClearVfx : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameController game;
    [SerializeField] private ScreenToGrid screenToGrid;

    [Header("Prefabs (keep as-is)")]
    [SerializeField] private GameObject horizontalVfxPrefab; // for cleared ROW
    [SerializeField] private GameObject verticalVfxPrefab;   // for cleared COL

    [Header("Spawn Parent (IMPORTANT)")]
    [Tooltip("Assign the same parent where dragging the prefab in the scene looks PERFECT.")]
    [SerializeField] private Transform vfxParent;

    [Header("VFX Layer (for VfxCamera culling)")]
    [SerializeField] private string vfxLayerName = "VFX_UI";

    [Header("Auto Destroy")]
    [SerializeField] private float destroyPadding = 0.25f;

    // This is the core requirement: color of the last placed piece of THIS turn.
    private Color lastPlacedPieceColor = Color.white;
    private bool hasPlacedThisTurn = false;

    private static readonly int _Color = Shader.PropertyToID("_Color");
    private static readonly int _BaseColor = Shader.PropertyToID("_BaseColor");
    private static readonly int _TintColor = Shader.PropertyToID("_TintColor");

    private void OnEnable()
    {
        if (game == null) return;
        game.OnCellsPlaced += HandleCellsPlaced;
        game.OnCellsCleared += HandleCellsCleared;
    }

    private void OnDisable()
    {
        if (game == null) return;
        game.OnCellsPlaced -= HandleCellsPlaced;
        game.OnCellsCleared -= HandleCellsCleared;
    }

    /// <summary>
    /// Called when a piece is placed. We capture the piece color as "this turn's color".
    /// </summary>
    private void HandleCellsPlaced(PieceDefinition piece, List<Vector2Int> placedCells)
    {
        lastPlacedPieceColor = (piece != null) ? piece.tileColor : Color.white;
        hasPlacedThisTurn = true;
    }

    /// <summary>
    /// Called when cells are cleared. Spawn VFX for cleared rows/cols using the last placed piece color.
    /// </summary>
    private void HandleCellsCleared(List<Vector2Int> clearedCells)
    {
        if (clearedCells == null || clearedCells.Count == 0) return;
        if (screenToGrid == null || screenToGrid.BoardRect == null) return;

        // If for some reason clears happen without a placement event (e.g. editor testing),
        // we still do VFX but fallback to white.
        Color tint = hasPlacedThisTurn ? lastPlacedPieceColor : Color.white;

        // Count how many cleared cells exist per row/col to identify full-line clears reliably.
        Dictionary<int, int> rowCounts = new Dictionary<int, int>();
        Dictionary<int, int> colCounts = new Dictionary<int, int>();

        for (int i = 0; i < clearedCells.Count; i++)
        {
            int x = clearedCells[i].x;
            int y = clearedCells[i].y;

            rowCounts[y] = rowCounts.TryGetValue(y, out int rc) ? rc + 1 : 1;
            colCounts[x] = colCounts.TryGetValue(x, out int cc) ? cc + 1 : 1;
        }

        RectTransform boardRect = screenToGrid.BoardRect;

        // Spawn row VFX where a full row (GridWidth) was cleared
        foreach (var kv in rowCounts)
        {
            int rowY = kv.Key;
            int count = kv.Value;
            if (count == Board.GridWidth)
                SpawnRow(boardRect, rowY, tint);
        }

        // Spawn col VFX where a full col (GridHeight) was cleared
        foreach (var kv in colCounts)
        {
            int colX = kv.Key;
            int count = kv.Value;
            if (count == Board.GridHeight)
                SpawnCol(boardRect, colX, tint);
        }

        // End of this resolution - keep the color until next placement (so any extra clear callbacks in the same resolution still match).
        // When the next piece is placed, HandleCellsPlaced will update it automatically.
    }

    private void SpawnRow(RectTransform boardRect, int y, Color tint)
    {
        if (horizontalVfxPrefab == null) return;

        Transform parent = (vfxParent != null) ? vfxParent : boardRect;
        Vector2 cell = screenToGrid.CellSize;
        Vector2 boardSize = boardRect.rect.size;

        // Board-local position of the row center
        Vector2 boardLocal = new Vector2(
            0f,
            (y + 0.5f) * cell.y - boardSize.y * 0.5f
        );

        SpawnAsIs(horizontalVfxPrefab, boardRect, parent, boardLocal, tint);
    }

    private void SpawnCol(RectTransform boardRect, int x, Color tint)
    {
        if (verticalVfxPrefab == null) return;

        Transform parent = (vfxParent != null) ? vfxParent : boardRect;
        Vector2 cell = screenToGrid.CellSize;
        Vector2 boardSize = boardRect.rect.size;

        // Board-local position of the col center
        Vector2 boardLocal = new Vector2(
            (x + 0.5f) * cell.x - boardSize.x * 0.5f,
            0f
        );

        SpawnAsIs(verticalVfxPrefab, boardRect, parent, boardLocal, tint);
    }

    /// <summary>
    /// Instantiate prefab without touching its scale/rotation. Only position + tint.
    /// </summary>
    private void SpawnAsIs(GameObject prefab, RectTransform boardRect, Transform parent, Vector2 boardLocalPos, Color tint)
    {

        // Convert board-local -> world -> parent-local
        Vector3 world = boardRect.TransformPoint(new Vector3(boardLocalPos.x, boardLocalPos.y, 0f));
        Vector3 parentLocal = parent.InverseTransformPoint(world);

        GameObject go = Instantiate(prefab, parent, false);

        // Only position: keep prefab scale/rotation as authored
        go.transform.localPosition = parentLocal;

        // Force VFX layer so VfxCamera can render it
        int layer = LayerMask.NameToLayer(vfxLayerName);
        if (layer >= 0) SetLayerRecursively(go.transform, layer);

        // Apply color to anything that could be rendering (particles/sprites/lines/material props)
        ApplyTint(go, tint);

        float life = EstimateMaxLifetime(go) + destroyPadding;
        Destroy(go, Mathf.Max(0.1f, life));
    }

    private static void SetLayerRecursively(Transform t, int layer)
    {
        t.gameObject.layer = layer;
        for (int i = 0; i < t.childCount; i++)
            SetLayerRecursively(t.GetChild(i), layer);
    }

    /// <summary>
    /// Force-tint:
    /// - ParticleSystem startColor
    /// - ColorOverLifetime / Trails if enabled
    /// - SpriteRenderer / LineRenderer / TrailRenderer
    /// - MaterialPropertyBlock for common color properties (_BaseColor/_Color/_TintColor)
    /// Also forces particles to Play (even if Play On Awake is off).
    /// </summary>
    private static void ApplyTint(GameObject root, Color color)
    {
        if (root == null) return;

        // 1) Particle Systems
        var pss = root.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < pss.Length; i++)
        {
            var ps = pss[i];

            var main = ps.main;
            main.startColor = color;

            var col = ps.colorOverLifetime;
            if (col.enabled)
                col.color = new ParticleSystem.MinMaxGradient(color);

            var trails = ps.trails;
            if (trails.enabled)
                trails.colorOverLifetime = new ParticleSystem.MinMaxGradient(color);

            ps.Clear(true);
            ps.Play(true);
        }

        // 2) Non-particle renderers often used for borders
        var srs = root.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < srs.Length; i++)
            srs[i].color = color;

        var lrs = root.GetComponentsInChildren<LineRenderer>(true);
        for (int i = 0; i < lrs.Length; i++)
        {
            lrs[i].startColor = color;
            lrs[i].endColor = color;
        }

        var trs = root.GetComponentsInChildren<TrailRenderer>(true);
        for (int i = 0; i < trs.Length; i++)
        {
            trs[i].startColor = color;
            trs[i].endColor = color;
        }

        // 3) Material properties (for shaders that ignore vertex colors)
        var mpb = new MaterialPropertyBlock();
        var renderers = root.GetComponentsInChildren<Renderer>(true);

        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            var mat = r.sharedMaterial;
            if (mat == null) continue;

            bool changed = false;
            mpb.Clear();
            r.GetPropertyBlock(mpb);

            if (mat.HasProperty(_BaseColor)) { mpb.SetColor(_BaseColor, color); changed = true; }
            if (mat.HasProperty(_Color)) { mpb.SetColor(_Color, color); changed = true; }
            if (mat.HasProperty(_TintColor)) { mpb.SetColor(_TintColor, color); changed = true; }

            if (changed) r.SetPropertyBlock(mpb);
        }
    }

    private static float EstimateMaxLifetime(GameObject root)
    {
        float max = 0f;
        var systems = root.GetComponentsInChildren<ParticleSystem>(true);

        for (int i = 0; i < systems.Length; i++)
        {
            var main = systems[i].main;
            float delay = GetMaxFromCurve(main.startDelay);
            float startLife = GetMaxFromCurve(main.startLifetime);
            float t = delay + main.duration + startLife;
            if (t > max) max = t;
        }

        return max;
    }

    private static float GetMaxFromCurve(ParticleSystem.MinMaxCurve curve)
    {
        switch (curve.mode)
        {
            case ParticleSystemCurveMode.Constant:
                return curve.constant;
            case ParticleSystemCurveMode.TwoConstants:
                return curve.constantMax;
            default:
                return Mathf.Max(0.0f, curve.curveMultiplier);
        }
    }
}
