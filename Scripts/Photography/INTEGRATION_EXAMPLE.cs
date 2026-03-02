using UnityEngine;

/// <summary>
/// Agent 18: Photography Mode Specialist - Integration Example
/// Demonstrates how to integrate the photography system into your game.
/// This file shows common use cases and best practices.
/// </summary>
public class INTEGRATION_EXAMPLE : MonoBehaviour
{
    // ========================================
    // EXAMPLE 1: Basic Photo Mode Setup
    // ========================================

    /// <summary>
    /// Basic setup - just enter photo mode when player presses P.
    /// </summary>
    private void Example1_BasicPhotoMode()
    {
        // In your game controller Update method:
        if (Input.GetKeyDown(KeyCode.P))
        {
            PhotoModeController.Instance.TogglePhotoMode();
        }
    }

    // ========================================
    // EXAMPLE 2: Apply Filters
    // ========================================

    /// <summary>
    /// Demonstrates how to apply and manage photo filters.
    /// </summary>
    private void Example2_ApplyFilters()
    {
        // Apply a single filter
        CameraEffects.Instance.ApplyFilter(FilterType.Sepia, 0.8f);

        // Stack multiple filters
        CameraEffects.Instance.ApplyFilter(FilterType.Vintage, 0.5f);
        CameraEffects.Instance.ApplyFilter(FilterType.Vignette, 0.6f);

        // Adjust filter intensity
        CameraEffects.Instance.ApplyFilter(FilterType.HDR, 0.3f);

        // Remove a specific filter
        CameraEffects.Instance.RemoveFilter(FilterType.Sepia);

        // Clear all filters
        CameraEffects.Instance.ClearAllFilters();

        // Get active filters
        var activeFilters = CameraEffects.Instance.GetActiveFilters();
        foreach (var filter in activeFilters)
        {
            Debug.Log($"Active: {filter.type} at {filter.intensity * 100}%");
        }
    }

    // ========================================
    // EXAMPLE 3: Capture Photo Manually
    // ========================================

    /// <summary>
    /// Demonstrates manual photo capture without photo mode.
    /// </summary>
    private void Example3_CapturePhoto()
    {
        Camera mainCamera = Camera.main;

        // Capture photo at default resolution
        PhotoMetadata photo = PhotoStorage.Instance.CapturePhoto(mainCamera);

        // Capture at specific resolution
        PhotoMetadata highResPhoto = PhotoStorage.Instance.CapturePhotoAtResolution(
            mainCamera,
            ExportResolution.UHD_4K
        );

        // Check quality
        if (photo.qualityRating >= 5f)
        {
            Debug.Log("Perfect shot!");
        }
    }

    // ========================================
    // EXAMPLE 4: Check Encyclopedia Progress
    // ========================================

    /// <summary>
    /// Demonstrates checking encyclopedia completion.
    /// </summary>
    private void Example4_CheckEncyclopedia()
    {
        // Get completion percentage
        float completion = EncyclopediaPhoto.Instance.GetCompletionPercentage();
        Debug.Log($"Encyclopedia: {completion}% complete");

        // Check specific species
        bool hasBluegillPhoto = EncyclopediaPhoto.Instance.HasPhotoForSpecies("bluegill");

        // Get entry details
        if (hasBluegillPhoto)
        {
            EncyclopediaPhotoEntry entry = EncyclopediaPhoto.Instance.GetEncyclopediaEntry("bluegill");
            Debug.Log($"Bluegill photo quality: {entry.bestPhotoQuality} stars");
        }

        // Get statistics
        PhotographerStats stats = EncyclopediaPhoto.Instance.GetStatistics();
        Debug.Log($"Species Photographed: {stats.speciesPhotographed}/60");
        Debug.Log($"Total Photos: {stats.totalPhotosTaken}");
        Debug.Log($"Average Quality: {stats.averageQuality} stars");
        Debug.Log($"Perfect Shots: {stats.perfectShots}");

        // Get missing species
        var missing = EncyclopediaPhoto.Instance.GetMissingSpecies();
        Debug.Log($"Still need to photograph: {string.Join(", ", missing)}");
    }

    // ========================================
    // EXAMPLE 5: Photo Challenges
    // ========================================

    /// <summary>
    /// Demonstrates working with photo challenges.
    /// </summary>
    private void Example5_PhotoChallenges()
    {
        // Get all challenges
        var allChallenges = PhotoChallenges.Instance.GetAllChallenges();

        // Get active (incomplete) challenges
        var activeChallenges = PhotoChallenges.Instance.GetActiveChallenges();

        // Get challenges by category
        var speciesChallenges = PhotoChallenges.Instance.GetChallengesByCategory(ChallengeCategory.Species);

        // Check specific challenge
        PhotoChallenge challenge = PhotoChallenges.Instance.GetChallengeByID("species_rare_1");
        if (challenge != null)
        {
            Debug.Log($"{challenge.challengeName}: {challenge.currentProgress}/{challenge.goalCount}");
            Debug.Log($"Progress: {challenge.GetProgressPercentage()}%");
        }

        // Get overall completion
        float completion = PhotoChallenges.Instance.GetCompletionPercentage();
        int completed = PhotoChallenges.Instance.GetCompletedCount();
        int total = PhotoChallenges.Instance.GetTotalChallengeCount();
        Debug.Log($"Challenges: {completed}/{total} ({completion}%)");
    }

    // ========================================
    // EXAMPLE 6: Photo Gallery Management
    // ========================================

    /// <summary>
    /// Demonstrates managing the photo gallery.
    /// </summary>
    private void Example6_PhotoGallery()
    {
        // Get all photos
        var allPhotos = PhotoStorage.Instance.GetPhotoGallery();
        Debug.Log($"Total photos in gallery: {allPhotos.Count}");

        // Get photos with fish
        var fishPhotos = PhotoStorage.Instance.GetPhotosWithFish();

        // Get photos of specific species
        var bluegillPhotos = PhotoStorage.Instance.GetPhotosByFishSpecies("bluegill");

        // Get specific photo
        PhotoMetadata photo = PhotoStorage.Instance.GetPhotoByID("photo_000042");

        // Delete photo
        PhotoStorage.Instance.DeletePhoto("photo_000042");

        // Set export preferences
        PhotoStorage.Instance.SetDefaultResolution(ExportResolution.QHD_1440p);
        PhotoStorage.Instance.SetDefaultFormat(ExportFormat.PNG);
    }

    // ========================================
    // EXAMPLE 7: Share Photos
    // ========================================

    /// <summary>
    /// Demonstrates photo sharing features.
    /// </summary>
    private void Example7_SharePhotos()
    {
        PhotoMetadata photo = PhotoStorage.Instance.GetPhotoByID("photo_000001");

        // Prepare photo for sharing (with watermark)
        string sharePath = ShareSystem.Instance.PreparePhotoForSharing(photo, includeStats: true);

        // Customize watermark
        ShareSystem.Instance.SetWatermarkEnabled(true);
        ShareSystem.Instance.SetWatermarkText("Bahnfish - MyUsername");
        ShareSystem.Instance.SetWatermarkPosition(WatermarkPosition.BottomRight);

        // Copy to clipboard
        ShareSystem.Instance.CopyPhotoToClipboard(photo);

        // Generate permalink
        string permalink = ShareSystem.Instance.GeneratePermalink(photo);
        Debug.Log($"Share link: {permalink}");

        // Export with custom settings
        ShareExportSettings settings = new ShareExportSettings
        {
            includeWatermark = true,
            includeStats = true,
            resolution = ExportResolution.UHD_4K,
            format = ExportFormat.PNG
        };
        string exportPath = ShareSystem.Instance.ExportPhoto(photo, settings);
    }

    // ========================================
    // EXAMPLE 8: Event Handling
    // ========================================

    private void OnEnable()
    {
        // Subscribe to photography events
        EventSystem.Subscribe("PhotoModeEntered", OnPhotoModeEntered);
        EventSystem.Subscribe("PhotoModeExited", OnPhotoModeExited);
        EventSystem.Subscribe<PhotoMetadata>("PhotoSaved", OnPhotoSaved);
        EventSystem.Subscribe<string>("EncyclopediaPhotoAdded", OnEncyclopediaPhotoAdded);
        EventSystem.Subscribe<PhotoChallenge>("PhotoChallengeCompleted", OnChallengeCompleted);
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        EventSystem.Unsubscribe("PhotoModeEntered", OnPhotoModeEntered);
        EventSystem.Unsubscribe("PhotoModeExited", OnPhotoModeExited);
        EventSystem.Unsubscribe<PhotoMetadata>("PhotoSaved", OnPhotoSaved);
        EventSystem.Unsubscribe<string>("EncyclopediaPhotoAdded", OnEncyclopediaPhotoAdded);
        EventSystem.Unsubscribe<PhotoChallenge>("PhotoChallengeCompleted", OnChallengeCompleted);
    }

    private void OnPhotoModeEntered()
    {
        Debug.Log("Photo mode activated!");
        // Pause game logic, disable player movement, etc.
    }

    private void OnPhotoModeExited()
    {
        Debug.Log("Photo mode deactivated!");
        // Resume game logic
    }

    private void OnPhotoSaved(PhotoMetadata photo)
    {
        Debug.Log($"Photo saved! Quality: {photo.qualityRating} stars");

        if (photo.qualityRating >= 5f)
        {
            // Perfect shot! Play special effects
            Debug.Log("PERFECT SHOT!");
        }

        if (!string.IsNullOrEmpty(photo.fishSpeciesID))
        {
            FishSpeciesData fish = FishDatabase.Instance.GetFishByID(photo.fishSpeciesID);
            Debug.Log($"Photographed: {fish.fishName}");
        }
    }

    private void OnEncyclopediaPhotoAdded(string fishID)
    {
        FishSpeciesData fish = FishDatabase.Instance.GetFishByID(fishID);
        Debug.Log($"New encyclopedia entry: {fish.fishName}!");

        // Show UI notification
        // Award achievement if appropriate
    }

    private void OnChallengeCompleted(PhotoChallenge challenge)
    {
        Debug.Log($"Challenge completed: {challenge.challengeName}");
        Debug.Log($"Reward: ${challenge.moneyReward}");

        // Show completion UI
        // Play celebration sound/effects
    }

    // ========================================
    // EXAMPLE 9: Camera Settings
    // ========================================

    /// <summary>
    /// Demonstrates adjusting camera settings.
    /// </summary>
    private void Example9_CameraSettings()
    {
        // Adjust Field of View
        PhotoModeController.Instance.SetFOV(45f); // Narrower for portraits
        PhotoModeController.Instance.SetFOV(90f); // Wider for landscapes

        // Adjust exposure (brightness)
        PhotoModeController.Instance.SetExposure(1.5f); // Brighter
        PhotoModeController.Instance.SetExposure(-1.0f); // Darker

        // Adjust contrast
        PhotoModeController.Instance.SetContrast(1.5f); // More contrast

        // Adjust saturation
        PhotoModeController.Instance.SetSaturation(1.3f); // More vibrant
        PhotoModeController.Instance.SetSaturation(0.0f); // Grayscale

        // Adjust brightness
        PhotoModeController.Instance.SetBrightness(20f);

        // Add camera tilt (Dutch angle)
        PhotoModeController.Instance.SetTilt(15f);

        // Depth of field
        PhotoModeController.Instance.SetFocusDistance(10f);
        PhotoModeController.Instance.SetDepthOfFieldBlur(0.5f);

        // Get current settings
        float currentFOV = PhotoModeController.Instance.GetFOV();
        float currentExposure = PhotoModeController.Instance.GetExposure();
    }

    // ========================================
    // EXAMPLE 10: Framing Tools
    // ========================================

    /// <summary>
    /// Demonstrates using composition aids.
    /// </summary>
    private void Example10_FramingTools()
    {
        // Toggle composition overlays
        FramingTools.Instance.ToggleRuleOfThirds();
        FramingTools.Instance.ToggleGoldenRatio();
        FramingTools.Instance.ToggleCenterCrosshair();
        FramingTools.Instance.ToggleSafeFrames();
        FramingTools.Instance.ToggleFocusPoint();

        // Analyze composition
        Vector2 fishPosition = new Vector2(Screen.width * 0.66f, Screen.height * 0.33f);
        float compositionScore = FramingTools.Instance.AnalyzeComposition(fishPosition);
        Debug.Log($"Composition quality: {compositionScore * 100}%");

        // Check if following rule of thirds
        bool followsRuleOfThirds = FramingTools.Instance.IsFollowingRuleOfThirds(fishPosition);

        // Check centering
        float centeringScore = FramingTools.Instance.AnalyzeCentering(fishPosition);
    }

    // ========================================
    // EXAMPLE 11: Fish Instance Setup
    // ========================================

    /// <summary>
    /// Demonstrates setting up fish instances for photography.
    /// </summary>
    private void Example11_FishInstanceSetup()
    {
        // When spawning a fish, add FishInstance component
        GameObject fishObject = new GameObject("Fish");
        FishInstance fishInstance = fishObject.AddComponent<FishInstance>();

        // Load species data
        FishSpeciesData bluegill = FishDatabase.Instance.GetFishByID("bluegill");

        // Initialize fish
        fishInstance.Initialize(bluegill);

        // Later, check if fish is in camera view
        Camera photoCamera = PhotoModeController.Instance.GetPhotoCamera();
        if (fishInstance.IsVisibleInCamera(photoCamera))
        {
            float coverage = fishInstance.GetScreenCoverage(photoCamera);
            Debug.Log($"Fish occupies {coverage * 100}% of frame");

            Vector2 screenPos = fishInstance.GetScreenPosition(photoCamera);
            Debug.Log($"Fish screen position: {screenPos}");
        }
    }

    // ========================================
    // EXAMPLE 12: Save/Load Integration
    // ========================================

    /// <summary>
    /// Demonstrates saving and loading photography data.
    /// </summary>
    private void Example12_SaveLoad()
    {
        // SAVING
        SaveData saveData = new SaveData();

        // Initialize photography data
        saveData.photographyData = new PhotographyData
        {
            totalPhotosTaken = EncyclopediaPhoto.Instance.GetTotalPhotosTaken(),
            averagePhotoQuality = EncyclopediaPhoto.Instance.GetAveragePhotoQuality(),
            perfectShots = EncyclopediaPhoto.Instance.GetPerfectShotCount(),
            completedChallenges = new System.Collections.Generic.List<string>(
                PhotoChallenges.Instance.GetCompletedChallenges().ConvertAll(c => c.challengeID)
            )
        };

        // Add encyclopedia photos
        var entries = EncyclopediaPhoto.Instance.GetAllEntries();
        foreach (var entry in entries)
        {
            saveData.photographyData.encyclopediaPhotos[entry.Key] = entry.Value.bestPhotoID;
        }

        // Save photo gallery
        var gallery = PhotoStorage.Instance.GetPhotoGallery();
        foreach (var photo in gallery)
        {
            SavedPhoto savedPhoto = new SavedPhoto
            {
                photoID = photo.photoID,
                fileName = photo.fileName,
                filePath = photo.filePath,
                fishSpeciesID = photo.fishSpeciesID,
                qualityRating = photo.qualityRating,
                filtersApplied = photo.filtersApplied
            };
            saveData.photographyData.photoGallery.Add(savedPhoto);
        }

        // LOADING
        PhotographyData photoData = saveData.photographyData;
        // Restore photography state from photoData
        Debug.Log($"Loaded {photoData.totalPhotosTaken} photos");
        Debug.Log($"Encyclopedia: {photoData.encyclopediaPhotos.Count} species");
        Debug.Log($"Challenges: {photoData.completedChallenges.Count} completed");
    }

    // ========================================
    // EXAMPLE 13: UI Panel Control
    // ========================================

    /// <summary>
    /// Demonstrates controlling photo UI panels.
    /// </summary>
    private void Example13_UIControl()
    {
        // Toggle different panels
        PhotoUI.Instance.ToggleGallery();
        PhotoUI.Instance.ToggleChallenges();
        PhotoUI.Instance.ToggleEncyclopedia();
        PhotoUI.Instance.ToggleSettingsPanel();
        PhotoUI.Instance.ToggleControlsHelp();

        // Show specific panel by name
        PhotoUI.Instance.ShowPanel("gallery");
        PhotoUI.Instance.ShowPanel("challenges");
        PhotoUI.Instance.ShowPanel("encyclopedia");
    }

    // ========================================
    // EXAMPLE 14: Complete Workflow
    // ========================================

    /// <summary>
    /// Complete workflow from catching fish to taking encyclopedia photo.
    /// </summary>
    private void Example14_CompleteWorkflow()
    {
        // 1. Player catches a fish
        string fishID = "bluegill";
        Debug.Log($"Fish caught: {fishID}");

        // 2. Suggest taking a photo
        Debug.Log("Press P to enter photo mode and photograph your catch!");

        // 3. Player enters photo mode (done via input)
        // PhotoModeController.Instance.EnterPhotoMode();

        // 4. Player takes photo
        // Photo automatically processed

        // 5. Check if it counts for encyclopedia
        // This happens automatically in EncyclopediaPhoto.OnPhotoSaved()

        // 6. Check encyclopedia progress
        float completion = EncyclopediaPhoto.Instance.GetCompletionPercentage();
        if (completion >= 100f)
        {
            Debug.Log("Encyclopedia complete! Master Photographer achievement!");
        }
    }
}
