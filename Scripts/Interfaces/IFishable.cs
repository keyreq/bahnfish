/// <summary>
/// Interface for objects that can perform fishing actions.
/// Implement this on any component that needs fishing capabilities.
/// </summary>
public interface IFishable
{
    /// <summary>
    /// Initiates the fishing action.
    /// Called when the player attempts to start fishing.
    /// </summary>
    void StartFishing();

    /// <summary>
    /// Stops the current fishing action.
    /// Called when the player cancels or completes fishing.
    /// </summary>
    void StopFishing();

    /// <summary>
    /// Checks if the object is currently fishing.
    /// </summary>
    /// <returns>True if fishing is active, false otherwise</returns>
    bool IsFishing();

    /// <summary>
    /// Gets the current fishing tool being used.
    /// </summary>
    /// <returns>The active fishing tool, or null if not fishing</returns>
    object GetCurrentTool();
}
