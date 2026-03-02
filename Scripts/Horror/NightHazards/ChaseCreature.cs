using UnityEngine;
using System.Collections;

/// <summary>
/// Large underwater creature that pursues player at low sanity.
/// Creates tense chase sequences requiring speed and evasion.
/// Agent 7: Sanity & Horror System
/// </summary>
public class ChaseCreature : MonoBehaviour
{
    private static ChaseCreature _instance;
    public static ChaseCreature Instance => _instance;

    [Header("Creature Prefab")]
    [SerializeField] private GameObject creaturePrefab;

    [Header("Spawn Conditions")]
    [SerializeField] private float sanityThreshold = 20f; // Spawn below 20%
    [SerializeField] private float spawnCooldown = 120f; // 2 minutes between spawns
    [SerializeField] private bool onlySpawnAtNight = true;

    [Header("Spawn Configuration")]
    [SerializeField] private float spawnDistance = 80f;
    [SerializeField] private float spawnDepth = -10f; // Below water surface

    [Header("Creature Properties")]
    [SerializeField] private float baseSpeed = 8f;
    [SerializeField] private float maxSpeed = 15f;
    [SerializeField] private float catchUpAcceleration = 2f;
    [SerializeField] private float damagePerSecond = 5f;
    [SerializeField] private float damageInterval = 1f;

    [Header("Chase Behavior")]
    [SerializeField] private float chaseRange = 100f;
    [SerializeField] private float giveUpDistance = 150f;
    [SerializeField] private float catchDistance = 8f; // Close enough to damage
    [SerializeField] private float emergeDuration = 3f; // Time emerging from water

    [Header("Escape Conditions")]
    [SerializeField] private bool escapeAtDock = true;
    [SerializeField] private float dockSafeDistance = 20f;
    [SerializeField] private bool escapeAtHighSanity = true;
    [SerializeField] private float escapeSanityThreshold = 40f;

    [Header("Status")]
    [SerializeField] private bool creatureActive = false;
    [SerializeField] private float currentSanity = 100f;
    [SerializeField] private bool isNighttime = false;
    private float cooldownTimer = 0f;
    private GameObject activeCreature;

    [Header("Audio")]
    [SerializeField] private AudioClip creatureRoarSound;
    [SerializeField] private AudioClip creatureChaseMusic;
    [SerializeField] private AudioClip creatureAttackSound;
    private AudioSource audioSource;
    private AudioSource musicSource;

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
        audioSource.spatialBlend = 1f;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
    }

    private void Start()
    {
        // Subscribe to events
        EventSystem.Subscribe<float>("SanityChanged", OnSanityChanged);
        EventSystem.Subscribe<TimeOfDay>("TimeOfDayChanged", OnTimeOfDayChanged);

        // Get player reference
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Initialize state
        if (SanityManager.Instance != null)
        {
            currentSanity = SanityManager.Instance.GetCurrentSanity();
        }

        if (TimeManager.Instance != null)
        {
            isNighttime = TimeManager.Instance.IsNighttime();
        }

        if (enableDebugLogging)
        {
            Debug.Log("[ChaseCreature] Initialized");
        }
    }

    private void Update()
    {
        // Update cooldown
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        // Check spawn conditions
        if (!creatureActive && CanSpawn())
        {
            SpawnCreature();
        }

        // Check escape conditions
        if (creatureActive && ShouldEscape())
        {
            DespawnCreature();
        }
    }

    private bool CanSpawn()
    {
        if (playerTransform == null) return false;
        if (cooldownTimer > 0f) return false;
        if (currentSanity >= sanityThreshold) return false;
        if (onlySpawnAtNight && !isNighttime) return false;

        return true;
    }

    private bool ShouldEscape()
    {
        // Escape if sanity restored
        if (escapeAtHighSanity && currentSanity >= escapeSanityThreshold)
        {
            return true;
        }

        // Escape if near dock
        if (escapeAtDock && IsNearDock())
        {
            return true;
        }

        return false;
    }

    private bool IsNearDock()
    {
        // Check if player is near any dock (would need dock reference system)
        // For now, check if player is at origin (starter dock)
        if (playerTransform == null) return false;

        float distanceToOrigin = Vector3.Distance(playerTransform.position, Vector3.zero);
        return distanceToOrigin < dockSafeDistance;
    }

    private void SpawnCreature()
    {
        if (creaturePrefab == null)
        {
            if (enableDebugLogging)
            {
                Debug.LogWarning("[ChaseCreature] No creature prefab assigned");
            }
            return;
        }

        // Calculate spawn position (behind player)
        Vector3 spawnPos = CalculateSpawnPosition();

        // Spawn creature
        activeCreature = Instantiate(creaturePrefab, spawnPos, Quaternion.identity);

        // Add behavior component
        CreatureBehavior behavior = activeCreature.GetComponent<CreatureBehavior>();
        if (behavior == null)
        {
            behavior = activeCreature.AddComponent<CreatureBehavior>();
        }

        behavior.Initialize(playerTransform, this, baseSpeed, maxSpeed, catchUpAcceleration,
                          catchDistance, giveUpDistance, damagePerSecond, damageInterval);

        creatureActive = true;
        cooldownTimer = spawnCooldown;

        // Play roar sound
        if (creatureRoarSound != null)
        {
            audioSource.transform.position = spawnPos;
            audioSource.PlayOneShot(creatureRoarSound);
        }

        // Start chase music
        if (creatureChaseMusic != null)
        {
            musicSource.clip = creatureChaseMusic;
            musicSource.Play();
        }

        // Publish event
        EventSystem.Publish("ChaseCreatureSpawned", spawnPos);

        if (enableDebugLogging)
        {
            Debug.LogWarning($"[ChaseCreature] CREATURE SPAWNED at {spawnPos}! FLEE!");
        }
    }

    private void DespawnCreature()
    {
        if (activeCreature != null)
        {
            // Trigger retreat animation/behavior
            CreatureBehavior behavior = activeCreature.GetComponent<CreatureBehavior>();
            if (behavior != null)
            {
                behavior.Retreat();
            }
            else
            {
                Destroy(activeCreature);
            }

            activeCreature = null;
        }

        creatureActive = false;

        // Stop chase music
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
        }

        // Publish event
        EventSystem.Publish("ChaseCreatureEscaped");

        if (enableDebugLogging)
        {
            Debug.Log("[ChaseCreature] Creature retreated!");
        }
    }

    private Vector3 CalculateSpawnPosition()
    {
        // Spawn behind player
        Vector3 behindPlayer = -playerTransform.forward * spawnDistance;
        Vector3 spawnPos = playerTransform.position + behindPlayer;
        spawnPos.y = spawnDepth;

        return spawnPos;
    }

    /// <summary>
    /// Called when creature catches player
    /// </summary>
    public void OnCreatureCatch()
    {
        if (creatureAttackSound != null)
        {
            audioSource.PlayOneShot(creatureAttackSound);
        }

        EventSystem.Publish("CreatureAttacking");

        if (enableDebugLogging)
        {
            Debug.LogWarning("[ChaseCreature] CREATURE ATTACKING!");
        }
    }

    /// <summary>
    /// Called when creature gives up chase
    /// </summary>
    public void OnCreatureGiveUp()
    {
        DespawnCreature();
    }

    /// <summary>
    /// Force spawn creature (for testing)
    /// </summary>
    public void ForceSpawn()
    {
        if (!creatureActive && playerTransform != null)
        {
            cooldownTimer = 0f;
            SpawnCreature();
        }
    }

    private void OnSanityChanged(float sanity)
    {
        currentSanity = sanity;
    }

    private void OnTimeOfDayChanged(TimeOfDay newTime)
    {
        isNighttime = (newTime == TimeOfDay.Night);
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<float>("SanityChanged", OnSanityChanged);
        EventSystem.Unsubscribe<TimeOfDay>("TimeOfDayChanged", OnTimeOfDayChanged);

        if (_instance == this)
        {
            _instance = null;
        }
    }
}

/// <summary>
/// Individual creature AI behavior.
/// Pursues player with acceleration, damages on contact.
/// </summary>
public class CreatureBehavior : MonoBehaviour
{
    private Transform targetTransform;
    private ChaseCreature manager;

    private float currentSpeed;
    private float baseSpeed;
    private float maxSpeed;
    private float acceleration;
    private float catchDistance;
    private float giveUpDistance;
    private float damagePerSecond;
    private float damageInterval;

    private bool isCatching = false;
    private bool isRetreating = false;
    private float damageTimer = 0f;
    private float retreatSpeed = 20f;

    public void Initialize(Transform target, ChaseCreature managerRef, float baseSpd, float maxSpd,
                          float accel, float catchDist, float giveUpDist, float dmgPerSec, float dmgInterval)
    {
        targetTransform = target;
        manager = managerRef;
        baseSpeed = baseSpd;
        maxSpeed = maxSpd;
        currentSpeed = baseSpd;
        acceleration = accel;
        catchDistance = catchDist;
        giveUpDistance = giveUpDist;
        damagePerSecond = dmgPerSec;
        damageInterval = dmgInterval;
    }

    private void Update()
    {
        if (targetTransform == null || isRetreating)
        {
            if (isRetreating)
            {
                RetreatBehavior();
            }
            return;
        }

        float distance = Vector3.Distance(transform.position, targetTransform.position);

        // Give up if too far
        if (distance > giveUpDistance)
        {
            if (manager != null)
            {
                manager.OnCreatureGiveUp();
            }
            return;
        }

        // Chase behavior
        ChaseBehavior(distance);
    }

    private void ChaseBehavior(float distance)
    {
        // Accelerate over time
        currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);

        // Move toward player
        Vector3 direction = (targetTransform.position - transform.position).normalized;
        transform.position += direction * currentSpeed * Time.deltaTime;

        // Look at player
        transform.LookAt(targetTransform);

        // Check if caught player
        if (distance <= catchDistance)
        {
            if (!isCatching)
            {
                isCatching = true;
                if (manager != null)
                {
                    manager.OnCreatureCatch();
                }
            }

            // Deal damage over time
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                damageTimer = 0f;
                EventSystem.Publish("BoatDamaged", damagePerSecond * damageInterval);
            }
        }
        else
        {
            isCatching = false;
            damageTimer = 0f;
        }
    }

    private void RetreatBehavior()
    {
        // Sink below water
        Vector3 retreatDir = Vector3.down;
        transform.position += retreatDir * retreatSpeed * Time.deltaTime;

        // Destroy when deep enough
        if (transform.position.y < -100f)
        {
            Destroy(gameObject);
        }
    }

    public void Retreat()
    {
        isRetreating = true;
    }
}
