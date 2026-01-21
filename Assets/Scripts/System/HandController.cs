using UnityEngine;

/// <summary>
/// Spawns and refills the 3 hand pieces.
/// Fundamental fix:
/// - Spawn pieces as children of their Slot RectTransforms (not in DragLayer).
/// - Pieces follow layout automatically; no "Start-time wrong slot position" problem.
/// - PieceDragView reparents itself to DragLayer only while dragging.
/// </summary>
public sealed class HandController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private RectTransform dragLayer;                       // Full-stretch DragLayer
    [SerializeField] private RectTransform[] slots = new RectTransform[3];  // HandArea/Slot0..2

    [Header("Prefab")]
    [SerializeField] private PieceDragView dragPiecePrefab;

    [Header("Piece Pool")]
    [SerializeField] private PieceDefinition[] piecePool;

    private void Start()
    {
        Spawn3();
    }

    private void Spawn3()
    {
        for (int i = 0; i < 3; i++)
        {
            if (slots[i] == null) continue;

            // Spawn under the slot so it inherits slot position/size from the layout system.
            var view = Instantiate(dragPiecePrefab, slots[i], false);

            // Bind the home slot for reparenting during drag.
            view.SetHomeSlot(slots[i]);

            // Fill
            view.SetPiece(RandomPiece());

            // Refill on placement
            view.OnPlaced += OnPlaced;

            // Ensure it's visible above slot background
            view.transform.SetAsLastSibling();
        }
    }

    private void OnPlaced(PieceDragView view)
    {
        // Block Blast rule: when one piece is placed, immediately refill that hand slot.
        view.SetPiece(RandomPiece());
    }

    private PieceDefinition RandomPiece()
    {
        if (piecePool == null || piecePool.Length == 0) return null;
        return piecePool[Random.Range(0, piecePool.Length)];
    }
}
