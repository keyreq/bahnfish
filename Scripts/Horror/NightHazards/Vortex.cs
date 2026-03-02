using UnityEngine;
using System.Collections;

/// <summary>
/// Swirling water hazard with tentacles that damages boat.
/// Spawns randomly at night as environmental danger.
/// Agent 7: Sanity & Horror System
/// </summary>
public class Vortex : MonoBehaviour
{
    [Header("Vortex Properties")]
    [SerializeField] private float damagePerSecond = 3f;
    [SerializeField] private float pullStrength = 5f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float innerRadius = 5f;
    [SerializeField] private float outerRadius = 15f;

    [Header("Lifetime")]
    [SerializeField] private float minLifetime = 30f;
    [SerializeField] private float maxLifetime = 60f;
    private float lifetime;
    private float lifetimeTimer = 0f;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem waterParticles;
    [SerializeField] private GameObject[] tentaclePrefabs;
    private GameObject[] activeTentacles;

    [Header("Audio")]
    [SerializeField] private AudioClip vortexAmbientSound;
    [SerializeField] private AudioClip tentacleAttackSound;
    private AudioSource audioSource;

    [Header("Status")]
    [SerializeField] private bool isActive = false;
    private bool isDamaging = false;

    [Header("Debug")]
    [SerializeField] private bool visualizeRadius = false;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 1f;
        audioSource.maxDistance = 100f;

        // Random lifetime
        lifetime = Random.Range(minLifetime, maxLifetime);
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        isActive = true;

        // Spawn tentacles
        if (tentaclePrefabs != null && tentaclePrefabs.Length > 0)
        {
            int tentacleCount = Random.Range(3, 6);
            activeTentacles = new GameObject[tentacleCount];

            for (int i = 0; i < tentacleCount; i++)
            {
                float angle = (360f / tentacleCount) * i;
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * innerRadius;
                Vector3 spawnPos = transform.position + offset;

                GameObject tentaclePrefab = tentaclePrefabs[Random.Range(0, tentaclePrefabs.Length)];
                GameObject tentacle = Instantiate(tentaclePrefab, spawnPos, Quaternion.identity, transform);

                // Make tentacles look at center
                tentacle.transform.LookAt(transform.position);

                activeTentacles[i] = tentacle;
            }
        }

        // Start particles
        if (waterParticles != null)
        {
            waterParticles.Play();
        }

        // Play ambient sound
        if (vortexAmbientSound != null)
        {
            audioSource.clip = vortexAmbientSound;
            audioSource.Play();
        }

        // Create trigger collider
        SphereCollider trigger = gameObject.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = outerRadius;

        EventSystem.Publish("VortexSpawned", transform.position);
    }

    private void Update()
    {
        if (!isActive) return;

        // Update lifetime
        lifetimeTimer += Time.deltaTime;
        if (lifetimeTimer >= lifetime)
        {
            Despawn();
            return;
        }

        // Rotate vortex
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Animate tentacles
        AnimateTentacles();
    }

    private void AnimateTentacles()
    {
        if (activeTentacles == null) return;

        float time = Time.time;

        foreach (GameObject tentacle in activeTentacles)
        {
            if (tentacle == null) continue;

            // Wave motion
            float wave = Mathf.Sin(time * 2f) * 0.5f;
            Vector3 scale = tentacle.transform.localScale;
            scale.y = 1f + wave;
            tentacle.transform.localScale = scale;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isActive) return;

        if (other.CompareTag("Player") || other.CompareTag("Boat"))
        {
            Transform target = other.transform;
            float distance = Vector3.Distance(transform.position, target.position);

            // Pull toward center
            if (distance > innerRadius)
            {
                Vector3 pullDirection = (transform.position - target.position).normalized;
                float pullFactor = Mathf.Clamp01(1f - (distance / outerRadius));
                Vector3 pullForce = pullDirection * pullStrength * pullFactor;

                // Apply force (if boat has rigidbody)
                Rigidbody rb = other.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce(pullForce, ForceMode.Force);
                }
            }

            // Deal damage if in inner radius
            if (distance <= innerRadius)
            {
                if (!isDamaging)
                {
                    isDamaging = true;
                    StartCoroutine(DamageCoroutine());
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Boat"))
        {
            isDamaging = false;
            StopCoroutine(DamageCoroutine());
        }
    }

    private IEnumerator DamageCoroutine()
    {
        while (isDamaging && isActive)
        {
            EventSystem.Publish("BoatDamaged", damagePerSecond);

            // Play attack sound
            if (tentacleAttackSound != null && !audioSource.isPlaying)
            {
                audioSource.PlayOneShot(tentacleAttackSound);
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private void Despawn()
    {
        isActive = false;

        // Stop effects
        if (waterParticles != null)
        {
            waterParticles.Stop();
        }

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        EventSystem.Publish("VortexDespawned", transform.position);

        // Fade out and destroy
        StartCoroutine(FadeOutAndDestroy());
    }

    private IEnumerator FadeOutAndDestroy()
    {
        float fadeTime = 2f;
        float elapsed = 0f;

        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / fadeTime);

            foreach (Renderer rend in renderers)
            {
                if (rend == null) continue;

                foreach (Material mat in rend.materials)
                {
                    Color color = mat.color;
                    color.a = alpha;
                    mat.color = color;
                }
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (!visualizeRadius) return;

        Gizmos.color = Color.blue;
        DrawCircle(transform.position, outerRadius);

        Gizmos.color = Color.red;
        DrawCircle(transform.position, innerRadius);
    }

    private void DrawCircle(Vector3 center, float radius)
    {
        int segments = 32;
        float angleStep = 360f / segments;

        Vector3 previousPoint = center + new Vector3(radius, 0f, 0f);

        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i;
            Vector3 newPoint = center + Quaternion.Euler(0f, angle, 0f) * new Vector3(radius, 0f, 0f);
            Gizmos.DrawLine(previousPoint, newPoint);
            previousPoint = newPoint;
        }
    }
}

/// <summary>
/// Spawner manager for vortexes at night.
/// </summary>
public class VortexSpawner : MonoBehaviour
{
    private static VortexSpawner _instance;
    public static VortexSpawner Instance => _instance;

    [Header("Vortex Prefab")]
    [SerializeField] private GameObject vortexPrefab;

    [Header("Spawn Configuration")]
    [SerializeField] private float spawnChance = 0.03f; // 3% per check
    [SerializeField] private float spawnCheckInterval = 15f;
    [SerializeField] private int maxActiveVortexes = 3;
    [SerializeField] private bool onlySpawnAtNight = true;

    [Header("Spawn Range")]
    [SerializeField] private float minSpawnDistance = 40f;
    [SerializeField] private float maxSpawnDistance = 100f;

    [Header("Status")]
    [SerializeField] private bool isNighttime = false;
    private float spawnTimer = 0f;
    private int activeVortexCount = 0;

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
    }

    private void Start()
    {
        EventSystem.Subscribe<TimeOfDay>("TimeOfDayChanged", OnTimeOfDayChanged);
        EventSystem.Subscribe<Vector3>("VortexSpawned", OnVortexSpawned);
        EventSystem.Subscribe<Vector3>("VortexDespawned", OnVortexDespawned);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        if (TimeManager.Instance != null)
        {
            isNighttime = TimeManager.Instance.IsNighttime();
        }
    }

    private void Update()
    {
        if (!CanSpawn()) return;

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnCheckInterval)
        {
            spawnTimer = 0f;

            if (Random.value < spawnChance)
            {
                SpawnVortex();
            }
        }
    }

    private bool CanSpawn()
    {
        if (playerTransform == null) return false;
        if (activeVortexCount >= maxActiveVortexes) return false;
        if (onlySpawnAtNight && !isNighttime) return false;
        if (vortexPrefab == null) return false;

        return true;
    }

    private void SpawnVortex()
    {
        // Random position around player
        float angle = Random.Range(0f, 360f);
        float distance = Random.Range(minSpawnDistance, maxSpawnDistance);

        Vector3 offset = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * distance;
        Vector3 spawnPos = playerTransform.position + offset;
        spawnPos.y = 0f; // Water surface

        Instantiate(vortexPrefab, spawnPos, Quaternion.identity);

        if (enableDebugLogging)
        {
            Debug.Log($"[VortexSpawner] Spawned vortex at {spawnPos}");
        }
    }

    public void ForceSpawn()
    {
        if (playerTransform != null && vortexPrefab != null)
        {
            SpawnVortex();
        }
    }

    private void OnVortexSpawned(Vector3 position)
    {
        activeVortexCount++;
    }

    private void OnVortexDespawned(Vector3 position)
    {
        activeVortexCount = Mathf.Max(0, activeVortexCount - 1);
    }

    private void OnTimeOfDayChanged(TimeOfDay newTime)
    {
        isNighttime = (newTime == TimeOfDay.Night);
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<TimeOfDay>("TimeOfDayChanged", OnTimeOfDayChanged);
        EventSystem.Unsubscribe<Vector3>("VortexSpawned", OnVortexSpawned);
        EventSystem.Unsubscribe<Vector3>("VortexDespawned", OnVortexDespawned);

        if (_instance == this)
        {
            _instance = null;
        }
    }
}
