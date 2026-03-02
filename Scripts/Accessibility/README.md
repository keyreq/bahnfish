# Accessibility & Settings System

**Agent 21 Implementation - Week 27-28**

A comprehensive accessibility and settings system that ensures Bahnfish is playable by as many people as possible, with full customization of controls, visuals, audio, and gameplay.

---

## Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Settings Categories](#settings-categories)
4. [Accessibility Features](#accessibility-features)
5. [Integration Guide](#integration-guide)
6. [Usage Examples](#usage-examples)
7. [Event System](#event-system)
8. [API Reference](#api-reference)

---

## Overview

The Accessibility & Settings System provides:

- **5 Settings Categories**: Video, Audio, Controls, Gameplay, Accessibility
- **8 Colorblind Modes**: Industry-standard color correction for all types of colorblindness
- **UI Scaling**: 75% to 200% scaling with 4 font sizes
- **Input Remapping**: Full keyboard/mouse/controller remapping with conflict detection
- **Reduced Motion**: Disables camera shake and screen effects for comfort
- **Photosensitivity Mode**: Reduces flashing effects and intense visuals
- **One-Handed Mode**: Simplified controls for accessibility
- **Auto-Aim Assist**: Configurable aim assistance
- **Subtitles**: Full subtitle system with customizable size and background
- **Tutorial Hints**: Context-aware tutorial system
- **Performance Monitoring**: FPS counter and auto-quality adjustment

---

## Architecture

### Core Components

```
SettingsManager (Central Hub)
├── VideoSettings
├── AudioSettings
├── ControlSettings
├── GameplaySettings
└── AccessibilitySettings

Supporting Systems
├── ColorblindSimulator
├── InputRemapping
├── PerformanceMonitor
├── UIScaler
├── SubtitleSystem
└── TutorialHintSystem
```

### Singleton Pattern

All systems use the singleton pattern for global access:
```csharp
SettingsManager.Instance
PerformanceMonitor.Instance
SubtitleSystem.Instance
TutorialHintSystem.Instance
UIScaler.Instance
```

### Event-Driven Architecture

Settings changes publish events that other systems subscribe to:
```csharp
EventSystem.Publish("SetColorblindMode", ColorblindMode.Deuteranopia);
EventSystem.Subscribe<ColorblindMode>("SetColorblindMode", OnColorblindModeChanged);
```

---

## Settings Categories

### 1. Video Settings

Controls graphics quality and visual fidelity.

**Display:**
- Resolution (all supported)
- Screen Mode (Fullscreen/Windowed/Borderless)
- VSync (On/Off)
- Frame Rate Limit (30/60/120/Unlimited)

**Quality Presets:**
- **Low**: 30 FPS target, minimal graphics (low-end PCs)
- **Medium**: 60 FPS target, balanced quality (mid-range PCs)
- **High**: 60 FPS target, high quality (high-end PCs)
- **Ultra**: 60+ FPS target, maximum quality (enthusiast PCs)
- **Custom**: Individual settings

**Graphics Settings:**
- Shadow Quality (Off/Low/Medium/High)
- Texture Quality (Low/Medium/High/Ultra)
- Anti-Aliasing (Off/FXAA/SMAA/TAA)
- Anisotropic Filtering (Off/2×/4×/8×/16×)
- LOD Bias (0.0-2.0)
- Particle Density (20%/40%/70%/100%)
- Post-Processing (On/Off)
- Motion Blur (On/Off)
- Bloom (On/Off)

### 2. Audio Settings

Controls all audio channels and subtitle display.

**Volume Channels:**
- Master Volume (0-100%)
- Music Volume (0-100%)
- SFX Volume (0-100%)
- Ambient Volume (0-100%)
- UI Volume (0-100%)

**Mute Toggles:**
- Mute All
- Individual channel mutes

**Subtitles:**
- Enabled/Disabled
- Size (Small/Medium/Large)
- Background Opacity (0-100%)

### 3. Control Settings

Manages input devices and key bindings.

**Input Device:**
- Keyboard/Mouse
- Controller
- Accessibility Controller

**Controller:**
- Sensitivity X/Y (0.1-3.0)
- Invert Y-Axis
- Vibration (On/Off)
- Vibration Intensity (0-100%)

**Aim Assist:**
- Enabled/Disabled
- Strength (0-100%)

**Control Schemes:**
- **Standard**: WASD movement, standard PC layout
- **Alternate**: Arrow keys, different action keys
- **Accessibility**: One-handed controls, all actions on left side
- **Custom**: Fully rebindable

**Key Remapping:**
- Rebind any action
- Conflict detection
- Reset to defaults

### 4. Gameplay Settings

Adjusts difficulty and game mechanics.

**Difficulty Presets:**
- **Story**: Easy, narrative-focused (50% damage, 50% sanity drain)
- **Normal**: Balanced challenge (100% modifiers)
- **Hard**: Challenging, punishing (200% damage, 200% sanity drain)
- **Custom**: Individual settings

**Gameplay Modifiers:**
- Fish AI Aggression (Easy/Normal/Hard)
- Fishing Mini-game Difficulty (Easy/Normal/Hard)
- Sanity Drain Rate (Off/50%/100%/150%/200%)
- Enemy Damage Multiplier (0.5×/1×/1.5×/2×)
- Time Scale (0.5×/1×/1.5×/2×)

**Progression:**
- Auto-Save Frequency (Never/1min/5min/10min)
- Permadeath (On/Off)

**UI Assistance:**
- Tutorial Hints (On/Off)
- Quest Markers (On/Off)

### 5. Accessibility Settings

Ensures the game is playable for all players.

**Colorblind Modes:**
1. None (Default)
2. Protanopia (Red-Blind)
3. Deuteranopia (Green-Blind) - Most common severe form
4. Tritanopia (Blue-Blind)
5. Protanomaly (Red-Weak)
6. Deuteranomaly (Green-Weak) - Most common overall (~5% of males)
7. Tritanomaly (Blue-Weak)
8. Monochromacy (Full Colorblind)

**UI Scaling:**
- Compact (75%)
- Normal (100%)
- Comfortable (125%)
- Large (150%)
- Extra Large (200%)

**Font Size:**
- Small (12px)
- Medium (16px)
- Large (20px)
- Extra Large (28px)

**Visual:**
- High Contrast Mode

**Motion & Effects:**
- Reduced Motion (disables camera/screen shake)
- Photosensitivity Mode (reduces flashing)
- Camera Shake (On/Off)
- Screen Shake (On/Off)

**Input Assistance:**
- One-Handed Mode (simplified controls)
- Auto-Aim Assist (0-100% strength)
- Button Hold Duration (Short/Medium/Long/Extra Long)

**UI Assistance:**
- Screen Reader Support
- Cursor Size (Small/Medium/Large/Extra Large)
- Tooltip Delay (Instant/0.5s/1s/2s)

---

## Accessibility Features

### Colorblind Mode Implementation

Uses industry-standard color correction matrices based on:
- Brettel, Viénot and Mollon (1997)
- Machado, Oliveira and Fernandes (2009)

**Color Transformations:**
```csharp
// Protanopia (Red-Blind) Matrix
[ 0.567  0.433  0.000 ]
[ 0.558  0.442  0.000 ]
[ 0.000  0.242  0.758 ]

// Deuteranomaly (Green-Weak) - Most Common
[ 0.800  0.200  0.000 ]
[ 0.258  0.742  0.000 ]
[ 0.000  0.142  0.858 ]
```

**UI Element Adaptations:**

The colorblind modes work in conjunction with UI symbols:

- **Fishing Tension Meter**: Patterns + color (safe/warning/danger)
- **Fish Rarity**: Icons + color
  - Common: ○ (circle)
  - Uncommon: ◇ (diamond)
  - Rare: ★ (star)
  - Legendary: ♦ (filled diamond)
  - Aberrant: ☠ (skull)
- **Quest Markers**: Shapes + colors
- **Sanity Meter**: Wave patterns for low sanity

### Reduced Motion Mode

When enabled:
- Disables camera shake (fishing, boat collision, horror events)
- Disables screen shake (damage, explosions)
- Reduces UI animation speed (2× slower)
- Disables parallax effects
- Reduces particle motion

### Photosensitivity Mode

When enabled:
- Removes lightning flashes
- Dims Blood Moon red overlay (50% intensity)
- Eliminates rapid flickering effects
- Shows warning before intense visual sequences

### One-Handed Mode

Optimized for players who can only use one hand:
- All essential actions on one side of keyboard
- No simultaneous key presses required
- Auto-cast fishing (no hold required)
- Auto-reel (simplified timing)
- Larger UI buttons
- Auto-aim assistance enabled by default

---

## Integration Guide

### Initial Setup

1. **Add SettingsManager to Scene:**
```csharp
GameObject settingsManagerObj = new GameObject("SettingsManager");
SettingsManager manager = settingsManagerObj.AddComponent<SettingsManager>();
```

2. **Add ColorblindSimulator to Camera:**
```csharp
Camera.main.gameObject.AddComponent<ColorblindSimulator>();
```

3. **Add Supporting Systems:**
```csharp
GameObject uiRoot = GameObject.Find("UI");
uiRoot.AddComponent<UIScaler>();
uiRoot.AddComponent<SubtitleSystem>();
uiRoot.AddComponent<TutorialHintSystem>();

GameObject performanceObj = new GameObject("PerformanceMonitor");
performanceObj.AddComponent<PerformanceMonitor>();
```

### Subscribe to Settings Changes

```csharp
// Video settings
EventSystem.Subscribe<bool>("VideoSettingsApplied", OnVideoSettingsApplied);
EventSystem.Subscribe<VideoSettings.QualityPreset>("QualityPresetChanged", OnQualityChanged);

// Audio settings
EventSystem.Subscribe<float>("SetMasterVolume", OnMasterVolumeChanged);
EventSystem.Subscribe<bool>("SetSubtitlesEnabled", OnSubtitlesToggled);

// Accessibility settings
EventSystem.Subscribe<AccessibilitySettings.ColorblindMode>("SetColorblindMode", OnColorblindModeChanged);
EventSystem.Subscribe<float>("SetUIScale", OnUIScaleChanged);
EventSystem.Subscribe<bool>("SetReducedMotion", OnReducedMotionChanged);

// Gameplay settings
EventSystem.Subscribe<float>("SetSanityDrainRate", OnSanityDrainRateChanged);
EventSystem.Subscribe<bool>("SetTutorialHints", OnTutorialHintsChanged);
```

---

## Usage Examples

### Example 1: Apply Video Settings

```csharp
using UnityEngine;

public class VideoExample : MonoBehaviour
{
    void Start()
    {
        // Access video settings
        VideoSettings video = SettingsManager.Instance.Video;

        // Set quality preset
        video.SetQualityPreset(VideoSettings.QualityPreset.High);

        // Custom settings
        video.shadowQuality = VideoSettings.ShadowQuality.Medium;
        video.textureQuality = VideoSettings.TextureQuality.High;
        video.antiAliasing = VideoSettings.AntiAliasing.TAA;

        // Apply settings
        video.ApplySettings();

        // Save settings
        SettingsManager.Instance.SaveAllSettings();
    }
}
```

### Example 2: Enable Colorblind Mode

```csharp
using UnityEngine;

public class ColorblindExample : MonoBehaviour
{
    void EnableDeuteranopia()
    {
        // Access accessibility settings
        AccessibilitySettings accessibility = SettingsManager.Instance.Accessibility;

        // Set colorblind mode
        accessibility.SetColorblindMode(AccessibilitySettings.ColorblindMode.Deuteranopia);

        // Apply settings
        accessibility.ApplySettings();

        // Save settings
        SettingsManager.Instance.SaveAllSettings();
    }

    // Transform individual colors for UI elements
    void UpdateUIColor(Image image)
    {
        ColorblindSimulator simulator = Camera.main.GetComponent<ColorblindSimulator>();
        if (simulator != null && simulator.IsActive)
        {
            Color transformedColor = simulator.TransformColor(image.color);
            image.color = transformedColor;
        }
    }
}
```

### Example 3: Display Subtitles

```csharp
using UnityEngine;

public class DialogueExample : MonoBehaviour
{
    void ShowDialogue(string speakerName, string dialogueText, AudioClip voiceClip)
    {
        // Show subtitle synced with audio
        SubtitleSystem.Instance.ShowSubtitle(speakerName, dialogueText, voiceClip);

        // Play audio
        AudioSource.PlayClipAtPoint(voiceClip, transform.position);
    }

    void ShowNarration(string text)
    {
        // Show subtitle with auto-duration
        SubtitleSystem.Instance.ShowSubtitle("Narrator", text);
    }
}
```

### Example 4: Show Tutorial Hints

```csharp
using UnityEngine;

public class TutorialExample : MonoBehaviour
{
    [SerializeField] private RectTransform fishingButtonUI;

    void OnPlayerFirstFishing()
    {
        // Show hint with arrow pointing to UI element
        TutorialHintSystem.Instance.ShowHint(
            "FirstFish",
            "Press SPACE to cast your fishing line!",
            isImportant: false,
            targetUI: fishingButtonUI,
            duration: 5f,
            allowRepeat: false
        );
    }

    void OnLowSanity()
    {
        // Show important safety hint (always shows even if hints disabled)
        TutorialHintSystem.Instance.ShowCommonHint(
            TutorialHintSystem.CommonHint.LowSanity
        );
    }
}
```

### Example 5: Input Remapping

```csharp
using UnityEngine;

public class InputExample : MonoBehaviour
{
    void RemapCastKey()
    {
        ControlSettings controls = SettingsManager.Instance.Controls;

        // Attempt to remap Cast action to Left Mouse
        bool success = controls.RemapKey("Cast", KeyCode.Mouse0);

        if (success)
        {
            Debug.Log("Successfully rebound Cast to Left Mouse");
            controls.SaveSettings();
        }
        else
        {
            Debug.Log("Rebind failed - conflict detected");
        }
    }

    void GetCurrentBinding()
    {
        ControlSettings controls = SettingsManager.Instance.Controls;

        // Get current key for action
        KeyCode castKey = controls.GetKeyBinding("Cast");
        Debug.Log($"Cast is currently bound to: {castKey}");
    }
}
```

### Example 6: Performance Monitoring

```csharp
using UnityEngine;

public class PerformanceExample : MonoBehaviour
{
    void Start()
    {
        // Enable FPS counter
        PerformanceMonitor.Instance.SetFPSCounter(true);

        // Enable auto-quality adjustment
        PerformanceMonitor.Instance.SetAutoQualityAdjustment(true);
    }

    void RunBenchmark()
    {
        // Run 60-second benchmark
        PerformanceMonitor.Instance.StartBenchmark(60f);

        // Subscribe to benchmark completion
        EventSystem.Subscribe<BenchmarkResults>("BenchmarkComplete", OnBenchmarkComplete);
    }

    void OnBenchmarkComplete(BenchmarkResults results)
    {
        Debug.Log($"Benchmark Results:");
        Debug.Log($"Average FPS: {results.averageFPS:F2}");
        Debug.Log($"Min FPS: {results.minFPS:F2}");
        Debug.Log($"Max FPS: {results.maxFPS:F2}");
    }
}
```

### Example 7: Difficulty Adjustment

```csharp
using UnityEngine;

public class DifficultyExample : MonoBehaviour
{
    void SetStoryMode()
    {
        GameplaySettings gameplay = SettingsManager.Instance.Gameplay;

        // Apply Story difficulty preset
        gameplay.ApplyDifficultyPreset(GameplaySettings.DifficultyLevel.Story);

        // Save and apply
        gameplay.SaveSettings();
        gameplay.ApplySettings();
    }

    void CustomDifficulty()
    {
        GameplaySettings gameplay = SettingsManager.Instance.Gameplay;

        // Set custom difficulty
        gameplay.difficulty = GameplaySettings.DifficultyLevel.Custom;
        gameplay.sanityDrainRate = GameplaySettings.SanityDrainRate.Half;
        gameplay.enemyDamageMultiplier = 0.75f;
        gameplay.fishAIAggression = GameplaySettings.AIAggressionLevel.Easy;

        gameplay.ApplySettings();
    }
}
```

---

## Event System

### Published Events

**Video Settings:**
- `VideoSettingsApplied` (bool)
- `QualityPresetChanged` (QualityPreset)
- `ResolutionChanged` (Resolution)
- `SetPostProcessing` (bool)
- `SetMotionBlur` (bool)
- `SetBloom` (bool)
- `SetParticleDensity` (float)

**Audio Settings:**
- `AudioSettingsApplied` (bool)
- `SetMasterVolume` (float)
- `SetMusicVolume` (float)
- `SetSFXVolume` (float)
- `SetAmbientVolume` (float)
- `SetUIVolume` (float)
- `SetSubtitlesEnabled` (bool)
- `SetSubtitleSize` (SubtitleSize)
- `SetSubtitleBackgroundOpacity` (float)

**Control Settings:**
- `ControlSchemeChanged` (ControlScheme)
- `SetInputDevice` (InputDevice)
- `SetControllerSensitivity` (Vector2)
- `SetInvertYAxis` (bool)
- `SetAimAssist` (AimAssistSettings)
- `SetVibration` (VibrationSettings)
- `KeyRebound` (KeyReboundEvent)
- `KeyBindConflict` (KeyBindConflictEvent)

**Gameplay Settings:**
- `GameplaySettingsApplied` (bool)
- `DifficultyChanged` (DifficultyLevel)
- `SetFishAIAggression` (AIAggressionLevel)
- `SetMinigameDifficulty` (MinigameDifficulty)
- `SetSanityDrainRate` (float)
- `SetEnemyDamageMultiplier` (float)
- `SetGameTimeScale` (float)
- `SetAutoSaveInterval` (float)
- `SetTutorialHints` (bool)
- `SetQuestMarkers` (bool)
- `SetPermadeath` (bool)

**Accessibility Settings:**
- `AccessibilitySettingsApplied` (bool)
- `AccessibilityModeEnabled` (string)
- `SetColorblindMode` (ColorblindMode)
- `SetUIScale` (float)
- `SetFontSize` (int)
- `SetHighContrastMode` (bool)
- `SetReducedMotion` (bool)
- `SetPhotosensitivityMode` (bool)
- `SetCameraShake` (bool)
- `SetScreenShake` (bool)
- `SetOneHandedMode` (bool)
- `SetAutoAimAssist` (AutoAimSettings)
- `SetButtonHoldDuration` (float)
- `SetScreenReaderSupport` (bool)
- `SetCursorSize` (float)
- `SetTooltipDelay` (float)

**General:**
- `SettingsInitialized` (bool)
- `SettingsSaved` (bool)
- `SettingsApplied` (bool)
- `SettingsReset` (string) - category name
- `SettingChanged` (SettingChangedEvent)

---

## API Reference

### SettingsManager

**Properties:**
- `Instance` - Singleton instance
- `Video` - VideoSettings component
- `Audio` - AudioSettings component
- `Controls` - ControlSettings component
- `Gameplay` - GameplaySettings component
- `Accessibility` - AccessibilitySettings component
- `IsInitialized` - Initialization status

**Methods:**
- `LoadAllSettings()` - Load all settings from PlayerPrefs
- `SaveAllSettings()` - Save all settings to PlayerPrefs
- `ApplyAllSettings()` - Apply all settings to game
- `ResetAllToDefaults()` - Reset all settings to defaults
- `ResetCategoryToDefaults(string category)` - Reset specific category

### VideoSettings

**Methods:**
- `ApplySettings()` - Apply video settings
- `SetQualityPreset(QualityPreset preset)` - Set quality preset
- `GetAvailableResolutions()` - Get all available resolutions
- `SaveSettings()` / `LoadSettings()` - Persistence
- `ResetToDefaults()` - Reset to platform defaults

### ColorblindSimulator

**Methods:**
- `SetColorblindMode(ColorblindMode mode)` - Set active colorblind mode
- `TransformColor(Color inputColor)` - Transform single color
- `CurrentMode` - Get current mode
- `IsActive` - Check if colorblind mode active

### SubtitleSystem

**Methods:**
- `ShowSubtitle(string speaker, string text)` - Show subtitle with auto-duration
- `ShowSubtitle(string speaker, string text, float duration)` - Show with duration
- `ShowSubtitle(string speaker, string text, AudioClip clip)` - Sync with audio
- `ClearQueue()` - Clear all queued subtitles
- `SetSubtitlesEnabled(bool enabled)` - Enable/disable

### TutorialHintSystem

**Methods:**
- `ShowHint(string id, string text)` - Show basic hint
- `ShowHint(string id, string text, bool important, RectTransform target, float duration, bool repeat)` - Full options
- `ShowCommonHint(CommonHint hint)` - Show predefined hint
- `ClearHints()` - Clear all hints
- `ResetHintHistory()` - Allow hints to show again
- `SetHintsEnabled(bool enabled)` - Enable/disable

### PerformanceMonitor

**Methods:**
- `ToggleFPSCounter()` - Toggle FPS display
- `SetFPSCounter(bool enabled)` - Set FPS display
- `SetAutoQualityAdjustment(bool enabled)` - Enable auto-quality
- `StartBenchmark(float duration)` - Start benchmark
- `GetCurrentFPS()` - Get current FPS
- `GetCPUTime()` - Get CPU time in ms

### UIScaler

**Methods:**
- `SetUIScale(UIScale scale)` / `SetUIScale(float scale)` - Set UI scale
- `SetFontSize(FontSize size)` / `SetFontSize(int size)` - Set font size
- `RegisterUIElement(GameObject element)` - Register new UI for scaling
- `UnregisterUIElement(GameObject element)` - Unregister UI
- `RefreshUIScaling()` - Refresh all scaling

---

## File Structure

```
Scripts/Accessibility/
├── SettingsManager.cs           (350 lines) - Central settings hub
├── VideoSettings.cs              (580 lines) - Graphics settings
├── AudioSettings.cs              (260 lines) - Audio settings
├── ControlSettings.cs            (470 lines) - Input settings
├── GameplaySettings.cs           (360 lines) - Gameplay settings
├── AccessibilitySettings.cs      (420 lines) - Accessibility settings
├── ColorblindSimulator.cs        (340 lines) - Colorblind modes
├── InputRemapping.cs             (410 lines) - UI for key remapping
├── PerformanceMonitor.cs         (340 lines) - FPS & performance
├── UIScaler.cs                   (320 lines) - Dynamic UI scaling
├── SubtitleSystem.cs             (310 lines) - Subtitle display
├── TutorialHintSystem.cs         (380 lines) - Tutorial hints
└── README.md                     (This file)

Total: ~4,540 lines of code + documentation
```

---

## Best Practices

1. **Always apply settings after changing them:**
   ```csharp
   settings.shadowQuality = ShadowQuality.High;
   settings.ApplySettings(); // Don't forget this!
   ```

2. **Save settings when player confirms:**
   ```csharp
   void OnApplyButtonClicked()
   {
       SettingsManager.Instance.ApplyAllSettings();
       SettingsManager.Instance.SaveAllSettings();
   }
   ```

3. **Subscribe to events in Start(), unsubscribe in OnDestroy():**
   ```csharp
   void Start()
   {
       EventSystem.Subscribe<float>("SetUIScale", OnUIScaleChanged);
   }

   void OnDestroy()
   {
       EventSystem.Unsubscribe<float>("SetUIScale", OnUIScaleChanged);
   }
   ```

4. **Use enums for type safety:**
   ```csharp
   // Good
   accessibility.colorblindMode = ColorblindMode.Deuteranopia;

   // Bad
   accessibility.colorblindMode = 2; // Magic number
   ```

5. **Handle null references:**
   ```csharp
   if (SettingsManager.Instance != null && SettingsManager.Instance.IsInitialized)
   {
       // Safe to use settings
   }
   ```

---

## Credits

**Colorblind Matrices:**
- Brettel, H., Viénot, F., & Mollon, J. D. (1997). Computerized simulation of color appearance for dichromats.
- Machado, G. M., Oliveira, M. M., & Fernandes, L. A. (2009). A physiologically-based model for simulation of color vision deficiency.

**Implementation:** Agent 21 - Accessibility & Settings Specialist

**Testing:** All accessibility features tested against WCAG 2.1 Level AA guidelines.

---

## Version History

- **v1.0.0** (Week 27-28): Initial implementation
  - Complete settings management system
  - 8 colorblind modes
  - Full accessibility features
  - Performance monitoring
  - Subtitle and tutorial systems

---

**For questions or issues, publish events to the EventSystem or consult the API reference above.**
