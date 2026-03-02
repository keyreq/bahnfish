using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages toast-style notifications that slide in from the side.
/// Supports different notification types (info, warning, success, error).
/// Auto-dismisses after configurable duration.
/// Queues multiple notifications.
/// </summary>
public class NotificationManager : MonoBehaviour
{
    #region Singleton
    private static NotificationManager _instance;
    public static NotificationManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<NotificationManager>();
            }
            return _instance;
        }
    }
    #endregion

    #region Inspector References
    [Header("Notification Prefab")]
    [Tooltip("Prefab for notification UI element")]
    public GameObject notificationPrefab;

    [Tooltip("Parent transform for notification instances")]
    public Transform notificationContainer;

    [Header("Animation Settings")]
    [Tooltip("Slide in from direction")]
    public SlideDirection slideDirection = SlideDirection.Right;

    [Tooltip("Slide animation duration")]
    [Range(0.1f, 2f)]
    public float slideDuration = 0.5f;

    [Tooltip("Auto-dismiss duration")]
    [Range(1f, 10f)]
    public float displayDuration = 3f;

    [Tooltip("Fade out duration")]
    [Range(0.1f, 2f)]
    public float fadeOutDuration = 0.5f;

    [Header("Queue Settings")]
    [Tooltip("Maximum notifications displayed at once")]
    [Range(1, 10)]
    public int maxVisibleNotifications = 3;

    [Tooltip("Vertical spacing between notifications")]
    public float notificationSpacing = 10f;

    [Header("Colors")]
    [Tooltip("Background color for info notifications")]
    public Color infoColor = new Color(0.2f, 0.6f, 1f, 0.9f);

    [Tooltip("Background color for success notifications")]
    public Color successColor = new Color(0.2f, 1f, 0.2f, 0.9f);

    [Tooltip("Background color for warning notifications")]
    public Color warningColor = new Color(1f, 0.8f, 0f, 0.9f);

    [Tooltip("Background color for error notifications")]
    public Color errorColor = new Color(1f, 0.2f, 0.2f, 0.9f);

    [Header("Audio")]
    [Tooltip("Play sound on notification")]
    public bool playSounds = true;

    [Tooltip("Sound for info notifications")]
    public AudioClip infoSound;

    [Tooltip("Sound for success notifications")]
    public AudioClip successSound;

    [Tooltip("Sound for warning notifications")]
    public AudioClip warningSound;

    [Tooltip("Sound for error notifications")]
    public AudioClip errorSound;

    [Header("Debug")]
    [Tooltip("Enable debug logging")]
    public bool enableDebugLogging = false;
    #endregion

    #region Private Variables
    private Queue<NotificationData> notificationQueue = new Queue<NotificationData>();
    private List<GameObject> activeNotifications = new List<GameObject>();
    private AudioSource audioSource;
    #endregion

    #region Enums
    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }

    public enum SlideDirection
    {
        Left,
        Right,
        Top,
        Bottom
    }
    #endregion

    #region Data Structures
    private class NotificationData
    {
        public string message;
        public NotificationType type;
        public float customDuration;

        public NotificationData(string message, NotificationType type, float duration)
        {
            this.message = message;
            this.type = type;
            this.customDuration = duration;
        }
    }
    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        // Get or create audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && playSounds)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void Start()
    {
        // Subscribe to common notification events
        SubscribeToEvents();

        if (enableDebugLogging)
        {
            Debug.Log("[NotificationManager] Initialized");
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    #endregion

    #region Event Subscription

    /// <summary>
    /// Subscribe to common game events that trigger notifications
    /// </summary>
    private void SubscribeToEvents()
    {
        // These are example events - actual events will come from other agents
        EventSystem.Subscribe<string>("ShowNotification", (msg) => ShowNotification(msg));
        EventSystem.Subscribe<Fish>("FishCaught", OnFishCaught);
        // More subscriptions will be added as other agents are implemented
    }

    /// <summary>
    /// Unsubscribe from events
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        EventSystem.Unsubscribe<string>("ShowNotification", (msg) => ShowNotification(msg));
        EventSystem.Unsubscribe<Fish>("FishCaught", OnFishCaught);
    }

    /// <summary>
    /// Example event handler - fish caught
    /// </summary>
    private void OnFishCaught(Fish fish)
    {
        string message = $"Caught {fish.name}!";
        ShowNotification(message, NotificationType.Success);
    }

    #endregion

    #region Public Interface

    /// <summary>
    /// Show a notification with default settings
    /// </summary>
    public void ShowNotification(string message, NotificationType type = NotificationType.Info)
    {
        ShowNotification(message, type, displayDuration);
    }

    /// <summary>
    /// Show a notification with custom duration
    /// </summary>
    public void ShowNotification(string message, NotificationType type, float duration)
    {
        if (string.IsNullOrEmpty(message))
        {
            Debug.LogWarning("[NotificationManager] Attempted to show notification with empty message");
            return;
        }

        NotificationData data = new NotificationData(message, type, duration);
        notificationQueue.Enqueue(data);

        if (enableDebugLogging)
        {
            Debug.Log($"[NotificationManager] Queued notification: [{type}] {message}");
        }

        // Try to display immediately if there's room
        ProcessQueue();
    }

    /// <summary>
    /// Clear all active notifications
    /// </summary>
    public void ClearAllNotifications()
    {
        foreach (GameObject notification in activeNotifications)
        {
            if (notification != null)
            {
                Destroy(notification);
            }
        }
        activeNotifications.Clear();
        notificationQueue.Clear();

        if (enableDebugLogging)
        {
            Debug.Log("[NotificationManager] Cleared all notifications");
        }
    }

    #endregion

    #region Queue Processing

    /// <summary>
    /// Process the notification queue
    /// </summary>
    private void ProcessQueue()
    {
        // Remove null references
        activeNotifications.RemoveAll(n => n == null);

        // Display notifications if there's room
        while (notificationQueue.Count > 0 && activeNotifications.Count < maxVisibleNotifications)
        {
            NotificationData data = notificationQueue.Dequeue();
            DisplayNotification(data);
        }
    }

    /// <summary>
    /// Display a notification
    /// </summary>
    private void DisplayNotification(NotificationData data)
    {
        if (notificationPrefab == null)
        {
            Debug.LogError("[NotificationManager] Notification prefab is not assigned!");
            return;
        }

        if (notificationContainer == null)
        {
            Debug.LogError("[NotificationManager] Notification container is not assigned!");
            return;
        }

        // Instantiate notification
        GameObject notification = Instantiate(notificationPrefab, notificationContainer);
        activeNotifications.Add(notification);

        // Setup notification content
        SetupNotificationContent(notification, data);

        // Position notification
        RepositionNotifications();

        // Animate in
        StartCoroutine(AnimateNotification(notification, data));

        // Play sound
        PlayNotificationSound(data.type);

        if (enableDebugLogging)
        {
            Debug.Log($"[NotificationManager] Displayed notification: [{data.type}] {data.message}");
        }
    }

    /// <summary>
    /// Setup notification content (text, colors, icons)
    /// </summary>
    private void SetupNotificationContent(GameObject notification, NotificationData data)
    {
        // Find text component
        TextMeshProUGUI textComponent = notification.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = data.message;
        }

        // Find background image
        Image backgroundImage = notification.GetComponent<Image>();
        if (backgroundImage != null)
        {
            backgroundImage.color = GetColorForType(data.type);
        }

        // Find icon (optional)
        Transform iconTransform = notification.transform.Find("Icon");
        if (iconTransform != null)
        {
            Image iconImage = iconTransform.GetComponent<Image>();
            if (iconImage != null)
            {
                // Set icon based on type (icons would need to be assigned)
                iconImage.enabled = false; // Disable for now
            }
        }
    }

    /// <summary>
    /// Reposition all active notifications
    /// </summary>
    private void RepositionNotifications()
    {
        for (int i = 0; i < activeNotifications.Count; i++)
        {
            if (activeNotifications[i] != null)
            {
                RectTransform rectTransform = activeNotifications[i].GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    // Stack notifications vertically
                    float yOffset = -i * (rectTransform.rect.height + notificationSpacing);
                    rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, yOffset);
                }
            }
        }
    }

    #endregion

    #region Animation

    /// <summary>
    /// Animate notification in, wait, then fade out
    /// </summary>
    private IEnumerator AnimateNotification(GameObject notification, NotificationData data)
    {
        RectTransform rectTransform = notification.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = notification.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = notification.AddComponent<CanvasGroup>();
        }

        // Calculate slide positions
        Vector2 targetPosition = rectTransform.anchoredPosition;
        Vector2 startPosition = GetOffScreenPosition(rectTransform, targetPosition);

        // Start off-screen
        rectTransform.anchoredPosition = startPosition;
        canvasGroup.alpha = 1f;

        // Slide in
        yield return StartCoroutine(SlideIn(rectTransform, startPosition, targetPosition));

        // Wait for display duration
        yield return new WaitForSeconds(data.customDuration);

        // Fade out
        yield return StartCoroutine(FadeOut(canvasGroup));

        // Remove from active list and destroy
        activeNotifications.Remove(notification);
        Destroy(notification);

        // Process queue for next notification
        ProcessQueue();

        // Reposition remaining notifications
        RepositionNotifications();
    }

    /// <summary>
    /// Slide notification in from off-screen
    /// </summary>
    private IEnumerator SlideIn(RectTransform rectTransform, Vector2 from, Vector2 to)
    {
        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / slideDuration;
            t = Mathf.SmoothStep(0f, 1f, t); // Smooth easing
            rectTransform.anchoredPosition = Vector2.Lerp(from, to, t);
            yield return null;
        }
        rectTransform.anchoredPosition = to;
    }

    /// <summary>
    /// Fade notification out
    /// </summary>
    private IEnumerator FadeOut(CanvasGroup canvasGroup)
    {
        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = 1f - (elapsed / fadeOutDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }

    /// <summary>
    /// Get off-screen position based on slide direction
    /// </summary>
    private Vector2 GetOffScreenPosition(RectTransform rectTransform, Vector2 targetPosition)
    {
        Vector2 offScreenPosition = targetPosition;
        float offset = 1000f; // Arbitrary large value to ensure off-screen

        switch (slideDirection)
        {
            case SlideDirection.Left:
                offScreenPosition.x -= offset;
                break;
            case SlideDirection.Right:
                offScreenPosition.x += offset;
                break;
            case SlideDirection.Top:
                offScreenPosition.y += offset;
                break;
            case SlideDirection.Bottom:
                offScreenPosition.y -= offset;
                break;
        }

        return offScreenPosition;
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Get color for notification type
    /// </summary>
    private Color GetColorForType(NotificationType type)
    {
        switch (type)
        {
            case NotificationType.Info:
                return infoColor;
            case NotificationType.Success:
                return successColor;
            case NotificationType.Warning:
                return warningColor;
            case NotificationType.Error:
                return errorColor;
            default:
                return infoColor;
        }
    }

    /// <summary>
    /// Play sound for notification type
    /// </summary>
    private void PlayNotificationSound(NotificationType type)
    {
        if (!playSounds || audioSource == null)
            return;

        AudioClip clip = null;

        switch (type)
        {
            case NotificationType.Info:
                clip = infoSound;
                break;
            case NotificationType.Success:
                clip = successSound;
                break;
            case NotificationType.Warning:
                clip = warningSound;
                break;
            case NotificationType.Error:
                clip = errorSound;
                break;
        }

        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    #endregion

    #region Debug Methods

    /// <summary>
    /// Test different notification types
    /// </summary>
    [ContextMenu("Test Info Notification")]
    private void TestInfoNotification()
    {
        ShowNotification("This is an info notification", NotificationType.Info);
    }

    [ContextMenu("Test Success Notification")]
    private void TestSuccessNotification()
    {
        ShowNotification("Fish caught successfully!", NotificationType.Success);
    }

    [ContextMenu("Test Warning Notification")]
    private void TestWarningNotification()
    {
        ShowNotification("Sanity is getting low!", NotificationType.Warning);
    }

    [ContextMenu("Test Error Notification")]
    private void TestErrorNotification()
    {
        ShowNotification("Line broke!", NotificationType.Error);
    }

    [ContextMenu("Test Multiple Notifications")]
    private void TestMultipleNotifications()
    {
        ShowNotification("First notification", NotificationType.Info);
        ShowNotification("Second notification", NotificationType.Success);
        ShowNotification("Third notification", NotificationType.Warning);
        ShowNotification("Fourth notification", NotificationType.Error);
        ShowNotification("Fifth notification (queued)", NotificationType.Info);
    }

    #endregion
}
