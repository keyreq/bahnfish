using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Agent 17: Crew & Companion Specialist - CompanionManager.cs
/// Central manager for all companion systems (pets and crew).
/// Coordinates between LoyaltySystem, MoraleSystem, and CompanionAbilitySystem.
/// </summary>
public class CompanionManager : MonoBehaviour
{
    private static CompanionManager _instance;
    public static CompanionManager Instance => _instance;

    [Header("Pet Database")]
    [SerializeField] private List<PetData> allPets = new List<PetData>();

    [Header("Active Companion")]
    [SerializeField] private PetCompanion activePetCompanion;
    [SerializeField] private string activePetID = "";

    [Header("Owned Pets")]
    [SerializeField] private Dictionary<string, OwnedPet> ownedPets = new Dictionary<string, OwnedPet>();

    [Header("Spawn Settings")]
    [SerializeField] private Transform petSpawnPoint;
    [SerializeField] private float spawnDistance = 3f;

    [Header("Settings")]
    [SerializeField] private bool enableDebugLogging = true;

    // System references
    private LoyaltySystem loyaltySystem;
    private MoraleSystem moraleSystem;
    private CompanionAbilitySystem abilitySystem;
    private CrewManager crewManager;

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
        // Get system references
        loyaltySystem = LoyaltySystem.Instance;
        moraleSystem = MoraleSystem.Instance;
        abilitySystem = CompanionAbilitySystem.Instance;
        crewManager = CrewManager.Instance;

        // Subscribe to events
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);
        EventSystem.Subscribe<Fish>("FishCaught", OnFishCaught);
        EventSystem.Subscribe<TimeChangedEventData>("TimeChanged", OnTimeChanged);

        // Load pet database if not set
        if (allPets.Count == 0)
        {
            LoadDefaultPets();
        }

        // Grant starter pet if no pets owned
        if (ownedPets.Count == 0)
        {
            GrantStarterPet();
        }

        if (enableDebugLogging)
        {
            Debug.Log($"[CompanionManager] Initialized with {allPets.Count} available pets");
        }
    }

    /// <summary>
    /// Grants the starter pet (usually a dog)
    /// </summary>
    private void GrantStarterPet()
    {
        PetData starterPet = allPets.FirstOrDefault(p => p.isStarterPet);
        if (starterPet != null)
        {
            UnlockPet(starterPet);
            SwitchActivePet(starterPet.petID);

            EventSystem.Publish("ShowNotification", $"You've received a companion: {starterPet.petName}!");

            if (enableDebugLogging)
            {
                Debug.Log($"[CompanionManager] Granted starter pet: {starterPet.petName}");
            }
        }
    }

    /// <summary>
    /// Unlocks a pet for the player
    /// </summary>
    /// <param name="petData">Pet to unlock</param>
    /// <returns>True if unlock was successful</returns>
    public bool UnlockPet(PetData petData)
    {
        if (petData == null)
        {
            return false;
        }

        // Check if already owned
        if (ownedPets.ContainsKey(petData.petID))
        {
            EventSystem.Publish("ShowNotification", $"You already have {petData.petName}!");
            return false;
        }

        // Check unlock requirements
        if (!CheckUnlockRequirements(petData))
        {
            EventSystem.Publish("ShowNotification", "Requirements not met to unlock this pet");
            return false;
        }

        // Deduct cost if applicable
        if (petData.unlockCost > 0f)
        {
            GameState state = GameManager.Instance.CurrentGameState;
            if (state.money < petData.unlockCost)
            {
                EventSystem.Publish("ShowNotification", "Not enough money!");
                return false;
            }

            state.money -= petData.unlockCost;
            GameManager.Instance.UpdateGameState(state);
        }

        // Add to owned pets
        OwnedPet ownedPet = new OwnedPet
        {
            petID = petData.petID,
            petData = petData,
            acquisitionDate = Time.time,
            instanceObject = null
        };

        ownedPets[petData.petID] = ownedPet;

        // Register with loyalty system
        if (loyaltySystem != null)
        {
            loyaltySystem.RegisterPet(petData);
        }

        // Publish event
        EventSystem.Publish("PetUnlocked", petData.petID);
        EventSystem.Publish("ShowNotification", $"Unlocked {petData.petName}!");

        if (enableDebugLogging)
        {
            Debug.Log($"[CompanionManager] Unlocked pet: {petData.petName}");
        }

        return true;
    }

    /// <summary>
    /// Switches the active pet companion
    /// </summary>
    /// <param name="petID">Pet ID to switch to</param>
    /// <returns>True if switch was successful</returns>
    public bool SwitchActivePet(string petID)
    {
        // Check if pet is owned
        if (!ownedPets.ContainsKey(petID))
        {
            return false;
        }

        // Deactivate current pet
        if (activePetCompanion != null)
        {
            activePetCompanion.Deactivate();
        }

        // Spawn new pet
        OwnedPet pet = ownedPets[petID];
        GameObject petInstance = SpawnPet(pet.petData);

        if (petInstance != null)
        {
            activePetCompanion = petInstance.GetComponent<PetCompanion>();
            activePetID = petID;
            pet.instanceObject = petInstance;
            ownedPets[petID] = pet;

            // Activate pet abilities
            if (loyaltySystem != null && abilitySystem != null)
            {
                float loyalty = loyaltySystem.GetLoyalty(petID);
                abilitySystem.ActivatePetPassiveAbility(pet.petData, loyalty);
            }

            // Publish event
            EventSystem.Publish("CompanionSwitched", petID);
            EventSystem.Publish("ShowNotification", $"{pet.petData.petName} is now following you!");

            if (enableDebugLogging)
            {
                Debug.Log($"[CompanionManager] Switched active pet to: {pet.petData.petName}");
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Spawns a pet in the world
    /// </summary>
    /// <param name="petData">Pet data</param>
    /// <returns>Spawned pet GameObject</returns>
    private GameObject SpawnPet(PetData petData)
    {
        if (petData.petPrefab == null)
        {
            Debug.LogWarning($"[CompanionManager] No prefab assigned for {petData.petName}");
            return null;
        }

        // Determine spawn position
        Vector3 spawnPosition = GetPetSpawnPosition();

        // Instantiate pet
        GameObject petInstance = Instantiate(petData.petPrefab, spawnPosition, Quaternion.identity);
        petInstance.name = $"Pet_{petData.petName}";

        // Configure pet component
        PetCompanion petComponent = petInstance.GetComponent<PetCompanion>();
        if (petComponent == null)
        {
            petComponent = petInstance.AddComponent<PetCompanion>();
        }

        return petInstance;
    }

    /// <summary>
    /// Gets spawn position for pet
    /// </summary>
    private Vector3 GetPetSpawnPosition()
    {
        if (petSpawnPoint != null)
        {
            return petSpawnPoint.position;
        }

        // Spawn near player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 offset = player.transform.right * spawnDistance;
            return player.transform.position + offset;
        }

        return Vector3.zero;
    }

    /// <summary>
    /// Gets active pet companion
    /// </summary>
    /// <returns>Active PetCompanion or null</returns>
    public PetCompanion GetActivePet()
    {
        return activePetCompanion;
    }

    /// <summary>
    /// Gets active pet data
    /// </summary>
    /// <returns>Active PetData or null</returns>
    public PetData GetActivePetData()
    {
        if (!string.IsNullOrEmpty(activePetID) && ownedPets.ContainsKey(activePetID))
        {
            return ownedPets[activePetID].petData;
        }
        return null;
    }

    /// <summary>
    /// Gets all owned pets
    /// </summary>
    /// <returns>List of owned pet data</returns>
    public List<PetData> GetOwnedPets()
    {
        return ownedPets.Values.Select(p => p.petData).ToList();
    }

    /// <summary>
    /// Gets pets available for unlock
    /// </summary>
    /// <returns>List of available pet data</returns>
    public List<PetData> GetAvailablePets()
    {
        return allPets.Where(p => !ownedPets.ContainsKey(p.petID) && !p.isStarterPet).ToList();
    }

    /// <summary>
    /// Checks if unlock requirements are met
    /// </summary>
    private bool CheckUnlockRequirements(PetData petData)
    {
        switch (petData.unlockType)
        {
            case PetUnlockType.Starter:
                return true;

            case PetUnlockType.Purchase:
                // Check money is handled in UnlockPet
                return true;

            case PetUnlockType.QuestReward:
                // Would need to check quest completion
                // For now, assume requirement is met
                return true;

            case PetUnlockType.RareFind:
                // Rare finds are unlocked through gameplay events
                return true;

            case PetUnlockType.StoryUnlock:
                // Would need to check story progress
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Feeds the active pet
    /// </summary>
    /// <returns>True if feeding was successful</returns>
    public bool FeedActivePet()
    {
        if (activePetCompanion != null)
        {
            return activePetCompanion.Feed();
        }
        return false;
    }

    /// <summary>
    /// Pets the active pet - THE KEY FEATURE!
    /// </summary>
    /// <returns>True if petting was successful</returns>
    public bool PetActivePet()
    {
        if (activePetCompanion != null)
        {
            activePetCompanion.OnPlayerInteract();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Plays with the active pet
    /// </summary>
    /// <returns>True if playing was successful</returns>
    public bool PlayWithActivePet()
    {
        if (activePetCompanion != null)
        {
            return activePetCompanion.Play();
        }
        return false;
    }

    /// <summary>
    /// Activates active pet's ability
    /// </summary>
    /// <returns>True if ability was activated</returns>
    public bool ActivateActivePetAbility()
    {
        if (activePetCompanion != null)
        {
            return activePetCompanion.ActivateAbility();
        }
        return false;
    }

    /// <summary>
    /// Gets companion summary (pets + crew)
    /// </summary>
    public CompanionSummary GetCompanionSummary()
    {
        CompanionSummary summary = new CompanionSummary
        {
            activePetID = activePetID,
            totalPetsOwned = ownedPets.Count,
            totalCrewHired = crewManager != null ? crewManager.GetHiredCrew().Count : 0,
            totalDailySalaryCost = crewManager != null ? crewManager.GetTotalDailySalary() : 0f
        };

        // Calculate average pet loyalty
        if (ownedPets.Count > 0 && loyaltySystem != null)
        {
            float totalLoyalty = 0f;
            foreach (var pet in ownedPets.Values)
            {
                totalLoyalty += loyaltySystem.GetLoyalty(pet.petID);
            }
            summary.averagePetLoyalty = totalLoyalty / ownedPets.Count;
        }

        // Calculate average crew morale
        if (crewManager != null && moraleSystem != null)
        {
            List<CrewMemberData> crew = crewManager.GetHiredCrew();
            if (crew.Count > 0)
            {
                float totalMorale = 0f;
                foreach (var member in crew)
                {
                    totalMorale += moraleSystem.GetMorale(member.crewID);
                }
                summary.averageCrewMorale = totalMorale / crew.Count;
            }
        }

        return summary;
    }

    #region Event Handlers

    private void OnFishCaught(Fish fish)
    {
        // Pet reacts to fish being caught
        if (activePetCompanion != null && fish != null)
        {
            // Could trigger pet reaction animation/sound
        }
    }

    private void OnTimeChanged(TimeChangedEventData data)
    {
        // Update pet/crew behaviors based on time of day
    }

    #endregion

    #region Save/Load Integration

    private void OnGatheringSaveData(SaveData data)
    {
        CompanionManagerSaveData saveData = new CompanionManagerSaveData
        {
            activePetID = this.activePetID,
            ownedPetsList = new List<OwnedPetSave>()
        };

        // Save owned pets
        foreach (var kvp in ownedPets)
        {
            saveData.ownedPetsList.Add(new OwnedPetSave
            {
                petID = kvp.Value.petID,
                acquisitionDate = kvp.Value.acquisitionDate
            });
        }

        data.companionManagerData = JsonUtility.ToJson(saveData);
    }

    private void OnApplyingSaveData(SaveData data)
    {
        if (string.IsNullOrEmpty(data.companionManagerData)) return;

        CompanionManagerSaveData saveData = JsonUtility.FromJson<CompanionManagerSaveData>(data.companionManagerData);

        // Restore owned pets
        ownedPets.Clear();
        foreach (OwnedPetSave savedPet in saveData.ownedPetsList)
        {
            PetData petData = allPets.FirstOrDefault(p => p.petID == savedPet.petID);
            if (petData != null)
            {
                OwnedPet pet = new OwnedPet
                {
                    petID = savedPet.petID,
                    petData = petData,
                    acquisitionDate = savedPet.acquisitionDate,
                    instanceObject = null
                };

                ownedPets[savedPet.petID] = pet;

                // Re-register with loyalty system
                if (loyaltySystem != null)
                {
                    loyaltySystem.RegisterPet(petData);
                }
            }
        }

        // Restore active pet
        if (!string.IsNullOrEmpty(saveData.activePetID) && ownedPets.ContainsKey(saveData.activePetID))
        {
            SwitchActivePet(saveData.activePetID);
        }

        if (enableDebugLogging)
        {
            Debug.Log($"[CompanionManager] Loaded {ownedPets.Count} owned pets");
        }
    }

    #endregion

    /// <summary>
    /// Loads default pets (placeholder)
    /// </summary>
    private void LoadDefaultPets()
    {
        // Pets should be created as ScriptableObjects in Unity Editor
        if (enableDebugLogging)
        {
            Debug.LogWarning("[CompanionManager] No pets configured. Create PetData ScriptableObjects!");
        }
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);
        EventSystem.Unsubscribe<Fish>("FishCaught", OnFishCaught);
        EventSystem.Unsubscribe<TimeChangedEventData>("TimeChanged", OnTimeChanged);

        if (_instance == this)
        {
            _instance = null;
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Unlock Test Pet")]
    private void UnlockTestPet()
    {
        if (allPets.Count > 1)
        {
            UnlockPet(allPets[1]);
        }
    }

    [ContextMenu("Switch Pet")]
    private void SwitchTestPet()
    {
        if (ownedPets.Count > 1)
        {
            string nextPet = ownedPets.Keys.FirstOrDefault(k => k != activePetID);
            if (nextPet != null)
            {
                SwitchActivePet(nextPet);
            }
        }
    }

    [ContextMenu("Print Companion Status")]
    private void PrintStatus()
    {
        CompanionSummary summary = GetCompanionSummary();
        Debug.Log($"=== Companion Manager Status ===");
        Debug.Log($"Active Pet: {activePetID}");
        Debug.Log($"Owned Pets: {summary.totalPetsOwned}");
        Debug.Log($"Average Pet Loyalty: {summary.averagePetLoyalty:F1}%");
        Debug.Log($"Hired Crew: {summary.totalCrewHired}");
        Debug.Log($"Average Crew Morale: {summary.averageCrewMorale:F1}%");
        Debug.Log($"Daily Salary Cost: ${summary.totalDailySalaryCost:F2}");
    }
#endif
}

/// <summary>
/// Owned pet data
/// </summary>
[Serializable]
public class OwnedPet
{
    public string petID;
    public PetData petData;
    public float acquisitionDate;
    public GameObject instanceObject;
}

/// <summary>
/// Companion summary statistics
/// </summary>
[Serializable]
public struct CompanionSummary
{
    public string activePetID;
    public int totalPetsOwned;
    public float averagePetLoyalty;
    public int totalCrewHired;
    public float averageCrewMorale;
    public float totalDailySalaryCost;
}

/// <summary>
/// Save data for companion manager
/// </summary>
[Serializable]
public class CompanionManagerSaveData
{
    public string activePetID;
    public List<OwnedPetSave> ownedPetsList;
}

/// <summary>
/// Saved owned pet data
/// </summary>
[Serializable]
public class OwnedPetSave
{
    public string petID;
    public float acquisitionDate;
}
