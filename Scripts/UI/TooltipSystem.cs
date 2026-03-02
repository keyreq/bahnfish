using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Manages hover tooltips for UI elements and in-game objects.
/// Shows item information, fish details, button descriptions, etc.
/// Follows mouse cursor with configurable offset.
/// Fade-in delay prevents tooltip spam.
/// </summary>
public class TooltipSystem : MonoBehaviour
{
    #region Singleton
    private static TooltipSystem _instance;
    public static TooltipSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<TooltipSystem>();
            }
            return _instance;
        }
    }
    #endregion

    #region Inspector References
    [Header("Tooltip UI")]
    [Tooltip("Main tooltip container")]
    public GameObject tooltipContainer;

    [Tooltip("Text component for tooltip content")]
    public TextMeshProUGUI tooltipText;

    [Tooltip("Background image")]
    public Image tooltipBackground;

    [Tooltip("Optional header text")]
    public TextMeshProUGUI tooltipHeader;

    [Header("Layout Settings")]
    [Tooltip("Padding around text")]
    public float padding = 10f;

    [Tooltip("Minimum width of tooltip")]
    public float minWidth = 100f;

    [Tooltip("Maximum width of tooltip")]
    public float maxWidth = 400f;

    [Header("Position Settings")]
    [Tooltip("Offset from cursor")]
    public Vector2 cursorOffset = new Vector2(20f, -20f);

    [Tooltip("Keep tooltip on screen")]
    public bool clampToScreen = true;

    [Tooltip("Screen edge padding")]
    public float screenPadding = 10f;

    [Header("Animation")]
    [Tooltip("Delay before showing tooltip")]
    [Range(0f, 2f)]
    public float showDelay = 0.5f;

    [Tooltip("Fade in duration")]
    [Range(0f, 1f)]
    public float fadeInDuration = 0.2f;

    [Tooltip("Fade out duration")]
    [Range(0f, 1f)]
    public float fadeOutDuration = 0.15f;

    [Header("Styling")]
    [Tooltip("Background color")]
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);

    [Tooltip("Text color")]
    public Color textColor = Color.white;

    [Tooltip("Header color")]
    public Color headerColor = new Color(1f, 0.9f, 0.5f);

    [Header("Debug")]
    [Tooltip("Enable debug logging")]
    public bool enableDebugLogging = false;
    #endregion

    #region Private Variables
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Canvas canvas;
    private bool isVisible = false;
    private Coroutine showCoroutine;
    private Coroutine fadeCoroutine;
    private string currentTooltipText = "";
    private string currentHeaderText = "";
    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        // Get components
        canvas = GetComponentInParent<Canvas>();
        rectTransform = tooltipContainer.GetComponent<RectTransform>();
        canvasGroup = tooltipContainer.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = tooltipContainer.AddComponent<CanvasGroup>();
        }

        // Apply initial styling
        ApplyStyling();
    }

    private void Start()
    {
        // Hide tooltip initially
        HideTooltipImmediate();

        if (enableDebugLogging)
        {
            Debug.Log("[TooltipSystem] Initialized");
        }
    }

    private void Update()
    {
        // Update tooltip position to follow cursor
        if (isVisible)
        {
            UpdateTooltipPosition();
        }
    }

    #endregion

    #region Styling

    /// <summary>
    /// Apply styling to tooltip elements
    /// </summary>
    private void ApplyStyling()
    {
        if (tooltipBackground != null)
        {
            tooltipBackground.color = backgroundColor;
        }

        if (tooltipText != null)
        {
            tooltipText.color = textColor;
        }

        if (tooltipHeader != null)
        {
            tooltipHeader.color = headerColor;
        }
    }

    #endregion

    #region Public Interface

    /// <summary>
    /// Show tooltip with text at cursor position
    /// </summary>
    public void ShowTooltip(string text)
    {
        ShowTooltip(text, "");
    }

    /// <summary>
    /// Show tooltip with header and text
    /// </summary>
    public void ShowTooltip(string text, string header)
    {
        if (string.IsNullOrEmpty(text))
        {
            HideTooltip();
            return;
        }

        currentTooltipText = text;
        currentHeaderText = header;

        // Cancel any existing show coroutine
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
        }

        // Start show with delay
        showCoroutine = StartCoroutine(ShowTooltipDelayed());

        if (enableDebugLogging)
        {
            Debug.Log($"[TooltipSystem] Showing tooltip: {text}");
        }
    }

    /// <summary>
    /// Show tooltip for a fish
    /// </summary>
    public void ShowFishTooltip(Fish fish)
    {
        if (fish == null)
        {
            HideTooltip();
            return;
        }

        string header = fish.name;
        string text = $"Value: ${fish.baseValue:F0}\n";
        text += $"Rarity: {fish.rarity}\n";
        text += $"Size: {fish.inventorySize.x}x{fish.inventorySize.y}\n";

        if (!string.IsNullOrEmpty(fish.description))
        {
            text += $"\n{fish.description}";
        }

        ShowTooltip(text, header);
    }

    /// <summary>
    /// Show tooltip at specific position (not following cursor)
    /// </summary>
    public void ShowTooltipAtPosition(string text, Vector2 position)
    {
        ShowTooltip(text);
        // Override position
        if (rectTransform != null)
        {
            rectTransform.position = position;
        }
    }

    /// <summary>
    /// Hide tooltip
    /// </summary>
    public void HideTooltip()
    {
        // Cancel show coroutine if running
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
            showCoroutine = null;
        }

        // Start fade out
        if (isVisible)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeOut());
        }

        if (enableDebugLogging)
        {
            Debug.Log("[TooltipSystem] Hiding tooltip");
        }
    }

    /// <summary>
    /// Hide tooltip immediately without animation
    /// </summary>
    public void HideTooltipImmediate()
    {
        isVisible = false;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        if (tooltipContainer != null)
        {
            tooltipContainer.SetActive(false);
        }
    }

    /// <summary>
    /// Check if tooltip is currently visible
    /// </summary>
    public bool IsVisible()
    {
        return isVisible;
    }

    #endregion

    #region Display

    /// <summary>
    /// Show tooltip after delay
    /// </summary>
    private IEnumerator ShowTooltipDelayed()
    {
        yield return new WaitForSeconds(showDelay);

        // Update content
        UpdateTooltipContent();

        // Update size
        UpdateTooltipSize();

        // Update position
        UpdateTooltipPosition();

        // Activate and fade in
        tooltipContainer.SetActive(true);
        isVisible = true;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeIn());

        showCoroutine = null;
    }

    /// <summary>
    /// Update tooltip content
    /// </summary>
    private void UpdateTooltipContent()
    {
        if (tooltipText != null)
        {
            tooltipText.text = currentTooltipText;
        }

        if (tooltipHeader != null)
        {
            if (!string.IsNullOrEmpty(currentHeaderText))
            {
                tooltipHeader.text = currentHeaderText;
                tooltipHeader.gameObject.SetActive(true);
            }
            else
            {
                tooltipHeader.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Update tooltip size based on content
    /// </summary>
    private void UpdateTooltipSize()
    {
        if (rectTransform == null)
            return;

        // Force text to update preferred size
        if (tooltipText != null)
        {
            tooltipText.ForceMeshUpdate();
        }

        // Calculate size based on text
        float width = Mathf.Clamp(tooltipText.preferredWidth + padding * 2, minWidth, maxWidth);
        float height = tooltipText.preferredHeight + padding * 2;

        // Add header height if present
        if (tooltipHeader != null && tooltipHeader.gameObject.activeSelf)
        {
            tooltipHeader.ForceMeshUpdate();
            height += tooltipHeader.preferredHeight + padding;
        }

        rectTransform.sizeDelta = new Vector2(width, height);
    }

    /// <summary>
    /// Update tooltip position to follow cursor
    /// </summary>
    private void UpdateTooltipPosition()
    {
        if (rectTransform == null || canvas == null)
            return;

        // Get mouse position in canvas space
        Vector2 mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            canvas.worldCamera,
            out mousePosition
        );

        // Apply offset
        Vector2 targetPosition = mousePosition + cursorOffset;

        // Clamp to screen if enabled
        if (clampToScreen)
        {
            targetPosition = ClampToScreen(targetPosition);
        }

        rectTransform.anchoredPosition = targetPosition;
    }

    /// <summary>
    /// Clamp tooltip position to screen bounds
    /// </summary>
    private Vector2 ClampToScreen(Vector2 position)
    {
        if (canvas == null || rectTransform == null)
            return position;

        RectTransform canvasRect = canvas.transform as RectTransform;
        Vector2 canvasSize = canvasRect.sizeDelta;

        // Get tooltip size
        Vector2 tooltipSize = rectTransform.sizeDelta;

        // Calculate bounds
        float minX = -canvasSize.x / 2 + screenPadding;
        float maxX = canvasSize.x / 2 - tooltipSize.x - screenPadding;
        float minY = -canvasSize.y / 2 + tooltipSize.y + screenPadding;
        float maxY = canvasSize.y / 2 - screenPadding;

        // Clamp position
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);

        return position;
    }

    #endregion

    #region Animation

    /// <summary>
    /// Fade in tooltip
    /// </summary>
    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = elapsed / fadeInDuration;
            yield return null;
        }
        canvasGroup.alpha = 1f;
        fadeCoroutine = null;
    }

    /// <summary>
    /// Fade out tooltip
    /// </summary>
    private IEnumerator FadeOut()
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        isVisible = false;
        tooltipContainer.SetActive(false);
        fadeCoroutine = null;
    }

    #endregion

    #region Debug Methods

    /// <summary>
    /// Test tooltip with sample text
    /// </summary>
    [ContextMenu("Test Simple Tooltip")]
    private void TestSimpleTooltip()
    {
        ShowTooltip("This is a test tooltip!");
    }

    /// <summary>
    /// Test tooltip with header
    /// </summary>
    [ContextMenu("Test Tooltip with Header")]
    private void TestTooltipWithHeader()
    {
        ShowTooltip("This tooltip has a header and body text.", "Test Header");
    }

    /// <summary>
    /// Test long tooltip
    /// </summary>
    [ContextMenu("Test Long Tooltip")]
    private void TestLongTooltip()
    {
        string longText = "This is a very long tooltip that should wrap to multiple lines. " +
                         "It contains a lot of text to test the width clamping and wrapping behavior.";
        ShowTooltip(longText, "Long Tooltip Test");
    }

    /// <summary>
    /// Test fish tooltip
    /// </summary>
    [ContextMenu("Test Fish Tooltip")]
    private void TestFishTooltip()
    {
        Fish testFish = new Fish
        {
            name = "Largemouth Bass",
            baseValue = 25f,
            rarity = FishRarity.Common,
            inventorySize = new Vector2Int(2, 1),
            description = "A common freshwater fish found in lakes and rivers."
        };
        ShowFishTooltip(testFish);
    }

    /// <summary>
    /// Test hide
    /// </summary>
    [ContextMenu("Hide Tooltip")]
    private void TestHideTooltip()
    {
        HideTooltip();
    }

    #endregion
}
