using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Dynamic UI scaling system.
/// Features:
/// - Scales all UI elements based on accessibility settings
/// - Font size adjustment
/// - Icon size scaling
/// - Maintains aspect ratios
/// - Ensures text readability at all scales
/// - Works with Unity's Canvas Scaler
/// </summary>
public class UIScaler : MonoBehaviour
{
    private static UIScaler _instance;
    public static UIScaler Instance => _instance;

    [Header("References")]
    [SerializeField] private Canvas[] canvases;
    [SerializeField] private bool autoFindCanvases = true;

    [Header("Base Settings")]
    [SerializeField] private float baseUIScale = 1f;
    [SerializeField] private int baseFontSize = 16;
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920, 1080);

    // Tracked UI elements
    private Dictionary<Text, int> textOriginalSizes = new Dictionary<Text, int>();
    private Dictionary<Image, Vector2> imageOriginalSizes = new Dictionary<Image, Vector2>();

    private float currentUIScale = 1f;
    private int currentFontSize = 16;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        if (autoFindCanvases)
        {
            FindAllCanvases();
        }

        CacheUIElements();
    }

    private void Start()
    {
        // Subscribe to settings events
        EventSystem.Subscribe<float>("SetUIScale", OnSetUIScale);
        EventSystem.Subscribe<int>("SetFontSize", OnSetFontSize);

        // Apply initial settings
        if (SettingsManager.Instance != null && SettingsManager.Instance.IsInitialized)
        {
            ApplyCurrentSettings();
        }
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<float>("SetUIScale", OnSetUIScale);
        EventSystem.Unsubscribe<int>("SetFontSize", OnSetFontSize);

        if (_instance == this)
        {
            _instance = null;
        }
    }

    /// <summary>
    /// Find all canvases in the scene.
    /// </summary>
    private void FindAllCanvases()
    {
        canvases = FindObjectsOfType<Canvas>();
        Debug.Log($"[UIScaler] Found {canvases.Length} canvases");
    }

    /// <summary>
    /// Cache all UI elements to scale.
    /// </summary>
    private void CacheUIElements()
    {
        textOriginalSizes.Clear();
        imageOriginalSizes.Clear();

        // Cache all Text components
        Text[] allTexts = FindObjectsOfType<Text>(true);
        foreach (Text text in allTexts)
        {
            textOriginalSizes[text] = text.fontSize;
        }

        // Cache all Image components (for icon scaling)
        Image[] allImages = FindObjectsOfType<Image>(true);
        foreach (Image image in allImages)
        {
            RectTransform rect = image.GetComponent<RectTransform>();
            if (rect != null)
            {
                imageOriginalSizes[image] = rect.sizeDelta;
            }
        }

        Debug.Log($"[UIScaler] Cached {textOriginalSizes.Count} text elements and {imageOriginalSizes.Count} images");
    }

    /// <summary>
    /// Apply current settings from SettingsManager.
    /// </summary>
    private void ApplyCurrentSettings()
    {
        if (SettingsManager.Instance == null) return;

        var accessibility = SettingsManager.Instance.Accessibility;
        SetUIScale(accessibility.uiScale);
        SetFontSize(accessibility.fontSize);
    }

    /// <summary>
    /// Set UI scale.
    /// </summary>
    public void SetUIScale(AccessibilitySettings.UIScale scale)
    {
        float scaleValue = GetScaleValue(scale);
        SetUIScale(scaleValue);
    }

    /// <summary>
    /// Set UI scale (raw value).
    /// </summary>
    public void SetUIScale(float scale)
    {
        currentUIScale = scale;

        // Apply to all canvases
        foreach (Canvas canvas in canvases)
        {
            if (canvas == null) continue;

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.scaleFactor = baseUIScale * currentUIScale;
            }
        }

        // Scale images
        foreach (var kvp in imageOriginalSizes)
        {
            if (kvp.Key == null) continue;

            RectTransform rect = kvp.Key.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = kvp.Value * currentUIScale;
            }
        }

        Debug.Log($"[UIScaler] UI scale set to {currentUIScale}");
        EventSystem.Publish("UIScaleChanged", currentUIScale);
    }

    /// <summary>
    /// Set font size.
    /// </summary>
    public void SetFontSize(AccessibilitySettings.FontSize size)
    {
        int fontSize = GetFontSizeValue(size);
        SetFontSize(fontSize);
    }

    /// <summary>
    /// Set font size (raw value).
    /// </summary>
    public void SetFontSize(int fontSize)
    {
        currentFontSize = fontSize;

        // Calculate multiplier from base
        float multiplier = (float)currentFontSize / baseFontSize;

        // Apply to all text elements
        foreach (var kvp in textOriginalSizes)
        {
            if (kvp.Key == null) continue;

            int scaledSize = Mathf.RoundToInt(kvp.Value * multiplier);
            kvp.Key.fontSize = scaledSize;
        }

        Debug.Log($"[UIScaler] Font size set to {currentFontSize} (base: {baseFontSize})");
        EventSystem.Publish("FontSizeChanged", currentFontSize);
    }

    /// <summary>
    /// Get scale value from enum.
    /// </summary>
    private float GetScaleValue(AccessibilitySettings.UIScale scale)
    {
        switch (scale)
        {
            case AccessibilitySettings.UIScale.Compact:
                return 0.75f;
            case AccessibilitySettings.UIScale.Normal:
                return 1f;
            case AccessibilitySettings.UIScale.Comfortable:
                return 1.25f;
            case AccessibilitySettings.UIScale.Large:
                return 1.5f;
            case AccessibilitySettings.UIScale.ExtraLarge:
                return 2f;
            default:
                return 1f;
        }
    }

    /// <summary>
    /// Get font size value from enum.
    /// </summary>
    private int GetFontSizeValue(AccessibilitySettings.FontSize size)
    {
        switch (size)
        {
            case AccessibilitySettings.FontSize.Small:
                return 12;
            case AccessibilitySettings.FontSize.Medium:
                return 16;
            case AccessibilitySettings.FontSize.Large:
                return 20;
            case AccessibilitySettings.FontSize.ExtraLarge:
                return 28;
            default:
                return 16;
        }
    }

    /// <summary>
    /// Register a new UI element for scaling.
    /// Call this when dynamically creating UI.
    /// </summary>
    public void RegisterUIElement(GameObject uiElement)
    {
        // Cache text components
        Text[] texts = uiElement.GetComponentsInChildren<Text>(true);
        foreach (Text text in texts)
        {
            if (!textOriginalSizes.ContainsKey(text))
            {
                textOriginalSizes[text] = text.fontSize;

                // Apply current scaling
                float multiplier = (float)currentFontSize / baseFontSize;
                text.fontSize = Mathf.RoundToInt(text.fontSize * multiplier);
            }
        }

        // Cache image components
        Image[] images = uiElement.GetComponentsInChildren<Image>(true);
        foreach (Image image in images)
        {
            RectTransform rect = image.GetComponent<RectTransform>();
            if (rect != null && !imageOriginalSizes.ContainsKey(image))
            {
                imageOriginalSizes[image] = rect.sizeDelta;

                // Apply current scaling
                rect.sizeDelta = rect.sizeDelta * currentUIScale;
            }
        }
    }

    /// <summary>
    /// Unregister UI element from scaling.
    /// Call this when destroying UI elements.
    /// </summary>
    public void UnregisterUIElement(GameObject uiElement)
    {
        Text[] texts = uiElement.GetComponentsInChildren<Text>(true);
        foreach (Text text in texts)
        {
            if (textOriginalSizes.ContainsKey(text))
            {
                textOriginalSizes.Remove(text);
            }
        }

        Image[] images = uiElement.GetComponentsInChildren<Image>(true);
        foreach (Image image in images)
        {
            if (imageOriginalSizes.ContainsKey(image))
            {
                imageOriginalSizes.Remove(image);
            }
        }
    }

    /// <summary>
    /// Refresh all UI scaling.
    /// Call this after scene changes or when new UI is added.
    /// </summary>
    public void RefreshUIScaling()
    {
        if (autoFindCanvases)
        {
            FindAllCanvases();
        }

        CacheUIElements();
        ApplyCurrentSettings();
    }

    #region Event Handlers

    private void OnSetUIScale(float scale)
    {
        SetUIScale(scale);
    }

    private void OnSetFontSize(int fontSize)
    {
        SetFontSize(fontSize);
    }

    #endregion

    #region Public Accessors

    public float CurrentUIScale => currentUIScale;
    public int CurrentFontSize => currentFontSize;

    #endregion
}
