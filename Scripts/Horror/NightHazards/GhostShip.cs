using UnityEngine;
using System.Collections;

/// <summary>
/// Ethereal ghost ship that appears in fog and steals catch.
/// Creates atmospheric horror and risk of losing progress.
/// Agent 7: Sanity & Horror System
/// </summary>
public class GhostShip : MonoBehaviour
{
    [Header("Ghost Ship Properties")]
    [SerializeField] private float stealPercentage = 0.25f; // Steals 25% of catch
    [SerializeField] private float stealRadius = 20f;
    [SerializeField] private float appearDuration = 10f;
    [SerializeField] private float movementSpeed = 3f;

    [Header("Visual Effects")]
    [SerializeField] private float ghostAlpha = 0.5f;
    [SerializeField] private Color ghostColor = new Color(0.5f, 0.8f, 1f, 0.5f);
    [SerializeField] private ParticleSystem fogParticles;
    [SerializeField] private Light ghostLight;

    [Header("Audio")]
    [SerializeField] private AudioClip ghostShipAmbient;
    [SerializeField] private AudioClip stealSound;
    [SerializeField] private AudioClip laughSound;
    private AudioSource audioSource;

    [Header("Status")]
    [SerializeField] private bool isActive = false;
    [SerializeField] private bool hasStolen = false;
    private float appearTimer = 0f;

    [Header("Movement")]
    [SerializeField] private Vector3 driftDirection;
    private Vector3 startPosition;

    [Header("Debug")]
    [SerializeField] private bool visualizeRadius = false;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 1f;
        audioSource.maxDistance = 100f;

        startPosition = transform.position;
        driftDirection = Random.onUnitSphere;
        driftDirection.y = 0f;
        driftDirection.Normalize();
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        isActive = true;

        // Setup ghost appearance
        SetupGhostMaterials();

        // Start fog particles
        if (fogParticles != null)
        {
            fogParticles.Play();
        }

        // Setup ghost light
        if (ghostLight != null)
        {
            ghostLight.color = ghostColor;
            ghostLight.intensity = 2f;
            ghostLight.range = 30f;
        }

        // Play ambient sound
        if (ghostShipAmbient != null)
        {
            audioSource.clip = ghostShipAmbient;
            audioSource.Play();
        }

        // Create trigger collider
        SphereCollider trigger = gameObject.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = stealRadius;

        EventSystem.Publish("GhostShipAppeared", transform.position);

        Debug.Log("[GhostShip] Ghost ship appeared!");
    }

    private void SetupGhostMaterials()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer rend in renderers)
        {
            if (rend == null) continue;

            foreach (Material mat in rend.materials)
            {
                // Make transparent
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;

                // Set ghost color
                Color color = mat.color;
                color = ghostColor;
                mat.color = color;

                // Enable emission
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", ghostColor * 0.5f);
            }
        }
    }

    private void Update()
    {
        if (!isActive) return;

        // Update appear timer
        appearTimer += Time.deltaTime;

        if (appearTimer >= appearDuration)
        {
            Disappear();
            return;
        }

        // Drift slowly
        transform.position += driftDirection * movementSpeed * Time.deltaTime;

        // Pulse ghost light
        if (ghostLight != null)
        {
            float pulse = Mathf.Sin(Time.time * 2f) * 0.5f + 1.5f;
            ghostLight.intensity = pulse;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive || hasStolen) return;

        if (other.CompareTag("Player") || other.CompareTag("Boat"))
        {
            StealCatch();
        }
    }

    private void StealCatch()
    {
        hasStolen = true;

        // Play steal sound
        if (stealSound != null)
        {
            audioSource.PlayOneShot(stealSound);
        }

        // Play laugh
        if (laughSound != null)
        {
            audioSource.PlayOneShot(laughSound, 0.5f);
        }

        // Publish event to actually remove fish from inventory
        EventSystem.Publish("GhostShipStealCatch", stealPercentage);

        Debug.LogWarning($"[GhostShip] GHOST SHIP STOLE {stealPercentage * 100}% OF CATCH!");

        // Disappear shortly after stealing
        StartCoroutine(DisappearAfterDelay(2f));
    }

    private IEnumerator DisappearAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Disappear();
    }

    private void Disappear()
    {
        isActive = false;

        // Stop particles
        if (fogParticles != null)
        {
            fogParticles.Stop();
        }

        EventSystem.Publish("GhostShipDisappeared", transform.position);

        // Fade out and destroy
        StartCoroutine(FadeOutAndDestroy());
    }

    private IEnumerator FadeOutAndDestroy()
    {
        float fadeTime = 3f;
        float elapsed = 0f;

        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float alpha = ghostAlpha * (1f - (elapsed / fadeTime));

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

            // Fade light
            if (ghostLight != null)
            {
                ghostLight.intensity *= (1f - Time.deltaTime / fadeTime);
            }

            // Fade audio
            if (audioSource != null)
            {
                audioSource.volume *= (1f - Time.deltaTime / fadeTime);
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (!visualizeRadius) return;

        Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, stealRadius);
    }
}

/// <summary>
/// Spawner manager for ghost ships.
/// Only spawns in foggy conditions at night.
/// </summary>
public class GhostShipSpawner : MonoBehaviour
{
    private static GhostShipSpawner _instance;
    public static GhostShipSpawner Instance => _instance;

    [Header("Ghost Ship Prefab")]
    [SerializeField] private GameObject ghostShipPrefab;

    [Header("Spawn Configuration")]
    [SerializeField] private float spawnChance = 0.05f; // 5% per check
    [SerializeField] private float spawnCheckInterval = 20f;
    [SerializeField] private int maxActiveShips = 1;
    [SerializeField] private bool requireFog = true;
    [SerializeField] private bool requireNight = true;

    [Header("Spawn Range")]
    [SerializeField] private float spawnDistance = 60f;

    [Header("Status")]
    [SerializeField] private bool isNighttime = false;
    [SerializeField] private bool isFoggy = false;
    private float spawnTimer = 0f;
    private int activeShipCount = 0;

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
        EventSystem.Subscribe<WeatherType>("WeatherChanged", OnWeatherChanged);
        EventSystem.Subscribe<Vector3>("GhostShipAppeared", OnGhostShipAppeared);
        EventSystem.Subscribe<Vector3>("GhostShipDisappeared", OnGhostShipDisappeared);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Initialize state
        if (TimeManager.Instance != null)
        {
            isNighttime = TimeManager.Instance.IsNighttime();
        }

        if (WeatherSystem.Instance != null)
        {
            isFoggy = WeatherSystem.Instance.IsFoggy();
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
                SpawnGhostShip();
            }
        }
    }

    private bool CanSpawn()
    {
        if (playerTransform == null) return false;
        if (activeShipCount >= maxActiveShips) return false;
        if (ghostShipPrefab == null) return false;
        if (requireNight && !isNighttime) return false;
        if (requireFog && !isFoggy) return false;

        return true;
    }

    private void SpawnGhostShip()
    {
        // Spawn at random angle around player
        float angle = Random.Range(0f, 360f);
        Vector3 offset = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * spawnDistance;
        Vector3 spawnPos = playerTransform.position + offset;
        spawnPos.y = 0f; // Water surface

        Instantiate(ghostShipPrefab, spawnPos, Quaternion.Euler(0f, angle + 180f, 0f));

        if (enableDebugLogging)
        {
            Debug.Log($"[GhostShipSpawner] Spawned ghost ship at {spawnPos}");
        }
    }

    public void ForceSpawn()
    {
        if (playerTransform != null && ghostShipPrefab != null)
        {
            SpawnGhostShip();
        }
    }

    private void OnGhostShipAppeared(Vector3 position)
    {
        activeShipCount++;
    }

    private void OnGhostShipDisappeared(Vector3 position)
    {
        activeShipCount = Mathf.Max(0, activeShipCount - 1);
    }

    private void OnTimeOfDayChanged(TimeOfDay newTime)
    {
        isNighttime = (newTime == TimeOfDay.Night);
    }

    private void OnWeatherChanged(WeatherType newWeather)
    {
        isFoggy = (newWeather == WeatherType.Fog);
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<TimeOfDay>("TimeOfDayChanged", OnTimeOfDayChanged);
        EventSystem.Unsubscribe<WeatherType>("WeatherChanged", OnWeatherChanged);
        EventSystem.Unsubscribe<Vector3>("GhostShipAppeared", OnGhostShipAppeared);
        EventSystem.Unsubscribe<Vector3>("GhostShipDisappeared", OnGhostShipDisappeared);

        if (_instance == this)
        {
            _instance = null;
        }
    }
}
