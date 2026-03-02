using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Comprehensive integration examples for the Accessibility & Settings System.
/// Demonstrates how to use all major features.
///
/// Usage:
/// 1. Attach to any GameObject in your scene
/// 2. Assign UI references in Inspector
/// 3. Call example methods to test functionality
/// </summary>
public class AccessibilityIntegrationExample : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button applySettingsButton;
    [SerializeField] private Button resetSettingsButton;
    [SerializeField] private Dropdown colorblindDropdown;
    [SerializeField] private Slider uiScaleSlider;
    [SerializeField] private Toggle subtitlesToggle;
    [SerializeField] private RectTransform fishingButtonUI;

    private void Start()
    {
        // Wait for settings to initialize
        if (SettingsManager.Instance != null && SettingsManager.Instance.IsInitialized)
        {
            InitializeUI();
            SubscribeToEvents();
        }
        else
        {
            // Subscribe to initialization event
            EventSystem.Subscribe<bool>("SettingsInitialized", OnSettingsInitialized);
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    #region Initialization

    /// <summary>
    /// Initialize UI elements with current settings.
    /// </summary>
    private void InitializeUI()
    {
        if (SettingsManager.Instance == null) return;

        // Set up buttons
        if (applySettingsButton != null)
        {
            applySettingsButton.onClick.AddListener(OnApplySettings);
        }

        if (resetSettingsButton != null)
        {
            resetSettingsButton.onClick.AddListener(OnResetSettings);
        }

        // Set up dropdowns
        if (colorblindDropdown != null)
        {
            colorblindDropdown.onValueChanged.AddListener(OnColorblindModeChanged);
            colorblindDropdown.value = (int)SettingsManager.Instance.Accessibility.colorblindMode;
        }

        // Set up sliders
        if (uiScaleSlider != null)
        {
            uiScaleSlider.onValueChanged.AddListener(OnUIScaleChanged);
            uiScaleSlider.value = (int)SettingsManager.Instance.Accessibility.uiScale;
        }

        // Set up toggles
        if (subtitlesToggle != null)
        {
            subtitlesToggle.onValueChanged.AddListener(OnSubtitlesToggled);
            subtitlesToggle.isOn = SettingsManager.Instance.Audio.subtitlesEnabled;
        }
    }

    /// <summary>
    /// Subscribe to settings events.
    /// </summary>
    private void SubscribeToEvents()
    {
        EventSystem.Subscribe<bool>("SettingsSaved", OnSettingsSaved);
        EventSystem.Subscribe<string>("SettingsReset", OnSettingsReset);
        EventSystem.Subscribe<SettingChangedEvent>("SettingChanged", OnSettingChanged);
        EventSystem.Subscribe<AccessibilitySettings.ColorblindMode>("SetColorblindMode", OnColorblindModeApplied);
        EventSystem.Subscribe<BenchmarkResults>("BenchmarkComplete", OnBenchmarkComplete);
    }

    /// <summary>
    /// Unsubscribe from events.
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        EventSystem.Unsubscribe<bool>("SettingsSaved", OnSettingsSaved);
        EventSystem.Unsubscribe<string>("SettingsReset", OnSettingsReset);
        EventSystem.Unsubscribe<SettingChangedEvent>("SettingChanged", OnSettingChanged);
        EventSystem.Unsubscribe<AccessibilitySettings.ColorblindMode>("SetColorblindMode", OnColorblindModeApplied);
        EventSystem.Unsubscribe<BenchmarkResults>("BenchmarkComplete", OnBenchmarkComplete);
    }

    #endregion

    #region Example 1: Video Settings

    /// <summary>
    /// Example: Configure video settings for high-end PC.
    /// </summary>
    [ContextMenu("Example: High Quality Video")]
    public void Example_HighQualityVideo()
    {
        if (SettingsManager.Instance == null) return;

        VideoSettings video = SettingsManager.Instance.Video;

        // Set High preset
        video.SetQualityPreset(VideoSettings.QualityPreset.High);

        // Or customize individual settings
        video.shadowQuality = VideoSettings.ShadowQuality.High;
        video.textureQuality = VideoSettings.TextureQuality.Ultra;
        video.antiAliasing = VideoSettings.AntiAliasing.TAA;
        video.postProcessingEnabled = true;
        video.motionBlurEnabled = true;
        video.bloomEnabled = true;

        // Apply
        video.ApplySettings();

        Debug.Log("High quality video settings applied");
    }

    /// <summary>
    /// Example: Configure video settings for low-end PC.
    /// </summary>
    [ContextMenu("Example: Performance Video")]
    public void Example_PerformanceVideo()
    {
        if (SettingsManager.Instance == null) return;

        VideoSettings video = SettingsManager.Instance.Video;

        // Set Low preset for maximum performance
        video.SetQualityPreset(VideoSettings.QualityPreset.Low);

        video.ApplySettings();

        Debug.Log("Performance video settings applied (Low preset)");
    }

    #endregion

    #region Example 2: Colorblind Modes

    /// <summary>
    /// Example: Enable Deuteranopia mode (most common colorblindness).
    /// </summary>
    [ContextMenu("Example: Enable Deuteranopia")]
    public void Example_EnableDeuteranopia()
    {
        if (SettingsManager.Instance == null) return;

        AccessibilitySettings accessibility = SettingsManager.Instance.Accessibility;

        // Enable Deuteranopia (green-blind)
        accessibility.SetColorblindMode(AccessibilitySettings.ColorblindMode.Deuteranopia);

        // Apply
        accessibility.ApplySettings();

        Debug.Log("Deuteranopia colorblind mode enabled");
    }

    /// <summary>
    /// Example: Cycle through all colorblind modes.
    /// </summary>
    [ContextMenu("Example: Cycle Colorblind Modes")]
    public void Example_CycleColorblindModes()
    {
        if (SettingsManager.Instance == null) return;

        AccessibilitySettings accessibility = SettingsManager.Instance.Accessibility;

        // Get next mode
        int currentMode = (int)accessibility.colorblindMode;
        int nextMode = (currentMode + 1) % 8; // 8 modes total

        accessibility.SetColorblindMode((AccessibilitySettings.ColorblindMode)nextMode);
        accessibility.ApplySettings();

        string modeName = AccessibilitySettings.GetColorblindModeName((AccessibilitySettings.ColorblindMode)nextMode);
        Debug.Log($"Colorblind mode: {modeName}");
    }

    #endregion

    #region Example 3: UI Scaling & Accessibility

    /// <summary>
    /// Example: Enable full accessibility mode.
    /// </summary>
    [ContextMenu("Example: Full Accessibility Mode")]
    public void Example_FullAccessibilityMode()
    {
        if (SettingsManager.Instance == null) return;

        AccessibilitySettings accessibility = SettingsManager.Instance.Accessibility;

        // Large UI
        accessibility.uiScale = AccessibilitySettings.UIScale.Large;

        // Large fonts
        accessibility.fontSize = AccessibilitySettings.FontSize.Large;

        // High contrast
        accessibility.highContrastMode = true;

        // Reduced motion
        accessibility.reducedMotionEnabled = true;
        accessibility.cameraShakeEnabled = false;
        accessibility.screenShakeEnabled = false;

        // One-handed mode
        accessibility.oneHandedMode = true;

        // Auto-aim assist
        accessibility.autoAimAssist = true;
        accessibility.autoAimStrength = 0.75f;

        // Longer hold durations
        accessibility.buttonHoldDuration = AccessibilitySettings.ButtonHoldDuration.Long;

        // Large cursor
        accessibility.cursorSize = AccessibilitySettings.CursorSize.Large;

        // Apply all
        accessibility.ApplySettings();

        Debug.Log("Full accessibility mode enabled");
    }

    /// <summary>
    /// Example: Enable reduced motion for motion sensitivity.
    /// </summary>
    [ContextMenu("Example: Reduced Motion")]
    public void Example_ReducedMotion()
    {
        if (SettingsManager.Instance == null) return;

        AccessibilitySettings accessibility = SettingsManager.Instance.Accessibility;

        // Enable reduced motion
        accessibility.reducedMotionEnabled = true;
        accessibility.photosensitivityMode = true;
        accessibility.cameraShakeEnabled = false;
        accessibility.screenShakeEnabled = false;

        accessibility.ApplySettings();

        Debug.Log("Reduced motion mode enabled");
    }

    #endregion

    #region Example 4: Subtitles

    /// <summary>
    /// Example: Show dialogue with subtitles.
    /// </summary>
    [ContextMenu("Example: Show Dialogue")]
    public void Example_ShowDialogue()
    {
        if (SubtitleSystem.Instance == null) return;

        // Show subtitle with auto-duration
        SubtitleSystem.Instance.ShowSubtitle(
            "Captain Ahab",
            "I've been fishing these waters for thirty years, but I've never seen anything like what you're about to encounter."
        );

        Debug.Log("Dialogue subtitle shown");
    }

    /// <summary>
    /// Example: Show narration.
    /// </summary>
    [ContextMenu("Example: Show Narration")]
    public void Example_ShowNarration()
    {
        if (SubtitleSystem.Instance == null) return;

        SubtitleSystem.Instance.ShowSubtitle(
            "Narrator",
            "As night falls, the waters grow dark and dangerous..."
        );

        Debug.Log("Narration subtitle shown");
    }

    #endregion

    #region Example 5: Tutorial Hints

    /// <summary>
    /// Example: Show tutorial hint for first fishing.
    /// </summary>
    [ContextMenu("Example: First Fishing Hint")]
    public void Example_FirstFishingHint()
    {
        if (TutorialHintSystem.Instance == null) return;

        // Show hint with arrow pointing to fishing button
        TutorialHintSystem.Instance.ShowHint(
            "FirstFish",
            "Press SPACE to cast your fishing line. Watch the tension meter to avoid breaking the line!",
            isImportant: false,
            targetUI: fishingButtonUI,
            duration: 7f,
            allowRepeat: false
        );

        Debug.Log("First fishing hint shown");
    }

    /// <summary>
    /// Example: Show important safety warning.
    /// </summary>
    [ContextMenu("Example: Low Sanity Warning")]
    public void Example_LowSanityWarning()
    {
        if (TutorialHintSystem.Instance == null) return;

        // Important hints always show, even if hints are disabled
        TutorialHintSystem.Instance.ShowCommonHint(
            TutorialHintSystem.CommonHint.LowSanity
        );

        Debug.Log("Low sanity warning shown (important)");
    }

    #endregion

    #region Example 6: Performance Monitoring

    /// <summary>
    /// Example: Enable performance monitoring.
    /// </summary>
    [ContextMenu("Example: Enable Performance Monitor")]
    public void Example_EnablePerformanceMonitor()
    {
        if (PerformanceMonitor.Instance == null) return;

        // Show FPS counter
        PerformanceMonitor.Instance.SetFPSCounter(true);

        // Enable auto-quality adjustment
        PerformanceMonitor.Instance.SetAutoQualityAdjustment(true);

        Debug.Log("Performance monitor enabled with auto-quality");
    }

    /// <summary>
    /// Example: Run performance benchmark.
    /// </summary>
    [ContextMenu("Example: Run Benchmark")]
    public void Example_RunBenchmark()
    {
        if (PerformanceMonitor.Instance == null) return;

        // Run 30-second benchmark
        PerformanceMonitor.Instance.StartBenchmark(30f);

        Debug.Log("30-second benchmark started");
    }

    #endregion

    #region Example 7: Input Remapping

    /// <summary>
    /// Example: Remap fishing key to mouse.
    /// </summary>
    [ContextMenu("Example: Remap Cast to Mouse")]
    public void Example_RemapCastKey()
    {
        if (SettingsManager.Instance == null) return;

        ControlSettings controls = SettingsManager.Instance.Controls;

        // Attempt to remap
        bool success = controls.RemapKey("Cast", KeyCode.Mouse0);

        if (success)
        {
            controls.SaveSettings();
            Debug.Log("Successfully rebound Cast to Left Mouse");
        }
        else
        {
            Debug.Log("Rebind failed - conflict detected");
        }
    }

    /// <summary>
    /// Example: Apply accessibility control scheme.
    /// </summary>
    [ContextMenu("Example: Accessibility Controls")]
    public void Example_AccessibilityControls()
    {
        if (SettingsManager.Instance == null) return;

        ControlSettings controls = SettingsManager.Instance.Controls;

        // Apply accessibility scheme (one-handed)
        controls.ApplyControlScheme(ControlSettings.ControlScheme.Accessibility);

        // Save and apply
        controls.SaveSettings();
        controls.ApplySettings();

        Debug.Log("Accessibility control scheme applied");
    }

    #endregion

    #region Example 8: Difficulty Settings

    /// <summary>
    /// Example: Set Story mode (easy).
    /// </summary>
    [ContextMenu("Example: Story Mode")]
    public void Example_StoryMode()
    {
        if (SettingsManager.Instance == null) return;

        GameplaySettings gameplay = SettingsManager.Instance.Gameplay;

        // Apply Story difficulty
        gameplay.ApplyDifficultyPreset(GameplaySettings.DifficultyLevel.Story);

        // Save and apply
        gameplay.SaveSettings();
        gameplay.ApplySettings();

        Debug.Log("Story mode (easy) applied");
    }

    /// <summary>
    /// Example: Set custom difficulty.
    /// </summary>
    [ContextMenu("Example: Custom Difficulty")]
    public void Example_CustomDifficulty()
    {
        if (SettingsManager.Instance == null) return;

        GameplaySettings gameplay = SettingsManager.Instance.Gameplay;

        // Custom settings
        gameplay.difficulty = GameplaySettings.DifficultyLevel.Custom;
        gameplay.sanityDrainRate = GameplaySettings.SanityDrainRate.Half;
        gameplay.enemyDamageMultiplier = 0.75f;
        gameplay.fishAIAggression = GameplaySettings.AIAggressionLevel.Easy;
        gameplay.tutorialHintsEnabled = true;
        gameplay.questMarkersEnabled = true;

        gameplay.ApplySettings();

        Debug.Log("Custom difficulty applied");
    }

    #endregion

    #region UI Callbacks

    private void OnApplySettings()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.ApplyAllSettings();
            SettingsManager.Instance.SaveAllSettings();
            Debug.Log("All settings applied and saved");
        }
    }

    private void OnResetSettings()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.ResetAllToDefaults();
            Debug.Log("All settings reset to defaults");
        }
    }

    private void OnColorblindModeChanged(int modeIndex)
    {
        if (SettingsManager.Instance != null)
        {
            AccessibilitySettings.ColorblindMode mode = (AccessibilitySettings.ColorblindMode)modeIndex;
            SettingsManager.Instance.Accessibility.SetColorblindMode(mode);
            SettingsManager.Instance.Accessibility.ApplySettings();
        }
    }

    private void OnUIScaleChanged(float scaleIndex)
    {
        if (SettingsManager.Instance != null)
        {
            AccessibilitySettings.UIScale scale = (AccessibilitySettings.UIScale)Mathf.RoundToInt(scaleIndex);
            SettingsManager.Instance.Accessibility.SetUIScale(scale);
            SettingsManager.Instance.Accessibility.ApplySettings();
        }
    }

    private void OnSubtitlesToggled(bool enabled)
    {
        if (SubtitleSystem.Instance != null)
        {
            SubtitleSystem.Instance.SetSubtitlesEnabled(enabled);
        }
    }

    #endregion

    #region Event Handlers

    private void OnSettingsInitialized(bool success)
    {
        if (success)
        {
            InitializeUI();
            SubscribeToEvents();
            Debug.Log("Settings initialized successfully");
        }
    }

    private void OnSettingsSaved(bool success)
    {
        Debug.Log($"Settings saved: {success}");
    }

    private void OnSettingsReset(string category)
    {
        Debug.Log($"Settings category reset: {category}");
    }

    private void OnSettingChanged(SettingChangedEvent eventData)
    {
        Debug.Log($"Setting changed: {eventData.settingName} = {eventData.value}");
    }

    private void OnColorblindModeApplied(AccessibilitySettings.ColorblindMode mode)
    {
        string modeName = AccessibilitySettings.GetColorblindModeName(mode);
        Debug.Log($"Colorblind mode applied: {modeName}");
    }

    private void OnBenchmarkComplete(BenchmarkResults results)
    {
        Debug.Log("===== BENCHMARK RESULTS =====");
        Debug.Log($"Duration: {results.duration}s");
        Debug.Log($"Average FPS: {results.averageFPS:F2}");
        Debug.Log($"Min FPS: {results.minFPS:F2}");
        Debug.Log($"Max FPS: {results.maxFPS:F2}");
        Debug.Log($"Total Frames: {results.frameCount}");
        Debug.Log("============================");
    }

    #endregion
}
