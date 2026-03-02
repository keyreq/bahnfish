using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Agent 17: Crew & Companion Specialist - CrewManager.cs
/// Manages crew hiring, assignments, salaries, and interactions.
/// </summary>
public class CrewManager : MonoBehaviour
{
    private static CrewManager _instance;
    public static CrewManager Instance => _instance;

    [Header("Crew Database")]
    [SerializeField] private List<CrewMemberData> availableCrewMembers = new List<CrewMemberData>();

    [Header("Hired Crew")]
    [SerializeField] private Dictionary<string, HiredCrewMember> hiredCrew = new Dictionary<string, HiredCrewMember>();
    [SerializeField] private int maxCrewSize = 4;
    [SerializeField] private bool canExpandCrewSize = true;
    [SerializeField] private int expandedMaxSize = 6;

    [Header("Salary Management")]
    [SerializeField] private bool autoPaySalaries = true;
    [SerializeField] private float salaryDayInterval = 86400f; // 24 hours in seconds

    [Header("Settings")]
    [SerializeField] private bool enableDebugLogging = true;

    // Systems
    private MoraleSystem moraleSystem;
    private CompanionAbilitySystem abilitySystem;

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
        moraleSystem = MoraleSystem.Instance;
        abilitySystem = CompanionAbilitySystem.Instance;

        // Subscribe to events
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);
        EventSystem.Subscribe("DayCompleted", OnDayCompleted);
        EventSystem.Subscribe<string>("CrewWantsToQuit", OnCrewWantsToQuit);

        // Load available crew members if not set
        if (availableCrewMembers.Count == 0)
        {
            LoadDefaultCrewMembers();
        }

        if (enableDebugLogging)
        {
            Debug.Log($"[CrewManager] Initialized with {availableCrewMembers.Count} available crew members");
        }
    }

    /// <summary>
    /// Hires a crew member
    /// </summary>
    /// <param name="crewData">Crew member data</param>
    /// <returns>True if hiring was successful</returns>
    public bool HireCrewMember(CrewMemberData crewData)
    {
        if (crewData == null)
        {
            Debug.LogError("[CrewManager] Cannot hire null crew member!");
            return false;
        }

        // Check if already hired
        if (hiredCrew.ContainsKey(crewData.crewID))
        {
            EventSystem.Publish("ShowNotification", $"{crewData.crewName} is already hired!");
            return false;
        }

        // Check crew size limit
        if (hiredCrew.Count >= maxCrewSize)
        {
            EventSystem.Publish("ShowNotification", "Crew is at maximum capacity!");
            return false;
        }

        // Check unlock requirements
        if (!CheckHireRequirements(crewData))
        {
            EventSystem.Publish("ShowNotification", "Requirements not met to hire this crew member");
            return false;
        }

        // Check compatibility with existing crew
        if (!CheckCrewCompatibility(crewData))
        {
            EventSystem.Publish("ShowNotification", $"{crewData.crewName} conflicts with existing crew!");
            return false;
        }

        // Create hired crew member
        HiredCrewMember hiredMember = new HiredCrewMember
        {
            crewID = crewData.crewID,
            crewData = crewData,
            hireDate = Time.time,
            assignedStation = crewData.assignedStation,
            stationObject = null
        };

        hiredCrew[crewData.crewID] = hiredMember;

        // Register with morale system
        if (moraleSystem != null)
        {
            moraleSystem.RegisterCrewMember(crewData);
        }

        // Activate crew skills
        if (abilitySystem != null && moraleSystem != null)
        {
            float morale = moraleSystem.GetMorale(crewData.crewID);
            abilitySystem.ActivateCrewPassiveSkills(crewData, morale);
        }

        // Publish event
        EventSystem.Publish("CrewHired", crewData.crewID);
        EventSystem.Publish("ShowNotification", $"Hired {crewData.crewName}!");

        if (enableDebugLogging)
        {
            Debug.Log($"[CrewManager] Hired {crewData.crewName} as {crewData.specialization}");
        }

        return true;
    }

    /// <summary>
    /// Fires a crew member
    /// </summary>
    /// <param name="crewID">Crew ID to fire</param>
    /// <returns>True if firing was successful</returns>
    public bool FireCrewMember(string crewID)
    {
        if (!hiredCrew.ContainsKey(crewID))
        {
            return false;
        }

        HiredCrewMember member = hiredCrew[crewID];

        // Remove from systems
        if (moraleSystem != null)
        {
            moraleSystem.RemoveCrewMember(crewID);
        }

        // Remove from hired crew
        hiredCrew.Remove(crewID);

        // Publish event
        EventSystem.Publish("CrewFired", crewID);
        EventSystem.Publish("ShowNotification", $"{member.crewData.crewName} has left the crew");

        if (enableDebugLogging)
        {
            Debug.Log($"[CrewManager] Fired {member.crewData.crewName}");
        }

        return true;
    }

    /// <summary>
    /// Pays salary to a specific crew member
    /// </summary>
    /// <param name="crewID">Crew ID</param>
    /// <returns>True if payment was successful</returns>
    public bool PayCrewSalary(string crewID)
    {
        if (!hiredCrew.ContainsKey(crewID))
        {
            return false;
        }

        HiredCrewMember member = hiredCrew[crewID];

        if (moraleSystem != null)
        {
            return moraleSystem.PaySalary(crewID, member.crewData);
        }

        return false;
    }

    /// <summary>
    /// Pays salaries to all crew members
    /// </summary>
    /// <returns>Number of successful payments</returns>
    public int PayAllSalaries()
    {
        int successfulPayments = 0;

        foreach (var kvp in hiredCrew)
        {
            if (PayCrewSalary(kvp.Key))
            {
                successfulPayments++;
            }
        }

        if (enableDebugLogging)
        {
            Debug.Log($"[CrewManager] Paid {successfulPayments}/{hiredCrew.Count} crew salaries");
        }

        return successfulPayments;
    }

    /// <summary>
    /// Gives a bonus to a crew member
    /// </summary>
    /// <param name="crewID">Crew ID</param>
    /// <param name="bonusAmount">Bonus amount</param>
    /// <returns>True if bonus was given</returns>
    public bool GiveCrewBonus(string crewID, float bonusAmount)
    {
        if (!hiredCrew.ContainsKey(crewID))
        {
            return false;
        }

        HiredCrewMember member = hiredCrew[crewID];

        if (moraleSystem != null)
        {
            return moraleSystem.GiveBonus(crewID, bonusAmount, member.crewData);
        }

        return false;
    }

    /// <summary>
    /// Assigns crew member to a station
    /// </summary>
    /// <param name="crewID">Crew ID</param>
    /// <param name="station">Station to assign to</param>
    /// <returns>True if assignment was successful</returns>
    public bool AssignToStation(string crewID, CrewStation station)
    {
        if (!hiredCrew.ContainsKey(crewID))
        {
            return false;
        }

        HiredCrewMember member = hiredCrew[crewID];
        member.assignedStation = station;
        hiredCrew[crewID] = member;

        EventSystem.Publish("CrewStationChanged", new CrewStationChangedEventData
        {
            crewID = crewID,
            newStation = station
        });

        if (enableDebugLogging)
        {
            Debug.Log($"[CrewManager] Assigned {member.crewData.crewName} to {station}");
        }

        return true;
    }

    /// <summary>
    /// Gets all hired crew members
    /// </summary>
    /// <returns>List of hired crew data</returns>
    public List<CrewMemberData> GetHiredCrew()
    {
        return hiredCrew.Values.Select(h => h.crewData).ToList();
    }

    /// <summary>
    /// Gets hired crew member by ID
    /// </summary>
    public HiredCrewMember GetHiredCrewMember(string crewID)
    {
        return hiredCrew.GetValueOrDefault(crewID, null);
    }

    /// <summary>
    /// Gets crew members available for hire at a location
    /// </summary>
    /// <param name="locationID">Location ID</param>
    /// <returns>List of available crew members</returns>
    public List<CrewMemberData> GetAvailableCrewAtLocation(string locationID)
    {
        return availableCrewMembers.Where(c =>
            c.hirableAtLocation == locationID &&
            !hiredCrew.ContainsKey(c.crewID) &&
            CheckHireRequirements(c)
        ).ToList();
    }

    /// <summary>
    /// Gets total daily salary cost
    /// </summary>
    /// <returns>Total daily salary</returns>
    public float GetTotalDailySalary()
    {
        float total = 0f;

        foreach (var member in hiredCrew.Values)
        {
            total += member.crewData.dailySalary;
        }

        return total;
    }

    /// <summary>
    /// Gets skill bonus for a specialization
    /// </summary>
    /// <param name="specialization">Specialization type</param>
    /// <returns>Combined skill bonus from all crew of that type</returns>
    public float GetSpecializationBonus(CrewSpecialization specialization)
    {
        float totalBonus = 0f;

        foreach (var member in hiredCrew.Values)
        {
            if (member.crewData.specialization == specialization && moraleSystem != null)
            {
                float morale = moraleSystem.GetMorale(member.crewID);
                totalBonus += member.crewData.GetEffectiveSkillBonus(morale);
            }
        }

        return totalBonus;
    }

    /// <summary>
    /// Checks if hire requirements are met
    /// </summary>
    private bool CheckHireRequirements(CrewMemberData crewData)
    {
        // Check if available from start
        if (crewData.availableFromStart)
        {
            return true;
        }

        // Check required quest
        if (!string.IsNullOrEmpty(crewData.requiredQuestID))
        {
            // Would need to check quest completion status
            // For now, assume requirement is met
        }

        // Check minimum player level
        // Would need player level system
        // For now, assume requirement is met

        return true;
    }

    /// <summary>
    /// Checks if crew member is compatible with existing crew
    /// </summary>
    private bool CheckCrewCompatibility(CrewMemberData newCrew)
    {
        foreach (var member in hiredCrew.Values)
        {
            if (!newCrew.IsCompatibleWith(member.crewData))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Expands maximum crew size
    /// </summary>
    /// <returns>True if expansion was successful</returns>
    public bool ExpandCrewSize()
    {
        if (!canExpandCrewSize || maxCrewSize >= expandedMaxSize)
        {
            return false;
        }

        maxCrewSize = expandedMaxSize;
        EventSystem.Publish("ShowNotification", $"Crew capacity expanded to {maxCrewSize}!");

        if (enableDebugLogging)
        {
            Debug.Log($"[CrewManager] Crew size expanded to {maxCrewSize}");
        }

        return true;
    }

    /// <summary>
    /// Loads default crew members (placeholder)
    /// </summary>
    private void LoadDefaultCrewMembers()
    {
        // Crew members should be created as ScriptableObjects in Unity Editor
        // This is just a placeholder
        if (enableDebugLogging)
        {
            Debug.LogWarning("[CrewManager] No crew members configured. Create CrewMemberData ScriptableObjects!");
        }
    }

    #region Event Handlers

    private void OnDayCompleted()
    {
        if (autoPaySalaries)
        {
            PayAllSalaries();
        }
    }

    private void OnCrewWantsToQuit(string crewID)
    {
        if (!hiredCrew.ContainsKey(crewID))
        {
            return;
        }

        HiredCrewMember member = hiredCrew[crewID];

        EventSystem.Publish("ShowNotification", $"WARNING: {member.crewData.crewName} is unhappy and may quit!");

        // Show quitting dialogue
        string dialogue = member.crewData.quittingDialogue.Length > 0
            ? member.crewData.quittingDialogue[UnityEngine.Random.Range(0, member.crewData.quittingDialogue.Length)]
            : "I can't work under these conditions!";

        EventSystem.Publish("ShowDialogue", new DialogueEventData
        {
            speakerName = member.crewData.crewName,
            dialogue = dialogue,
            portrait = member.crewData.portrait
        });

        // Actually quit if morale is at minimum
        if (moraleSystem != null)
        {
            float morale = moraleSystem.GetMorale(crewID);
            if (morale <= member.crewData.minimumMorale)
            {
                FireCrewMember(crewID);
            }
        }
    }

    #endregion

    #region Save/Load Integration

    private void OnGatheringSaveData(SaveData data)
    {
        CrewManagerSaveData saveData = new CrewManagerSaveData
        {
            hiredCrewList = new List<HiredCrewSave>(),
            maxCrewSize = this.maxCrewSize
        };

        // Save hired crew
        foreach (var kvp in hiredCrew)
        {
            HiredCrewMember member = kvp.Value;
            saveData.hiredCrewList.Add(new HiredCrewSave
            {
                crewID = member.crewID,
                hireDate = member.hireDate,
                assignedStation = member.assignedStation
            });
        }

        data.crewManagerData = JsonUtility.ToJson(saveData);
    }

    private void OnApplyingSaveData(SaveData data)
    {
        if (string.IsNullOrEmpty(data.crewManagerData)) return;

        CrewManagerSaveData saveData = JsonUtility.FromJson<CrewManagerSaveData>(data.crewManagerData);

        // Restore hired crew
        hiredCrew.Clear();
        foreach (HiredCrewSave savedCrew in saveData.hiredCrewList)
        {
            // Find crew data
            CrewMemberData crewData = availableCrewMembers.FirstOrDefault(c => c.crewID == savedCrew.crewID);
            if (crewData != null)
            {
                HiredCrewMember member = new HiredCrewMember
                {
                    crewID = savedCrew.crewID,
                    crewData = crewData,
                    hireDate = savedCrew.hireDate,
                    assignedStation = savedCrew.assignedStation,
                    stationObject = null
                };

                hiredCrew[savedCrew.crewID] = member;

                // Re-register with systems
                if (moraleSystem != null)
                {
                    moraleSystem.RegisterCrewMember(crewData);
                }
            }
        }

        maxCrewSize = saveData.maxCrewSize;

        if (enableDebugLogging)
        {
            Debug.Log($"[CrewManager] Loaded {hiredCrew.Count} hired crew members");
        }
    }

    #endregion

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);
        EventSystem.Unsubscribe("DayCompleted", OnDayCompleted);
        EventSystem.Unsubscribe<string>("CrewWantsToQuit", OnCrewWantsToQuit);

        if (_instance == this)
        {
            _instance = null;
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Hire Test Crew")]
    private void HireTestCrew()
    {
        if (availableCrewMembers.Count > 0)
        {
            HireCrewMember(availableCrewMembers[0]);
        }
    }

    [ContextMenu("Pay All Salaries")]
    private void PayAllSalariesEditor()
    {
        PayAllSalaries();
    }

    [ContextMenu("Print Crew Status")]
    private void PrintStatus()
    {
        Debug.Log($"=== Crew Manager Status ===");
        Debug.Log($"Hired Crew: {hiredCrew.Count}/{maxCrewSize}");
        Debug.Log($"Total Daily Salary: ${GetTotalDailySalary():F2}");
        foreach (var member in hiredCrew.Values)
        {
            float morale = moraleSystem != null ? moraleSystem.GetMorale(member.crewID) : 0f;
            Debug.Log($"  - {member.crewData.crewName} ({member.crewData.specialization}): Morale {morale:F1}%");
        }
    }
#endif
}

/// <summary>
/// Hired crew member data
/// </summary>
[Serializable]
public class HiredCrewMember
{
    public string crewID;
    public CrewMemberData crewData;
    public float hireDate;
    public CrewStation assignedStation;
    public GameObject stationObject;
}

/// <summary>
/// Crew station changed event data
/// </summary>
[Serializable]
public struct CrewStationChangedEventData
{
    public string crewID;
    public CrewStation newStation;
}

/// <summary>
/// Dialogue event data
/// </summary>
[Serializable]
public struct DialogueEventData
{
    public string speakerName;
    public string dialogue;
    public Sprite portrait;
}

/// <summary>
/// Save data for crew manager
/// </summary>
[Serializable]
public class CrewManagerSaveData
{
    public List<HiredCrewSave> hiredCrewList;
    public int maxCrewSize;
}

/// <summary>
/// Saved hired crew data
/// </summary>
[Serializable]
public class HiredCrewSave
{
    public string crewID;
    public float hireDate;
    public CrewStation assignedStation;
}
