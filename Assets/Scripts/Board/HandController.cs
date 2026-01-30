using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class HandController : MonoBehaviour
{
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private RectTransform dragLayer;                       
    [SerializeField] private RectTransform[] slots = new RectTransform[3];  
    [SerializeField] private GameController game;
    [SerializeField] private ScreenToGrid screenToGrid;

    [Header("Piece Set")]
    [SerializeField] private PieceDefinition[] piecePool;

    [Header("Game Over UI")]
    [Tooltip("씬에 있는 GameOver 부모 오브젝트(스크립트: GameOverView)를 연결하세요. 비어있으면 자동 탐색합니다.")]
    [SerializeField] private GameOverView gameOverView;

    [Tooltip("게임오버 UI 표시 후 로비로 돌아가기까지 대기 시간(초).")]
    [SerializeField] private float returnToLobbyDelay = 3f;

    [Tooltip("SceneLoader가 없을 때 사용할 로비 씬 이름.")]
    [SerializeField] private string lobbySceneName = "Lobby";

    private readonly PieceDragView[] active = new PieceDragView[3];

    private bool isGameOver;

    private void Start()
    {
        SpawnInitialHand();

        if (gameOverView == null)
            gameOverView = FindObjectOfType<GameOverView>(true);

        // (예외) 시작부터 손패가 전부 막혀있는 경우 즉시 게임오버
        if (!HasAnyMove())
            TriggerGameOver();
    }

    private void SpawnInitialHand() // 손패 3개 생성. 배치 성공 시 해당 슬롯은 바로 재생성
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

    private void HandlePlaced(PieceDragView view) // 정상 배치 시 호출
    {
        if (isGameOver) return;
        if (view == null) return;

        // 배치 성공한 슬롯은 바로 재생성
        view.SetPiece(RandomPiece());

        // 손패 3개 모두 더 이상 둘 곳 없으면 게임오버
        if (!HasAnyMove())
            TriggerGameOver();
    }

    private bool HasAnyMove()
    {
        if (game == null) return false;

        for (int i = 0; i < active.Length; i++)
        {
            var v = active[i];
            if (v == null) continue;

            var p = v.CurrentPiece;
            if (p == null || !p.IsValid()) continue;

            if (CanPlaceAnywhere(p))
                return true;
        }

        return false;
    }

    private bool CanPlaceAnywhere(PieceDefinition piece)
    {
        if (game == null || piece == null || !piece.IsValid()) return false;

        // piece.blocks 기준으로 origin 탐색 범위를 계산해서 불필요한 검사 줄이기
        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;

        for (int i = 0; i < piece.blocks.Length; i++)
        {
            Vector2Int b = piece.blocks[i];
            if (b.x < minX) minX = b.x;
            if (b.y < minY) minY = b.y;
            if (b.x > maxX) maxX = b.x;
            if (b.y > maxY) maxY = b.y;
        }

        int startX = -minX;
        int endX = Board.GridWidth - 1 - maxX;
        int startY = -minY;
        int endY = Board.GridHeight - 1 - maxY;

        for (int y = startY; y <= endY; y++)
        {
            for (int x = startX; x <= endX; x++)
            {
                if (game.CanPlace(piece, new Vector2Int(x, y)))
                    return true;
            }
        }

        return false;
    }

    private void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        // 손패 드래그 막기
        for (int i = 0; i < active.Length; i++)
        {
            if (active[i] != null)
            {
                active[i].enabled = false;

                // 드래그 뷰 자체가 UI 위에 떠서 GameOver를 가릴 수 있으니,
                // 게임오버 순간에는 손패 블록을 화면에서 숨깁니다.
                if (active[i].gameObject.activeSelf)
                    active[i].gameObject.SetActive(false);
            }
        }

        // 드래그 레이어가 따로 있다면 같이 숨김 (블록 프리뷰/드래그 잔상이 남는 경우 대비)
        if (dragLayer != null && dragLayer.gameObject.activeSelf)
            dragLayer.gameObject.SetActive(false);

        // GameOver UI가 가려지지 않도록 배치된 타일(타일레이어) 숨김
        ClearPlacedTileLayer();
        if (gameOverView == null)
            gameOverView = FindObjectOfType<GameOverView>(true);

        if (gameOverView != null && game != null)
            gameOverView.Show(game.Score, game.BestScore);
        else
            Debug.LogWarning("[HandController] GameOverView를 찾지 못했거나 GameController 참조가 없습니다.");

        StartCoroutine(ReturnToLobbyRoutine());
    }

    private IEnumerator ReturnToLobbyRoutine()
    {
        yield return new WaitForSeconds(returnToLobbyDelay);

        // SceneLoader가 있으면 페이드 포함 로딩
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadLobbyScene();
        }
        else
        {
            SceneManager.LoadScene(lobbySceneName);
        }
    }


    private void ClearPlacedTileLayer()
    {
        // GameOver UI가 보드 타일(placed tiles)에 가려지는 경우가 있어,
        // 먼저 TileLayer(배치된 타일) 쪽을 비웁니다.

        // 1) BoardView가 있으면 안전하게 전체 타일 비우기
        var boardView = FindObjectOfType<BoardView>(true);
        if (boardView != null)
        {
            boardView.ClearAllVisuals();
            return;
        }

        // 2) fallback: 이름이 TileLayer인 오브젝트를 찾아 비활성화
        var tileLayerGo = GameObject.Find("TileLayer");
        if (tileLayerGo != null)
        {
            tileLayerGo.SetActive(false);
            return;
        }

        // 3) 또 다른 fallback: RectTransform 중 이름이 tileLayer인 것 탐색
        var rects = FindObjectsOfType<RectTransform>(true);
        for (int i = 0; i < rects.Length; i++)
        {
            var r = rects[i];
            if (r != null && (r.name == "tileLayer" || r.name == "Tile Layer"))
            {
                r.gameObject.SetActive(false);
                break;
            }
        }
    }
    private PieceDefinition RandomPiece() // piecePool 안에서 랜덤 선택
    {
        if (piecePool == null || piecePool.Length == 0) return null;
        return piecePool[Random.Range(0, piecePool.Length)];
    }
}
