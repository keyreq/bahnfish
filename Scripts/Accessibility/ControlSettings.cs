using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages control settings and input remapping.
/// Features:
/// - Input device selection (Keyboard/Mouse, Controller, Accessibility Controller)
/// - Key rebinding system with conflict detection
/// - Controller sensitivity (X/Y axis)
/// - Invert Y-axis option
/// - Aim assist settings
/// - Hold vs. Toggle for actions
/// - Control scheme presets (Standard, Alternate, Accessibility)
/// - Vibration settings
/// </summary>
public class ControlSettings : MonoBehaviour
{
    #region Enums

    public enum InputDevice
    {
        KeyboardMouse,
        Controller,
        AccessibilityController
    }

    public enum ControlScheme
    {
        Standard,
        Alternate,
        Accessibility,
        Custom
    }

    public enum InteractionMode
    {
        Hold,
        Toggle
    }

    #endregion

    #region Settings Variables

    [Header("Input Device")]
    public InputDevice currentInputDevice = InputDevice.KeyboardMouse;
    public ControlScheme controlScheme = ControlScheme.Standard;

    [Header("Controller Settings")]
    [Range(0.1f, 3f)]
    public float controllerSensitivityX = 1f;
    [Range(0.1f, 3f)]
    public float controllerSensitivityY = 1f;
    public bool invertYAxis = false;

    [Header("Aim Assist")]
    public bool aimAssistEnabled = false;
    [Range(0f, 1f)]
    public float aimAssistStrength = 0.5f;

    [Header("Interaction Mode")]
    public InteractionMode interactionMode = InteractionMode.Hold;

    [Header("Vibration")]
    public bool vibrationEnabled = true;
    [Range(0f, 1f)]
    public float vibrationIntensity = 1f;

    #endregion

    private Dictionary<string, KeyCode> keyBindings = new Dictionary<string, KeyCode>();
    private Dictionary<string, KeyCode> defaultKeyBindings = new Dictionary<string, KeyCode>();
    private bool isInitialized = false;

    /// <summary>
    /// Initialize control settings.
    /// </summary>
    public void Initialize()
    {
        if (isInitialized) return;

        InitializeDefaultBindings();
        CopyDefaultBindings();

        isInitialized = true;
        Debug.Log("[ControlSettings] Initialized");
    }

    /// <summary>
    /// Initialize default key bindings for all control schemes.
    /// </summary>
    private void InitializeDefaultBindings()
    {
        defaultKeyBindings.Clear();

        // Movement
        defaultKeyBindings["MoveForward"] = KeyCode.W;
        defaultKeyBindings["MoveBackward"] = KeyCode.S;
        defaultKeyBindings["TurnLeft"] = KeyCode.A;
        defaultKeyBindings["TurnRight"] = KeyCode.D;

        // Actions
        defaultKeyBindings["Cast"] = KeyCode.Space;
        defaultKeyBindings["Reel"] = KeyCode.Space;
        defaultKeyBindings["Interact"] = KeyCode.E;
        defaultKeyBindings["Release"] = KeyCode.R;

        // UI
        defaultKeyBindings["Inventory"] = KeyCode.Tab;
        defaultKeyBindings["Journal"] = KeyCode.J;
        defaultKeyBindings["Map"] = KeyCode.M;
        defaultKeyBindings["Pause"] = KeyCode.Escape;
        defaultKeyBindings["QuickSlot1"] = KeyCode.Alpha1;
        defaultKeyBindings["QuickSlot2"] = KeyCode.Alpha2;
        defaultKeyBindings["QuickSlot3"] = KeyCode.Alpha3;
        defaultKeyBindings["QuickSlot4"] = KeyCode.Alpha4;

        // Camera
        defaultKeyBindings["CameraZoomIn"] = KeyCode.Mouse3;
        defaultKeyBindings["CameraZoomOut"] = KeyCode.Mouse4;

        // Accessibility
        defaultKeyBindings["ToggleSubtitles"] = KeyCode.H;
        defaultKeyBindings["ToggleAssist"] = KeyCode.F1;
    }

    /// <summary>
    /// Copy default bindings to active bindings.
    /// </summary>
    private void CopyDefaultBindings()
    {
        keyBindings.Clear();
        foreach (var kvp in defaultKeyBindings)
        {
            keyBindings[kvp.Key] = kvp.Value;
        }
    }

    #region Control Schemes

    /// <summary>
    /// Apply control scheme preset.
    /// </summary>
    public void ApplyControlScheme(ControlScheme scheme)
    {
        controlScheme = scheme;

        switch (scheme)
        {
            case ControlScheme.Standard:
                ApplyStandardScheme();
                break;
            case ControlScheme.Alternate:
                ApplyAlternateScheme();
                break;
            case ControlScheme.Accessibility:
                ApplyAccessibilityScheme();
                break;
            case ControlScheme.Custom:
                // Keep current bindings
                break;
        }

        EventSystem.Publish("ControlSchemeChanged", scheme);
    }

    /// <summary>
    /// Apply Standard control scheme.
    /// WASD movement, Space for actions, standard PC layout.
    /// </summary>
    private void ApplyStandardScheme()
    {
        keyBindings["MoveForward"] = KeyCode.W;
        keyBindings["MoveBackward"] = KeyCode.S;
        keyBindings["TurnLeft"] = KeyCode.A;
        keyBindings["TurnRight"] = KeyCode.D;
        keyBindings["Cast"] = KeyCode.Space;
        keyBindings["Reel"] = KeyCode.Space;
        keyBindings["Interact"] = KeyCode.E;
        keyBindings["Release"] = KeyCode.R;
        keyBindings["Inventory"] = KeyCode.Tab;
        keyBindings["Map"] = KeyCode.M;
    }

    /// <summary>
    /// Apply Alternate control scheme.
    /// Arrow keys for movement, different action keys.
    /// </summary>
    private void ApplyAlternateScheme()
    {
        keyBindings["MoveForward"] = KeyCode.UpArrow;
        keyBindings["MoveBackward"] = KeyCode.DownArrow;
        keyBindings["TurnLeft"] = KeyCode.LeftArrow;
        keyBindings["TurnRight"] = KeyCode.RightArrow;
        keyBindings["Cast"] = KeyCode.Return;
        keyBindings["Reel"] = KeyCode.Return;
        keyBindings["Interact"] = KeyCode.RightControl;
        keyBindings["Release"] = KeyCode.Delete;
        keyBindings["Inventory"] = KeyCode.I;
        keyBindings["Map"] = KeyCode.N;
    }

    /// <summary>
    /// Apply Accessibility control scheme.
    /// One-handed controls, all on left side of keyboard.
    /// </summary>
    private void ApplyAccessibilityScheme()
    {
        keyBindings["MoveForward"] = KeyCode.W;
        keyBindings["MoveBackward"] = KeyCode.S;
        keyBindings["TurnLeft"] = KeyCode.A;
        keyBindings["TurnRight"] = KeyCode.D;
        keyBindings["Cast"] = KeyCode.Space;
        keyBindings["Reel"] = KeyCode.Space;
        keyBindings["Interact"] = KeyCode.E;
        keyBindings["Release"] = KeyCode.Q;
        keyBindings["Inventory"] = KeyCode.Tab;
        keyBindings["Map"] = KeyCode.G;

        // Enable auto-assist features for accessibility
        aimAssistEnabled = true;
        aimAssistStrength = 0.75f;
        interactionMode = InteractionMode.Toggle;
    }

    #endregion

    #region Key Remapping

    /// <summary>
    /// Rebind a key for an action.
    /// Checks for conflicts.
    /// </summary>
    /// <param name="action">Action name</param>
    /// <param name="newKey">New key to bind</param>
    /// <returns>True if rebind successful, false if conflict detected</returns>
    public bool RemapKey(string action, KeyCode newKey)
    {
        if (!keyBindings.ContainsKey(action))
        {
            Debug.LogWarning($"[ControlSettings] Action '{action}' not found");
            return false;
        }

        // Check for conflicts
        string conflictAction = GetConflictingAction(newKey, action);
        if (!string.IsNullOrEmpty(conflictAction))
        {
            Debug.LogWarning($"[ControlSettings] Key {newKey} is already bound to '{conflictAction}'");
            EventSystem.Publish("KeyBindConflict", new KeyBindConflictEvent(action, conflictAction, newKey));
            return false;
        }

        // Rebind
        keyBindings[action] = newKey;
        controlScheme = ControlScheme.Custom;

        Debug.Log($"[ControlSettings] Rebound '{action}' to {newKey}");
        EventSystem.Publish("KeyRebound", new KeyReboundEvent(action, newKey));

        return true;
    }

    /// <summary>
    /// Get action that conflicts with the given key.
    /// </summary>
    /// <param name="key">Key to check</param>
    /// <param name="excludeAction">Action to exclude from conflict check</param>
    /// <returns>Conflicting action name, or empty string if no conflict</returns>
    public string GetConflictingAction(KeyCode key, string excludeAction = "")
    {
        foreach (var kvp in keyBindings)
        {
            if (kvp.Key != excludeAction && kvp.Value == key)
            {
                return kvp.Key;
            }
        }
        return string.Empty;
    }

    /// <summary>
    /// Get current key binding for an action.
    /// </summary>
    public KeyCode GetKeyBinding(string action)
    {
        if (keyBindings.ContainsKey(action))
        {
            return keyBindings[action];
        }
        return KeyCode.None;
    }

    /// <summary>
    /// Get all key bindings.
    /// </summary>
    public Dictionary<string, KeyCode> GetAllBindings()
    {
        return new Dictionary<string, KeyCode>(keyBindings);
    }

    #endregion

    #region Apply Settings

    /// <summary>
    /// Apply all control settings.
    /// </summary>
    public void ApplySettings()
    {
        // Publish settings for input systems to handle
        EventSystem.Publish("SetInputDevice", currentInputDevice);
        EventSystem.Publish("SetControllerSensitivity", new Vector2(controllerSensitivityX, controllerSensitivityY));
        EventSystem.Publish("SetInvertYAxis", invertYAxis);
        EventSystem.Publish("SetAimAssist", new AimAssistSettings { enabled = aimAssistEnabled, strength = aimAssistStrength });
        EventSystem.Publish("SetInteractionMode", interactionMode);
        EventSystem.Publish("SetVibration", new VibrationSettings { enabled = vibrationEnabled, intensity = vibrationIntensity });
        EventSystem.Publish("SetKeyBindings", keyBindings);

        Debug.Log("[ControlSettings] Settings applied");
    }

    #endregion

    #region Save/Load

    /// <summary>
    /// Save settings to PlayerPrefs.
    /// </summary>
    public void SaveSettings()
    {
        PlayerPrefs.SetInt("Controls_InputDevice", (int)currentInputDevice);
        PlayerPrefs.SetInt("Controls_ControlScheme", (int)controlScheme);
        PlayerPrefs.SetFloat("Controls_SensitivityX", controllerSensitivityX);
        PlayerPrefs.SetFloat("Controls_SensitivityY", controllerSensitivityY);
        PlayerPrefs.SetInt("Controls_InvertY", invertYAxis ? 1 : 0);
        PlayerPrefs.SetInt("Controls_AimAssist", aimAssistEnabled ? 1 : 0);
        PlayerPrefs.SetFloat("Controls_AimAssistStrength", aimAssistStrength);
        PlayerPrefs.SetInt("Controls_InteractionMode", (int)interactionMode);
        PlayerPrefs.SetInt("Controls_Vibration", vibrationEnabled ? 1 : 0);
        PlayerPrefs.SetFloat("Controls_VibrationIntensity", vibrationIntensity);

        // Save key bindings
        foreach (var kvp in keyBindings)
        {
            PlayerPrefs.SetInt($"KeyBind_{kvp.Key}", (int)kvp.Value);
        }
    }

    /// <summary>
    /// Load settings from PlayerPrefs.
    /// </summary>
    public void LoadSettings()
    {
        currentInputDevice = (InputDevice)PlayerPrefs.GetInt("Controls_InputDevice", 0);
        controlScheme = (ControlScheme)PlayerPrefs.GetInt("Controls_ControlScheme", 0);
        controllerSensitivityX = PlayerPrefs.GetFloat("Controls_SensitivityX", 1f);
        controllerSensitivityY = PlayerPrefs.GetFloat("Controls_SensitivityY", 1f);
        invertYAxis = PlayerPrefs.GetInt("Controls_InvertY", 0) == 1;
        aimAssistEnabled = PlayerPrefs.GetInt("Controls_AimAssist", 0) == 1;
        aimAssistStrength = PlayerPrefs.GetFloat("Controls_AimAssistStrength", 0.5f);
        interactionMode = (InteractionMode)PlayerPrefs.GetInt("Controls_InteractionMode", 0);
        vibrationEnabled = PlayerPrefs.GetInt("Controls_Vibration", 1) == 1;
        vibrationIntensity = PlayerPrefs.GetFloat("Controls_VibrationIntensity", 1f);

        // Load key bindings
        foreach (var kvp in defaultKeyBindings)
        {
            if (PlayerPrefs.HasKey($"KeyBind_{kvp.Key}"))
            {
                keyBindings[kvp.Key] = (KeyCode)PlayerPrefs.GetInt($"KeyBind_{kvp.Key}");
            }
        }
    }

    #endregion

    #region Reset

    /// <summary>
    /// Reset to default settings.
    /// </summary>
    public void ResetToDefaults()
    {
        currentInputDevice = InputDevice.KeyboardMouse;
        controlScheme = ControlScheme.Standard;
        controllerSensitivityX = 1f;
        controllerSensitivityY = 1f;
        invertYAxis = false;
        aimAssistEnabled = false;
        aimAssistStrength = 0.5f;
        interactionMode = InteractionMode.Hold;
        vibrationEnabled = true;
        vibrationIntensity = 1f;

        CopyDefaultBindings();
    }

    #endregion

    #region Data Transfer

    /// <summary>
    /// Get settings data for save system integration.
    /// </summary>
    public ControlSettingsData GetData()
    {
        var data = new ControlSettingsData
        {
            inputDevice = (int)currentInputDevice,
            controlScheme = (int)controlScheme,
            sensitivityX = controllerSensitivityX,
            sensitivityY = controllerSensitivityY,
            invertYAxis = invertYAxis,
            aimAssistEnabled = aimAssistEnabled,
            aimAssistStrength = aimAssistStrength,
            interactionMode = (int)interactionMode,
            vibrationEnabled = vibrationEnabled,
            vibrationIntensity = vibrationIntensity
        };

        // Serialize key bindings
        data.keyBindings = new Dictionary<string, int>();
        foreach (var kvp in keyBindings)
        {
            data.keyBindings[kvp.Key] = (int)kvp.Value;
        }

        return data;
    }

    /// <summary>
    /// Set settings from data structure.
    /// </summary>
    public void SetData(ControlSettingsData data)
    {
        if (data == null) return;

        currentInputDevice = (InputDevice)data.inputDevice;
        controlScheme = (ControlScheme)data.controlScheme;
        controllerSensitivityX = data.sensitivityX;
        controllerSensitivityY = data.sensitivityY;
        invertYAxis = data.invertYAxis;
        aimAssistEnabled = data.aimAssistEnabled;
        aimAssistStrength = data.aimAssistStrength;
        interactionMode = (InteractionMode)data.interactionMode;
        vibrationEnabled = data.vibrationEnabled;
        vibrationIntensity = data.vibrationIntensity;

        // Deserialize key bindings
        if (data.keyBindings != null)
        {
            keyBindings.Clear();
            foreach (var kvp in data.keyBindings)
            {
                keyBindings[kvp.Key] = (KeyCode)kvp.Value;
            }
        }
    }

    #endregion
}

/// <summary>
/// Serializable control settings data.
/// </summary>
[Serializable]
public class ControlSettingsData
{
    public int inputDevice = 0;
    public int controlScheme = 0;
    public float sensitivityX = 1f;
    public float sensitivityY = 1f;
    public bool invertYAxis = false;
    public bool aimAssistEnabled = false;
    public float aimAssistStrength = 0.5f;
    public int interactionMode = 0;
    public bool vibrationEnabled = true;
    public float vibrationIntensity = 1f;
    public Dictionary<string, int> keyBindings = new Dictionary<string, int>();
}

/// <summary>
/// Aim assist settings event data.
/// </summary>
[Serializable]
public class AimAssistSettings
{
    public bool enabled;
    public float strength;
}

/// <summary>
/// Vibration settings event data.
/// </summary>
[Serializable]
public class VibrationSettings
{
    public bool enabled;
    public float intensity;
}

/// <summary>
/// Key bind conflict event data.
/// </summary>
[Serializable]
public class KeyBindConflictEvent
{
    public string action;
    public string conflictingAction;
    public KeyCode key;

    public KeyBindConflictEvent(string act, string conflict, KeyCode k)
    {
        action = act;
        conflictingAction = conflict;
        key = k;
    }
}

/// <summary>
/// Key rebound event data.
/// </summary>
[Serializable]
public class KeyReboundEvent
{
    public string action;
    public KeyCode newKey;

    public KeyReboundEvent(string act, KeyCode key)
    {
        action = act;
        newKey = key;
    }
}
