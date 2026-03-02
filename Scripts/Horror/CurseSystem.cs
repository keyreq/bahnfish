using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages persistent curses from 0 sanity events.
/// Curses must be cleansed by visiting mystic NPC.
/// Agent 7: Sanity & Horror System
/// </summary>
public class CurseSystem : MonoBehaviour
{
    private static CurseSystem _instance;
    public static CurseSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CurseSystem>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("CurseSystem");
                    _instance = go.AddComponent<CurseSystem>();
                }
            }
            return _instance;
        }
    }

    [Header("Available Curses")]
    [SerializeField] private List<CurseType> availableCurses = new List<CurseType>
    {
        CurseType.RottingCatch,
        CurseType.BrokenCompass,
        CurseType.HauntedHull,
        CurseType.LeakingBoat,
        CurseType.TangledNets
    };

    [Header("Active Curses")]
    [SerializeField] private List<ActiveCurse> activeCurses = new List<ActiveCurse>();

    [Header("Curse Configuration")]
    [SerializeField] private int maxSimultaneousCurses = 3;
    [SerializeField] private float curseChanceOnInsanity = 0.3f; // 30% chance per insanity event

    [Header("Curse Effects")]
    [SerializeField] private float rottingMultiplier = 2f; // Fish spoil 2x faster
    [SerializeField] private float compassDistortionAngle = 45f;
    [SerializeField] private float leakDamagePerSecond = 1f;
    [SerializeField] private float tangledNetPenalty = 0.5f; // 50% fishing speed

    [Header("Cleansing")]
    [SerializeField] private float cleanseCostPerCurse = 100f;

    [Header("Audio")]
    [SerializeField] private AudioClip curseAppliedSound;
    [SerializeField] private AudioClip curseRemovedSound;
    private AudioSource audioSource;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;

    private float leakTimer = 0f;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        // Subscribe to insanity events
        EventSystem.Subscribe("InsanityTriggered", OnInsanityTriggered);

        if (enableDebugLogging)
        {
            Debug.Log("[CurseSystem] Initialized");
        }
    }

    private void Update()
    {
        // Apply active curse effects
        foreach (ActiveCurse curse in activeCurses)
        {
            ApplyCurseEffects(curse);
        }
    }

    private void OnInsanityTriggered()
    {
        if (Random.value < curseChanceOnInsanity)
        {
            ApplyRandomCurse();
        }
    }

    private void ApplyRandomCurse()
    {
        if (activeCurses.Count >= maxSimultaneousCurses)
        {
            if (enableDebugLogging)
            {
                Debug.Log("[CurseSystem] Max curses reached, cannot apply more");
            }
            return;
        }

        // Get curses not currently active
        List<CurseType> availableToApply = new List<CurseType>();
        foreach (CurseType curseType in availableCurses)
        {
            if (!HasCurse(curseType))
            {
                availableToApply.Add(curseType);
            }
        }

        if (availableToApply.Count == 0)
        {
            if (enableDebugLogging)
            {
                Debug.Log("[CurseSystem] No available curses to apply");
            }
            return;
        }

        // Apply random curse
        CurseType selectedCurse = availableToApply[Random.Range(0, availableToApply.Count)];
        ApplyCurse(selectedCurse);
    }

    /// <summary>
    /// Apply a specific curse to the player
    /// </summary>
    public void ApplyCurse(CurseType curseType)
    {
        if (HasCurse(curseType))
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[CurseSystem] Already cursed with {curseType}");
            }
            return;
        }

        ActiveCurse newCurse = new ActiveCurse
        {
            curseType = curseType,
            appliedTime = Time.time,
            isActive = true
        };

        activeCurses.Add(newCurse);

        // Play curse sound
        if (curseAppliedSound != null)
        {
            audioSource.PlayOneShot(curseAppliedSound);
        }

        // Publish event
        EventSystem.Publish("CurseApplied", curseType);

        if (enableDebugLogging)
        {
            Debug.LogWarning($"[CurseSystem] CURSE APPLIED: {curseType}!");
        }
    }

    /// <summary>
    /// Remove a specific curse
    /// </summary>
    public bool RemoveCurse(CurseType curseType)
    {
        ActiveCurse curse = activeCurses.Find(c => c.curseType == curseType);

        if (curse != null)
        {
            activeCurses.Remove(curse);

            // Play remove sound
            if (curseRemovedSound != null)
            {
                audioSource.PlayOneShot(curseRemovedSound);
            }

            // Publish event
            EventSystem.Publish("CurseRemoved", curseType);

            if (enableDebugLogging)
            {
                Debug.Log($"[CurseSystem] Curse removed: {curseType}");
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Cleanse all curses (costs money)
    /// </summary>
    public bool CleanseAllCurses()
    {
        if (activeCurses.Count == 0) return false;

        float totalCost = activeCurses.Count * cleanseCostPerCurse;

        // Check if player has enough money
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.CurrentGameState.money < totalCost)
            {
                if (enableDebugLogging)
                {
                    Debug.Log($"[CurseSystem] Cannot afford cleansing. Cost: {totalCost}");
                }
                return false;
            }

            // Deduct cost
            GameManager.Instance.CurrentGameState.money -= totalCost;
        }

        // Remove all curses
        int count = activeCurses.Count;
        activeCurses.Clear();

        // Play sound
        if (curseRemovedSound != null)
        {
            audioSource.PlayOneShot(curseRemovedSound);
        }

        // Publish event
        EventSystem.Publish("AllCursesCleansed", count);

        if (enableDebugLogging)
        {
            Debug.Log($"[CurseSystem] All {count} curses cleansed for {totalCost} money");
        }

        return true;
    }

    /// <summary>
    /// Check if player has a specific curse
    /// </summary>
    public bool HasCurse(CurseType curseType)
    {
        return activeCurses.Exists(c => c.curseType == curseType);
    }

    /// <summary>
    /// Get all active curses
    /// </summary>
    public List<CurseType> GetActiveCurses()
    {
        List<CurseType> curses = new List<CurseType>();
        foreach (ActiveCurse curse in activeCurses)
        {
            curses.Add(curse.curseType);
        }
        return curses;
    }

    /// <summary>
    /// Get curse count
    /// </summary>
    public int GetCurseCount()
    {
        return activeCurses.Count;
    }

    /// <summary>
    /// Get total cleansing cost
    /// </summary>
    public float GetCleansingCost()
    {
        return activeCurses.Count * cleanseCostPerCurse;
    }

    private void ApplyCurseEffects(ActiveCurse curse)
    {
        switch (curse.curseType)
        {
            case CurseType.RottingCatch:
                // Fish spoil faster - would integrate with inventory system
                EventSystem.Publish("FishSpoilRateModifier", rottingMultiplier);
                break;

            case CurseType.BrokenCompass:
                // Distort compass reading
                EventSystem.Publish("CompassDistorted", compassDistortionAngle);
                break;

            case CurseType.HauntedHull:
                // Random visual/audio glitches
                if (Random.value < 0.01f) // 1% per frame
                {
                    EventSystem.Publish("HauntedGlitch");
                }
                break;

            case CurseType.LeakingBoat:
                // Constant slow damage
                leakTimer += Time.deltaTime;
                if (leakTimer >= 1f)
                {
                    leakTimer = 0f;
                    EventSystem.Publish("BoatDamaged", leakDamagePerSecond);
                }
                break;

            case CurseType.TangledNets:
                // Fishing speed penalty
                EventSystem.Publish("FishingSpeedModifier", tangledNetPenalty);
                break;
        }
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe("InsanityTriggered", OnInsanityTriggered);

        if (_instance == this)
        {
            _instance = null;
        }
    }
}

/// <summary>
/// Types of curses that can afflict the player
/// </summary>
[System.Serializable]
public enum CurseType
{
    RottingCatch,   // Fish spoil faster
    BrokenCompass,  // Navigation harder
    HauntedHull,    // Visual/audio glitches
    LeakingBoat,    // Constant slow damage
    TangledNets     // Fishing speed reduced
}

/// <summary>
/// Represents an active curse on the player
/// </summary>
[System.Serializable]
public class ActiveCurse
{
    public CurseType curseType;
    public float appliedTime;
    public bool isActive;
}
