using UnityEngine;
using UnityEngine.UI;

public sealed class HandController : MonoBehaviour
{
    [Header("Scene Refs")]
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private RectTransform dragLayer;                       // Full-stretch DragLayer
    [SerializeField] private RectTransform[] slots = new RectTransform[3];  // HandArea/Slot0..2
    [SerializeField] private GameController game;
    [SerializeField] private ScreenToGrid screenToGrid;

    [Header("Piece Pool")]
    [SerializeField] private PieceDefinition[] piecePool;

    private PieceDragView[] active = new PieceDragView[3];

    private void Awake()
    {
        if (rootCanvas == null) rootCanvas = GetComponentInParent<Canvas>();
        if (dragLayer == null && rootCanvas != null) dragLayer = rootCanvas.transform as RectTransform;

        if (game == null) game = FindObjectOfType<GameController>();
        if (screenToGrid == null) screenToGrid = FindObjectOfType<ScreenToGrid>();
    }

    private void Start()
    {
        Spawn3();
    }

    private void Spawn3()
    {
        for (int i = 0; i < 3; i++)
        {
            if (slots[i] == null) continue;

            // Runtime-only view (NO DragBlock prefab)
            var go = new GameObject($"HandPiece_{i}", typeof(RectTransform), typeof(CanvasGroup), typeof(LayoutElement));
            go.SetActive(false);

            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(slots[i], false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;

            // LayoutGroup 영향 완전 차단 (손패 슬롯/영역 내려가는 현상 근절)
            var le = go.GetComponent<LayoutElement>();
            le.ignoreLayout = true;

            var view = go.AddComponent<PieceDragView>();
            view.Initialize(game, screenToGrid, rootCanvas, dragLayer, slots[i]);

            view.OnPlaced += OnPlaced;
            view.SetPiece(RandomPiece());

            go.SetActive(true);

            // slot background 위에 보이게
            go.transform.SetAsLastSibling();

            active[i] = view;
        }
    }

    private void OnPlaced(PieceDragView view)
    {
        // placed 되면 즉시 그 슬롯 새 블록 지급
        view.SetPiece(RandomPiece());
    }

    private PieceDefinition RandomPiece()
    {
        if (piecePool == null || piecePool.Length == 0) return null;
        return piecePool[Random.Range(0, piecePool.Length)];
    }
}
