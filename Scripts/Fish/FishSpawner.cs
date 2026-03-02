using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Agent 8: Fish AI & Behavior Agent - FishSpawner.cs
/// Manages spawn points and automatic fish spawning with timers.
/// Spawns fish every 5-10 seconds based on conditions.
/// </summary>
public class FishSpawner : MonoBehaviour
{
    [Header("Spawn Point Configuration")]
    [Tooltip("Spawn points where fish can appear")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    [Tooltip("If no spawn points assigned, spawn in area around this spawner")]
    [SerializeField] private bool useAreaSpawning = true;

    [Tooltip("Radius for area-based spawning")]
    [SerializeField] private float spawnRadius = 20f;

    [Header("Spawn Timing")]
    [Tooltip("Minimum time between spawns (seconds)")]
    [SerializeField] private float minSpawnInterval = 5f;

    [Tooltip("Maximum time between spawns (seconds)")]
    [SerializeField] private float maxSpawnInterval = 10f;

    [Tooltip("Enable automatic spawning")]
    [SerializeField] private bool autoSpawn = true;

    [Header("Spawn Conditions")]
    [Tooltip("Minimum depth for spawning fish")]
    [SerializeField] private float minDepth = 0f;

    [Tooltip("Maximum depth for spawning fish")]
    [SerializeField] private float maxDepth = 20f;

    [Tooltip("Location ID for this spawner")]
    [SerializeField] private string locationID = "starter_lake";

    [Header("Spawn Limits")]
    [Tooltip("Maximum fish this spawner can create at once")]
    [SerializeField] private int maxLocalFish = 10;

    [Tooltip("Only spawn fish if player is within this distance")]
    [SerializeField] private float playerProximityRange = 50f;

    [Tooltip("Don't spawn if player is closer than this")]
    [SerializeField] private float minPlayerDistance = 5f;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;

    private FishManager fishManager;
    private float nextSpawnTime;
    private int fishSpawnedByThisSpawner = 0;
    private Transform playerTransform;

    private void Start()
    {
        fishManager = FishManager.Instance;

        if (fishManager == null)
        {
            Debug.LogError("[FishSpawner] FishManager not found! Spawner will not function.");
            enabled = false;
            return;
        }

        // Find player (mock for now, will integrate with Agent 2 later)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("[FishSpawner] Player not found. Using spawner position as reference.");
        }

        // Generate spawn points if using area spawning and no points defined
        if (useAreaSpawning && spawnPoints.Count == 0)
        {
            GenerateSpawnPoints();
        }

        ScheduleNextSpawn();
    }

    private void Update()
    {
        if (!autoSpawn) return;

        if (Time.time >= nextSpawnTime)
        {
            TrySpawnFish();
            ScheduleNextSpawn();
        }
    }

    /// <summary>
    /// Generates spawn points in a circle around the spawner.
    /// </summary>
    private void GenerateSpawnPoints()
    {
        int numPoints = 8;
        float angleStep = 360f / numPoints;

        for (int i = 0; i < numPoints; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = transform.position.x + Mathf.Cos(angle) * spawnRadius;
            float z = transform.position.z + Mathf.Sin(angle) * spawnRadius;

            GameObject spawnPoint = new GameObject($"SpawnPoint_{i}");
            spawnPoint.transform.position = new Vector3(x, transform.position.y, z);
            spawnPoint.transform.parent = transform;
            spawnPoints.Add(spawnPoint.transform);
        }

        Debug.Log($"[FishSpawner] Generated {numPoints} spawn points in radius {spawnRadius}m");
    }

    /// <summary>
    /// Schedules the next spawn attempt.
    /// </summary>
    private void ScheduleNextSpawn()
    {
        float interval = Random.Range(minSpawnInterval, maxSpawnInterval);
        nextSpawnTime = Time.time + interval;
    }

    /// <summary>
    /// Attempts to spawn a fish if conditions are met.
    /// </summary>
    private void TrySpawnFish()
    {
        // Check if we've reached local limit
        if (fishSpawnedByThisSpawner >= maxLocalFish)
        {
            return;
        }

        // Check player proximity
        if (!IsPlayerInRange())
        {
            return;
        }

        // Get spawn position
        Vector3 spawnPosition = GetRandomSpawnPosition();

        // Check if player is too close to spawn position
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(spawnPosition, playerTransform.position);
            if (distanceToPlayer < minPlayerDistance)
            {
                return; // Too close to player
            }
        }

        // Spawn fish through FishManager
        GameObject spawnedFish = fishManager.SpawnFish(
            position: spawnPosition,
            locationID: locationID
        );

        if (spawnedFish != null)
        {
            fishSpawnedByThisSpawner++;

            // Subscribe to fish despawn event to track count
            FishAI fishAI = spawnedFish.GetComponent<FishAI>();
            if (fishAI != null)
            {
                fishAI.OnFishDespawned += OnFishDespawned;
            }
        }
    }

    /// <summary>
    /// Called when a fish spawned by this spawner is despawned.
    /// </summary>
    private void OnFishDespawned()
    {
        fishSpawnedByThisSpawner = Mathf.Max(0, fishSpawnedByThisSpawner - 1);
    }

    /// <summary>
    /// Checks if player is within spawn range.
    /// </summary>
    private bool IsPlayerInRange()
    {
        if (playerTransform == null) return true; // If no player, always spawn

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        return distance <= playerProximityRange;
    }

    /// <summary>
    /// Gets a random spawn position from the spawn points.
    /// </summary>
    private Vector3 GetRandomSpawnPosition()
    {
        if (spawnPoints.Count == 0)
        {
            // Random position around spawner
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            return transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
        }

        // Pick random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        return spawnPoint.position;
    }

    /// <summary>
    /// Manually spawns a fish at this spawner.
    /// </summary>
    public GameObject SpawnFishManual()
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();
        return fishManager.SpawnFish(spawnPosition, locationID);
    }

    /// <summary>
    /// Enables or disables auto-spawning.
    /// </summary>
    public void SetAutoSpawn(bool enabled)
    {
        autoSpawn = enabled;
        if (enabled)
        {
            ScheduleNextSpawn();
        }
    }

    /// <summary>
    /// Updates the location ID for this spawner.
    /// </summary>
    public void SetLocationID(string newLocationID)
    {
        locationID = newLocationID;
    }

    /// <summary>
    /// Adds a spawn point to this spawner.
    /// </summary>
    public void AddSpawnPoint(Transform point)
    {
        if (!spawnPoints.Contains(point))
        {
            spawnPoints.Add(point);
        }
    }

    /// <summary>
    /// Removes a spawn point from this spawner.
    /// </summary>
    public void RemoveSpawnPoint(Transform point)
    {
        spawnPoints.Remove(point);
    }

    /// <summary>
    /// Clears all spawn points.
    /// </summary>
    public void ClearSpawnPoints()
    {
        spawnPoints.Clear();
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        // Draw spawn radius
        Gizmos.color = Color.cyan;
        DrawWireCircle(transform.position, spawnRadius, 32);

        // Draw player proximity range
        Gizmos.color = Color.yellow;
        DrawWireCircle(transform.position, playerProximityRange, 32);

        // Draw min player distance
        Gizmos.color = Color.red;
        DrawWireCircle(transform.position, minPlayerDistance, 16);

        // Draw spawn points
        if (spawnPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (Transform point in spawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 1f);
                }
            }
        }
    }

    private void DrawWireCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            );
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Spawn Fish Now")]
    private void SpawnFishNow()
    {
        if (Application.isPlaying)
        {
            SpawnFishManual();
        }
        else
        {
            Debug.LogWarning("[FishSpawner] Can only spawn fish in Play mode.");
        }
    }

    [ContextMenu("Generate Spawn Points")]
    private void GenerateSpawnPointsEditor()
    {
        // Clear existing spawn points
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        spawnPoints.Clear();

        GenerateSpawnPoints();
    }
#endif
}
