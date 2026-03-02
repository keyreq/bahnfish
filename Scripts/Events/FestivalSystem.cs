using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Agent 19: Dynamic Events Agent - FestivalSystem.cs
/// Manages town festivals and seasonal celebrations.
///
/// FESTIVAL TYPES:
/// 1. Fishing Tournament - Biggest catch wins $1000
/// 2. Night Market - All prices -20%, special items
/// 3. Harvest Festival - Sell bonuses +50%
/// 4. Dark Moon Festival - Mystic offers free curse cleansing
///
/// EFFECTS:
/// - Occurs every 7 days
/// - Duration: Full day (12 real-time minutes)
/// - NPCs in festive locations
/// - Special dialogues
/// - Town decorations
/// - Unique music
/// </summary>
public class FestivalSystem : MonoBehaviour
{
    private static FestivalSystem _instance;
    public static FestivalSystem Instance => _instance;

    [Header("Festival Configuration")]
    [SerializeField] private bool isFestivalActive = false;
    [SerializeField] private FestivalType currentFestivalType = FestivalType.FishingTournament;
    [SerializeField] private float festivalStartTime = 0f;
    [SerializeField] private float festivalDuration = 720f; // 12 minutes (1 full game day)

    [Header("Tournament Tracking")]
    [SerializeField] private Fish biggestCatchSoFar;
    [SerializeField] private float biggestCatchWeight = 0f;
    [SerializeField] private int tournamentPrize = 1000;
    [SerializeField] private bool tournamentCompleted = false;

    [Header("Festival Bonuses")]
    [SerializeField] private float nightMarketDiscount = 0.8f; // 20% off
    [SerializeField] private float harvestFestivalBonus = 1.5f; // +50% sell prices
    [SerializeField] private bool freeCurseCleansing = false;

    [Header("Decorations")]
    [SerializeField] private List<GameObject> festivalDecorations = new List<GameObject>();
    [SerializeField] private Transform dockTransform;

    [Header("Audio")]
    [SerializeField] private AudioClip festivalMusic;
    [SerializeField] private AudioSource musicSource;

    [Header("Statistics")]
    [SerializeField] private int festivalsAttended = 0;
    [SerializeField] private int tournamentCompetitions = 0;
    [SerializeField] private int tournamentWins = 0;

    [Header("Debug")]
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
        EventSystem.Subscribe<GameEvent>("EventStarted", OnEventStarted);
        EventSystem.Subscribe<GameEvent>("EventEnded", OnEventEnded);
        EventSystem.Subscribe<Fish>("FishCaught", OnFishCaught);

        // Setup audio
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = 0.5f;
        }
    }

    private void OnEventStarted(GameEvent gameEvent)
    {
        if (gameEvent.data.eventType != EventType.Festival) return;

        // Determine festival type based on season or random
        currentFestivalType = DetermineFestivalType();
        StartFestival(currentFestivalType);
    }

    private void OnEventEnded(GameEvent gameEvent)
    {
        if (gameEvent.data.eventType != EventType.Festival) return;

        EndFestival();
    }

    /// <summary>
    /// Determines which type of festival to run
    /// </summary>
    private FestivalType DetermineFestivalType()
    {
        // Could be based on season, current day, or random
        // For now, cycle through them
        int festivalIndex = festivalsAttended % 4;

        return festivalIndex switch
        {
            0 => FestivalType.FishingTournament,
            1 => FestivalType.NightMarket,
            2 => FestivalType.HarvestFestival,
            3 => FestivalType.DarkMoonFestival,
            _ => FestivalType.FishingTournament
        };
    }

    /// <summary>
    /// Starts a festival
    /// </summary>
    public void StartFestival(FestivalType festivalType)
    {
        isFestivalActive = true;
        currentFestivalType = festivalType;
        festivalStartTime = Time.time;
        festivalsAttended++;

        // Reset festival-specific data
        ResetFestivalData();

        // Apply festival effects
        ApplyFestivalEffects(festivalType);

        // Activate decorations
        ActivateDecorations();

        // Play festival music
        if (festivalMusic != null && musicSource != null)
        {
            musicSource.clip = festivalMusic;
            musicSource.Play();
        }

        // Update NPC dialogues
        EventSystem.Publish("UpdateNPCDialogues", "festival");

        // Show notification
        string message = GetFestivalStartMessage(festivalType);
        EventSystem.Publish("ShowNotification", message);

        if (enableDebugLogging)
        {
            Debug.Log($"[FestivalSystem] {festivalType} festival has begun!");
        }
    }

    /// <summary>
    /// Ends the current festival
    /// </summary>
    public void EndFestival()
    {
        string endMessage = GetFestivalEndMessage(currentFestivalType);

        // Award tournament prize if applicable
        if (currentFestivalType == FestivalType.FishingTournament && !tournamentCompleted)
        {
            AwardTournamentPrize();
        }

        // Remove festival effects
        RemoveFestivalEffects(currentFestivalType);

        // Deactivate decorations
        DeactivateDecorations();

        // Stop music
        if (musicSource != null)
        {
            musicSource.Stop();
        }

        // Restore normal NPC dialogues
        EventSystem.Publish("UpdateNPCDialogues", "normal");

        EventSystem.Publish("ShowNotification", endMessage);

        if (enableDebugLogging)
        {
            Debug.Log($"[FestivalSystem] {currentFestivalType} festival has ended!");
        }

        isFestivalActive = false;
    }

    /// <summary>
    /// Resets festival-specific tracking data
    /// </summary>
    private void ResetFestivalData()
    {
        biggestCatchSoFar = null;
        biggestCatchWeight = 0f;
        tournamentCompleted = false;
    }

    /// <summary>
    /// Applies effects for specific festival type
    /// </summary>
    private void ApplyFestivalEffects(FestivalType festivalType)
    {
        switch (festivalType)
        {
            case FestivalType.FishingTournament:
                tournamentCompetitions++;
                EventSystem.Publish("ShowNotification", "Fishing Tournament: Catch the biggest fish to win!");
                break;

            case FestivalType.NightMarket:
                EventSystem.Publish("ShopPriceMultiplier", nightMarketDiscount);
                EventSystem.Publish("EnableSpecialShopItems", true);
                break;

            case FestivalType.HarvestFestival:
                EventSystem.Publish("SellPriceMultiplier", harvestFestivalBonus);
                break;

            case FestivalType.DarkMoonFestival:
                freeCurseCleansing = true;
                EventSystem.Publish("EnableFreeCurseCleansing", true);
                EventSystem.Publish("ShowNotification", "The Mystic offers free curse cleansing during the Dark Moon Festival!");
                break;
        }
    }

    /// <summary>
    /// Removes festival effects
    /// </summary>
    private void RemoveFestivalEffects(FestivalType festivalType)
    {
        switch (festivalType)
        {
            case FestivalType.NightMarket:
                EventSystem.Publish("ShopPriceMultiplier", 1f);
                EventSystem.Publish("EnableSpecialShopItems", false);
                break;

            case FestivalType.HarvestFestival:
                EventSystem.Publish("SellPriceMultiplier", 1f);
                break;

            case FestivalType.DarkMoonFestival:
                freeCurseCleansing = false;
                EventSystem.Publish("EnableFreeCurseCleansing", false);
                break;
        }
    }

    /// <summary>
    /// Activates festival decorations in town
    /// </summary>
    private void ActivateDecorations()
    {
        foreach (GameObject decoration in festivalDecorations)
        {
            if (decoration != null)
            {
                decoration.SetActive(true);
            }
        }

        EventSystem.Publish("UpdateDockDecorations", "festival");
    }

    /// <summary>
    /// Deactivates festival decorations
    /// </summary>
    private void DeactivateDecorations()
    {
        foreach (GameObject decoration in festivalDecorations)
        {
            if (decoration != null)
            {
                decoration.SetActive(false);
            }
        }

        EventSystem.Publish("UpdateDockDecorations", "normal");
    }

    /// <summary>
    /// Tracks fish caught during tournament
    /// </summary>
    private void OnFishCaught(Fish fish)
    {
        if (!isFestivalActive) return;
        if (currentFestivalType != FestivalType.FishingTournament) return;

        // Check if this is the biggest catch
        if (fish.weight > biggestCatchWeight)
        {
            biggestCatchWeight = fish.weight;
            biggestCatchSoFar = fish;

            EventSystem.Publish("ShowNotification", $"New tournament leader! {fish.name} at {fish.weight:F1} lbs!");

            if (enableDebugLogging)
            {
                Debug.Log($"[FestivalSystem] New biggest catch: {fish.name} ({fish.weight:F1} lbs)");
            }
        }
    }

    /// <summary>
    /// Awards the tournament prize to the winner
    /// </summary>
    private void AwardTournamentPrize()
    {
        if (biggestCatchSoFar == null)
        {
            EventSystem.Publish("ShowNotification", "Tournament ended with no catches. No prize awarded.");
            return;
        }

        // Award money
        EventSystem.Publish("AddMoney", tournamentPrize);

        tournamentCompleted = true;
        tournamentWins++;

        EventSystem.Publish("ShowNotification",
            $"You won the tournament with a {biggestCatchSoFar.name} weighing {biggestCatchWeight:F1} lbs! Prize: ${tournamentPrize}");

        if (enableDebugLogging)
        {
            Debug.Log($"[FestivalSystem] Tournament won! Prize: ${tournamentPrize}");
        }
    }

    /// <summary>
    /// Gets the festival start message
    /// </summary>
    private string GetFestivalStartMessage(FestivalType festivalType)
    {
        return festivalType switch
        {
            FestivalType.FishingTournament => "The Fishing Tournament begins! Catch the biggest fish!",
            FestivalType.NightMarket => "The Night Market opens! Special items and discounts available!",
            FestivalType.HarvestFestival => "The Harvest Festival celebrates! Sell your catch for bonus prices!",
            FestivalType.DarkMoonFestival => "The Dark Moon Festival begins! Free curse cleansing available!",
            _ => "A festival has begun!"
        };
    }

    /// <summary>
    /// Gets the festival end message
    /// </summary>
    private string GetFestivalEndMessage(FestivalType festivalType)
    {
        return festivalType switch
        {
            FestivalType.FishingTournament => biggestCatchSoFar != null
                ? $"Tournament ended! Winner: {biggestCatchSoFar.name} ({biggestCatchWeight:F1} lbs)"
                : "Tournament ended with no catches.",
            FestivalType.NightMarket => "The Night Market has closed for the night.",
            FestivalType.HarvestFestival => "The Harvest Festival has concluded. Thank you for celebrating with us!",
            FestivalType.DarkMoonFestival => "The Dark Moon Festival ends as dawn approaches.",
            _ => "The festival has ended."
        };
    }

    /// <summary>
    /// Gets current festival information
    /// </summary>
    public FestivalInfo GetCurrentFestival()
    {
        return new FestivalInfo
        {
            isActive = isFestivalActive,
            festivalType = currentFestivalType,
            timeRemaining = festivalDuration - (Time.time - festivalStartTime),
            biggestCatch = biggestCatchSoFar,
            biggestCatchWeight = biggestCatchWeight
        };
    }

    public bool IsActive()
    {
        return isFestivalActive;
    }

    public void ForceTrigger(FestivalType type)
    {
        StartFestival(type);
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<GameEvent>("EventStarted", OnEventStarted);
        EventSystem.Unsubscribe<GameEvent>("EventEnded", OnEventEnded);
        EventSystem.Unsubscribe<Fish>("FishCaught", OnFishCaught);

        if (_instance == this)
        {
            _instance = null;
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Start Fishing Tournament")]
    private void StartTournament()
    {
        StartFestival(FestivalType.FishingTournament);
    }

    [ContextMenu("Start Night Market")]
    private void StartNightMarket()
    {
        StartFestival(FestivalType.NightMarket);
    }

    [ContextMenu("Start Harvest Festival")]
    private void StartHarvestFestival()
    {
        StartFestival(FestivalType.HarvestFestival);
    }

    [ContextMenu("Start Dark Moon Festival")]
    private void StartDarkMoonFestival()
    {
        StartFestival(FestivalType.DarkMoonFestival);
    }

    [ContextMenu("End Current Festival")]
    private void EndFestivalEditor()
    {
        if (isFestivalActive)
        {
            EndFestival();
        }
    }
#endif
}

/// <summary>
/// Types of festivals
/// </summary>
[System.Serializable]
public enum FestivalType
{
    FishingTournament,
    NightMarket,
    HarvestFestival,
    DarkMoonFestival
}

/// <summary>
/// Festival information structure
/// </summary>
[System.Serializable]
public struct FestivalInfo
{
    public bool isActive;
    public FestivalType festivalType;
    public float timeRemaining;
    public Fish biggestCatch;
    public float biggestCatchWeight;
}
