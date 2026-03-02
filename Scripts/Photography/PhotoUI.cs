using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Agent 18: Photography Mode Specialist - PhotoUI.cs
/// Manages all UI elements for photo mode including settings, gallery, and challenges.
/// Provides intuitive interface for photography features.
/// </summary>
public class PhotoUI : MonoBehaviour
{
    public static PhotoUI Instance { get; private set; }

    [Header("Main Panels")]
    [SerializeField] private GameObject photoModePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject galleryPanel;
    [SerializeField] private GameObject challengesPanel;
    [SerializeField] private GameObject encyclopediaPanel;

    [Header("Camera Settings UI")]
    [SerializeField] private Slider fovSlider;
    [SerializeField] private Slider exposureSlider;
    [SerializeField] private Slider contrastSlider;
    [SerializeField] private Slider saturationSlider;
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private Slider tiltSlider;
    [SerializeField] private Text fovValueText;
    [SerializeField] private Text exposureValueText;

    [Header("Filter UI")]
    [SerializeField] private GameObject filterListContainer;
    [SerializeField] private GameObject filterButtonPrefab;
    [SerializeField] private Slider filterIntensitySlider;
    [SerializeField] private Text currentFilterText;

    [Header("Gallery UI")]
    [SerializeField] private GameObject photoThumbnailPrefab;
    [SerializeField] private Transform galleryGridContainer;
    [SerializeField] private RawImage fullPhotoDisplay;
    [SerializeField] private Text photoInfoText;

    [Header("Challenges UI")]
    [SerializeField] private GameObject challengeItemPrefab;
    [SerializeField] private Transform challengeListContainer;
    [SerializeField] private Text challengeProgressText;

    [Header("Encyclopedia UI")]
    [SerializeField] private GameObject encyclopediaEntryPrefab;
    [SerializeField] private Transform encyclopediaGridContainer;
    [SerializeField] private Text encyclopediaStatsText;

    [Header("Controls Help")]
    [SerializeField] private GameObject controlsHelpPanel;
    [SerializeField] private Text controlsHelpText;

    private FilterType currentSelectedFilter = FilterType.Sepia;
    private PhotoMetadata currentlyViewedPhoto;

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
        // Subscribe to photo mode events
        EventSystem.Subscribe("PhotoModeEntered", OnPhotoModeEntered);
        EventSystem.Subscribe("PhotoModeExited", OnPhotoModeExited);
        EventSystem.Subscribe<PhotoMetadata>("PhotoSaved", OnPhotoSaved);

        // Initialize UI
        InitializeFilterList();
        InitializeCameraSettingsUI();
        HideAllPanels();
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe("PhotoModeEntered", OnPhotoModeEntered);
        EventSystem.Unsubscribe("PhotoModeExited", OnPhotoModeExited);
        EventSystem.Unsubscribe<PhotoMetadata>("PhotoSaved", OnPhotoSaved);
    }

    private void Update()
    {
        // Quick toggle help panel
        if (Input.GetKeyDown(KeyCode.H))
        {
            ToggleControlsHelp();
        }

        // Toggle settings panel
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleSettingsPanel();
        }

        // Toggle gallery
        if (Input.GetKeyDown(KeyCode.G))
        {
            ToggleGallery();
        }
    }

    #region Event Handlers

    private void OnPhotoModeEntered()
    {
        ShowPhotoModeUI();
    }

    private void OnPhotoModeExited()
    {
        HideAllPanels();
    }

    private void OnPhotoSaved(PhotoMetadata photo)
    {
        // Show brief notification
        ShowPhotoSavedNotification(photo);
    }

    #endregion

    #region UI Initialization

    /// <summary>
    /// Initializes camera settings UI.
    /// </summary>
    private void InitializeCameraSettingsUI()
    {
        if (fovSlider != null)
        {
            fovSlider.minValue = 30f;
            fovSlider.maxValue = 90f;
            fovSlider.value = 60f;
            fovSlider.onValueChanged.AddListener(OnFOVChanged);
        }

        if (exposureSlider != null)
        {
            exposureSlider.minValue = -2f;
            exposureSlider.maxValue = 2f;
            exposureSlider.value = 0f;
            exposureSlider.onValueChanged.AddListener(OnExposureChanged);
        }

        if (contrastSlider != null)
        {
            contrastSlider.minValue = 0f;
            contrastSlider.maxValue = 2f;
            contrastSlider.value = 1f;
            contrastSlider.onValueChanged.AddListener(OnContrastChanged);
        }

        if (saturationSlider != null)
        {
            saturationSlider.minValue = 0f;
            saturationSlider.maxValue = 2f;
            saturationSlider.value = 1f;
            saturationSlider.onValueChanged.AddListener(OnSaturationChanged);
        }

        if (brightnessSlider != null)
        {
            brightnessSlider.minValue = -50f;
            brightnessSlider.maxValue = 50f;
            brightnessSlider.value = 0f;
            brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
        }

        if (tiltSlider != null)
        {
            tiltSlider.minValue = -45f;
            tiltSlider.maxValue = 45f;
            tiltSlider.value = 0f;
            tiltSlider.onValueChanged.AddListener(OnTiltChanged);
        }

        if (filterIntensitySlider != null)
        {
            filterIntensitySlider.minValue = 0f;
            filterIntensitySlider.maxValue = 1f;
            filterIntensitySlider.value = 1f;
            filterIntensitySlider.onValueChanged.AddListener(OnFilterIntensityChanged);
        }
    }

    /// <summary>
    /// Initializes filter list UI.
    /// </summary>
    private void InitializeFilterList()
    {
        if (filterListContainer == null || filterButtonPrefab == null) return;

        // Clear existing buttons
        foreach (Transform child in filterListContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // Create button for each filter
        foreach (FilterType filterType in System.Enum.GetValues(typeof(FilterType)))
        {
            GameObject buttonObj = Instantiate(filterButtonPrefab, filterListContainer.transform);
            Button button = buttonObj.GetComponent<Button>();
            Text buttonText = buttonObj.GetComponentInChildren<Text>();

            if (buttonText != null)
            {
                buttonText.text = CameraEffects.GetFilterName(filterType);
            }

            if (button != null)
            {
                FilterType capturedFilter = filterType;
                button.onClick.AddListener(() => OnFilterSelected(capturedFilter));
            }
        }
    }

    #endregion

    #region Camera Settings Handlers

    private void OnFOVChanged(float value)
    {
        if (PhotoModeController.Instance != null)
        {
            PhotoModeController.Instance.SetFOV(value);
        }

        if (fovValueText != null)
        {
            fovValueText.text = $"{value:F0}°";
        }
    }

    private void OnExposureChanged(float value)
    {
        if (PhotoModeController.Instance != null)
        {
            PhotoModeController.Instance.SetExposure(value);
        }

        if (exposureValueText != null)
        {
            exposureValueText.text = $"{value:F1}";
        }
    }

    private void OnContrastChanged(float value)
    {
        if (PhotoModeController.Instance != null)
        {
            PhotoModeController.Instance.SetContrast(value);
        }
    }

    private void OnSaturationChanged(float value)
    {
        if (PhotoModeController.Instance != null)
        {
            PhotoModeController.Instance.SetSaturation(value);
        }
    }

    private void OnBrightnessChanged(float value)
    {
        if (PhotoModeController.Instance != null)
        {
            PhotoModeController.Instance.SetBrightness(value);
        }
    }

    private void OnTiltChanged(float value)
    {
        if (PhotoModeController.Instance != null)
        {
            PhotoModeController.Instance.SetTilt(value);
        }
    }

    #endregion

    #region Filter Handlers

    private void OnFilterSelected(FilterType filterType)
    {
        currentSelectedFilter = filterType;

        if (currentFilterText != null)
        {
            currentFilterText.text = $"Filter: {CameraEffects.GetFilterName(filterType)}";
        }

        // Apply filter with current intensity
        if (CameraEffects.Instance != null && filterIntensitySlider != null)
        {
            CameraEffects.Instance.ApplyFilter(filterType, filterIntensitySlider.value);
        }
    }

    private void OnFilterIntensityChanged(float value)
    {
        if (CameraEffects.Instance != null)
        {
            CameraEffects.Instance.ApplyFilter(currentSelectedFilter, value);
        }
    }

    #endregion

    #region Panel Management

    /// <summary>
    /// Shows photo mode UI.
    /// </summary>
    private void ShowPhotoModeUI()
    {
        if (photoModePanel != null)
        {
            photoModePanel.SetActive(true);
        }

        ShowControlsHelp();
    }

    /// <summary>
    /// Hides all UI panels.
    /// </summary>
    private void HideAllPanels()
    {
        if (photoModePanel != null) photoModePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (galleryPanel != null) galleryPanel.SetActive(false);
        if (challengesPanel != null) challengesPanel.SetActive(false);
        if (encyclopediaPanel != null) encyclopediaPanel.SetActive(false);
        if (controlsHelpPanel != null) controlsHelpPanel.SetActive(false);
    }

    /// <summary>
    /// Toggles settings panel.
    /// </summary>
    public void ToggleSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
        }
    }

    /// <summary>
    /// Toggles gallery panel.
    /// </summary>
    public void ToggleGallery()
    {
        if (galleryPanel != null)
        {
            bool newState = !galleryPanel.activeSelf;
            galleryPanel.SetActive(newState);

            if (newState)
            {
                RefreshGallery();
            }
        }
    }

    /// <summary>
    /// Toggles challenges panel.
    /// </summary>
    public void ToggleChallenges()
    {
        if (challengesPanel != null)
        {
            bool newState = !challengesPanel.activeSelf;
            challengesPanel.SetActive(newState);

            if (newState)
            {
                RefreshChallenges();
            }
        }
    }

    /// <summary>
    /// Toggles encyclopedia panel.
    /// </summary>
    public void ToggleEncyclopedia()
    {
        if (encyclopediaPanel != null)
        {
            bool newState = !encyclopediaPanel.activeSelf;
            encyclopediaPanel.SetActive(newState);

            if (newState)
            {
                RefreshEncyclopedia();
            }
        }
    }

    /// <summary>
    /// Shows controls help.
    /// </summary>
    private void ShowControlsHelp()
    {
        if (controlsHelpPanel != null)
        {
            controlsHelpPanel.SetActive(true);
            UpdateControlsHelpText();
        }
    }

    /// <summary>
    /// Toggles controls help.
    /// </summary>
    public void ToggleControlsHelp()
    {
        if (controlsHelpPanel != null)
        {
            controlsHelpPanel.SetActive(!controlsHelpPanel.activeSelf);
        }
    }

    /// <summary>
    /// Updates controls help text.
    /// </summary>
    private void UpdateControlsHelpText()
    {
        if (controlsHelpText == null) return;

        controlsHelpText.text = @"PHOTO MODE CONTROLS

WASD - Move Camera
Q/E - Move Up/Down
Shift - Sprint (2x speed)
Ctrl - Slow (0.5x speed)
Right Mouse - Look Around
Scroll - Zoom (FOV)

SPACE - Take Photo
TAB - Settings
G - Gallery
H - Toggle Help
R - Reset Camera
P - Exit Photo Mode";
    }

    #endregion

    #region Gallery Management

    /// <summary>
    /// Refreshes gallery display.
    /// </summary>
    private void RefreshGallery()
    {
        if (PhotoStorage.Instance == null || galleryGridContainer == null) return;

        // Clear existing thumbnails
        foreach (Transform child in galleryGridContainer)
        {
            Destroy(child.gameObject);
        }

        // Get all photos
        List<PhotoMetadata> photos = PhotoStorage.Instance.GetPhotoGallery();

        // Create thumbnail for each photo
        foreach (PhotoMetadata photo in photos)
        {
            CreatePhotoThumbnail(photo);
        }
    }

    /// <summary>
    /// Creates a thumbnail for a photo.
    /// </summary>
    private void CreatePhotoThumbnail(PhotoMetadata photo)
    {
        if (photoThumbnailPrefab == null || galleryGridContainer == null) return;

        GameObject thumbnailObj = Instantiate(photoThumbnailPrefab, galleryGridContainer);
        Button thumbnailButton = thumbnailObj.GetComponent<Button>();

        if (thumbnailButton != null)
        {
            thumbnailButton.onClick.AddListener(() => ViewPhoto(photo));
        }

        // In a real implementation, would load and display actual thumbnail
        // For now, just setup the click handler
    }

    /// <summary>
    /// Views a photo in full size.
    /// </summary>
    private void ViewPhoto(PhotoMetadata photo)
    {
        currentlyViewedPhoto = photo;

        // Display photo info
        if (photoInfoText != null)
        {
            photoInfoText.text = GetPhotoInfoText(photo);
        }

        // In a real implementation, would load and display the actual photo
        Debug.Log($"[PhotoUI] Viewing photo: {photo.photoID}");
    }

    /// <summary>
    /// Gets photo info text for display.
    /// </summary>
    private string GetPhotoInfoText(PhotoMetadata photo)
    {
        string info = $"Photo ID: {photo.photoID}\n";
        info += $"Date: {photo.captureDate:yyyy-MM-dd HH:mm:ss}\n";
        info += $"Resolution: {photo.resolution.x}x{photo.resolution.y}\n";

        if (!string.IsNullOrEmpty(photo.fishSpeciesID))
        {
            FishSpeciesData fishData = FishDatabase.Instance?.GetFishByID(photo.fishSpeciesID);
            if (fishData != null)
            {
                info += $"\nFish: {fishData.fishName}\n";
                info += $"Rarity: {fishData.rarity}\n";
            }
        }

        info += $"\nLocation: {photo.locationID}\n";
        info += $"Time: {photo.timeOfDay:F1}\n";
        info += $"Weather: {photo.weatherType}\n";

        if (photo.qualityRating > 0)
        {
            info += $"\nQuality: {photo.qualityRating:F1} stars\n";
        }

        if (photo.filtersApplied.Count > 0)
        {
            info += $"\nFilters: {string.Join(", ", photo.filtersApplied)}\n";
        }

        return info;
    }

    #endregion

    #region Challenges Display

    /// <summary>
    /// Refreshes challenges display.
    /// </summary>
    private void RefreshChallenges()
    {
        if (PhotoChallenges.Instance == null || challengeListContainer == null) return;

        // Clear existing items
        foreach (Transform child in challengeListContainer)
        {
            Destroy(child.gameObject);
        }

        // Get all challenges
        List<PhotoChallenge> challenges = PhotoChallenges.Instance.GetAllChallenges();

        // Create item for each challenge
        foreach (PhotoChallenge challenge in challenges)
        {
            CreateChallengeItem(challenge);
        }

        // Update progress text
        if (challengeProgressText != null)
        {
            int completed = PhotoChallenges.Instance.GetCompletedCount();
            int total = PhotoChallenges.Instance.GetTotalChallengeCount();
            challengeProgressText.text = $"Challenges: {completed}/{total} ({PhotoChallenges.Instance.GetCompletionPercentage():F1}%)";
        }
    }

    /// <summary>
    /// Creates a challenge list item.
    /// </summary>
    private void CreateChallengeItem(PhotoChallenge challenge)
    {
        if (challengeItemPrefab == null || challengeListContainer == null) return;

        GameObject itemObj = Instantiate(challengeItemPrefab, challengeListContainer);
        Text itemText = itemObj.GetComponentInChildren<Text>();

        if (itemText != null)
        {
            string status = challenge.isCompleted ? "[COMPLETE]" : $"[{challenge.currentProgress}/{challenge.goalCount}]";
            itemText.text = $"{status} {challenge.challengeName} - ${challenge.moneyReward}";
        }
    }

    #endregion

    #region Encyclopedia Display

    /// <summary>
    /// Refreshes encyclopedia display.
    /// </summary>
    private void RefreshEncyclopedia()
    {
        if (EncyclopediaPhoto.Instance == null || encyclopediaGridContainer == null) return;

        // Clear existing entries
        foreach (Transform child in encyclopediaGridContainer)
        {
            Destroy(child.gameObject);
        }

        // Get all fish species
        if (FishDatabase.Instance != null)
        {
            List<FishSpeciesData> allFish = FishDatabase.Instance.GetAllFish();

            foreach (FishSpeciesData fish in allFish)
            {
                CreateEncyclopediaEntry(fish);
            }
        }

        // Update stats text
        if (encyclopediaStatsText != null)
        {
            PhotographerStats stats = EncyclopediaPhoto.Instance.GetStatistics();
            encyclopediaStatsText.text = $@"Encyclopedia Progress
Photographed: {stats.speciesPhotographed}/60 ({stats.completionPercentage:F1}%)
Total Photos: {stats.totalPhotosTaken}
Average Quality: {stats.averageQuality:F1} stars
Perfect Shots: {stats.perfectShots}";
        }
    }

    /// <summary>
    /// Creates an encyclopedia entry.
    /// </summary>
    private void CreateEncyclopediaEntry(FishSpeciesData fish)
    {
        if (encyclopediaEntryPrefab == null || encyclopediaGridContainer == null) return;

        GameObject entryObj = Instantiate(encyclopediaEntryPrefab, encyclopediaGridContainer);
        Text entryText = entryObj.GetComponentInChildren<Text>();

        bool hasPhoto = EncyclopediaPhoto.Instance.HasPhotoForSpecies(fish.fishID);

        if (entryText != null)
        {
            if (hasPhoto)
            {
                EncyclopediaPhotoEntry entry = EncyclopediaPhoto.Instance.GetEncyclopediaEntry(fish.fishID);
                entryText.text = $"{fish.fishName}\n{entry.bestPhotoQuality:F1} stars";
            }
            else
            {
                entryText.text = $"{fish.fishName}\n[No Photo]";
            }
        }
    }

    #endregion

    #region Notifications

    /// <summary>
    /// Shows photo saved notification.
    /// </summary>
    private void ShowPhotoSavedNotification(PhotoMetadata photo)
    {
        string message = "Photo Saved!";

        if (photo.qualityRating >= 5f)
        {
            message += " (5 STARS - PERFECT!)";
        }
        else if (photo.qualityRating >= 3f)
        {
            message += $" ({photo.qualityRating:F1} stars)";
        }

        Debug.Log($"[PhotoUI] {message}");
        // In a real implementation, would show UI toast notification
    }

    #endregion

    #region Public API

    /// <summary>
    /// Shows a specific panel.
    /// </summary>
    public void ShowPanel(string panelName)
    {
        switch (panelName.ToLower())
        {
            case "settings":
                ToggleSettingsPanel();
                break;
            case "gallery":
                ToggleGallery();
                break;
            case "challenges":
                ToggleChallenges();
                break;
            case "encyclopedia":
                ToggleEncyclopedia();
                break;
            case "help":
                ToggleControlsHelp();
                break;
        }
    }

    #endregion
}
