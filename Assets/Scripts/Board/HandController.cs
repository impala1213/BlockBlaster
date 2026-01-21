using UnityEngine;

public sealed class HandController : MonoBehaviour
{
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private RectTransform dragLayer;                       
    [SerializeField] private RectTransform[] slots = new RectTransform[3];  
    [SerializeField] private GameController game;
    [SerializeField] private ScreenToGrid screenToGrid;

    [Header("Piece Set")]
    [SerializeField] private PieceDefinition[] piecePool;

    private readonly PieceDragView[] active = new PieceDragView[3];
    private void Start()
    {
        SpawnInitialHand();
    }

    private void SpawnInitialHand() // 손패 3개 생성 우선은 하나 쓸때마다 바로 재생성
    {
        for (int i = 0; i < 3; i++)
        {
            if (slots == null || slots.Length <= i || slots[i] == null) continue;

            var go = new GameObject($"HandPiece_{i}", typeof(RectTransform), typeof(CanvasGroup));
            go.SetActive(false);

            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(slots[i], false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;

            var view = go.AddComponent<PieceDragView>();
            view.Initialize(game, screenToGrid, rootCanvas, dragLayer, slots[i]);

            view.OnPlaced += HandlePlaced;
            view.SetPiece(RandomPiece());

            go.SetActive(true);
            go.transform.SetAsLastSibling();

            active[i] = view;
        }
    }

    private void HandlePlaced(PieceDragView view) // 정상 배치시 호출
    {
        if (view == null) return;
        view.SetPiece(RandomPiece()); //배치 성공한 슬롯에 바로 재생성
    }

    private PieceDefinition RandomPiece() // piecePool안에서 랜덤 선택
    {
        if (piecePool == null || piecePool.Length == 0) return null;
        return piecePool[Random.Range(0, piecePool.Length)];
    }
}
