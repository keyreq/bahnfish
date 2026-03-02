using UnityEngine;
using System.Collections;

/// <summary>
/// Agent 17: Crew & Companion Specialist - PetCompanion.cs
/// Pet companion AI with following behavior, idle animations, and petting interaction.
/// THE CORE PETTING FEATURE from Cast n Chill inspiration!
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PetCompanion : MonoBehaviour
{
    [Header("Pet Configuration")]
    [SerializeField] private PetData petData;
    [SerializeField] private string petID;

    [Header("Following Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float followDistanceMin = 3f;
    [SerializeField] private float followDistanceMax = 7f;
    [SerializeField] private float returnSpeed = 5f;
    [SerializeField] private float smoothTime = 0.3f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private float idleChangeInterval = 5f;

    [Header("Interaction")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private GameObject heartParticles;
    [SerializeField] private AudioSource audioSource;

    [Header("Status")]
    [SerializeField] private bool isActive = false;
    [SerializeField] private bool isBeingPetted = false;
    [SerializeField] private float currentLoyalty = 50f;
    [SerializeField] private PetState currentState = PetState.Idle;

    // Private fields
    private Rigidbody rb;
    private Vector3 velocity = Vector3.zero;
    private float lastIdleChangeTime = 0f;
    private LoyaltySystem loyaltySystem;
    private CompanionAbilitySystem abilitySystem;
    private bool canBePetted = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // Pet floats/swims
        rb.isKinematic = false;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // 3D sound
        }
    }

    private void Start()
    {
        loyaltySystem = LoyaltySystem.Instance;
        abilitySystem = CompanionAbilitySystem.Instance;

        // Subscribe to events
        EventSystem.Subscribe<string>("PetPetted", OnPetPetted);
        EventSystem.Subscribe<LoyaltyChangedEventData>("LoyaltyChanged", OnLoyaltyChanged);

        if (playerTransform == null)
        {
            // Try to find player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        Initialize();
    }

    /// <summary>
    /// Initializes the pet companion
    /// </summary>
    private void Initialize()
    {
        if (petData == null)
        {
            Debug.LogError($"[PetCompanion] Pet data not assigned for {gameObject.name}!");
            return;
        }

        petID = petData.petID;
        followDistanceMin = petData.followDistance - 2f;
        followDistanceMax = petData.followDistance + 2f;

        // Get loyalty from system
        if (loyaltySystem != null)
        {
            currentLoyalty = loyaltySystem.GetLoyalty(petID);
        }

        // Activate passive ability
        if (abilitySystem != null)
        {
            abilitySystem.ActivatePetPassiveAbility(petData, currentLoyalty);
        }

        isActive = true;
        SetState(PetState.Following);

        Debug.Log($"[PetCompanion] {petData.petName} initialized with loyalty: {currentLoyalty:F1}%");
    }

    private void Update()
    {
        if (!isActive || playerTransform == null) return;

        UpdateBehavior();
        UpdateAnimation();
        CheckPlayerProximity();
    }

    /// <summary>
    /// Updates pet behavior based on current state
    /// </summary>
    private void UpdateBehavior()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        switch (currentState)
        {
            case PetState.Idle:
                HandleIdleState(distanceToPlayer);
                break;

            case PetState.Following:
                HandleFollowingState(distanceToPlayer);
                break;

            case PetState.BeingPetted:
                // Do nothing, locked in place
                break;

            case PetState.Playing:
                HandlePlayingState();
                break;
        }
    }

    /// <summary>
    /// Handles idle state behavior
    /// </summary>
    private void HandleIdleState(float distanceToPlayer)
    {
        // Random idle animations
        if (Time.time >= lastIdleChangeTime + idleChangeInterval)
        {
            PlayRandomIdleAnimation();
            lastIdleChangeTime = Time.time;
        }

        // Return to following if player moves too far
        if (distanceToPlayer > followDistanceMax)
        {
            SetState(PetState.Following);
        }
    }

    /// <summary>
    /// Handles following state behavior
    /// </summary>
    private void HandleFollowingState(float distanceToPlayer)
    {
        if (distanceToPlayer > followDistanceMin)
        {
            // Move towards player
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            Vector3 targetPosition = playerTransform.position - (directionToPlayer * followDistanceMin);

            // Smooth movement
            Vector3 newPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime, returnSpeed * petData.moveSpeed);
            rb.MovePosition(newPosition);

            // Face movement direction
            if (velocity.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }
        else
        {
            // Close enough, switch to idle
            SetState(PetState.Idle);
        }
    }

    /// <summary>
    /// Handles playing state behavior
    /// </summary>
    private void HandlePlayingState()
    {
        // Play state would have fetch/play animations
        // For now, just return to idle after a short time
    }

    /// <summary>
    /// Updates animation based on current state
    /// </summary>
    private void UpdateAnimation()
    {
        if (animator == null) return;

        switch (currentState)
        {
            case PetState.Idle:
                animator.Play(petData.idleAnimationName);
                break;

            case PetState.Following:
                animator.Play(petData.movingAnimationName);
                break;

            case PetState.BeingPetted:
                animator.Play(petData.pettingAnimationName);
                break;
        }
    }

    /// <summary>
    /// Plays a random idle animation variant
    /// </summary>
    private void PlayRandomIdleAnimation()
    {
        if (animator == null) return;

        // Could have multiple idle variants
        int randomVariant = Random.Range(0, 3);
        string animName = $"{petData.idleAnimationName}_{randomVariant}";

        // Fallback to default if variant doesn't exist
        if (animator.HasState(0, Animator.StringToHash(animName)))
        {
            animator.Play(animName);
        }
        else
        {
            animator.Play(petData.idleAnimationName);
        }

        // Random vocal sound
        if (petData.vocalSounds.Length > 0 && Random.value < 0.3f)
        {
            PlaySound(petData.vocalSounds[Random.Range(0, petData.vocalSounds.Length)]);
        }
    }

    /// <summary>
    /// Checks if player is close enough to interact
    /// </summary>
    private void CheckPlayerProximity()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= interactionRange && canBePetted)
        {
            // Show interaction prompt
            EventSystem.Publish("ShowInteractionPrompt", $"Pet {petData.petName} [E]");
        }
    }

    /// <summary>
    /// Called when player interacts with pet (petting)
    /// </summary>
    public void OnPlayerInteract()
    {
        if (!canBePetted || isBeingPetted) return;

        if (loyaltySystem != null && loyaltySystem.CanPetNow(petID, petData))
        {
            StartCoroutine(PettingCoroutine());
        }
    }

    /// <summary>
    /// Petting interaction coroutine - THE KEY FEATURE!
    /// </summary>
    private IEnumerator PettingCoroutine()
    {
        isBeingPetted = true;
        canBePetted = false;
        SetState(PetState.BeingPetted);

        // Stop movement
        rb.velocity = Vector3.zero;

        // Play petting animation
        if (animator != null)
        {
            animator.Play(petData.pettingAnimationName);
        }

        // Play petting sound
        if (petData.pettingSound != null)
        {
            PlaySound(petData.pettingSound);
        }

        // Spawn heart particles
        if (heartParticles != null || petData.pettingParticles != null)
        {
            GameObject particles = petData.pettingParticles != null ? petData.pettingParticles : heartParticles;
            Instantiate(particles, transform.position + Vector3.up, Quaternion.identity);
        }

        // Call loyalty system
        if (loyaltySystem != null)
        {
            bool success = loyaltySystem.PetCompanion(petID, petData);
            if (success)
            {
                EventSystem.Publish("ShowNotification", $"{petData.petName} loved that!");
            }
        }

        // Wait for animation to finish (approximately 2 seconds)
        yield return new WaitForSeconds(2f);

        // Return to normal behavior
        isBeingPetted = false;
        SetState(PetState.Idle);

        // Cooldown before can be petted again
        yield return new WaitForSeconds(petData.pettingCooldown);
        canBePetted = true;
    }

    /// <summary>
    /// Activates pet's active ability
    /// </summary>
    public bool ActivateAbility()
    {
        if (abilitySystem != null && loyaltySystem != null)
        {
            float loyalty = loyaltySystem.GetLoyalty(petID);
            return abilitySystem.ActivatePetActiveAbility(petID, petData, loyalty);
        }
        return false;
    }

    /// <summary>
    /// Feeds the pet
    /// </summary>
    public bool Feed()
    {
        if (loyaltySystem != null)
        {
            bool success = loyaltySystem.FeedPet(petID, petData);
            if (success)
            {
                // Play eating animation
                if (animator != null)
                {
                    animator.Play("Eat");
                }

                // Play happy sound
                if (petData.vocalSounds.Length > 0)
                {
                    PlaySound(petData.vocalSounds[Random.Range(0, petData.vocalSounds.Length)]);
                }
            }
            return success;
        }
        return false;
    }

    /// <summary>
    /// Plays with the pet
    /// </summary>
    public bool Play()
    {
        if (loyaltySystem != null)
        {
            bool success = loyaltySystem.PlayWithPet(petID, petData);
            if (success)
            {
                StartCoroutine(PlayCoroutine());
            }
            return success;
        }
        return false;
    }

    /// <summary>
    /// Play interaction coroutine
    /// </summary>
    private IEnumerator PlayCoroutine()
    {
        SetState(PetState.Playing);

        // Play animation
        if (animator != null)
        {
            animator.Play("Play");
        }

        // Play happy sounds
        for (int i = 0; i < 3; i++)
        {
            if (petData.vocalSounds.Length > 0)
            {
                PlaySound(petData.vocalSounds[Random.Range(0, petData.vocalSounds.Length)]);
            }
            yield return new WaitForSeconds(1f);
        }

        SetState(PetState.Idle);
    }

    /// <summary>
    /// Sets the pet's current state
    /// </summary>
    private void SetState(PetState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            OnStateChanged(newState);
        }
    }

    /// <summary>
    /// Called when state changes
    /// </summary>
    private void OnStateChanged(PetState newState)
    {
        switch (newState)
        {
            case PetState.Idle:
                rb.velocity = Vector3.zero;
                lastIdleChangeTime = Time.time;
                break;

            case PetState.Following:
                break;

            case PetState.BeingPetted:
                rb.velocity = Vector3.zero;
                break;

            case PetState.Playing:
                break;
        }
    }

    /// <summary>
    /// Plays a sound
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Deactivates the pet
    /// </summary>
    public void Deactivate()
    {
        isActive = false;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Activates the pet
    /// </summary>
    public void Activate()
    {
        gameObject.SetActive(true);
        isActive = true;
        SetState(PetState.Following);
    }

    #region Event Handlers

    private void OnPetPetted(string pettedPetID)
    {
        if (pettedPetID == petID)
        {
            // This pet was petted
        }
    }

    private void OnLoyaltyChanged(LoyaltyChangedEventData data)
    {
        if (data.petID == petID)
        {
            currentLoyalty = data.newLoyalty;

            // Update abilities based on new loyalty
            if (abilitySystem != null)
            {
                abilitySystem.ActivatePetPassiveAbility(petData, currentLoyalty);
            }
        }
    }

    #endregion

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<string>("PetPetted", OnPetPetted);
        EventSystem.Unsubscribe<LoyaltyChangedEventData>("LoyaltyChanged", OnLoyaltyChanged);
    }

    #region Public Accessors

    public PetData GetPetData() => petData;
    public string GetPetID() => petID;
    public float GetCurrentLoyalty() => currentLoyalty;
    public bool IsActive() => isActive;
    public PetState GetCurrentState() => currentState;

    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Draw follow range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, followDistanceMin);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, followDistanceMax);

        // Draw interaction range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
#endif
}

/// <summary>
/// Pet behavior states
/// </summary>
public enum PetState
{
    Idle,
    Following,
    BeingPetted,
    Playing,
    Sleeping,
    Eating
}
