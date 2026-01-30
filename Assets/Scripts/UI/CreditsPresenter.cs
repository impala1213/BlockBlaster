using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum CreditsEndBehavior
{
    Stop = 0,
    LoadScene = 1
}

public enum CreditsStartPosition
{
    Top = 0,
    Bottom = 1
}

public class CreditsPresenter : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private CreditData creditData;
    [SerializeField] private string resourcesFallbackPath = "Scriptable/CreditData";

    [Header("UI References")]
    [SerializeField] private RectTransform scrollRoot;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private TMP_Text creditsText;

    [Header("Scroll")]
    [SerializeField] private float scrollSpeed = 30f;
    [SerializeField] private float startPadding = 40f;
    [SerializeField] private float endPadding = 80f;
    [SerializeField] private CreditsStartPosition startPosition = CreditsStartPosition.Top;
    [SerializeField] private float startDelay = 1f;
    [SerializeField] private int topBlankLines = 30;
    [SerializeField] private CreditsEndBehavior endBehavior = CreditsEndBehavior.Stop;
    [SerializeField] private string endSceneName = "Lobby";

    private float contentHeight;
    private float viewportHeight;
    private bool isScrolling;
    private float remainingDelay;

    private void OnEnable()
    {
        Initialize();
    }

    private void Update()
    {
        if (!isScrolling || content == null)
        {
            return;
        }

        if (remainingDelay > 0f)
        {
            remainingDelay -= Time.deltaTime;
            return;
        }

        Vector2 pos = content.anchoredPosition;
        pos.y += scrollSpeed * Time.deltaTime;
        content.anchoredPosition = pos;

        if (pos.y >= GetEndThreshold())
        {
            HandleEnd();
        }
    }

    private void Initialize()
    {
        if (!EnsureReferences())
        {
            enabled = false;
            return;
        }

        BuildCreditsText();
        ResizeContentToText();
        ResetScrollPosition();
    }

    private bool EnsureReferences()
    {
        if (creditsText == null)
        {
            creditsText = GetComponentInChildren<TMP_Text>(true);
        }

        if (scrollRect == null || content == null)
        {
            CreateScrollViewIfNeeded();
        }

        if (creditsText == null || scrollRect == null || content == null)
        {
            Debug.LogError("CreditsPresenter: UI references are missing.");
            return false;
        }

        if (scrollRect.viewport != null)
        {
            viewportHeight = scrollRect.viewport.rect.height;
        }
        else
        {
            viewportHeight = scrollRect.GetComponent<RectTransform>().rect.height;
        }

        return true;
    }

    private void CreateScrollViewIfNeeded()
    {
        if (creditsText == null)
        {
            return;
        }

        RectTransform sourceRect = creditsText.GetComponent<RectTransform>();
        Transform parent = scrollRoot != null ? scrollRoot : sourceRect.parent;

        GameObject scrollRootGo = new GameObject("CreditsScrollRect", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ScrollRect));
        scrollRootGo.transform.SetParent(parent, false);

        RectTransform scrollRectTransform = scrollRootGo.GetComponent<RectTransform>();
        CopyRectTransform(sourceRect, scrollRectTransform);

        Image background = scrollRootGo.GetComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0f);

        scrollRect = scrollRootGo.GetComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.inertia = false;

        GameObject viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
        viewportGo.transform.SetParent(scrollRootGo.transform, false);

        RectTransform viewportRect = viewportGo.GetComponent<RectTransform>();
        StretchRectTransform(viewportRect);

        Image viewportImage = viewportGo.GetComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0f);

        GameObject contentGo = new GameObject("Content", typeof(RectTransform));
        contentGo.transform.SetParent(viewportGo.transform, false);

        content = contentGo.GetComponent<RectTransform>();
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(0f, 600f);

        creditsText.transform.SetParent(contentGo.transform, false);
        ConfigureTextRect(creditsText.GetComponent<RectTransform>());

        scrollRect.viewport = viewportRect;
        scrollRect.content = content;
    }

    private void ConfigureTextRect(RectTransform rect)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0f, 0f);
    }

    private void BuildCreditsText()
    {
        if (creditData == null && !string.IsNullOrEmpty(resourcesFallbackPath))
        {
            creditData = Resources.Load<CreditData>(resourcesFallbackPath);
        }

        if (creditData == null)
        {
            creditsText.text = "No CreditData assigned.";
            return;
        }

        StringBuilder builder = new StringBuilder(512);

        int safeBlankLines = Mathf.Max(0, topBlankLines);
        for (int i = 0; i < safeBlankLines; i++)
        {
            builder.AppendLine();
        }

        foreach (CreditData.CreditSection section in creditData.sections)
        {
            if (!string.IsNullOrWhiteSpace(section.title))
            {
                builder.Append("<size=120%><b>");
                builder.Append(section.title);
                builder.Append("</b></size>");
                builder.AppendLine();
            }

            foreach (CreditData.TeamEntry entry in section.teamEntries)
            {
                if (entry == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(entry.role))
                {
                    builder.Append("- <b>");
                    builder.Append(entry.role);
                    builder.Append("</b>");
                    builder.AppendLine();
                }

                if (entry.names != null && entry.names.Count > 0)
                {
                    for (int i = 0; i < entry.names.Count; i++)
                    {
                        if (string.IsNullOrWhiteSpace(entry.names[i]))
                        {
                            continue;
                        }

                        builder.Append("  - ");
                        builder.Append(entry.names[i]);
                        builder.AppendLine();
                    }
                }

                builder.AppendLine();
            }

            foreach (CreditData.ResourceEntry entry in section.resourceEntries)
            {
                if (entry == null)
                {
                    continue;
                }

                builder.Append("- <b>");
                builder.Append(string.IsNullOrWhiteSpace(entry.assetName) ? "Resource" : entry.assetName);
                builder.Append("</b>");
                builder.AppendLine();

                AppendField(builder, "Creator", entry.creator);
                AppendField(builder, "Source", entry.source);
                AppendField(builder, "License", entry.license);
                AppendField(builder, "Link", entry.link);
                builder.AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(section.customLines))
            {
                string normalized = section.customLines.Replace("\r\n", "\n").TrimEnd();
                string[] lines = normalized.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    builder.AppendLine(lines[i]);
                }
            }

            builder.AppendLine();
        }

        creditsText.text = builder.ToString().TrimEnd();
    }

    private void AppendField(StringBuilder builder, string label, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        builder.Append("  ");
        builder.Append(label);
        builder.Append(": ");
        builder.Append(value);
        builder.AppendLine();
    }

    private void ResizeContentToText()
    {
        Canvas.ForceUpdateCanvases();
        creditsText.ForceMeshUpdate();
        contentHeight = Mathf.Max(creditsText.preferredHeight + startPadding + endPadding, viewportHeight);
        content.sizeDelta = new Vector2(content.sizeDelta.x, contentHeight);
    }

    private void ResetScrollPosition()
    {
        Vector2 pos = content.anchoredPosition;

        if (startPosition == CreditsStartPosition.Top)
        {
            pos.y = 0f;
        }
        else
        {
            pos.y = -viewportHeight - startPadding;
        }

        content.anchoredPosition = pos;

        remainingDelay = Mathf.Max(0f, startDelay);
        isScrolling = scrollSpeed > 0f && contentHeight > viewportHeight;
    }

    private void HandleEnd()
    {
        isScrolling = false;

        if (endBehavior == CreditsEndBehavior.Stop)
        {
            return;
        }

        if (!string.IsNullOrEmpty(endSceneName))
        {
            if (endSceneName == "Lobby" && SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadLobbyScene();
                return;
            }

            SceneManager.LoadScene(endSceneName);
        }
    }

    private float GetEndThreshold()
    {
        float scrollable = Mathf.Max(0f, contentHeight - viewportHeight);
        return scrollable / 2f;
    }

    private void CopyRectTransform(RectTransform source, RectTransform target)
    {
        target.anchorMin = source.anchorMin;
        target.anchorMax = source.anchorMax;
        target.pivot = source.pivot;
        target.anchoredPosition = source.anchoredPosition;
        target.sizeDelta = source.sizeDelta;
        target.localRotation = source.localRotation;
        target.localScale = source.localScale;
    }

    private void StretchRectTransform(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
    }
}
