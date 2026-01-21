using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fundamental fix for pooling:
/// - Always write sprite/material/color deterministically.
/// - If piece has null sprite/material, reset to the prefab defaults instead of leaving stale values.
/// </summary>
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

    public void ApplyLayout(Vector2 anchoredPos, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;
    }

    public void ApplyVisual(PieceDefinition piece)
    {
        if (image == null) return;

        image.sprite = (piece != null && piece.tileSprite != null) ? piece.tileSprite : defaultSprite;
        image.material = (piece != null && piece.tileMaterial != null) ? piece.tileMaterial : defaultMaterial;
        image.color = (piece != null) ? piece.tileColor : defaultColor;
    }
}
