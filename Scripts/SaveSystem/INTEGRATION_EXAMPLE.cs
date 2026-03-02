using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Example integration showing how other agents should use the SaveSystem.
/// This demonstrates the proper way to save and load custom data.
///
/// DELETE THIS FILE - It's just an example for other developers!
/// </summary>
public class INTEGRATION_EXAMPLE : MonoBehaviour
{
    // Example: Inventory System Integration
    private List<SerializedItem> myInventory = new List<SerializedItem>();

    void Start()
    {
        // Subscribe to save/load events
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnSaveRequested);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnLoadRequested);
    }

    void OnDestroy()
    {
        // Always unsubscribe to prevent memory leaks
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnSaveRequested);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnLoadRequested);
    }

    /// <summary>
    /// Called when the save system is gathering data.
    /// Add your system's data to the SaveData object.
    /// </summary>
    private void OnSaveRequested(SaveData data)
    {
        Debug.Log("[EXAMPLE] Saving inventory data...");

        // Clear existing inventory in save data
        data.inventoryItems.Clear();

        // Add all inventory items to save data
        foreach (var item in myInventory)
        {
            data.inventoryItems.Add(item);
        }

        Debug.Log($"[EXAMPLE] Saved {myInventory.Count} items");
    }

    /// <summary>
    /// Called when the save system is loading data.
    /// Restore your system's state from the SaveData object.
    /// </summary>
    private void OnLoadRequested(SaveData data)
    {
        Debug.Log("[EXAMPLE] Loading inventory data...");

        // Clear current inventory
        myInventory.Clear();

        // Restore inventory from save data
        foreach (var item in data.inventoryItems)
        {
            myInventory.Add(item);
        }

        Debug.Log($"[EXAMPLE] Loaded {myInventory.Count} items");
    }

    // ===== EXAMPLE: How to trigger saves and loads =====

    void Update()
    {
        // Example: Save when player presses F5
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveGame();
        }

        // Example: Load when player presses F9
        if (Input.GetKeyDown(KeyCode.F9))
        {
            LoadGame();
        }
    }

    /// <summary>
    /// Example: How to save the game
    /// </summary>
    private void SaveGame()
    {
        // Method 1: Direct call
        SaveManager.Instance.SaveGame();

        // Method 2: Via event system (preferred for loose coupling)
        // EventSystem.Publish("RequestSave", new SaveData());
    }

    /// <summary>
    /// Example: How to load the game
    /// </summary>
    private void LoadGame()
    {
        // Check if save data exists
        if (!SaveManager.Instance.HasSaveData())
        {
            Debug.LogWarning("[EXAMPLE] No save data found!");
            return;
        }

        // Method 1: Direct call
        SaveManager.Instance.LoadGame();

        // Method 2: Via event system (preferred for loose coupling)
        // EventSystem.Publish("RequestLoad");
    }

    // ===== EXAMPLE: Creating SerializedItems =====

    /// <summary>
    /// Example: How to create a fish item for inventory
    /// </summary>
    private SerializedItem CreateFishItem(string fishID, string fishName, float weight)
    {
        SerializedItem item = new SerializedItem
        {
            itemID = fishID,
            itemName = fishName,
            itemType = "fish",
            quantity = 1,
            value = weight * 10f, // Base value calculation

            // Fish-specific data
            fishSpecies = fishID,
            fishWeight = weight,
            isAberrant = false,
            rarity = FishRarity.Common,

            // Inventory grid placement
            gridX = 0,
            gridY = 0,
            width = 2,  // Fish takes 2x1 grid space
            height = 1,
            rotation = 0,

            // Item condition
            durability = 100f,
            isFresh = true
        };

        return item;
    }

    /// <summary>
    /// Example: How to create an upgrade item
    /// </summary>
    private SerializedItem CreateUpgradeItem(string upgradeID, string upgradeName)
    {
        SerializedItem item = new SerializedItem
        {
            itemID = upgradeID,
            itemName = upgradeName,
            itemType = "upgrade",
            quantity = 1,
            value = 500f,

            // Upgrade-specific data stored in additionalData
            additionalData = JsonUtility.ToJson(new { level = 1, maxLevel = 5 }),

            // Grid placement
            gridX = 0,
            gridY = 0,
            width = 1,
            height = 1,
            rotation = 0,

            durability = 100f
        };

        return item;
    }

    // ===== EXAMPLE: Adding custom data fields =====

    /// <summary>
    /// Example: If you need to save data that doesn't fit in SaveData,
    /// you have two options:
    ///
    /// Option 1: Add a new field to SaveData.cs
    /// - Open SaveData.cs
    /// - Add your field: public List<MyCustomData> myData = new List<MyCustomData>();
    /// - Save and load it like shown above
    ///
    /// Option 2: Use the additionalData field as JSON
    /// - Store complex data as JSON string
    /// - This example shows how:
    /// </summary>
    private void SaveCustomDataExample(SaveData data)
    {
        // Create custom data structure
        var customData = new
        {
            playerName = "Fisher123",
            favoriteLocation = "deep_ocean",
            playstyle = "night_fisher",
            achievements = new string[] { "first_catch", "night_survivor" }
        };

        // Serialize to JSON and store in unused field
        string jsonData = JsonUtility.ToJson(customData);

        // Store in inventoryData field if you're not using inventory
        // or add a new field to SaveData.cs
        // data.inventoryData = jsonData;

        Debug.Log($"[EXAMPLE] Saved custom data: {jsonData}");
    }

    /// <summary>
    /// Example: Loading custom JSON data
    /// </summary>
    private void LoadCustomDataExample(SaveData data)
    {
        // string jsonData = data.inventoryData;
        // var customData = JsonUtility.FromJson<MyCustomDataType>(jsonData);
        // Debug.Log($"[EXAMPLE] Player name: {customData.playerName}");
    }

    // ===== EXAMPLE: Listening to save/load completion =====

    void ExampleListenToEvents()
    {
        // Listen for save completion
        EventSystem.Subscribe<SaveData>("SaveComplete", (data) =>
        {
            Debug.Log("[EXAMPLE] Save completed successfully!");
            // Show notification to player
            // Update UI
        });

        // Listen for save failure
        EventSystem.Subscribe<string>("SaveFailed", (error) =>
        {
            Debug.LogError($"[EXAMPLE] Save failed: {error}");
            // Show error message to player
        });

        // Listen for load completion
        EventSystem.Subscribe<SaveData>("LoadComplete", (data) =>
        {
            Debug.Log("[EXAMPLE] Load completed successfully!");
            // Rebuild your system state
            // Update UI
        });

        // Listen for load failure
        EventSystem.Subscribe<string>("LoadFailed", (error) =>
        {
            Debug.LogError($"[EXAMPLE] Load failed: {error}");
            // Show error message to player
            // Maybe offer to start new game
        });
    }

    // ===== EXAMPLE: Checking if it's safe to save =====

    /// <summary>
    /// Example: If your system has critical operations that shouldn't
    /// be interrupted by auto-save, you can prevent saves:
    /// </summary>
    void ExampleSafeSaveCheck()
    {
        bool isFishing = true; // Your critical operation flag

        EventSystem.Subscribe<bool>("CheckSafeToSave", (isSafe) =>
        {
            if (isFishing)
            {
                // Signal that it's NOT safe to save right now
                // This will prevent auto-save from triggering
                isSafe = false;
            }
        });
    }

    // ===== EXAMPLE: Manual backup management =====

    void ExampleBackupManagement()
    {
        // Get information about available backups
        System.IO.FileInfo[] backups = SaveManager.Instance.GetBackupInfo();

        Debug.Log($"[EXAMPLE] Found {backups.Length} backups:");
        foreach (var backup in backups)
        {
            Debug.Log($"  - {backup.Name}");
            Debug.Log($"    Created: {backup.CreationTime}");
            Debug.Log($"    Size: {backup.Length / 1024}KB");
        }
    }
}
