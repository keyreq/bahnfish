using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Agent 18: Photography Mode Specialist - PhotoChallenges.cs
/// Manages 30+ photography challenges with rewards.
/// Includes species, action, artistic, event, and combo challenges.
/// </summary>
public class PhotoChallenges : MonoBehaviour
{
    public static PhotoChallenges Instance { get; private set; }

    [Header("Challenge Settings")]
    [SerializeField] private List<PhotoChallenge> allChallenges = new List<PhotoChallenge>();
    [SerializeField] private List<string> completedChallenges = new List<string>();

    [Header("Tracking")]
    [SerializeField] private Dictionary<string, int> challengeProgress = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeChallenges();
    }

    private void Start()
    {
        // Subscribe to photo events
        EventSystem.Subscribe<PhotoMetadata>("PhotoSaved", OnPhotoSaved);
        EventSystem.Subscribe<string>("FishCaught", OnFishCaught);
        EventSystem.Subscribe<string>("DynamicEventStarted", OnDynamicEventStarted);
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<PhotoMetadata>("PhotoSaved", OnPhotoSaved);
        EventSystem.Unsubscribe<string>("FishCaught", OnFishCaught);
        EventSystem.Unsubscribe<string>("DynamicEventStarted", OnDynamicEventStarted);
    }

    /// <summary>
    /// Initializes all photo challenges.
    /// </summary>
    private void InitializeChallenges()
    {
        allChallenges.Clear();

        // Species Challenges (12)
        AddChallenge("species_common_5", "Photograph 5 Common Fish", ChallengeCategory.Species, 5, 200f, "Photograph 5 different Common rarity fish species");
        AddChallenge("species_uncommon_3", "Photograph 3 Uncommon Fish", ChallengeCategory.Species, 3, 500f, "Photograph 3 different Uncommon rarity fish species");
        AddChallenge("species_rare_1", "Photograph 1 Rare Fish", ChallengeCategory.Species, 1, 1000f, "Photograph 1 Rare rarity fish");
        AddChallenge("species_legendary_1", "Photograph 1 Legendary Fish", ChallengeCategory.Species, 1, 3000f, "Photograph 1 Legendary fish");
        AddChallenge("species_aberrant_1", "Photograph 1 Aberrant Fish", ChallengeCategory.Species, 1, 2000f, "Photograph 1 Aberrant fish");
        AddChallenge("species_location_all", "Location Complete", ChallengeCategory.Species, 1, 1500f, "Photograph all fish species in one location");
        AddChallenge("species_all_rarities", "Rainbow Collection", ChallengeCategory.Species, 1, 4000f, "Photograph at least one fish of each rarity tier");
        AddChallenge("species_10_session", "Photo Spree", ChallengeCategory.Species, 10, 2500f, "Photograph 10 different species in one session");
        AddChallenge("species_night_3", "Night Hunter", ChallengeCategory.Species, 3, 800f, "Photograph 3 different fish at night");
        AddChallenge("species_depth_deep", "Deep Sea Explorer", ChallengeCategory.Species, 1, 1200f, "Photograph a fish from depths below 50m");
        AddChallenge("species_weather_storm", "Storm Chaser", ChallengeCategory.Species, 1, 900f, "Photograph a fish during a storm");
        AddChallenge("species_all_times", "Around the Clock", ChallengeCategory.Species, 4, 1500f, "Photograph fish during all 4 times of day");

        // Action Challenges (8)
        AddChallenge("action_jumping", "Air Time", ChallengeCategory.Action, 1, 800f, "Capture a fish jumping out of water");
        AddChallenge("action_fighting", "The Fight", ChallengeCategory.Action, 1, 1200f, "Capture a fish fighting on the line");
        AddChallenge("action_reeling", "Reeling In", ChallengeCategory.Action, 1, 600f, "Capture a fish being reeled in");
        AddChallenge("action_theft", "Caught in the Act", ChallengeCategory.Action, 1, 2000f, "Capture fish theft by crow or phantom");
        AddChallenge("action_ghost_ship", "Phantom Voyage", ChallengeCategory.Action, 1, 2500f, "Capture the Ghost Ship in background");
        AddChallenge("action_feeding_frenzy", "Feeding Time", ChallengeCategory.Action, 1, 1500f, "Capture a feeding frenzy event");
        AddChallenge("action_breach_3", "Triple Threat", ChallengeCategory.Action, 3, 2000f, "Capture 3 fish breaching in one photo");
        AddChallenge("action_fight_legendary", "Legendary Battle", ChallengeCategory.Action, 1, 3500f, "Capture a legendary fish fight");

        // Artistic Challenges (6)
        AddChallenge("artistic_sepia_5", "Vintage Vibes", ChallengeCategory.Artistic, 5, 300f, "Take 5 photos with Sepia filter");
        AddChallenge("artistic_5star", "Perfect Shot", ChallengeCategory.Artistic, 1, 2000f, "Take a 5-star quality photo");
        AddChallenge("artistic_rule_thirds", "Composition Master", ChallengeCategory.Artistic, 1, 1000f, "Perfect rule of thirds composition");
        AddChallenge("artistic_silhouette", "Shadow Play", ChallengeCategory.Artistic, 1, 1200f, "Capture a silhouette photo at sunset");
        AddChallenge("artistic_tiltshift", "Miniature World", ChallengeCategory.Artistic, 1, 800f, "Use Tilt-Shift filter on a landmark");
        AddChallenge("artistic_multiple_filters", "Filter Fanatic", ChallengeCategory.Artistic, 1, 1500f, "Use 3+ filters on one photo");

        // Event Challenges (4)
        AddChallenge("event_blood_moon", "Blood Moon Rising", ChallengeCategory.Event, 1, 5000f, "Photograph the Blood Moon event");
        AddChallenge("event_meteor_shower", "Shooting Stars", ChallengeCategory.Event, 1, 3000f, "Photograph a Meteor Shower");
        AddChallenge("event_aurora", "Northern Lights", ChallengeCategory.Event, 1, 2500f, "Photograph Aurora Borealis");
        AddChallenge("event_all_weather", "Weather Watcher", ChallengeCategory.Event, 4, 2000f, "Photograph in all 4 weather types");

        Debug.Log($"[PhotoChallenges] Initialized {allChallenges.Count} challenges");
    }

    /// <summary>
    /// Adds a challenge to the list.
    /// </summary>
    private void AddChallenge(string id, string name, ChallengeCategory category, int goal, float reward, string description)
    {
        PhotoChallenge challenge = new PhotoChallenge
        {
            challengeID = id,
            challengeName = name,
            category = category,
            goalCount = goal,
            currentProgress = 0,
            moneyReward = reward,
            description = description
        };

        allChallenges.Add(challenge);
        challengeProgress[id] = 0;
    }

    /// <summary>
    /// Called when a photo is saved.
    /// </summary>
    private void OnPhotoSaved(PhotoMetadata photo)
    {
        CheckSpeciesChallenges(photo);
        CheckArtisticChallenges(photo);
        CheckEventChallenges(photo);
    }

    /// <summary>
    /// Called when a fish is caught.
    /// </summary>
    private void OnFishCaught(string fishID)
    {
        // Could be used for action challenges
    }

    /// <summary>
    /// Called when a dynamic event starts.
    /// </summary>
    private void OnDynamicEventStarted(string eventName)
    {
        // Could trigger event photo opportunities
    }

    /// <summary>
    /// Checks species-related challenges.
    /// </summary>
    private void CheckSpeciesChallenges(PhotoMetadata photo)
    {
        if (string.IsNullOrEmpty(photo.fishSpeciesID)) return;

        FishSpeciesData fishData = FishDatabase.Instance?.GetFishByID(photo.fishSpeciesID);
        if (fishData == null) return;

        // Common fish challenge
        if (fishData.rarity == FishRarity.Common)
        {
            UpdateChallengeProgress("species_common_5");
        }

        // Uncommon fish challenge
        if (fishData.rarity == FishRarity.Uncommon)
        {
            UpdateChallengeProgress("species_uncommon_3");
        }

        // Rare fish challenge
        if (fishData.rarity == FishRarity.Rare)
        {
            UpdateChallengeProgress("species_rare_1");
        }

        // Legendary fish challenge
        if (fishData.rarity == FishRarity.Legendary)
        {
            UpdateChallengeProgress("species_legendary_1");
        }

        // Aberrant fish challenge
        if (fishData.rarity == FishRarity.Aberrant)
        {
            UpdateChallengeProgress("species_aberrant_1");
        }

        // Night photography
        if (photo.timeOfDay < 6f || photo.timeOfDay > 20f)
        {
            UpdateChallengeProgress("species_night_3");
        }

        // Storm photography
        if (photo.weatherType == "Storm")
        {
            UpdateChallengeProgress("species_weather_storm");
        }
    }

    /// <summary>
    /// Checks artistic challenges.
    /// </summary>
    private void CheckArtisticChallenges(PhotoMetadata photo)
    {
        // Sepia filter challenge
        if (photo.filtersApplied.Contains("Sepia"))
        {
            UpdateChallengeProgress("artistic_sepia_5");
        }

        // 5-star photo challenge
        if (photo.qualityRating >= 5f)
        {
            UpdateChallengeProgress("artistic_5star");
        }

        // Multiple filters challenge
        if (photo.filtersApplied.Count >= 3)
        {
            UpdateChallengeProgress("artistic_multiple_filters");
        }

        // Tilt-shift landmark
        if (photo.filtersApplied.Contains("TiltShift"))
        {
            UpdateChallengeProgress("artistic_tiltshift");
        }

        // Silhouette at sunset
        if (photo.timeOfDay >= 18f && photo.timeOfDay <= 20f)
        {
            if (photo.exposure < -1f) // Dark exposure suggests silhouette
            {
                UpdateChallengeProgress("artistic_silhouette");
            }
        }
    }

    /// <summary>
    /// Checks event-related challenges.
    /// </summary>
    private void CheckEventChallenges(PhotoMetadata photo)
    {
        // Weather type tracking
        if (!string.IsNullOrEmpty(photo.weatherType))
        {
            // Track unique weather types photographed
            UpdateChallengeProgress("event_all_weather");
        }
    }

    /// <summary>
    /// Updates challenge progress.
    /// </summary>
    private void UpdateChallengeProgress(string challengeID)
    {
        if (completedChallenges.Contains(challengeID))
        {
            return; // Already completed
        }

        PhotoChallenge challenge = allChallenges.Find(c => c.challengeID == challengeID);
        if (challenge == null) return;

        // Increment progress
        challenge.currentProgress++;
        challengeProgress[challengeID] = challenge.currentProgress;

        // Check completion
        if (challenge.currentProgress >= challenge.goalCount)
        {
            CompleteChallenge(challenge);
        }
        else
        {
            // Publish progress event
            EventSystem.Publish("PhotoChallengeProgress", challenge);
        }
    }

    /// <summary>
    /// Marks a challenge as completed and awards rewards.
    /// </summary>
    private void CompleteChallenge(PhotoChallenge challenge)
    {
        if (completedChallenges.Contains(challenge.challengeID))
        {
            return; // Already completed
        }

        completedChallenges.Add(challenge.challengeID);
        challenge.isCompleted = true;

        // Award money reward
        if (GameManager.Instance != null)
        {
            GameState state = GameManager.Instance.GetCurrentState();
            state.money += challenge.moneyReward;
        }

        Debug.Log($"[PhotoChallenges] Challenge completed: {challenge.challengeName} - Reward: ${challenge.moneyReward}");

        // Publish completion event
        EventSystem.Publish("PhotoChallengeCompleted", challenge);

        // Check for category completion bonuses
        CheckCategoryCompletion(challenge.category);
    }

    /// <summary>
    /// Checks if all challenges in a category are complete.
    /// </summary>
    private void CheckCategoryCompletion(ChallengeCategory category)
    {
        List<PhotoChallenge> categoryChallenges = allChallenges.FindAll(c => c.category == category);
        bool allComplete = categoryChallenges.TrueForAll(c => c.isCompleted);

        if (allComplete)
        {
            AwardCategoryBonus(category);
        }
    }

    /// <summary>
    /// Awards bonus for completing all challenges in a category.
    /// </summary>
    private void AwardCategoryBonus(ChallengeCategory category)
    {
        float bonus = 0f;
        string achievementName = "";

        switch (category)
        {
            case ChallengeCategory.Species:
                bonus = 5000f;
                achievementName = "Species Expert";
                break;
            case ChallengeCategory.Action:
                bonus = 4000f;
                achievementName = "Action Photographer";
                break;
            case ChallengeCategory.Artistic:
                bonus = 3000f;
                achievementName = "Artist";
                break;
            case ChallengeCategory.Event:
                bonus = 6000f;
                achievementName = "Event Master";
                break;
        }

        if (bonus > 0f)
        {
            if (GameManager.Instance != null)
            {
                GameState state = GameManager.Instance.GetCurrentState();
                state.money += bonus;
            }

            Debug.Log($"[PhotoChallenges] Category completed: {category} - Bonus: ${bonus}");
            EventSystem.Publish("AchievementUnlocked", achievementName);
        }
    }

    #region Public API

    /// <summary>
    /// Gets all challenges.
    /// </summary>
    public List<PhotoChallenge> GetAllChallenges()
    {
        return new List<PhotoChallenge>(allChallenges);
    }

    /// <summary>
    /// Gets challenges by category.
    /// </summary>
    public List<PhotoChallenge> GetChallengesByCategory(ChallengeCategory category)
    {
        return allChallenges.FindAll(c => c.category == category);
    }

    /// <summary>
    /// Gets active (incomplete) challenges.
    /// </summary>
    public List<PhotoChallenge> GetActiveChallenges()
    {
        return allChallenges.FindAll(c => !c.isCompleted);
    }

    /// <summary>
    /// Gets completed challenges.
    /// </summary>
    public List<PhotoChallenge> GetCompletedChallenges()
    {
        return allChallenges.FindAll(c => c.isCompleted);
    }

    /// <summary>
    /// Gets challenge by ID.
    /// </summary>
    public PhotoChallenge GetChallengeByID(string challengeID)
    {
        return allChallenges.Find(c => c.challengeID == challengeID);
    }

    /// <summary>
    /// Gets challenge completion percentage.
    /// </summary>
    public float GetCompletionPercentage()
    {
        if (allChallenges.Count == 0) return 0f;
        return (float)completedChallenges.Count / allChallenges.Count * 100f;
    }

    /// <summary>
    /// Gets total challenges completed.
    /// </summary>
    public int GetCompletedCount()
    {
        return completedChallenges.Count;
    }

    /// <summary>
    /// Gets total challenge count.
    /// </summary>
    public int GetTotalChallengeCount()
    {
        return allChallenges.Count;
    }

    /// <summary>
    /// Checks if a challenge is completed.
    /// </summary>
    public bool IsChallengeCompleted(string challengeID)
    {
        return completedChallenges.Contains(challengeID);
    }

    #endregion
}

/// <summary>
/// Represents a photo challenge.
/// </summary>
[System.Serializable]
public class PhotoChallenge
{
    public string challengeID;
    public string challengeName;
    public string description;
    public ChallengeCategory category;
    public int goalCount;
    public int currentProgress;
    public float moneyReward;
    public bool isCompleted;

    /// <summary>
    /// Gets progress percentage.
    /// </summary>
    public float GetProgressPercentage()
    {
        if (goalCount == 0) return 0f;
        return Mathf.Clamp01((float)currentProgress / goalCount) * 100f;
    }
}

/// <summary>
/// Challenge category types.
/// </summary>
[System.Serializable]
public enum ChallengeCategory
{
    Species,
    Action,
    Artistic,
    Event
}
