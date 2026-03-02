using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using System.Linq;

/// <summary>
/// Pre-build validation tests
/// These tests verify the project is ready for building
/// Run before creating release builds
/// </summary>
public class BuildTests
{
    [Test]
    public void Test_ScenesExistInBuildSettings()
    {
        Assert.Greater(EditorBuildSettings.scenes.Length, 0, "No scenes in build settings!");
        Debug.Log($"✓ {EditorBuildSettings.scenes.Length} scenes in build settings");
    }

    [Test]
    public void Test_AllScenesEnabled()
    {
        var disabledScenes = EditorBuildSettings.scenes.Where(s => !s.enabled).ToArray();
        Assert.IsEmpty(disabledScenes, $"Some scenes are disabled in build settings: {string.Join(", ", disabledScenes.Select(s => s.path))}");
        Debug.Log("✓ All scenes enabled in build settings");
    }

    [Test]
    public void Test_PlayerSettingsConfigured()
    {
        Assert.IsNotEmpty(PlayerSettings.companyName, "Company name not set!");
        Assert.IsNotEmpty(PlayerSettings.productName, "Product name not set!");
        Assert.IsNotEmpty(PlayerSettings.bundleVersion, "Bundle version not set!");

        Debug.Log($"✓ Company: {PlayerSettings.companyName}");
        Debug.Log($"✓ Product: {PlayerSettings.productName}");
        Debug.Log($"✓ Version: {PlayerSettings.bundleVersion}");
    }

    [Test]
    public void Test_ColorSpaceIsLinear()
    {
        Assert.AreEqual(ColorSpace.Linear, PlayerSettings.colorSpace, "Color space should be Linear for better visuals!");
        Debug.Log("✓ Color space is Linear");
    }

    [Test]
    public void Test_MultiThreadedRenderingEnabled()
    {
        Assert.IsTrue(PlayerSettings.MTRendering, "Multi-threaded rendering should be enabled for better performance!");
        Debug.Log("✓ Multi-threaded rendering enabled");
    }

    [Test]
    public void Test_NoMissingScriptsInResources()
    {
        // Check for missing script references in Resources folder
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject go in allObjects)
        {
            Component[] components = go.GetComponents<Component>();
            foreach (Component c in components)
            {
                Assert.IsNotNull(c, $"Missing script on GameObject: {go.name}!");
            }
        }
        Debug.Log($"✓ No missing scripts found in {allObjects.Length} GameObjects");
    }

    [Test]
    public void Test_RequiredFoldersExist()
    {
        string[] requiredFolders = new string[]
        {
            "Assets/Scripts",
            "Assets/Scripts/Core",
            "Assets/Scripts/Player",
            "Assets/Scripts/Fishing",
            "Assets/Scripts/Inventory"
        };

        foreach (string folder in requiredFolders)
        {
            Assert.IsTrue(AssetDatabase.IsValidFolder(folder), $"Required folder missing: {folder}");
        }

        Debug.Log($"✓ All {requiredFolders.Length} required folders exist");
    }

    [Test]
    public void Test_AudioSettingsConfigured()
    {
        AudioConfiguration audioConfig = AudioSettings.GetConfiguration();
        Assert.Greater(audioConfig.sampleRate, 0, "Audio sample rate not configured!");
        Debug.Log($"✓ Audio configured: {audioConfig.sampleRate}Hz, {audioConfig.numVirtualVoices} voices");
    }

    [Test]
    public void Test_QualitySettingsExist()
    {
        string[] qualityLevels = QualitySettings.names;
        Assert.Greater(qualityLevels.Length, 0, "No quality levels configured!");

        // Check for expected quality levels
        Assert.Contains("Low", qualityLevels, "Missing 'Low' quality level");
        Assert.Contains("Medium", qualityLevels, "Missing 'Medium' quality level");
        Assert.Contains("High", qualityLevels, "Missing 'High' quality level");

        Debug.Log($"✓ {qualityLevels.Length} quality levels configured: {string.Join(", ", qualityLevels)}");
    }

    [Test]
    public void Test_TargetFrameRateReasonable()
    {
        // In builds, target frame rate should be -1 (unlimited) or reasonable value
        int targetFrameRate = Application.targetFrameRate;
        Assert.IsTrue(targetFrameRate == -1 || (targetFrameRate >= 30 && targetFrameRate <= 300),
            $"Target frame rate {targetFrameRate} is unreasonable!");
        Debug.Log($"✓ Target frame rate: {(targetFrameRate == -1 ? "Unlimited" : targetFrameRate.ToString())}");
    }

    [Test]
    public void Test_ScriptingDefineSymbolsValid()
    {
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
        // Add any required define symbols here
        Debug.Log($"✓ Define symbols: {(string.IsNullOrEmpty(defines) ? "None" : defines)}");
    }
}
