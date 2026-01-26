using System.Collections.Generic;
using UnityEngine;

public sealed class LineClearVfx : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameController game;
    [SerializeField] private ScreenToGrid screenToGrid;

    [Header("Prefabs (as-is)")]
    [SerializeField] private GameObject horizontalVfxPrefab; // 가로 줄
    [SerializeField] private GameObject verticalVfxPrefab;   // 세로 줄

    [Header("Spawn Parent (IMPORTANT: set this to where dragging prefab fits perfectly)")]
    [SerializeField] private Transform vfxParent;

    [Header("Auto Destroy")]
    [SerializeField] private float destroyPadding = 0.25f;

    private Color pendingColor = Color.white;

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

    private void HandleCellsPlaced(PieceDefinition piece, List<Vector2Int> placedCells)
    {
        // “이번 턴에 놓은 블록 색” 저장
        pendingColor = (piece != null) ? piece.tileColor : Color.white;
    }

    private void HandleCellsCleared(List<Vector2Int> clearedCells)
    {
        if (clearedCells == null || clearedCells.Count == 0) return;
        if (screenToGrid == null || screenToGrid.BoardRect == null) return;

        RectTransform boardRect = screenToGrid.BoardRect;

        Dictionary<int, int> rowCounts = new Dictionary<int, int>();
        Dictionary<int, int> colCounts = new Dictionary<int, int>();

        for (int i = 0; i < clearedCells.Count; i++)
        {
            int x = clearedCells[i].x;
            int y = clearedCells[i].y;

            rowCounts[y] = rowCounts.TryGetValue(y, out int rc) ? rc + 1 : 1;
            colCounts[x] = colCounts.TryGetValue(x, out int cc) ? cc + 1 : 1;
        }

        foreach (var kv in rowCounts)
        {
            if (kv.Value == Board.GridWidth)
                SpawnRow(boardRect, kv.Key);
        }

        foreach (var kv in colCounts)
        {
            if (kv.Value == Board.GridHeight)
                SpawnCol(boardRect, kv.Key);
        }
    }

    private void SpawnRow(RectTransform boardRect, int y)
    {
        if (horizontalVfxPrefab == null) return;

        Transform parent = (vfxParent != null) ? vfxParent : boardRect;
        Vector2 cell = screenToGrid.CellSize;
        Vector2 boardSize = boardRect.rect.size;

        // 해당 행 중앙
        Vector2 boardLocal = new Vector2(
            0f,
            (y + 0.5f) * cell.y - boardSize.y * 0.5f
        );

        SpawnAsIs(horizontalVfxPrefab, boardRect, parent, boardLocal);
    }

    private void SpawnCol(RectTransform boardRect, int x)
    {
        if (verticalVfxPrefab == null) return;

        Transform parent = (vfxParent != null) ? vfxParent : boardRect;
        Vector2 cell = screenToGrid.CellSize;
        Vector2 boardSize = boardRect.rect.size;

        // 해당 열 중앙
        Vector2 boardLocal = new Vector2(
            (x + 0.5f) * cell.x - boardSize.x * 0.5f,
            0f
        );

        SpawnAsIs(verticalVfxPrefab, boardRect, parent, boardLocal);
    }

    private void SpawnAsIs(GameObject prefab, RectTransform boardRect, Transform parent, Vector2 boardLocalPos)
    {
        Vector3 world = boardRect.TransformPoint(new Vector3(boardLocalPos.x, boardLocalPos.y, 0f));
        Vector3 parentLocal = parent.InverseTransformPoint(world);

        GameObject go = Instantiate(prefab, parent, false);

        go.transform.localPosition = parentLocal;

        var systems = go.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < systems.Length; i++)
        {
            var ps = systems[i];
            var main = ps.main;
            main.startColor = pendingColor;

            ps.Clear(true);
            ps.Play(true);
        }

        float life = EstimateMaxLifetime(go) + destroyPadding;
        Destroy(go, Mathf.Max(0.1f, life));
    }

    private static void SetLayerRecursively(Transform t, int layer)
    {
        t.gameObject.layer = layer;
        for (int i = 0; i < t.childCount; i++)
            SetLayerRecursively(t.GetChild(i), layer);
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
            case ParticleSystemCurveMode.Constant: return curve.constant;
            case ParticleSystemCurveMode.TwoConstants: return curve.constantMax;
            default: return Mathf.Max(0.0f, curve.curveMultiplier);
        }
    }
}
