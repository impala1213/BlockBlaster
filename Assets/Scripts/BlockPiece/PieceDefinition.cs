using UnityEngine;

[CreateAssetMenu(menuName = "BlockBlast/Piece Definition", fileName = "Piece_")]
public sealed class PieceDefinition : ScriptableObject
{
    public string pieceId;

    [Header("Shape blocks offsets")]
    public Vector2Int[] blocks; // 블록의 좌표 예시로 가로 3칸 블록은 (0,0) (1,0) (2,0)

    [Header("Drag anchor (must be one of blocks)")]
    public Vector2Int dragAnchor = Vector2Int.zero; // drag후 부착시 drag 기준점이 될 점. 3칸에서 가운데를 기준으로 하고싶으면 (1,0)

    [Header("Visual (for BOTH drag + placed tiles)")]
    public Sprite tileSprite;            
    public Color tileColor = Color.white;  
    public Material tileMaterial;          

    public bool IsValid() // 블록 유효한지 검증
    {
        if (blocks == null || blocks.Length == 0) return false;

        for (int i = 0; i < blocks.Length; i++)
        {
            if (blocks[i] == dragAnchor)
                return true;
        }
        return false;
    }
}
