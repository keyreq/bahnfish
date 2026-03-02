using UnityEngine;
using System.Collections;

/// <summary>
/// Agent 19: Dynamic Events Agent - MeteorShowerEvent.cs
/// Handles meteor shower events - beautiful celestial displays with fishing bonuses.
///
/// METEOR SHOWER EFFECTS:
/// - Occurs every 3 days (30% chance if 3+ days passed)
/// - Duration: 30 real-time minutes
/// - +200% rare fish spawn rate
/// - +50% legendary fish chance
/// - +25% catch success rate
/// - Meteor visual effects
/// - Meteor fragments can be dredged
/// - No negative effects (pure bonus!)
/// </summary>
public class MeteorShowerEvent : MonoBehaviour
{
    private static MeteorShowerEvent _instance;
    public static MeteorShowerEvent Instance => _instance;

    [Header("Meteor Shower Configuration")]
    [SerializeField] private bool isMeteorShowerActive = false;
    [SerializeField] private float meteorShowerStartTime = 0f;
    [SerializeField] private float meteorShowerDuration = 1800f; // 30 minutes

    [Header("Meteor Spawning")]
    [SerializeField] private GameObject meteorPrefab;
    [SerializeField] private float meteorSpawnInterval = 5f; // Spawn meteor every 5 seconds
    [SerializeField] private int maxActiveMeteors = 10;
    [SerializeField] private float meteorFragmentDropChance = 0.1f; // 10% chance per meteor
    private float nextMeteorSpawnTime = 0f;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem starParticles;
    [SerializeField] private Color skyGlow = new Color(0.6f, 0.7f, 1f, 0.3f);

    [Header("Audio")]
    [SerializeField] private AudioClip meteorWhistle;
    [SerializeField] private AudioClip meteorSplash;
    [SerializeField] private AudioClip ambientMusic;
    [SerializeField] private AudioSource musicSource;

    [Header("Bonuses")]
    [SerializeField] private float rareFishBonus = 3f; // +200% = 3x multiplier
    [SerializeField] private float legendaryBonus = 1.5f; // +50%
    [SerializeField] private float catchSuccessBonus = 0.25f; // +25%

    [Header("Statistics")]
    [SerializeField] private int meteorsSeen = 0;
    [SerializeField] private int fragmentsCollected = 0;
    [SerializeField] private int rareFishCaught = 0;

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

        // Setup audio source
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = 0.4f;
        }
    }

    private void Update()
    {
        if (!isMeteorShowerActive) return;

        // Spawn meteors periodically
        if (Time.time >= nextMeteorSpawnTime)
        {
            SpawnMeteor();
            nextMeteorSpawnTime = Time.time + meteorSpawnInterval + Random.Range(-2f, 2f);
        }
    }

    private void OnEventStarted(GameEvent gameEvent)
    {
        if (gameEvent.data.eventType != EventType.MeteorShower) return;

        StartMeteorShower();
    }

    private void OnEventEnded(GameEvent gameEvent)
    {
        if (gameEvent.data.eventType != EventType.MeteorShower) return;

        EndMeteorShower();
    }

    /// <summary>
    /// Starts the meteor shower event
    /// </summary>
    private void StartMeteorShower()
    {
        isMeteorShowerActive = true;
        meteorShowerStartTime = Time.time;
        meteorsSeen = 0;
        fragmentsCollected = 0;
        rareFishCaught = 0;
        nextMeteorSpawnTime = Time.time + 5f;

        // Apply bonuses
        EventSystem.Publish("RareFishSpawnBonus", rareFishBonus);
        EventSystem.Publish("LegendaryFishSpawnBonus", legendaryBonus);
        EventSystem.Publish("CatchSuccessBonus", catchSuccessBonus);

        // Visual effects
        if (starParticles != null)
        {
            starParticles.Play();
        }

        RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, skyGlow, 0.3f);

        // Play ambient music
        if (ambientMusic != null && musicSource != null)
        {
            musicSource.clip = ambientMusic;
            musicSource.Play();
        }

        EventSystem.Publish("ShowNotification", "A meteor shower illuminates the sky! Rare fish are abundant!");

        if (enableDebugLogging)
        {
            Debug.Log("[MeteorShowerEvent] Meteor shower has begun!");
        }
    }

    /// <summary>
    /// Ends the meteor shower event
    /// </summary>
    private void EndMeteorShower()
    {
        isMeteorShowerActive = false;

        // Remove bonuses
        EventSystem.Publish("RareFishSpawnBonus", 1f);
        EventSystem.Publish("LegendaryFishSpawnBonus", 1f);
        EventSystem.Publish("CatchSuccessBonus", 0f);

        // Stop visual effects
        if (starParticles != null)
        {
            starParticles.Stop();
        }

        // Stop music
        if (musicSource != null)
        {
            musicSource.Stop();
        }

        EventSystem.Publish("ShowNotification", $"The meteor shower ends. You witnessed {meteorsSeen} meteors!");

        if (enableDebugLogging)
        {
            Debug.Log($"[MeteorShowerEvent] Meteor shower ended. Meteors: {meteorsSeen}, Fragments: {fragmentsCollected}, Rare fish: {rareFishCaught}");
        }
    }

    /// <summary>
    /// Spawns a meteor in the sky
    /// </summary>
    private void SpawnMeteor()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // Random position in sky around player
        Vector3 skyPosition = player.transform.position + new Vector3(
            Random.Range(-100f, 100f),
            Random.Range(100f, 200f),
            Random.Range(-100f, 100f)
        );

        // Random direction (downward)
        Vector3 direction = new Vector3(
            Random.Range(-1f, 1f),
            -1f,
            Random.Range(-1f, 1f)
        ).normalized;

        // Create meteor
        if (meteorPrefab != null)
        {
            GameObject meteor = Instantiate(meteorPrefab, skyPosition, Quaternion.LookRotation(direction));
            Meteor meteorScript = meteor.GetComponent<Meteor>();
            if (meteorScript != null)
            {
                meteorScript.Initialize(direction, meteorFragmentDropChance);
            }
        }
        else
        {
            // Create simple meteor effect if no prefab
            CreateSimpleMeteor(skyPosition, direction);
        }

        // Play whistle sound
        if (meteorWhistle != null)
        {
            AudioSource.PlayClipAtPoint(meteorWhistle, skyPosition, 0.5f);
        }

        meteorsSeen++;
    }

    /// <summary>
    /// Creates a simple meteor effect without prefab
    /// </summary>
    private void CreateSimpleMeteor(Vector3 startPos, Vector3 direction)
    {
        GameObject meteor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        meteor.name = "Meteor";
        meteor.transform.position = startPos;
        meteor.transform.localScale = Vector3.one * 2f;

        // Add glow material
        Renderer renderer = meteor.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", Color.cyan * 2f);
        }

        // Add trail
        TrailRenderer trail = meteor.AddComponent<TrailRenderer>();
        trail.time = 2f;
        trail.startWidth = 1f;
        trail.endWidth = 0.1f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.startColor = Color.cyan;
        trail.endColor = Color.clear;

        // Add movement script
        SimpleMeteor script = meteor.AddComponent<SimpleMeteor>();
        script.Initialize(direction, 50f, meteorFragmentDropChance);
    }

    /// <summary>
    /// Called when meteor hits water and potentially drops fragment
    /// </summary>
    public void OnMeteorImpact(Vector3 position, bool dropFragment)
    {
        // Play splash sound
        if (meteorSplash != null)
        {
            AudioSource.PlayClipAtPoint(meteorSplash, position, 0.7f);
        }

        // Create splash effect
        EventSystem.Publish("CreateSplashEffect", position);

        // Drop fragment if rolled successfully
        if (dropFragment)
        {
            fragmentsCollected++;
            EventSystem.Publish("SpawnMeteorFragment", position);

            if (enableDebugLogging)
            {
                Debug.Log($"[MeteorShowerEvent] Meteor fragment spawned at {position}");
            }
        }
    }

    private void OnFishCaught(Fish fish)
    {
        if (!isMeteorShowerActive) return;

        if (fish.rarity == FishRarity.Rare || fish.rarity == FishRarity.Legendary)
        {
            rareFishCaught++;
        }
    }

    public bool IsActive()
    {
        return isMeteorShowerActive;
    }

    public void ForceTrigger()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.ForceTriggerEvent("meteor_shower");
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
}

/// <summary>
/// Simple meteor behavior for when no prefab is available
/// </summary>
public class SimpleMeteor : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private bool dropFragment;
    private bool hasImpacted = false;

    public void Initialize(Vector3 dir, float spd, float fragmentChance)
    {
        direction = dir;
        speed = spd;
        dropFragment = Random.value < fragmentChance;
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        // Check if below water level
        if (transform.position.y < 0f && !hasImpacted)
        {
            Impact();
        }

        // Destroy if too far down
        if (transform.position.y < -50f)
        {
            Destroy(gameObject);
        }
    }

    private void Impact()
    {
        hasImpacted = true;

        if (MeteorShowerEvent.Instance != null)
        {
            MeteorShowerEvent.Instance.OnMeteorImpact(transform.position, dropFragment);
        }

        Destroy(gameObject, 1f);
    }
}

/// <summary>
/// Full meteor behavior script (to be used with meteor prefab)
/// </summary>
public class Meteor : MonoBehaviour
{
    private Vector3 direction;
    private float speed = 50f;
    private bool dropFragment;
    private bool hasImpacted = false;

    [SerializeField] private ParticleSystem trailParticles;
    [SerializeField] private Light meteorLight;

    public void Initialize(Vector3 dir, float fragmentChance)
    {
        direction = dir;
        dropFragment = Random.value < fragmentChance;

        if (meteorLight != null)
        {
            meteorLight.color = Color.cyan;
            meteorLight.intensity = 5f;
            meteorLight.range = 30f;
        }

        if (trailParticles != null)
        {
            trailParticles.Play();
        }
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        // Check if below water level
        if (transform.position.y < 0f && !hasImpacted)
        {
            Impact();
        }

        // Destroy if too far down
        if (transform.position.y < -50f)
        {
            Destroy(gameObject);
        }
    }

    private void Impact()
    {
        hasImpacted = true;

        if (MeteorShowerEvent.Instance != null)
        {
            MeteorShowerEvent.Instance.OnMeteorImpact(transform.position, dropFragment);
        }

        if (trailParticles != null)
        {
            trailParticles.Stop();
        }

        Destroy(gameObject, 2f);
    }
}
