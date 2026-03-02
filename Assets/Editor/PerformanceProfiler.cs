using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using System.IO;
using System.Text;
using System;

/// <summary>
/// Performance profiling utility for Bahnfish
/// Collects and reports performance metrics
/// Based on PERFORMANCE_REPORT.md specifications
/// </summary>
public class PerformanceProfiler : EditorWindow
{
    private bool isProfiling = false;
    private float profilingDuration = 300f; // 5 minutes default
    private float elapsedTime = 0f;

    private StringBuilder report = new StringBuilder();

    // Performance metrics
    private float avgFPS = 0f;
    private float minFPS = float.MaxValue;
    private float maxFPS = 0f;
    private int frameCount = 0;

    private long totalMemory = 0;
    private long audioMemory = 0;
    private long textureMemory = 0;

    [MenuItem("Tools/Performance Profiler")]
    public static void ShowWindow()
    {
        GetWindow<PerformanceProfiler>("Performance Profiler");
    }

    private void OnGUI()
    {
        GUILayout.Label("Performance Profiler", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // Profiling controls
        GUILayout.Label("Profiling Settings:", EditorStyles.boldLabel);
        profilingDuration = EditorGUILayout.FloatField("Duration (seconds):", profilingDuration);

        GUILayout.Space(10);

        if (!isProfiling)
        {
            if (GUILayout.Button("Start Profiling", GUILayout.Height(30)))
            {
                StartProfiling();
            }
        }
        else
        {
            if (GUILayout.Button("Stop Profiling", GUILayout.Height(30)))
            {
                StopProfiling();
            }

            GUILayout.Space(10);
            GUILayout.Label($"Profiling... {elapsedTime:F1}s / {profilingDuration:F1}s");

            // Show current metrics
            GUILayout.Space(10);
            GUILayout.Label("Current Metrics:", EditorStyles.boldLabel);
            GUILayout.Label($"FPS: {1f / Time.deltaTime:F1}");
            GUILayout.Label($"Avg FPS: {avgFPS:F1}");
            GUILayout.Label($"Min FPS: {minFPS:F1}");
            GUILayout.Label($"Max FPS: {maxFPS:F1}");
            GUILayout.Label($"Total Memory: {totalMemory / 1024 / 1024} MB");
        }

        GUILayout.Space(20);

        // Quick profiling options
        GUILayout.Label("Quick Tests:", EditorStyles.boldLabel);
        if (GUILayout.Button("Profile 1 Minute"))
        {
            profilingDuration = 60f;
            StartProfiling();
        }
        if (GUILayout.Button("Profile 5 Minutes"))
        {
            profilingDuration = 300f;
            StartProfiling();
        }
        if (GUILayout.Button("Profile 30 Minutes"))
        {
            profilingDuration = 1800f;
            StartProfiling();
        }

        GUILayout.Space(10);

        // Export report
        if (GUILayout.Button("Export Report"))
        {
            ExportReport();
        }

        // Quick performance check
        GUILayout.Space(20);
        if (GUILayout.Button("Quick Performance Check"))
        {
            QuickPerformanceCheck();
        }
    }

    private void StartProfiling()
    {
        isProfiling = true;
        elapsedTime = 0f;
        frameCount = 0;
        avgFPS = 0f;
        minFPS = float.MaxValue;
        maxFPS = 0f;

        report.Clear();
        report.AppendLine("===========================================");
        report.AppendLine("Bahnfish Performance Report");
        report.AppendLine($"Date: {DateTime.Now}");
        report.AppendLine($"Unity Version: {Application.unityVersion}");
        report.AppendLine($"Target Platform: {EditorUserBuildSettings.activeBuildTarget}");
        report.AppendLine("===========================================\n");

        EditorApplication.update += OnUpdate;
        Debug.Log("Performance profiling started...");
    }

    private void StopProfiling()
    {
        isProfiling = false;
        EditorApplication.update -= OnUpdate;

        // Finalize report
        report.AppendLine("\n===========================================");
        report.AppendLine("Performance Summary");
        report.AppendLine("===========================================");
        report.AppendLine($"Duration: {elapsedTime:F1}s");
        report.AppendLine($"Frames: {frameCount}");
        report.AppendLine($"Average FPS: {avgFPS:F1}");
        report.AppendLine($"Min FPS: {minFPS:F1}");
        report.AppendLine($"Max FPS: {maxFPS:F1}");
        report.AppendLine($"Total Memory: {totalMemory / 1024 / 1024} MB");
        report.AppendLine($"Audio Memory: {audioMemory / 1024 / 1024} MB");
        report.AppendLine($"Texture Memory: {textureMemory / 1024 / 1024} MB");
        report.AppendLine("===========================================");

        Debug.Log("Performance profiling stopped!");
        Debug.Log($"Average FPS: {avgFPS:F1}");
    }

    private void OnUpdate()
    {
        if (!isProfiling) return;

        elapsedTime += Time.deltaTime;

        // Collect FPS
        float currentFPS = 1f / Time.deltaTime;
        avgFPS = (avgFPS * frameCount + currentFPS) / (frameCount + 1);
        minFPS = Mathf.Min(minFPS, currentFPS);
        maxFPS = Mathf.Max(maxFPS, currentFPS);
        frameCount++;

        // Collect memory metrics
        totalMemory = Profiler.GetTotalAllocatedMemoryLong();
        audioMemory = Profiler.GetAllocatedMemoryForGraphicsDriver();
        textureMemory = Profiler.GetAllocatedMemoryForGraphicsDriver();

        // Stop if duration reached
        if (elapsedTime >= profilingDuration)
        {
            StopProfiling();
        }

        Repaint();
    }

    private void ExportReport()
    {
        string path = EditorUtility.SaveFilePanel(
            "Export Performance Report",
            "",
            $"performance_report_{DateTime.Now:yyyy-MM-dd_HH-mm}.txt",
            "txt");

        if (string.IsNullOrEmpty(path)) return;

        File.WriteAllText(path, report.ToString());
        Debug.Log($"Performance report exported to: {path}");
        EditorUtility.RevealInFinder(path);
    }

    private void QuickPerformanceCheck()
    {
        Debug.Log("===========================================");
        Debug.Log("Quick Performance Check");
        Debug.Log("===========================================");

        // Memory
        long totalMem = Profiler.GetTotalAllocatedMemoryLong();
        long totalReserved = Profiler.GetTotalReservedMemoryLong();
        long totalUnused = Profiler.GetTotalUnusedReservedMemoryLong();

        Debug.Log($"Total Allocated: {totalMem / 1024 / 1024} MB");
        Debug.Log($"Total Reserved: {totalReserved / 1024 / 1024} MB");
        Debug.Log($"Total Unused: {totalUnused / 1024 / 1024} MB");

        // Check target met
        bool memoryTargetMet = (totalMem / 1024 / 1024) < 2048; // < 2GB
        Debug.Log($"Memory Target (<2GB): {(memoryTargetMet ? "✓ MET" : "✗ FAILED")}");

        // Audio sources
        var audioSources = FindObjectsOfType<AudioSource>();
        Debug.Log($"Active AudioSources: {audioSources.Length} (target: <32)");
        bool audioTargetMet = audioSources.Length <= 32;
        Debug.Log($"Audio Pooling Target: {(audioTargetMet ? "✓ MET" : "✗ FAILED")}");

        // Particle systems
        var particleSystems = FindObjectsOfType<ParticleSystem>();
        int totalParticles = 0;
        foreach (var ps in particleSystems)
        {
            totalParticles += ps.particleCount;
        }
        Debug.Log($"Active Particle Systems: {particleSystems.Length}");
        Debug.Log($"Total Particles: {totalParticles} (target: <10,000)");
        bool particleTargetMet = totalParticles <= 10000;
        Debug.Log($"Particle Pooling Target: {(particleTargetMet ? "✓ MET" : "✗ FAILED")}");

        // Quality settings
        Debug.Log($"Current Quality Level: {QualitySettings.GetQualityLevel()} ({QualitySettings.names[QualitySettings.GetQualityLevel()]})");
        Debug.Log($"VSync: {QualitySettings.vSyncCount}");
        Debug.Log($"Anisotropic Filtering: {QualitySettings.anisotropicFiltering}");

        // Scene stats
        var sceneStats = UnityEditor.SceneManagement.EditorSceneManager.GetSceneManagerSetup();
        Debug.Log($"Loaded Scenes: {sceneStats.Length}");

        Debug.Log("===========================================");
    }
}
