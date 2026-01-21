using UnityEngine;
using UnityEngine.UI;

public sealed class TileView : MonoBehaviour
{
    private RectTransform rect;
    private Image image;

    private Sprite defaultSprite;
    private Material defaultMaterial;
    private Color defaultColor;

    private void Awake()
    {
        rect = (RectTransform)transform;
        image = GetComponent<Image>();
        if (image != null)
        {
            image.raycastTarget = false;
            defaultSprite = image.sprite;
            defaultMaterial = image.material;
            defaultColor = image.color;
        }
    }

    public void ApplyLayout(Vector2 anchoredPos, Vector2 size) // 타일이 시각적으로 중앙에 보이게 타일 고정용
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;
    }

    public void ApplyVisual(PieceDefinition piece) 
    {
        if (image == null) return;

        // 기존 타일 모양 덮어쓰게 잘 안풀려서 
        image.sprite = (piece != null && piece.tileSprite != null) ? piece.tileSprite : defaultSprite;
        image.color = (piece != null) ? piece.tileColor : defaultColor;
        image.material = (piece != null && piece.tileMaterial != null) ? piece.tileMaterial : defaultMaterial;
    }
}
