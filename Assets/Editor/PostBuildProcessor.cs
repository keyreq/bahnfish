using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using System.IO;

/// <summary>
/// Post-build processor to copy additional files and create archives
/// Automatically runs after each build
/// </summary>
public class PostBuildProcessor
{
    [PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        Debug.Log($"\n========== Post-Processing Build ==========");
        Debug.Log($"Target: {target}");
        Debug.Log($"Path: {pathToBuiltProject}");

        string buildDirectory = Path.GetDirectoryName(pathToBuiltProject);

        // Copy README and legal files
        CopyFileToBuild(buildDirectory, "PLAYER_MANUAL.md", "README.txt");
        CopyFileToBuild(buildDirectory, "CHANGELOG.md", "CHANGELOG.txt");

        // Copy license if it exists
        if (File.Exists("LICENSE"))
        {
            CopyFileToBuild(buildDirectory, "LICENSE", "LICENSE.txt");
        }

        // Platform-specific post-processing
        switch (target)
        {
            case BuildTarget.StandaloneWindows:
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
        Debug.Log("==========================================\n");
    }

    private static void PostProcessWindows(string buildDirectory)
    {
        Debug.Log("Windows post-processing...");

        // Create batch file for easy launching
        CreateWindowsLaunchBatch(buildDirectory);

        Debug.Log("  - Created launch.bat");
    }

    private static void PostProcessMac(string buildDirectory)
    {
        Debug.Log("macOS post-processing...");

        // Note: Code signing and notarization would happen here if certificates are available
        Debug.Log("  - macOS build ready (code signing not configured)");
    }

    private static void PostProcessLinux(string buildDirectory)
    {
        Debug.Log("Linux post-processing...");

        // Create .desktop file for Linux
        CreateLinuxDesktopFile(buildDirectory);

        // Create launch script
        CreateLinuxLaunchScript(buildDirectory);

        Debug.Log("  - Created .desktop file");
        Debug.Log("  - Created launch.sh");
    }

    private static void CopyFileToBuild(string buildDirectory, string sourceFile, string destFileName)
    {
        string sourcePath = Path.Combine(Directory.GetCurrentDirectory(), sourceFile);
        string destPath = Path.Combine(buildDirectory, destFileName);

        if (File.Exists(sourcePath))
        {
            File.Copy(sourcePath, destPath, true);
            Debug.Log($"  - Copied {sourceFile} → {destFileName}");
        }
        else
        {
            Debug.LogWarning($"  - Source file not found: {sourceFile}");
        }
    }

    private static void CreateWindowsLaunchBatch(string buildDirectory)
    {
        string batchFile = Path.Combine(buildDirectory, "launch.bat");
        string content = @"@echo off
echo Starting Bahnfish...
start Bahnfish.exe
";
        File.WriteAllText(batchFile, content);
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
    }

    private static void CreateLinuxLaunchScript(string buildDirectory)
    {
        string scriptFile = Path.Combine(buildDirectory, "launch.sh");
        string content = @"#!/bin/bash
echo ""Starting Bahnfish...""
./Bahnfish.x86_64
";
        File.WriteAllText(scriptFile, content);

        // Note: Setting execute permissions would require platform-specific code
        Debug.Log("  - Remember to set execute permissions: chmod +x launch.sh");
    }
}
