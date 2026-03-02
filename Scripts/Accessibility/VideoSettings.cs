using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages video/graphics settings.
/// Features:
/// - Resolution (all supported resolutions)
/// - Fullscreen/Windowed/Borderless
/// - VSync, frame rate limit
/// - Graphics quality presets (Low/Medium/High/Ultra)
/// - Individual quality settings (shadows, textures, AA, etc.)
/// - Real-time application without restart
/// </summary>
public class VideoSettings : MonoBehaviour
{
    #region Enums

    public enum ScreenMode
    {
        Fullscreen,
        Windowed,
        BorderlessWindowed
    }

    public enum QualityPreset
    {
        Low,
        Medium,
        High,
        Ultra,
        Custom
    }

    public enum ShadowQuality
    {
        Off,
        Low,
        Medium,
        High
    }

    public enum TextureQuality
    {
        Low,
        Medium,
        High,
        Ultra
    }

    public enum AntiAliasing
    {
        Off,
        FXAA,
        SMAA,
        TAA
    }

    public enum AnisotropicFiltering
    {
        Off,
        X2,
        X4,
        X8,
        X16
    }

    public enum FrameRateLimit
    {
        Limit30,
        Limit60,
        Limit120,
        Unlimited
    }

    #endregion

    #region Settings Variables

    [Header("Display")]
    public Resolution currentResolution;
    public ScreenMode screenMode = ScreenMode.Fullscreen;
    public bool vSyncEnabled = true;
    public FrameRateLimit frameRateLimit = FrameRateLimit.Limit60;

    [Header("Quality")]
    public QualityPreset qualityPreset = QualityPreset.High;
    public ShadowQuality shadowQuality = ShadowQuality.High;
    public TextureQuality textureQuality = TextureQuality.High;
    public AntiAliasing antiAliasing = AntiAliasing.TAA;
    public AnisotropicFiltering anisotropicFiltering = AnisotropicFiltering.X8;

    [Header("Graphics Features")]
    [Range(0f, 2f)]
    public float lodBias = 1f;
    [Range(0.2f, 1f)]
    public float particleDensity = 1f;
    public bool postProcessingEnabled = true;
    public bool motionBlurEnabled = true;
    public bool bloomEnabled = true;

    #endregion

    private Resolution[] availableResolutions;
    private bool isInitialized = false;

    /// <summary>
    /// Initialize video settings.
    /// </summary>
    public void Initialize()
    {
        if (isInitialized) return;

        // Get available resolutions
        availableResolutions = Screen.resolutions;
        currentResolution = Screen.currentResolution;

        // Set platform-specific defaults
        SetPlatformDefaults();

        isInitialized = true;
        Debug.Log("[VideoSettings] Initialized");
    }

    /// <summary>
    /// Set platform-specific default settings.
    /// </summary>
    private void SetPlatformDefaults()
    {
        // PC defaults - aim for high quality
        #if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
        qualityPreset = QualityPreset.High;
        screenMode = ScreenMode.Fullscreen;
        vSyncEnabled = true;
        frameRateLimit = FrameRateLimit.Limit60;
        #endif

        // Console defaults
        #if UNITY_PS4 || UNITY_PS5 || UNITY_XBOXONE || UNITY_GAMECORE
        qualityPreset = QualityPreset.High;
        screenMode = ScreenMode.Fullscreen;
        vSyncEnabled = true;
        frameRateLimit = FrameRateLimit.Limit60;
        #endif

        // Mobile defaults - prioritize performance
        #if UNITY_ANDROID || UNITY_IOS
        qualityPreset = QualityPreset.Medium;
        screenMode = ScreenMode.Fullscreen;
        vSyncEnabled = false;
        frameRateLimit = FrameRateLimit.Limit30;
        #endif
    }

    #region Apply Settings

    /// <summary>
    /// Apply all video settings to the game.
    /// </summary>
    public void ApplySettings()
    {
        ApplyResolution();
        ApplyScreenMode();
        ApplyVSync();
        ApplyFrameRateLimit();
        ApplyQualitySettings();
        ApplyGraphicsFeatures();

        Debug.Log("[VideoSettings] Settings applied");
        EventSystem.Publish("VideoSettingsApplied", true);
    }

    /// <summary>
    /// Apply resolution setting.
    /// </summary>
    private void ApplyResolution()
    {
        Screen.SetResolution(currentResolution.width, currentResolution.height, GetFullScreenMode());
    }

    /// <summary>
    /// Apply screen mode setting.
    /// </summary>
    private void ApplyScreenMode()
    {
        Screen.fullScreenMode = GetFullScreenMode();
    }

    /// <summary>
    /// Get Unity FullScreenMode from our ScreenMode enum.
    /// </summary>
    private FullScreenMode GetFullScreenMode()
    {
        switch (screenMode)
        {
            case ScreenMode.Fullscreen:
                return FullScreenMode.ExclusiveFullScreen;
            case ScreenMode.Windowed:
                return FullScreenMode.Windowed;
            case ScreenMode.BorderlessWindowed:
                return FullScreenMode.FullScreenWindow;
            default:
                return FullScreenMode.ExclusiveFullScreen;
        }
    }

    /// <summary>
    /// Apply VSync setting.
    /// </summary>
    private void ApplyVSync()
    {
        QualitySettings.vSyncCount = vSyncEnabled ? 1 : 0;
    }

    /// <summary>
    /// Apply frame rate limit.
    /// </summary>
    private void ApplyFrameRateLimit()
    {
        switch (frameRateLimit)
        {
            case FrameRateLimit.Limit30:
                Application.targetFrameRate = 30;
                break;
            case FrameRateLimit.Limit60:
                Application.targetFrameRate = 60;
                break;
            case FrameRateLimit.Limit120:
                Application.targetFrameRate = 120;
                break;
            case FrameRateLimit.Unlimited:
                Application.targetFrameRate = -1;
                break;
        }
    }

    /// <summary>
    /// Apply quality settings based on preset or custom values.
    /// </summary>
    private void ApplyQualitySettings()
    {
        switch (qualityPreset)
        {
            case QualityPreset.Low:
                ApplyLowPreset();
                break;
            case QualityPreset.Medium:
                ApplyMediumPreset();
                break;
            case QualityPreset.High:
                ApplyHighPreset();
                break;
            case QualityPreset.Ultra:
                ApplyUltraPreset();
                break;
            case QualityPreset.Custom:
                ApplyCustomSettings();
                break;
        }
    }

    /// <summary>
    /// Apply Low quality preset.
    /// Target: 30 FPS on low-end PCs
    /// </summary>
    private void ApplyLowPreset()
    {
        shadowQuality = ShadowQuality.Off;
        textureQuality = TextureQuality.Low;
        antiAliasing = AntiAliasing.Off;
        anisotropicFiltering = AnisotropicFiltering.Off;
        lodBias = 2f;
        particleDensity = 0.2f;
        postProcessingEnabled = false;
        motionBlurEnabled = false;
        bloomEnabled = false;

        ApplyCustomSettings();
    }

    /// <summary>
    /// Apply Medium quality preset.
    /// Target: 60 FPS on mid-range PCs
    /// </summary>
    private void ApplyMediumPreset()
    {
        shadowQuality = ShadowQuality.Low;
        textureQuality = TextureQuality.Medium;
        antiAliasing = AntiAliasing.FXAA;
        anisotropicFiltering = AnisotropicFiltering.X4;
        lodBias = 1f;
        particleDensity = 0.4f;
        postProcessingEnabled = true;
        motionBlurEnabled = false;
        bloomEnabled = true;

        ApplyCustomSettings();
    }

    /// <summary>
    /// Apply High quality preset.
    /// Target: 60 FPS on high-end PCs
    /// </summary>
    private void ApplyHighPreset()
    {
        shadowQuality = ShadowQuality.Medium;
        textureQuality = TextureQuality.High;
        antiAliasing = AntiAliasing.SMAA;
        anisotropicFiltering = AnisotropicFiltering.X8;
        lodBias = 0.5f;
        particleDensity = 0.7f;
        postProcessingEnabled = true;
        motionBlurEnabled = true;
        bloomEnabled = true;

        ApplyCustomSettings();
    }

    /// <summary>
    /// Apply Ultra quality preset.
    /// Target: 60+ FPS on enthusiast PCs
    /// </summary>
    private void ApplyUltraPreset()
    {
        shadowQuality = ShadowQuality.High;
        textureQuality = TextureQuality.Ultra;
        antiAliasing = AntiAliasing.TAA;
        anisotropicFiltering = AnisotropicFiltering.X16;
        lodBias = 0f;
        particleDensity = 1f;
        postProcessingEnabled = true;
        motionBlurEnabled = true;
        bloomEnabled = true;

        ApplyCustomSettings();
    }

    /// <summary>
    /// Apply custom quality settings.
    /// </summary>
    private void ApplyCustomSettings()
    {
        // Shadows
        switch (shadowQuality)
        {
            case ShadowQuality.Off:
                QualitySettings.shadows = ShadowQuality.Disable;
                QualitySettings.shadowDistance = 0f;
                break;
            case ShadowQuality.Low:
                QualitySettings.shadows = ShadowQuality.HardOnly;
                QualitySettings.shadowDistance = 50f;
                QualitySettings.shadowResolution = UnityEngine.ShadowResolution.Low;
                break;
            case ShadowQuality.Medium:
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.shadowDistance = 100f;
                QualitySettings.shadowResolution = UnityEngine.ShadowResolution.Medium;
                break;
            case ShadowQuality.High:
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.shadowDistance = 150f;
                QualitySettings.shadowResolution = UnityEngine.ShadowResolution.High;
                break;
        }

        // Textures
        switch (textureQuality)
        {
            case TextureQuality.Low:
                QualitySettings.masterTextureLimit = 2;
                break;
            case TextureQuality.Medium:
                QualitySettings.masterTextureLimit = 1;
                break;
            case TextureQuality.High:
            case TextureQuality.Ultra:
                QualitySettings.masterTextureLimit = 0;
                break;
        }

        // Anisotropic Filtering
        switch (anisotropicFiltering)
        {
            case AnisotropicFiltering.Off:
                QualitySettings.anisotropicFiltering = UnityEngine.AnisotropicFiltering.Disable;
                break;
            case AnisotropicFiltering.X2:
            case AnisotropicFiltering.X4:
            case AnisotropicFiltering.X8:
            case AnisotropicFiltering.X16:
                QualitySettings.anisotropicFiltering = UnityEngine.AnisotropicFiltering.Enable;
                break;
        }

        // LOD Bias
        QualitySettings.lodBias = lodBias;

        // Particle density is handled per-particle system in ApplyGraphicsFeatures
    }

    /// <summary>
    /// Apply graphics features (post-processing, motion blur, etc.)
    /// </summary>
    private void ApplyGraphicsFeatures()
    {
        // Publish events for post-processing controllers to handle
        EventSystem.Publish("SetPostProcessing", postProcessingEnabled);
        EventSystem.Publish("SetMotionBlur", motionBlurEnabled);
        EventSystem.Publish("SetBloom", bloomEnabled);
        EventSystem.Publish("SetParticleDensity", particleDensity);
    }

    #endregion

    #region Save/Load

    /// <summary>
    /// Save settings to PlayerPrefs.
    /// </summary>
    public void SaveSettings()
    {
        PlayerPrefs.SetInt("Video_ResolutionWidth", currentResolution.width);
        PlayerPrefs.SetInt("Video_ResolutionHeight", currentResolution.height);
        PlayerPrefs.SetInt("Video_RefreshRate", currentResolution.refreshRate);
        PlayerPrefs.SetInt("Video_ScreenMode", (int)screenMode);
        PlayerPrefs.SetInt("Video_VSync", vSyncEnabled ? 1 : 0);
        PlayerPrefs.SetInt("Video_FrameRateLimit", (int)frameRateLimit);
        PlayerPrefs.SetInt("Video_QualityPreset", (int)qualityPreset);
        PlayerPrefs.SetInt("Video_ShadowQuality", (int)shadowQuality);
        PlayerPrefs.SetInt("Video_TextureQuality", (int)textureQuality);
        PlayerPrefs.SetInt("Video_AntiAliasing", (int)antiAliasing);
        PlayerPrefs.SetInt("Video_AnisotropicFiltering", (int)anisotropicFiltering);
        PlayerPrefs.SetFloat("Video_LODBias", lodBias);
        PlayerPrefs.SetFloat("Video_ParticleDensity", particleDensity);
        PlayerPrefs.SetInt("Video_PostProcessing", postProcessingEnabled ? 1 : 0);
        PlayerPrefs.SetInt("Video_MotionBlur", motionBlurEnabled ? 1 : 0);
        PlayerPrefs.SetInt("Video_Bloom", bloomEnabled ? 1 : 0);
    }

    /// <summary>
    /// Load settings from PlayerPrefs.
    /// </summary>
    public void LoadSettings()
    {
        if (PlayerPrefs.HasKey("Video_ResolutionWidth"))
        {
            int width = PlayerPrefs.GetInt("Video_ResolutionWidth");
            int height = PlayerPrefs.GetInt("Video_ResolutionHeight");
            int refreshRate = PlayerPrefs.GetInt("Video_RefreshRate", 60);

            // Find matching resolution
            currentResolution = FindClosestResolution(width, height, refreshRate);
        }

        screenMode = (ScreenMode)PlayerPrefs.GetInt("Video_ScreenMode", (int)ScreenMode.Fullscreen);
        vSyncEnabled = PlayerPrefs.GetInt("Video_VSync", 1) == 1;
        frameRateLimit = (FrameRateLimit)PlayerPrefs.GetInt("Video_FrameRateLimit", (int)FrameRateLimit.Limit60);
        qualityPreset = (QualityPreset)PlayerPrefs.GetInt("Video_QualityPreset", (int)QualityPreset.High);
        shadowQuality = (ShadowQuality)PlayerPrefs.GetInt("Video_ShadowQuality", (int)ShadowQuality.High);
        textureQuality = (TextureQuality)PlayerPrefs.GetInt("Video_TextureQuality", (int)TextureQuality.High);
        antiAliasing = (AntiAliasing)PlayerPrefs.GetInt("Video_AntiAliasing", (int)AntiAliasing.TAA);
        anisotropicFiltering = (AnisotropicFiltering)PlayerPrefs.GetInt("Video_AnisotropicFiltering", (int)AnisotropicFiltering.X8);
        lodBias = PlayerPrefs.GetFloat("Video_LODBias", 1f);
        particleDensity = PlayerPrefs.GetFloat("Video_ParticleDensity", 1f);
        postProcessingEnabled = PlayerPrefs.GetInt("Video_PostProcessing", 1) == 1;
        motionBlurEnabled = PlayerPrefs.GetInt("Video_MotionBlur", 1) == 1;
        bloomEnabled = PlayerPrefs.GetInt("Video_Bloom", 1) == 1;
    }

    #endregion

    #region Reset

    /// <summary>
    /// Reset to platform-specific defaults.
    /// </summary>
    public void ResetToDefaults()
    {
        currentResolution = Screen.currentResolution;
        SetPlatformDefaults();
    }

    #endregion

    #region Data Transfer

    /// <summary>
    /// Get settings data for save system integration.
    /// </summary>
    public VideoSettingsData GetData()
    {
        return new VideoSettingsData
        {
            resolutionWidth = currentResolution.width,
            resolutionHeight = currentResolution.height,
            refreshRate = currentResolution.refreshRate,
            screenMode = (int)screenMode,
            vSyncEnabled = vSyncEnabled,
            frameRateLimit = (int)frameRateLimit,
            qualityPreset = (int)qualityPreset,
            shadowQuality = (int)shadowQuality,
            textureQuality = (int)textureQuality,
            antiAliasing = (int)antiAliasing,
            anisotropicFiltering = (int)anisotropicFiltering,
            lodBias = lodBias,
            particleDensity = particleDensity,
            postProcessingEnabled = postProcessingEnabled,
            motionBlurEnabled = motionBlurEnabled,
            bloomEnabled = bloomEnabled
        };
    }

    /// <summary>
    /// Set settings from data structure.
    /// </summary>
    public void SetData(VideoSettingsData data)
    {
        if (data == null) return;

        currentResolution = FindClosestResolution(data.resolutionWidth, data.resolutionHeight, data.refreshRate);
        screenMode = (ScreenMode)data.screenMode;
        vSyncEnabled = data.vSyncEnabled;
        frameRateLimit = (FrameRateLimit)data.frameRateLimit;
        qualityPreset = (QualityPreset)data.qualityPreset;
        shadowQuality = (ShadowQuality)data.shadowQuality;
        textureQuality = (TextureQuality)data.textureQuality;
        antiAliasing = (AntiAliasing)data.antiAliasing;
        anisotropicFiltering = (AnisotropicFiltering)data.anisotropicFiltering;
        lodBias = data.lodBias;
        particleDensity = data.particleDensity;
        postProcessingEnabled = data.postProcessingEnabled;
        motionBlurEnabled = data.motionBlurEnabled;
        bloomEnabled = data.bloomEnabled;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Find closest matching resolution from available resolutions.
    /// </summary>
    private Resolution FindClosestResolution(int width, int height, int refreshRate)
    {
        if (availableResolutions == null || availableResolutions.Length == 0)
        {
            availableResolutions = Screen.resolutions;
        }

        // Try exact match first
        foreach (var res in availableResolutions)
        {
            if (res.width == width && res.height == height && res.refreshRate == refreshRate)
            {
                return res;
            }
        }

        // Try matching width and height, ignore refresh rate
        foreach (var res in availableResolutions)
        {
            if (res.width == width && res.height == height)
            {
                return res;
            }
        }

        // Return current resolution as fallback
        return Screen.currentResolution;
    }

    /// <summary>
    /// Get all available resolutions.
    /// </summary>
    public Resolution[] GetAvailableResolutions()
    {
        return availableResolutions;
    }

    /// <summary>
    /// Set quality preset and apply.
    /// </summary>
    public void SetQualityPreset(QualityPreset preset)
    {
        qualityPreset = preset;
        ApplyQualitySettings();
        EventSystem.Publish("QualityPresetChanged", preset);
    }

    #endregion
}

/// <summary>
/// Serializable video settings data.
/// </summary>
[Serializable]
public class VideoSettingsData
{
    public int resolutionWidth = 1920;
    public int resolutionHeight = 1080;
    public int refreshRate = 60;
    public int screenMode = 0;
    public bool vSyncEnabled = true;
    public int frameRateLimit = 1;
    public int qualityPreset = 2;
    public int shadowQuality = 3;
    public int textureQuality = 2;
    public int antiAliasing = 3;
    public int anisotropicFiltering = 3;
    public float lodBias = 1f;
    public float particleDensity = 1f;
    public bool postProcessingEnabled = true;
    public bool motionBlurEnabled = true;
    public bool bloomEnabled = true;
}
