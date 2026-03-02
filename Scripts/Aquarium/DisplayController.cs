using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Agent 16: Aquarium & Breeding Specialist - DisplayController.cs
/// Handles visual fish display in tanks with swimming animations and behavior.
/// Manages tank decorations, viewing modes, and photo mode integration.
/// </summary>
public class DisplayController : MonoBehaviour
{
    private static DisplayController _instance;
    public static DisplayController Instance => _instance;

    [Header("Display Configuration")]
    [SerializeField] private GameObject fishDisplayPrefab;
    [SerializeField] private Transform tankDisplayParent;
    [SerializeField] private float swimSpeed = 1f;
    [SerializeField] private float patrolRadius = 5f;

    [Header("LOD Settings")]
    [SerializeField] private int maxVisibleFish = 50;
    [SerializeField] private float lodDistance = 20f;

    [Header("Viewing Modes")]
    [SerializeField] private ViewingMode currentViewMode = ViewingMode.Overview;
    [SerializeField] private DisplayFish focusedFish;

    [Header("Day/Night Behavior")]
    [SerializeField] private bool useDayNightBehavior = true;
    [SerializeField] private float nightActivityMultiplier = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool showDebugGizmos = true;

    // Active display instances
    private Dictionary<string, FishDisplayInstance> activeFishDisplays = new Dictionary<string, FishDisplayInstance>();

    // Current tank being viewed
    private string currentTankID;

    // Events
    public event System.Action<DisplayFish> OnFishSelected;
    public event System.Action OnViewModeChanged;

    #region Unity Lifecycle

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
        // Subscribe to aquarium events
        EventSystem.Subscribe<FishTankChangeData>("FishAddedToTank", OnFishAddedToTank);
        EventSystem.Subscribe<FishTankChangeData>("FishRemovedFromTank", OnFishRemovedFromTank);
        EventSystem.Subscribe<TimeChangedEventData>("TimeChanged", OnTimeChanged);

        if (enableDebugLogs)
        {
            Debug.Log("[DisplayController] Initialized");
        }
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<FishTankChangeData>("FishAddedToTank", OnFishAddedToTank);
        EventSystem.Unsubscribe<FishTankChangeData>("FishRemovedFromTank", OnFishRemovedFromTank);
        EventSystem.Unsubscribe<TimeChangedEventData>("TimeChanged", OnTimeChanged);
    }

    private void Update()
    {
        UpdateFishAnimations();
        UpdateLOD();
    }

    #endregion

    #region Tank Viewing

    /// <summary>
    /// Displays a specific tank.
    /// </summary>
    public void DisplayTank(string tankID)
    {
        // Clear current displays
        ClearAllDisplays();

        currentTankID = tankID;

        // Get fish in tank
        if (AquariumManager.Instance != null)
        {
            List<DisplayFish> fishList = AquariumManager.Instance.GetTankFish(tankID);

            foreach (var fish in fishList)
            {
                CreateFishDisplay(fish);
            }

            if (enableDebugLogs)
            {
                Debug.Log($"[DisplayController] Displaying tank {tankID} with {fishList.Count} fish");
            }
        }
    }

    /// <summary>
    /// Creates a visual display for a fish.
    /// </summary>
    private void CreateFishDisplay(DisplayFish fish)
    {
        if (fish == null || activeFishDisplays.ContainsKey(fish.uniqueID))
        {
            return;
        }

        // Get fish species data for prefab
        FishSpeciesData speciesData = FishDatabase.Instance?.GetFishByID(fish.speciesID);

        GameObject prefab = fishDisplayPrefab;

        if (speciesData != null && speciesData.fishPrefab != null)
        {
            prefab = speciesData.fishPrefab;
        }

        if (prefab == null)
        {
            Debug.LogWarning($"[DisplayController] No prefab for fish: {fish.speciesName}");
            return;
        }

        // Instantiate display
        Vector3 spawnPos = GetRandomSpawnPosition();
        GameObject displayObj = Instantiate(prefab, spawnPos, Quaternion.identity, tankDisplayParent);

        // Create display instance
        FishDisplayInstance display = new FishDisplayInstance
        {
            fish = fish,
            displayObject = displayObj,
            targetPosition = spawnPos,
            patrolCenter = spawnPos,
            isVisible = true
        };

        // Apply genetic appearance
        ApplyGeneticAppearance(display);

        activeFishDisplays[fish.uniqueID] = display;

        if (enableDebugLogs)
        {
            Debug.Log($"[DisplayController] Created display for {fish.speciesName}");
        }
    }

    /// <summary>
    /// Removes a fish display.
    /// </summary>
    private void RemoveFishDisplay(string fishID)
    {
        if (activeFishDisplays.TryGetValue(fishID, out FishDisplayInstance display))
        {
            if (display.displayObject != null)
            {
                Destroy(display.displayObject);
            }

            activeFishDisplays.Remove(fishID);

            if (enableDebugLogs)
            {
                Debug.Log($"[DisplayController] Removed display for fish {fishID}");
            }
        }
    }

    /// <summary>
    /// Clears all active displays.
    /// </summary>
    public void ClearAllDisplays()
    {
        foreach (var display in activeFishDisplays.Values)
        {
            if (display.displayObject != null)
            {
                Destroy(display.displayObject);
            }
        }

        activeFishDisplays.Clear();
        currentTankID = null;
    }

    #endregion

    #region Fish Animations

    /// <summary>
    /// Updates fish swimming animations.
    /// </summary>
    private void UpdateFishAnimations()
    {
        float deltaTime = Time.deltaTime;
        TimeOfDay timeOfDay = TimeOfDay.Day;

        // Get current time of day
        if (GameManager.Instance != null)
        {
            timeOfDay = GameManager.Instance.CurrentGameState.timeOfDay;
        }

        float activityMultiplier = (useDayNightBehavior && timeOfDay == TimeOfDay.Night) ?
            nightActivityMultiplier : 1f;

        foreach (var display in activeFishDisplays.Values)
        {
            if (!display.isVisible || display.displayObject == null)
            {
                continue;
            }

            // Update patrol behavior
            UpdatePatrolBehavior(display, deltaTime, activityMultiplier);

            // Update swimming animation
            UpdateSwimmingAnimation(display, deltaTime);
        }
    }

    /// <summary>
    /// Updates patrol movement for a fish.
    /// </summary>
    private void UpdatePatrolBehavior(FishDisplayInstance display, float deltaTime, float activityMultiplier)
    {
        // Check if reached target
        Vector3 toTarget = display.targetPosition - display.displayObject.transform.position;

        if (toTarget.magnitude < 0.5f)
        {
            // Pick new patrol point
            display.targetPosition = display.patrolCenter + Random.insideUnitSphere * patrolRadius;
        }

        // Move toward target
        Vector3 direction = toTarget.normalized;
        float speed = swimSpeed * activityMultiplier;

        // Apply genetic speed modifier
        if (display.fish.traits != null)
        {
            speed *= display.fish.traits.sizeMultiplier; // Larger fish swim differently
        }

        display.displayObject.transform.position += direction * speed * deltaTime;

        // Rotate to face movement direction
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            display.displayObject.transform.rotation = Quaternion.Slerp(
                display.displayObject.transform.rotation,
                targetRotation,
                deltaTime * 2f
            );
        }
    }

    /// <summary>
    /// Updates swimming animation (bobbing, tail movement, etc.).
    /// </summary>
    private void UpdateSwimmingAnimation(FishDisplayInstance display, float deltaTime)
    {
        // Simple bobbing motion
        float bobOffset = Mathf.Sin(Time.time * 2f) * 0.1f;
        Vector3 pos = display.displayObject.transform.position;
        pos.y += bobOffset * deltaTime;
        display.displayObject.transform.position = pos;

        // In full implementation, this would trigger animator parameters
    }

    #endregion

    #region Genetic Appearance

    /// <summary>
    /// Applies genetic traits to fish appearance.
    /// </summary>
    private void ApplyGeneticAppearance(FishDisplayInstance display)
    {
        if (display.fish.traits == null || display.displayObject == null)
        {
            return;
        }

        GeneticTraits traits = display.fish.traits;

        // Apply size
        display.displayObject.transform.localScale = Vector3.one * traits.sizeMultiplier;

        // Apply color
        Color color = GetColorFromVariant(traits.colorVariant);
        Renderer[] renderers = display.displayObject.GetComponentsInChildren<Renderer>();

        foreach (var renderer in renderers)
        {
            renderer.material.color = color;
        }

        // Apply bioluminescence
        if (traits.hasBioluminescence)
        {
            Light glowLight = display.displayObject.GetComponentInChildren<Light>();

            if (glowLight == null)
            {
                glowLight = display.displayObject.AddComponent<Light>();
            }

            glowLight.type = LightType.Point;
            glowLight.range = 3f;
            glowLight.intensity = 2f;
            glowLight.color = color;
        }

        // Apply aberrant effects
        if (traits.isAberrant)
        {
            ApplyAberrantEffects(display);
        }
    }

    /// <summary>
    /// Gets Unity Color from FishColor variant.
    /// </summary>
    private Color GetColorFromVariant(FishColor variant)
    {
        switch (variant)
        {
            case FishColor.Natural:
                return Color.white;
            case FishColor.Golden:
                return new Color(1f, 0.84f, 0f);
            case FishColor.Albino:
                return new Color(0.95f, 0.95f, 0.95f);
            case FishColor.Melanistic:
                return new Color(0.1f, 0.1f, 0.1f);
            case FishColor.Blue:
                return new Color(0.2f, 0.5f, 1f);
            case FishColor.Red:
                return new Color(1f, 0.2f, 0.2f);
            case FishColor.Purple:
                return new Color(0.6f, 0.2f, 0.8f);
            case FishColor.Rainbow:
                // Rainbow cycles through hues
                return Color.HSVToRGB(Mathf.PingPong(Time.time * 0.5f, 1f), 1f, 1f);
            default:
                return Color.white;
        }
    }

    /// <summary>
    /// Applies aberrant visual effects.
    /// </summary>
    private void ApplyAberrantEffects(FishDisplayInstance display)
    {
        // Add pulsing glow
        Light aberrantGlow = display.displayObject.AddComponent<Light>();
        aberrantGlow.type = LightType.Point;
        aberrantGlow.range = 5f;
        aberrantGlow.color = Color.green;
        aberrantGlow.intensity = Mathf.PingPong(Time.time * 2f, 3f);

        // Add particle effect in full implementation
    }

    #endregion

    #region Viewing Modes

    /// <summary>
    /// Sets the current viewing mode.
    /// </summary>
    public void SetViewMode(ViewingMode mode)
    {
        currentViewMode = mode;
        OnViewModeChanged?.Invoke();

        if (enableDebugLogs)
        {
            Debug.Log($"[DisplayController] View mode changed to {mode}");
        }
    }

    /// <summary>
    /// Focuses on a specific fish.
    /// </summary>
    public void FocusFish(DisplayFish fish)
    {
        focusedFish = fish;
        currentViewMode = ViewingMode.IndividualFocus;

        OnFishSelected?.Invoke(fish);
        EventSystem.Publish("FishFocused", fish);

        if (enableDebugLogs)
        {
            Debug.Log($"[DisplayController] Focused on {fish.speciesName}");
        }
    }

    /// <summary>
    /// Clears fish focus.
    /// </summary>
    public void ClearFocus()
    {
        focusedFish = null;
        currentViewMode = ViewingMode.Overview;
    }

    #endregion

    #region LOD Management

    /// <summary>
    /// Updates level of detail for fish displays.
    /// </summary>
    private void UpdateLOD()
    {
        if (Camera.main == null)
        {
            return;
        }

        Vector3 cameraPos = Camera.main.transform.position;
        int visibleCount = 0;

        foreach (var display in activeFishDisplays.Values)
        {
            if (display.displayObject == null)
            {
                continue;
            }

            float distance = Vector3.Distance(cameraPos, display.displayObject.transform.position);
            bool shouldBeVisible = distance < lodDistance && visibleCount < maxVisibleFish;

            if (display.isVisible != shouldBeVisible)
            {
                display.isVisible = shouldBeVisible;
                display.displayObject.SetActive(shouldBeVisible);
            }

            if (shouldBeVisible)
            {
                visibleCount++;
            }
        }
    }

    #endregion

    #region Utility

    /// <summary>
    /// Gets a random spawn position within the tank.
    /// </summary>
    private Vector3 GetRandomSpawnPosition()
    {
        return tankDisplayParent != null ?
            tankDisplayParent.position + Random.insideUnitSphere * patrolRadius :
            Random.insideUnitSphere * patrolRadius;
    }

    /// <summary>
    /// Gets the display instance for a fish.
    /// </summary>
    public FishDisplayInstance GetFishDisplay(string fishID)
    {
        activeFishDisplays.TryGetValue(fishID, out FishDisplayInstance display);
        return display;
    }

    #endregion

    #region Event Handlers

    private void OnFishAddedToTank(FishTankChangeData data)
    {
        // If currently viewing this tank, add the display
        if (data.tankID == currentTankID)
        {
            CreateFishDisplay(data.fish);
        }
    }

    private void OnFishRemovedFromTank(FishTankChangeData data)
    {
        RemoveFishDisplay(data.fish.uniqueID);
    }

    private void OnTimeChanged(TimeChangedEventData timeData)
    {
        // Day/night behavior changes are handled in UpdateFishAnimations
    }

    #endregion

    #region Debug

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos)
        {
            return;
        }

        // Draw patrol radii
        foreach (var display in activeFishDisplays.Values)
        {
            if (display.displayObject != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(display.patrolCenter, patrolRadius);

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(display.displayObject.transform.position, display.targetPosition);
            }
        }
    }

    #endregion
}

/// <summary>
/// Represents a visual display instance of a fish.
/// </summary>
[System.Serializable]
public class FishDisplayInstance
{
    public DisplayFish fish;
    public GameObject displayObject;
    public Vector3 targetPosition;
    public Vector3 patrolCenter;
    public bool isVisible;
}

/// <summary>
/// Viewing mode for aquarium display.
/// </summary>
[System.Serializable]
public enum ViewingMode
{
    Overview,           // View entire tank
    CloseUp,           // Closer view of tank area
    IndividualFocus    // Focus on specific fish
}
