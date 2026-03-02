using UnityEngine;

/// <summary>
/// Interface for objects that can be interacted with by the player.
/// Used for docks, NPCs, objects, etc.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Called when the player interacts with this object
    /// </summary>
    void Interact();

    /// <summary>
    /// Returns the text to display for the interaction prompt
    /// </summary>
    string GetInteractionPrompt();

    /// <summary>
    /// Returns whether this object can currently be interacted with
    /// </summary>
    bool CanInteract();

    /// <summary>
    /// Returns the maximum distance at which this object can be interacted with
    /// </summary>
    float GetInteractionRange();
}
