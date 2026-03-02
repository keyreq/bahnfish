using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Agent 12: Audio System Specialist - UIAudioController.cs
/// Manages all UI audio including button clicks, hovers, notifications, and menu transitions.
/// Provides easy integration with Unity UI components.
/// </summary>
public class UIAudioController : MonoBehaviour
{
    #region Inspector Settings
    [Header("UI Sound Effects")]
    [Tooltip("Button hover sound")]
    [SerializeField] private string buttonHoverSound = "button_hover";

    [Tooltip("Button click sound")]
    [SerializeField] private string buttonClickSound = "button_click";

    [Tooltip("Button disabled sound")]
    [SerializeField] private string buttonDisabledSound = "button_disabled";

    [Tooltip("Menu open sound")]
    [SerializeField] private string menuOpenSound = "menu_open";

    [Tooltip("Menu close sound")]
    [SerializeField] private string menuCloseSound = "menu_close";

    [Tooltip("Menu transition sound")]
    [SerializeField] private string menuTransitionSound = "menu_transition";

    [Tooltip("Notification sound")]
    [SerializeField] private string notificationSound = "notification_info";

    [Tooltip("Warning notification sound")]
    [SerializeField] private string warningSound = "notification_warning";

    [Tooltip("Error notification sound")]
    [SerializeField] private string errorSound = "notification_error";

    [Tooltip("Achievement unlocked sound")]
    [SerializeField] private string achievementSound = "achievement_fanfare";

    [Tooltip("Slider drag sound")]
    [SerializeField] private string sliderDragSound = "slider_drag";

    [Tooltip("Tab switch sound")]
    [SerializeField] private string tabSwitchSound = "tab_switch";

    [Header("Auto-Attach to Buttons")]
    [Tooltip("Automatically attach audio to all buttons in scene")]
    [SerializeField] private bool autoAttachToButtons = true;

    [Tooltip("Automatically attach audio to all sliders in scene")]
    [SerializeField] private bool autoAttachToSliders = true;

    [Header("Volume Preview")]
    [Tooltip("Enable volume preview when adjusting volume sliders")]
    [SerializeField] private bool enableVolumePreview = true;

    [Tooltip("Preview sound for volume testing")]
    [SerializeField] private string volumePreviewSound = "button_click";

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = false;
    #endregion

    #region Private Variables
    private AudioManager audioManager;
    private SoundEffectManager sfxManager;

    // Volume preview cooldown
    private float lastVolumePreviewTime = 0f;
    private const float volumePreviewCooldown = 0.2f;
    #endregion

    #region Initialization
    private void Start()
    {
        audioManager = AudioManager.Instance;
        sfxManager = FindObjectOfType<SoundEffectManager>();

        // Subscribe to UI events
        SubscribeToEvents();

        // Auto-attach to UI elements
        if (autoAttachToButtons)
        {
            AttachToAllButtons();
        }

        if (autoAttachToSliders)
        {
            AttachToAllSliders();
        }

        if (enableDebugLogging)
        {
            Debug.Log("[UIAudioController] Initialized");
        }
    }

    /// <summary>
    /// Subscribes to UI-related game events.
    /// </summary>
    private void SubscribeToEvents()
    {
        EventSystem.Subscribe<string>("ShowNotification", OnNotification);
        EventSystem.Subscribe<string>("ShowWarning", OnWarning);
        EventSystem.Subscribe<string>("ShowError", OnError);
        EventSystem.Subscribe<string>("AchievementUnlocked", OnAchievementUnlocked);
        EventSystem.Subscribe("MenuOpened", OnMenuOpened);
        EventSystem.Subscribe("MenuClosed", OnMenuClosed);
        EventSystem.Subscribe<string>("TabSwitched", OnTabSwitched);
    }
    #endregion

    #region Auto-Attach to UI Elements
    /// <summary>
    /// Automatically attaches audio to all buttons in the scene.
    /// </summary>
    private void AttachToAllButtons()
    {
        Button[] buttons = FindObjectsOfType<Button>(true);

        foreach (Button button in buttons)
        {
            // Check if already has UIButtonAudio component
            if (button.GetComponent<UIButtonAudio>() == null)
            {
                UIButtonAudio buttonAudio = button.gameObject.AddComponent<UIButtonAudio>();
                buttonAudio.Initialize(this);
            }
        }

        if (enableDebugLogging)
        {
            Debug.Log($"[UIAudioController] Attached audio to {buttons.Length} buttons");
        }
    }

    /// <summary>
    /// Automatically attaches audio to all sliders in the scene.
    /// </summary>
    private void AttachToAllSliders()
    {
        Slider[] sliders = FindObjectsOfType<Slider>(true);

        foreach (Slider slider in sliders)
        {
            // Check if already has UISliderAudio component
            if (slider.GetComponent<UISliderAudio>() == null)
            {
                UISliderAudio sliderAudio = slider.gameObject.AddComponent<UISliderAudio>();
                sliderAudio.Initialize(this);
            }
        }

        if (enableDebugLogging)
        {
            Debug.Log($"[UIAudioController] Attached audio to {sliders.Length} sliders");
        }
    }
    #endregion

    #region Sound Playback
    /// <summary>
    /// Plays a UI sound effect.
    /// </summary>
    public void PlayUISound(string soundID)
    {
        if (sfxManager != null)
        {
            sfxManager.PlaySound2D(soundID);
        }

        if (enableDebugLogging)
        {
            Debug.Log($"[UIAudioController] Playing UI sound: {soundID}");
        }
    }

    /// <summary>
    /// Plays button hover sound.
    /// </summary>
    public void PlayButtonHover()
    {
        PlayUISound(buttonHoverSound);
    }

    /// <summary>
    /// Plays button click sound.
    /// </summary>
    public void PlayButtonClick()
    {
        PlayUISound(buttonClickSound);
    }

    /// <summary>
    /// Plays button disabled sound.
    /// </summary>
    public void PlayButtonDisabled()
    {
        PlayUISound(buttonDisabledSound);
    }

    /// <summary>
    /// Plays menu open sound.
    /// </summary>
    public void PlayMenuOpen()
    {
        PlayUISound(menuOpenSound);
    }

    /// <summary>
    /// Plays menu close sound.
    /// </summary>
    public void PlayMenuClose()
    {
        PlayUISound(menuCloseSound);
    }

    /// <summary>
    /// Plays menu transition sound.
    /// </summary>
    public void PlayMenuTransition()
    {
        PlayUISound(menuTransitionSound);
    }

    /// <summary>
    /// Plays notification sound.
    /// </summary>
    public void PlayNotification()
    {
        PlayUISound(notificationSound);
    }

    /// <summary>
    /// Plays warning sound.
    /// </summary>
    public void PlayWarning()
    {
        PlayUISound(warningSound);
    }

    /// <summary>
    /// Plays error sound.
    /// </summary>
    public void PlayError()
    {
        PlayUISound(errorSound);
    }

    /// <summary>
    /// Plays achievement sound.
    /// </summary>
    public void PlayAchievement()
    {
        PlayUISound(achievementSound);
    }

    /// <summary>
    /// Plays slider drag sound.
    /// </summary>
    public void PlaySliderDrag()
    {
        PlayUISound(sliderDragSound);
    }

    /// <summary>
    /// Plays tab switch sound.
    /// </summary>
    public void PlayTabSwitch()
    {
        PlayUISound(tabSwitchSound);
    }

    /// <summary>
    /// Plays volume preview sound (with cooldown).
    /// </summary>
    public void PlayVolumePreview()
    {
        if (!enableVolumePreview) return;

        if (Time.time - lastVolumePreviewTime >= volumePreviewCooldown)
        {
            PlayUISound(volumePreviewSound);
            lastVolumePreviewTime = Time.time;
        }
    }
    #endregion

    #region Event Handlers
    private void OnNotification(string message)
    {
        PlayNotification();
    }

    private void OnWarning(string message)
    {
        PlayWarning();
    }

    private void OnError(string message)
    {
        PlayError();
    }

    private void OnAchievementUnlocked(string achievementID)
    {
        PlayAchievement();
    }

    private void OnMenuOpened()
    {
        PlayMenuOpen();
    }

    private void OnMenuClosed()
    {
        PlayMenuClose();
    }

    private void OnTabSwitched(string tabName)
    {
        PlayTabSwitch();
    }
    #endregion

    #region Cleanup
    private void OnDestroy()
    {
        EventSystem.Unsubscribe<string>("ShowNotification", OnNotification);
        EventSystem.Unsubscribe<string>("ShowWarning", OnWarning);
        EventSystem.Unsubscribe<string>("ShowError", OnError);
        EventSystem.Unsubscribe<string>("AchievementUnlocked", OnAchievementUnlocked);
        EventSystem.Unsubscribe("MenuOpened", OnMenuOpened);
        EventSystem.Unsubscribe("MenuClosed", OnMenuClosed);
        EventSystem.Unsubscribe<string>("TabSwitched", OnTabSwitched);
    }
    #endregion
}

/// <summary>
/// Component attached to UI buttons for audio feedback.
/// </summary>
public class UIButtonAudio : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    private UIAudioController audioController;
    private Button button;

    public void Initialize(UIAudioController controller)
    {
        audioController = controller;
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && button.interactable && audioController != null)
        {
            audioController.PlayButtonHover();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (audioController == null) return;

        if (button != null && button.interactable)
        {
            audioController.PlayButtonClick();
        }
        else
        {
            audioController.PlayButtonDisabled();
        }
    }
}

/// <summary>
/// Component attached to UI sliders for audio feedback.
/// </summary>
public class UISliderAudio : MonoBehaviour
{
    private UIAudioController audioController;
    private Slider slider;
    private bool isDragging = false;

    public void Initialize(UIAudioController controller)
    {
        audioController = controller;
        slider = GetComponent<Slider>();

        if (slider != null)
        {
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }

    private void OnSliderValueChanged(float value)
    {
        // Check if this is a volume slider (by name convention)
        if (slider.name.ToLower().Contains("volume") && audioController != null)
        {
            audioController.PlayVolumePreview();
        }
    }

    private void OnDestroy()
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
    }
}
