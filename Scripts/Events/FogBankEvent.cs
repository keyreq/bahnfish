using UnityEngine;
using System.Collections;

/// <summary>
/// Agent 19: Dynamic Events Agent - FogBankEvent.cs
/// Handles random fog bank encounters at sea.
///
/// FOG BANK EFFECTS:
/// - 5% chance per hour while at sea
/// - Duration: 5-15 minutes
/// - Visibility drops to 20m
/// - Ghost ship spawn rate 50%
/// - +100% aberrant fish
/// - Navigation becomes challenging
/// - Fog-only fish variants spawn
/// </summary>
public class FogBankEvent : MonoBehaviour
{
    private static FogBankEvent _instance;
    public static FogBankEvent Instance => _instance;

    [Header("Fog Bank Configuration")]
    [SerializeField] private bool isFogBankActive = false;
    [SerializeField] private float fogBankStartTime = 0f;
    [SerializeField] private float fogBankDuration = 600f; // 10 minutes average

    [Header("Fog Settings")]
    [SerializeField] private float fogDensity = 0.05f;
    [SerializeField] private Color fogColor = new Color(0.7f, 0.7f, 0.75f);
    [SerializeField] private float visibility = 20f;
    [SerializeField] private ParticleSystem fogParticles;

    [Header("Spawning Modifiers")]
    [SerializeField] private float ghostShipSpawnBonus = 1.5f; // 50% increase
    [SerializeField] private float aberrantFishBonus = 2f; // +100%

    [Header("Audio")]
    [SerializeField] private AudioClip fogAmbience;
    [SerializeField] private AudioClip fogHorn; // Distant ship horn
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private float hornInterval = 45f; // Every 45 seconds
    private float nextHornTime = 0f;

    [Header("Statistics")]
    [SerializeField] private int ghostShipsEncountered = 0;
    [SerializeField] private int fogFishCaught = 0;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;

    // Original fog settings to restore
    private bool originalFogEnabled;
    private float originalFogDensity;
    private Color originalFogColor;
    private float originalFarClipPlane;

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
        EventSystem.Subscribe<Vector3>("GhostShipAppeared", OnGhostShipAppeared);
        EventSystem.Subscribe<Fish>("FishCaught", OnFishCaught);

        // Setup audio source
        if (ambientSource == null)
        {
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.loop = true;
            ambientSource.spatialBlend = 0f;
            ambientSource.volume = 0.6f;
        }

        // Store original fog settings
        originalFogEnabled = RenderSettings.fog;
        originalFogDensity = RenderSettings.fogDensity;
        originalFogColor = RenderSettings.fogColor;
        if (Camera.main != null)
        {
            originalFarClipPlane = Camera.main.farClipPlane;
        }
    }

    private void Update()
    {
        if (!isFogBankActive) return;

        // Play distant fog horn periodically
        if (Time.time >= nextHornTime && fogHorn != null)
        {
            AudioSource.PlayClipAtPoint(fogHorn, Camera.main.transform.position, 0.3f);
            nextHornTime = Time.time + hornInterval + Random.Range(-10f, 10f);
        }

        // Pulse fog density slightly
        float pulse = Mathf.Sin(Time.time * 0.2f) * 0.005f;
        RenderSettings.fogDensity = fogDensity + pulse;
    }

    private void OnEventStarted(GameEvent gameEvent)
    {
        if (gameEvent.data.eventType != EventType.FogBank) return;

        StartFogBank(gameEvent.data.durationMinutes * 60f);
    }

    private void OnEventEnded(GameEvent gameEvent)
    {
        if (gameEvent.data.eventType != EventType.FogBank) return;

        EndFogBank();
    }

    /// <summary>
    /// Starts a fog bank event
    /// </summary>
    public void StartFogBank(float duration = -1f)
    {
        isFogBankActive = true;
        fogBankStartTime = Time.time;

        // Random duration between 5-15 minutes if not specified
        if (duration < 0f)
        {
            fogBankDuration = Random.Range(300f, 900f);
        }
        else
        {
            fogBankDuration = duration;
        }

        ghostShipsEncountered = 0;
        fogFishCaught = 0;
        nextHornTime = Time.time + 10f;

        // Apply fog effects
        ApplyFogEffects();

        // Apply spawn modifiers
        EventSystem.Publish("GhostShipSpawnBonus", ghostShipSpawnBonus);
        EventSystem.Publish("AberrantFishSpawnBonus", aberrantFishBonus);

        // Enable fog-only fish
        EventSystem.Publish("EnableFogFish", true);

        // Play ambient fog sound
        if (fogAmbience != null && ambientSource != null)
        {
            ambientSource.clip = fogAmbience;
            ambientSource.Play();
        }

        EventSystem.Publish("ShowNotification", "A thick fog bank rolls in... visibility is limited!");

        if (enableDebugLogging)
        {
            Debug.Log($"[FogBankEvent] Fog bank appeared! Duration: {fogBankDuration / 60f:F1} minutes");
        }
    }

    /// <summary>
    /// Ends the fog bank event
    /// </summary>
    public void EndFogBank()
    {
        isFogBankActive = false;

        // Restore fog settings
        RestoreFogSettings();

        // Remove spawn modifiers
        EventSystem.Publish("GhostShipSpawnBonus", 1f);
        EventSystem.Publish("AberrantFishSpawnBonus", 1f);

        // Disable fog-only fish
        EventSystem.Publish("EnableFogFish", false);

        // Stop ambient sound
        if (ambientSource != null)
        {
            ambientSource.Stop();
        }

        // Stop fog particles
        if (fogParticles != null)
        {
            fogParticles.Stop();
        }

        EventSystem.Publish("ShowNotification", $"The fog clears. You caught {fogFishCaught} fish in the mist.");

        if (enableDebugLogging)
        {
            Debug.Log($"[FogBankEvent] Fog bank dissipated. Ghost ships: {ghostShipsEncountered}, Fish: {fogFishCaught}");
        }
    }

    /// <summary>
    /// Applies fog visual effects
    /// </summary>
    private void ApplyFogEffects()
    {
        // Enable fog
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.fogColor = fogColor;

        // Reduce camera far clip plane
        if (Camera.main != null)
        {
            Camera.main.farClipPlane = visibility;
        }

        // Start fog particles
        if (fogParticles != null)
        {
            fogParticles.Play();
        }

        // Desaturate lighting
        RenderSettings.ambientIntensity = 0.6f;

        // Apply post-processing for muffled effect
        EventSystem.Publish("ApplyFogPostProcessing", true);
    }

    /// <summary>
    /// Restores original fog settings
    /// </summary>
    private void RestoreFogSettings()
    {
        RenderSettings.fog = originalFogEnabled;
        RenderSettings.fogDensity = originalFogDensity;
        RenderSettings.fogColor = originalFogColor;

        if (Camera.main != null)
        {
            Camera.main.farClipPlane = originalFarClipPlane;
        }

        RenderSettings.ambientIntensity = 1f;

        EventSystem.Publish("RemoveFogPostProcessing", true);
    }

    /// <summary>
    /// Checks if fog bank should spawn (5% chance per hour at sea)
    /// </summary>
    public bool ShouldSpawnFogBank()
    {
        if (isFogBankActive) return false;

        // Check if player is at sea (not docked)
        // TODO: Integrate with dock/location system

        return Random.value < 0.05f;
    }

    /// <summary>
    /// Gets time remaining in fog bank
    /// </summary>
    public float GetTimeRemaining()
    {
        if (!isFogBankActive) return 0f;

        return Mathf.Max(0f, (fogBankStartTime + fogBankDuration) - Time.time);
    }

    private void OnGhostShipAppeared(Vector3 position)
    {
        if (!isFogBankActive) return;

        ghostShipsEncountered++;

        if (enableDebugLogging)
        {
            Debug.Log($"[FogBankEvent] Ghost ship appeared in fog! Total: {ghostShipsEncountered}");
        }
    }

    private void OnFishCaught(Fish fish)
    {
        if (!isFogBankActive) return;

        fogFishCaught++;

        // Check if it's a fog-exclusive variant
        if (fish.id.Contains("fog"))
        {
            EventSystem.Publish("ShowNotification", $"Caught a rare fog-dweller: {fish.name}!");
        }
    }

    public bool IsActive()
    {
        return isFogBankActive;
    }

    public void ForceTrigger()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.ForceTriggerEvent("fog_bank");
        }
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<GameEvent>("EventStarted", OnEventStarted);
        EventSystem.Unsubscribe<GameEvent>("EventEnded", OnEventEnded);
        EventSystem.Unsubscribe<Vector3>("GhostShipAppeared", OnGhostShipAppeared);
        EventSystem.Unsubscribe<Fish>("FishCaught", OnFishCaught);

        if (_instance == this)
        {
            _instance = null;
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Force Fog Bank")]
    private void ForceFogBankEditor()
    {
        ForceTrigger();
    }

    [ContextMenu("End Fog Bank")]
    private void EndFogBankEditor()
    {
        if (isFogBankActive)
        {
            EndFogBank();
        }
    }

    [ContextMenu("Test Fog Roll")]
    private void TestFogRoll()
    {
        bool shouldSpawn = ShouldSpawnFogBank();
        Debug.Log($"Fog bank roll result: {(shouldSpawn ? "SPAWN" : "no spawn")}");
    }
#endif
}
