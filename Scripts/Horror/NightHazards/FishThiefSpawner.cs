using UnityEngine;
using System.Collections;

/// <summary>
/// Spawns crows/phantoms at night that steal fish from player inventory.
/// KEY MECHANIC from Dredge - makes night dangerous!
/// Agent 7: Sanity & Horror System
/// </summary>
public class FishThiefSpawner : MonoBehaviour
{
    private static FishThiefSpawner _instance;
    public static FishThiefSpawner Instance => _instance;

    [Header("Spawn Configuration")]
    [SerializeField] private GameObject crowPrefab;
    [SerializeField] private GameObject phantomPrefab;
    [SerializeField] private float baseSpawnChance = 0.05f; // 5% per check
    [SerializeField] private float spawnCheckInterval = 10f; // Check every 10 seconds
    [SerializeField] private float lowSanityMultiplier = 3f; // 3x chance at low sanity

    [Header("Spawn Conditions")]
    [SerializeField] private bool onlySpawnAtNight = true;
    [SerializeField] private float minSanityForCrows = 100f; // Crows spawn anytime at night
    [SerializeField] private float minSanityForPhantoms = 30f; // Phantoms only at low sanity

    [Header("Spawn Range")]
    [SerializeField] private float minSpawnDistance = 30f;
    [SerializeField] private float maxSpawnDistance = 60f;
    [SerializeField] private float spawnHeight = 20f; // Height above player

    [Header("Active Thieves")]
    [SerializeField] private int maxActiveThieves = 3;
    private int activeThievesCount = 0;

    [Header("Status")]
    [SerializeField] private bool isNighttime = false;
    [SerializeField] private float currentSanity = 100f;
    private float spawnTimer = 0f;

    [Header("Audio")]
    [SerializeField] private AudioClip crowCawSound;
    [SerializeField] private AudioClip phantomWailSound;
    [SerializeField] private AudioClip fishStolenSound;
    private AudioSource audioSource;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;

    private Transform playerTransform;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    private void Start()
    {
        // Subscribe to events
        EventSystem.Subscribe<TimeOfDay>("TimeOfDayChanged", OnTimeOfDayChanged);
        EventSystem.Subscribe<float>("SanityChanged", OnSanityChanged);

        // Get player reference
        if (GameManager.Instance != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        // Initialize from game state
        if (TimeManager.Instance != null)
        {
            isNighttime = TimeManager.Instance.IsNighttime();
        }

        if (SanityManager.Instance != null)
        {
            currentSanity = SanityManager.Instance.GetCurrentSanity();
        }

        if (enableDebugLogging)
        {
            Debug.Log("[FishThiefSpawner] Initialized");
        }
    }

    private void Update()
    {
        if (!CanSpawn()) return;

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnCheckInterval)
        {
            spawnTimer = 0f;
            CheckForSpawn();
        }
    }

    private bool CanSpawn()
    {
        if (playerTransform == null) return false;
        if (activeThievesCount >= maxActiveThieves) return false;
        if (onlySpawnAtNight && !isNighttime) return false;

        return true;
    }

    private void CheckForSpawn()
    {
        float spawnChance = CalculateSpawnChance();

        if (Random.value < spawnChance)
        {
            SpawnThief();
        }
    }

    private float CalculateSpawnChance()
    {
        float chance = baseSpawnChance;

        // Increase chance at low sanity
        if (currentSanity < 30f)
        {
            float sanityFactor = 1f - (currentSanity / 30f);
            chance *= (1f + (lowSanityMultiplier * sanityFactor));
        }

        // Increase chance based on how many fish player has (would need inventory integration)
        // For now, use base chance

        return Mathf.Clamp01(chance);
    }

    private void SpawnThief()
    {
        // Decide between crow and phantom based on sanity
        bool spawnPhantom = (currentSanity < minSanityForPhantoms) && Random.value < 0.5f;

        GameObject prefabToSpawn = spawnPhantom ? phantomPrefab : crowPrefab;

        if (prefabToSpawn == null)
        {
            if (enableDebugLogging)
            {
                Debug.LogWarning("[FishThiefSpawner] Missing prefab for thief spawn");
            }
            return;
        }

        // Calculate spawn position
        Vector3 spawnPosition = CalculateSpawnPosition();

        // Spawn the thief
        GameObject thief = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        FishThief thiefComponent = thief.GetComponent<FishThief>();

        if (thiefComponent != null)
        {
            thiefComponent.Initialize(playerTransform, this);
        }

        activeThievesCount++;

        // Play spawn audio
        AudioClip spawnSound = spawnPhantom ? phantomWailSound : crowCawSound;
        if (spawnSound != null)
        {
            audioSource.PlayOneShot(spawnSound);
        }

        // Publish event
        string thiefType = spawnPhantom ? "Phantom" : "Crow";
        EventSystem.Publish("FishThiefSpawned", thiefType);

        if (enableDebugLogging)
        {
            Debug.Log($"[FishThiefSpawner] Spawned {thiefType} at {spawnPosition}");
        }
    }

    private Vector3 CalculateSpawnPosition()
    {
        // Spawn in random direction around player
        float angle = Random.Range(0f, 360f);
        float distance = Random.Range(minSpawnDistance, maxSpawnDistance);

        Vector3 offset = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * distance;
        offset.y = spawnHeight;

        return playerTransform.position + offset;
    }

    /// <summary>
    /// Called when a thief successfully steals a fish
    /// </summary>
    public void OnFishStolen(string thiefType)
    {
        if (fishStolenSound != null)
        {
            audioSource.PlayOneShot(fishStolenSound);
        }

        EventSystem.Publish("FishStolenByThief", thiefType);

        if (enableDebugLogging)
        {
            Debug.LogWarning($"[FishThiefSpawner] {thiefType} STOLE A FISH!");
        }
    }

    /// <summary>
    /// Called when a thief is destroyed
    /// </summary>
    public void OnThiefDestroyed()
    {
        activeThievesCount = Mathf.Max(0, activeThievesCount - 1);
    }

    /// <summary>
    /// Force spawn a thief (for testing)
    /// </summary>
    public void ForceSpawnThief(bool phantom = false)
    {
        if (playerTransform == null) return;

        GameObject prefab = phantom ? phantomPrefab : crowPrefab;
        if (prefab == null) return;

        Vector3 spawnPos = CalculateSpawnPosition();
        GameObject thief = Instantiate(prefab, spawnPos, Quaternion.identity);

        FishThief thiefComponent = thief.GetComponent<FishThief>();
        if (thiefComponent != null)
        {
            thiefComponent.Initialize(playerTransform, this);
        }

        activeThievesCount++;

        if (enableDebugLogging)
        {
            Debug.Log($"[FishThiefSpawner] Force spawned {(phantom ? "Phantom" : "Crow")}");
        }
    }

    private void OnTimeOfDayChanged(TimeOfDay newTime)
    {
        isNighttime = (newTime == TimeOfDay.Night);

        if (enableDebugLogging)
        {
            Debug.Log($"[FishThiefSpawner] Time changed. Night: {isNighttime}");
        }
    }

    private void OnSanityChanged(float sanity)
    {
        currentSanity = sanity;
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<TimeOfDay>("TimeOfDayChanged", OnTimeOfDayChanged);
        EventSystem.Unsubscribe<float>("SanityChanged", OnSanityChanged);

        if (_instance == this)
        {
            _instance = null;
        }
    }
}

/// <summary>
/// Individual fish thief behavior (crow or phantom).
/// Flies toward player, steals one fish, and escapes.
/// </summary>
public class FishThief : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float approachSpeed = 10f;
    [SerializeField] private float escapeSpeed = 15f;
    [SerializeField] private float stealDistance = 5f;
    [SerializeField] private float bobAmount = 2f;
    [SerializeField] private float bobSpeed = 2f;

    [Header("Behavior")]
    [SerializeField] private float lifetime = 60f; // Despawn after 60 seconds
    [SerializeField] private float escapeDistance = 100f;

    [Header("Status")]
    [SerializeField] private bool hasStolen = false;
    [SerializeField] private bool isEscaping = false;

    private Transform targetTransform;
    private FishThiefSpawner spawner;
    private Vector3 escapeDirection;
    private float lifetimeTimer = 0f;
    private Vector3 startPosition;

    public void Initialize(Transform target, FishThiefSpawner spawnerRef)
    {
        targetTransform = target;
        spawner = spawnerRef;
        startPosition = transform.position;
        escapeDirection = (transform.position - target.position).normalized;
    }

    private void Update()
    {
        if (targetTransform == null)
        {
            Destroy(gameObject);
            return;
        }

        lifetimeTimer += Time.deltaTime;
        if (lifetimeTimer >= lifetime)
        {
            DestroyThief();
            return;
        }

        if (isEscaping)
        {
            EscapeBehavior();
        }
        else
        {
            ApproachBehavior();
        }

        // Apply bobbing motion
        ApplyBobbing();
    }

    private void ApproachBehavior()
    {
        // Move toward player
        Vector3 direction = (targetTransform.position - transform.position).normalized;
        transform.position += direction * approachSpeed * Time.deltaTime;

        // Look at player
        transform.LookAt(targetTransform);

        // Check if close enough to steal
        float distance = Vector3.Distance(transform.position, targetTransform.position);
        if (distance <= stealDistance && !hasStolen)
        {
            StealFish();
        }
    }

    private void EscapeBehavior()
    {
        // Move away from player
        transform.position += escapeDirection * escapeSpeed * Time.deltaTime;

        // Despawn if far enough
        float distance = Vector3.Distance(transform.position, targetTransform.position);
        if (distance >= escapeDistance)
        {
            DestroyThief();
        }
    }

    private void ApplyBobbing()
    {
        // Sine wave bobbing
        float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        Vector3 pos = transform.position;
        pos.y = startPosition.y + bob;
        transform.position = pos;
    }

    private void StealFish()
    {
        hasStolen = true;
        isEscaping = true;

        // Notify spawner
        if (spawner != null)
        {
            string thiefType = name.Contains("Phantom") ? "Phantom" : "Crow";
            spawner.OnFishStolen(thiefType);
        }

        // TODO: Integrate with Agent 6 (Inventory) to actually remove a fish
        // For now, just publish event
        EventSystem.Publish("RemoveRandomFishFromInventory", 1);
    }

    private void DestroyThief()
    {
        if (spawner != null)
        {
            spawner.OnThiefDestroyed();
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (spawner != null)
        {
            spawner.OnThiefDestroyed();
        }
    }
}
