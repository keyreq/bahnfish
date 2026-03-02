using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Dynamic input remapping UI system.
/// Features:
/// - Visual key rebinding interface
/// - Conflict detection (prevents duplicate bindings)
/// - Reset to defaults button
/// - Save custom bindings
/// - Support for keyboard, mouse, and gamepad
/// - Support for accessibility controllers
/// - Real-time visual feedback
/// </summary>
public class InputRemapping : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject remapButtonPrefab;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private Text statusText;
    [SerializeField] private Button resetButton;

    [Header("Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color listeningColor = Color.yellow;
    [SerializeField] private Color conflictColor = Color.red;

    private ControlSettings controlSettings;
    private Dictionary<string, RemapButton> remapButtons = new Dictionary<string, RemapButton>();
    private string currentlyRemapping = null;
    private bool isListening = false;

    /// <summary>
    /// Inner class to represent a remap button.
    /// </summary>
    private class RemapButton
    {
        public string actionName;
        public Button button;
        public Text keyText;
        public Image background;
    }

    private void Awake()
    {
        if (SettingsManager.Instance != null)
        {
            controlSettings = SettingsManager.Instance.Controls;
        }
    }

    private void Start()
    {
        InitializeRemapButtons();

        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetToDefaults);
        }

        // Subscribe to events
        EventSystem.Subscribe<KeyBindConflictEvent>("KeyBindConflict", OnKeyBindConflict);
        EventSystem.Subscribe<KeyReboundEvent>("KeyRebound", OnKeyRebound);
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<KeyBindConflictEvent>("KeyBindConflict", OnKeyBindConflict);
        EventSystem.Unsubscribe<KeyReboundEvent>("KeyRebound", OnKeyRebound);
    }

    /// <summary>
    /// Initialize remap buttons for all actions.
    /// </summary>
    private void InitializeRemapButtons()
    {
        if (controlSettings == null || buttonContainer == null)
        {
            Debug.LogWarning("[InputRemapping] Missing required references");
            return;
        }

        // Get all key bindings
        var bindings = controlSettings.GetAllBindings();

        foreach (var kvp in bindings)
        {
            CreateRemapButton(kvp.Key, kvp.Value);
        }
    }

    /// <summary>
    /// Create a remap button for an action.
    /// </summary>
    private void CreateRemapButton(string actionName, KeyCode currentKey)
    {
        if (remapButtonPrefab == null)
        {
            Debug.LogWarning("[InputRemapping] Remap button prefab not assigned");
            return;
        }

        GameObject buttonObj = Instantiate(remapButtonPrefab, buttonContainer);
        Button button = buttonObj.GetComponent<Button>();
        Text keyText = buttonObj.GetComponentInChildren<Text>();
        Image background = buttonObj.GetComponent<Image>();

        if (button != null && keyText != null)
        {
            // Set up button
            keyText.text = GetKeyDisplayName(currentKey);
            button.onClick.AddListener(() => StartRemap(actionName));

            // Store reference
            RemapButton remapButton = new RemapButton
            {
                actionName = actionName,
                button = button,
                keyText = keyText,
                background = background
            };

            remapButtons[actionName] = remapButton;

            // Set label (if exists)
            Text labelText = buttonObj.transform.Find("Label")?.GetComponent<Text>();
            if (labelText != null)
            {
                labelText.text = GetActionDisplayName(actionName);
            }
        }
    }

    /// <summary>
    /// Start remapping an action.
    /// </summary>
    private void StartRemap(string actionName)
    {
        if (isListening)
        {
            CancelRemap();
        }

        currentlyRemapping = actionName;
        isListening = true;

        // Visual feedback
        if (remapButtons.ContainsKey(actionName))
        {
            var remapButton = remapButtons[actionName];
            if (remapButton.background != null)
            {
                remapButton.background.color = listeningColor;
            }
            remapButton.keyText.text = "Press a key...";
        }

        if (statusText != null)
        {
            statusText.text = $"Press a key to bind to {GetActionDisplayName(actionName)}. Press ESC to cancel.";
        }

        StartCoroutine(ListenForInput());
    }

    /// <summary>
    /// Listen for input to remap.
    /// </summary>
    private IEnumerator ListenForInput()
    {
        while (isListening)
        {
            // Check for keyboard input
            foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(keyCode))
                {
                    // Cancel on Escape
                    if (keyCode == KeyCode.Escape)
                    {
                        CancelRemap();
                        yield break;
                    }

                    // Attempt to remap
                    bool success = controlSettings.RemapKey(currentlyRemapping, keyCode);

                    if (success)
                    {
                        CompleteRemap(keyCode);
                    }
                    else
                    {
                        // Conflict detected - ControlSettings will publish conflict event
                        // Visual feedback handled in OnKeyBindConflict
                    }

                    yield break;
                }
            }

            yield return null;
        }
    }

    /// <summary>
    /// Complete the remap operation.
    /// </summary>
    private void CompleteRemap(KeyCode newKey)
    {
        if (remapButtons.ContainsKey(currentlyRemapping))
        {
            var remapButton = remapButtons[currentlyRemapping];
            remapButton.keyText.text = GetKeyDisplayName(newKey);

            if (remapButton.background != null)
            {
                remapButton.background.color = normalColor;
            }
        }

        if (statusText != null)
        {
            statusText.text = $"{GetActionDisplayName(currentlyRemapping)} rebound to {GetKeyDisplayName(newKey)}";
        }

        // Save settings
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.SaveAllSettings();
        }

        isListening = false;
        currentlyRemapping = null;
    }

    /// <summary>
    /// Cancel the remap operation.
    /// </summary>
    private void CancelRemap()
    {
        if (currentlyRemapping != null && remapButtons.ContainsKey(currentlyRemapping))
        {
            var remapButton = remapButtons[currentlyRemapping];
            KeyCode currentKey = controlSettings.GetKeyBinding(currentlyRemapping);
            remapButton.keyText.text = GetKeyDisplayName(currentKey);

            if (remapButton.background != null)
            {
                remapButton.background.color = normalColor;
            }
        }

        if (statusText != null)
        {
            statusText.text = "Remap cancelled.";
        }

        isListening = false;
        currentlyRemapping = null;
    }

    /// <summary>
    /// Reset all bindings to defaults.
    /// </summary>
    private void ResetToDefaults()
    {
        if (controlSettings != null)
        {
            controlSettings.ResetToDefaults();
            controlSettings.ApplySettings();

            // Update UI
            RefreshAllButtons();

            if (statusText != null)
            {
                statusText.text = "All key bindings reset to defaults.";
            }

            // Save settings
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.SaveAllSettings();
            }
        }
    }

    /// <summary>
    /// Refresh all button displays.
    /// </summary>
    private void RefreshAllButtons()
    {
        var bindings = controlSettings.GetAllBindings();

        foreach (var kvp in bindings)
        {
            if (remapButtons.ContainsKey(kvp.Key))
            {
                var remapButton = remapButtons[kvp.Key];
                remapButton.keyText.text = GetKeyDisplayName(kvp.Value);

                if (remapButton.background != null)
                {
                    remapButton.background.color = normalColor;
                }
            }
        }
    }

    /// <summary>
    /// Event handler for key bind conflicts.
    /// </summary>
    private void OnKeyBindConflict(KeyBindConflictEvent eventData)
    {
        if (statusText != null)
        {
            statusText.text = $"Conflict! {GetKeyDisplayName(eventData.key)} is already bound to {GetActionDisplayName(eventData.conflictingAction)}.";
        }

        // Flash conflict color
        if (currentlyRemapping != null && remapButtons.ContainsKey(currentlyRemapping))
        {
            var remapButton = remapButtons[currentlyRemapping];
            if (remapButton.background != null)
            {
                StartCoroutine(FlashConflictColor(remapButton.background));
            }
        }

        // Reset to current key
        CancelRemap();
    }

    /// <summary>
    /// Event handler for successful key rebind.
    /// </summary>
    private void OnKeyRebound(KeyReboundEvent eventData)
    {
        // Already handled in CompleteRemap
    }

    /// <summary>
    /// Flash conflict color on background.
    /// </summary>
    private IEnumerator FlashConflictColor(Image background)
    {
        Color original = background.color;
        background.color = conflictColor;
        yield return new WaitForSeconds(0.5f);
        background.color = original;
    }

    /// <summary>
    /// Get display-friendly name for a key.
    /// </summary>
    private string GetKeyDisplayName(KeyCode key)
    {
        // Handle special cases
        switch (key)
        {
            case KeyCode.Mouse0:
                return "Left Mouse";
            case KeyCode.Mouse1:
                return "Right Mouse";
            case KeyCode.Mouse2:
                return "Middle Mouse";
            case KeyCode.Mouse3:
                return "Mouse 4";
            case KeyCode.Mouse4:
                return "Mouse 5";
            case KeyCode.LeftShift:
            case KeyCode.RightShift:
                return "Shift";
            case KeyCode.LeftControl:
            case KeyCode.RightControl:
                return "Ctrl";
            case KeyCode.LeftAlt:
            case KeyCode.RightAlt:
                return "Alt";
            case KeyCode.Alpha1:
            case KeyCode.Alpha2:
            case KeyCode.Alpha3:
            case KeyCode.Alpha4:
            case KeyCode.Alpha5:
            case KeyCode.Alpha6:
            case KeyCode.Alpha7:
            case KeyCode.Alpha8:
            case KeyCode.Alpha9:
            case KeyCode.Alpha0:
                return key.ToString().Replace("Alpha", "");
            default:
                return key.ToString();
        }
    }

    /// <summary>
    /// Get display-friendly name for an action.
    /// </summary>
    private string GetActionDisplayName(string actionName)
    {
        // Add spaces before capital letters
        string result = "";
        for (int i = 0; i < actionName.Length; i++)
        {
            if (i > 0 && char.IsUpper(actionName[i]))
            {
                result += " ";
            }
            result += actionName[i];
        }
        return result;
    }
}
