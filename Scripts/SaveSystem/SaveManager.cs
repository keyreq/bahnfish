using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages game save and load operations with automatic backup and validation.
/// Implements the ISaveSystem interface with enhanced features.
/// Features:
/// - JSON serialization using Unity's JsonUtility
/// - Automatic backup system (keeps last 3 saves)
/// - Save data validation and corruption detection
/// - Auto-save with configurable interval
/// - Cloud save integration (via CloudSaveHandler)
/// - Event-based notifications for save/load operations
/// </summary>
public class SaveManager : MonoBehaviour, ISaveSystem
{
    private static SaveManager _instance;

    /// <summary>
    /// Singleton instance accessor.
    /// </summary>
    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SaveManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("SaveManager");
                    _instance = go.AddComponent<SaveManager>();
                }
            }
            return _instance;
        }
    }

    [Header("Save Settings")]
    [SerializeField] private string saveFileName = "savegame.json";
    [SerializeField] private bool prettyPrint = true;
    [SerializeField] private int maxBackups = 3;

    [Header("Auto-Save")]
    [SerializeField] private bool enableAutoSave = true;
    [SerializeField] private float autoSaveInterval = 300f; // 5 minutes in seconds

    [Header("Validation")]
    [SerializeField] private bool validateOnSave = true;
    [SerializeField] private bool validateOnLoad = true;

    [Header("Cloud Integration")]
    [SerializeField] private bool syncWithCloud = false;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;

    private string savePath;
    private string saveDirectory;
    private float autoSaveTimer;
    private SaveData currentSaveData;

    /// <summary>
    /// Initializes the save manager singleton.
    /// </summary>
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        InitializePaths();
        LogDebug($"Initialized. Save path: {savePath}");
    }

    private void Start()
    {
        // Subscribe to game events
        EventSystem.Subscribe<SaveData>("RequestSave", OnRequestSave);
        EventSystem.Subscribe("RequestLoad", OnRequestLoad);
    }

    private void Update()
    {
        // Auto-save timer
        if (enableAutoSave)
        {
            autoSaveTimer += Time.deltaTime;
            if (autoSaveTimer >= autoSaveInterval)
            {
                AutoSave();
                autoSaveTimer = 0f;
            }
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        EventSystem.Unsubscribe<SaveData>("RequestSave", OnRequestSave);
        EventSystem.Unsubscribe("RequestLoad", OnRequestLoad);

        if (_instance == this)
        {
            _instance = null;
        }
    }

    /// <summary>
    /// Initializes save file paths.
    /// </summary>
    private void InitializePaths()
    {
        saveDirectory = Application.persistentDataPath;
        savePath = Path.Combine(saveDirectory, saveFileName);

        // Ensure save directory exists
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }

        LogDebug($"Save directory: {saveDirectory}");
    }

    /// <summary>
    /// Saves the current game state to disk.
    /// Creates a backup before saving and validates the data.
    /// </summary>
    public void SaveGame()
    {
        try
        {
            LogDebug("Starting save operation...");

            // Create backup of existing save
            if (File.Exists(savePath))
            {
                CreateBackup();
            }

            // Gather save data from all systems
            SaveData data = GatherSaveData();

            // Validate save data before writing
            if (validateOnSave)
            {
                SaveValidator.ValidationResult validation = SaveValidator.ValidateSaveData(data);
                if (!validation.IsValid)
                {
                    Debug.LogError($"[SaveManager] Save validation failed:\n{validation.GetSummary()}");
                    EventSystem.Publish("SaveFailed", "Validation failed");
                    return;
                }

                if (validation.Warnings.Count > 0)
                {
                    LogDebug($"Save validation warnings:\n{validation.GetSummary()}");
                }
            }

            // Serialize to JSON
            string json = JsonUtility.ToJson(data, prettyPrint);

            // Write to file
            File.WriteAllText(savePath, json);

            currentSaveData = data;
            LogDebug("Game saved successfully");

            // Sync with cloud if enabled
            if (syncWithCloud && CloudSaveHandler.Instance != null)
            {
                CloudSaveHandler.Instance.UploadToCloud(savePath);
            }

            // Notify all systems that save is complete
            EventSystem.Publish("SaveComplete", data);
            EventSystem.Publish("OnSaveComplete", data);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to save game: {e.Message}\n{e.StackTrace}");
            EventSystem.Publish("SaveFailed", e.Message);
        }
    }

    /// <summary>
    /// Loads the game state from disk.
    /// Validates the data and falls back to backup if corrupted.
    /// </summary>
    public void LoadGame()
    {
        if (!HasSaveData())
        {
            Debug.LogWarning("[SaveManager] No save data found to load");
            EventSystem.Publish("LoadFailed", "No save data found");
            return;
        }

        try
        {
            LogDebug("Starting load operation...");

            // Check for newer cloud save
            if (syncWithCloud && CloudSaveHandler.Instance != null && CloudSaveHandler.Instance.HasNewerCloudSave())
            {
                CloudSaveHandler.Instance.DownloadFromCloud(savePath);
            }

            // Read file
            string json = File.ReadAllText(savePath);

            // Deserialize from JSON
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            // Validate save data
            if (validateOnLoad)
            {
                SaveValidator.ValidationResult validation = SaveValidator.ValidateSaveData(data);
                if (!validation.IsValid)
                {
                    Debug.LogError($"[SaveManager] Save data validation failed:\n{validation.GetSummary()}");

                    // Try to restore from backup
                    if (TryRestoreFromBackup(out data))
                    {
                        LogDebug("Successfully restored from backup");
                    }
                    else
                    {
                        Debug.LogError("[SaveManager] All backups are invalid");
                        EventSystem.Publish("LoadFailed", "Corrupted save file");
                        return;
                    }
                }
                else if (validation.Warnings.Count > 0)
                {
                    LogDebug($"Load validation warnings:\n{validation.GetSummary()}");
                }
            }

            // Apply data to all systems
            ApplySaveData(data);

            currentSaveData = data;
            LogDebug("Game loaded successfully");

            // Notify all systems that load is complete
            EventSystem.Publish("LoadComplete", data);
            EventSystem.Publish("OnLoadComplete", data);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to load game: {e.Message}\n{e.StackTrace}");
            EventSystem.Publish("LoadFailed", e.Message);
        }
    }

    /// <summary>
    /// Checks if save data exists on disk.
    /// </summary>
    /// <returns>True if save file exists</returns>
    public bool HasSaveData()
    {
        return File.Exists(savePath);
    }

    /// <summary>
    /// Deletes all save data including backups.
    /// </summary>
    public void DeleteSaveData()
    {
        try
        {
            // Delete main save file
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                LogDebug("Main save file deleted");
            }

            // Delete all backups
            DeleteAllBackups();

            currentSaveData = null;
            LogDebug("All save data deleted");

            EventSystem.Publish("SaveDataDeleted", true);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to delete save data: {e.Message}");
            EventSystem.Publish("SaveDataDeleted", false);
        }
    }

    /// <summary>
    /// Gets the full path to the save file.
    /// </summary>
    /// <returns>Absolute path to save file</returns>
    public string GetSavePath()
    {
        return savePath;
    }

    /// <summary>
    /// Performs an auto-save operation.
    /// Only saves if the game is in a safe state.
    /// </summary>
    public void AutoSave()
    {
        // Check if it's safe to auto-save (not during critical operations)
        if (!IsSafeToSave())
        {
            LogDebug("Skipping auto-save: game is in unsafe state");
            return;
        }

        LogDebug("Auto-saving...");
        SaveGame();
    }

    /// <summary>
    /// Gathers save data from all game systems.
    /// </summary>
    /// <returns>Complete save data structure</returns>
    private SaveData GatherSaveData()
    {
        SaveData data = new SaveData();

        // Update timestamp and metadata
        data.saveTimestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        data.gameVersion = Application.version;
        data.totalPlayTime += Time.realtimeSinceStartup; // Accumulate play time

        // Get data from GameManager
        if (GameManager.Instance != null)
        {
            GameState state = GameManager.Instance.CurrentGameState;
            data.playerPosition = new SerializableVector3(state.playerPosition);
            data.currentTime = state.currentTime;
            data.timeOfDay = state.timeOfDay;
            data.currentWeather = state.weather;
            data.sanity = state.sanity;
            data.money = state.money;
            data.fuel = state.fuel;
            data.currentLocationID = state.currentLocationID;
        }

        // Allow other systems to add their data via event
        EventSystem.Publish("GatheringSaveData", data);

        return data;
    }

    /// <summary>
    /// Applies loaded save data to all game systems.
    /// </summary>
    /// <param name="data">The loaded save data</param>
    private void ApplySaveData(SaveData data)
    {
        // Apply to GameManager
        if (GameManager.Instance != null)
        {
            GameState state = new GameState
            {
                playerPosition = data.playerPosition.ToVector3(),
                currentTime = data.currentTime,
                timeOfDay = data.timeOfDay,
                weather = data.currentWeather,
                sanity = data.sanity,
                money = data.money,
                fuel = data.fuel,
                currentLocationID = data.currentLocationID
            };

            GameManager.Instance.UpdateGameState(state);
        }

        // Allow other systems to load their data via event
        EventSystem.Publish("ApplyingSaveData", data);
    }

    /// <summary>
    /// Creates a backup of the current save file.
    /// Maintains up to 'maxBackups' backup files.
    /// </summary>
    private void CreateBackup()
    {
        try
        {
            // Get all existing backups
            List<string> backups = GetBackupFiles();

            // If we have max backups, delete the oldest
            if (backups.Count >= maxBackups)
            {
                // Sort by creation time (oldest first)
                var sortedBackups = backups
                    .Select(b => new FileInfo(b))
                    .OrderBy(fi => fi.CreationTime)
                    .ToList();

                // Delete oldest backups to make room
                while (sortedBackups.Count >= maxBackups)
                {
                    File.Delete(sortedBackups[0].FullName);
                    LogDebug($"Deleted old backup: {sortedBackups[0].Name}");
                    sortedBackups.RemoveAt(0);
                }
            }

            // Create new backup with timestamp
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupPath = Path.Combine(saveDirectory, $"{Path.GetFileNameWithoutExtension(saveFileName)}_backup_{timestamp}.json");
            File.Copy(savePath, backupPath, true);

            LogDebug($"Backup created: {Path.GetFileName(backupPath)}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to create backup: {e.Message}");
        }
    }

    /// <summary>
    /// Attempts to restore save data from a backup file.
    /// Tries backups from newest to oldest.
    /// </summary>
    /// <param name="restoredData">The restored save data if successful</param>
    /// <returns>True if restoration succeeded</returns>
    private bool TryRestoreFromBackup(out SaveData restoredData)
    {
        restoredData = null;

        List<string> backups = GetBackupFiles();
        if (backups.Count == 0)
        {
            LogDebug("No backups available for restoration");
            return false;
        }

        // Sort backups by creation time (newest first)
        var sortedBackups = backups
            .Select(b => new FileInfo(b))
            .OrderByDescending(fi => fi.CreationTime)
            .ToList();

        // Try each backup from newest to oldest
        foreach (var backup in sortedBackups)
        {
            try
            {
                LogDebug($"Attempting to restore from backup: {backup.Name}");

                string json = File.ReadAllText(backup.FullName);
                SaveData data = JsonUtility.FromJson<SaveData>(json);

                // Validate the backup
                SaveValidator.ValidationResult validation = SaveValidator.ValidateSaveData(data);
                if (validation.IsValid)
                {
                    restoredData = data;
                    LogDebug($"Successfully restored from backup: {backup.Name}");
                    return true;
                }
                else
                {
                    LogDebug($"Backup {backup.Name} is invalid, trying next...");
                }
            }
            catch (System.Exception e)
            {
                LogDebug($"Failed to restore from {backup.Name}: {e.Message}");
            }
        }

        return false;
    }

    /// <summary>
    /// Gets all backup files in the save directory.
    /// </summary>
    /// <returns>List of backup file paths</returns>
    private List<string> GetBackupFiles()
    {
        string backupPattern = $"{Path.GetFileNameWithoutExtension(saveFileName)}_backup_*.json";
        string[] files = Directory.GetFiles(saveDirectory, backupPattern);
        return new List<string>(files);
    }

    /// <summary>
    /// Deletes all backup files.
    /// </summary>
    private void DeleteAllBackups()
    {
        List<string> backups = GetBackupFiles();
        foreach (string backup in backups)
        {
            try
            {
                File.Delete(backup);
                LogDebug($"Deleted backup: {Path.GetFileName(backup)}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to delete backup {backup}: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Checks if it's currently safe to save the game.
    /// </summary>
    /// <returns>True if safe to save</returns>
    private bool IsSafeToSave()
    {
        // Check if game is paused or in menu
        if (Time.timeScale == 0f)
            return false;

        // Check if any critical operations are in progress
        // This can be extended by other systems publishing to "CheckSafeToSave" event
        bool isSafe = true;
        EventSystem.Publish("CheckSafeToSave", isSafe);

        return isSafe;
    }

    /// <summary>
    /// Event handler for save requests.
    /// </summary>
    private void OnRequestSave(SaveData data)
    {
        SaveGame();
    }

    /// <summary>
    /// Event handler for load requests.
    /// </summary>
    private void OnRequestLoad()
    {
        LoadGame();
    }

    /// <summary>
    /// Logs debug messages if debugging is enabled.
    /// </summary>
    private void LogDebug(string message)
    {
        if (enableDebugLogging)
        {
            Debug.Log($"[SaveManager] {message}");
        }
    }

    /// <summary>
    /// Gets the current save data (read-only).
    /// </summary>
    public SaveData CurrentSaveData => currentSaveData;

    /// <summary>
    /// Gets information about available backup files.
    /// </summary>
    /// <returns>Array of backup file info</returns>
    public FileInfo[] GetBackupInfo()
    {
        List<string> backups = GetBackupFiles();
        return backups.Select(b => new FileInfo(b)).OrderByDescending(fi => fi.CreationTime).ToArray();
    }
}
