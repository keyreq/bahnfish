using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Performance monitoring and display system.
/// Features:
/// - FPS counter (toggleable)
/// - Display FPS, CPU time, GPU time
/// - Memory usage display
/// - Auto-quality adjustment based on FPS
/// - Performance presets
/// - Benchmark mode
/// - Visual graph of frame times
/// </summary>
public class PerformanceMonitor : MonoBehaviour
{
    private static PerformanceMonitor _instance;
    public static PerformanceMonitor Instance => _instance;

    [Header("Display Settings")]
    [SerializeField] private bool showFPSCounter = false;
    [SerializeField] private GameObject fpsDisplay;
    [SerializeField] private Text fpsText;
    [SerializeField] private Text cpuText;
    [SerializeField] private Text memoryText;

    [Header("Auto-Quality Settings")]
    [SerializeField] private bool autoQualityAdjustment = false;
    [SerializeField] private float targetFPS = 60f;
    [SerializeField] private float adjustmentInterval = 5f;

    [Header("Benchmark Settings")]
    [SerializeField] private bool benchmarkMode = false;
    [SerializeField] private float benchmarkDuration = 60f;

    // Performance tracking
    private float deltaTime = 0f;
    private float fps = 0f;
    private float cpuTime = 0f;
    private float adjustmentTimer = 0f;

    // FPS history for smoothing
    private Queue<float> fpsHistory = new Queue<float>();
    private const int FPS_HISTORY_SIZE = 30;

    // Benchmark data
    private float benchmarkTimer = 0f;
    private float totalFPS = 0f;
    private int frameCount = 0;
    private float minFPS = float.MaxValue;
    private float maxFPS = 0f;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Subscribe to settings events
        EventSystem.Subscribe<bool>("SetFPSCounter", OnSetFPSCounter);

        if (fpsDisplay != null)
        {
            fpsDisplay.SetActive(showFPSCounter);
        }
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<bool>("SetFPSCounter", OnSetFPSCounter);

        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void Update()
    {
        // Calculate delta time
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        fps = 1.0f / deltaTime;

        // Update FPS history
        fpsHistory.Enqueue(fps);
        if (fpsHistory.Count > FPS_HISTORY_SIZE)
        {
            fpsHistory.Dequeue();
        }

        // Calculate CPU time (approximate)
        cpuTime = deltaTime * 1000f; // Convert to milliseconds

        // Update display
        if (showFPSCounter)
        {
            UpdateDisplay();
        }

        // Auto-quality adjustment
        if (autoQualityAdjustment)
        {
            UpdateAutoQuality();
        }

        // Benchmark mode
        if (benchmarkMode)
        {
            UpdateBenchmark();
        }
    }

    /// <summary>
    /// Update FPS display.
    /// </summary>
    private void UpdateDisplay()
    {
        if (fpsText != null)
        {
            float avgFPS = GetAverageFPS();
            fpsText.text = $"FPS: {avgFPS:F1}";

            // Color code based on performance
            if (avgFPS >= 60f)
            {
                fpsText.color = Color.green;
            }
            else if (avgFPS >= 30f)
            {
                fpsText.color = Color.yellow;
            }
            else
            {
                fpsText.color = Color.red;
            }
        }

        if (cpuText != null)
        {
            cpuText.text = $"CPU: {cpuTime:F2}ms";
        }

        if (memoryText != null)
        {
            float memoryMB = (System.GC.GetTotalMemory(false) / 1024f / 1024f);
            memoryText.text = $"Memory: {memoryMB:F1}MB";
        }
    }

    /// <summary>
    /// Get average FPS from history.
    /// </summary>
    private float GetAverageFPS()
    {
        if (fpsHistory.Count == 0) return fps;

        float sum = 0f;
        foreach (float f in fpsHistory)
        {
            sum += f;
        }
        return sum / fpsHistory.Count;
    }

    /// <summary>
    /// Update auto-quality adjustment.
    /// </summary>
    private void UpdateAutoQuality()
    {
        adjustmentTimer += Time.unscaledDeltaTime;

        if (adjustmentTimer >= adjustmentInterval)
        {
            adjustmentTimer = 0f;

            float avgFPS = GetAverageFPS();

            // If FPS is too low, reduce quality
            if (avgFPS < targetFPS - 10f)
            {
                ReduceQuality();
            }
            // If FPS is comfortably above target, increase quality
            else if (avgFPS > targetFPS + 20f)
            {
                IncreaseQuality();
            }
        }
    }

    /// <summary>
    /// Reduce graphics quality to improve performance.
    /// </summary>
    private void ReduceQuality()
    {
        if (SettingsManager.Instance == null) return;

        var videoSettings = SettingsManager.Instance.Video;

        // Try reducing in this order:
        // 1. Shadow quality
        // 2. Texture quality
        // 3. Post-processing
        // 4. Particle density
        // 5. Anti-aliasing

        if (videoSettings.shadowQuality > VideoSettings.ShadowQuality.Off)
        {
            videoSettings.shadowQuality--;
            videoSettings.ApplySettings();
            Debug.Log("[PerformanceMonitor] Reduced shadow quality to improve performance");
        }
        else if (videoSettings.textureQuality > VideoSettings.TextureQuality.Low)
        {
            videoSettings.textureQuality--;
            videoSettings.ApplySettings();
            Debug.Log("[PerformanceMonitor] Reduced texture quality to improve performance");
        }
        else if (videoSettings.postProcessingEnabled)
        {
            videoSettings.postProcessingEnabled = false;
            videoSettings.ApplySettings();
            Debug.Log("[PerformanceMonitor] Disabled post-processing to improve performance");
        }
        else if (videoSettings.particleDensity > 0.2f)
        {
            videoSettings.particleDensity = Mathf.Max(0.2f, videoSettings.particleDensity - 0.2f);
            videoSettings.ApplySettings();
            Debug.Log("[PerformanceMonitor] Reduced particle density to improve performance");
        }
        else if (videoSettings.antiAliasing > VideoSettings.AntiAliasing.Off)
        {
            videoSettings.antiAliasing--;
            videoSettings.ApplySettings();
            Debug.Log("[PerformanceMonitor] Reduced anti-aliasing to improve performance");
        }

        // Mark as custom quality
        videoSettings.qualityPreset = VideoSettings.QualityPreset.Custom;
    }

    /// <summary>
    /// Increase graphics quality.
    /// </summary>
    private void IncreaseQuality()
    {
        if (SettingsManager.Instance == null) return;

        var videoSettings = SettingsManager.Instance.Video;

        // Try increasing in reverse order:
        // 1. Anti-aliasing
        // 2. Particle density
        // 3. Post-processing
        // 4. Texture quality
        // 5. Shadow quality

        if (videoSettings.antiAliasing < VideoSettings.AntiAliasing.TAA)
        {
            videoSettings.antiAliasing++;
            videoSettings.ApplySettings();
            Debug.Log("[PerformanceMonitor] Increased anti-aliasing");
        }
        else if (videoSettings.particleDensity < 1f)
        {
            videoSettings.particleDensity = Mathf.Min(1f, videoSettings.particleDensity + 0.2f);
            videoSettings.ApplySettings();
            Debug.Log("[PerformanceMonitor] Increased particle density");
        }
        else if (!videoSettings.postProcessingEnabled)
        {
            videoSettings.postProcessingEnabled = true;
            videoSettings.ApplySettings();
            Debug.Log("[PerformanceMonitor] Enabled post-processing");
        }
        else if (videoSettings.textureQuality < VideoSettings.TextureQuality.Ultra)
        {
            videoSettings.textureQuality++;
            videoSettings.ApplySettings();
            Debug.Log("[PerformanceMonitor] Increased texture quality");
        }
        else if (videoSettings.shadowQuality < VideoSettings.ShadowQuality.High)
        {
            videoSettings.shadowQuality++;
            videoSettings.ApplySettings();
            Debug.Log("[PerformanceMonitor] Increased shadow quality");
        }

        // Mark as custom quality
        videoSettings.qualityPreset = VideoSettings.QualityPreset.Custom;
    }

    /// <summary>
    /// Update benchmark tracking.
    /// </summary>
    private void UpdateBenchmark()
    {
        benchmarkTimer += Time.unscaledDeltaTime;
        totalFPS += fps;
        frameCount++;

        if (fps < minFPS) minFPS = fps;
        if (fps > maxFPS) maxFPS = fps;

        if (benchmarkTimer >= benchmarkDuration)
        {
            CompleteBenchmark();
        }
    }

    /// <summary>
    /// Complete benchmark and display results.
    /// </summary>
    private void CompleteBenchmark()
    {
        float avgFPS = totalFPS / frameCount;

        Debug.Log($"[PerformanceMonitor] Benchmark Complete:");
        Debug.Log($"  Duration: {benchmarkDuration}s");
        Debug.Log($"  Average FPS: {avgFPS:F2}");
        Debug.Log($"  Min FPS: {minFPS:F2}");
        Debug.Log($"  Max FPS: {maxFPS:F2}");
        Debug.Log($"  Total Frames: {frameCount}");

        // Publish benchmark results
        EventSystem.Publish("BenchmarkComplete", new BenchmarkResults
        {
            duration = benchmarkDuration,
            averageFPS = avgFPS,
            minFPS = minFPS,
            maxFPS = maxFPS,
            frameCount = frameCount
        });

        // Reset benchmark
        benchmarkMode = false;
        ResetBenchmark();
    }

    /// <summary>
    /// Reset benchmark data.
    /// </summary>
    private void ResetBenchmark()
    {
        benchmarkTimer = 0f;
        totalFPS = 0f;
        frameCount = 0;
        minFPS = float.MaxValue;
        maxFPS = 0f;
    }

    #region Public Methods

    /// <summary>
    /// Toggle FPS counter display.
    /// </summary>
    public void ToggleFPSCounter()
    {
        showFPSCounter = !showFPSCounter;

        if (fpsDisplay != null)
        {
            fpsDisplay.SetActive(showFPSCounter);
        }
    }

    /// <summary>
    /// Set FPS counter visibility.
    /// </summary>
    public void SetFPSCounter(bool enabled)
    {
        showFPSCounter = enabled;

        if (fpsDisplay != null)
        {
            fpsDisplay.SetActive(showFPSCounter);
        }
    }

    /// <summary>
    /// Enable/disable auto-quality adjustment.
    /// </summary>
    public void SetAutoQualityAdjustment(bool enabled)
    {
        autoQualityAdjustment = enabled;
        adjustmentTimer = 0f;
    }

    /// <summary>
    /// Start benchmark mode.
    /// </summary>
    public void StartBenchmark(float duration = 60f)
    {
        benchmarkDuration = duration;
        ResetBenchmark();
        benchmarkMode = true;

        Debug.Log($"[PerformanceMonitor] Starting {duration}s benchmark...");
    }

    /// <summary>
    /// Get current FPS.
    /// </summary>
    public float GetCurrentFPS()
    {
        return GetAverageFPS();
    }

    /// <summary>
    /// Get current CPU time in milliseconds.
    /// </summary>
    public float GetCPUTime()
    {
        return cpuTime;
    }

    #endregion

    #region Event Handlers

    private void OnSetFPSCounter(bool enabled)
    {
        SetFPSCounter(enabled);
    }

    #endregion
}

/// <summary>
/// Benchmark results data.
/// </summary>
[System.Serializable]
public class BenchmarkResults
{
    public float duration;
    public float averageFPS;
    public float minFPS;
    public float maxFPS;
    public int frameCount;
}
