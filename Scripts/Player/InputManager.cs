using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Manages input mapping and rebinding for player controls.
/// Supports keyboard, mouse, and controller inputs.
/// Allows runtime key rebinding and saving custom key bindings.
/// </summary>
public class InputManager : MonoBehaviour
{
    private static InputManager _instance;
    public static InputManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("InputManager");
                _instance = go.AddComponent<InputManager>();
            }
            return _instance;
        }
    }

    [System.Serializable]
    public class InputBinding
    {
        public string actionName;
        public KeyCode primaryKey;
        public KeyCode alternateKey;
        public string axisName;

        public InputBinding(string name, KeyCode primary, KeyCode alternate = KeyCode.None, string axis = "")
        {
            actionName = name;
            primaryKey = primary;
            alternateKey = alternate;
            axisName = axis;
        }
    }

    // Default input bindings
    private Dictionary<string, InputBinding> inputBindings = new Dictionary<string, InputBinding>();

    // Rebinding state
    private bool isRebinding = false;
    private string rebindingAction = "";
    private Action<KeyCode> rebindCallback;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeDefaultBindings();
        LoadBindings();
    }

    private void InitializeDefaultBindings()
    {
        // Movement
        inputBindings["MoveForward"] = new InputBinding("MoveForward", KeyCode.W, KeyCode.UpArrow, "Vertical");
        inputBindings["MoveBackward"] = new InputBinding("MoveBackward", KeyCode.S, KeyCode.DownArrow, "Vertical");
        inputBindings["TurnLeft"] = new InputBinding("TurnLeft", KeyCode.A, KeyCode.LeftArrow, "Horizontal");
        inputBindings["TurnRight"] = new InputBinding("TurnRight", KeyCode.D, KeyCode.RightArrow, "Horizontal");

        // Actions
        inputBindings["Interact"] = new InputBinding("Interact", KeyCode.E, KeyCode.F);
        inputBindings["Fish"] = new InputBinding("Fish", KeyCode.Mouse0, KeyCode.Space);
        inputBindings["Reel"] = new InputBinding("Reel", KeyCode.Mouse0, KeyCode.Space);
        inputBindings["Release"] = new InputBinding("Release", KeyCode.Mouse1, KeyCode.LeftControl);

        // UI
        inputBindings["Inventory"] = new InputBinding("Inventory", KeyCode.I, KeyCode.Tab);
        inputBindings["Journal"] = new InputBinding("Journal", KeyCode.J);
        inputBindings["Map"] = new InputBinding("Map", KeyCode.M);
        inputBindings["Pause"] = new InputBinding("Pause", KeyCode.Escape, KeyCode.P);

        // Utility
        inputBindings["Sprint"] = new InputBinding("Sprint", KeyCode.LeftShift);
        inputBindings["Crouch"] = new InputBinding("Crouch", KeyCode.C);
    }

    #region Input Checking

    /// <summary>
    /// Check if an action button is pressed this frame
    /// </summary>
    public bool GetButtonDown(string actionName)
    {
        if (!inputBindings.ContainsKey(actionName)) return false;

        InputBinding binding = inputBindings[actionName];
        return Input.GetKeyDown(binding.primaryKey) ||
               (binding.alternateKey != KeyCode.None && Input.GetKeyDown(binding.alternateKey));
    }

    /// <summary>
    /// Check if an action button is currently held
    /// </summary>
    public bool GetButton(string actionName)
    {
        if (!inputBindings.ContainsKey(actionName)) return false;

        InputBinding binding = inputBindings[actionName];
        return Input.GetKey(binding.primaryKey) ||
               (binding.alternateKey != KeyCode.None && Input.GetKey(binding.alternateKey));
    }

    /// <summary>
    /// Check if an action button was released this frame
    /// </summary>
    public bool GetButtonUp(string actionName)
    {
        if (!inputBindings.ContainsKey(actionName)) return false;

        InputBinding binding = inputBindings[actionName];
        return Input.GetKeyUp(binding.primaryKey) ||
               (binding.alternateKey != KeyCode.None && Input.GetKeyUp(binding.alternateKey));
    }

    /// <summary>
    /// Get axis value for movement inputs (-1 to 1)
    /// </summary>
    public float GetAxis(string actionName)
    {
        if (!inputBindings.ContainsKey(actionName)) return 0f;

        InputBinding binding = inputBindings[actionName];

        // Use Unity's Input axis if defined
        if (!string.IsNullOrEmpty(binding.axisName))
        {
            return Input.GetAxis(binding.axisName);
        }

        // Otherwise, check key states
        float value = 0f;
        if (Input.GetKey(binding.primaryKey)) value += 1f;
        if (Input.GetKey(binding.alternateKey)) value -= 1f;

        return Mathf.Clamp(value, -1f, 1f);
    }

    #endregion

    #region Key Rebinding

    /// <summary>
    /// Start rebinding an action
    /// </summary>
    public void StartRebinding(string actionName, bool isPrimary, Action<KeyCode> onComplete)
    {
        if (!inputBindings.ContainsKey(actionName))
        {
            Debug.LogWarning($"Action {actionName} not found");
            return;
        }

        isRebinding = true;
        rebindingAction = actionName;
        rebindCallback = onComplete;

        Debug.Log($"Press a key to rebind {actionName}...");
    }

    /// <summary>
    /// Cancel current rebinding operation
    /// </summary>
    public void CancelRebinding()
    {
        isRebinding = false;
        rebindingAction = "";
        rebindCallback = null;
    }

    private void Update()
    {
        if (isRebinding)
        {
            CheckForRebindInput();
        }
    }

    private void CheckForRebindInput()
    {
        // Check for any key press
        foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode))
            {
                // Ignore escape (cancel rebind)
                if (keyCode == KeyCode.Escape)
                {
                    CancelRebinding();
                    return;
                }

                // Set the new key binding
                if (inputBindings.ContainsKey(rebindingAction))
                {
                    inputBindings[rebindingAction].primaryKey = keyCode;
                    SaveBindings();

                    rebindCallback?.Invoke(keyCode);
                    Debug.Log($"{rebindingAction} rebound to {keyCode}");
                }

                isRebinding = false;
                rebindingAction = "";
                rebindCallback = null;
                return;
            }
        }
    }

    /// <summary>
    /// Reset all bindings to default
    /// </summary>
    public void ResetToDefaults()
    {
        InitializeDefaultBindings();
        SaveBindings();
        Debug.Log("Input bindings reset to defaults");
    }

    /// <summary>
    /// Get the current binding for an action
    /// </summary>
    public InputBinding GetBinding(string actionName)
    {
        return inputBindings.ContainsKey(actionName) ? inputBindings[actionName] : null;
    }

    /// <summary>
    /// Get all bindings (for UI display)
    /// </summary>
    public Dictionary<string, InputBinding> GetAllBindings()
    {
        return new Dictionary<string, InputBinding>(inputBindings);
    }

    #endregion

    #region Save/Load

    private void SaveBindings()
    {
        foreach (var kvp in inputBindings)
        {
            PlayerPrefs.SetInt($"Input_{kvp.Key}_Primary", (int)kvp.Value.primaryKey);
            PlayerPrefs.SetInt($"Input_{kvp.Key}_Alternate", (int)kvp.Value.alternateKey);
        }
        PlayerPrefs.Save();
    }

    private void LoadBindings()
    {
        foreach (var kvp in inputBindings)
        {
            if (PlayerPrefs.HasKey($"Input_{kvp.Key}_Primary"))
            {
                kvp.Value.primaryKey = (KeyCode)PlayerPrefs.GetInt($"Input_{kvp.Key}_Primary");
            }
            if (PlayerPrefs.HasKey($"Input_{kvp.Key}_Alternate"))
            {
                kvp.Value.alternateKey = (KeyCode)PlayerPrefs.GetInt($"Input_{kvp.Key}_Alternate");
            }
        }
    }

    #endregion

    #region Controller Support

    /// <summary>
    /// Check if a controller is connected
    /// </summary>
    public bool IsControllerConnected()
    {
        return Input.GetJoystickNames().Length > 0;
    }

    /// <summary>
    /// Get controller input for movement
    /// </summary>
    public Vector2 GetControllerMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        return new Vector2(horizontal, vertical);
    }

    #endregion
}
