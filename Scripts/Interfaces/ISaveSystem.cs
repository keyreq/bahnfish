/// <summary>
/// Interface for the save/load system.
/// Implement this interface to create custom save systems or extend the default one.
/// </summary>
public interface ISaveSystem
{
    /// <summary>
    /// Saves the current game state to persistent storage.
    /// </summary>
    void SaveGame();

    /// <summary>
    /// Loads the game state from persistent storage.
    /// </summary>
    void LoadGame();

    /// <summary>
    /// Checks if save data exists.
    /// </summary>
    /// <returns>True if save data is found, false otherwise</returns>
    bool HasSaveData();

    /// <summary>
    /// Deletes all save data. Use with caution.
    /// </summary>
    void DeleteSaveData();

    /// <summary>
    /// Gets the path where save data is stored.
    /// </summary>
    /// <returns>Full file path to save data</returns>
    string GetSavePath();

    /// <summary>
    /// Auto-saves the game at regular intervals.
    /// Should be called on a timer or at safe points.
    /// </summary>
    void AutoSave();
}
