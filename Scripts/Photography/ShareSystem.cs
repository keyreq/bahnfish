using UnityEngine;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Agent 18: Photography Mode Specialist - ShareSystem.cs
/// Manages photo sharing features including watermarks, stat overlays, and export.
/// Prepares photos for social sharing with customizable branding.
/// </summary>
public class ShareSystem : MonoBehaviour
{
    public static ShareSystem Instance { get; private set; }

    [Header("Watermark Settings")]
    [SerializeField] private bool enableWatermark = true;
    [SerializeField] private string watermarkText = "Bahnfish";
    [SerializeField] private WatermarkPosition watermarkPosition = WatermarkPosition.BottomRight;
    [SerializeField] private int watermarkFontSize = 24;
    [SerializeField] private Color watermarkColor = new Color(1f, 1f, 1f, 0.7f);

    [Header("Stats Overlay Settings")]
    [SerializeField] private bool includeStatsOverlay = false;
    [SerializeField] private Color overlayBackgroundColor = new Color(0f, 0f, 0f, 0.7f);
    [SerializeField] private Color overlayTextColor = Color.white;

    [Header("Export Settings")]
    [SerializeField] private string sharedPhotosDirectory = "SharedPhotos";

    private string fullSharedPath;
    private Texture2D watermarkTexture;
    private Font watermarkFont;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Setup shared photos directory
        fullSharedPath = Path.Combine(Application.persistentDataPath, sharedPhotosDirectory);
        if (!Directory.Exists(fullSharedPath))
        {
            Directory.CreateDirectory(fullSharedPath);
        }
    }

    /// <summary>
    /// Prepares a photo for sharing with watermark and optional stats overlay.
    /// </summary>
    public string PreparePhotoForSharing(PhotoMetadata photo, bool includeStats = false)
    {
        if (photo == null || string.IsNullOrEmpty(photo.filePath))
        {
            Debug.LogError("[ShareSystem] Invalid photo metadata!");
            return null;
        }

        // Load original photo
        Texture2D originalPhoto = LoadPhotoTexture(photo.filePath);
        if (originalPhoto == null)
        {
            Debug.LogError("[ShareSystem] Failed to load photo texture!");
            return null;
        }

        // Create a copy to modify
        Texture2D sharePhoto = new Texture2D(originalPhoto.width, originalPhoto.height, TextureFormat.RGB24, false);
        sharePhoto.SetPixels(originalPhoto.GetPixels());
        sharePhoto.Apply();

        // Apply watermark
        if (enableWatermark)
        {
            ApplyWatermark(sharePhoto, photo);
        }

        // Apply stats overlay
        if (includeStats || includeStatsOverlay)
        {
            ApplyStatsOverlay(sharePhoto, photo);
        }

        // Save shared photo
        string sharedFilename = $"Shared_{photo.fileName}";
        string sharedPath = Path.Combine(fullSharedPath, sharedFilename);
        SaveTextureToDisk(sharePhoto, sharedPath);

        Debug.Log($"[ShareSystem] Photo prepared for sharing: {sharedPath}");

        // Cleanup
        Destroy(originalPhoto);
        Destroy(sharePhoto);

        return sharedPath;
    }

    /// <summary>
    /// Applies watermark to photo texture.
    /// </summary>
    private void ApplyWatermark(Texture2D texture, PhotoMetadata photo)
    {
        // Create watermark text
        string watermark = GenerateWatermarkText(photo);

        // Calculate watermark position
        Vector2Int position = CalculateWatermarkPosition(texture.width, texture.height);

        // Draw watermark text (simplified - in real implementation would use TextMesh or UI)
        // For now, we'll add a simple semi-transparent overlay
        DrawWatermarkOverlay(texture, position, watermark);
    }

    /// <summary>
    /// Generates watermark text.
    /// </summary>
    private string GenerateWatermarkText(PhotoMetadata photo)
    {
        string playerName = GetPlayerName();
        return $"{watermarkText} - {playerName}";
    }

    /// <summary>
    /// Gets player name from game state.
    /// </summary>
    private string GetPlayerName()
    {
        // Try to get player name from save data or settings
        // For now, return default
        return "Captain";
    }

    /// <summary>
    /// Calculates watermark position based on settings.
    /// </summary>
    private Vector2Int CalculateWatermarkPosition(int width, int height)
    {
        int margin = 20;

        switch (watermarkPosition)
        {
            case WatermarkPosition.TopLeft:
                return new Vector2Int(margin, height - margin - watermarkFontSize);
            case WatermarkPosition.TopRight:
                return new Vector2Int(width - margin - 200, height - margin - watermarkFontSize);
            case WatermarkPosition.BottomLeft:
                return new Vector2Int(margin, margin);
            case WatermarkPosition.BottomRight:
            default:
                return new Vector2Int(width - margin - 200, margin);
        }
    }

    /// <summary>
    /// Draws watermark overlay on texture.
    /// </summary>
    private void DrawWatermarkOverlay(Texture2D texture, Vector2Int position, string text)
    {
        // Draw semi-transparent background for watermark
        int bgWidth = 220;
        int bgHeight = 40;

        Color bgColor = new Color(0f, 0f, 0f, 0.6f);

        for (int x = position.x; x < position.x + bgWidth && x < texture.width; x++)
        {
            for (int y = position.y; y < position.y + bgHeight && y < texture.height; y++)
            {
                Color original = texture.GetPixel(x, y);
                Color blended = Color.Lerp(original, bgColor, bgColor.a);
                texture.SetPixel(x, y, blended);
            }
        }

        texture.Apply();
    }

    /// <summary>
    /// Applies stats overlay to photo.
    /// </summary>
    private void ApplyStatsOverlay(Texture2D texture, PhotoMetadata photo)
    {
        // Create stats overlay in bottom-left corner
        int overlayWidth = 300;
        int overlayHeight = 150;
        int margin = 20;

        Color bgColor = overlayBackgroundColor;

        // Draw overlay background
        for (int x = margin; x < margin + overlayWidth && x < texture.width; x++)
        {
            for (int y = margin; y < margin + overlayHeight && y < texture.height; y++)
            {
                Color original = texture.GetPixel(x, y);
                Color blended = Color.Lerp(original, bgColor, bgColor.a);
                texture.SetPixel(x, y, blended);
            }
        }

        // In a real implementation, would render text with stats
        // For now, just mark the overlay area
        texture.Apply();
    }

    /// <summary>
    /// Generates stats text for overlay.
    /// </summary>
    private string GenerateStatsText(PhotoMetadata photo)
    {
        List<string> stats = new List<string>();

        // Fish species
        if (!string.IsNullOrEmpty(photo.fishSpeciesID))
        {
            FishSpeciesData fishData = FishDatabase.Instance?.GetFishByID(photo.fishSpeciesID);
            if (fishData != null)
            {
                stats.Add($"Species: {fishData.fishName}");
                stats.Add($"Rarity: {fishData.rarity}");
            }
        }

        // Location
        if (!string.IsNullOrEmpty(photo.locationID))
        {
            stats.Add($"Location: {photo.locationID}");
        }

        // Time and weather
        stats.Add($"Time: {photo.timeOfDay:F1}");
        stats.Add($"Weather: {photo.weatherType}");

        // Quality rating
        if (photo.qualityRating > 0)
        {
            stats.Add($"Quality: {photo.qualityRating:F1} stars");
        }

        stats.Add("Captured in Bahnfish");

        return string.Join("\n", stats);
    }

    /// <summary>
    /// Copies photo to clipboard (Windows only).
    /// </summary>
    public void CopyPhotoToClipboard(PhotoMetadata photo)
    {
        string sharedPath = PreparePhotoForSharing(photo, includeStatsOverlay);

        if (!string.IsNullOrEmpty(sharedPath))
        {
            // In a real implementation, would use platform-specific clipboard API
            Debug.Log($"[ShareSystem] Photo copied to clipboard: {sharedPath}");
            EventSystem.Publish("PhotoCopiedToClipboard", photo);
        }
    }

    /// <summary>
    /// Generates a permalink for future online integration.
    /// </summary>
    public string GeneratePermalink(PhotoMetadata photo)
    {
        // Generate a unique permalink ID
        string permalinkID = System.Guid.NewGuid().ToString();

        // In a real implementation, would upload to server and get URL
        string permalink = $"https://bahnfish.com/photos/{permalinkID}";

        Debug.Log($"[ShareSystem] Generated permalink: {permalink}");

        return permalink;
    }

    /// <summary>
    /// Exports photo with custom settings.
    /// </summary>
    public string ExportPhoto(PhotoMetadata photo, ShareExportSettings settings)
    {
        // Temporarily apply settings
        bool previousWatermark = enableWatermark;
        bool previousStats = includeStatsOverlay;

        enableWatermark = settings.includeWatermark;
        includeStatsOverlay = settings.includeStats;

        // Prepare photo
        string exportPath = PreparePhotoForSharing(photo, settings.includeStats);

        // Restore settings
        enableWatermark = previousWatermark;
        includeStatsOverlay = previousStats;

        return exportPath;
    }

    /// <summary>
    /// Loads photo texture from file.
    /// </summary>
    private Texture2D LoadPhotoTexture(string filepath)
    {
        if (!File.Exists(filepath))
        {
            Debug.LogError($"[ShareSystem] Photo file not found: {filepath}");
            return null;
        }

        byte[] fileData = File.ReadAllBytes(filepath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return texture;
    }

    /// <summary>
    /// Saves texture to disk.
    /// </summary>
    private void SaveTextureToDisk(Texture2D texture, string filepath)
    {
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(filepath, bytes);
    }

    #region Public API

    /// <summary>
    /// Enables or disables watermark.
    /// </summary>
    public void SetWatermarkEnabled(bool enabled)
    {
        enableWatermark = enabled;
    }

    /// <summary>
    /// Sets watermark text.
    /// </summary>
    public void SetWatermarkText(string text)
    {
        watermarkText = text;
    }

    /// <summary>
    /// Sets watermark position.
    /// </summary>
    public void SetWatermarkPosition(WatermarkPosition position)
    {
        watermarkPosition = position;
    }

    /// <summary>
    /// Enables or disables stats overlay.
    /// </summary>
    public void SetStatsOverlayEnabled(bool enabled)
    {
        includeStatsOverlay = enabled;
    }

    /// <summary>
    /// Gets shared photos directory.
    /// </summary>
    public string GetSharedPhotosDirectory()
    {
        return fullSharedPath;
    }

    /// <summary>
    /// Gets whether watermark is enabled.
    /// </summary>
    public bool IsWatermarkEnabled()
    {
        return enableWatermark;
    }

    /// <summary>
    /// Gets whether stats overlay is enabled.
    /// </summary>
    public bool IsStatsOverlayEnabled()
    {
        return includeStatsOverlay;
    }

    #endregion
}

/// <summary>
/// Watermark position options.
/// </summary>
[System.Serializable]
public enum WatermarkPosition
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

/// <summary>
/// Share export settings.
/// </summary>
[System.Serializable]
public class ShareExportSettings
{
    public bool includeWatermark = true;
    public bool includeStats = false;
    public ExportResolution resolution = ExportResolution.HD_1080p;
    public ExportFormat format = ExportFormat.PNG;
}
