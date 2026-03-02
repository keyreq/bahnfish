using UnityEngine;

/// <summary>
/// Agent 18: Photography Mode Specialist - FishInstance.cs
/// Helper component that should be attached to fish GameObjects in the scene.
/// Allows the photography system to identify and analyze fish in photos.
/// </summary>
public class FishInstance : MonoBehaviour
{
    [Header("Fish Data")]
    [Tooltip("Reference to the fish species data for this instance")]
    public FishSpeciesData speciesData;

    [Header("Instance Properties")]
    [Tooltip("Actual weight of this specific fish instance")]
    public float actualWeight;

    [Tooltip("Actual length of this specific fish instance")]
    public float actualLength;

    [Tooltip("Is this fish currently being photographed?")]
    public bool isBeingPhotographed = false;

    private void Awake()
    {
        // Ensure fish has proper tag for detection
        if (!gameObject.CompareTag("Fish"))
        {
            gameObject.tag = "Fish";
        }
    }

    /// <summary>
    /// Initializes the fish instance with species data.
    /// </summary>
    public void Initialize(FishSpeciesData species)
    {
        speciesData = species;

        if (species != null)
        {
            // Generate random weight and length from species ranges
            actualWeight = species.GetRandomWeight();
            actualLength = species.GetRandomLength();
        }
    }

    /// <summary>
    /// Gets the fish species ID.
    /// </summary>
    public string GetSpeciesID()
    {
        return speciesData != null ? speciesData.fishID : "unknown";
    }

    /// <summary>
    /// Gets the fish display name.
    /// </summary>
    public string GetDisplayName()
    {
        return speciesData != null ? speciesData.fishName : "Unknown Fish";
    }

    /// <summary>
    /// Gets the fish rarity.
    /// </summary>
    public FishRarity GetRarity()
    {
        return speciesData != null ? speciesData.rarity : FishRarity.Common;
    }

    /// <summary>
    /// Checks if fish is visible in camera view.
    /// </summary>
    public bool IsVisibleInCamera(Camera camera)
    {
        if (camera == null) return false;

        Vector3 viewportPoint = camera.WorldToViewportPoint(transform.position);

        // Check if in front of camera and within viewport bounds
        return viewportPoint.z > 0 &&
               viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
               viewportPoint.y >= 0 && viewportPoint.y <= 1;
    }

    /// <summary>
    /// Calculates how much of the screen this fish occupies.
    /// </summary>
    public float GetScreenCoverage(Camera camera)
    {
        if (camera == null) return 0f;

        // Get fish bounds
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null) return 0f;

        Bounds bounds = renderer.bounds;

        // Project bounds to screen space
        Vector3[] corners = new Vector3[8];
        corners[0] = camera.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.min.y, bounds.min.z));
        corners[1] = camera.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.min.y, bounds.max.z));
        corners[2] = camera.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.max.y, bounds.min.z));
        corners[3] = camera.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.max.y, bounds.max.z));
        corners[4] = camera.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.min.y, bounds.min.z));
        corners[5] = camera.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.min.y, bounds.max.z));
        corners[6] = camera.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.max.y, bounds.min.z));
        corners[7] = camera.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.max.y, bounds.max.z));

        // Find min/max screen coordinates
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (Vector3 corner in corners)
        {
            if (corner.z > 0) // Only consider points in front of camera
            {
                minX = Mathf.Min(minX, corner.x);
                maxX = Mathf.Max(maxX, corner.x);
                minY = Mathf.Min(minY, corner.y);
                maxY = Mathf.Max(maxY, corner.y);
            }
        }

        // Calculate screen area
        float screenWidth = maxX - minX;
        float screenHeight = maxY - minY;
        float fishScreenArea = screenWidth * screenHeight;

        // Calculate total screen area
        float totalScreenArea = Screen.width * Screen.height;

        // Return coverage percentage
        return Mathf.Clamp01(fishScreenArea / totalScreenArea);
    }

    /// <summary>
    /// Gets the screen position of the fish.
    /// </summary>
    public Vector2 GetScreenPosition(Camera camera)
    {
        if (camera == null) return Vector2.zero;

        Vector3 screenPoint = camera.WorldToScreenPoint(transform.position);
        return new Vector2(screenPoint.x, screenPoint.y);
    }

    /// <summary>
    /// Called when fish enters photo frame.
    /// </summary>
    public void OnEnterPhotoFrame()
    {
        isBeingPhotographed = true;
    }

    /// <summary>
    /// Called when fish exits photo frame.
    /// </summary>
    public void OnExitPhotoFrame()
    {
        isBeingPhotographed = false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Draw fish bounds
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(renderer.bounds.center, renderer.bounds.size);
        }

        // Draw species info
        if (speciesData != null)
        {
            UnityEditor.Handles.Label(transform.position + Vector3.up,
                $"{speciesData.fishName}\n{speciesData.rarity}\n{actualWeight:F1}kg");
        }
    }
#endif
}
