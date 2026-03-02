using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Subtitle display system for dialog and audio.
/// Features:
/// - Display subtitles for all dialog
/// - Speaker name labels
/// - Background box with adjustable opacity
/// - Font size based on accessibility settings
/// - Queue system (prevents overlapping subtitles)
/// - Timing based on audio duration or manual duration
/// - Fade in/out animations
/// </summary>
public class SubtitleSystem : MonoBehaviour
{
    private static SubtitleSystem _instance;
    public static SubtitleSystem Instance => _instance;

    [Header("UI References")]
    [SerializeField] private GameObject subtitlePanel;
    [SerializeField] private Text speakerNameText;
    [SerializeField] private Text subtitleText;
    [SerializeField] private Image backgroundImage;

    [Header("Settings")]
    [SerializeField] private bool subtitlesEnabled = true;
    [SerializeField] private float defaultDuration = 3f;
    [SerializeField] private float fadeTime = 0.3f;
    [SerializeField] private float characterReadTime = 0.05f; // Time per character for auto-duration

    [Header("Colors")]
    [SerializeField] private Color speakerNameColor = Color.yellow;
    [SerializeField] private Color subtitleColor = Color.white;

    // Subtitle queue
    private Queue<SubtitleEntry> subtitleQueue = new Queue<SubtitleEntry>();
    private bool isDisplaying = false;
    private Coroutine displayCoroutine = null;

    // Current settings
    private float backgroundOpacity = 0.7f;
    private int fontSize = 16;

    /// <summary>
    /// Subtitle entry structure.
    /// </summary>
    private class SubtitleEntry
    {
        public string speakerName;
        public string text;
        public float duration;
        public AudioClip audioClip;
    }

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
        // Subscribe to settings events
        EventSystem.Subscribe<bool>("SetSubtitlesEnabled", OnSetSubtitlesEnabled);
        EventSystem.Subscribe<AudioSettings.SubtitleSize>("SetSubtitleSize", OnSetSubtitleSize);
        EventSystem.Subscribe<float>("SetSubtitleBackgroundOpacity", OnSetBackgroundOpacity);

        // Initialize
        if (subtitlePanel != null)
        {
            subtitlePanel.SetActive(false);
        }

        ApplySettings();
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<bool>("SetSubtitlesEnabled", OnSetSubtitlesEnabled);
        EventSystem.Unsubscribe<AudioSettings.SubtitleSize>("SetSubtitleSize", OnSetSubtitleSize);
        EventSystem.Unsubscribe<float>("SetSubtitleBackgroundOpacity", OnSetBackgroundOpacity);

        if (_instance == this)
        {
            _instance = null;
        }
    }

    /// <summary>
    /// Apply current settings.
    /// </summary>
    private void ApplySettings()
    {
        if (backgroundImage != null)
        {
            Color bgColor = backgroundImage.color;
            bgColor.a = backgroundOpacity;
            backgroundImage.color = bgColor;
        }

        if (subtitleText != null)
        {
            subtitleText.fontSize = fontSize;
            subtitleText.color = subtitleColor;
        }

        if (speakerNameText != null)
        {
            speakerNameText.fontSize = fontSize + 2;
            speakerNameText.color = speakerNameColor;
        }
    }

    #region Public Methods

    /// <summary>
    /// Show a subtitle with speaker name.
    /// Duration is auto-calculated based on text length.
    /// </summary>
    public void ShowSubtitle(string speakerName, string text)
    {
        float duration = CalculateAutoDuration(text);
        ShowSubtitle(speakerName, text, duration);
    }

    /// <summary>
    /// Show a subtitle with speaker name and custom duration.
    /// </summary>
    public void ShowSubtitle(string speakerName, string text, float duration)
    {
        if (!subtitlesEnabled) return;

        SubtitleEntry entry = new SubtitleEntry
        {
            speakerName = speakerName,
            text = text,
            duration = duration,
            audioClip = null
        };

        subtitleQueue.Enqueue(entry);
        ProcessQueue();
    }

    /// <summary>
    /// Show a subtitle synced with an audio clip.
    /// </summary>
    public void ShowSubtitle(string speakerName, string text, AudioClip audioClip)
    {
        if (!subtitlesEnabled) return;

        float duration = audioClip != null ? audioClip.length : CalculateAutoDuration(text);

        SubtitleEntry entry = new SubtitleEntry
        {
            speakerName = speakerName,
            text = text,
            duration = duration,
            audioClip = audioClip
        };

        subtitleQueue.Enqueue(entry);
        ProcessQueue();
    }

    /// <summary>
    /// Clear all queued subtitles.
    /// </summary>
    public void ClearQueue()
    {
        subtitleQueue.Clear();

        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
            displayCoroutine = null;
        }

        if (subtitlePanel != null)
        {
            subtitlePanel.SetActive(false);
        }

        isDisplaying = false;
    }

    /// <summary>
    /// Enable or disable subtitles.
    /// </summary>
    public void SetSubtitlesEnabled(bool enabled)
    {
        subtitlesEnabled = enabled;

        if (!enabled)
        {
            ClearQueue();
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Process subtitle queue.
    /// </summary>
    private void ProcessQueue()
    {
        if (isDisplaying || subtitleQueue.Count == 0) return;

        displayCoroutine = StartCoroutine(DisplayNextSubtitle());
    }

    /// <summary>
    /// Display next subtitle in queue.
    /// </summary>
    private IEnumerator DisplayNextSubtitle()
    {
        isDisplaying = true;

        SubtitleEntry entry = subtitleQueue.Dequeue();

        // Set text
        if (speakerNameText != null)
        {
            speakerNameText.text = entry.speakerName;
        }

        if (subtitleText != null)
        {
            subtitleText.text = entry.text;
        }

        // Show panel
        if (subtitlePanel != null)
        {
            subtitlePanel.SetActive(true);
        }

        // Fade in
        yield return StartCoroutine(FadePanel(0f, 1f, fadeTime));

        // Wait for duration
        yield return new WaitForSeconds(entry.duration);

        // Fade out
        yield return StartCoroutine(FadePanel(1f, 0f, fadeTime));

        // Hide panel
        if (subtitlePanel != null)
        {
            subtitlePanel.SetActive(false);
        }

        isDisplaying = false;

        // Process next subtitle
        if (subtitleQueue.Count > 0)
        {
            ProcessQueue();
        }
    }

    /// <summary>
    /// Fade subtitle panel.
    /// </summary>
    private IEnumerator FadePanel(float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float alpha = Mathf.Lerp(fromAlpha, toAlpha, t);

            SetPanelAlpha(alpha);

            yield return null;
        }

        SetPanelAlpha(toAlpha);
    }

    /// <summary>
    /// Set panel alpha.
    /// </summary>
    private void SetPanelAlpha(float alpha)
    {
        if (backgroundImage != null)
        {
            Color bgColor = backgroundImage.color;
            bgColor.a = backgroundOpacity * alpha;
            backgroundImage.color = bgColor;
        }

        if (speakerNameText != null)
        {
            Color nameColor = speakerNameText.color;
            nameColor.a = alpha;
            speakerNameText.color = nameColor;
        }

        if (subtitleText != null)
        {
            Color textColor = subtitleText.color;
            textColor.a = alpha;
            subtitleText.color = textColor;
        }
    }

    /// <summary>
    /// Calculate auto duration based on text length.
    /// </summary>
    private float CalculateAutoDuration(string text)
    {
        float baseDuration = text.Length * characterReadTime;
        return Mathf.Max(baseDuration, defaultDuration);
    }

    #endregion

    #region Event Handlers

    private void OnSetSubtitlesEnabled(bool enabled)
    {
        SetSubtitlesEnabled(enabled);
    }

    private void OnSetSubtitleSize(AudioSettings.SubtitleSize size)
    {
        switch (size)
        {
            case AudioSettings.SubtitleSize.Small:
                fontSize = 14;
                break;
            case AudioSettings.SubtitleSize.Medium:
                fontSize = 18;
                break;
            case AudioSettings.SubtitleSize.Large:
                fontSize = 24;
                break;
        }

        ApplySettings();
    }

    private void OnSetBackgroundOpacity(float opacity)
    {
        backgroundOpacity = opacity;
        ApplySettings();
    }

    #endregion

    #region Public Accessors

    public bool SubtitlesEnabled => subtitlesEnabled;
    public int QueueSize => subtitleQueue.Count;

    #endregion
}
