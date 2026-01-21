using UnityEngine;

[CreateAssetMenu(menuName = "BlockBlast/Piece Definition", fileName = "Piece_")]
public sealed class PieceDefinition : ScriptableObject
{
    public string pieceId;

    [Header("Shape blocks offsets")]
    public Vector2Int[] blocks;

    [Header("Drag anchor (must be one of blocks)")]
    public Vector2Int dragAnchor = Vector2Int.zero;

    [Header("Visual (for BOTH drag + placed tiles)")]
    public Sprite tileSprite;              // 보드에 박히는 타일 스프라이트
    public Color tileColor = Color.white;  // 보드에 박히는 타일 색
    public Material tileMaterial;          // 선택(특수 효과 있으면)

    public bool IsValid()
    {
        if (blocks == null || blocks.Length == 0) return false;

        bool anchorFound = false;
        for (int i = 0; i < blocks.Length; i++)
        {
            if (blocks[i] == dragAnchor) anchorFound = true;
        }
        return anchorFound;
    }
}
