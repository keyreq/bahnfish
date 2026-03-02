using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Agent 17: Crew & Companion Specialist - MoraleSystem.cs
/// Manages crew member morale, payment tracking, and performance modifiers.
/// </summary>
public class MoraleSystem : MonoBehaviour
{
    private static MoraleSystem _instance;
    public static MoraleSystem Instance => _instance;

    [Header("Morale Tracking")]
    [SerializeField] private Dictionary<string, CrewMoraleData> crewMoraleData = new Dictionary<string, CrewMoraleData>();

    [Header("Settings")]
    [SerializeField] private float dailyMoraleDecay = 2f;
    [SerializeField] private float dailyDecayCheckInterval = 3600f; // 1 hour
    [SerializeField] private float synergyMoraleBonus = 5f;
    [SerializeField] private float conflictMoralePenalty = 10f;
    [SerializeField] private bool enableDebugLogging = true;

    [Header("Status")]
    [SerializeField] private float lastDecayCheckTime = 0f;
    [SerializeField] private float lastPaymentCheckTime = 0f;
    [SerializeField] private int currentDay = 0;

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
        lastDecayCheckTime = Time.time;
        lastPaymentCheckTime = Time.time;

        // Subscribe to events
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);
        EventSystem.Subscribe("DayCompleted", OnDayCompleted);
        EventSystem.Subscribe<WeatherType>("WeatherChanged", OnWeatherChanged);

        if (enableDebugLogging)
        {
            Debug.Log("[MoraleSystem] Initialized");
        }
    }

    private void Update()
    {
        // Check for morale decay periodically
        if (Time.time >= lastDecayCheckTime + dailyDecayCheckInterval)
        {
            ProcessMoraleDecay();
            lastDecayCheckTime = Time.time;
        }

        // Check for salary payments (daily)
        if (Time.time >= lastPaymentCheckTime + 86400f) // 24 hours
        {
            ProcessDailySalaries();
            lastPaymentCheckTime = Time.time;
        }
    }

    /// <summary>
    /// Registers a new crew member in the morale system
    /// </summary>
    /// <param name="crewData">Crew member data</param>
    public void RegisterCrewMember(CrewMemberData crewData)
    {
        if (crewData == null || crewMoraleData.ContainsKey(crewData.crewID))
        {
            return;
        }

        CrewMoraleData moraleData = new CrewMoraleData
        {
            crewID = crewData.crewID,
            currentMorale = crewData.startingMorale,
            dailySalary = crewData.dailySalary,
            lastPaymentTime = Time.time,
            hireDate = Time.time,
            daysEmployed = 0,
            totalBonusReceived = 0f,
            consecutiveLatePayments = 0
        };

        crewMoraleData[crewData.crewID] = moraleData;

        // Check for synergies/conflicts with existing crew
        UpdateCrewRelationships();

        if (enableDebugLogging)
        {
            Debug.Log($"[MoraleSystem] Registered crew: {crewData.crewName} (ID: {crewData.crewID})");
        }

        EventSystem.Publish("CrewRegistered", crewData.crewID);
    }

    /// <summary>
    /// Removes a crew member from the system
    /// </summary>
    /// <param name="crewID">Crew ID to remove</param>
    public void RemoveCrewMember(string crewID)
    {
        if (crewMoraleData.ContainsKey(crewID))
        {
            crewMoraleData.Remove(crewID);
            UpdateCrewRelationships();

            if (enableDebugLogging)
            {
                Debug.Log($"[MoraleSystem] Removed crew: {crewID}");
            }

            EventSystem.Publish("CrewRemoved", crewID);
        }
    }

    /// <summary>
    /// Pays salary to a crew member
    /// </summary>
    /// <param name="crewID">Crew ID</param>
    /// <param name="crewData">Crew data reference</param>
    /// <returns>True if payment was successful</returns>
    public bool PaySalary(string crewID, CrewMemberData crewData)
    {
        if (!crewMoraleData.ContainsKey(crewID) || crewData == null)
        {
            return false;
        }

        CrewMoraleData moraleData = crewMoraleData[crewID];

        // Check if player has enough money
        GameState state = GameManager.Instance.CurrentGameState;
        if (state.money < moraleData.dailySalary)
        {
            // Cannot afford payment
            moraleData.consecutiveLatePayments++;
            float moraleLoss = crewData.latePaymentMoralePenalty * moraleData.consecutiveLatePayments;
            ModifyMorale(crewID, moraleLoss, crewData);

            EventSystem.Publish("SalaryPaymentFailed", crewID);
            EventSystem.Publish("ShowNotification", $"Cannot afford {crewData.crewName}'s salary!");
            return false;
        }

        // Deduct payment
        state.money -= moraleData.dailySalary;
        GameManager.Instance.UpdateGameState(state);

        // Update morale data
        moraleData.lastPaymentTime = Time.time;
        moraleData.consecutiveLatePayments = 0;
        moraleData.totalPaid += moraleData.dailySalary;

        // Morale boost for on-time payment
        ModifyMorale(crewID, crewData.paymentMoraleBonus, crewData);

        crewMoraleData[crewID] = moraleData;

        EventSystem.Publish("SalaryPaid", new SalaryPaidEventData
        {
            crewID = crewID,
            amount = moraleData.dailySalary,
            newMorale = moraleData.currentMorale
        });

        if (enableDebugLogging)
        {
            Debug.Log($"[MoraleSystem] Paid ${moraleData.dailySalary} to crew {crewID}. Morale: {moraleData.currentMorale:F1}%");
        }

        return true;
    }

    /// <summary>
    /// Gives a bonus payment to crew member
    /// </summary>
    /// <param name="crewID">Crew ID</param>
    /// <param name="bonusAmount">Bonus amount</param>
    /// <param name="crewData">Crew data reference</param>
    /// <returns>True if bonus was given</returns>
    public bool GiveBonus(string crewID, float bonusAmount, CrewMemberData crewData)
    {
        if (!crewMoraleData.ContainsKey(crewID) || crewData == null)
        {
            return false;
        }

        // Check if player has enough money
        GameState state = GameManager.Instance.CurrentGameState;
        if (state.money < bonusAmount)
        {
            EventSystem.Publish("ShowNotification", "Not enough money for bonus!");
            return false;
        }

        // Deduct bonus
        state.money -= bonusAmount;
        GameManager.Instance.UpdateGameState(state);

        // Update morale
        CrewMoraleData moraleData = crewMoraleData[crewID];
        moraleData.totalBonusReceived += bonusAmount;

        float moraleGain = crewData.bonusMoraleGain;
        ModifyMorale(crewID, moraleGain, crewData);

        crewMoraleData[crewID] = moraleData;

        EventSystem.Publish("BonusGiven", new SalaryPaidEventData
        {
            crewID = crewID,
            amount = bonusAmount,
            newMorale = moraleData.currentMorale
        });

        if (enableDebugLogging)
        {
            Debug.Log($"[MoraleSystem] Gave ${bonusAmount} bonus to crew {crewID}. Morale: {moraleData.currentMorale:F1}%");
        }

        return true;
    }

    /// <summary>
    /// Modifies crew member's morale
    /// </summary>
    /// <param name="crewID">Crew ID</param>
    /// <param name="amount">Amount to change (positive or negative)</param>
    /// <param name="crewData">Crew data reference (optional)</param>
    public void ModifyMorale(string crewID, float amount, CrewMemberData crewData = null)
    {
        if (!crewMoraleData.ContainsKey(crewID))
        {
            return;
        }

        CrewMoraleData moraleData = crewMoraleData[crewID];
        float oldMorale = moraleData.currentMorale;

        moraleData.currentMorale = Mathf.Clamp(moraleData.currentMorale + amount, 0f, 100f);
        crewMoraleData[crewID] = moraleData;

        // Check for quitting
        if (crewData != null && moraleData.currentMorale <= crewData.minimumMorale)
        {
            EventSystem.Publish("CrewWantsToQuit", crewID);
        }

        // Publish morale change event
        EventSystem.Publish("CrewMoraleChanged", new MoraleChangedEventData
        {
            crewID = crewID,
            oldMorale = oldMorale,
            newMorale = moraleData.currentMorale,
            changeAmount = amount
        });

        if (enableDebugLogging && Mathf.Abs(amount) > 0.1f)
        {
            Debug.Log($"[MoraleSystem] Crew {crewID} morale: {oldMorale:F1}% -> {moraleData.currentMorale:F1}% ({amount:+F1;-F1})");
        }
    }

    /// <summary>
    /// Processes daily morale decay for all crew
    /// </summary>
    private void ProcessMoraleDecay()
    {
        foreach (var kvp in crewMoraleData)
        {
            string crewID = kvp.Key;
            CrewMoraleData moraleData = kvp.Value;

            // Apply base decay
            moraleData.currentMorale = Mathf.Max(moraleData.currentMorale - (dailyMoraleDecay / 24f), 0f);
            crewMoraleData[crewID] = moraleData;
        }
    }

    /// <summary>
    /// Processes daily salary payments
    /// </summary>
    private void ProcessDailySalaries()
    {
        // This will be called by day completion, but also runs as backup
        List<string> crewToProcess = new List<string>(crewMoraleData.Keys);

        foreach (string crewID in crewToProcess)
        {
            CrewMoraleData moraleData = crewMoraleData[crewID];
            moraleData.daysEmployed++;
            crewMoraleData[crewID] = moraleData;
        }

        EventSystem.Publish("DailySalaryCheckRequired", crewToProcess.Count);
    }

    /// <summary>
    /// Updates crew relationship bonuses/penalties
    /// </summary>
    private void UpdateCrewRelationships()
    {
        // This would check all crew pairs for synergies and conflicts
        // For now, just placeholder logic
        if (crewMoraleData.Count >= 2)
        {
            // Apply synergy bonuses to compatible crew
            // Apply conflict penalties to incompatible crew
        }
    }

    /// <summary>
    /// Gets current morale for a crew member
    /// </summary>
    /// <param name="crewID">Crew ID</param>
    /// <returns>Morale percentage (0-100)</returns>
    public float GetMorale(string crewID)
    {
        if (crewMoraleData.ContainsKey(crewID))
        {
            return crewMoraleData[crewID].currentMorale;
        }
        return 0f;
    }

    /// <summary>
    /// Gets full morale data for a crew member
    /// </summary>
    public CrewMoraleData GetMoraleData(string crewID)
    {
        return crewMoraleData.GetValueOrDefault(crewID, null);
    }

    /// <summary>
    /// Gets all crew IDs currently employed
    /// </summary>
    public List<string> GetAllCrewIDs()
    {
        return new List<string>(crewMoraleData.Keys);
    }

    /// <summary>
    /// Calculates effective skill bonus for a crew member based on morale
    /// </summary>
    /// <param name="crewID">Crew ID</param>
    /// <param name="crewData">Crew data reference</param>
    /// <returns>Effective skill bonus percentage</returns>
    public float GetEffectiveSkillBonus(string crewID, CrewMemberData crewData)
    {
        if (!crewMoraleData.ContainsKey(crewID) || crewData == null)
        {
            return 0f;
        }

        float morale = crewMoraleData[crewID].currentMorale;
        return crewData.GetEffectiveSkillBonus(morale);
    }

    #region Event Handlers

    private void OnDayCompleted()
    {
        currentDay++;
        ProcessDailySalaries();
    }

    private void OnWeatherChanged(WeatherType newWeather)
    {
        // Apply storm morale penalty
        if (newWeather == WeatherType.Storm)
        {
            foreach (var kvp in crewMoraleData)
            {
                // Would need crew data reference to get specific penalty
                ModifyMorale(kvp.Key, -10f);
            }
        }
    }

    #endregion

    #region Save/Load Integration

    private void OnGatheringSaveData(SaveData data)
    {
        MoraleSystemSaveData saveData = new MoraleSystemSaveData
        {
            crewMoraleDataList = new List<CrewMoraleData>(crewMoraleData.Values),
            lastDecayCheckTime = this.lastDecayCheckTime,
            lastPaymentCheckTime = this.lastPaymentCheckTime,
            currentDay = this.currentDay
        };

        data.moraleSystemData = JsonUtility.ToJson(saveData);
    }

    private void OnApplyingSaveData(SaveData data)
    {
        if (string.IsNullOrEmpty(data.moraleSystemData)) return;

        MoraleSystemSaveData saveData = JsonUtility.FromJson<MoraleSystemSaveData>(data.moraleSystemData);

        // Restore morale data
        crewMoraleData.Clear();
        foreach (CrewMoraleData moraleData in saveData.crewMoraleDataList)
        {
            crewMoraleData[moraleData.crewID] = moraleData;
        }

        lastDecayCheckTime = saveData.lastDecayCheckTime;
        lastPaymentCheckTime = saveData.lastPaymentCheckTime;
        currentDay = saveData.currentDay;

        if (enableDebugLogging)
        {
            Debug.Log($"[MoraleSystem] Loaded morale data for {crewMoraleData.Count} crew members");
        }
    }

    #endregion

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);
        EventSystem.Unsubscribe("DayCompleted", OnDayCompleted);
        EventSystem.Unsubscribe<WeatherType>("WeatherChanged", OnWeatherChanged);

        if (_instance == this)
        {
            _instance = null;
        }
    }
}

/// <summary>
/// Crew morale data structure
/// </summary>
[Serializable]
public class CrewMoraleData
{
    public string crewID;
    public float currentMorale;
    public float dailySalary;
    public float lastPaymentTime;
    public float hireDate;
    public int daysEmployed;
    public float totalPaid;
    public float totalBonusReceived;
    public int consecutiveLatePayments;
}

/// <summary>
/// Morale changed event data
/// </summary>
[Serializable]
public struct MoraleChangedEventData
{
    public string crewID;
    public float oldMorale;
    public float newMorale;
    public float changeAmount;
}

/// <summary>
/// Salary payment event data
/// </summary>
[Serializable]
public struct SalaryPaidEventData
{
    public string crewID;
    public float amount;
    public float newMorale;
}

/// <summary>
/// Save data for morale system
/// </summary>
[Serializable]
public class MoraleSystemSaveData
{
    public List<CrewMoraleData> crewMoraleDataList;
    public float lastDecayCheckTime;
    public float lastPaymentCheckTime;
    public int currentDay;
}
