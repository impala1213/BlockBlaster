using TMPro;
using UnityEngine;
using UnityEngine.UI;
public sealed class GameOverView : MonoBehaviour
{
    [Header("Root (optional)")]
    [SerializeField] private GameObject root;

    [Header("Texts")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text bestText;

    [SerializeField] private Text scoreTextLegacy;
    [SerializeField] private Text bestTextLegacy;

    [Header("Behavior")]
    [SerializeField] private bool hideOnAwake = true;

    private bool initialized;

    private void Awake()
    {
        EnsureInitialized();

        if (hideOnAwake)
            Hide();
    }

    public void Show(int score, int bestScore)
    {
        EnsureInitialized();
        if (root != null)
        {
            if (!root.activeSelf)
                root.SetActive(true);

            BringToFront(root.transform);

            var cg = root.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
        }

        SetTexts(score, bestScore);
    }

    public void Hide()
    {
        EnsureInitialized();

        if (root != null && root.activeSelf)
            root.SetActive(false);
    }

    private void EnsureInitialized()
    {
        if (initialized) return;
        initialized = true;

        if (root == null) root = gameObject;

        if (scoreText == null && scoreTextLegacy == null)
            CacheTextByName("GameOverScore", out scoreText, out scoreTextLegacy);

        if (bestText == null && bestTextLegacy == null)
            CacheTextByName("GameOverHScore", out bestText, out bestTextLegacy);
    }

    private void SetTexts(int score, int bestScore)
    {
        if (scoreText != null) scoreText.text = $"{score}";
        if (bestText != null) bestText.text = $"{bestScore}";

        if (scoreTextLegacy != null) scoreTextLegacy.text = $"{score}";
        if (bestTextLegacy != null) bestTextLegacy.text = $"{bestScore}";
    }

    private void CacheTextByName(string childName, out TMP_Text tmp, out Text legacy)
    {
        tmp = null;
        legacy = null;

        Transform t = transform.Find(childName);
        if (t == null)
        {
            var all = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].name == childName)
                {
                    t = all[i];
                    break;
                }
            }
        }

        if (t == null) return;

        tmp = t.GetComponent<TMP_Text>();
        if (tmp != null) return;

        legacy = t.GetComponent<Text>();
    }

    private void BringToFront(Transform t)
    {
        if (t == null) return;

        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
            canvas = canvas.rootCanvas;

        if (canvas != null)
        {
            Transform top = t;
            while (top.parent != null && top.parent != canvas.transform)
                top = top.parent;

            top.SetAsLastSibling();

            //이유는 모르겠지만 TileLayer에 가려진다..그래서 우선순위 그냥 높여버렸음
            var nested = top.GetComponent<Canvas>();
            if (nested == null)
                nested = top.gameObject.AddComponent<Canvas>();

            nested.overrideSorting = true;
            nested.sortingOrder = 5;

            if (top.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
                top.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        else
        {
            t.SetAsLastSibling();
        }
    }
}
