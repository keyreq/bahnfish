using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Controls the interactive cooking station on the boat or at the dock.
/// Manages player interaction, visual cooking process, and UI integration.
/// Can be placed as a component on cooking station GameObjects.
/// </summary>
public class CookingStationController : MonoBehaviour
{
    [Header("Station Settings")]
    [SerializeField] private StationType stationType = StationType.BoatCookingStation;
    [SerializeField] private float interactionDistance = 3f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject cookingFireEffect;
    [SerializeField] private GameObject smokeEffect;
    [SerializeField] private ParticleSystem sparkles;
    [SerializeField] private Light cookingLight;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip cookingSound;
    [SerializeField] private AudioClip completionSound;
    [SerializeField] private AudioClip failureSound;

    [Header("Animation")]
    [SerializeField] private Animator stationAnimator;
    [SerializeField] private string cookingAnimationTrigger = "StartCooking";
    [SerializeField] private string idleAnimationTrigger = "Idle";

    [Header("UI References")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private string interactionText = "Press E to Cook";

    [Header("Station State")]
    [SerializeField] private bool isPlayerNearby = false;
    [SerializeField] private bool isStationActive = false;

    // Properties
    public StationType Type => stationType;
    public bool IsActive => isStationActive;
    public bool IsPlayerNearby => isPlayerNearby;

    private Transform playerTransform;
    private RecipeData currentRecipe;

    private void Start()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Subscribe to cooking events
        if (CookingSystem.Instance != null)
        {
            CookingSystem.Instance.OnCookingStarted += OnCookingStarted;
            CookingSystem.Instance.OnCookingCompleted += OnCookingCompleted;
            CookingSystem.Instance.OnCookingFailed += OnCookingFailed;
        }

        // Initialize visual state
        SetCookingEffects(false);

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (CookingSystem.Instance != null)
        {
            CookingSystem.Instance.OnCookingStarted -= OnCookingStarted;
            CookingSystem.Instance.OnCookingCompleted -= OnCookingCompleted;
            CookingSystem.Instance.OnCookingFailed -= OnCookingFailed;
        }
    }

    private void Update()
    {
        CheckPlayerDistance();
        UpdateCookingVisuals();
        HandleInput();
    }

    /// <summary>
    /// Checks if player is within interaction distance.
    /// </summary>
    private void CheckPlayerDistance()
    {
        if (playerTransform == null)
            return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool wasNearby = isPlayerNearby;
        isPlayerNearby = distance <= interactionDistance;

        // Show/hide interaction prompt
        if (isPlayerNearby != wasNearby && interactionPrompt != null)
        {
            interactionPrompt.SetActive(isPlayerNearby && !isStationActive);
        }
    }

    /// <summary>
    /// Updates visual effects based on cooking state.
    /// </summary>
    private void UpdateCookingVisuals()
    {
        if (CookingSystem.Instance == null)
            return;

        bool isCooking = CookingSystem.Instance.IsCooking;

        if (isCooking != isStationActive)
        {
            isStationActive = isCooking;
            SetCookingEffects(isCooking);
        }

        // Update light intensity based on cooking progress
        if (isCooking && cookingLight != null && CookingSystem.Instance.CurrentCooking != null)
        {
            float progress = CookingSystem.Instance.CurrentCooking.GetProgress();
            cookingLight.intensity = Mathf.Lerp(0.5f, 2f, progress);
        }
    }

    /// <summary>
    /// Handles player input for interaction.
    /// </summary>
    private void HandleInput()
    {
        if (!isPlayerNearby || isStationActive)
            return;

        // Check for interaction input (E key or button)
        if (Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("Interact"))
        {
            OpenCookingMenu();
        }
    }

    /// <summary>
    /// Opens the cooking menu UI.
    /// </summary>
    public void OpenCookingMenu()
    {
        // Publish event to open cooking UI
        EventSystem.Publish("OpenCookingUI", this);

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    /// <summary>
    /// Starts cooking a recipe at this station.
    /// </summary>
    public bool StartCooking(RecipeData recipe)
    {
        if (recipe == null)
            return false;

        // Check if recipe requires special station
        if (recipe.requiresSpecialStation && !string.IsNullOrEmpty(recipe.requiredStation))
        {
            if (!IsCorrectStationType(recipe.requiredStation))
            {
                Debug.LogWarning($"[CookingStation] Recipe {recipe.recipeName} requires {recipe.requiredStation}");
                return false;
            }
        }

        // Start cooking through the system
        if (CookingSystem.Instance != null)
        {
            return CookingSystem.Instance.StartCooking(recipe.recipeID);
        }

        return false;
    }

    /// <summary>
    /// Checks if this station matches the required type.
    /// </summary>
    private bool IsCorrectStationType(string requiredStation)
    {
        return stationType.ToString().ToLower().Contains(requiredStation.ToLower());
    }

    /// <summary>
    /// Sets the cooking visual effects on/off.
    /// </summary>
    private void SetCookingEffects(bool active)
    {
        if (cookingFireEffect != null)
            cookingFireEffect.SetActive(active);

        if (smokeEffect != null)
            smokeEffect.SetActive(active);

        if (sparkles != null)
        {
            if (active)
                sparkles.Play();
            else
                sparkles.Stop();
        }

        if (cookingLight != null)
            cookingLight.enabled = active;

        // Play audio
        if (audioSource != null && cookingSound != null)
        {
            if (active)
            {
                audioSource.clip = cookingSound;
                audioSource.loop = true;
                audioSource.Play();
            }
            else
            {
                audioSource.Stop();
            }
        }

        // Trigger animation
        if (stationAnimator != null)
        {
            string trigger = active ? cookingAnimationTrigger : idleAnimationTrigger;
            stationAnimator.SetTrigger(trigger);
        }
    }

    // ===== Event Handlers =====

    private void OnCookingStarted(RecipeData recipe)
    {
        currentRecipe = recipe;
        Debug.Log($"[CookingStation] Started cooking {recipe.recipeName}");
    }

    private void OnCookingCompleted(RecipeData recipe, int quantity)
    {
        // Play completion effects
        if (sparkles != null)
            sparkles.Play();

        if (audioSource != null && completionSound != null)
        {
            audioSource.PlayOneShot(completionSound);
        }

        // Flash light
        if (cookingLight != null)
        {
            StartCoroutine(FlashLight());
        }

        Debug.Log($"[CookingStation] Completed cooking {recipe.recipeName}");
        currentRecipe = null;
    }

    private void OnCookingFailed(RecipeData recipe)
    {
        // Play failure effects
        if (audioSource != null && failureSound != null)
        {
            audioSource.PlayOneShot(failureSound);
        }

        Debug.Log($"[CookingStation] Failed to cook {recipe.recipeName}");
        currentRecipe = null;
    }

    /// <summary>
    /// Flashes the cooking light for visual feedback.
    /// </summary>
    private System.Collections.IEnumerator FlashLight()
    {
        if (cookingLight == null)
            yield break;

        float originalIntensity = cookingLight.intensity;

        for (int i = 0; i < 3; i++)
        {
            cookingLight.intensity = originalIntensity * 3f;
            yield return new WaitForSeconds(0.1f);
            cookingLight.intensity = originalIntensity;
            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// Gets available recipes for this station type.
    /// </summary>
    public List<RecipeData> GetAvailableRecipes()
    {
        if (CookingSystem.Instance == null)
            return new List<RecipeData>();

        List<RecipeData> available = new List<RecipeData>();

        // Get all unlocked recipes
        foreach (var recipe in CookingSystem.Instance.GetUnlockedRecipes())
        {
            // Check if recipe requires special station
            if (recipe.requiresSpecialStation && !string.IsNullOrEmpty(recipe.requiredStation))
            {
                if (IsCorrectStationType(recipe.requiredStation))
                {
                    available.Add(recipe);
                }
            }
            else
            {
                // Recipe can be made at any station
                available.Add(recipe);
            }
        }

        return available;
    }

    /// <summary>
    /// Gets the cooking progress as a percentage (0-1).
    /// </summary>
    public float GetCookingProgress()
    {
        if (CookingSystem.Instance == null || !CookingSystem.Instance.IsCooking)
            return 0f;

        return CookingSystem.Instance.CurrentCooking.GetProgress();
    }

    /// <summary>
    /// Gets the remaining cooking time in seconds.
    /// </summary>
    public float GetRemainingTime()
    {
        if (CookingSystem.Instance == null || !CookingSystem.Instance.IsCooking)
            return 0f;

        return CookingSystem.Instance.CurrentCooking.GetRemainingTime();
    }

    /// <summary>
    /// Cancels the current cooking operation.
    /// </summary>
    public void CancelCooking()
    {
        if (CookingSystem.Instance != null)
        {
            CookingSystem.Instance.CancelCooking();
        }
    }

    // ===== Gizmos =====

    private void OnDrawGizmosSelected()
    {
        // Draw interaction radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);

        // Draw station icon
        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
    }

    // ===== Debug Methods =====

    [ContextMenu("Debug: Test Start Cooking")]
    public void DebugTestCooking()
    {
        if (CookingSystem.Instance != null)
        {
            var recipes = GetAvailableRecipes();
            if (recipes.Count > 0)
            {
                StartCooking(recipes[0]);
                Debug.Log($"[CookingStation] Started test cooking: {recipes[0].recipeName}");
            }
            else
            {
                Debug.Log("[CookingStation] No available recipes");
            }
        }
    }

    [ContextMenu("Debug: Cancel Cooking")]
    public void DebugCancelCooking()
    {
        CancelCooking();
        Debug.Log("[CookingStation] Cancelled cooking");
    }
}

/// <summary>
/// Types of cooking stations.
/// </summary>
[System.Serializable]
public enum StationType
{
    BoatCookingStation,     // Basic cooking on the boat
    DockKitchen,            // Full kitchen at the dock
    Campfire,               // Campfire for basic recipes
    Smokehouse,             // For smoking fish
    PreservationStation     // For salting and freezing
}

/// <summary>
/// Data for station interaction events.
/// </summary>
[System.Serializable]
public class StationInteractionData
{
    public CookingStationController station;
    public StationType stationType;
    public Vector3 position;
}
