using UnityEngine;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Validates save data integrity and detects corruption.
/// Performs comprehensive checks on save files to ensure data validity.
/// </summary>
public class SaveValidator
{
    private const string EXPECTED_SAVE_VERSION = "1.0.0";
    private static readonly string[] VALID_LOCATIONS = { "starter_lake" }; // Will be expanded by other agents

    /// <summary>
    /// Validation result containing status and error messages.
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; }
        public List<string> Warnings { get; set; }

        public ValidationResult()
        {
            IsValid = true;
            Errors = new List<string>();
            Warnings = new List<string>();
        }

        public void AddError(string error)
        {
            Errors.Add(error);
            IsValid = false;
        }

        public void AddWarning(string warning)
        {
            Warnings.Add(warning);
        }

        public string GetSummary()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Validation Status: {(IsValid ? "VALID" : "INVALID")}");

            if (Errors.Count > 0)
            {
                sb.AppendLine($"\nErrors ({Errors.Count}):");
                foreach (var error in Errors)
                {
                    sb.AppendLine($"  - {error}");
                }
            }

            if (Warnings.Count > 0)
            {
                sb.AppendLine($"\nWarnings ({Warnings.Count}):");
                foreach (var warning in Warnings)
                {
                    sb.AppendLine($"  - {warning}");
                }
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Validates the integrity and correctness of save data.
    /// </summary>
    /// <param name="data">The save data to validate</param>
    /// <returns>Validation result with status and any errors/warnings</returns>
    public static ValidationResult ValidateSaveData(SaveData data)
    {
        ValidationResult result = new ValidationResult();

        if (data == null)
        {
            result.AddError("Save data is null");
            return result;
        }

        // Validate meta information
        ValidateMetaData(data, result);

        // Validate player state
        ValidatePlayerState(data, result);

        // Validate resources
        ValidateResources(data, result);

        // Validate location data
        ValidateLocationData(data, result);

        // Validate inventory
        ValidateInventory(data, result);

        // Validate fish collection
        ValidateFishCollection(data, result);

        // Validate statistics
        ValidateStatistics(data, result);

        return result;
    }

    /// <summary>
    /// Validates meta information fields.
    /// </summary>
    private static void ValidateMetaData(SaveData data, ValidationResult result)
    {
        // Check save version
        if (string.IsNullOrEmpty(data.saveVersion))
        {
            result.AddError("Save version is missing");
        }
        else if (data.saveVersion != EXPECTED_SAVE_VERSION)
        {
            result.AddWarning($"Save version mismatch. Expected: {EXPECTED_SAVE_VERSION}, Got: {data.saveVersion}");
        }

        // Check timestamp
        if (string.IsNullOrEmpty(data.saveTimestamp))
        {
            result.AddError("Save timestamp is missing");
        }
        else
        {
            try
            {
                DateTime.Parse(data.saveTimestamp);
            }
            catch
            {
                result.AddError($"Invalid timestamp format: {data.saveTimestamp}");
            }
        }

        // Check game version
        if (string.IsNullOrEmpty(data.gameVersion))
        {
            result.AddWarning("Game version is missing");
        }

        // Check play time
        if (data.totalPlayTime < 0)
        {
            result.AddError($"Invalid total play time: {data.totalPlayTime}");
        }
    }

    /// <summary>
    /// Validates player state fields.
    /// </summary>
    private static void ValidatePlayerState(SaveData data, ValidationResult result)
    {
        // Validate position (check for NaN or extreme values)
        Vector3 pos = data.playerPosition.ToVector3();
        if (float.IsNaN(pos.x) || float.IsNaN(pos.y) || float.IsNaN(pos.z))
        {
            result.AddError("Player position contains NaN values");
        }
        else if (pos.magnitude > 100000f) // Sanity check for extreme positions
        {
            result.AddWarning($"Player position seems extreme: {pos}");
        }

        // Validate rotation
        Quaternion rot = data.playerRotation.ToQuaternion();
        if (float.IsNaN(rot.x) || float.IsNaN(rot.y) || float.IsNaN(rot.z) || float.IsNaN(rot.w))
        {
            result.AddError("Player rotation contains NaN values");
        }

        // Validate time
        if (data.currentTime < 0f || data.currentTime >= 24f)
        {
            result.AddError($"Invalid current time: {data.currentTime} (must be 0-24)");
        }
    }

    /// <summary>
    /// Validates resource values.
    /// </summary>
    private static void ValidateResources(SaveData data, ValidationResult result)
    {
        // Validate money
        if (data.money < 0)
        {
            result.AddError($"Invalid money value: {data.money} (cannot be negative)");
        }
        else if (data.money > 1000000f)
        {
            result.AddWarning($"Money value seems very high: {data.money}");
        }

        // Validate scrap
        if (data.scrap < 0)
        {
            result.AddError($"Invalid scrap value: {data.scrap} (cannot be negative)");
        }

        // Validate relics
        if (data.relics < 0)
        {
            result.AddError($"Invalid relics count: {data.relics} (cannot be negative)");
        }

        // Validate fuel
        if (data.fuel < 0 || data.fuel > 100)
        {
            result.AddError($"Invalid fuel value: {data.fuel} (must be 0-100)");
        }

        // Validate sanity
        if (data.sanity < 0 || data.sanity > 100)
        {
            result.AddError($"Invalid sanity value: {data.sanity} (must be 0-100)");
        }
    }

    /// <summary>
    /// Validates location data.
    /// </summary>
    private static void ValidateLocationData(SaveData data, ValidationResult result)
    {
        // Check current location
        if (string.IsNullOrEmpty(data.currentLocationID))
        {
            result.AddError("Current location ID is missing");
        }

        // Check unlocked locations
        if (data.unlockedLocations == null)
        {
            result.AddError("Unlocked locations list is null");
        }
        else if (data.unlockedLocations.Count == 0)
        {
            result.AddWarning("No locations unlocked (player should have at least starter location)");
        }
    }

    /// <summary>
    /// Validates inventory data.
    /// </summary>
    private static void ValidateInventory(SaveData data, ValidationResult result)
    {
        if (data.inventoryItems == null)
        {
            result.AddError("Inventory items list is null");
            return;
        }

        // Validate inventory capacity
        if (data.inventoryCapacity <= 0)
        {
            result.AddError($"Invalid inventory capacity: {data.inventoryCapacity}");
        }

        // Validate each item
        for (int i = 0; i < data.inventoryItems.Count; i++)
        {
            SerializedItem item = data.inventoryItems[i];

            if (string.IsNullOrEmpty(item.itemID))
            {
                result.AddError($"Item {i} has missing ID");
            }

            if (item.quantity <= 0)
            {
                result.AddError($"Item {i} ({item.itemName}) has invalid quantity: {item.quantity}");
            }

            if (item.value < 0)
            {
                result.AddError($"Item {i} ({item.itemName}) has negative value: {item.value}");
            }

            if (item.durability < 0 || item.durability > 100)
            {
                result.AddError($"Item {i} ({item.itemName}) has invalid durability: {item.durability}");
            }

            // Validate grid position
            if (item.gridX < 0 || item.gridY < 0)
            {
                result.AddError($"Item {i} ({item.itemName}) has invalid grid position: ({item.gridX}, {item.gridY})");
            }

            if (item.width <= 0 || item.height <= 0)
            {
                result.AddError($"Item {i} ({item.itemName}) has invalid dimensions: {item.width}x{item.height}");
            }
        }
    }

    /// <summary>
    /// Validates fish collection data.
    /// </summary>
    private static void ValidateFishCollection(SaveData data, ValidationResult result)
    {
        if (data.fishCaughtCount == null)
        {
            result.AddError("Fish caught count dictionary is null");
        }
        else
        {
            foreach (var kvp in data.fishCaughtCount)
            {
                if (kvp.Value < 0)
                {
                    result.AddError($"Fish species '{kvp.Key}' has negative catch count: {kvp.Value}");
                }
            }
        }

        if (data.discoveredFishSpecies == null)
        {
            result.AddError("Discovered fish species list is null");
        }

        if (data.fishRecordWeights == null)
        {
            result.AddError("Fish record weights dictionary is null");
        }
        else
        {
            foreach (var kvp in data.fishRecordWeights)
            {
                if (kvp.Value <= 0)
                {
                    result.AddError($"Fish species '{kvp.Key}' has invalid record weight: {kvp.Value}");
                }
            }
        }
    }

    /// <summary>
    /// Validates game statistics.
    /// </summary>
    private static void ValidateStatistics(SaveData data, ValidationResult result)
    {
        if (data.totalFishCaught < 0)
        {
            result.AddError($"Invalid total fish caught: {data.totalFishCaught}");
        }

        if (data.totalMoneyEarned < 0)
        {
            result.AddError($"Invalid total money earned: {data.totalMoneyEarned}");
        }

        if (data.nightsSurvived < 0)
        {
            result.AddError($"Invalid nights survived: {data.nightsSurvived}");
        }

        if (data.lowestSanity < 0 || data.lowestSanity > 100)
        {
            result.AddError($"Invalid lowest sanity: {data.lowestSanity}");
        }

        if (data.deathCount < 0)
        {
            result.AddError($"Invalid death count: {data.deathCount}");
        }
    }

    /// <summary>
    /// Calculates a checksum for save data to detect corruption.
    /// </summary>
    /// <param name="jsonData">The JSON string of save data</param>
    /// <returns>MD5 checksum hash</returns>
    public static string CalculateChecksum(string jsonData)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(jsonData);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// Verifies that a checksum matches the save data.
    /// </summary>
    /// <param name="jsonData">The JSON string of save data</param>
    /// <param name="expectedChecksum">The expected checksum</param>
    /// <returns>True if checksums match</returns>
    public static bool VerifyChecksum(string jsonData, string expectedChecksum)
    {
        string actualChecksum = CalculateChecksum(jsonData);
        return actualChecksum.Equals(expectedChecksum, StringComparison.OrdinalIgnoreCase);
    }
}
