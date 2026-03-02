using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles player interaction with objects in the world.
/// Uses raycast to detect interactable objects (docks, NPCs, items).
/// Publishes OnInteractionTriggered events via EventSystem.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 5f;
    [SerializeField] private LayerMask interactableLayer = ~0;
    [SerializeField] private string interactButton = "Interact";

    [Header("Raycast Settings")]
    [SerializeField] private Transform raycastOrigin;
    [SerializeField] private bool useSphereCast = true;
    [SerializeField] private float sphereCastRadius = 1f;

    [Header("UI Feedback")]
    [SerializeField] private bool showInteractionPrompt = true;
    [SerializeField] private Text interactionPromptText;
    [SerializeField] private GameObject interactionPromptUI;

    [Header("Debug")]
    [SerializeField] private bool drawDebugRays = true;

    // State
    private IInteractable currentInteractable;
    private GameObject currentInteractableObject;
    private bool canInteract = true;

    private void Awake()
    {
        if (raycastOrigin == null)
        {
            raycastOrigin = transform;
        }

        // Try to find UI elements if not assigned
        if (showInteractionPrompt && interactionPromptUI == null)
        {
            // Will be created by UI Agent later
            Debug.Log("PlayerInteraction: No prompt UI assigned. Will create basic prompt.");
        }
    }

    private void Update()
    {
        if (!canInteract) return;

        // Detect interactable objects
        DetectInteractables();

        // Handle interaction input
        if (currentInteractable != null && InputManager.Instance.GetButtonDown(interactButton))
        {
            TriggerInteraction();
        }

        // Update UI
        UpdateInteractionUI();
    }

    private void DetectInteractables()
    {
        RaycastHit hit;
        bool hitSomething = false;

        Vector3 rayOrigin = raycastOrigin.position;
        Vector3 rayDirection = raycastOrigin.forward;

        if (useSphereCast)
        {
            // Use sphere cast for more forgiving detection
            hitSomething = Physics.SphereCast(rayOrigin, sphereCastRadius, rayDirection, out hit, interactionRange, interactableLayer);
        }
        else
        {
            // Use standard raycast
            hitSomething = Physics.Raycast(rayOrigin, rayDirection, out hit, interactionRange, interactableLayer);
        }

        // Debug visualization
        if (drawDebugRays)
        {
            Color rayColor = hitSomething ? Color.green : Color.red;
            Debug.DrawRay(rayOrigin, rayDirection * interactionRange, rayColor);
        }

        // Check if we hit an interactable
        if (hitSomething)
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null && interactable.CanInteract())
            {
                // Check if within interaction range
                float distance = Vector3.Distance(rayOrigin, hit.point);
                if (distance <= interactable.GetInteractionRange())
                {
                    SetCurrentInteractable(interactable, hit.collider.gameObject);
                    return;
                }
            }
        }

        // No valid interactable found
        ClearCurrentInteractable();
    }

    private void SetCurrentInteractable(IInteractable interactable, GameObject obj)
    {
        if (currentInteractable != interactable)
        {
            currentInteractable = interactable;
            currentInteractableObject = obj;

            // Publish event for other systems
            EventSystem.Publish("OnInteractableDetected", obj);
        }
    }

    private void ClearCurrentInteractable()
    {
        if (currentInteractable != null)
        {
            currentInteractable = null;
            currentInteractableObject = null;

            EventSystem.Publish("OnInteractableLost", (GameObject)null);
        }
    }

    private void TriggerInteraction()
    {
        if (currentInteractable == null || !currentInteractable.CanInteract())
            return;

        // Call interact on the object
        currentInteractable.Interact();

        // Create event data
        float distance = Vector3.Distance(transform.position, currentInteractableObject.transform.position);
        InteractionEventData eventData = new InteractionEventData(
            currentInteractableObject,
            distance,
            currentInteractableObject.transform.position
        );

        // Publish interaction event
        EventSystem.Publish("OnInteractionTriggered", eventData);

        Debug.Log($"Interacted with: {currentInteractableObject.name}");
    }

    private void UpdateInteractionUI()
    {
        if (!showInteractionPrompt) return;

        if (currentInteractable != null && currentInteractable.CanInteract())
        {
            // Show prompt
            if (interactionPromptUI != null)
            {
                interactionPromptUI.SetActive(true);
            }

            // Update prompt text
            if (interactionPromptText != null)
            {
                string promptText = currentInteractable.GetInteractionPrompt();
                interactionPromptText.text = $"[E] {promptText}";
            }
        }
        else
        {
            // Hide prompt
            if (interactionPromptUI != null)
            {
                interactionPromptUI.SetActive(false);
            }
        }
    }

    #region Public API

    /// <summary>
    /// Get the currently detected interactable
    /// </summary>
    public IInteractable GetCurrentInteractable()
    {
        return currentInteractable;
    }

    /// <summary>
    /// Get the GameObject of current interactable
    /// </summary>
    public GameObject GetCurrentInteractableObject()
    {
        return currentInteractableObject;
    }

    /// <summary>
    /// Check if player can currently interact
    /// </summary>
    public bool CanInteract()
    {
        return canInteract && currentInteractable != null && currentInteractable.CanInteract();
    }

    /// <summary>
    /// Enable or disable interaction
    /// </summary>
    public void SetInteractionEnabled(bool enabled)
    {
        canInteract = enabled;

        if (!enabled)
        {
            ClearCurrentInteractable();
        }
    }

    /// <summary>
    /// Set interaction range
    /// </summary>
    public void SetInteractionRange(float range)
    {
        interactionRange = range;
    }

    /// <summary>
    /// Manually trigger interaction (for UI buttons, etc.)
    /// </summary>
    public void ManualInteract()
    {
        TriggerInteraction();
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        if (raycastOrigin == null) return;

        // Draw interaction range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(raycastOrigin.position, interactionRange);

        // Draw forward ray
        Gizmos.color = currentInteractable != null ? Color.green : Color.red;
        Gizmos.DrawRay(raycastOrigin.position, raycastOrigin.forward * interactionRange);

        // Draw sphere cast radius if enabled
        if (useSphereCast)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(raycastOrigin.position + raycastOrigin.forward * interactionRange, sphereCastRadius);
        }

        // Draw current interactable
        if (currentInteractableObject != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, currentInteractableObject.transform.position);
            Gizmos.DrawWireSphere(currentInteractableObject.transform.position, 0.5f);
        }
    }
}
