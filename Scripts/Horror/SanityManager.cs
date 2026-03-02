using UnityEngine;
using System;

/// <summary>
/// Core sanity system managing player mental state.
/// Drains sanity at night, triggers horror effects at low sanity.
/// Agent 7: Sanity & Horror System
/// </summary>
public class SanityManager : MonoBehaviour
{
    private static SanityManager _instance;
    public static SanityManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SanityManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("SanityManager");
                    _instance = go.AddComponent<SanityManager>();
                }
            }
            return _instance;
        }
    }

    [Header("Sanity Configuration")]
    [SerializeField] private float maxSanity = 100f;
    [SerializeField] private float currentSanity = 100f;

    [Header("Drain Rates")]
    [SerializeField] private float dayDrainRate = 0f; // No drain during day
    [SerializeField] private float nightDrainRate = 0.5f; // 0.5 per second at night
    [SerializeField] private float duskDrainRate = 0.2f; // Transition periods

    [Header("Lantern Upgrades")]
    [SerializeField] private int lanternLevel = 1;
    [SerializeField] private float lanternReductionPerLevel = 0.1f; // 10% reduction per level

    [Header("Critical Thresholds")]
    [SerializeField] private float lowSanityThreshold = 30f; // Hazards spawn
    [SerializeField] private float criticalSanityThreshold = 10f; // Major dangers
    [SerializeField] private float insaneThreshold = 0f; // Curse trigger

    [Header("Talisman Effect")]
    [SerializeField] private bool talismanActive = false;
    [SerializeField] private float talismanDuration = 60f; // 60 seconds
    private float talismanTimer = 0f;

    [Header("Status")]
    [SerializeField] private bool isDraining = false;
    [SerializeField] private TimeOfDay lastTimeOfDay = TimeOfDay.Day;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;

    // Events
    public event Action<float> OnSanityChanged;
    public event Action OnInsanityTrigger;
    public event Action OnSanityRestored;
    public event Action<float> OnLowSanity; // Passes sanity percentage

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        currentSanity = maxSanity;
    }

    private void Start()
    {
        // Subscribe to time events
        EventSystem.Subscribe<TimeOfDay>("TimeOfDayChanged", OnTimeOfDayChanged);

        // Initialize from GameState if available
        if (GameManager.Instance != null)
        {
            currentSanity = GameManager.Instance.CurrentGameState.sanity;
        }

        if (enableDebugLogging)
        {
            Debug.Log("[SanityManager] Initialized. Starting sanity: " + currentSanity);
        }
    }

    private void Update()
    {
        // Update talisman timer
        if (talismanActive)
        {
            talismanTimer -= Time.deltaTime;
            if (talismanTimer <= 0f)
            {
                talismanActive = false;
                if (enableDebugLogging)
                {
                    Debug.Log("[SanityManager] Talisman protection expired");
                }
            }
        }

        // Drain sanity if conditions met
        if (isDraining && !talismanActive)
        {
            DrainSanity();
        }

        // Check for threshold events
        CheckThresholds();
    }

    private void DrainSanity()
    {
        float drainRate = GetCurrentDrainRate();

        if (drainRate > 0f)
        {
            float previousSanity = currentSanity;
            currentSanity -= drainRate * Time.deltaTime;
            currentSanity = Mathf.Clamp(currentSanity, 0f, maxSanity);

            if (currentSanity != previousSanity)
            {
                UpdateGameState();
                OnSanityChanged?.Invoke(currentSanity);
                EventSystem.Publish("SanityChanged", currentSanity);
            }

            // Trigger insanity event if we just hit zero
            if (currentSanity == 0f && previousSanity > 0f)
            {
                TriggerInsanity();
            }
        }
    }

    private float GetCurrentDrainRate()
    {
        float baseDrain = 0f;

        switch (lastTimeOfDay)
        {
            case TimeOfDay.Day:
            case TimeOfDay.Dawn:
                baseDrain = dayDrainRate;
                break;
            case TimeOfDay.Dusk:
                baseDrain = duskDrainRate;
                break;
            case TimeOfDay.Night:
                baseDrain = nightDrainRate;
                break;
        }

        // Apply lantern reduction
        float lanternReduction = 1f - (lanternReductionPerLevel * (lanternLevel - 1));
        lanternReduction = Mathf.Clamp(lanternReduction, 0.1f, 1f); // Min 10% drain

        return baseDrain * lanternReduction;
    }

    private void CheckThresholds()
    {
        float sanityPercent = (currentSanity / maxSanity) * 100f;

        if (sanityPercent <= lowSanityThreshold && sanityPercent > 0f)
        {
            OnLowSanity?.Invoke(sanityPercent);
        }
    }

    private void TriggerInsanity()
    {
        OnInsanityTrigger?.Invoke();
        EventSystem.Publish("InsanityTriggered");

        if (enableDebugLogging)
        {
            Debug.LogWarning("[SanityManager] INSANITY TRIGGERED! Sanity reached 0!");
        }
    }

    /// <summary>
    /// Restore sanity by specified amount (instant)
    /// </summary>
    public void RestoreSanity(float amount)
    {
        float previousSanity = currentSanity;
        currentSanity = Mathf.Clamp(currentSanity + amount, 0f, maxSanity);

        if (enableDebugLogging)
        {
            Debug.Log($"[SanityManager] Restored {amount} sanity. Current: {currentSanity}");
        }

        UpdateGameState();
        OnSanityChanged?.Invoke(currentSanity);
        EventSystem.Publish("SanityChanged", currentSanity);

        if (currentSanity == maxSanity && previousSanity < maxSanity)
        {
            OnSanityRestored?.Invoke();
            EventSystem.Publish("SanityFullyRestored");
        }
    }

    /// <summary>
    /// Fully restore sanity (docking at port)
    /// </summary>
    public void FullRestoreSanity()
    {
        RestoreSanity(maxSanity);
    }

    /// <summary>
    /// Activate talisman protection (temporary halt of drain)
    /// </summary>
    public void ActivateTalisman(float duration = -1f)
    {
        talismanActive = true;
        talismanTimer = duration > 0f ? duration : talismanDuration;

        if (enableDebugLogging)
        {
            Debug.Log($"[SanityManager] Talisman activated for {talismanTimer} seconds");
        }

        EventSystem.Publish("TalismanActivated", talismanTimer);
    }

    /// <summary>
    /// Upgrade lantern level
    /// </summary>
    public void UpgradeLantern()
    {
        lanternLevel++;

        if (enableDebugLogging)
        {
            Debug.Log($"[SanityManager] Lantern upgraded to level {lanternLevel}");
        }

        EventSystem.Publish("LanternUpgraded", lanternLevel);
    }

    /// <summary>
    /// Set lantern level directly
    /// </summary>
    public void SetLanternLevel(int level)
    {
        lanternLevel = Mathf.Max(1, level);
    }

    /// <summary>
    /// Get current sanity value
    /// </summary>
    public float GetCurrentSanity()
    {
        return currentSanity;
    }

    /// <summary>
    /// Get sanity as percentage (0-100)
    /// </summary>
    public float GetSanityPercentage()
    {
        return (currentSanity / maxSanity) * 100f;
    }

    /// <summary>
    /// Check if sanity is in low range
    /// </summary>
    public bool IsLowSanity()
    {
        return GetSanityPercentage() <= lowSanityThreshold;
    }

    /// <summary>
    /// Check if sanity is in critical range
    /// </summary>
    public bool IsCriticalSanity()
    {
        return GetSanityPercentage() <= criticalSanityThreshold;
    }

    /// <summary>
    /// Check if player is insane (0 sanity)
    /// </summary>
    public bool IsInsane()
    {
        return currentSanity <= insaneThreshold;
    }

    /// <summary>
    /// Check if talisman is currently active
    /// </summary>
    public bool IsTalismanActive()
    {
        return talismanActive;
    }

    /// <summary>
    /// Get current drain rate per second
    /// </summary>
    public float GetDrainRate()
    {
        return GetCurrentDrainRate();
    }

    private void OnTimeOfDayChanged(TimeOfDay newTimeOfDay)
    {
        lastTimeOfDay = newTimeOfDay;

        // Enable draining at night/dusk
        isDraining = (newTimeOfDay == TimeOfDay.Night || newTimeOfDay == TimeOfDay.Dusk);

        if (enableDebugLogging)
        {
            Debug.Log($"[SanityManager] Time changed to {newTimeOfDay}. Draining: {isDraining}");
        }
    }

    private void UpdateGameState()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CurrentGameState.sanity = currentSanity;
        }
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<TimeOfDay>("TimeOfDayChanged", OnTimeOfDayChanged);

        if (_instance == this)
        {
            _instance = null;
        }
    }

    // Public methods for debugging/testing
    public void SetSanity(float value)
    {
        currentSanity = Mathf.Clamp(value, 0f, maxSanity);
        UpdateGameState();
        OnSanityChanged?.Invoke(currentSanity);
        EventSystem.Publish("SanityChanged", currentSanity);
    }

    public void ModifySanity(float delta)
    {
        if (delta > 0)
        {
            RestoreSanity(delta);
        }
        else
        {
            currentSanity = Mathf.Clamp(currentSanity + delta, 0f, maxSanity);
            UpdateGameState();
            OnSanityChanged?.Invoke(currentSanity);
            EventSystem.Publish("SanityChanged", currentSanity);
        }
    }
}
