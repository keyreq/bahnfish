using UnityEngine;
using System;

/// <summary>
/// Manages accessibility settings.
/// Features:
/// - 8 colorblind modes (Protanopia, Deuteranopia, Tritanopia, etc.)
/// - UI scaling (75%-200%)
/// - Font size (Small/Medium/Large/Extra Large)
/// - High contrast mode
/// - Screen reader support
/// - Reduced motion (disables camera shake, screen shake)
/// - Photosensitivity mode (reduces flashing effects)
/// - One-handed mode (simplified controls)
/// - Auto-aim assist
/// - Button hold duration adjustment
/// - Cursor size
/// - Tooltip delay
/// </summary>
public class AccessibilitySettings : MonoBehaviour
{
    #region Enums

    public enum ColorblindMode
    {
        None,
        Protanopia,      // Red-blind
        Deuteranopia,    // Green-blind
        Tritanopia,      // Blue-blind
        Protanomaly,     // Red-weak
        Deuteranomaly,   // Green-weak
        Tritanomaly,     // Blue-weak
        Monochromacy     // Full colorblind
    }

    public enum UIScale
    {
        Compact,      // 75%
        Normal,       // 100%
        Comfortable,  // 125%
        Large,        // 150%
        ExtraLarge    // 200%
    }

    public enum FontSize
    {
        Small,       // 12px
        Medium,      // 16px
        Large,       // 20px
        ExtraLarge   // 28px
    }

    public enum CursorSize
    {
        Small,
        Medium,
        Large,
        ExtraLarge
    }

    public enum TooltipDelay
    {
        Instant,     // 0s
        Short,       // 0.5s
        Medium,      // 1s
        Long         // 2s
    }

    public enum ButtonHoldDuration
    {
        Short,       // 0.3s
        Medium,      // 0.6s
        Long,        // 1.0s
        ExtraLong    // 2.0s
    }

    #endregion

    #region Settings Variables

    [Header("Visual Accessibility")]
    public ColorblindMode colorblindMode = ColorblindMode.None;
    public UIScale uiScale = UIScale.Normal;
    public FontSize fontSize = FontSize.Medium;
    public bool highContrastMode = false;

    [Header("Motion & Effects")]
    public bool reducedMotionEnabled = false;
    public bool photosensitivityMode = false;
    public bool cameraShakeEnabled = true;
    public bool screenShakeEnabled = true;

    [Header("Input Assistance")]
    public bool oneHandedMode = false;
    public bool autoAimAssist = false;
    [Range(0f, 1f)]
    public float autoAimStrength = 0.5f;
    public ButtonHoldDuration buttonHoldDuration = ButtonHoldDuration.Short;

    [Header("UI Assistance")]
    public bool screenReaderSupport = false;
    public CursorSize cursorSize = CursorSize.Medium;
    public TooltipDelay tooltipDelay = TooltipDelay.Short;

    #endregion

    private bool isInitialized = false;

    /// <summary>
    /// Initialize accessibility settings.
    /// </summary>
    public void Initialize()
    {
        if (isInitialized) return;

        isInitialized = true;
        Debug.Log("[AccessibilitySettings] Initialized");
    }

    #region Apply Settings

    /// <summary>
    /// Apply all accessibility settings.
    /// </summary>
    public void ApplySettings()
    {
        ApplyColorblindMode();
        ApplyUIScale();
        ApplyFontSize();
        ApplyHighContrastMode();
        ApplyMotionSettings();
        ApplyInputAssistance();
        ApplyUIAssistance();

        Debug.Log("[AccessibilitySettings] Settings applied");
        EventSystem.Publish("AccessibilitySettingsApplied", true);
    }

    /// <summary>
    /// Apply colorblind mode shader.
    /// </summary>
    private void ApplyColorblindMode()
    {
        EventSystem.Publish("SetColorblindMode", colorblindMode);

        if (colorblindMode != ColorblindMode.None)
        {
            EventSystem.Publish("AccessibilityModeEnabled", "Colorblind");
        }
    }

    /// <summary>
    /// Apply UI scaling.
    /// </summary>
    private void ApplyUIScale()
    {
        float scale = GetUIScaleValue();
        EventSystem.Publish("SetUIScale", scale);
    }

    /// <summary>
    /// Get UI scale multiplier from enum.
    /// </summary>
    private float GetUIScaleValue()
    {
        switch (uiScale)
        {
            case UIScale.Compact:
                return 0.75f;
            case UIScale.Normal:
                return 1f;
            case UIScale.Comfortable:
                return 1.25f;
            case UIScale.Large:
                return 1.5f;
            case UIScale.ExtraLarge:
                return 2f;
            default:
                return 1f;
        }
    }

    /// <summary>
    /// Apply font size.
    /// </summary>
    private void ApplyFontSize()
    {
        int baseFontSize = GetBaseFontSize();
        EventSystem.Publish("SetFontSize", baseFontSize);
    }

    /// <summary>
    /// Get base font size in pixels.
    /// </summary>
    private int GetBaseFontSize()
    {
        switch (fontSize)
        {
            case FontSize.Small:
                return 12;
            case FontSize.Medium:
                return 16;
            case FontSize.Large:
                return 20;
            case FontSize.ExtraLarge:
                return 28;
            default:
                return 16;
        }
    }

    /// <summary>
    /// Apply high contrast mode.
    /// </summary>
    private void ApplyHighContrastMode()
    {
        EventSystem.Publish("SetHighContrastMode", highContrastMode);

        if (highContrastMode)
        {
            EventSystem.Publish("AccessibilityModeEnabled", "HighContrast");
        }
    }

    /// <summary>
    /// Apply motion and effects settings.
    /// </summary>
    private void ApplyMotionSettings()
    {
        // Reduced motion overrides individual shake settings
        bool effectiveCameraShake = !reducedMotionEnabled && cameraShakeEnabled;
        bool effectiveScreenShake = !reducedMotionEnabled && screenShakeEnabled;

        EventSystem.Publish("SetReducedMotion", reducedMotionEnabled);
        EventSystem.Publish("SetPhotosensitivityMode", photosensitivityMode);
        EventSystem.Publish("SetCameraShake", effectiveCameraShake);
        EventSystem.Publish("SetScreenShake", effectiveScreenShake);

        if (reducedMotionEnabled)
        {
            EventSystem.Publish("AccessibilityModeEnabled", "ReducedMotion");
        }

        if (photosensitivityMode)
        {
            EventSystem.Publish("AccessibilityModeEnabled", "Photosensitivity");
        }
    }

    /// <summary>
    /// Apply input assistance settings.
    /// </summary>
    private void ApplyInputAssistance()
    {
        EventSystem.Publish("SetOneHandedMode", oneHandedMode);
        EventSystem.Publish("SetAutoAimAssist", new AutoAimSettings { enabled = autoAimAssist, strength = autoAimStrength });
        EventSystem.Publish("SetButtonHoldDuration", GetButtonHoldDurationSeconds());

        if (oneHandedMode)
        {
            EventSystem.Publish("AccessibilityModeEnabled", "OneHanded");
        }
    }

    /// <summary>
    /// Get button hold duration in seconds.
    /// </summary>
    private float GetButtonHoldDurationSeconds()
    {
        switch (buttonHoldDuration)
        {
            case ButtonHoldDuration.Short:
                return 0.3f;
            case ButtonHoldDuration.Medium:
                return 0.6f;
            case ButtonHoldDuration.Long:
                return 1.0f;
            case ButtonHoldDuration.ExtraLong:
                return 2.0f;
            default:
                return 0.3f;
        }
    }

    /// <summary>
    /// Apply UI assistance settings.
    /// </summary>
    private void ApplyUIAssistance()
    {
        EventSystem.Publish("SetScreenReaderSupport", screenReaderSupport);
        EventSystem.Publish("SetCursorSize", GetCursorSizeMultiplier());
        EventSystem.Publish("SetTooltipDelay", GetTooltipDelaySeconds());

        if (screenReaderSupport)
        {
            EventSystem.Publish("AccessibilityModeEnabled", "ScreenReader");
        }
    }

    /// <summary>
    /// Get cursor size multiplier.
    /// </summary>
    private float GetCursorSizeMultiplier()
    {
        switch (cursorSize)
        {
            case CursorSize.Small:
                return 0.75f;
            case CursorSize.Medium:
                return 1f;
            case CursorSize.Large:
                return 1.5f;
            case CursorSize.ExtraLarge:
                return 2f;
            default:
                return 1f;
        }
    }

    /// <summary>
    /// Get tooltip delay in seconds.
    /// </summary>
    private float GetTooltipDelaySeconds()
    {
        switch (tooltipDelay)
        {
            case TooltipDelay.Instant:
                return 0f;
            case TooltipDelay.Short:
                return 0.5f;
            case TooltipDelay.Medium:
                return 1f;
            case TooltipDelay.Long:
                return 2f;
            default:
                return 0.5f;
        }
    }

    #endregion

    #region Save/Load

    /// <summary>
    /// Save settings to PlayerPrefs.
    /// </summary>
    public void SaveSettings()
    {
        PlayerPrefs.SetInt("Accessibility_ColorblindMode", (int)colorblindMode);
        PlayerPrefs.SetInt("Accessibility_UIScale", (int)uiScale);
        PlayerPrefs.SetInt("Accessibility_FontSize", (int)fontSize);
        PlayerPrefs.SetInt("Accessibility_HighContrast", highContrastMode ? 1 : 0);

        PlayerPrefs.SetInt("Accessibility_ReducedMotion", reducedMotionEnabled ? 1 : 0);
        PlayerPrefs.SetInt("Accessibility_Photosensitivity", photosensitivityMode ? 1 : 0);
        PlayerPrefs.SetInt("Accessibility_CameraShake", cameraShakeEnabled ? 1 : 0);
        PlayerPrefs.SetInt("Accessibility_ScreenShake", screenShakeEnabled ? 1 : 0);

        PlayerPrefs.SetInt("Accessibility_OneHandedMode", oneHandedMode ? 1 : 0);
        PlayerPrefs.SetInt("Accessibility_AutoAim", autoAimAssist ? 1 : 0);
        PlayerPrefs.SetFloat("Accessibility_AutoAimStrength", autoAimStrength);
        PlayerPrefs.SetInt("Accessibility_ButtonHoldDuration", (int)buttonHoldDuration);

        PlayerPrefs.SetInt("Accessibility_ScreenReader", screenReaderSupport ? 1 : 0);
        PlayerPrefs.SetInt("Accessibility_CursorSize", (int)cursorSize);
        PlayerPrefs.SetInt("Accessibility_TooltipDelay", (int)tooltipDelay);
    }

    /// <summary>
    /// Load settings from PlayerPrefs.
    /// </summary>
    public void LoadSettings()
    {
        colorblindMode = (ColorblindMode)PlayerPrefs.GetInt("Accessibility_ColorblindMode", 0);
        uiScale = (UIScale)PlayerPrefs.GetInt("Accessibility_UIScale", 1);
        fontSize = (FontSize)PlayerPrefs.GetInt("Accessibility_FontSize", 1);
        highContrastMode = PlayerPrefs.GetInt("Accessibility_HighContrast", 0) == 1;

        reducedMotionEnabled = PlayerPrefs.GetInt("Accessibility_ReducedMotion", 0) == 1;
        photosensitivityMode = PlayerPrefs.GetInt("Accessibility_Photosensitivity", 0) == 1;
        cameraShakeEnabled = PlayerPrefs.GetInt("Accessibility_CameraShake", 1) == 1;
        screenShakeEnabled = PlayerPrefs.GetInt("Accessibility_ScreenShake", 1) == 1;

        oneHandedMode = PlayerPrefs.GetInt("Accessibility_OneHandedMode", 0) == 1;
        autoAimAssist = PlayerPrefs.GetInt("Accessibility_AutoAim", 0) == 1;
        autoAimStrength = PlayerPrefs.GetFloat("Accessibility_AutoAimStrength", 0.5f);
        buttonHoldDuration = (ButtonHoldDuration)PlayerPrefs.GetInt("Accessibility_ButtonHoldDuration", 0);

        screenReaderSupport = PlayerPrefs.GetInt("Accessibility_ScreenReader", 0) == 1;
        cursorSize = (CursorSize)PlayerPrefs.GetInt("Accessibility_CursorSize", 1);
        tooltipDelay = (TooltipDelay)PlayerPrefs.GetInt("Accessibility_TooltipDelay", 1);
    }

    #endregion

    #region Reset

    /// <summary>
    /// Reset to default settings.
    /// </summary>
    public void ResetToDefaults()
    {
        colorblindMode = ColorblindMode.None;
        uiScale = UIScale.Normal;
        fontSize = FontSize.Medium;
        highContrastMode = false;

        reducedMotionEnabled = false;
        photosensitivityMode = false;
        cameraShakeEnabled = true;
        screenShakeEnabled = true;

        oneHandedMode = false;
        autoAimAssist = false;
        autoAimStrength = 0.5f;
        buttonHoldDuration = ButtonHoldDuration.Short;

        screenReaderSupport = false;
        cursorSize = CursorSize.Medium;
        tooltipDelay = TooltipDelay.Short;
    }

    #endregion

    #region Data Transfer

    /// <summary>
    /// Get settings data for save system integration.
    /// </summary>
    public AccessibilitySettingsData GetData()
    {
        return new AccessibilitySettingsData
        {
            colorblindMode = (int)colorblindMode,
            uiScale = (int)uiScale,
            fontSize = (int)fontSize,
            highContrastMode = highContrastMode,
            reducedMotionEnabled = reducedMotionEnabled,
            photosensitivityMode = photosensitivityMode,
            cameraShakeEnabled = cameraShakeEnabled,
            screenShakeEnabled = screenShakeEnabled,
            oneHandedMode = oneHandedMode,
            autoAimAssist = autoAimAssist,
            autoAimStrength = autoAimStrength,
            buttonHoldDuration = (int)buttonHoldDuration,
            screenReaderSupport = screenReaderSupport,
            cursorSize = (int)cursorSize,
            tooltipDelay = (int)tooltipDelay
        };
    }

    /// <summary>
    /// Set settings from data structure.
    /// </summary>
    public void SetData(AccessibilitySettingsData data)
    {
        if (data == null) return;

        colorblindMode = (ColorblindMode)data.colorblindMode;
        uiScale = (UIScale)data.uiScale;
        fontSize = (FontSize)data.fontSize;
        highContrastMode = data.highContrastMode;

        reducedMotionEnabled = data.reducedMotionEnabled;
        photosensitivityMode = data.photosensitivityMode;
        cameraShakeEnabled = data.cameraShakeEnabled;
        screenShakeEnabled = data.screenShakeEnabled;

        oneHandedMode = data.oneHandedMode;
        autoAimAssist = data.autoAimAssist;
        autoAimStrength = data.autoAimStrength;
        buttonHoldDuration = (ButtonHoldDuration)data.buttonHoldDuration;

        screenReaderSupport = data.screenReaderSupport;
        cursorSize = (CursorSize)data.cursorSize;
        tooltipDelay = (TooltipDelay)data.tooltipDelay;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Enable a specific colorblind mode.
    /// </summary>
    public void SetColorblindMode(ColorblindMode mode)
    {
        colorblindMode = mode;
        ApplyColorblindMode();
        EventSystem.Publish("SettingChanged", new SettingChangedEvent("ColorblindMode", mode));
    }

    /// <summary>
    /// Set UI scale.
    /// </summary>
    public void SetUIScale(UIScale scale)
    {
        uiScale = scale;
        ApplyUIScale();
        EventSystem.Publish("SettingChanged", new SettingChangedEvent("UIScale", scale));
    }

    /// <summary>
    /// Toggle reduced motion mode.
    /// </summary>
    public void ToggleReducedMotion()
    {
        reducedMotionEnabled = !reducedMotionEnabled;
        ApplyMotionSettings();
        EventSystem.Publish("SettingChanged", new SettingChangedEvent("ReducedMotion", reducedMotionEnabled));
    }

    /// <summary>
    /// Toggle photosensitivity mode.
    /// </summary>
    public void TogglePhotosensitivityMode()
    {
        photosensitivityMode = !photosensitivityMode;
        ApplyMotionSettings();
        EventSystem.Publish("SettingChanged", new SettingChangedEvent("PhotosensitivityMode", photosensitivityMode));
    }

    /// <summary>
    /// Get colorblind mode name for display.
    /// </summary>
    public static string GetColorblindModeName(ColorblindMode mode)
    {
        switch (mode)
        {
            case ColorblindMode.None:
                return "None (Default)";
            case ColorblindMode.Protanopia:
                return "Protanopia (Red-Blind)";
            case ColorblindMode.Deuteranopia:
                return "Deuteranopia (Green-Blind)";
            case ColorblindMode.Tritanopia:
                return "Tritanopia (Blue-Blind)";
            case ColorblindMode.Protanomaly:
                return "Protanomaly (Red-Weak)";
            case ColorblindMode.Deuteranomaly:
                return "Deuteranomaly (Green-Weak)";
            case ColorblindMode.Tritanomaly:
                return "Tritanomaly (Blue-Weak)";
            case ColorblindMode.Monochromacy:
                return "Monochromacy (Full Colorblind)";
            default:
                return "Unknown";
        }
    }

    #endregion
}

/// <summary>
/// Serializable accessibility settings data.
/// </summary>
[Serializable]
public class AccessibilitySettingsData
{
    public int colorblindMode = 0;
    public int uiScale = 1;
    public int fontSize = 1;
    public bool highContrastMode = false;

    public bool reducedMotionEnabled = false;
    public bool photosensitivityMode = false;
    public bool cameraShakeEnabled = true;
    public bool screenShakeEnabled = true;

    public bool oneHandedMode = false;
    public bool autoAimAssist = false;
    public float autoAimStrength = 0.5f;
    public int buttonHoldDuration = 0;

    public bool screenReaderSupport = false;
    public int cursorSize = 1;
    public int tooltipDelay = 1;
}

/// <summary>
/// Auto-aim settings event data.
/// </summary>
[Serializable]
public class AutoAimSettings
{
    public bool enabled;
    public float strength;
}
