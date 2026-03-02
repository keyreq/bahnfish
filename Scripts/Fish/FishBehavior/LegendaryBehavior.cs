using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Agent 8: Fish AI & Behavior Agent - LegendaryBehavior.cs
/// Boss-level fish with unique mechanics and abilities.
/// Features: Special attacks, phases, summons, requires specific bait.
/// Trophy catches with longer fight sequences.
/// </summary>
public class LegendaryBehavior : MonoBehaviour, IFishBehavior
{
    [Header("Legendary Stats")]
    [SerializeField] private float baseSpeed = 4f;
    [SerializeField] private float powerMultiplier = 3f;
    [SerializeField] private int currentPhase = 1;
    [SerializeField] private int maxPhases = 3;

    [Header("Boss Mechanics")]
    [SerializeField] private float phaseTransitionStamina = 66f;
    [SerializeField] private bool canSummonMinions = true;
    [SerializeField] private int maxMinions = 3;
    [SerializeField] private float summonCooldown = 15f;
    [SerializeField] private GameObject minionPrefab;

    [Header("Special Abilities")]
    [SerializeField] private bool hasRageMode = true;
    [SerializeField] private float rageThreshold = 33f;
    [SerializeField] private bool canCreateVortex = true;
    [SerializeField] private float vortexCooldown = 20f;
    [SerializeField] private bool hasRegeneration = false;
    [SerializeField] private float regenRate = 2f;

    [Header("Advanced Movement")]
    [SerializeField] private bool useAdvancedAI = true;
    [SerializeField] private float chargeSpeed = 8f;
    [SerializeField] private float chargeCooldown = 10f;
    [SerializeField] private float circleDistance = 15f;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem auraEffect;
    [SerializeField] private Color phaseColor = Color.red;
    [SerializeField] private bool showPhaseTransition = true;

    private FishSpeciesData speciesData;
    private List<GameObject> summonedMinions = new List<GameObject>();
    private float summonTimer;
    private float vortexTimer;
    private float chargeTimer;
    private bool isCharging = false;
    private Vector3 chargeTarget;
    private bool isInRageMode = false;

    // Movement patterns
    private enum LegendaryPattern
    {
        Circling,
        Charging,
        Retreating,
        Summoning,
        VortexAttack
    }

    private LegendaryPattern currentPattern = LegendaryPattern.Circling;
    private float patternTimer;

    public void Initialize(FishSpeciesData data)
    {
        speciesData = data;
        baseSpeed *= data.speedMultiplier;
        currentPhase = 1;
        summonTimer = summonCooldown;
        vortexTimer = vortexCooldown;
        chargeTimer = chargeCooldown;
        patternTimer = Random.Range(5f, 10f);

        SetupVisualEffects();

        Debug.Log($"[LegendaryBehavior] Legendary fish {data.fishName} initialized with {maxPhases} phases");
    }

    private void SetupVisualEffects()
    {
        if (auraEffect == null && showPhaseTransition)
        {
            // Create particle effect for legendary aura
            GameObject auraObj = new GameObject("LegendaryAura");
            auraObj.transform.parent = transform;
            auraObj.transform.localPosition = Vector3.zero;

            auraEffect = auraObj.AddComponent<ParticleSystem>();
            var main = auraEffect.main;
            main.startColor = phaseColor;
            main.startSize = 2f;
            main.startSpeed = 1f;
            main.maxParticles = 50;
        }
    }

    public void UpdateMovement(FishAI fish, Rigidbody rb)
    {
        if (fish == null || rb == null) return;

        UpdatePhase(fish);
        UpdateAbilities(fish);
        UpdatePattern();

        Vector3 movement = CalculateLegendaryMovement(fish, rb);

        // Apply movement
        rb.velocity = Vector3.Lerp(rb.velocity, movement, Time.fixedDeltaTime * 3f);

        // Rotate towards movement
        if (rb.velocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(rb.velocity.normalized);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, 5f * Time.fixedDeltaTime);
        }
    }

    private void UpdatePhase(FishAI fish)
    {
        float staminaPercent = (fish.GetStamina() / speciesData.staminaDuration) * 100f;

        // Check for phase transition
        if (staminaPercent <= phaseTransitionStamina && currentPhase < maxPhases)
        {
            TransitionToNextPhase();
        }

        // Check for rage mode
        if (hasRageMode && staminaPercent <= rageThreshold && !isInRageMode)
        {
            EnterRageMode();
        }
    }

    private void TransitionToNextPhase()
    {
        currentPhase++;
        Debug.Log($"[LegendaryBehavior] Transitioning to Phase {currentPhase}!");

        // Reset timers for more aggressive behavior
        summonTimer = 0;
        vortexTimer = 0;
        chargeTimer = 0;

        // Visual effect
        if (showPhaseTransition && auraEffect != null)
        {
            auraEffect.Play();
        }

        // Increase power
        powerMultiplier *= 1.3f;
    }

    private void EnterRageMode()
    {
        isInRageMode = true;
        powerMultiplier *= 1.5f;
        Debug.Log("[LegendaryBehavior] RAGE MODE ACTIVATED!");

        // Change visual effects
        if (auraEffect != null)
        {
            var main = auraEffect.main;
            main.startColor = Color.red;
            auraEffect.Play();
        }
    }

    private void UpdateAbilities(FishAI fish)
    {
        if (!fish.IsHooked()) return;

        // Update cooldowns
        summonTimer -= Time.fixedDeltaTime;
        vortexTimer -= Time.fixedDeltaTime;
        chargeTimer -= Time.fixedDeltaTime;

        // Summon minions
        if (canSummonMinions && summonTimer <= 0 && summonedMinions.Count < maxMinions)
        {
            SummonMinion(fish);
            summonTimer = summonCooldown / currentPhase; // Faster summons in later phases
        }

        // Create vortex
        if (canCreateVortex && vortexTimer <= 0)
        {
            CreateVortex(fish);
            vortexTimer = vortexCooldown;
        }

        // Regeneration
        if (hasRegeneration)
        {
            // Legendary fish can regenerate stamina slowly
            // TODO: Integrate with FishAI stamina system
        }
    }

    private void UpdatePattern()
    {
        patternTimer -= Time.fixedDeltaTime;

        if (patternTimer <= 0)
        {
            // Choose new pattern
            int patternCount = System.Enum.GetValues(typeof(LegendaryPattern)).Length;
            currentPattern = (LegendaryPattern)Random.Range(0, patternCount);
            patternTimer = Random.Range(5f, 10f);

            Debug.Log($"[LegendaryBehavior] Switching to pattern: {currentPattern}");
        }
    }

    private Vector3 CalculateLegendaryMovement(FishAI fish, Rigidbody rb)
    {
        Vector3 movement = Vector3.zero;

        if (isCharging)
        {
            movement = ExecuteCharge(fish, rb);
        }
        else
        {
            movement = currentPattern switch
            {
                LegendaryPattern.Circling => CirclePlayer(fish),
                LegendaryPattern.Charging => InitiateCharge(fish),
                LegendaryPattern.Retreating => RetreatMovement(fish),
                LegendaryPattern.Summoning => SummonMovement(fish, rb),
                LegendaryPattern.VortexAttack => VortexMovement(fish, rb),
                _ => CirclePlayer(fish)
            };
        }

        // Apply power multiplier
        movement *= powerMultiplier;

        return movement;
    }

    private Vector3 CirclePlayer(FishAI fish)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return Vector3.zero;

        Vector3 toPlayer = player.transform.position - fish.transform.position;
        float distance = toPlayer.magnitude;

        // Maintain circle distance
        if (distance < circleDistance)
        {
            // Move away
            return -toPlayer.normalized * baseSpeed * 0.5f;
        }
        else
        {
            // Circle around player
            Vector3 perpendicular = Vector3.Cross(toPlayer, Vector3.up).normalized;
            return perpendicular * baseSpeed;
        }
    }

    private Vector3 InitiateCharge(FishAI fish)
    {
        if (chargeTimer > 0) return CirclePlayer(fish);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return Vector3.zero;

        isCharging = true;
        chargeTarget = player.transform.position;
        chargeTimer = chargeCooldown;

        Debug.Log("[LegendaryBehavior] CHARGING!");

        return Vector3.zero;
    }

    private Vector3 ExecuteCharge(FishAI fish, Rigidbody rb)
    {
        Vector3 toTarget = chargeTarget - fish.transform.position;

        if (toTarget.magnitude < 2f)
        {
            isCharging = false;
            return Vector3.zero;
        }

        return toTarget.normalized * chargeSpeed * powerMultiplier;
    }

    private Vector3 RetreatMovement(FishAI fish)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return Vector3.zero;

        Vector3 away = (fish.transform.position - player.transform.position).normalized;
        return away * baseSpeed * 1.5f;
    }

    private Vector3 SummonMovement(FishAI fish, Rigidbody rb)
    {
        // Stay relatively still while summoning
        return rb.velocity * 0.5f;
    }

    private Vector3 VortexMovement(FishAI fish, Rigidbody rb)
    {
        // Spiral movement while creating vortex
        float time = Time.time * 3f;
        Vector3 spiral = new Vector3(
            Mathf.Cos(time) * 5f,
            Mathf.Sin(time * 0.5f) * 2f,
            Mathf.Sin(time) * 5f
        );

        return spiral;
    }

    private void SummonMinion(FishAI fish)
    {
        Vector3 spawnPosition = fish.transform.position + Random.onUnitSphere * 5f;
        spawnPosition.y = Mathf.Min(spawnPosition.y, -2f); // Keep underwater

        // Spawn a common fish as minion
        FishManager fishManager = FishManager.Instance;
        if (fishManager != null)
        {
            GameObject minion = fishManager.SpawnFish(spawnPosition);
            if (minion != null)
            {
                summonedMinions.Add(minion);
                Debug.Log("[LegendaryBehavior] Summoned minion!");
            }
        }
    }

    private void CreateVortex(FishAI fish)
    {
        Debug.Log("[LegendaryBehavior] Creating vortex!");

        // TODO: Spawn vortex hazard (integrate with Agent 7 - Horror System)
        // For now, just log the event

        // Visual effect placeholder
        if (auraEffect != null)
        {
            auraEffect.Play();
        }
    }

    private void CleanupMinions()
    {
        foreach (GameObject minion in summonedMinions)
        {
            if (minion != null)
            {
                Destroy(minion);
            }
        }
        summonedMinions.Clear();
    }

    private void OnDrawGizmos()
    {
        // Draw circle distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, circleDistance);

        // Draw current pattern
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Vector3 labelPos = transform.position + Vector3.up * 3f;

            // Show phase
            Gizmos.DrawWireSphere(labelPos, currentPhase);

            // Show charge target
            if (isCharging)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, chargeTarget);
                Gizmos.DrawSphere(chargeTarget, 1f);
            }
        }

        // Show rage mode
        if (isInRageMode)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 5f);
        }
    }

    private void OnDestroy()
    {
        CleanupMinions();

        if (auraEffect != null)
        {
            Destroy(auraEffect.gameObject);
        }
    }
}
