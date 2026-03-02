using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Agent 18: Photography Mode Specialist - FramingTools.cs
/// Provides composition aids and framing tools for photography mode.
/// Includes rule of thirds, golden ratio, center crosshair, and focus indicators.
/// </summary>
public class FramingTools : MonoBehaviour
{
    public static FramingTools Instance { get; private set; }

    [Header("Overlay Settings")]
    [SerializeField] private bool showRuleOfThirds = false;
    [SerializeField] private bool showGoldenRatio = false;
    [SerializeField] private bool showCenterCrosshair = false;
    [SerializeField] private bool showSafeFrames = false;
    [SerializeField] private bool showFocusPoint = false;

    [Header("UI References")]
    [SerializeField] private Canvas overlayCanvas;
    [SerializeField] private RectTransform ruleOfThirdsGrid;
    [SerializeField] private RectTransform goldenRatioSpiral;
    [SerializeField] private RectTransform centerCrosshair;
    [SerializeField] private RectTransform safeFrameGuide;
    [SerializeField] private RectTransform focusPointIndicator;

    [Header("Colors")]
    [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private Color focusColor = new Color(1f, 1f, 0f, 0.8f);

    [Header("Focus Point")]
    [SerializeField] private Vector2 focusPoint = new Vector2(0.5f, 0.5f);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Subscribe to photo mode events
        EventSystem.Subscribe("PhotoModeEntered", OnPhotoModeEntered);
        EventSystem.Subscribe("PhotoModeExited", OnPhotoModeExited);

        // Initially hide all overlays
        HideAllOverlays();
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe("PhotoModeEntered", OnPhotoModeEntered);
        EventSystem.Unsubscribe("PhotoModeExited", OnPhotoModeExited);
    }

    private void OnPhotoModeEntered()
    {
        // Show overlays based on settings
        UpdateOverlays();
    }

    private void OnPhotoModeExited()
    {
        // Hide all overlays
        HideAllOverlays();
    }

    private void Update()
    {
        // Update focus point based on mouse position in photo mode
        if (PhotoModeController.Instance != null && PhotoModeController.Instance.IsInPhotoMode())
        {
            if (showFocusPoint)
            {
                UpdateFocusPoint();
            }
        }
    }

    /// <summary>
    /// Updates the focus point indicator based on mouse position.
    /// </summary>
    private void UpdateFocusPoint()
    {
        if (focusPointIndicator != null)
        {
            Vector2 mousePos = Input.mousePosition;
            focusPointIndicator.position = mousePos;
        }
    }

    /// <summary>
    /// Toggles rule of thirds grid.
    /// </summary>
    public void ToggleRuleOfThirds()
    {
        showRuleOfThirds = !showRuleOfThirds;
        UpdateOverlays();
    }

    /// <summary>
    /// Toggles golden ratio spiral.
    /// </summary>
    public void ToggleGoldenRatio()
    {
        showGoldenRatio = !showGoldenRatio;
        UpdateOverlays();
    }

    /// <summary>
    /// Toggles center crosshair.
    /// </summary>
    public void ToggleCenterCrosshair()
    {
        showCenterCrosshair = !showCenterCrosshair;
        UpdateOverlays();
    }

    /// <summary>
    /// Toggles safe frame guides.
    /// </summary>
    public void ToggleSafeFrames()
    {
        showSafeFrames = !showSafeFrames;
        UpdateOverlays();
    }

    /// <summary>
    /// Toggles focus point indicator.
    /// </summary>
    public void ToggleFocusPoint()
    {
        showFocusPoint = !showFocusPoint;
        UpdateOverlays();
    }

    /// <summary>
    /// Updates all overlay visibility.
    /// </summary>
    private void UpdateOverlays()
    {
        if (ruleOfThirdsGrid != null)
        {
            ruleOfThirdsGrid.gameObject.SetActive(showRuleOfThirds);
        }

        if (goldenRatioSpiral != null)
        {
            goldenRatioSpiral.gameObject.SetActive(showGoldenRatio);
        }

        if (centerCrosshair != null)
        {
            centerCrosshair.gameObject.SetActive(showCenterCrosshair);
        }

        if (safeFrameGuide != null)
        {
            safeFrameGuide.gameObject.SetActive(showSafeFrames);
        }

        if (focusPointIndicator != null)
        {
            focusPointIndicator.gameObject.SetActive(showFocusPoint);
        }
    }

    /// <summary>
    /// Hides all overlays.
    /// </summary>
    private void HideAllOverlays()
    {
        showRuleOfThirds = false;
        showGoldenRatio = false;
        showCenterCrosshair = false;
        showSafeFrames = false;
        showFocusPoint = false;
        UpdateOverlays();
    }

    /// <summary>
    /// Analyzes photo composition based on rule of thirds.
    /// Returns a score from 0 to 1.
    /// </summary>
    public float AnalyzeComposition(Vector2 subjectPosition)
    {
        // Normalize subject position (0-1 range)
        Vector2 normalized = new Vector2(
            subjectPosition.x / Screen.width,
            subjectPosition.y / Screen.height
        );

        // Rule of thirds intersection points
        Vector2[] intersections = new Vector2[]
        {
            new Vector2(1f/3f, 1f/3f),
            new Vector2(2f/3f, 1f/3f),
            new Vector2(1f/3f, 2f/3f),
            new Vector2(2f/3f, 2f/3f)
        };

        // Find distance to nearest intersection
        float minDistance = float.MaxValue;
        foreach (Vector2 intersection in intersections)
        {
            float distance = Vector2.Distance(normalized, intersection);
            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }

        // Convert distance to score (closer = better)
        // Max distance is ~0.47 (corner to intersection)
        float score = 1f - Mathf.Clamp01(minDistance / 0.47f);

        return score;
    }

    /// <summary>
    /// Checks if subject is centered (for symmetrical compositions).
    /// Returns a score from 0 to 1.
    /// </summary>
    public float AnalyzeCentering(Vector2 subjectPosition)
    {
        Vector2 normalized = new Vector2(
            subjectPosition.x / Screen.width,
            subjectPosition.y / Screen.height
        );

        Vector2 center = new Vector2(0.5f, 0.5f);
        float distance = Vector2.Distance(normalized, center);

        // Max distance is ~0.7 (corner to center)
        float score = 1f - Mathf.Clamp01(distance / 0.7f);

        return score;
    }

    /// <summary>
    /// Determines if subject follows rule of thirds (within threshold).
    /// </summary>
    public bool IsFollowingRuleOfThirds(Vector2 subjectPosition, float threshold = 0.1f)
    {
        Vector2 normalized = new Vector2(
            subjectPosition.x / Screen.width,
            subjectPosition.y / Screen.height
        );

        // Check if near any rule of thirds line
        bool nearVerticalLine = Mathf.Abs(normalized.x - 1f/3f) < threshold ||
                                Mathf.Abs(normalized.x - 2f/3f) < threshold;
        bool nearHorizontalLine = Mathf.Abs(normalized.y - 1f/3f) < threshold ||
                                  Mathf.Abs(normalized.y - 2f/3f) < threshold;

        return nearVerticalLine || nearHorizontalLine;
    }

    /// <summary>
    /// Calculates the area occupied by subject in frame (0-1).
    /// </summary>
    public float CalculateSubjectCoverage(Rect subjectBounds)
    {
        float screenArea = Screen.width * Screen.height;
        float subjectArea = subjectBounds.width * subjectBounds.height;
        return Mathf.Clamp01(subjectArea / screenArea);
    }

    /// <summary>
    /// Checks if subject is in the golden ratio spiral position.
    /// </summary>
    public float AnalyzeGoldenRatio(Vector2 subjectPosition)
    {
        Vector2 normalized = new Vector2(
            subjectPosition.x / Screen.width,
            subjectPosition.y / Screen.height
        );

        // Golden ratio point (approximation)
        Vector2 goldenPoint = new Vector2(0.618f, 0.618f);
        float distance = Vector2.Distance(normalized, goldenPoint);

        float score = 1f - Mathf.Clamp01(distance / 0.5f);
        return score;
    }

    /// <summary>
    /// Sets the focus point position.
    /// </summary>
    public void SetFocusPoint(Vector2 position)
    {
        focusPoint = position;
    }

    /// <summary>
    /// Gets the current focus point.
    /// </summary>
    public Vector2 GetFocusPoint()
    {
        return focusPoint;
    }

    #region Public API

    /// <summary>
    /// Gets whether rule of thirds is visible.
    /// </summary>
    public bool IsRuleOfThirdsVisible()
    {
        return showRuleOfThirds;
    }

    /// <summary>
    /// Gets whether golden ratio is visible.
    /// </summary>
    public bool IsGoldenRatioVisible()
    {
        return showGoldenRatio;
    }

    /// <summary>
    /// Gets whether center crosshair is visible.
    /// </summary>
    public bool IsCenterCrosshairVisible()
    {
        return showCenterCrosshair;
    }

    /// <summary>
    /// Gets whether safe frames are visible.
    /// </summary>
    public bool AreSafeFramesVisible()
    {
        return showSafeFrames;
    }

    /// <summary>
    /// Gets whether focus point is visible.
    /// </summary>
    public bool IsFocusPointVisible()
    {
        return showFocusPoint;
    }

    /// <summary>
    /// Enables or disables rule of thirds.
    /// </summary>
    public void SetRuleOfThirds(bool enabled)
    {
        showRuleOfThirds = enabled;
        UpdateOverlays();
    }

    /// <summary>
    /// Enables or disables golden ratio.
    /// </summary>
    public void SetGoldenRatio(bool enabled)
    {
        showGoldenRatio = enabled;
        UpdateOverlays();
    }

    /// <summary>
    /// Enables or disables center crosshair.
    /// </summary>
    public void SetCenterCrosshair(bool enabled)
    {
        showCenterCrosshair = enabled;
        UpdateOverlays();
    }

    /// <summary>
    /// Enables or disables safe frames.
    /// </summary>
    public void SetSafeFrames(bool enabled)
    {
        showSafeFrames = enabled;
        UpdateOverlays();
    }

    /// <summary>
    /// Enables or disables focus point.
    /// </summary>
    public void SetFocusPoint(bool enabled)
    {
        showFocusPoint = enabled;
        UpdateOverlays();
    }

    #endregion
}
