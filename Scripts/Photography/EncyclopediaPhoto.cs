using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Agent 18: Photography Mode Specialist - EncyclopediaPhoto.cs
/// Manages fish encyclopedia photo system with quality rating and completion tracking.
/// Players must photograph all 60 fish species with minimum 3-star quality.
/// </summary>
public class EncyclopediaPhoto : MonoBehaviour
{
    public static EncyclopediaPhoto Instance { get; private set; }

    [Header("Encyclopedia Settings")]
    [SerializeField] private int totalFishSpecies = 60;
    [SerializeField] private float minQualityForEncyclopedia = 3f;
    [SerializeField] private float minFishCoveragePercent = 0.30f; // 30% of frame

    [Header("Encyclopedia Data")]
    [SerializeField] private Dictionary<string, EncyclopediaPhotoEntry> encyclopediaPhotos = new Dictionary<string, EncyclopediaPhotoEntry>();

    [Header("Statistics")]
    [SerializeField] private int totalPhotosTaken = 0;
    [SerializeField] private float averagePhotoQuality = 0f;
    [SerializeField] private int perfectShots = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Subscribe to photo events
        EventSystem.Subscribe<PhotoMetadata>("PhotoSaved", OnPhotoSaved);
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<PhotoMetadata>("PhotoSaved", OnPhotoSaved);
    }

    /// <summary>
    /// Called when a photo is saved.
    /// </summary>
    private void OnPhotoSaved(PhotoMetadata photo)
    {
        totalPhotosTaken++;

        // Only process if photo contains a fish
        if (string.IsNullOrEmpty(photo.fishSpeciesID))
        {
            return;
        }

        // Rate the photo quality
        float quality = RatePhotoQuality(photo);
        photo.qualityRating = quality;

        // Update statistics
        UpdateAverageQuality(quality);

        if (quality >= 5f)
        {
            perfectShots++;
        }

        // Check if photo qualifies for encyclopedia
        if (quality >= minQualityForEncyclopedia)
        {
            AddOrUpdateEncyclopediaPhoto(photo);
        }

        // Check for completion milestones
        CheckCompletionMilestones();
    }

    /// <summary>
    /// Rates photo quality on a 1-5 star scale.
    /// </summary>
    private float RatePhotoQuality(PhotoMetadata photo)
    {
        float totalScore = 0f;
        float maxScore = 5f;

        // Factor 1: Composition (0-1 stars)
        float compositionScore = RateComposition(photo);
        totalScore += compositionScore;

        // Factor 2: Focus (0-1 stars)
        float focusScore = RateFocus(photo);
        totalScore += focusScore;

        // Factor 3: Lighting/Exposure (0-1 stars)
        float lightingScore = RateLighting(photo);
        totalScore += lightingScore;

        // Factor 4: Visibility/Coverage (0-1 stars)
        float visibilityScore = RateVisibility(photo);
        totalScore += visibilityScore;

        // Factor 5: Rarity Bonus (0-1 stars)
        float rarityBonus = GetRarityBonus(photo.fishSpeciesID);
        totalScore += rarityBonus;

        return Mathf.Clamp(totalScore, 1f, 5f);
    }

    /// <summary>
    /// Rates composition quality (rule of thirds, centering).
    /// </summary>
    private float RateComposition(PhotoMetadata photo)
    {
        if (FramingTools.Instance == null) return 0.5f;

        // Estimate fish position (simplified - would use actual fish screen position)
        Vector2 fishPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        // Check rule of thirds alignment
        float ruleOfThirdsScore = FramingTools.Instance.AnalyzeComposition(fishPosition);

        // Also check centering (some compositions prefer centered)
        float centeringScore = FramingTools.Instance.AnalyzeCentering(fishPosition);

        // Use the better of the two scores
        return Mathf.Max(ruleOfThirdsScore, centeringScore);
    }

    /// <summary>
    /// Rates focus quality.
    /// </summary>
    private float RateFocus(PhotoMetadata photo)
    {
        // In a real implementation, this would analyze image sharpness
        // For now, assume good focus if fish is detected
        return string.IsNullOrEmpty(photo.fishSpeciesID) ? 0.3f : 0.8f;
    }

    /// <summary>
    /// Rates lighting and exposure.
    /// </summary>
    private float RateLighting(PhotoMetadata photo)
    {
        // Check exposure settings
        float exposureScore = 1f - Mathf.Abs(photo.exposure) / 2f; // -2 to +2 range

        // Consider time of day
        float timeScore = 1f;
        if (photo.timeOfDay < 6f || photo.timeOfDay > 20f)
        {
            timeScore = 0.6f; // Night photos are harder
        }

        return (exposureScore + timeScore) / 2f;
    }

    /// <summary>
    /// Rates fish visibility and coverage.
    /// </summary>
    private float RateVisibility(PhotoMetadata photo)
    {
        // In real implementation, would calculate actual fish coverage
        // Assume decent coverage if fish was detected
        if (string.IsNullOrEmpty(photo.fishSpeciesID))
        {
            return 0f;
        }

        // Simplified: assume 40% coverage if fish detected
        float estimatedCoverage = 0.40f;

        if (estimatedCoverage >= minFishCoveragePercent)
        {
            return Mathf.Clamp01(estimatedCoverage);
        }

        return estimatedCoverage / minFishCoveragePercent;
    }

    /// <summary>
    /// Gets rarity bonus for fish species.
    /// </summary>
    private float GetRarityBonus(string fishSpeciesID)
    {
        if (string.IsNullOrEmpty(fishSpeciesID)) return 0f;

        FishSpeciesData fishData = FishDatabase.Instance?.GetFishByID(fishSpeciesID);
        if (fishData == null) return 0f;

        switch (fishData.rarity)
        {
            case FishRarity.Common:
                return 0.2f;
            case FishRarity.Uncommon:
                return 0.4f;
            case FishRarity.Rare:
                return 0.6f;
            case FishRarity.Legendary:
                return 1.0f;
            case FishRarity.Aberrant:
                return 0.8f;
            default:
                return 0.2f;
        }
    }

    /// <summary>
    /// Adds or updates encyclopedia photo entry.
    /// </summary>
    private void AddOrUpdateEncyclopediaPhoto(PhotoMetadata photo)
    {
        string fishID = photo.fishSpeciesID;

        // Check if we already have a photo for this species
        if (encyclopediaPhotos.ContainsKey(fishID))
        {
            EncyclopediaPhotoEntry existing = encyclopediaPhotos[fishID];

            // Only replace if new photo is better quality
            if (photo.qualityRating > existing.bestPhotoQuality)
            {
                existing.bestPhotoID = photo.photoID;
                existing.bestPhotoQuality = photo.qualityRating;
                existing.captureDate = photo.captureDate;

                Debug.Log($"[EncyclopediaPhoto] Updated encyclopedia photo for {fishID} with {photo.qualityRating:F1} star quality");

                EventSystem.Publish("EncyclopediaPhotoUpdated", fishID);
            }
        }
        else
        {
            // Add new entry
            EncyclopediaPhotoEntry entry = new EncyclopediaPhotoEntry
            {
                fishSpeciesID = fishID,
                bestPhotoID = photo.photoID,
                bestPhotoQuality = photo.qualityRating,
                captureDate = photo.captureDate
            };

            encyclopediaPhotos[fishID] = entry;

            Debug.Log($"[EncyclopediaPhoto] Added {fishID} to encyclopedia with {photo.qualityRating:F1} star quality");

            EventSystem.Publish("EncyclopediaPhotoAdded", fishID);
        }
    }

    /// <summary>
    /// Updates average photo quality statistic.
    /// </summary>
    private void UpdateAverageQuality(float newQuality)
    {
        averagePhotoQuality = ((averagePhotoQuality * (totalPhotosTaken - 1)) + newQuality) / totalPhotosTaken;
    }

    /// <summary>
    /// Checks and awards completion milestones.
    /// </summary>
    private void CheckCompletionMilestones()
    {
        int speciesPhotographed = encyclopediaPhotos.Count;

        // 10 species milestone
        if (speciesPhotographed == 10)
        {
            AwardMilestone("Amateur Photographer", 500f);
        }
        // 25 species milestone
        else if (speciesPhotographed == 25)
        {
            AwardMilestone("Aspiring Photographer", 2000f);
            UnlockFilter(FilterType.Polaroid);
        }
        // 40 species milestone
        else if (speciesPhotographed == 40)
        {
            AwardMilestone("Professional Photographer", 5000f);
        }
        // 60 species milestone (complete)
        else if (speciesPhotographed == 60)
        {
            AwardMilestone("Master Photographer", 15000f);
            UnlockGhostCompanion();
        }
    }

    /// <summary>
    /// Awards a milestone achievement.
    /// </summary>
    private void AwardMilestone(string achievementName, float moneyReward)
    {
        Debug.Log($"[EncyclopediaPhoto] Achievement unlocked: {achievementName}! Reward: ${moneyReward}");

        // Award money
        if (GameManager.Instance != null)
        {
            GameState state = GameManager.Instance.GetCurrentState();
            state.money += moneyReward;
        }

        // Publish achievement event
        EventSystem.Publish("AchievementUnlocked", achievementName);
    }

    /// <summary>
    /// Unlocks a photo filter.
    /// </summary>
    private void UnlockFilter(FilterType filter)
    {
        Debug.Log($"[EncyclopediaPhoto] Unlocked filter: {CameraEffects.GetFilterName(filter)}");
        EventSystem.Publish("FilterUnlocked", filter);
    }

    /// <summary>
    /// Unlocks the Ghost Companion.
    /// </summary>
    private void UnlockGhostCompanion()
    {
        Debug.Log("[EncyclopediaPhoto] Ghost Companion unlocked!");
        EventSystem.Publish("GhostCompanionUnlocked");
    }

    #region Public API

    /// <summary>
    /// Gets encyclopedia completion percentage.
    /// </summary>
    public float GetCompletionPercentage()
    {
        return (float)encyclopediaPhotos.Count / totalFishSpecies * 100f;
    }

    /// <summary>
    /// Gets number of species photographed.
    /// </summary>
    public int GetSpeciesPhotographed()
    {
        return encyclopediaPhotos.Count;
    }

    /// <summary>
    /// Checks if a fish species has been photographed.
    /// </summary>
    public bool HasPhotoForSpecies(string fishSpeciesID)
    {
        return encyclopediaPhotos.ContainsKey(fishSpeciesID);
    }

    /// <summary>
    /// Gets encyclopedia photo entry for a fish species.
    /// </summary>
    public EncyclopediaPhotoEntry GetEncyclopediaEntry(string fishSpeciesID)
    {
        return encyclopediaPhotos.ContainsKey(fishSpeciesID) ? encyclopediaPhotos[fishSpeciesID] : null;
    }

    /// <summary>
    /// Gets all encyclopedia entries.
    /// </summary>
    public Dictionary<string, EncyclopediaPhotoEntry> GetAllEntries()
    {
        return new Dictionary<string, EncyclopediaPhotoEntry>(encyclopediaPhotos);
    }

    /// <summary>
    /// Gets total photos taken.
    /// </summary>
    public int GetTotalPhotosTaken()
    {
        return totalPhotosTaken;
    }

    /// <summary>
    /// Gets average photo quality.
    /// </summary>
    public float GetAveragePhotoQuality()
    {
        return averagePhotoQuality;
    }

    /// <summary>
    /// Gets perfect shot count.
    /// </summary>
    public int GetPerfectShotCount()
    {
        return perfectShots;
    }

    /// <summary>
    /// Gets missing species (not yet photographed).
    /// </summary>
    public List<string> GetMissingSpecies()
    {
        List<string> missing = new List<string>();

        if (FishDatabase.Instance != null)
        {
            List<FishSpeciesData> allFish = FishDatabase.Instance.GetAllFish();
            foreach (FishSpeciesData fish in allFish)
            {
                if (!encyclopediaPhotos.ContainsKey(fish.fishID))
                {
                    missing.Add(fish.fishID);
                }
            }
        }

        return missing;
    }

    /// <summary>
    /// Gets photographer statistics.
    /// </summary>
    public PhotographerStats GetStatistics()
    {
        return new PhotographerStats
        {
            totalPhotosTaken = totalPhotosTaken,
            speciesPhotographed = encyclopediaPhotos.Count,
            completionPercentage = GetCompletionPercentage(),
            averageQuality = averagePhotoQuality,
            perfectShots = perfectShots
        };
    }

    #endregion
}

/// <summary>
/// Encyclopedia photo entry for a fish species.
/// </summary>
[System.Serializable]
public class EncyclopediaPhotoEntry
{
    public string fishSpeciesID;
    public string bestPhotoID;
    public float bestPhotoQuality;
    public System.DateTime captureDate;
}

/// <summary>
/// Photographer statistics.
/// </summary>
[System.Serializable]
public class PhotographerStats
{
    public int totalPhotosTaken;
    public int speciesPhotographed;
    public float completionPercentage;
    public float averageQuality;
    public int perfectShots;
}
