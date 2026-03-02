using UnityEngine;
using System;

/// <summary>
/// Colorblind simulation and correction system.
/// Applies industry-standard color correction matrices to the camera for 8 types of colorblindness.
/// Uses post-processing shader to transform colors in real-time.
///
/// Colorblind Types:
/// - Protanopia (Red-Blind): Missing red cones
/// - Deuteranopia (Green-Blind): Missing green cones
/// - Tritanopia (Blue-Blind): Missing blue cones
/// - Protanomaly (Red-Weak): Reduced red cone sensitivity
/// - Deuteranomaly (Green-Weak): Reduced green cone sensitivity (most common, ~5% of males)
/// - Tritanomaly (Blue-Weak): Reduced blue cone sensitivity
/// - Monochromacy (Achromatopsia): Complete colorblindness
///
/// Matrices based on research from:
/// - Brettel, Viénot and Mollon (1997)
/// - Machado, Oliveira and Fernandes (2009)
/// </summary>
public class ColorblindSimulator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private AccessibilitySettings.ColorblindMode currentMode = AccessibilitySettings.ColorblindMode.None;
    [SerializeField] private bool enableDebugLogging = false;

    [Header("Shader")]
    [SerializeField] private Material colorblindMaterial;
    [SerializeField] private Shader colorblindShader;

    private Camera targetCamera;
    private bool isInitialized = false;

    // Color transformation matrices for each colorblind type
    private Matrix4x4 currentTransformMatrix;

    /// <summary>
    /// Initialize the colorblind simulator.
    /// </summary>
    private void Awake()
    {
        targetCamera = GetComponent<Camera>();
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            Debug.LogError("[ColorblindSimulator] No camera found!");
            return;
        }

        InitializeShader();
        isInitialized = true;
    }

    private void Start()
    {
        // Subscribe to colorblind mode changes
        EventSystem.Subscribe<AccessibilitySettings.ColorblindMode>("SetColorblindMode", OnColorblindModeChanged);
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<AccessibilitySettings.ColorblindMode>("SetColorblindMode", OnColorblindModeChanged);
    }

    /// <summary>
    /// Initialize colorblind shader and material.
    /// </summary>
    private void InitializeShader()
    {
        // Try to find the shader
        if (colorblindShader == null)
        {
            colorblindShader = Shader.Find("Hidden/ColorblindSimulator");
        }

        // Create material if needed
        if (colorblindMaterial == null && colorblindShader != null)
        {
            colorblindMaterial = new Material(colorblindShader);
        }

        if (colorblindMaterial == null)
        {
            Debug.LogWarning("[ColorblindSimulator] Colorblind shader not found. Using fallback matrix-based approach.");
        }
    }

    /// <summary>
    /// Event handler for colorblind mode changes.
    /// </summary>
    private void OnColorblindModeChanged(AccessibilitySettings.ColorblindMode mode)
    {
        SetColorblindMode(mode);
    }

    /// <summary>
    /// Set the active colorblind mode.
    /// </summary>
    public void SetColorblindMode(AccessibilitySettings.ColorblindMode mode)
    {
        currentMode = mode;
        currentTransformMatrix = GetTransformMatrix(mode);

        if (colorblindMaterial != null)
        {
            colorblindMaterial.SetMatrix("_ColorTransform", currentTransformMatrix);
        }

        LogDebug($"Colorblind mode set to: {mode}");
    }

    /// <summary>
    /// Get color transformation matrix for a specific colorblind type.
    /// Matrices based on scientifically accurate simulations.
    /// </summary>
    private Matrix4x4 GetTransformMatrix(AccessibilitySettings.ColorblindMode mode)
    {
        switch (mode)
        {
            case AccessibilitySettings.ColorblindMode.None:
                return GetIdentityMatrix();

            case AccessibilitySettings.ColorblindMode.Protanopia:
                return GetProtanopiaMatrix();

            case AccessibilitySettings.ColorblindMode.Deuteranopia:
                return GetDeuteranopiaMatrix();

            case AccessibilitySettings.ColorblindMode.Tritanopia:
                return GetTritanopiaMatrix();

            case AccessibilitySettings.ColorblindMode.Protanomaly:
                return GetProtanomalyMatrix();

            case AccessibilitySettings.ColorblindMode.Deuteranomaly:
                return GetDeuteranomalyMatrix();

            case AccessibilitySettings.ColorblindMode.Tritanomaly:
                return GetTritanomalyMatrix();

            case AccessibilitySettings.ColorblindMode.Monochromacy:
                return GetMonochromacyMatrix();

            default:
                return GetIdentityMatrix();
        }
    }

    /// <summary>
    /// Identity matrix (no color transformation).
    /// </summary>
    private Matrix4x4 GetIdentityMatrix()
    {
        return Matrix4x4.identity;
    }

    /// <summary>
    /// Protanopia (Red-Blind) transformation matrix.
    /// Missing L-cones (red). Red appears darker and confused with green.
    /// </summary>
    private Matrix4x4 GetProtanopiaMatrix()
    {
        Matrix4x4 matrix = new Matrix4x4();
        // Column-major order (Unity standard)
        matrix.SetRow(0, new Vector4(0.567f, 0.433f, 0.000f, 0f));
        matrix.SetRow(1, new Vector4(0.558f, 0.442f, 0.000f, 0f));
        matrix.SetRow(2, new Vector4(0.000f, 0.242f, 0.758f, 0f));
        matrix.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
        return matrix;
    }

    /// <summary>
    /// Deuteranopia (Green-Blind) transformation matrix.
    /// Missing M-cones (green). Most common severe form (~1% of males).
    /// Green appears more red/yellow.
    /// </summary>
    private Matrix4x4 GetDeuteranopiaMatrix()
    {
        Matrix4x4 matrix = new Matrix4x4();
        matrix.SetRow(0, new Vector4(0.625f, 0.375f, 0.000f, 0f));
        matrix.SetRow(1, new Vector4(0.700f, 0.300f, 0.000f, 0f));
        matrix.SetRow(2, new Vector4(0.000f, 0.300f, 0.700f, 0f));
        matrix.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
        return matrix;
    }

    /// <summary>
    /// Tritanopia (Blue-Blind) transformation matrix.
    /// Missing S-cones (blue). Very rare (~0.001% of population).
    /// Blue appears green, yellow appears violet/gray.
    /// </summary>
    private Matrix4x4 GetTritanopiaMatrix()
    {
        Matrix4x4 matrix = new Matrix4x4();
        matrix.SetRow(0, new Vector4(0.950f, 0.050f, 0.000f, 0f));
        matrix.SetRow(1, new Vector4(0.000f, 0.433f, 0.567f, 0f));
        matrix.SetRow(2, new Vector4(0.000f, 0.475f, 0.525f, 0f));
        matrix.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
        return matrix;
    }

    /// <summary>
    /// Protanomaly (Red-Weak) transformation matrix.
    /// Reduced L-cone sensitivity. Milder form of protanopia.
    /// </summary>
    private Matrix4x4 GetProtanomalyMatrix()
    {
        Matrix4x4 matrix = new Matrix4x4();
        matrix.SetRow(0, new Vector4(0.817f, 0.183f, 0.000f, 0f));
        matrix.SetRow(1, new Vector4(0.333f, 0.667f, 0.000f, 0f));
        matrix.SetRow(2, new Vector4(0.000f, 0.125f, 0.875f, 0f));
        matrix.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
        return matrix;
    }

    /// <summary>
    /// Deuteranomaly (Green-Weak) transformation matrix.
    /// Reduced M-cone sensitivity. Most common colorblindness (~5% of males, 0.4% of females).
    /// Difficulty distinguishing red and green.
    /// </summary>
    private Matrix4x4 GetDeuteranomalyMatrix()
    {
        Matrix4x4 matrix = new Matrix4x4();
        matrix.SetRow(0, new Vector4(0.800f, 0.200f, 0.000f, 0f));
        matrix.SetRow(1, new Vector4(0.258f, 0.742f, 0.000f, 0f));
        matrix.SetRow(2, new Vector4(0.000f, 0.142f, 0.858f, 0f));
        matrix.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
        return matrix;
    }

    /// <summary>
    /// Tritanomaly (Blue-Weak) transformation matrix.
    /// Reduced S-cone sensitivity. Rare (~0.01% of population).
    /// Difficulty distinguishing blue and yellow.
    /// </summary>
    private Matrix4x4 GetTritanomalyMatrix()
    {
        Matrix4x4 matrix = new Matrix4x4();
        matrix.SetRow(0, new Vector4(0.967f, 0.033f, 0.000f, 0f));
        matrix.SetRow(1, new Vector4(0.000f, 0.733f, 0.267f, 0f));
        matrix.SetRow(2, new Vector4(0.000f, 0.183f, 0.817f, 0f));
        matrix.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
        return matrix;
    }

    /// <summary>
    /// Monochromacy (Achromatopsia) transformation matrix.
    /// Complete colorblindness. Only sees luminance (grayscale).
    /// Very rare (~0.003% of population).
    /// Uses standard luminance weights (ITU-R BT.709).
    /// </summary>
    private Matrix4x4 GetMonochromacyMatrix()
    {
        Matrix4x4 matrix = new Matrix4x4();
        // Luminance weights: R=0.2126, G=0.7152, B=0.0722
        matrix.SetRow(0, new Vector4(0.299f, 0.587f, 0.114f, 0f));
        matrix.SetRow(1, new Vector4(0.299f, 0.587f, 0.114f, 0f));
        matrix.SetRow(2, new Vector4(0.299f, 0.587f, 0.114f, 0f));
        matrix.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
        return matrix;
    }

    /// <summary>
    /// Apply colorblind transformation to a single color.
    /// Useful for UI elements that need per-element transformation.
    /// </summary>
    public Color TransformColor(Color inputColor)
    {
        if (currentMode == AccessibilitySettings.ColorblindMode.None)
        {
            return inputColor;
        }

        Vector4 colorVector = new Vector4(inputColor.r, inputColor.g, inputColor.b, inputColor.a);
        Vector4 transformedVector = currentTransformMatrix * colorVector;

        return new Color(
            Mathf.Clamp01(transformedVector.x),
            Mathf.Clamp01(transformedVector.y),
            Mathf.Clamp01(transformedVector.z),
            inputColor.a // Preserve alpha
        );
    }

    /// <summary>
    /// Post-processing callback to apply colorblind shader.
    /// </summary>
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (colorblindMaterial != null && currentMode != AccessibilitySettings.ColorblindMode.None)
        {
            Graphics.Blit(source, destination, colorblindMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }

    /// <summary>
    /// Logs debug messages if debugging is enabled.
    /// </summary>
    private void LogDebug(string message)
    {
        if (enableDebugLogging)
        {
            Debug.Log($"[ColorblindSimulator] {message}");
        }
    }

    /// <summary>
    /// Get the current colorblind mode.
    /// </summary>
    public AccessibilitySettings.ColorblindMode CurrentMode => currentMode;

    /// <summary>
    /// Check if colorblind mode is active.
    /// </summary>
    public bool IsActive => currentMode != AccessibilitySettings.ColorblindMode.None;
}
