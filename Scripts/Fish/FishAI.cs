using UnityEngine;

/// <summary>
/// Agent 8: Fish AI & Behavior Agent - FishAI.cs
/// Controls individual fish behavior, movement, and interaction with fishing mechanics.
/// Delegates specific movement patterns to behavior components.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class FishAI : MonoBehaviour
{
    [Header("Fish Data")]
    [SerializeField] private FishSpeciesData speciesData;
    private Fish fishInstance;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float turnSpeed = 2f;
    [SerializeField] private float currentDepth = 5f;

    [Header("Behavior State")]
    [SerializeField] private FishState currentState = FishState.Idle;
    [SerializeField] private float energyLevel = 100f;
    [SerializeField] private float aggressionLevel = 0.5f;

    [Header("Detection")]
    [SerializeField] private float baitDetectionRadius = 10f;
    [SerializeField] private float playerDetectionRadius = 15f;
    [SerializeField] private LayerMask baitLayer;

    [Header("Hooked Behavior")]
    [SerializeField] private bool isHooked = false;
    [SerializeField] private float currentStamina = 100f;
    [SerializeField] private float staminaDrainRate = 5f;

    // Components
    private Rigidbody rb;
    private IFishBehavior behavior;

    // Events
    public System.Action OnFishDespawned;
    public System.Action<FishAI> OnFishHooked;
    public System.Action<FishAI> OnFishEscaped;
    public System.Action<FishAI> OnFishCaught;

    // Targeting
    private Transform currentTarget;
    private Vector3 wanderTarget;
    private float wanderTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // Fish float in water
        rb.drag = 2f; // Water resistance
    }

    private void Start()
    {
        InitializeBehavior();
        SetRandomWanderTarget();
    }

    private void Update()
    {
        UpdateState();
        UpdateBehavior();
        UpdateStamina();
    }

    private void FixedUpdate()
    {
        if (behavior != null)
        {
            behavior.UpdateMovement(this, rb);
        }
        else
        {
            DefaultMovement();
        }
    }

    /// <summary>
    /// Initializes the fish with species data.
    /// </summary>
    public void Initialize(FishSpeciesData data)
    {
        speciesData = data;
        fishInstance = data.ToFishInstance();

        // Set movement parameters from species data
        moveSpeed *= data.speedMultiplier;
        aggressionLevel = data.aggression;
        currentStamina = data.staminaDuration;

        // Set depth preference
        currentDepth = Random.Range(data.minDepth, data.maxDepth);

        InitializeBehavior();

        Debug.Log($"[FishAI] Initialized {data.fishName} at depth {currentDepth}m");
    }

    /// <summary>
    /// Initializes the appropriate behavior component based on species.
    /// </summary>
    private void InitializeBehavior()
    {
        if (speciesData == null) return;

        // Remove existing behavior
        if (behavior != null && behavior is Component component)
        {
            Destroy(component);
        }

        // Add appropriate behavior component
        switch (speciesData.behaviorType)
        {
            case FishBehaviorType.Normal:
                behavior = gameObject.AddComponent<NormalBehavior>();
                break;

            case FishBehaviorType.Aberrant:
                behavior = gameObject.AddComponent<AberrantBehavior>();
                break;

            case FishBehaviorType.Legendary:
                behavior = gameObject.AddComponent<LegendaryBehavior>();
                break;

            default:
                behavior = gameObject.AddComponent<NormalBehavior>();
                break;
        }

        behavior.Initialize(speciesData);
    }

    /// <summary>
    /// Updates the fish's AI state.
    /// </summary>
    private void UpdateState()
    {
        if (isHooked)
        {
            currentState = FishState.Hooked;
            return;
        }

        // Check for nearby bait
        Collider[] baits = Physics.OverlapSphere(transform.position, baitDetectionRadius, baitLayer);
        if (baits.Length > 0)
        {
            currentTarget = baits[0].transform;
            currentState = FishState.Approaching;
            return;
        }

        // Check for nearby player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float playerDistance = Vector3.Distance(transform.position, player.transform.position);
            if (playerDistance < playerDetectionRadius)
            {
                // Flee if low aggression, investigate if high aggression
                currentState = aggressionLevel < 0.3f ? FishState.Fleeing : FishState.Investigating;
                currentTarget = player.transform;
                return;
            }
        }

        // Default to wandering
        currentState = FishState.Wandering;
        currentTarget = null;
    }

    /// <summary>
    /// Updates behavior based on current state.
    /// </summary>
    private void UpdateBehavior()
    {
        switch (currentState)
        {
            case FishState.Idle:
                // Do nothing, just float
                break;

            case FishState.Wandering:
                UpdateWandering();
                break;

            case FishState.Approaching:
                // Move towards target (bait)
                break;

            case FishState.Fleeing:
                // Move away from player
                break;

            case FishState.Investigating:
                // Circle around player
                break;

            case FishState.Hooked:
                // Struggle behavior handled by fishing system
                break;
        }
    }

    /// <summary>
    /// Updates wandering behavior.
    /// </summary>
    private void UpdateWandering()
    {
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0)
        {
            SetRandomWanderTarget();
        }
    }

    /// <summary>
    /// Sets a random wander target within depth range.
    /// </summary>
    private void SetRandomWanderTarget()
    {
        float randomX = transform.position.x + Random.Range(-10f, 10f);
        float randomZ = transform.position.z + Random.Range(-10f, 10f);
        float depthVariation = Random.Range(-2f, 2f);

        wanderTarget = new Vector3(randomX, -(currentDepth + depthVariation), randomZ);
        wanderTimer = Random.Range(3f, 8f);
    }

    /// <summary>
    /// Default movement when no specific behavior is assigned.
    /// </summary>
    private void DefaultMovement()
    {
        Vector3 targetPosition = wanderTarget;

        if (currentTarget != null)
        {
            targetPosition = currentTarget.position;
        }

        // Move towards target
        Vector3 direction = (targetPosition - transform.position).normalized;
        rb.velocity = direction * moveSpeed;

        // Rotate towards movement direction
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        // Maintain depth
        MaintainDepth();
    }

    /// <summary>
    /// Keeps fish at preferred depth.
    /// </summary>
    private void MaintainDepth()
    {
        float targetY = -currentDepth;
        float currentY = transform.position.y;

        if (Mathf.Abs(currentY - targetY) > 0.5f)
        {
            float verticalForce = (targetY - currentY) * 2f;
            rb.AddForce(Vector3.up * verticalForce, ForceMode.Acceleration);
        }
    }

    /// <summary>
    /// Updates fish stamina when hooked.
    /// </summary>
    private void UpdateStamina()
    {
        if (!isHooked) return;

        currentStamina -= staminaDrainRate * Time.deltaTime;

        if (currentStamina <= 0)
        {
            currentStamina = 0;
            OnFishExhausted();
        }
    }

    /// <summary>
    /// Called when fish is hooked by player.
    /// </summary>
    public void OnHooked()
    {
        isHooked = true;
        currentStamina = speciesData != null ? speciesData.staminaDuration : 30f;
        OnFishHooked?.Invoke(this);
        Debug.Log($"[FishAI] Fish hooked: {speciesData?.fishName ?? "Unknown"}");
    }

    /// <summary>
    /// Called when fish escapes from hook.
    /// </summary>
    public void OnEscape()
    {
        isHooked = false;
        currentStamina = 100f;
        currentState = FishState.Fleeing;
        OnFishEscaped?.Invoke(this);
        Debug.Log($"[FishAI] Fish escaped!");
    }

    /// <summary>
    /// Called when fish is successfully caught.
    /// </summary>
    public void OnCaught()
    {
        OnFishCaught?.Invoke(this);
        Debug.Log($"[FishAI] Fish caught: {speciesData?.fishName ?? "Unknown"}");

        // Despawn this fish
        Despawn();
    }

    /// <summary>
    /// Called when fish runs out of stamina.
    /// </summary>
    private void OnFishExhausted()
    {
        // Fish is exhausted and easier to reel in
        Debug.Log($"[FishAI] Fish exhausted!");
    }

    /// <summary>
    /// Checks if fish prefers the given bait.
    /// </summary>
    public bool PrefersBait(BaitType bait)
    {
        return speciesData != null && speciesData.PrefersBait(bait);
    }

    /// <summary>
    /// Gets the bite chance multiplier for given bait.
    /// </summary>
    public float GetBiteChanceMultiplier(BaitType bait)
    {
        if (speciesData != null && speciesData.PrefersBait(bait))
        {
            return speciesData.baitPreferenceMultiplier;
        }
        return 1f;
    }

    /// <summary>
    /// Triggers a bite on bait (called by fishing system).
    /// </summary>
    public bool TryBite(BaitType bait)
    {
        if (isHooked) return false;

        float biteChance = 0.3f * GetBiteChanceMultiplier(bait);

        if (Random.value < biteChance)
        {
            OnHooked();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Despawns this fish.
    /// </summary>
    public void Despawn()
    {
        OnFishDespawned?.Invoke();
        Destroy(gameObject);
    }

    /// <summary>
    /// Gets the Fish instance data for this fish.
    /// </summary>
    public Fish GetFishData()
    {
        return fishInstance;
    }

    /// <summary>
    /// Gets the species data for this fish.
    /// </summary>
    public FishSpeciesData GetSpeciesData()
    {
        return speciesData;
    }

    public Vector3 GetWanderTarget() => wanderTarget;
    public FishState GetState() => currentState;
    public float GetStamina() => currentStamina;
    public bool IsHooked() => isHooked;

    private void OnDrawGizmos()
    {
        // Draw detection radii
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, baitDetectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);

        // Draw wander target
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, wanderTarget);
            Gizmos.DrawWireSphere(wanderTarget, 0.5f);
        }

        // Draw preferred depth
        if (speciesData != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 depthPos = transform.position;
            depthPos.y = -currentDepth;
            Gizmos.DrawWireSphere(depthPos, 1f);
        }
    }
}

/// <summary>
/// Possible states for fish AI.
/// </summary>
public enum FishState
{
    Idle,          // Not moving
    Wandering,     // Random movement
    Approaching,   // Moving towards bait
    Fleeing,       // Running from player
    Investigating, // Curious about player
    Hooked         // Caught on line
}

/// <summary>
/// Interface for fish behavior components.
/// </summary>
public interface IFishBehavior
{
    void Initialize(FishSpeciesData data);
    void UpdateMovement(FishAI fish, Rigidbody rb);
}
