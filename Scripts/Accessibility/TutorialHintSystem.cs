using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Context-sensitive tutorial hint system.
/// Features:
/// - Context-aware hints based on player state
/// - Can be disabled in settings
/// - Hint history (prevents repeating hints)
/// - Important hints always show (safety warnings)
/// - Hint UI with optional arrow pointing to relevant UI
/// - Fade in/out animations
/// - Smart hint timing (not during combat/critical moments)
/// </summary>
public class TutorialHintSystem : MonoBehaviour
{
    private static TutorialHintSystem _instance;
    public static TutorialHintSystem Instance => _instance;

    [Header("UI References")]
    [SerializeField] private GameObject hintPanel;
    [SerializeField] private Text hintText;
    [SerializeField] private Image hintBackground;
    [SerializeField] private Image hintArrow;

    [Header("Settings")]
    [SerializeField] private bool hintsEnabled = true;
    [SerializeField] private float hintDuration = 5f;
    [SerializeField] private float fadeTime = 0.5f;
    [SerializeField] private float minTimeBetweenHints = 3f;

    [Header("Arrow")]
    [SerializeField] private bool useArrow = true;
    [SerializeField] private float arrowDistance = 100f;

    // Hint history
    private HashSet<string> shownHints = new HashSet<string>();
    private List<string> hintHistory = new List<string>();

    // Timing
    private float lastHintTime = 0f;
    private bool isShowingHint = false;
    private Coroutine hintCoroutine = null;

    // Hint queue
    private Queue<TutorialHint> hintQueue = new Queue<TutorialHint>();

    /// <summary>
    /// Tutorial hint data.
    /// </summary>
    private class TutorialHint
    {
        public string id;
        public string text;
        public bool isImportant;
        public RectTransform targetUI;
        public float duration;
        public bool allowRepeat;
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Subscribe to settings events
        EventSystem.Subscribe<bool>("SetTutorialHints", OnSetTutorialHints);

        // Initialize
        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
        }

        LoadHintHistory();
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<bool>("SetTutorialHints", OnSetTutorialHints);

        SaveHintHistory();

        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void Update()
    {
        // Process hint queue
        if (!isShowingHint && hintQueue.Count > 0)
        {
            float timeSinceLastHint = Time.time - lastHintTime;
            if (timeSinceLastHint >= minTimeBetweenHints)
            {
                ProcessNextHint();
            }
        }
    }

    #region Public Methods

    /// <summary>
    /// Show a tutorial hint.
    /// </summary>
    public void ShowHint(string hintId, string hintText)
    {
        ShowHint(hintId, hintText, false, null, hintDuration, false);
    }

    /// <summary>
    /// Show a tutorial hint with full options.
    /// </summary>
    /// <param name="hintId">Unique hint identifier</param>
    /// <param name="hintText">Hint text to display</param>
    /// <param name="isImportant">If true, shows even if hints are disabled</param>
    /// <param name="targetUI">UI element to point arrow at</param>
    /// <param name="duration">How long to show hint</param>
    /// <param name="allowRepeat">If true, can show again even if already shown</param>
    public void ShowHint(string hintId, string hintText, bool isImportant, RectTransform targetUI = null, float duration = 0f, bool allowRepeat = false)
    {
        // Check if hints are enabled (unless important)
        if (!hintsEnabled && !isImportant)
        {
            return;
        }

        // Check if already shown (unless important or repeat allowed)
        if (!isImportant && !allowRepeat && shownHints.Contains(hintId))
        {
            return;
        }

        // Check if safe to show hint
        if (!IsSafeToShowHint())
        {
            return;
        }

        // Create hint entry
        TutorialHint hint = new TutorialHint
        {
            id = hintId,
            text = hintText,
            isImportant = isImportant,
            targetUI = targetUI,
            duration = duration > 0f ? duration : hintDuration,
            allowRepeat = allowRepeat
        };

        // Add to queue
        hintQueue.Enqueue(hint);
    }

    /// <summary>
    /// Show a common hint (predefined hints).
    /// </summary>
    public void ShowCommonHint(CommonHint hint)
    {
        string hintId = hint.ToString();
        string hintText = GetCommonHintText(hint);
        bool isImportant = IsCommonHintImportant(hint);

        ShowHint(hintId, hintText, isImportant);
    }

    /// <summary>
    /// Clear all queued hints.
    /// </summary>
    public void ClearHints()
    {
        hintQueue.Clear();

        if (hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
            hintCoroutine = null;
        }

        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
        }

        isShowingHint = false;
    }

    /// <summary>
    /// Reset hint history (allows all hints to show again).
    /// </summary>
    public void ResetHintHistory()
    {
        shownHints.Clear();
        hintHistory.Clear();
        SaveHintHistory();
        Debug.Log("[TutorialHintSystem] Hint history reset");
    }

    /// <summary>
    /// Enable or disable hints.
    /// </summary>
    public void SetHintsEnabled(bool enabled)
    {
        hintsEnabled = enabled;

        if (!enabled)
        {
            ClearHints();
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Process next hint in queue.
    /// </summary>
    private void ProcessNextHint()
    {
        if (hintQueue.Count == 0) return;

        TutorialHint hint = hintQueue.Dequeue();

        // Mark as shown
        if (!hint.allowRepeat)
        {
            shownHints.Add(hint.id);
            hintHistory.Add(hint.id);
        }

        // Display hint
        hintCoroutine = StartCoroutine(DisplayHint(hint));
    }

    /// <summary>
    /// Display a hint.
    /// </summary>
    private IEnumerator DisplayHint(TutorialHint hint)
    {
        isShowingHint = true;
        lastHintTime = Time.time;

        // Set text
        if (hintText != null)
        {
            hintText.text = hint.text;
        }

        // Position arrow
        if (useArrow && hintArrow != null && hint.targetUI != null)
        {
            PositionArrow(hint.targetUI);
            hintArrow.gameObject.SetActive(true);
        }
        else if (hintArrow != null)
        {
            hintArrow.gameObject.SetActive(false);
        }

        // Show panel
        if (hintPanel != null)
        {
            hintPanel.SetActive(true);
        }

        // Fade in
        yield return StartCoroutine(FadePanel(0f, 1f, fadeTime));

        // Wait for duration
        yield return new WaitForSeconds(hint.duration);

        // Fade out
        yield return StartCoroutine(FadePanel(1f, 0f, fadeTime));

        // Hide panel
        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
        }

        isShowingHint = false;
    }

    /// <summary>
    /// Fade hint panel.
    /// </summary>
    private IEnumerator FadePanel(float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float alpha = Mathf.Lerp(fromAlpha, toAlpha, t);

            SetPanelAlpha(alpha);

            yield return null;
        }

        SetPanelAlpha(toAlpha);
    }

    /// <summary>
    /// Set panel alpha.
    /// </summary>
    private void SetPanelAlpha(float alpha)
    {
        if (hintBackground != null)
        {
            Color bgColor = hintBackground.color;
            bgColor.a = 0.8f * alpha;
            hintBackground.color = bgColor;
        }

        if (hintText != null)
        {
            Color textColor = hintText.color;
            textColor.a = alpha;
            hintText.color = textColor;
        }

        if (hintArrow != null && hintArrow.gameObject.activeSelf)
        {
            Color arrowColor = hintArrow.color;
            arrowColor.a = alpha;
            hintArrow.color = arrowColor;
        }
    }

    /// <summary>
    /// Position arrow to point at target UI element.
    /// </summary>
    private void PositionArrow(RectTransform target)
    {
        if (hintArrow == null || target == null) return;

        RectTransform arrowRect = hintArrow.GetComponent<RectTransform>();
        if (arrowRect == null) return;

        // Calculate direction from hint panel to target
        Vector3 direction = target.position - hintPanel.transform.position;
        direction.z = 0f;

        // Position arrow
        arrowRect.position = hintPanel.transform.position + direction.normalized * arrowDistance;

        // Rotate arrow to point at target
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        arrowRect.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    /// <summary>
    /// Check if it's safe to show a hint.
    /// Don't show during combat, cutscenes, or other critical moments.
    /// </summary>
    private bool IsSafeToShowHint()
    {
        // Check if game is paused
        if (Time.timeScale == 0f)
        {
            return false;
        }

        // Check for critical states via events
        bool isSafe = true;
        EventSystem.Publish("CheckSafeForHint", isSafe);

        return isSafe;
    }

    /// <summary>
    /// Save hint history to PlayerPrefs.
    /// </summary>
    private void SaveHintHistory()
    {
        string historyJson = string.Join(",", hintHistory.ToArray());
        PlayerPrefs.SetString("TutorialHintHistory", historyJson);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Load hint history from PlayerPrefs.
    /// </summary>
    private void LoadHintHistory()
    {
        if (PlayerPrefs.HasKey("TutorialHintHistory"))
        {
            string historyJson = PlayerPrefs.GetString("TutorialHintHistory");
            string[] hintArray = historyJson.Split(',');

            hintHistory.Clear();
            shownHints.Clear();

            foreach (string hintId in hintArray)
            {
                if (!string.IsNullOrEmpty(hintId))
                {
                    hintHistory.Add(hintId);
                    shownHints.Add(hintId);
                }
            }

            Debug.Log($"[TutorialHintSystem] Loaded {hintHistory.Count} hints from history");
        }
    }

    #endregion

    #region Common Hints

    /// <summary>
    /// Common hint types.
    /// </summary>
    public enum CommonHint
    {
        FirstFish,
        FirstNight,
        LowSanity,
        LowFuel,
        InventoryFull,
        NewLocation,
        DangerousWaters,
        BloodMoon,
        SaveGame,
        UpgradeAvailable
    }

    /// <summary>
    /// Get text for common hint.
    /// </summary>
    private string GetCommonHintText(CommonHint hint)
    {
        switch (hint)
        {
            case CommonHint.FirstFish:
                return "Press SPACE to cast your fishing line. Watch the tension meter!";
            case CommonHint.FirstNight:
                return "Night is falling. Stay alert - dangerous creatures emerge in the dark.";
            case CommonHint.LowSanity:
                return "WARNING: Your sanity is low. Return to shore or find a way to recover.";
            case CommonHint.LowFuel:
                return "Fuel is running low. Consider returning to refuel.";
            case CommonHint.InventoryFull:
                return "Your inventory is full. Sell or store items to make room.";
            case CommonHint.NewLocation:
                return "New location discovered! Check the map to see where you can explore.";
            case CommonHint.DangerousWaters:
                return "You're entering dangerous waters. Upgrade your boat before venturing further.";
            case CommonHint.BloodMoon:
                return "WARNING: Blood Moon rising! Extremely dangerous aberrant creatures will appear.";
            case CommonHint.SaveGame:
                return "Remember to save your progress regularly. Press ESC to open the menu.";
            case CommonHint.UpgradeAvailable:
                return "You have enough resources to purchase upgrades. Visit the shop!";
            default:
                return "Tutorial hint";
        }
    }

    /// <summary>
    /// Check if common hint is important (always shows).
    /// </summary>
    private bool IsCommonHintImportant(CommonHint hint)
    {
        switch (hint)
        {
            case CommonHint.LowSanity:
            case CommonHint.DangerousWaters:
            case CommonHint.BloodMoon:
                return true;
            default:
                return false;
        }
    }

    #endregion

    #region Event Handlers

    private void OnSetTutorialHints(bool enabled)
    {
        SetHintsEnabled(enabled);
    }

    #endregion

    #region Public Accessors

    public bool HintsEnabled => hintsEnabled;
    public int HintsShownCount => shownHints.Count;
    public bool IsShowingHint => isShowingHint;

    #endregion
}
