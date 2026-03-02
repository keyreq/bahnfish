using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Central HUD controller that manages all HUD elements.
/// Implements singleton pattern and subscribes to all relevant game events.
/// Handles show/hide animations and contextual visibility of UI elements.
/// </summary>
public class HUDManager : MonoBehaviour
{
    #region Singleton
    private static HUDManager _instance;
    public static HUDManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<HUDManager>();
            }
            return _instance;
        }
    }
    #endregion

    #region Inspector References
    [Header("HUD Components")]
    [Tooltip("Reference to the TimeDisplay component")]
    public TimeDisplay timeDisplay;

    [Tooltip("Reference to the SanityMeter component")]
    public SanityMeter sanityMeter;

    [Tooltip("Reference to the ResourceDisplay component")]
    public ResourceDisplay resourceDisplay;

    [Tooltip("Reference to the TensionMeter component (fishing only)")]
    public TensionMeter tensionMeter;

    [Header("Canvas Settings")]
    [Tooltip("Main HUD Canvas Group for fade in/out")]
    public CanvasGroup hudCanvasGroup;

    [Tooltip("Canvas Scaler component")]
    public CanvasScaler canvasScaler;

    [Header("Animation Settings")]
    [Tooltip("Duration of fade in/out animations")]
    [Range(0.1f, 2f)]
    public float fadeDuration = 0.5f;

    [Tooltip("Show HUD on start")]
    public bool showOnStart = true;

    [Header("Debug Settings")]
    [Tooltip("Enable debug logging")]
    public bool enableDebugLogging = false;
    #endregion

    #region Private Variables
    private bool isHUDVisible = true;
    private Coroutine fadeCoroutine;
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

        // Setup canvas if not assigned
        SetupCanvas();
    }

    private void Start()
    {
        // Subscribe to game events
        SubscribeToEvents();

        // Initial HUD visibility
        if (showOnStart)
        {
            ShowHUD(false); // Show immediately without fade
        }
        else
        {
            HideHUD(false);
        }

        if (enableDebugLogging)
        {
            Debug.Log("[HUDManager] Initialized successfully");
        }
    }

    private void OnDestroy()
    {
        // Clean up event subscriptions
        UnsubscribeFromEvents();
    }

    #endregion

    #region Canvas Setup

    /// <summary>
    /// Setup canvas scaler with proper settings
    /// </summary>
    private void SetupCanvas()
    {
        if (canvasScaler == null)
        {
            canvasScaler = GetComponent<CanvasScaler>();
        }

        if (canvasScaler != null)
        {
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasScaler.matchWidthOrHeight = 0.5f;
        }

        if (hudCanvasGroup == null)
        {
            hudCanvasGroup = GetComponent<CanvasGroup>();
            if (hudCanvasGroup == null)
            {
                hudCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    #endregion

    #region Event Subscription

    /// <summary>
    /// Subscribe to all relevant game events
    /// </summary>
    private void SubscribeToEvents()
    {
        EventSystem.Subscribe<GameState>("GameStateUpdated", OnGameStateUpdated);
        EventSystem.Subscribe<bool>("GamePaused", OnGamePaused);
        EventSystem.Subscribe("GameInitialized", OnGameInitialized);
    }

    /// <summary>
    /// Unsubscribe from all events to prevent memory leaks
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        EventSystem.Unsubscribe<GameState>("GameStateUpdated", OnGameStateUpdated);
        EventSystem.Unsubscribe<bool>("GamePaused", OnGamePaused);
        EventSystem.Unsubscribe("GameInitialized", OnGameInitialized);
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Called when game initializes
    /// </summary>
    private void OnGameInitialized()
    {
        if (enableDebugLogging)
        {
            Debug.Log("[HUDManager] Game initialized, updating HUD");
        }

        // Update HUD with initial game state
        if (GameManager.Instance != null)
        {
            UpdateHUD(GameManager.Instance.CurrentGameState);
        }
    }

    /// <summary>
    /// Called when game state is updated
    /// </summary>
    private void OnGameStateUpdated(GameState state)
    {
        UpdateHUD(state);
    }

    /// <summary>
    /// Called when game is paused or resumed
    /// </summary>
    private void OnGamePaused(bool isPaused)
    {
        // Could dim HUD or show pause indicator here
        if (enableDebugLogging)
        {
            Debug.Log($"[HUDManager] Game {(isPaused ? "paused" : "resumed")}");
        }
    }

    #endregion

    #region Public Interface

    /// <summary>
    /// Update all HUD elements with current game state
    /// </summary>
    public void UpdateHUD(GameState state)
    {
        if (state == null)
        {
            Debug.LogWarning("[HUDManager] Attempted to update HUD with null GameState");
            return;
        }

        // Update each HUD component (they will handle their own updates)
        // Components subscribe to specific events, so this is a fallback/force update
        if (enableDebugLogging)
        {
            Debug.Log($"[HUDManager] Updating HUD - Sanity: {state.sanity}, Money: {state.money}, Fuel: {state.fuel}");
        }
    }

    /// <summary>
    /// Show the HUD with optional fade animation
    /// </summary>
    public void ShowHUD(bool animated = true)
    {
        if (isHUDVisible && hudCanvasGroup.alpha >= 1f)
            return;

        isHUDVisible = true;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        if (animated)
        {
            fadeCoroutine = StartCoroutine(FadeHUD(1f));
        }
        else
        {
            hudCanvasGroup.alpha = 1f;
            hudCanvasGroup.interactable = true;
            hudCanvasGroup.blocksRaycasts = true;
        }

        if (enableDebugLogging)
        {
            Debug.Log("[HUDManager] Showing HUD");
        }
    }

    /// <summary>
    /// Hide the HUD with optional fade animation
    /// </summary>
    public void HideHUD(bool animated = true)
    {
        if (!isHUDVisible && hudCanvasGroup.alpha <= 0f)
            return;

        isHUDVisible = false;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        if (animated)
        {
            fadeCoroutine = StartCoroutine(FadeHUD(0f));
        }
        else
        {
            hudCanvasGroup.alpha = 0f;
            hudCanvasGroup.interactable = false;
            hudCanvasGroup.blocksRaycasts = false;
        }

        if (enableDebugLogging)
        {
            Debug.Log("[HUDManager] Hiding HUD");
        }
    }

    /// <summary>
    /// Toggle HUD visibility
    /// </summary>
    public void ToggleHUD(bool animated = true)
    {
        if (isHUDVisible)
        {
            HideHUD(animated);
        }
        else
        {
            ShowHUD(animated);
        }
    }

    /// <summary>
    /// Show or hide specific HUD elements based on context
    /// </summary>
    public void SetElementVisibility(string elementName, bool visible)
    {
        switch (elementName.ToLower())
        {
            case "tension":
            case "tensionmeter":
                if (tensionMeter != null)
                {
                    tensionMeter.gameObject.SetActive(visible);
                }
                break;

            case "sanity":
            case "sanitymeter":
                if (sanityMeter != null)
                {
                    sanityMeter.gameObject.SetActive(visible);
                }
                break;

            case "time":
            case "timedisplay":
                if (timeDisplay != null)
                {
                    timeDisplay.gameObject.SetActive(visible);
                }
                break;

            case "resources":
            case "resourcedisplay":
                if (resourceDisplay != null)
                {
                    resourceDisplay.gameObject.SetActive(visible);
                }
                break;

            default:
                Debug.LogWarning($"[HUDManager] Unknown HUD element: {elementName}");
                break;
        }
    }

    /// <summary>
    /// Check if HUD is currently visible
    /// </summary>
    public bool IsHUDVisible()
    {
        return isHUDVisible;
    }

    #endregion

    #region Animation

    /// <summary>
    /// Fade HUD in or out
    /// </summary>
    private IEnumerator FadeHUD(float targetAlpha)
    {
        float startAlpha = hudCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled time so it works when paused
            float t = elapsed / fadeDuration;
            hudCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        hudCanvasGroup.alpha = targetAlpha;
        hudCanvasGroup.interactable = targetAlpha > 0f;
        hudCanvasGroup.blocksRaycasts = targetAlpha > 0f;

        fadeCoroutine = null;
    }

    #endregion

    #region Debug Methods

    /// <summary>
    /// Force update all HUD components (for testing)
    /// </summary>
    [ContextMenu("Force Update HUD")]
    public void ForceUpdateHUD()
    {
        if (GameManager.Instance != null)
        {
            UpdateHUD(GameManager.Instance.CurrentGameState);
        }
        else
        {
            Debug.LogWarning("[HUDManager] Cannot force update - GameManager not found");
        }
    }

    /// <summary>
    /// Toggle debug logging
    /// </summary>
    [ContextMenu("Toggle Debug Logging")]
    public void ToggleDebugLogging()
    {
        enableDebugLogging = !enableDebugLogging;
        Debug.Log($"[HUDManager] Debug logging {(enableDebugLogging ? "enabled" : "disabled")}");
    }

    #endregion
}
