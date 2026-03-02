# Bahnfish - Build Automation

**Platform**: Unity 2022 LTS
**CI/CD**: GitHub Actions / Jenkins / Local Scripts
**Status**: Phase 6 - Build Pipeline Setup

---

## Overview

This document contains build automation scripts and procedures for creating Bahnfish builds across multiple platforms.

---

## Unity Build Script

Create this file as `Assets/Editor/BuildScript.cs`:

```csharp
using UnityEditor;
using UnityEngine;
using System;
using System.IO;

/// <summary>
/// Automated build script for Bahnfish
/// Can be called from command line or CI/CD pipeline
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
        Debug.Log("Starting build for all platforms...");

        BuildWindows();
        BuildMac();
        BuildLinux();

        Debug.Log("All builds complete!");
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
        Debug.Log($"Building {platformName}...");
        Debug.Log($"Output: {buildPath}");

        // Ensure build directory exists
        string directory = Path.GetDirectoryName(buildPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Get scenes to build
        string[] scenes = GetScenePaths();

        // Build options
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = target,
            options = development ? BuildOptions.Development : BuildOptions.None
        };

        // Perform build
        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

        // Check result
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"{platformName} build succeeded!");
            Debug.Log($"Size: {report.summary.totalSize / (1024 * 1024)} MB");
            Debug.Log($"Time: {report.summary.totalTime}");
        }
        else
        {
            Debug.LogError($"{platformName} build failed!");
            Debug.LogError($"Errors: {report.summary.totalErrors}");

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
        // Player settings
        PlayerSettings.companyName = "Your Studio Name";
        PlayerSettings.productName = "Bahnfish";
        PlayerSettings.bundleVersion = VERSION;

        // Graphics settings
        PlayerSettings.colorSpace = ColorSpace.Linear; // Better visuals
        PlayerSettings.MTRendering = true; // Multi-threaded rendering

        // Platform-specific settings
        ConfigureWindowsSettings();
        ConfigureMacSettings();
        ConfigureLinuxSettings();

        Debug.Log("Build settings configured!");
    }

    private static void ConfigureWindowsSettings()
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetArchitecture(BuildTargetGroup.Standalone, 1); // x86_64
    }

    private static void ConfigureMacSettings()
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetArchitecture(BuildTargetGroup.Standalone, 2); // Universal (Intel + Apple Silicon)
    }

    private static void ConfigureLinuxSettings()
    {
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
    }
}
```

---

## Batch Build Scripts

### Windows (Build All Platforms)

Create `build_all.bat`:

```batch
@echo off
echo ================================================
echo Bahnfish - Build All Platforms
echo ================================================

REM Set Unity path (adjust to your installation)
set UNITY="C:\Program Files\Unity\Hub\Editor\2022.3.XX\Editor\Unity.exe"

REM Set project path
set PROJECT_PATH=%cd%

echo.
echo Configuring build settings...
%UNITY% -quit -batchmode -projectPath "%PROJECT_PATH%" -executeMethod BuildScript.ConfigureBuildSettings -logFile build_config.log

echo.
echo Building Windows...
%UNITY% -quit -batchmode -projectPath "%PROJECT_PATH%" -executeMethod BuildScript.BuildWindows -logFile build_windows.log

echo.
echo Building macOS...
%UNITY% -quit -batchmode -projectPath "%PROJECT_PATH%" -executeMethod BuildScript.BuildMac -logFile build_mac.log

echo.
echo Building Linux...
%UNITY% -quit -batchmode -projectPath "%PROJECT_PATH%" -executeMethod BuildScript.BuildLinux -logFile build_linux.log

echo.
echo ================================================
echo All builds complete! Check Builds/ directory
echo ================================================
pause
```

### macOS/Linux (Build All Platforms)

Create `build_all.sh`:

```bash
#!/bin/bash

echo "================================================"
echo "Bahnfish - Build All Platforms"
echo "================================================"

# Set Unity path (adjust to your installation)
UNITY="/Applications/Unity/Hub/Editor/2022.3.XX/Unity.app/Contents/MacOS/Unity"

# Set project path
PROJECT_PATH="$(pwd)"

echo ""
echo "Configuring build settings..."
"$UNITY" -quit -batchmode -projectPath "$PROJECT_PATH" -executeMethod BuildScript.ConfigureBuildSettings -logFile build_config.log

echo ""
echo "Building Windows..."
"$UNITY" -quit -batchmode -projectPath "$PROJECT_PATH" -executeMethod BuildScript.BuildWindows -logFile build_windows.log

echo ""
echo "Building macOS..."
"$UNITY" -quit -batchmode -projectPath "$PROJECT_PATH" -executeMethod BuildScript.BuildMac -logFile build_mac.log

echo ""
echo "Building Linux..."
"$UNITY" -quit -batchmode -projectPath "$PROJECT_PATH" -executeMethod BuildScript.BuildLinux -logFile build_linux.log

echo ""
echo "================================================"
echo "All builds complete! Check Builds/ directory"
echo "================================================"
```

Make executable: `chmod +x build_all.sh`

---

## GitHub Actions CI/CD

Create `.github/workflows/build.yml`:

```yaml
name: Build Bahnfish

on:
  push:
    branches: [ master, develop ]
  pull_request:
    branches: [ master ]
  workflow_dispatch: # Manual trigger

env:
  UNITY_VERSION: 2022.3.XX # Update to your Unity version

jobs:
  build:
    name: Build for ${{ matrix.targetPlatform }}
    runs-on: ubuntu-latest
    strategy:
      fail-fast: false
      matrix:
        targetPlatform:
          - StandaloneWindows64
          - StandaloneOSX
          - StandaloneLinux64

    steps:
      # Checkout repository
      - name: Checkout repository
        uses: actions/checkout@v3
        with:
          lfs: true

      # Cache Library folder
      - name: Cache Unity Library
        uses: actions/cache@v3
        with:
          path: Library
          key: Library-${{ matrix.targetPlatform }}-${{ hashFiles('Assets/**', 'Packages/**', 'ProjectSettings/**') }}
          restore-keys: |
            Library-${{ matrix.targetPlatform }}-
            Library-

      # Build project
      - name: Build project
        uses: game-ci/unity-builder@v3
        env:
          UNITY_LICENSE: ${{ secrets.UNITY_LICENSE }}
          UNITY_EMAIL: ${{ secrets.UNITY_EMAIL }}
          UNITY_PASSWORD: ${{ secrets.UNITY_PASSWORD }}
        with:
          targetPlatform: ${{ matrix.targetPlatform }}
          buildMethod: BuildScript.BuildPlatform
          versioning: Semantic

      # Upload build artifact
      - name: Upload build
        uses: actions/upload-artifact@v3
        with:
          name: Bahnfish-${{ matrix.targetPlatform }}
          path: build/${{ matrix.targetPlatform }}

      # Upload to itch.io (optional, on release)
      - name: Deploy to itch.io
        if: startsWith(github.ref, 'refs/tags/')
        uses: manleydev/butler-publish-itchio-action@v1.0.3
        env:
          BUTLER_CREDENTIALS: ${{ secrets.BUTLER_API_KEY }}
          CHANNEL: ${{ matrix.targetPlatform }}
          ITCH_GAME: bahnfish
          ITCH_USER: yourusername
          PACKAGE: build/${{ matrix.targetPlatform }}
          VERSION: ${{ github.ref_name }}
```

---

## Post-Build Processing

Create `Assets/Editor/PostBuildProcessor.cs`:

```csharp
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using System.IO;

/// <summary>
/// Post-build processor to copy additional files and create archives
/// </summary>
public class PostBuildProcessor
{
    [PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        Debug.Log($"Post-processing build for {target}...");

        string buildDirectory = Path.GetDirectoryName(pathToBuiltProject);

        // Copy README and legal files
        CopyFileToBuild(buildDirectory, "PLAYER_MANUAL.md", "README.txt");
        CopyFileToBuild(buildDirectory, "LICENSE", "LICENSE.txt");

        // Platform-specific post-processing
        switch (target)
        {
            case BuildTarget.StandaloneWindows64:
                PostProcessWindows(buildDirectory);
                break;
            case BuildTarget.StandaloneOSX:
                PostProcessMac(buildDirectory);
                break;
            case BuildTarget.StandaloneLinux64:
                PostProcessLinux(buildDirectory);
                break;
        }

        Debug.Log("Post-processing complete!");
    }

    private static void PostProcessWindows(string buildDirectory)
    {
        Debug.Log("Windows post-processing...");
        // Add Windows-specific files (DirectX installer, etc.)
    }

    private static void PostProcessMac(string buildDirectory)
    {
        Debug.Log("macOS post-processing...");
        // Code signing (if certificates available)
        // Notarization (for macOS distribution)
    }

    private static void PostProcessLinux(string buildDirectory)
    {
        Debug.Log("Linux post-processing...");
        // Create .desktop file for Linux
        CreateLinuxDesktopFile(buildDirectory);
    }

    private static void CopyFileToBuild(string buildDirectory, string sourceFile, string destFileName)
    {
        string sourcePath = Path.Combine(Directory.GetCurrentDirectory(), sourceFile);
        string destPath = Path.Combine(buildDirectory, destFileName);

        if (File.Exists(sourcePath))
        {
            File.Copy(sourcePath, destPath, true);
            Debug.Log($"Copied {sourceFile} to build");
        }
        else
        {
            Debug.LogWarning($"Source file not found: {sourceFile}");
        }
    }

    private static void CreateLinuxDesktopFile(string buildDirectory)
    {
        string desktopFile = Path.Combine(buildDirectory, "Bahnfish.desktop");
        string content = @"[Desktop Entry]
Name=Bahnfish
Comment=Cozy fishing meets cosmic horror
Exec=./Bahnfish.x86_64
Icon=UnityPlayer.png
Terminal=false
Type=Application
Categories=Game;
";
        File.WriteAllText(desktopFile, content);
        Debug.Log("Created .desktop file for Linux");
    }
}
```

---

## Compression and Packaging

### Windows PowerShell Script

Create `package_builds.ps1`:

```powershell
# Package builds into ZIP archives

$version = "1.0.0"
$buildsDir = "Builds"

Write-Host "Packaging builds for distribution..." -ForegroundColor Green

# Package Windows build
$windowsDir = "$buildsDir\Bahnfish_v${version}_Windows"
if (Test-Path $windowsDir) {
    Write-Host "Packaging Windows build..."
    Compress-Archive -Path $windowsDir -DestinationPath "$buildsDir\Bahnfish_v${version}_Windows.zip" -Force
}

# Package macOS build
$macDir = "$buildsDir\Bahnfish_v${version}_macOS"
if (Test-Path $macDir) {
    Write-Host "Packaging macOS build..."
    Compress-Archive -Path $macDir -DestinationPath "$buildsDir\Bahnfish_v${version}_macOS.zip" -Force
}

# Package Linux build
$linuxDir = "$buildsDir\Bahnfish_v${version}_Linux"
if (Test-Path $linuxDir) {
    Write-Host "Packaging Linux build..."
    Compress-Archive -Path $linuxDir -DestinationPath "$buildsDir\Bahnfish_v${version}_Linux.zip" -Force
}

Write-Host "Packaging complete!" -ForegroundColor Green
Write-Host "Archives created in $buildsDir directory"
```

### Bash Script

Create `package_builds.sh`:

```bash
#!/bin/bash

VERSION="1.0.0"
BUILDS_DIR="Builds"

echo "Packaging builds for distribution..."

# Package Windows build
if [ -d "$BUILDS_DIR/Bahnfish_v${VERSION}_Windows" ]; then
    echo "Packaging Windows build..."
    cd "$BUILDS_DIR"
    zip -r "Bahnfish_v${VERSION}_Windows.zip" "Bahnfish_v${VERSION}_Windows"
    cd ..
fi

# Package macOS build
if [ -d "$BUILDS_DIR/Bahnfish_v${VERSION}_macOS" ]; then
    echo "Packaging macOS build..."
    cd "$BUILDS_DIR"
    zip -r "Bahnfish_v${VERSION}_macOS.zip" "Bahnfish_v${VERSION}_macOS"
    cd ..
fi

# Package Linux build
if [ -d "$BUILDS_DIR/Bahnfish_v${VERSION}_Linux" ]; then
    echo "Packaging Linux build..."
    cd "$BUILDS_DIR"
    tar -czf "Bahnfish_v${VERSION}_Linux.tar.gz" "Bahnfish_v${VERSION}_Linux"
    cd ..
fi

echo "Packaging complete!"
echo "Archives created in $BUILDS_DIR directory"
```

---

## Build Verification Checklist

After each build, verify:

### All Platforms
- [ ] Game launches without errors
- [ ] Main menu loads
- [ ] New game starts successfully
- [ ] Save/Load works
- [ ] Settings persist
- [ ] Audio plays correctly
- [ ] All scenes load
- [ ] No missing assets
- [ ] Version number correct
- [ ] README included
- [ ] File size reasonable (< 2GB)

### Windows Specific
- [ ] .exe runs on Windows 10/11
- [ ] No administrator privileges required
- [ ] Installer works (if applicable)
- [ ] Uninstaller works (if applicable)

### macOS Specific
- [ ] .app bundle properly signed
- [ ] Notarized (if distributing outside App Store)
- [ ] Runs on Intel and Apple Silicon
- [ ] No Gatekeeper warnings

### Linux Specific
- [ ] Binary has execute permissions
- [ ] .desktop file works
- [ ] Dependencies documented
- [ ] Runs on Ubuntu 20.04+ / similar distros

---

## Distribution Checklist

### Steam
- [ ] Upload builds via SteamPipe
- [ ] Set depots (Windows/Mac/Linux)
- [ ] Test Steam Cloud saves
- [ ] Test Steam Achievements (if applicable)
- [ ] Verify store page
- [ ] Set launch options

### Itch.io
- [ ] Upload builds (Windows/Mac/Linux)
- [ ] Set pricing
- [ ] Upload screenshots
- [ ] Set tags
- [ ] Configure downloads
- [ ] Test downloads

### Epic Games Store
- [ ] Register product
- [ ] Upload builds
- [ ] Set up store page
- [ ] Configure achievements (if applicable)
- [ ] Submit for review

### GOG
- [ ] Upload builds
- [ ] Create installer
- [ ] Submit for curation
- [ ] Provide DRM-free guarantee

---

## Build Troubleshooting

### Common Build Errors

**Error**: "Building project failed"
- **Solution**: Check build logs for specific errors. Verify all scenes are in Build Settings.

**Error**: "Scripting backend not supported"
- **Solution**: Ensure IL2CPP is installed for target platform.

**Error**: "Module 'Windows' not installed"
- **Solution**: Install platform support via Unity Hub.

**Error**: "Build size too large (>2GB)"
- **Solution**: Enable compression, remove unused assets, optimize textures.

**Error**: "Missing dependencies on Linux"
- **Solution**: Document required libraries in README.

---

## Automated Testing (Optional)

Create `Assets/Editor/BuildTests.cs` for automated testing:

```csharp
using UnityEngine;
using UnityEditor;
using NUnit.Framework;

/// <summary>
/// Automated tests to run before building
/// </summary>
public class BuildTests
{
    [Test]
    public void VerifyAllScenesInBuild()
    {
        Assert.Greater(EditorBuildSettings.scenes.Length, 0, "No scenes in build settings!");
    }

    [Test]
    public void VerifyPlayerSettings()
    {
        Assert.IsNotEmpty(PlayerSettings.companyName, "Company name not set!");
        Assert.IsNotEmpty(PlayerSettings.productName, "Product name not set!");
        Assert.IsNotEmpty(PlayerSettings.bundleVersion, "Bundle version not set!");
    }

    [Test]
    public void VerifyNoMissingScripts()
    {
        // Check for missing script references
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject go in allObjects)
        {
            Component[] components = go.GetComponents<Component>();
            foreach (Component c in components)
            {
                Assert.IsNotNull(c, $"Missing script on {go.name}!");
            }
        }
    }
}
```

---

## Build Schedule Recommendation

### Development Builds
- **Frequency**: Daily (automated via CI)
- **Purpose**: Testing, QA
- **Distribution**: Internal only

### Release Candidate Builds
- **Frequency**: Weekly during Phase 6
- **Purpose**: Beta testing, bug verification
- **Distribution**: Beta testers

### Final Release Build
- **Timing**: Week 36 (end of Phase 6)
- **Purpose**: Public release
- **Distribution**: Steam, Itch.io, etc.

### Patch Builds
- **Frequency**: As needed post-launch
- **Purpose**: Bug fixes, hotfixes
- **Distribution**: Automatic updates

---

## Version Numbering

Use Semantic Versioning (MAJOR.MINOR.PATCH):

- **1.0.0**: Initial release
- **1.0.1**: Bug fix patch
- **1.1.0**: New features (minor update)
- **2.0.0**: Major update (breaking changes)

Example:
- 1.0.0: Launch
- 1.0.1: Day-1 hotfix
- 1.1.0: New location DLC
- 1.2.0: Quality of life features
- 2.0.0: Multiplayer update

---

## Additional Resources

### Unity Documentation
- [Build Player Pipeline](https://docs.unity3d.com/Manual/BuildPlayerPipeline.html)
- [Command Line Arguments](https://docs.unity3d.com/Manual/CommandLineArguments.html)
- [IL2CPP Overview](https://docs.unity3d.com/Manual/IL2CPP.html)

### CI/CD Resources
- [Unity GameCI](https://game.ci/)
- [GitHub Actions for Unity](https://unity.com/solutions/ci-cd)
- [Butler (itch.io)](https://itch.io/docs/butler/)

---

*Build automation setup complete! Update version numbers and Unity paths before building.*
