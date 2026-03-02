using UnityEngine;
using System.Collections;

/// <summary>
/// Agent 19: Dynamic Events Agent - BloodMoonEvent.cs
/// Handles the Blood Moon event - rare monthly occurrence where aberrant fish spawn everywhere.
///
/// BLOOD MOON EFFECTS:
/// - Occurs every 10 days (10% chance if 10+ days passed)
/// - Duration: One full night (6 real-time minutes)
/// - 100% aberrant fish spawns
/// - 3x hazard spawn rate
/// - 2x sanity drain
/// - 10x fish values
/// - Guaranteed legendary fish spawn
/// - Triple relics from catches
/// - Red moon and sky visuals
/// </summary>
public class BloodMoonEvent : MonoBehaviour
{
    private static BloodMoonEvent _instance;
    public static BloodMoonEvent Instance => _instance;

    [Header("Blood Moon Configuration")]
    [SerializeField] private bool isBloodMoonActive = false;
    [SerializeField] private float bloodMoonStartTime = 0f;
    [SerializeField] private float bloodMoonEndTime = 0f;

    [Header("Visual Effects")]
    [SerializeField] private Color bloodMoonSkyColor = new Color(0.5f, 0.1f, 0.1f, 1f);
    [SerializeField] private Color bloodMoonWaterColor = new Color(0.4f, 0.05f, 0.05f, 1f);
    [SerializeField] private Color bloodMoonVignette = new Color(0.5f, 0f, 0f, 0.6f);
    [SerializeField] private Light moonLight;
    [SerializeField] private GameObject bloodMoonParticles;

    [Header("Audio")]
    [SerializeField] private AudioClip bloodMoonMusic;
    [SerializeField] private AudioClip[] eerieSounds;
    [SerializeField] private AudioSource ambientAudioSource;
    [SerializeField] private float soundInterval = 15f;
    private float nextSoundTime = 0f;

    [Header("Statistics")]
    [SerializeField] private int aberrantFishCaught = 0;
    [SerializeField] private int legendariesCaught = 0;
    [SerializeField] private bool legendarySpawned = false;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;

    // Original values to restore
    private Color originalSkyColor;
    private Color originalWaterColor;
    private Color originalMoonColor;
    private float originalMoonIntensity;

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
        // Subscribe to event system
        EventSystem.Subscribe<GameEvent>("EventStarted", OnEventStarted);
        EventSystem.Subscribe<GameEvent>("EventEnded", OnEventEnded);
        EventSystem.Subscribe<Fish>("FishCaught", OnFishCaught);

        // Setup audio source
        if (ambientAudioSource == null)
        {
            ambientAudioSource = gameObject.AddComponent<AudioSource>();
            ambientAudioSource.loop = true;
            ambientAudioSource.spatialBlend = 0f; // 2D sound
            ambientAudioSource.volume = 0.5f;
        }
    }

    private void Update()
    {
        if (!isBloodMoonActive) return;

        // Play random eerie sounds
        if (Time.time >= nextSoundTime && eerieSounds.Length > 0)
        {
            PlayEerieSound();
            nextSoundTime = Time.time + soundInterval + Random.Range(-5f, 5f);
        }

        // Pulse the moon light
        if (moonLight != null)
        {
            float pulse = Mathf.Sin(Time.time * 0.5f) * 0.3f + 1f;
            moonLight.intensity = originalMoonIntensity * pulse;
        }
    }

    private void OnEventStarted(GameEvent gameEvent)
    {
        if (gameEvent.data.eventType != EventType.BloodMoon) return;

        StartBloodMoon();
    }

    private void OnEventEnded(GameEvent gameEvent)
    {
        if (gameEvent.data.eventType != EventType.BloodMoon) return;

        EndBloodMoon();
    }

    /// <summary>
    /// Starts the Blood Moon event
    /// </summary>
    private void StartBloodMoon()
    {
        isBloodMoonActive = true;
        bloodMoonStartTime = Time.time;
        bloodMoonEndTime = Time.time + (6f * 60f); // 6 minutes
        aberrantFishCaught = 0;
        legendariesCaught = 0;
        legendarySpawned = false;

        // Apply visual effects
        ApplyBloodMoonVisuals();

        // Play blood moon music
        if (bloodMoonMusic != null && ambientAudioSource != null)
        {
            ambientAudioSource.clip = bloodMoonMusic;
            ambientAudioSource.Play();
        }

        // Force all fish to be aberrant
        EventSystem.Publish("ForceAberrantFish", true);

        // Triple hazard spawns
        EventSystem.Publish("HazardSpawnMultiplierOverride", 3f);

        // Double sanity drain
        EventSystem.Publish("SanityDrainMultiplierOverride", 2f);

        // 10x fish values
        EventSystem.Publish("FishValueMultiplierOverride", 10f);

        // Triple relic drops
        EventSystem.Publish("RelicDropMultiplierOverride", 3f);

        // Spawn guaranteed legendary after 1 minute
        StartCoroutine(SpawnGuaranteedLegendary());

        // Show notification
        EventSystem.Publish("ShowNotification", "THE BLOOD MOON RISES... BEWARE!");

        if (enableDebugLogging)
        {
            Debug.Log("[BloodMoonEvent] BLOOD MOON HAS RISEN!");
        }
    }

    /// <summary>
    /// Ends the Blood Moon event
    /// </summary>
    private void EndBloodMoon()
    {
        isBloodMoonActive = false;

        // Restore visual effects
        RestoreOriginalVisuals();

        // Stop music
        if (ambientAudioSource != null)
        {
            ambientAudioSource.Stop();
        }

        // Stop forcing aberrant fish
        EventSystem.Publish("ForceAberrantFish", false);

        // Restore normal spawn rates
        EventSystem.Publish("HazardSpawnMultiplierOverride", 1f);
        EventSystem.Publish("SanityDrainMultiplierOverride", 1f);
        EventSystem.Publish("FishValueMultiplierOverride", 1f);
        EventSystem.Publish("RelicDropMultiplierOverride", 1f);

        // Deactivate particle effects
        if (bloodMoonParticles != null)
        {
            bloodMoonParticles.SetActive(false);
        }

        // Show end notification
        EventSystem.Publish("ShowNotification", $"The Blood Moon wanes. You caught {aberrantFishCaught} aberrant fish!");

        if (enableDebugLogging)
        {
            Debug.Log($"[BloodMoonEvent] Blood Moon ended. Stats: {aberrantFishCaught} aberrant, {legendariesCaught} legendary");
        }
    }

    /// <summary>
    /// Applies Blood Moon visual effects
    /// </summary>
    private void ApplyBloodMoonVisuals()
    {
        // Store original values
        if (RenderSettings.skybox != null)
        {
            originalSkyColor = RenderSettings.ambientSkyColor;
        }

        if (moonLight != null)
        {
            originalMoonColor = moonLight.color;
            originalMoonIntensity = moonLight.intensity;

            // Apply blood moon color
            moonLight.color = new Color(0.8f, 0.1f, 0.1f);
            moonLight.intensity = originalMoonIntensity * 1.5f;
        }

        // Set ambient lighting to red
        RenderSettings.ambientSkyColor = bloodMoonSkyColor;
        RenderSettings.ambientEquatorColor = bloodMoonSkyColor * 0.7f;
        RenderSettings.ambientGroundColor = bloodMoonSkyColor * 0.5f;

        // Apply fog
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.3f, 0.05f, 0.05f);
        RenderSettings.fogDensity = 0.01f;

        // Activate blood moon particles (red fog)
        if (bloodMoonParticles != null)
        {
            bloodMoonParticles.SetActive(true);
        }

        // Publish visual override event for post-processing
        EventSystem.Publish("ApplyBloodMoonPostProcessing", bloodMoonVignette);

        // Water tint
        EventSystem.Publish("SetWaterTint", bloodMoonWaterColor);
    }

    /// <summary>
    /// Restores original visuals
    /// </summary>
    private void RestoreOriginalVisuals()
    {
        if (moonLight != null)
        {
            moonLight.color = originalMoonColor;
            moonLight.intensity = originalMoonIntensity;
        }

        RenderSettings.ambientSkyColor = originalSkyColor;

        // Restore fog
        RenderSettings.fog = false;

        // Remove post-processing
        EventSystem.Publish("RemoveBloodMoonPostProcessing", true);

        // Restore water color
        EventSystem.Publish("RestoreWaterTint", true);
    }

    /// <summary>
    /// Spawns guaranteed legendary fish after delay
    /// </summary>
    private IEnumerator SpawnGuaranteedLegendary()
    {
        yield return new WaitForSeconds(60f); // Wait 1 minute

        if (!isBloodMoonActive) yield break;

        // Find player position
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && FishManager.Instance != null)
        {
            Vector3 spawnPos = player.transform.position + Random.insideUnitSphere * 30f;
            spawnPos.y = Random.Range(-10f, -5f); // Underwater

            // Force spawn legendary
            EventSystem.Publish("ForceSpawnLegendary", spawnPos);
            legendarySpawned = true;

            EventSystem.Publish("ShowNotification", "A legendary presence stirs in the crimson depths...");

            if (enableDebugLogging)
            {
                Debug.Log("[BloodMoonEvent] Guaranteed legendary fish spawned!");
            }
        }
    }

    /// <summary>
    /// Plays a random eerie sound
    /// </summary>
    private void PlayEerieSound()
    {
        if (eerieSounds.Length == 0) return;

        AudioClip sound = eerieSounds[Random.Range(0, eerieSounds.Length)];
        AudioSource.PlayClipAtPoint(sound, Camera.main.transform.position, 0.3f);
    }

    /// <summary>
    /// Tracks fish caught during blood moon
    /// </summary>
    private void OnFishCaught(Fish fish)
    {
        if (!isBloodMoonActive) return;

        if (fish.isAberrant)
        {
            aberrantFishCaught++;
        }

        if (fish.rarity == FishRarity.Legendary)
        {
            legendariesCaught++;
        }
    }

    /// <summary>
    /// Gets blood moon statistics
    /// </summary>
    public BloodMoonStats GetStats()
    {
        return new BloodMoonStats
        {
            isActive = isBloodMoonActive,
            aberrantCaught = aberrantFishCaught,
            legendariesCaught = legendariesCaught,
            legendarySpawned = legendarySpawned,
            timeRemaining = Mathf.Max(0f, bloodMoonEndTime - Time.time)
        };
    }

    /// <summary>
    /// Checks if Blood Moon is currently active
    /// </summary>
    public bool IsActive()
    {
        return isBloodMoonActive;
    }

    /// <summary>
    /// Force triggers Blood Moon (for testing)
    /// </summary>
    public void ForceTrigger()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.ForceTriggerEvent("blood_moon");
        }
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
    [ContextMenu("Force Blood Moon")]
    private void ForceBloodMoonEditor()
    {
        ForceTrigger();
    }

    [ContextMenu("End Blood Moon")]
    private void EndBloodMoonEditor()
    {
        if (isBloodMoonActive)
        {
            EndBloodMoon();
        }
    }
#endif
}

/// <summary>
/// Blood Moon statistics
/// </summary>
[System.Serializable]
public struct BloodMoonStats
{
    public bool isActive;
    public int aberrantCaught;
    public int legendariesCaught;
    public bool legendarySpawned;
    public float timeRemaining;
}
