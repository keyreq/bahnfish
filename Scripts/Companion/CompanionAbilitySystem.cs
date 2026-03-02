using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Agent 17: Crew & Companion Specialist - CompanionAbilitySystem.cs
/// Manages pet and crew abilities, cooldowns, and buff application.
/// </summary>
public class CompanionAbilitySystem : MonoBehaviour
{
    private static CompanionAbilitySystem _instance;
    public static CompanionAbilitySystem Instance => _instance;

    [Header("Active Abilities")]
    [SerializeField] private Dictionary<string, AbilityData> activeAbilities = new Dictionary<string, AbilityData>();

    [Header("Applied Buffs")]
    [SerializeField] private List<ActiveBuff> activeBuffs = new List<ActiveBuff>();

    [Header("Settings")]
    [SerializeField] private bool enableDebugLogging = true;

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
        Initialize();
    }

    private void Initialize()
    {
        // Subscribe to events
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);
        EventSystem.Subscribe<string>("PetRegistered", OnPetRegistered);
        EventSystem.Subscribe<string>("CrewRegistered", OnCrewRegistered);

        if (enableDebugLogging)
        {
            Debug.Log("[CompanionAbilitySystem] Initialized");
        }
    }

    private void Update()
    {
        UpdateBuffDurations();
        UpdateAbilityCooldowns();
    }

    /// <summary>
    /// Activates a pet's passive ability
    /// </summary>
    /// <param name="petData">Pet data</param>
    /// <param name="loyalty">Current loyalty</param>
    public void ActivatePetPassiveAbility(PetData petData, float loyalty)
    {
        if (petData == null || petData.passiveAbility == PetPassiveAbility.None)
        {
            return;
        }

        float effectiveValue = petData.GetEffectivePassiveValue(loyalty);
        string buffID = $"pet_passive_{petData.petID}";

        ApplyPassiveBuff(buffID, petData.passiveAbility.ToString(), effectiveValue, petData.passiveAbilityDescription);

        if (enableDebugLogging)
        {
            Debug.Log($"[CompanionAbilitySystem] Activated {petData.petName}'s passive ability: {petData.passiveAbility} (x{effectiveValue:F2})");
        }
    }

    /// <summary>
    /// Activates a pet's active ability
    /// </summary>
    /// <param name="petID">Pet ID</param>
    /// <param name="petData">Pet data</param>
    /// <param name="loyalty">Current loyalty</param>
    /// <returns>True if ability was activated</returns>
    public bool ActivatePetActiveAbility(string petID, PetData petData, float loyalty)
    {
        if (petData == null || petData.activeAbility == PetActiveAbility.None)
        {
            return false;
        }

        string abilityKey = $"pet_active_{petID}";

        // Check cooldown
        if (activeAbilities.ContainsKey(abilityKey))
        {
            float remainingCooldown = activeAbilities[abilityKey].cooldownEndTime - Time.time;
            if (remainingCooldown > 0f)
            {
                EventSystem.Publish("ShowNotification", $"Ability on cooldown: {remainingCooldown:F0}s");
                return false;
            }
        }

        // Execute ability based on type
        bool success = ExecutePetActiveAbility(petData, loyalty);

        if (success)
        {
            // Set cooldown
            float cooldownSeconds = petData.activeAbilityCooldown * 60f;
            activeAbilities[abilityKey] = new AbilityData
            {
                abilityID = abilityKey,
                cooldownEndTime = Time.time + cooldownSeconds,
                lastActivationTime = Time.time
            };

            EventSystem.Publish("PetAbilityActivated", new AbilityActivatedEventData
            {
                companionID = petID,
                abilityName = petData.activeAbility.ToString(),
                cooldownDuration = cooldownSeconds
            });

            if (enableDebugLogging)
            {
                Debug.Log($"[CompanionAbilitySystem] Activated {petData.petName}'s ability: {petData.activeAbility}");
            }
        }

        return success;
    }

    /// <summary>
    /// Executes pet active ability effects
    /// </summary>
    private bool ExecutePetActiveAbility(PetData petData, float loyalty)
    {
        float effectiveness = petData.GetAbilityEffectiveness(loyalty);

        switch (petData.activeAbility)
        {
            case PetActiveAbility.Fetch:
                // Dog: Retrieve dropped items
                EventSystem.Publish("RetrieveDroppedItems", effectiveness);
                return true;

            case PetActiveAbility.Stealth:
                // Cat: Hide from one hazard
                ApplyTemporaryBuff($"stealth_{petData.petID}", "Stealth", 1f, "Immune to next hazard", 60f);
                return true;

            case PetActiveAbility.Scout:
                // Seabird: Reveal nearby fish schools
                float scoutDuration = 480f * effectiveness; // 8 minutes base
                EventSystem.Publish("RevealFishSchools", scoutDuration);
                ApplyTemporaryBuff($"scout_{petData.petID}", "Scout", effectiveness, "Fish schools revealed", scoutDuration);
                return true;

            case PetActiveAbility.TreasureDive:
                // Otter: Find 1 relic
                EventSystem.Publish("GrantRelic", 1);
                EventSystem.Publish("ShowNotification", "Your otter found a relic!");
                return true;

            case PetActiveAbility.ShellShield:
                // Hermit Crab: Protect all fish from next theft
                float shieldDuration = 900f * effectiveness; // 15 minutes base
                ApplyTemporaryBuff($"shellshield_{petData.petID}", "Shell Shield", 1f, "Fish protected from theft", shieldDuration);
                EventSystem.Publish("EnableFishProtection", shieldDuration);
                return true;

            case PetActiveAbility.EtherealPhase:
                // Ghost: Immune to hazards for 30s
                float phaseDuration = 30f * effectiveness;
                ApplyTemporaryBuff($"etherealphase_{petData.petID}", "Ethereal Phase", 1f, "Immune to hazards", phaseDuration);
                EventSystem.Publish("EnableHazardImmunity", phaseDuration);
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Activates a crew member's passive skills
    /// </summary>
    /// <param name="crewData">Crew data</param>
    /// <param name="morale">Current morale</param>
    public void ActivateCrewPassiveSkills(CrewMemberData crewData, float morale)
    {
        if (crewData == null)
        {
            return;
        }

        float effectiveBonus = crewData.GetEffectiveSkillBonus(morale);
        string buffID = $"crew_passive_{crewData.crewID}";

        // Apply skill bonuses based on specialization
        switch (crewData.specialization)
        {
            case CrewSpecialization.Fisherman:
                ApplyCrewSkillBuff(buffID, "Fishing", effectiveBonus / 100f, crewData.primarySkillDescription);
                break;

            case CrewSpecialization.Navigator:
                ApplyCrewSkillBuff(buffID, "Navigation", effectiveBonus / 100f, crewData.primarySkillDescription);
                break;

            case CrewSpecialization.MaintenanceEngineer:
                ApplyCrewSkillBuff(buffID, "Maintenance", effectiveBonus / 100f, crewData.primarySkillDescription);
                break;

            case CrewSpecialization.Cook:
                ApplyCrewSkillBuff(buffID, "Cooking", effectiveBonus / 100f, crewData.primarySkillDescription);
                break;

            case CrewSpecialization.Defender:
                ApplyCrewSkillBuff(buffID, "Defense", effectiveBonus / 100f, crewData.primarySkillDescription);
                break;
        }

        if (enableDebugLogging)
        {
            Debug.Log($"[CompanionAbilitySystem] Activated {crewData.crewName}'s skills: {crewData.specialization} (+{effectiveBonus:F1}%)");
        }
    }

    /// <summary>
    /// Activates a crew member's special ability
    /// </summary>
    /// <param name="crewID">Crew ID</param>
    /// <param name="crewData">Crew data</param>
    /// <returns>True if ability was activated</returns>
    public bool ActivateCrewSpecialAbility(string crewID, CrewMemberData crewData)
    {
        if (crewData == null || !crewData.hasSpecialAbility)
        {
            return false;
        }

        string abilityKey = $"crew_special_{crewID}";

        // Check cooldown
        if (activeAbilities.ContainsKey(abilityKey))
        {
            float remainingCooldown = (activeAbilities[abilityKey].cooldownEndTime - Time.time) / 3600f;
            if (remainingCooldown > 0f)
            {
                EventSystem.Publish("ShowNotification", $"Ability on cooldown: {remainingCooldown:F1}h");
                return false;
            }
        }

        // Execute special ability (implementation would depend on specific abilities)
        EventSystem.Publish("CrewSpecialAbilityActivated", crewID);

        // Set cooldown
        float cooldownSeconds = crewData.specialAbilityCooldown * 3600f;
        activeAbilities[abilityKey] = new AbilityData
        {
            abilityID = abilityKey,
            cooldownEndTime = Time.time + cooldownSeconds,
            lastActivationTime = Time.time
        };

        if (enableDebugLogging)
        {
            Debug.Log($"[CompanionAbilitySystem] Activated {crewData.crewName}'s special ability");
        }

        return true;
    }

    /// <summary>
    /// Applies a passive buff (permanent until removed)
    /// </summary>
    private void ApplyPassiveBuff(string buffID, string buffName, float multiplier, string description)
    {
        // Remove existing buff if present
        activeBuffs.RemoveAll(b => b.buffID == buffID);

        ActiveBuff buff = new ActiveBuff
        {
            buffID = buffID,
            buffName = buffName,
            multiplier = multiplier,
            description = description,
            isPermanent = true,
            startTime = Time.time,
            duration = 0f
        };

        activeBuffs.Add(buff);
        EventSystem.Publish("BuffApplied", buff);
    }

    /// <summary>
    /// Applies a temporary buff with duration
    /// </summary>
    private void ApplyTemporaryBuff(string buffID, string buffName, float multiplier, string description, float duration)
    {
        // Remove existing buff if present
        activeBuffs.RemoveAll(b => b.buffID == buffID);

        ActiveBuff buff = new ActiveBuff
        {
            buffID = buffID,
            buffName = buffName,
            multiplier = multiplier,
            description = description,
            isPermanent = false,
            startTime = Time.time,
            duration = duration
        };

        activeBuffs.Add(buff);
        EventSystem.Publish("BuffApplied", buff);
    }

    /// <summary>
    /// Applies a crew skill buff
    /// </summary>
    private void ApplyCrewSkillBuff(string buffID, string skillType, float bonus, string description)
    {
        // Remove existing buff
        activeBuffs.RemoveAll(b => b.buffID == buffID);

        ActiveBuff buff = new ActiveBuff
        {
            buffID = buffID,
            buffName = $"{skillType} Skill",
            multiplier = 1f + bonus,
            description = description,
            isPermanent = true,
            startTime = Time.time,
            duration = 0f,
            skillType = skillType
        };

        activeBuffs.Add(buff);
        EventSystem.Publish("BuffApplied", buff);
    }

    /// <summary>
    /// Removes a buff
    /// </summary>
    /// <param name="buffID">Buff ID to remove</param>
    public void RemoveBuff(string buffID)
    {
        ActiveBuff removedBuff = activeBuffs.Find(b => b.buffID == buffID);
        if (removedBuff != null)
        {
            activeBuffs.Remove(removedBuff);
            EventSystem.Publish("BuffRemoved", removedBuff);

            if (enableDebugLogging)
            {
                Debug.Log($"[CompanionAbilitySystem] Removed buff: {removedBuff.buffName}");
            }
        }
    }

    /// <summary>
    /// Updates buff durations and removes expired buffs
    /// </summary>
    private void UpdateBuffDurations()
    {
        List<ActiveBuff> expiredBuffs = new List<ActiveBuff>();

        foreach (ActiveBuff buff in activeBuffs)
        {
            if (!buff.isPermanent)
            {
                float elapsed = Time.time - buff.startTime;
                if (elapsed >= buff.duration)
                {
                    expiredBuffs.Add(buff);
                }
            }
        }

        foreach (ActiveBuff buff in expiredBuffs)
        {
            RemoveBuff(buff.buffID);
        }
    }

    /// <summary>
    /// Updates ability cooldowns
    /// </summary>
    private void UpdateAbilityCooldowns()
    {
        List<string> completedCooldowns = new List<string>();

        foreach (var kvp in activeAbilities)
        {
            if (Time.time >= kvp.Value.cooldownEndTime)
            {
                completedCooldowns.Add(kvp.Key);
            }
        }

        foreach (string abilityKey in completedCooldowns)
        {
            activeAbilities.Remove(abilityKey);
            EventSystem.Publish("AbilityCooldownComplete", abilityKey);
        }
    }

    /// <summary>
    /// Gets all active buffs
    /// </summary>
    public List<ActiveBuff> GetActiveBuffs()
    {
        return new List<ActiveBuff>(activeBuffs);
    }

    /// <summary>
    /// Gets total multiplier for a specific skill type
    /// </summary>
    /// <param name="skillType">Skill type (e.g., "Fishing", "Navigation")</param>
    /// <returns>Combined multiplier</returns>
    public float GetSkillMultiplier(string skillType)
    {
        float totalMultiplier = 1f;

        foreach (ActiveBuff buff in activeBuffs)
        {
            if (buff.skillType == skillType)
            {
                totalMultiplier *= buff.multiplier;
            }
        }

        return totalMultiplier;
    }

    /// <summary>
    /// Checks if an ability is on cooldown
    /// </summary>
    public bool IsAbilityOnCooldown(string abilityKey, out float remainingTime)
    {
        if (activeAbilities.ContainsKey(abilityKey))
        {
            remainingTime = activeAbilities[abilityKey].cooldownEndTime - Time.time;
            return remainingTime > 0f;
        }

        remainingTime = 0f;
        return false;
    }

    #region Event Handlers

    private void OnPetRegistered(string petID)
    {
        // Pet registered, abilities will be activated by CompanionManager
    }

    private void OnCrewRegistered(string crewID)
    {
        // Crew registered, skills will be activated by CrewManager
    }

    #endregion

    #region Save/Load Integration

    private void OnGatheringSaveData(SaveData data)
    {
        AbilitySystemSaveData saveData = new AbilitySystemSaveData
        {
            activeAbilitiesList = new List<AbilityData>(activeAbilities.Values),
            activeBuffsList = activeBuffs
        };

        data.abilitySystemData = JsonUtility.ToJson(saveData);
    }

    private void OnApplyingSaveData(SaveData data)
    {
        if (string.IsNullOrEmpty(data.abilitySystemData)) return;

        AbilitySystemSaveData saveData = JsonUtility.FromJson<AbilitySystemSaveData>(data.abilitySystemData);

        // Restore abilities
        activeAbilities.Clear();
        foreach (AbilityData abilityData in saveData.activeAbilitiesList)
        {
            activeAbilities[abilityData.abilityID] = abilityData;
        }

        // Restore buffs
        activeBuffs = saveData.activeBuffsList;

        if (enableDebugLogging)
        {
            Debug.Log($"[CompanionAbilitySystem] Loaded {activeAbilities.Count} abilities and {activeBuffs.Count} buffs");
        }
    }

    #endregion

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);
        EventSystem.Unsubscribe<string>("PetRegistered", OnPetRegistered);
        EventSystem.Unsubscribe<string>("CrewRegistered", OnCrewRegistered);

        if (_instance == this)
        {
            _instance = null;
        }
    }
}

/// <summary>
/// Ability data for cooldown tracking
/// </summary>
[Serializable]
public class AbilityData
{
    public string abilityID;
    public float cooldownEndTime;
    public float lastActivationTime;
}

/// <summary>
/// Active buff data
/// </summary>
[Serializable]
public class ActiveBuff
{
    public string buffID;
    public string buffName;
    public float multiplier;
    public string description;
    public bool isPermanent;
    public float startTime;
    public float duration;
    public string skillType; // For crew skills (Fishing, Navigation, etc.)
}

/// <summary>
/// Ability activation event data
/// </summary>
[Serializable]
public struct AbilityActivatedEventData
{
    public string companionID;
    public string abilityName;
    public float cooldownDuration;
}

/// <summary>
/// Save data for ability system
/// </summary>
[Serializable]
public class AbilitySystemSaveData
{
    public List<AbilityData> activeAbilitiesList;
    public List<ActiveBuff> activeBuffsList;
}
