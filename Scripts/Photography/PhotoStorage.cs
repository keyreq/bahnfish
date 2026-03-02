using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Agent 18: Photography Mode Specialist - PhotoStorage.cs
/// Manages screenshot capture, photo storage, and metadata tracking.
/// Supports high-resolution exports up to 4K with comprehensive metadata.
/// </summary>
public class PhotoStorage : MonoBehaviour
{
    public static PhotoStorage Instance { get; private set; }

    [Header("Storage Settings")]
    [SerializeField] private string photoDirectory = "Photos";
    [SerializeField] private ExportResolution defaultResolution = ExportResolution.HD_1080p;
    [SerializeField] private ExportFormat defaultFormat = ExportFormat.PNG;
    [SerializeField] private int maxPhotosInGallery = 500;

    [Header("Photo Gallery")]
    [SerializeField] private List<PhotoMetadata> photoGallery = new List<PhotoMetadata>();

    private string fullPhotoPath;
    private int nextPhotoID = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Setup photo directory
        fullPhotoPath = Path.Combine(Application.persistentDataPath, photoDirectory);
        if (!Directory.Exists(fullPhotoPath))
        {
            Directory.CreateDirectory(fullPhotoPath);
            Debug.Log($"[PhotoStorage] Created photo directory: {fullPhotoPath}");
        }

        LoadPhotoGallery();
    }

    /// <summary>
    /// Captures a photo from the specified camera.
    /// </summary>
    public PhotoMetadata CapturePhoto(Camera camera)
    {
        if (camera == null)
        {
            Debug.LogError("[PhotoStorage] Cannot capture photo - camera is null!");
            return null;
        }

        // Get photo settings
        int width, height;
        GetResolutionDimensions(defaultResolution, out width, out height);

        // Capture screenshot
        RenderTexture renderTexture = new RenderTexture(width, height, 24);
        camera.targetTexture = renderTexture;
        camera.Render();

        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
        RenderTexture.active = renderTexture;
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshot.Apply();

        camera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);

        // Apply filters if any
        if (CameraEffects.Instance != null)
        {
            screenshot = CameraEffects.Instance.ApplyFiltersToTexture(screenshot);
        }

        // Create metadata
        PhotoMetadata metadata = CreatePhotoMetadata(screenshot);

        // Save to disk
        string filename = GenerateFilename();
        string filepath = Path.Combine(fullPhotoPath, filename);
        SavePhotoToDisk(screenshot, filepath, defaultFormat);

        metadata.filePath = filepath;
        metadata.fileName = filename;

        // Add to gallery
        AddToGallery(metadata);

        // Publish event
        EventSystem.Publish("PhotoSaved", metadata);

        Debug.Log($"[PhotoStorage] Photo captured: {filename}");

        return metadata;
    }

    /// <summary>
    /// Captures a photo at a specific resolution.
    /// </summary>
    public PhotoMetadata CapturePhotoAtResolution(Camera camera, ExportResolution resolution)
    {
        ExportResolution previousResolution = defaultResolution;
        defaultResolution = resolution;
        PhotoMetadata metadata = CapturePhoto(camera);
        defaultResolution = previousResolution;
        return metadata;
    }

    /// <summary>
    /// Creates photo metadata for captured photo.
    /// </summary>
    private PhotoMetadata CreatePhotoMetadata(Texture2D screenshot)
    {
        PhotoMetadata metadata = new PhotoMetadata();
        metadata.photoID = GeneratePhotoID();
        metadata.captureDate = DateTime.Now;
        metadata.resolution = new Vector2Int(screenshot.width, screenshot.height);

        // Get game state information
        if (PhotoModeController.Instance != null)
        {
            metadata.cameraPosition = PhotoModeController.Instance.GetPhotoCamera().transform.position;
            metadata.cameraRotation = PhotoModeController.Instance.GetPhotoCamera().transform.rotation;
            metadata.fov = PhotoModeController.Instance.GetFOV();
            metadata.exposure = PhotoModeController.Instance.GetExposure();
        }

        // Get location (if available)
        metadata.locationID = GetCurrentLocation();

        // Get time and weather
        if (TimeManager.Instance != null)
        {
            metadata.timeOfDay = TimeManager.Instance.GetCurrentTime();
        }

        if (WeatherSystem.Instance != null)
        {
            metadata.weatherType = WeatherSystem.Instance.GetCurrentWeather().ToString();
        }

        // Get applied filters
        if (CameraEffects.Instance != null)
        {
            List<PhotoFilter> activeFilters = CameraEffects.Instance.GetActiveFilters();
            foreach (PhotoFilter filter in activeFilters)
            {
                metadata.filtersApplied.Add(filter.type.ToString());
            }
        }

        // Detect fish in frame (if any)
        metadata.fishSpeciesID = DetectFishInFrame();

        return metadata;
    }

    /// <summary>
    /// Saves photo to disk.
    /// </summary>
    private void SavePhotoToDisk(Texture2D texture, string filepath, ExportFormat format)
    {
        byte[] bytes;

        if (format == ExportFormat.PNG)
        {
            bytes = texture.EncodeToPNG();
        }
        else
        {
            bytes = texture.EncodeToJPG(85); // 85% quality for JPG
        }

        File.WriteAllBytes(filepath, bytes);
    }

    /// <summary>
    /// Generates a unique filename for the photo.
    /// </summary>
    private string GenerateFilename()
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string extension = defaultFormat == ExportFormat.PNG ? ".png" : ".jpg";
        return $"Bahnfish_{timestamp}{extension}";
    }

    /// <summary>
    /// Generates a unique photo ID.
    /// </summary>
    private string GeneratePhotoID()
    {
        string id = $"photo_{nextPhotoID:D6}";
        nextPhotoID++;
        return id;
    }

    /// <summary>
    /// Adds photo to gallery.
    /// </summary>
    private void AddToGallery(PhotoMetadata metadata)
    {
        photoGallery.Add(metadata);

        // Enforce gallery size limit
        while (photoGallery.Count > maxPhotosInGallery)
        {
            PhotoMetadata oldest = photoGallery[0];
            photoGallery.RemoveAt(0);

            // Delete file
            if (File.Exists(oldest.filePath))
            {
                File.Delete(oldest.filePath);
            }
        }

        SavePhotoGallery();
    }

    /// <summary>
    /// Gets the current location ID.
    /// </summary>
    private string GetCurrentLocation()
    {
        // Try to get from LocationManager or GameManager
        if (GameManager.Instance != null)
        {
            return GameManager.Instance.GetCurrentState().currentLocationID;
        }
        return "unknown";
    }

    /// <summary>
    /// Detects if there's a fish in the camera frame.
    /// </summary>
    private string DetectFishInFrame()
    {
        // Raycast or object detection to find fish in frame
        // This is a simplified version - real implementation would use object detection
        GameObject[] fishObjects = GameObject.FindGameObjectsWithTag("Fish");

        if (PhotoModeController.Instance != null)
        {
            Camera cam = PhotoModeController.Instance.GetPhotoCamera();

            foreach (GameObject fish in fishObjects)
            {
                Vector3 viewportPoint = cam.WorldToViewportPoint(fish.transform.position);

                // Check if fish is in frame and in front of camera
                if (viewportPoint.z > 0 &&
                    viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
                    viewportPoint.y >= 0 && viewportPoint.y <= 1)
                {
                    // Get fish species ID
                    FishInstance fishInstance = fish.GetComponent<FishInstance>();
                    if (fishInstance != null && fishInstance.speciesData != null)
                    {
                        return fishInstance.speciesData.fishID;
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Gets resolution dimensions.
    /// </summary>
    private void GetResolutionDimensions(ExportResolution resolution, out int width, out int height)
    {
        switch (resolution)
        {
            case ExportResolution.HD_1080p:
                width = 1920;
                height = 1080;
                break;
            case ExportResolution.QHD_1440p:
                width = 2560;
                height = 1440;
                break;
            case ExportResolution.UHD_4K:
                width = 3840;
                height = 2160;
                break;
            default:
                width = 1920;
                height = 1080;
                break;
        }
    }

    /// <summary>
    /// Loads photo gallery from save data.
    /// </summary>
    private void LoadPhotoGallery()
    {
        string galleryPath = Path.Combine(Application.persistentDataPath, "photo_gallery.json");

        if (File.Exists(galleryPath))
        {
            string json = File.ReadAllText(galleryPath);
            PhotoGalleryData data = JsonUtility.FromJson<PhotoGalleryData>(json);

            if (data != null)
            {
                photoGallery = data.photos;
                nextPhotoID = data.nextPhotoID;
                Debug.Log($"[PhotoStorage] Loaded {photoGallery.Count} photos from gallery");
            }
        }
    }

    /// <summary>
    /// Saves photo gallery to disk.
    /// </summary>
    private void SavePhotoGallery()
    {
        PhotoGalleryData data = new PhotoGalleryData
        {
            photos = photoGallery,
            nextPhotoID = nextPhotoID
        };

        string json = JsonUtility.ToJson(data, true);
        string galleryPath = Path.Combine(Application.persistentDataPath, "photo_gallery.json");
        File.WriteAllText(galleryPath, json);
    }

    #region Public API

    /// <summary>
    /// Gets all photos in gallery.
    /// </summary>
    public List<PhotoMetadata> GetPhotoGallery()
    {
        return new List<PhotoMetadata>(photoGallery);
    }

    /// <summary>
    /// Gets photo by ID.
    /// </summary>
    public PhotoMetadata GetPhotoByID(string photoID)
    {
        return photoGallery.Find(p => p.photoID == photoID);
    }

    /// <summary>
    /// Deletes a photo from gallery.
    /// </summary>
    public void DeletePhoto(string photoID)
    {
        PhotoMetadata photo = GetPhotoByID(photoID);
        if (photo != null)
        {
            if (File.Exists(photo.filePath))
            {
                File.Delete(photo.filePath);
            }

            photoGallery.Remove(photo);
            SavePhotoGallery();

            Debug.Log($"[PhotoStorage] Deleted photo: {photoID}");
        }
    }

    /// <summary>
    /// Gets photos with fish.
    /// </summary>
    public List<PhotoMetadata> GetPhotosWithFish()
    {
        return photoGallery.FindAll(p => !string.IsNullOrEmpty(p.fishSpeciesID));
    }

    /// <summary>
    /// Gets photos of a specific fish species.
    /// </summary>
    public List<PhotoMetadata> GetPhotosByFishSpecies(string fishSpeciesID)
    {
        return photoGallery.FindAll(p => p.fishSpeciesID == fishSpeciesID);
    }

    /// <summary>
    /// Gets total photo count.
    /// </summary>
    public int GetPhotoCount()
    {
        return photoGallery.Count;
    }

    /// <summary>
    /// Gets photo directory path.
    /// </summary>
    public string GetPhotoDirectory()
    {
        return fullPhotoPath;
    }

    /// <summary>
    /// Sets default export resolution.
    /// </summary>
    public void SetDefaultResolution(ExportResolution resolution)
    {
        defaultResolution = resolution;
    }

    /// <summary>
    /// Sets default export format.
    /// </summary>
    public void SetDefaultFormat(ExportFormat format)
    {
        defaultFormat = format;
    }

    #endregion
}

/// <summary>
/// Photo metadata containing all information about a captured photo.
/// </summary>
[System.Serializable]
public class PhotoMetadata
{
    public string photoID;
    public string fileName;
    public string filePath;
    public DateTime captureDate;
    public Vector2Int resolution;

    // Camera settings
    public Vector3 cameraPosition;
    public Quaternion cameraRotation;
    public float fov;
    public float exposure;

    // Game state
    public string locationID;
    public float timeOfDay;
    public string weatherType;

    // Fish information
    public string fishSpeciesID;
    public float qualityRating;

    // Filters applied
    public List<string> filtersApplied = new List<string>();

    // Additional metadata
    public string tags;
    public string notes;

    public PhotoMetadata()
    {
        captureDate = DateTime.Now;
        filtersApplied = new List<string>();
        qualityRating = 0f;
    }
}

/// <summary>
/// Serializable container for photo gallery data.
/// </summary>
[System.Serializable]
public class PhotoGalleryData
{
    public List<PhotoMetadata> photos = new List<PhotoMetadata>();
    public int nextPhotoID = 1;
}

/// <summary>
/// Export resolution options.
/// </summary>
[System.Serializable]
public enum ExportResolution
{
    HD_1080p,
    QHD_1440p,
    UHD_4K
}

/// <summary>
/// Export format options.
/// </summary>
[System.Serializable]
public enum ExportFormat
{
    PNG,
    JPG
}
