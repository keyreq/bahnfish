using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages meal buff application, tracking, and expiration.
/// Handles buff stacking rules and stat modifications.
/// Singleton pattern for easy access throughout the game.
/// </summary>
public class MealBuffSystem : MonoBehaviour
{
    public static MealBuffSystem Instance { get; private set; }

    [Header("Active Buffs")]
    [SerializeField] private List<ActiveBuff> activeBuffs = new List<ActiveBuff>();

    [Header("Settings")]
    [Tooltip("Maximum number of simultaneous buffs")]
    [SerializeField] private int maxActiveBuffs = 10;

    [Tooltip("Show debug logs for buff operations")]
    [SerializeField] private bool debugMode = false;

    // Events
    public event System.Action<ActiveBuff> OnBuffApplied;
    public event System.Action<ActiveBuff> OnBuffExpired;
    public event System.Action<ActiveBuff> OnBuffRefreshed;

    // Properties
    public List<ActiveBuff> ActiveBuffs => new List<ActiveBuff>(activeBuffs);
    public int ActiveBuffCount => activeBuffs.Count;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Subscribe to save/load events
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);
    }

    private void Update()
    {
        UpdateBuffTimers();
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);
    }

    /// <summary>
    /// Applies a buff from a consumed meal.
    /// </summary>
    public bool ApplyBuff(MealBuff mealBuff, string sourceName)
    {
        if (mealBuff == null)
        {
            Debug.LogWarning("[MealBuffSystem] Cannot apply null buff");
            return false;
        }

        // Check if we've hit the max buff limit
        if (activeBuffs.Count >= maxActiveBuffs)
        {
            Debug.LogWarning($"[MealBuffSystem] Maximum active buffs reached ({maxActiveBuffs})");
            EventSystem.Publish("BuffLimitReached", this);
            return false;
        }

        // Check for existing buff of same type (no stacking)
        ActiveBuff existingBuff = activeBuffs.FirstOrDefault(b => b.buffType == mealBuff.buffType);

        if (existingBuff != null)
        {
            // Refresh existing buff (reset timer, update strength if higher)
            RefreshBuff(existingBuff, mealBuff);
            return true;
        }

        // Create new active buff
        ActiveBuff newBuff = new ActiveBuff
        {
            buffType = mealBuff.buffType,
            buffStrength = mealBuff.buffStrength,
            duration = mealBuff.duration,
            remainingTime = mealBuff.duration,
            sourceName = sourceName,
            description = mealBuff.description,
            appliedTime = Time.time
        };

        activeBuffs.Add(newBuff);
        ApplyBuffEffects(newBuff);

        // Trigger events
        OnBuffApplied?.Invoke(newBuff);
        EventSystem.Publish("BuffApplied", newBuff);

        if (debugMode)
            Debug.Log($"[MealBuffSystem] Applied {newBuff.buffType} buff (+{newBuff.buffStrength}% for {newBuff.duration}s)");

        return true;
    }

    /// <summary>
    /// Refreshes an existing buff (used when consuming same meal type again).
    /// </summary>
    private void RefreshBuff(ActiveBuff existingBuff, MealBuff newBuff)
    {
        // Reset timer to full duration
        existingBuff.remainingTime = newBuff.duration;
        existingBuff.duration = newBuff.duration;

        // Update strength if new buff is stronger
        if (newBuff.buffStrength > existingBuff.buffStrength)
        {
            // Remove old effect
            RemoveBuffEffects(existingBuff);

            // Update strength
            existingBuff.buffStrength = newBuff.buffStrength;

            // Reapply with new strength
            ApplyBuffEffects(existingBuff);
        }

        // Trigger events
        OnBuffRefreshed?.Invoke(existingBuff);
        EventSystem.Publish("BuffRefreshed", existingBuff);

        if (debugMode)
            Debug.Log($"[MealBuffSystem] Refreshed {existingBuff.buffType} buff");
    }

    /// <summary>
    /// Applies the stat modifications for a buff.
    /// </summary>
    private void ApplyBuffEffects(ActiveBuff buff)
    {
        // Publish specific event for each buff type
        switch (buff.buffType)
        {
            case BuffType.FishingLuck:
                EventSystem.Publish("ModifyFishingLuck", buff.buffStrength);
                break;

            case BuffType.LineStrength:
                EventSystem.Publish("ModifyLineStrength", buff.buffStrength);
                break;

            case BuffType.SpeedBoost:
                EventSystem.Publish("ModifyBoatSpeed", buff.buffStrength);
                break;

            case BuffType.SanityShield:
                EventSystem.Publish("ModifySanityDrain", -buff.buffStrength);
                break;

            case BuffType.NightVision:
                EventSystem.Publish("ModifyNightVision", buff.buffStrength);
                break;

            case BuffType.CoinMultiplier:
                EventSystem.Publish("ModifySellValue", buff.buffStrength);
                break;

            case BuffType.XPBoost:
                EventSystem.Publish("ModifyXPGain", buff.buffStrength);
                break;

            case BuffType.WeatherResistance:
                EventSystem.Publish("ModifyWeatherResistance", buff.buffStrength);
                break;
        }
    }

    /// <summary>
    /// Removes the stat modifications for a buff.
    /// </summary>
    private void RemoveBuffEffects(ActiveBuff buff)
    {
        // Publish reverse events
        switch (buff.buffType)
        {
            case BuffType.FishingLuck:
                EventSystem.Publish("ModifyFishingLuck", -buff.buffStrength);
                break;

            case BuffType.LineStrength:
                EventSystem.Publish("ModifyLineStrength", -buff.buffStrength);
                break;

            case BuffType.SpeedBoost:
                EventSystem.Publish("ModifyBoatSpeed", -buff.buffStrength);
                break;

            case BuffType.SanityShield:
                EventSystem.Publish("ModifySanityDrain", buff.buffStrength);
                break;

            case BuffType.NightVision:
                EventSystem.Publish("ModifyNightVision", -buff.buffStrength);
                break;

            case BuffType.CoinMultiplier:
                EventSystem.Publish("ModifySellValue", -buff.buffStrength);
                break;

            case BuffType.XPBoost:
                EventSystem.Publish("ModifyXPGain", -buff.buffStrength);
                break;

            case BuffType.WeatherResistance:
                EventSystem.Publish("ModifyWeatherResistance", -buff.buffStrength);
                break;
        }
    }

    /// <summary>
    /// Updates all active buff timers and handles expiration.
    /// </summary>
    private void UpdateBuffTimers()
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            ActiveBuff buff = activeBuffs[i];
            buff.remainingTime -= Time.deltaTime;

            // Check if buff expired
            if (buff.remainingTime <= 0)
            {
                ExpireBuff(buff);
            }
        }
    }

    /// <summary>
    /// Expires a buff and removes its effects.
    /// </summary>
    private void ExpireBuff(ActiveBuff buff)
    {
        activeBuffs.Remove(buff);
        RemoveBuffEffects(buff);

        // Trigger events
        OnBuffExpired?.Invoke(buff);
        EventSystem.Publish("BuffExpired", buff);

        if (debugMode)
            Debug.Log($"[MealBuffSystem] {buff.buffType} buff expired");
    }

    /// <summary>
    /// Manually removes a specific buff.
    /// </summary>
    public bool RemoveBuff(BuffType buffType)
    {
        ActiveBuff buff = activeBuffs.FirstOrDefault(b => b.buffType == buffType);

        if (buff != null)
        {
            ExpireBuff(buff);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes all active buffs.
    /// </summary>
    public void ClearAllBuffs()
    {
        while (activeBuffs.Count > 0)
        {
            ExpireBuff(activeBuffs[0]);
        }

        if (debugMode)
            Debug.Log("[MealBuffSystem] Cleared all buffs");
    }

    /// <summary>
    /// Checks if a specific buff type is active.
    /// </summary>
    public bool HasBuff(BuffType buffType)
    {
        return activeBuffs.Any(b => b.buffType == buffType);
    }

    /// <summary>
    /// Gets the active buff of a specific type.
    /// </summary>
    public ActiveBuff GetBuff(BuffType buffType)
    {
        return activeBuffs.FirstOrDefault(b => b.buffType == buffType);
    }

    /// <summary>
    /// Gets the total buff strength for a specific type.
    /// </summary>
    public float GetBuffStrength(BuffType buffType)
    {
        ActiveBuff buff = GetBuff(buffType);
        return buff != null ? buff.buffStrength : 0f;
    }

    /// <summary>
    /// Gets the combined strength of all active buffs (for UI display).
    /// </summary>
    public Dictionary<BuffType, float> GetAllBuffStrengths()
    {
        Dictionary<BuffType, float> strengths = new Dictionary<BuffType, float>();

        foreach (var buff in activeBuffs)
        {
            strengths[buff.buffType] = buff.buffStrength;
        }

        return strengths;
    }

    /// <summary>
    /// Gets remaining time for a specific buff type.
    /// </summary>
    public float GetBuffRemainingTime(BuffType buffType)
    {
        ActiveBuff buff = GetBuff(buffType);
        return buff != null ? buff.remainingTime : 0f;
    }

    /// <summary>
    /// Extends the duration of an active buff.
    /// </summary>
    public bool ExtendBuff(BuffType buffType, float additionalTime)
    {
        ActiveBuff buff = GetBuff(buffType);

        if (buff != null)
        {
            buff.remainingTime += additionalTime;
            buff.duration += additionalTime;

            if (debugMode)
                Debug.Log($"[MealBuffSystem] Extended {buffType} by {additionalTime}s");

            return true;
        }

        return false;
    }

    // ===== Save/Load Integration =====

    private void OnGatheringSaveData(SaveData saveData)
    {
        // Save active buffs
        List<SerializedBuff> serializedBuffs = new List<SerializedBuff>();

        foreach (var buff in activeBuffs)
        {
            serializedBuffs.Add(new SerializedBuff
            {
                buffType = buff.buffType,
                buffStrength = buff.buffStrength,
                remainingTime = buff.remainingTime,
                duration = buff.duration,
                sourceName = buff.sourceName,
                description = buff.description
            });
        }

        saveData.activeBuffs = serializedBuffs;

        if (debugMode)
            Debug.Log($"[MealBuffSystem] Saved {serializedBuffs.Count} active buffs");
    }

    private void OnApplyingSaveData(SaveData saveData)
    {
        // Clear existing buffs
        ClearAllBuffs();

        // Load saved buffs
        if (saveData.activeBuffs != null)
        {
            foreach (var serialized in saveData.activeBuffs)
            {
                ActiveBuff buff = new ActiveBuff
                {
                    buffType = serialized.buffType,
                    buffStrength = serialized.buffStrength,
                    remainingTime = serialized.remainingTime,
                    duration = serialized.duration,
                    sourceName = serialized.sourceName,
                    description = serialized.description,
                    appliedTime = Time.time
                };

                activeBuffs.Add(buff);
                ApplyBuffEffects(buff);
            }

            if (debugMode)
                Debug.Log($"[MealBuffSystem] Loaded {saveData.activeBuffs.Count} active buffs");
        }
    }

    // ===== Debug Methods =====

    /// <summary>
    /// Logs all active buffs for debugging.
    /// </summary>
    [ContextMenu("Debug: Print Active Buffs")]
    public void DebugPrintActiveBuffs()
    {
        Debug.Log("=== ACTIVE BUFFS ===");
        Debug.Log($"Total: {activeBuffs.Count}/{maxActiveBuffs}");

        foreach (var buff in activeBuffs)
        {
            int minutesLeft = Mathf.FloorToInt(buff.remainingTime / 60f);
            int secondsLeft = Mathf.FloorToInt(buff.remainingTime % 60f);
            Debug.Log($"{buff.buffType}: +{buff.buffStrength}% ({minutesLeft}m {secondsLeft}s remaining)");
        }
    }
}

/// <summary>
/// Represents an active buff currently affecting the player.
/// </summary>
[System.Serializable]
public class ActiveBuff
{
    public BuffType buffType;
    public float buffStrength;
    public float duration;
    public float remainingTime;
    public string sourceName;
    public string description;
    public float appliedTime;

    /// <summary>
    /// Gets the percentage of time remaining (0-1).
    /// </summary>
    public float GetTimePercentage()
    {
        return duration > 0 ? remainingTime / duration : 0f;
    }

    /// <summary>
    /// Gets a formatted string of remaining time.
    /// </summary>
    public string GetFormattedTimeRemaining()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    /// <summary>
    /// Gets a description string for UI display.
    /// </summary>
    public string GetDisplayText()
    {
        return $"{buffType}: +{buffStrength:F0}% ({GetFormattedTimeRemaining()})";
    }
}

/// <summary>
/// Serialized version of ActiveBuff for save/load.
/// </summary>
[System.Serializable]
public class SerializedBuff
{
    public BuffType buffType;
    public float buffStrength;
    public float remainingTime;
    public float duration;
    public string sourceName;
    public string description;
}
