using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns spontaneous obstacles (rocks, debris) at low sanity.
/// Forces navigation skill and can damage boat.
/// Agent 7: Sanity & Horror System
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    private static ObstacleSpawner _instance;
    public static ObstacleSpawner Instance => _instance;

    [Header("Obstacle Prefabs")]
    [SerializeField] private GameObject[] rockPrefabs;
    [SerializeField] private GameObject[] debrisPrefabs;

    [Header("Spawn Configuration")]
    [SerializeField] private float sanityThreshold = 30f; // Only spawn below 30%
    [SerializeField] private float spawnChance = 0.1f; // 10% per check
    [SerializeField] private float spawnCheckInterval = 5f;
    [SerializeField] private int maxActiveObstacles = 10;

    [Header("Spawn Range")]
    [SerializeField] private float minSpawnDistance = 15f;
    [SerializeField] private float maxSpawnDistance = 40f;
    [SerializeField] private float spawnAngleAhead = 60f; // Spawn ahead of player direction

    [Header("Obstacle Properties")]
    [SerializeField] private float obstacleDamage = 10f;
    [SerializeField] private float obstacleLifetime = 30f; // Despawn after 30 seconds
    [SerializeField] private bool fadeInObstacles = true;
    [SerializeField] private float fadeInDuration = 2f;

    [Header("Active Obstacles")]
    private List<GameObject> activeObstacles = new List<GameObject>();

    [Header("Status")]
    [SerializeField] private float currentSanity = 100f;
    [SerializeField] private bool canSpawn = false;
    private float spawnTimer = 0f;

    [Header("Audio")]
    [SerializeField] private AudioClip obstacleAppearSound;
    [SerializeField] private AudioClip obstacleCollisionSound;
    private AudioSource audioSource;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool visualizeSpawnRange = false;

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
        audioSource.spatialBlend = 1f; // 3D sound
    }

    private void Start()
    {
        // Subscribe to sanity events
        EventSystem.Subscribe<float>("SanityChanged", OnSanityChanged);

        // Get player reference
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Initialize sanity
        if (SanityManager.Instance != null)
        {
            currentSanity = SanityManager.Instance.GetCurrentSanity();
            UpdateSpawnEligibility();
        }

        if (enableDebugLogging)
        {
            Debug.Log("[ObstacleSpawner] Initialized");
        }
    }

    private void Update()
    {
        if (!canSpawn || playerTransform == null) return;

        // Clean up null obstacles
        activeObstacles.RemoveAll(obj => obj == null);

        if (activeObstacles.Count >= maxActiveObstacles) return;

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnCheckInterval)
        {
            spawnTimer = 0f;
            CheckForSpawn();
        }
    }

    private void CheckForSpawn()
    {
        if (Random.value < spawnChance)
        {
            SpawnObstacle();
        }
    }

    private void SpawnObstacle()
    {
        // Choose random prefab
        GameObject[] prefabArray = Random.value < 0.7f ? rockPrefabs : debrisPrefabs;

        if (prefabArray == null || prefabArray.Length == 0)
        {
            if (enableDebugLogging)
            {
                Debug.LogWarning("[ObstacleSpawner] No obstacle prefabs assigned");
            }
            return;
        }

        GameObject prefab = prefabArray[Random.Range(0, prefabArray.Length)];

        if (prefab == null) return;

        // Calculate spawn position (ahead of player)
        Vector3 spawnPosition = CalculateSpawnPosition();

        // Spawn obstacle
        GameObject obstacle = Instantiate(prefab, spawnPosition, Random.rotation);
        obstacle.tag = "Obstacle";

        // Add obstacle behavior component
        ObstacleBehavior behavior = obstacle.AddComponent<ObstacleBehavior>();
        behavior.Initialize(obstacleDamage, obstacleLifetime, this);

        // Fade in if enabled
        if (fadeInObstacles)
        {
            behavior.FadeIn(fadeInDuration);
        }

        // Add to active list
        activeObstacles.Add(obstacle);

        // Play sound
        if (obstacleAppearSound != null)
        {
            audioSource.transform.position = spawnPosition;
            audioSource.PlayOneShot(obstacleAppearSound);
        }

        // Publish event
        EventSystem.Publish("ObstacleSpawned", spawnPosition);

        if (enableDebugLogging)
        {
            Debug.Log($"[ObstacleSpawner] Spawned obstacle at {spawnPosition}. Active: {activeObstacles.Count}");
        }
    }

    private Vector3 CalculateSpawnPosition()
    {
        // Get player forward direction
        Vector3 playerForward = playerTransform.forward;

        // Random angle within cone ahead of player
        float angle = Random.Range(-spawnAngleAhead, spawnAngleAhead);
        Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
        Vector3 direction = rotation * playerForward;

        // Random distance
        float distance = Random.Range(minSpawnDistance, maxSpawnDistance);

        // Calculate position
        Vector3 spawnPos = playerTransform.position + (direction * distance);

        // Set to water level (y = 0 or water surface)
        spawnPos.y = 0f;

        return spawnPos;
    }

    private void OnSanityChanged(float sanity)
    {
        currentSanity = sanity;
        UpdateSpawnEligibility();
    }

    private void UpdateSpawnEligibility()
    {
        bool previousCanSpawn = canSpawn;
        canSpawn = currentSanity < sanityThreshold;

        if (canSpawn && !previousCanSpawn && enableDebugLogging)
        {
            Debug.LogWarning("[ObstacleSpawner] Sanity critical! Obstacles will now spawn!");
        }
    }

    /// <summary>
    /// Called when obstacle collides with player
    /// </summary>
    public void OnObstacleCollision(GameObject obstacle)
    {
        if (obstacleCollisionSound != null)
        {
            audioSource.transform.position = obstacle.transform.position;
            audioSource.PlayOneShot(obstacleCollisionSound);
        }

        EventSystem.Publish("BoatDamaged", obstacleDamage);

        if (enableDebugLogging)
        {
            Debug.LogWarning($"[ObstacleSpawner] Boat hit obstacle! Damage: {obstacleDamage}");
        }
    }

    /// <summary>
    /// Remove obstacle from active list
    /// </summary>
    public void RemoveObstacle(GameObject obstacle)
    {
        activeObstacles.Remove(obstacle);
    }

    /// <summary>
    /// Clear all active obstacles
    /// </summary>
    public void ClearAllObstacles()
    {
        foreach (GameObject obstacle in activeObstacles)
        {
            if (obstacle != null)
            {
                Destroy(obstacle);
            }
        }

        activeObstacles.Clear();

        if (enableDebugLogging)
        {
            Debug.Log("[ObstacleSpawner] Cleared all obstacles");
        }
    }

    /// <summary>
    /// Force spawn an obstacle (for testing)
    /// </summary>
    public void ForceSpawnObstacle()
    {
        if (playerTransform != null)
        {
            SpawnObstacle();
        }
    }

    private void OnDrawGizmos()
    {
        if (!visualizeSpawnRange || playerTransform == null) return;

        Gizmos.color = Color.red;

        // Draw spawn range arc
        Vector3 forward = playerTransform.forward;
        Vector3 leftBound = Quaternion.Euler(0, -spawnAngleAhead, 0) * forward;
        Vector3 rightBound = Quaternion.Euler(0, spawnAngleAhead, 0) * forward;

        // Draw lines
        Gizmos.DrawLine(playerTransform.position, playerTransform.position + leftBound * maxSpawnDistance);
        Gizmos.DrawLine(playerTransform.position, playerTransform.position + rightBound * maxSpawnDistance);

        // Draw arc
        for (int i = 0; i < 10; i++)
        {
            float t1 = i / 10f;
            float t2 = (i + 1) / 10f;
            float angle1 = Mathf.Lerp(-spawnAngleAhead, spawnAngleAhead, t1);
            float angle2 = Mathf.Lerp(-spawnAngleAhead, spawnAngleAhead, t2);

            Vector3 dir1 = Quaternion.Euler(0, angle1, 0) * forward * maxSpawnDistance;
            Vector3 dir2 = Quaternion.Euler(0, angle2, 0) * forward * maxSpawnDistance;

            Gizmos.DrawLine(playerTransform.position + dir1, playerTransform.position + dir2);
        }
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<float>("SanityChanged", OnSanityChanged);

        if (_instance == this)
        {
            _instance = null;
        }
    }
}

/// <summary>
/// Behavior for individual obstacle instance.
/// Handles collision, lifetime, and fade effects.
/// </summary>
public class ObstacleBehavior : MonoBehaviour
{
    private float damage = 10f;
    private float lifetime = 30f;
    private float lifetimeTimer = 0f;
    private ObstacleSpawner spawner;
    private bool hasCollided = false;

    private Renderer[] renderers;
    private float fadeTimer = 0f;
    private float fadeDuration = 0f;
    private bool isFading = false;

    public void Initialize(float damageAmount, float lifetimeValue, ObstacleSpawner spawnerRef)
    {
        damage = damageAmount;
        lifetime = lifetimeValue;
        spawner = spawnerRef;

        // Get all renderers for fade effect
        renderers = GetComponentsInChildren<Renderer>();

        // Add collider if missing
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
        }

        // Add rigidbody for physics
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }
    }

    public void FadeIn(float duration)
    {
        isFading = true;
        fadeDuration = duration;
        fadeTimer = 0f;

        // Start transparent
        SetAlpha(0f);
    }

    private void Update()
    {
        // Handle fade in
        if (isFading)
        {
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Clamp01(fadeTimer / fadeDuration);
            SetAlpha(alpha);

            if (fadeTimer >= fadeDuration)
            {
                isFading = false;
            }
        }

        // Handle lifetime
        lifetimeTimer += Time.deltaTime;
        if (lifetimeTimer >= lifetime)
        {
            DestroyObstacle();
        }
    }

    private void SetAlpha(float alpha)
    {
        foreach (Renderer rend in renderers)
        {
            if (rend == null) continue;

            foreach (Material mat in rend.materials)
            {
                Color color = mat.color;
                color.a = alpha;
                mat.color = color;

                // Enable transparency
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasCollided) return;

        if (other.CompareTag("Player") || other.CompareTag("Boat"))
        {
            hasCollided = true;

            if (spawner != null)
            {
                spawner.OnObstacleCollision(gameObject);
            }

            // Don't destroy immediately - let player navigate around it
        }
    }

    private void DestroyObstacle()
    {
        if (spawner != null)
        {
            spawner.RemoveObstacle(gameObject);
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (spawner != null)
        {
            spawner.RemoveObstacle(gameObject);
        }
    }
}
