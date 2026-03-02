using UnityEditor;
using UnityEngine;
using System;
using System.IO;

/// <summary>
/// Automated build script for Bahnfish
/// Can be called from command line or CI/CD pipeline
///
/// Command line examples:
/// unity -quit -batchmode -executeMethod BuildScript.BuildAll
/// unity -quit -batchmode -executeMethod BuildScript.BuildWindows
/// </summary>
public class BuildScript
{
    // Build version (update this for each release)
    private const string VERSION = "1.0.0";

    // Build output directory
    private static readonly string BUILD_DIR = Path.Combine(Directory.GetCurrentDirectory(), "Builds");

    /// <summary>
    /// Build all platforms
    /// Command line: unity -quit -batchmode -executeMethod BuildScript.BuildAll
    /// </summary>
    [MenuItem("Build/Build All Platforms")]
    public static void BuildAll()
    {
        Debug.Log("==============================================");
        Debug.Log("Starting build for all platforms...");
        Debug.Log($"Version: {VERSION}");
        Debug.Log("==============================================");

        BuildWindows();
        BuildMac();
        BuildLinux();

        Debug.Log("==============================================");
        Debug.Log("All builds complete!");
        Debug.Log($"Builds available in: {BUILD_DIR}");
        Debug.Log("==============================================");
    }

    /// <summary>
    /// Build Windows (64-bit)
    /// Command line: unity -quit -batchmode -executeMethod BuildScript.BuildWindows
    /// </summary>
    [MenuItem("Build/Build Windows")]
    public static void BuildWindows()
    {
        string buildPath = Path.Combine(BUILD_DIR, $"Bahnfish_v{VERSION}_Windows", "Bahnfish.exe");
        BuildPlatform(BuildTarget.StandaloneWindows64, buildPath, "Windows");
    }

    /// <summary>
    /// Build macOS (Universal)
    /// Command line: unity -quit -batchmode -executeMethod BuildScript.BuildMac
    /// </summary>
    [MenuItem("Build/Build macOS")]
    public static void BuildMac()
    {
        string buildPath = Path.Combine(BUILD_DIR, $"Bahnfish_v{VERSION}_macOS", "Bahnfish.app");
        BuildPlatform(BuildTarget.StandaloneOSX, buildPath, "macOS");
    }

    /// <summary>
    /// Build Linux (64-bit)
    /// Command line: unity -quit -batchmode -executeMethod BuildScript.BuildLinux
    /// </summary>
    [MenuItem("Build/Build Linux")]
    public static void BuildLinux()
    {
        string buildPath = Path.Combine(BUILD_DIR, $"Bahnfish_v{VERSION}_Linux", "Bahnfish.x86_64");
        BuildPlatform(BuildTarget.StandaloneLinux64, buildPath, "Linux");
    }

    /// <summary>
    /// Build development build (with debugging)
    /// </summary>
    [MenuItem("Build/Build Development (Windows)")]
    public static void BuildDevelopment()
    {
        string buildPath = Path.Combine(BUILD_DIR, $"Bahnfish_v{VERSION}_Windows_Dev", "Bahnfish.exe");
        BuildPlatform(BuildTarget.StandaloneWindows64, buildPath, "Windows (Development)", true);
    }

    /// <summary>
    /// Core build method
    /// </summary>
    private static void BuildPlatform(BuildTarget target, string buildPath, string platformName, bool development = false)
    {
        Debug.Log($"\n========== Building {platformName} ==========");
        Debug.Log($"Target: {target}");
        Debug.Log($"Output: {buildPath}");
        Debug.Log($"Development: {development}");

        // Ensure build directory exists
        string directory = Path.GetDirectoryName(buildPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            Debug.Log($"Created directory: {directory}");
        }

        // Get scenes to build
        string[] scenes = GetScenePaths();
        Debug.Log($"Scenes to build: {scenes.Length}");
        foreach (string scene in scenes)
        {
            Debug.Log($"  - {scene}");
        }

        // Build options
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = target,
            options = development ? BuildOptions.Development : BuildOptions.None
        };

        // Perform build
        Debug.Log("Starting build process...");
        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

        // Check result
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"\n========== {platformName} Build SUCCEEDED ==========");
            Debug.Log($"Size: {report.summary.totalSize / (1024 * 1024)} MB");
            Debug.Log($"Time: {report.summary.totalTime}");
            Debug.Log($"Warnings: {report.summary.totalWarnings}");
            Debug.Log($"Errors: {report.summary.totalErrors}");
            Debug.Log("=====================================================\n");
        }
        else
        {
            Debug.LogError($"\n========== {platformName} Build FAILED ==========");
            Debug.LogError($"Result: {report.summary.result}");
            Debug.LogError($"Errors: {report.summary.totalErrors}");
            Debug.LogError($"Warnings: {report.summary.totalWarnings}");
            Debug.LogError("=================================================\n");

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(1); // Exit with error code
            }
        }
    }

    /// <summary>
    /// Get all scenes in build settings
    /// </summary>
    private static string[] GetScenePaths()
    {
        var scenes = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i] = EditorBuildSettings.scenes[i].path;
        }
        return scenes;
    }

    /// <summary>
    /// Configure build settings before building
    /// </summary>
    [MenuItem("Build/Configure Build Settings")]
    public static void ConfigureBuildSettings()
    {
        Debug.Log("Configuring build settings...");

        // Player settings
        PlayerSettings.companyName = "Your Studio Name";
        PlayerSettings.productName = "Bahnfish";
        PlayerSettings.bundleVersion = VERSION;

        // Graphics settings
        PlayerSettings.colorSpace = ColorSpace.Linear; // Better visuals
        PlayerSettings.MTRendering = true; // Multi-threaded rendering

        // Quality settings
        QualitySettings.vSyncCount = 0; // Let player choose via settings
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;

        // Platform-specific settings
        ConfigureWindowsSettings();
        ConfigureMacSettings();
        ConfigureLinuxSettings();

        Debug.Log("Build settings configured successfully!");
        Debug.Log($"Company: {PlayerSettings.companyName}");
        Debug.Log($"Product: {PlayerSettings.productName}");
        Debug.Log($"Version: {PlayerSettings.bundleVersion}");
    }

    private static void ConfigureWindowsSettings()
    {
        Debug.Log("Configuring Windows settings...");
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetArchitecture(BuildTargetGroup.Standalone, 1); // x86_64

        // Windows specific
        PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
        PlayerSettings.defaultScreenWidth = 1920;
        PlayerSettings.defaultScreenHeight = 1080;
        PlayerSettings.runInBackground = false;
    }

    private static void ConfigureMacSettings()
    {
        Debug.Log("Configuring macOS settings...");
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetArchitecture(BuildTargetGroup.Standalone, 2); // Universal (Intel + Apple Silicon)

        // macOS specific
        PlayerSettings.macOS.buildNumber = VERSION;
    }

    private static void ConfigureLinuxSettings()
    {
        Debug.Log("Configuring Linux settings...");
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetArchitecture(BuildTargetGroup.Standalone, 1); // x86_64
    }

    /// <summary>
    /// Clean build directory
    /// </summary>
    [MenuItem("Build/Clean Build Directory")]
    public static void CleanBuildDirectory()
    {
        if (Directory.Exists(BUILD_DIR))
        {
            Directory.Delete(BUILD_DIR, true);
            Debug.Log("Build directory cleaned!");
        }
        else
        {
            Debug.Log("Build directory doesn't exist, nothing to clean.");
        }
    }

    /// <summary>
    /// Show build directory in file explorer
    /// </summary>
    [MenuItem("Build/Show Build Directory")]
    public static void ShowBuildDirectory()
    {
        if (!Directory.Exists(BUILD_DIR))
        {
            Directory.CreateDirectory(BUILD_DIR);
        }
        EditorUtility.RevealInFinder(BUILD_DIR);
    }

    /// <summary>
    /// Print build info
    /// </summary>
    [MenuItem("Build/Print Build Info")]
    public static void PrintBuildInfo()
    {
        Debug.Log("==============================================");
        Debug.Log("Bahnfish Build Information");
        Debug.Log("==============================================");
        Debug.Log($"Version: {VERSION}");
        Debug.Log($"Unity Version: {Application.unityVersion}");
        Debug.Log($"Company: {PlayerSettings.companyName}");
        Debug.Log($"Product: {PlayerSettings.productName}");
        Debug.Log($"Build Directory: {BUILD_DIR}");
        Debug.Log($"Scenes in Build: {EditorBuildSettings.scenes.Length}");
        Debug.Log("==============================================");
    }
}
