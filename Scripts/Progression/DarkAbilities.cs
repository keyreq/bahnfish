using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Agent 9: Progression & Economy Agent - DarkAbilities.cs
/// Manages supernatural dark abilities unlocked with relics.
/// 6 powerful abilities inspired by Dredge's supernatural mechanics.
/// </summary>
public class DarkAbilities : MonoBehaviour
{
    public static DarkAbilities Instance { get; private set; }

    [Header("Ability Data")]
    [SerializeField] private List<DarkAbilityData> _allAbilities = new List<DarkAbilityData>();
    private Dictionary<string, DarkAbilityData> _abilitiesByID = new Dictionary<string, DarkAbilityData>();
    private HashSet<string> _unlockedAbilities = new HashSet<string>();
    private Dictionary<string, float> _abilityCooldowns = new Dictionary<string, float>();

    [Header("Debug")]
    [SerializeField] private bool _enableDebugLogs = true;

    // Events
    public event System.Action<DarkAbilityData> OnAbilityUnlocked;
    public event System.Action<DarkAbilityData> OnAbilityActivated;
    public event System.Action<DarkAbilityData> OnAbilityDeactivated;
    public event System.Action<DarkAbilityData, float> OnAbilityCooldownUpdated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeAbilities();
    }

    private void Start()
    {
        // Subscribe to save/load events
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);

        if (_enableDebugLogs)
        {
            Debug.Log($"[DarkAbilities] Initialized with {_allAbilities.Count} dark abilities");
        }
    }

    private void Update()
    {
        // Update cooldowns
        UpdateCooldowns();
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);
    }

    #region Initialization

    /// <summary>
    /// Initializes all 6 dark abilities from design document.
    /// </summary>
    private void InitializeAbilities()
    {
        if (_allAbilities.Count == 0)
        {
            CreateDefaultAbilities();
        }

        // Build lookup dictionary
        _abilitiesByID.Clear();
        foreach (var ability in _allAbilities)
        {
            _abilitiesByID[ability.abilityID] = ability;
            _abilityCooldowns[ability.abilityID] = 0f;
        }
    }

    /// <summary>
    /// Creates all 6 dark abilities from design document.
    /// </summary>
    private void CreateDefaultAbilities()
    {
        // 1. Abyssal Sprint - Speed boost
        _allAbilities.Add(new DarkAbilityData
        {
            abilityID = "abyssal_sprint",
            abilityName = "Abyssal Sprint",
            description = "Channel dark energy for a temporary massive speed boost. Perfect for escaping danger.",
            relicCost = 5,
            cooldownSeconds = 60f,
            durationSeconds = 10f,
            abilityType = AbilityType.Buff,
            effect = new AbilityEffect
            {
                effectType = AbilityEffectType.SpeedBoost,
                magnitude = 2.0f // 2x speed
            }
        });

        // 2. Tidal Gate - Teleportation
        _allAbilities.Add(new DarkAbilityData
        {
            abilityID = "tidal_gate",
            abilityName = "Tidal Gate",
            description = "Teleport between discovered ancient altars. Instant travel across the map.",
            relicCost = 10,
            cooldownSeconds = 120f,
            durationSeconds = 0f, // Instant effect
            abilityType = AbilityType.Utility,
            effect = new AbilityEffect
            {
                effectType = AbilityEffectType.Teleport,
                magnitude = 0f
            }
        });

        // 3. Siren's Call - Attract rare fish
        _allAbilities.Add(new DarkAbilityData
        {
            abilityID = "sirens_call",
            abilityName = "Siren's Call",
            description = "Emit an otherworldly song that attracts rare fish to your location.",
            relicCost = 8,
            cooldownSeconds = 90f,
            durationSeconds = 30f,
            abilityType = AbilityType.Buff,
            effect = new AbilityEffect
            {
                effectType = AbilityEffectType.RareFishAttraction,
                magnitude = 3.0f // 3x rare spawn rate
            }
        });

        // 4. Temporal Anchor - Slow time in mini-games
        _allAbilities.Add(new DarkAbilityData
        {
            abilityID = "temporal_anchor",
            abilityName = "Temporal Anchor",
            description = "Slow down time during fishing mini-games, making difficult catches easier.",
            relicCost = 12,
            cooldownSeconds = 45f,
            durationSeconds = 15f,
            abilityType = AbilityType.Buff,
            effect = new AbilityEffect
            {
                effectType = AbilityEffectType.TimeSlowdown,
                magnitude = 0.5f // 50% normal speed
            }
        });

        // 5. Eldritch Eye - See hidden spots
        _allAbilities.Add(new DarkAbilityData
        {
            abilityID = "eldritch_eye",
            abilityName = "Eldritch Eye",
            description = "Open your third eye to reveal hidden fishing spots and secrets.",
            relicCost = 15,
            cooldownSeconds = 180f,
            durationSeconds = 60f,
            abilityType = AbilityType.Buff,
            effect = new AbilityEffect
            {
                effectType = AbilityEffectType.RevealSecrets,
                magnitude = 1.0f
            }
        });

        // 6. Void Storage - Temporary extra inventory (risky)
        _allAbilities.Add(new DarkAbilityData
        {
            abilityID = "void_storage",
            abilityName = "Void Storage",
            description = "Access the void for +20 temporary inventory slots. WARNING: Items may vanish!",
            relicCost = 20,
            cooldownSeconds = 300f,
            durationSeconds = 120f,
            abilityType = AbilityType.RiskyBuff,
            effect = new AbilityEffect
            {
                effectType = AbilityEffectType.TemporaryStorage,
                magnitude = 20f, // +20 slots
                risk = 0.1f // 10% chance per item to vanish
            }
        });
    }

    #endregion

    #region Unlock Abilities

    /// <summary>
    /// Attempts to unlock a dark ability with relics.
    /// </summary>
    public bool UnlockAbility(string abilityID)
    {
        if (!_abilitiesByID.TryGetValue(abilityID, out DarkAbilityData ability))
        {
            Debug.LogWarning($"[DarkAbilities] Unknown ability ID: {abilityID}");
            return false;
        }

        // Check if already unlocked
        if (IsAbilityUnlocked(abilityID))
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[DarkAbilities] {ability.abilityName} already unlocked");
            }
            return false;
        }

        // Try to spend relics
        if (EconomySystem.Instance.SpendRelics(ability.relicCost, $"Unlock {ability.abilityName}"))
        {
            _unlockedAbilities.Add(abilityID);

            // Fire events
            OnAbilityUnlocked?.Invoke(ability);
            EventSystem.Publish("DarkAbilityUnlocked", ability);

            if (_enableDebugLogs)
            {
                Debug.Log($"[DarkAbilities] Unlocked {ability.abilityName} for {ability.relicCost} relics");
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if an ability is unlocked.
    /// </summary>
    public bool IsAbilityUnlocked(string abilityID)
    {
        return _unlockedAbilities.Contains(abilityID);
    }

    /// <summary>
    /// Checks if an ability can be unlocked right now.
    /// </summary>
    public bool CanUnlockAbility(string abilityID)
    {
        if (!_abilitiesByID.TryGetValue(abilityID, out DarkAbilityData ability))
        {
            return false;
        }

        // Already unlocked?
        if (IsAbilityUnlocked(abilityID)) return false;

        // Can afford?
        return EconomySystem.Instance.CanAffordRelics(ability.relicCost);
    }

    #endregion

    #region Activate Abilities

    /// <summary>
    /// Activates a dark ability.
    /// </summary>
    public bool ActivateAbility(string abilityID)
    {
        if (!_abilitiesByID.TryGetValue(abilityID, out DarkAbilityData ability))
        {
            Debug.LogWarning($"[DarkAbilities] Unknown ability ID: {abilityID}");
            return false;
        }

        // Check if unlocked
        if (!IsAbilityUnlocked(abilityID))
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[DarkAbilities] {ability.abilityName} not unlocked");
            }
            return false;
        }

        // Check cooldown
        if (!IsAbilityReady(abilityID))
        {
            if (_enableDebugLogs)
            {
                float remaining = GetRemainingCooldown(abilityID);
                Debug.Log($"[DarkAbilities] {ability.abilityName} on cooldown ({remaining:F1}s remaining)");
            }
            return false;
        }

        // Activate ability
        ApplyAbilityEffect(ability);

        // Start cooldown
        _abilityCooldowns[abilityID] = ability.cooldownSeconds;

        // Fire events
        OnAbilityActivated?.Invoke(ability);
        EventSystem.Publish("DarkAbilityActivated", new AbilityActivatedData(ability, ability.durationSeconds));

        if (_enableDebugLogs)
        {
            Debug.Log($"[DarkAbilities] Activated {ability.abilityName}");
        }

        // Schedule deactivation if it has a duration
        if (ability.durationSeconds > 0)
        {
            Invoke(nameof(DeactivateAbility), ability.durationSeconds);
        }

        return true;
    }

    /// <summary>
    /// Applies the effect of an ability.
    /// </summary>
    private void ApplyAbilityEffect(DarkAbilityData ability)
    {
        switch (ability.effect.effectType)
        {
            case AbilityEffectType.SpeedBoost:
                EventSystem.Publish("SpeedBoostActivated", ability.effect.magnitude);
                break;

            case AbilityEffectType.Teleport:
                EventSystem.Publish("TeleportRequested", ability);
                break;

            case AbilityEffectType.RareFishAttraction:
                EventSystem.Publish("RareFishAttractionActivated", ability.effect.magnitude);
                break;

            case AbilityEffectType.TimeSlowdown:
                EventSystem.Publish("TimeSlowdownActivated", ability.effect.magnitude);
                break;

            case AbilityEffectType.RevealSecrets:
                EventSystem.Publish("SecretsRevealed", ability.effect.magnitude);
                break;

            case AbilityEffectType.TemporaryStorage:
                EventSystem.Publish("VoidStorageActivated", new VoidStorageData(
                    (int)ability.effect.magnitude,
                    ability.effect.risk,
                    ability.durationSeconds
                ));
                break;
        }
    }

    /// <summary>
    /// Deactivates ability effects.
    /// </summary>
    private void DeactivateAbility()
    {
        // Find which ability just finished
        // In a real implementation, you'd track active abilities
        EventSystem.Publish("DarkAbilityDeactivated", Time.time);

        if (_enableDebugLogs)
        {
            Debug.Log("[DarkAbilities] Ability effect ended");
        }
    }

    /// <summary>
    /// Checks if an ability is ready to use.
    /// </summary>
    public bool IsAbilityReady(string abilityID)
    {
        if (!_abilityCooldowns.TryGetValue(abilityID, out float cooldown))
        {
            return true;
        }
        return cooldown <= 0f;
    }

    /// <summary>
    /// Gets remaining cooldown time for an ability.
    /// </summary>
    public float GetRemainingCooldown(string abilityID)
    {
        if (_abilityCooldowns.TryGetValue(abilityID, out float cooldown))
        {
            return Mathf.Max(0f, cooldown);
        }
        return 0f;
    }

    /// <summary>
    /// Updates all ability cooldowns.
    /// </summary>
    private void UpdateCooldowns()
    {
        List<string> keys = new List<string>(_abilityCooldowns.Keys);
        foreach (var abilityID in keys)
        {
            if (_abilityCooldowns[abilityID] > 0)
            {
                _abilityCooldowns[abilityID] -= Time.deltaTime;

                // Fire cooldown update event
                if (_abilitiesByID.TryGetValue(abilityID, out DarkAbilityData ability))
                {
                    OnAbilityCooldownUpdated?.Invoke(ability, _abilityCooldowns[abilityID]);
                }

                // Clamp to 0
                if (_abilityCooldowns[abilityID] < 0)
                {
                    _abilityCooldowns[abilityID] = 0f;

                    if (_enableDebugLogs && _abilitiesByID.TryGetValue(abilityID, out DarkAbilityData readyAbility))
                    {
                        Debug.Log($"[DarkAbilities] {readyAbility.abilityName} is now ready!");
                        EventSystem.Publish("AbilityReady", readyAbility);
                    }
                }
            }
        }
    }

    #endregion

    #region Query Methods

    /// <summary>
    /// Gets an ability by ID.
    /// </summary>
    public DarkAbilityData GetAbility(string abilityID)
    {
        return _abilitiesByID.TryGetValue(abilityID, out DarkAbilityData ability) ? ability : null;
    }

    /// <summary>
    /// Gets all unlocked abilities.
    /// </summary>
    public List<DarkAbilityData> GetUnlockedAbilities()
    {
        return _allAbilities.Where(a => IsAbilityUnlocked(a.abilityID)).ToList();
    }

    /// <summary>
    /// Gets all abilities available for unlock.
    /// </summary>
    public List<DarkAbilityData> GetAvailableAbilities()
    {
        return _allAbilities.Where(a => CanUnlockAbility(a.abilityID)).ToList();
    }

    /// <summary>
    /// Gets all ready-to-use abilities.
    /// </summary>
    public List<DarkAbilityData> GetReadyAbilities()
    {
        return _allAbilities.Where(a => IsAbilityUnlocked(a.abilityID) && IsAbilityReady(a.abilityID)).ToList();
    }

    #endregion

    #region Save/Load

    private void OnGatheringSaveData(SaveData data)
    {
        data.unlockedAbilities.Clear();
        data.unlockedAbilities.AddRange(_unlockedAbilities);

        if (_enableDebugLogs)
        {
            Debug.Log($"[DarkAbilities] Saved {_unlockedAbilities.Count} unlocked abilities");
        }
    }

    private void OnApplyingSaveData(SaveData data)
    {
        _unlockedAbilities.Clear();
        foreach (var abilityID in data.unlockedAbilities)
        {
            _unlockedAbilities.Add(abilityID);
        }

        if (_enableDebugLogs)
        {
            Debug.Log($"[DarkAbilities] Loaded {_unlockedAbilities.Count} unlocked abilities");
        }
    }

    #endregion

    #region Debug Methods

    [ContextMenu("Print All Abilities")]
    public void PrintAllAbilities()
    {
        Debug.Log($"=== Dark Abilities ({_allAbilities.Count}) ===");
        foreach (var ability in _allAbilities.OrderBy(a => a.relicCost))
        {
            string status = IsAbilityUnlocked(ability.abilityID) ? "UNLOCKED" : "LOCKED";
            string ready = IsAbilityReady(ability.abilityID) ? "READY" : $"COOLDOWN: {GetRemainingCooldown(ability.abilityID):F1}s";
            Debug.Log($"[{status}] {ability.abilityName} ({ability.relicCost} relics) - {ready}");
        }
    }

    [ContextMenu("Unlock All Abilities (Debug)")]
    private void DebugUnlockAll()
    {
        foreach (var ability in _allAbilities)
        {
            _unlockedAbilities.Add(ability.abilityID);
        }
        Debug.Log("[DarkAbilities] All abilities unlocked (DEBUG)");
    }

    [ContextMenu("Activate Abyssal Sprint")]
    private void DebugActivateSprint()
    {
        ActivateAbility("abyssal_sprint");
    }

    #endregion
}

#region Data Structures

/// <summary>
/// Dark ability data.
/// </summary>
[System.Serializable]
public class DarkAbilityData
{
    public string abilityID;
    public string abilityName;
    public string description;
    public int relicCost;
    public float cooldownSeconds;
    public float durationSeconds; // 0 for instant effects
    public AbilityType abilityType;
    public AbilityEffect effect;
}

/// <summary>
/// Ability effect definition.
/// </summary>
[System.Serializable]
public class AbilityEffect
{
    public AbilityEffectType effectType;
    public float magnitude;
    public float risk = 0f; // For risky abilities (0.0 to 1.0)
}

/// <summary>
/// Ability types.
/// </summary>
public enum AbilityType
{
    Buff,
    Utility,
    RiskyBuff
}

/// <summary>
/// Ability effect types.
/// </summary>
public enum AbilityEffectType
{
    SpeedBoost,
    Teleport,
    RareFishAttraction,
    TimeSlowdown,
    RevealSecrets,
    TemporaryStorage
}

/// <summary>
/// Ability activated event data.
/// </summary>
[System.Serializable]
public struct AbilityActivatedData
{
    public DarkAbilityData ability;
    public float duration;

    public AbilityActivatedData(DarkAbilityData ability, float duration)
    {
        this.ability = ability;
        this.duration = duration;
    }
}

/// <summary>
/// Void storage activation data.
/// </summary>
[System.Serializable]
public struct VoidStorageData
{
    public int bonusSlots;
    public float riskPerItem;
    public float duration;

    public VoidStorageData(int bonusSlots, float riskPerItem, float duration)
    {
        this.bonusSlots = bonusSlots;
        this.riskPerItem = riskPerItem;
        this.duration = duration;
    }
}

#endregion
