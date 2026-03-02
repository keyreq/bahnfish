using UnityEngine;

/// <summary>
/// Agent 8: Fish AI & Behavior Agent - AberrantBehavior.cs
/// Erratic, unnatural movement for aberrant (mutated) fish.
/// Features: Phase through obstacles, glow effects, night-only spawns.
/// Supernatural and unpredictable behavior patterns.
/// </summary>
public class AberrantBehavior : MonoBehaviour, IFishBehavior
{
    [Header("Aberrant Movement")]
    [SerializeField] private float baseSpeed = 3f;
    [SerializeField] private float erraticMultiplier = 2.5f;
    [SerializeField] private float teleportChance = 0.02f;
    [SerializeField] private float teleportDistance = 10f;

    [Header("Supernatural Effects")]
    [SerializeField] private bool canPhase = true;
    [SerializeField] private float phaseInterval = 5f;
    [SerializeField] private float phaseDuration = 2f;
    [SerializeField] private bool isPhasing = false;

    [Header("Glow Effect")]
    [SerializeField] private bool enableGlow = true;
    [SerializeField] private Light glowLight;
    [SerializeField] private float glowIntensity = 2f;
    [SerializeField] private float glowPulseSpeed = 2f;

    [Header("Erratic Behavior")]
    [SerializeField] private float directionChangeInterval = 0.5f;
    [SerializeField] private float movementChaos = 0.7f;

    private FishSpeciesData speciesData;
    private Vector3 currentDirection;
    private float directionTimer;
    private float phaseTimer;
    private float teleportTimer;
    private Collider fishCollider;
    private Renderer fishRenderer;

    public void Initialize(FishSpeciesData data)
    {
        speciesData = data;
        baseSpeed *= data.speedMultiplier;
        currentDirection = Random.onUnitSphere;
        directionTimer = directionChangeInterval;
        phaseTimer = phaseInterval;
        teleportTimer = Random.Range(5f, 15f);

        fishCollider = GetComponent<Collider>();
        fishRenderer = GetComponentInChildren<Renderer>();

        SetupGlowEffect();
    }

    private void SetupGlowEffect()
    {
        if (!enableGlow) return;

        // Create glow light if it doesn't exist
        if (glowLight == null)
        {
            GameObject lightObj = new GameObject("AberrantGlow");
            lightObj.transform.parent = transform;
            lightObj.transform.localPosition = Vector3.zero;

            glowLight = lightObj.AddComponent<Light>();
            glowLight.type = LightType.Point;
            glowLight.range = 5f;
            glowLight.intensity = glowIntensity;
            glowLight.color = speciesData != null ? speciesData.aberrantGlowColor : Color.green;
        }
    }

    public void UpdateMovement(FishAI fish, Rigidbody rb)
    {
        if (fish == null || rb == null) return;

        UpdateErraticDirection();
        UpdatePhasing();
        UpdateTeleportation(fish, rb);
        UpdateGlowEffect();

        Vector3 movement = CalculateMovement(fish, rb);

        // Apply movement
        rb.velocity = movement;

        // Rotate erratically
        RotateErratically(rb);
    }

    private Vector3 CalculateMovement(FishAI fish, Rigidbody rb)
    {
        Vector3 baseMovement = Vector3.zero;

        switch (fish.GetState())
        {
            case FishState.Idle:
                baseMovement = ErraticFloat(rb);
                break;

            case FishState.Wandering:
                baseMovement = ErraticWander(fish, rb);
                break;

            case FishState.Approaching:
                baseMovement = ErraticApproach(fish, rb);
                break;

            case FishState.Fleeing:
                baseMovement = ErraticFlee(fish, rb);
                break;

            case FishState.Investigating:
                baseMovement = ErraticInvestigate(fish, rb);
                break;

            case FishState.Hooked:
                baseMovement = ErraticStruggle(fish, rb);
                break;
        }

        // Add chaos to movement
        baseMovement += Random.insideUnitSphere * movementChaos;

        return baseMovement;
    }

    private Vector3 ErraticFloat(Rigidbody rb)
    {
        // Random drifting with sudden jerks
        Vector3 drift = rb.velocity * 0.9f;
        if (Random.value < 0.1f)
        {
            drift += Random.onUnitSphere * baseSpeed;
        }
        return drift;
    }

    private Vector3 ErraticWander(FishAI fish, Rigidbody rb)
    {
        // Wander with sudden direction changes
        return currentDirection * baseSpeed * erraticMultiplier;
    }

    private Vector3 ErraticApproach(FishAI fish, Rigidbody rb)
    {
        GameObject bait = GameObject.FindGameObjectWithTag("Bait");
        if (bait != null)
        {
            Vector3 toBait = (bait.transform.position - fish.transform.position).normalized;
            // Spiral towards bait instead of direct approach
            Vector3 perpendicular = Vector3.Cross(toBait, Vector3.up).normalized;
            return (toBait + perpendicular * 0.5f).normalized * baseSpeed * erraticMultiplier;
        }
        return ErraticWander(fish, rb);
    }

    private Vector3 ErraticFlee(FishAI fish, Rigidbody rb)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 away = (fish.transform.position - player.transform.position).normalized;
            // Zigzag away
            Vector3 zigzag = Vector3.Cross(away, Vector3.up).normalized * Mathf.Sin(Time.time * 5f);
            return (away + zigzag).normalized * baseSpeed * erraticMultiplier * 1.5f;
        }
        return ErraticWander(fish, rb);
    }

    private Vector3 ErraticInvestigate(FishAI fish, Rigidbody rb)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Erratic circling with sudden direction reversals
            Vector3 toPlayer = player.transform.position - fish.transform.position;
            Vector3 perpendicular = Vector3.Cross(toPlayer, Vector3.up).normalized;
            float reversal = Mathf.Sign(Mathf.Sin(Time.time * 2f));
            return perpendicular * reversal * baseSpeed * erraticMultiplier;
        }
        return ErraticWander(fish, rb);
    }

    private Vector3 ErraticStruggle(FishAI fish, Rigidbody rb)
    {
        // Violent, unpredictable struggling
        float stamina = fish.GetStamina();
        float struggleIntensity = (stamina / 100f) * 2f;

        Vector3 struggle = new Vector3(
            Mathf.Sin(Time.time * 10f) * struggleIntensity,
            Mathf.Cos(Time.time * 7f) * struggleIntensity,
            Mathf.Sin(Time.time * 13f) * struggleIntensity
        );

        return struggle * baseSpeed * erraticMultiplier * 2f;
    }

    private void UpdateErraticDirection()
    {
        directionTimer -= Time.fixedDeltaTime;
        if (directionTimer <= 0)
        {
            currentDirection = Random.onUnitSphere;
            directionTimer = directionChangeInterval * Random.Range(0.5f, 2f);
        }
    }

    private void UpdatePhasing()
    {
        if (!canPhase) return;

        phaseTimer -= Time.fixedDeltaTime;

        if (phaseTimer <= 0)
        {
            if (!isPhasing)
            {
                StartPhase();
            }
            else
            {
                EndPhase();
            }
        }
    }

    private void StartPhase()
    {
        isPhasing = true;
        phaseTimer = phaseDuration;

        // Disable collision
        if (fishCollider != null)
        {
            fishCollider.enabled = false;
        }

        // Make semi-transparent
        if (fishRenderer != null)
        {
            foreach (Material mat in fishRenderer.materials)
            {
                Color color = mat.color;
                color.a = 0.3f;
                mat.color = color;
            }
        }

        Debug.Log("[AberrantBehavior] Fish phasing through obstacles");
    }

    private void EndPhase()
    {
        isPhasing = false;
        phaseTimer = phaseInterval;

        // Re-enable collision
        if (fishCollider != null)
        {
            fishCollider.enabled = true;
        }

        // Make opaque again
        if (fishRenderer != null)
        {
            foreach (Material mat in fishRenderer.materials)
            {
                Color color = mat.color;
                color.a = 1f;
                mat.color = color;
            }
        }
    }

    private void UpdateTeleportation(FishAI fish, Rigidbody rb)
    {
        teleportTimer -= Time.fixedDeltaTime;

        if (teleportTimer <= 0)
        {
            if (Random.value < teleportChance)
            {
                TeleportFish(fish, rb);
            }
            teleportTimer = Random.Range(5f, 15f);
        }
    }

    private void TeleportFish(FishAI fish, Rigidbody rb)
    {
        Vector3 randomOffset = Random.onUnitSphere * teleportDistance;
        Vector3 newPosition = fish.transform.position + randomOffset;

        // Clamp to water level (negative Y)
        newPosition.y = Mathf.Min(newPosition.y, -1f);

        fish.transform.position = newPosition;
        rb.velocity = Vector3.zero;

        Debug.Log("[AberrantBehavior] Fish teleported!");

        // Visual effect placeholder
        // TODO: Add particle effect or flash
    }

    private void UpdateGlowEffect()
    {
        if (!enableGlow || glowLight == null) return;

        // Pulse glow intensity
        float pulse = Mathf.Sin(Time.time * glowPulseSpeed) * 0.5f + 0.5f;
        glowLight.intensity = glowIntensity * (0.5f + pulse * 0.5f);
    }

    private void RotateErratically(Rigidbody rb)
    {
        // Add random torque for unnatural rotation
        Vector3 randomTorque = Random.insideUnitSphere * 10f;
        rb.AddTorque(randomTorque, ForceMode.Acceleration);

        // Dampen rotation to prevent spinning out of control
        rb.angularVelocity *= 0.95f;
    }

    private void OnDrawGizmos()
    {
        // Draw teleport range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, teleportDistance);

        // Draw current direction
        Gizmos.color = Color.yellow;
        if (Application.isPlaying)
        {
            Gizmos.DrawRay(transform.position, currentDirection * 3f);
        }

        // Show phasing state
        if (isPhasing)
        {
            Gizmos.color = new Color(1f, 0f, 1f, 0.3f);
            Gizmos.DrawSphere(transform.position, 1f);
        }
    }

    private void OnDestroy()
    {
        // Clean up glow light
        if (glowLight != null)
        {
            Destroy(glowLight.gameObject);
        }
    }
}
