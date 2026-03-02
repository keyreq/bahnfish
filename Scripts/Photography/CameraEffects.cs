using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Agent 18: Photography Mode Specialist - CameraEffects.cs
/// Manages photo filters and post-processing effects for the photography system.
/// Provides 20+ Instagram-quality filters with real-time preview and intensity controls.
/// </summary>
public class CameraEffects : MonoBehaviour
{
    public static CameraEffects Instance { get; private set; }

    [Header("Active Filters")]
    [SerializeField] private List<PhotoFilter> activeFilters = new List<PhotoFilter>();
    [SerializeField] private Dictionary<FilterType, float> filterIntensities = new Dictionary<FilterType, float>();

    [Header("Material References")]
    [SerializeField] private Material sepiamat;
    [SerializeField] private Material blackAndWhiteMat;
    [SerializeField] private Material vintageMat;
    [SerializeField] private Material hdrMat;
    [SerializeField] private Material bloomMat;

    private Camera photoCamera;
    private Material currentEffectMaterial;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Initialize filter intensities
        foreach (FilterType filterType in System.Enum.GetValues(typeof(FilterType)))
        {
            filterIntensities[filterType] = 0f;
        }
    }

    private void Start()
    {
        photoCamera = Camera.main;
    }

    /// <summary>
    /// Applies a filter with the specified intensity.
    /// </summary>
    public void ApplyFilter(FilterType filterType, float intensity)
    {
        intensity = Mathf.Clamp01(intensity);
        filterIntensities[filterType] = intensity;

        // Update active filters list
        PhotoFilter existingFilter = activeFilters.Find(f => f.type == filterType);
        if (existingFilter != null)
        {
            existingFilter.intensity = intensity;
            if (intensity <= 0f)
            {
                activeFilters.Remove(existingFilter);
            }
        }
        else if (intensity > 0f)
        {
            activeFilters.Add(new PhotoFilter(filterType, intensity));
        }
    }

    /// <summary>
    /// Removes a filter.
    /// </summary>
    public void RemoveFilter(FilterType filterType)
    {
        filterIntensities[filterType] = 0f;
        activeFilters.RemoveAll(f => f.type == filterType);
    }

    /// <summary>
    /// Clears all active filters.
    /// </summary>
    public void ClearAllFilters()
    {
        activeFilters.Clear();
        foreach (FilterType filterType in System.Enum.GetValues(typeof(FilterType)))
        {
            filterIntensities[filterType] = 0f;
        }
    }

    /// <summary>
    /// Gets the intensity of a specific filter.
    /// </summary>
    public float GetFilterIntensity(FilterType filterType)
    {
        return filterIntensities.ContainsKey(filterType) ? filterIntensities[filterType] : 0f;
    }

    /// <summary>
    /// Gets all active filters.
    /// </summary>
    public List<PhotoFilter> GetActiveFilters()
    {
        return new List<PhotoFilter>(activeFilters);
    }

    /// <summary>
    /// Applies filter effects to a texture.
    /// This is called during photo capture.
    /// </summary>
    public Texture2D ApplyFiltersToTexture(Texture2D sourceTexture)
    {
        if (activeFilters.Count == 0)
        {
            return sourceTexture;
        }

        Texture2D resultTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGB24, false);
        Color[] pixels = sourceTexture.GetPixels();

        // Apply each active filter
        foreach (PhotoFilter filter in activeFilters)
        {
            pixels = ApplyFilterToPixels(pixels, filter);
        }

        resultTexture.SetPixels(pixels);
        resultTexture.Apply();
        return resultTexture;
    }

    /// <summary>
    /// Applies a single filter to pixel data.
    /// </summary>
    private Color[] ApplyFilterToPixels(Color[] pixels, PhotoFilter filter)
    {
        Color[] result = new Color[pixels.Length];

        for (int i = 0; i < pixels.Length; i++)
        {
            result[i] = ApplyFilterToColor(pixels[i], filter);
        }

        return result;
    }

    /// <summary>
    /// Applies filter effect to a single color.
    /// </summary>
    private Color ApplyFilterToColor(Color original, PhotoFilter filter)
    {
        Color filtered = original;
        float intensity = filter.intensity;

        switch (filter.type)
        {
            case FilterType.Sepia:
                filtered = ApplySepia(original, intensity);
                break;
            case FilterType.BlackAndWhite:
                filtered = ApplyBlackAndWhite(original, intensity);
                break;
            case FilterType.Vintage:
                filtered = ApplyVintage(original, intensity);
                break;
            case FilterType.FilmNoir:
                filtered = ApplyFilmNoir(original, intensity);
                break;
            case FilterType.Polaroid:
                filtered = ApplyPolaroid(original, intensity);
                break;
            case FilterType.OilPaint:
                filtered = ApplyOilPaint(original, intensity);
                break;
            case FilterType.Watercolor:
                filtered = ApplyWatercolor(original, intensity);
                break;
            case FilterType.Sketch:
                filtered = ApplySketch(original, intensity);
                break;
            case FilterType.CelShading:
                filtered = ApplyCelShading(original, intensity);
                break;
            case FilterType.Impressionist:
                filtered = ApplyImpressionist(original, intensity);
                break;
            case FilterType.HDR:
                filtered = ApplyHDR(original, intensity);
                break;
            case FilterType.Bloom:
                filtered = ApplyBloom(original, intensity);
                break;
            case FilterType.Vignette:
                // Vignette requires position info, handled separately
                filtered = original;
                break;
            case FilterType.Sharpness:
                filtered = ApplySharpness(original, intensity);
                break;
            case FilterType.ColorPop:
                filtered = ApplyColorPop(original, intensity);
                break;
            case FilterType.Fisheye:
                // Fisheye requires UV distortion, handled separately
                filtered = original;
                break;
            case FilterType.TiltShift:
                // Tilt-shift requires position info, handled separately
                filtered = original;
                break;
            case FilterType.ChromaticAberration:
                // Chromatic aberration requires offset sampling, handled separately
                filtered = original;
                break;
            case FilterType.Glitch:
                filtered = ApplyGlitch(original, intensity);
                break;
            case FilterType.Retrowave:
                filtered = ApplyRetrowave(original, intensity);
                break;
        }

        return filtered;
    }

    #region Filter Implementations

    private Color ApplySepia(Color color, float intensity)
    {
        float gray = color.r * 0.393f + color.g * 0.769f + color.b * 0.189f;
        Color sepia = new Color(
            Mathf.Clamp01(gray + 0.2f),
            Mathf.Clamp01(gray + 0.1f),
            Mathf.Clamp01(gray - 0.1f),
            color.a
        );
        return Color.Lerp(color, sepia, intensity);
    }

    private Color ApplyBlackAndWhite(Color color, float intensity)
    {
        float gray = color.grayscale;
        Color bw = new Color(gray, gray, gray, color.a);
        return Color.Lerp(color, bw, intensity);
    }

    private Color ApplyVintage(Color color, float intensity)
    {
        // Faded colors with slight yellow tint
        Color faded = new Color(
            color.r * 0.9f + 0.1f,
            color.g * 0.85f + 0.05f,
            color.b * 0.7f,
            color.a
        );
        return Color.Lerp(color, faded, intensity);
    }

    private Color ApplyFilmNoir(Color color, float intensity)
    {
        float gray = color.grayscale;
        // High contrast black and white
        gray = (gray - 0.5f) * 1.5f + 0.5f;
        gray = Mathf.Clamp01(gray);
        Color noir = new Color(gray, gray, gray, color.a);
        return Color.Lerp(color, noir, intensity);
    }

    private Color ApplyPolaroid(Color color, float intensity)
    {
        // Slightly washed out with cool tint
        Color polaroid = new Color(
            color.r * 0.95f + 0.05f,
            color.g * 0.95f + 0.05f,
            color.b * 0.95f + 0.1f,
            color.a
        );
        return Color.Lerp(color, polaroid, intensity);
    }

    private Color ApplyOilPaint(Color color, float intensity)
    {
        // Saturated and slightly smoothed
        float h, s, v;
        Color.RGBToHSV(color, out h, out s, out v);
        s = Mathf.Clamp01(s * 1.3f);
        Color oilPaint = Color.HSVToRGB(h, s, v);
        oilPaint.a = color.a;
        return Color.Lerp(color, oilPaint, intensity);
    }

    private Color ApplyWatercolor(Color color, float intensity)
    {
        // Soft, washed colors
        float h, s, v;
        Color.RGBToHSV(color, out h, out s, out v);
        s *= 0.7f;
        v = Mathf.Lerp(v, 0.9f, 0.3f);
        Color watercolor = Color.HSVToRGB(h, s, v);
        watercolor.a = color.a;
        return Color.Lerp(color, watercolor, intensity);
    }

    private Color ApplySketch(Color color, float intensity)
    {
        // Edge detection approximation (simplified)
        float gray = 1f - color.grayscale;
        Color sketch = new Color(gray, gray, gray, color.a);
        return Color.Lerp(color, sketch, intensity);
    }

    private Color ApplyCelShading(Color color, float intensity)
    {
        // Posterize effect
        int levels = 4;
        float r = Mathf.Floor(color.r * levels) / levels;
        float g = Mathf.Floor(color.g * levels) / levels;
        float b = Mathf.Floor(color.b * levels) / levels;
        Color cel = new Color(r, g, b, color.a);
        return Color.Lerp(color, cel, intensity);
    }

    private Color ApplyImpressionist(Color color, float intensity)
    {
        // Slightly blurred with vibrant colors
        float h, s, v;
        Color.RGBToHSV(color, out h, out s, out v);
        s = Mathf.Clamp01(s * 1.2f);
        Color impressionist = Color.HSVToRGB(h, s, v);
        impressionist.a = color.a;
        return Color.Lerp(color, impressionist, intensity);
    }

    private Color ApplyHDR(Color color, float intensity)
    {
        // Tone mapping for high dynamic range
        float exposure = 1.5f;
        Color hdr = new Color(
            1f - Mathf.Exp(-color.r * exposure),
            1f - Mathf.Exp(-color.g * exposure),
            1f - Mathf.Exp(-color.b * exposure),
            color.a
        );
        return Color.Lerp(color, hdr, intensity);
    }

    private Color ApplyBloom(Color color, float intensity)
    {
        // Brighten bright areas
        float luminance = color.grayscale;
        if (luminance > 0.5f)
        {
            float bloomAmount = (luminance - 0.5f) * 2f * intensity;
            color.r = Mathf.Clamp01(color.r + bloomAmount);
            color.g = Mathf.Clamp01(color.g + bloomAmount);
            color.b = Mathf.Clamp01(color.b + bloomAmount);
        }
        return color;
    }

    private Color ApplySharpness(Color color, float intensity)
    {
        // Increase contrast slightly
        float factor = 1f + intensity * 0.5f;
        Color sharp = new Color(
            Mathf.Clamp01((color.r - 0.5f) * factor + 0.5f),
            Mathf.Clamp01((color.g - 0.5f) * factor + 0.5f),
            Mathf.Clamp01((color.b - 0.5f) * factor + 0.5f),
            color.a
        );
        return sharp;
    }

    private Color ApplyColorPop(Color color, float intensity)
    {
        // Selective color (keep saturated colors, desaturate others)
        float h, s, v;
        Color.RGBToHSV(color, out h, out s, out v);

        if (s < 0.3f)
        {
            // Desaturate low-saturation colors
            s *= (1f - intensity);
            Color desaturated = Color.HSVToRGB(h, s, v);
            desaturated.a = color.a;
            return desaturated;
        }
        else
        {
            // Boost high-saturation colors
            s = Mathf.Clamp01(s * (1f + intensity * 0.5f));
            Color saturated = Color.HSVToRGB(h, s, v);
            saturated.a = color.a;
            return saturated;
        }
    }

    private Color ApplyGlitch(Color color, float intensity)
    {
        // Random RGB channel shifts
        if (Random.value < intensity * 0.1f)
        {
            int channel = Random.Range(0, 3);
            if (channel == 0) color.r = Random.value;
            else if (channel == 1) color.g = Random.value;
            else color.b = Random.value;
        }
        return color;
    }

    private Color ApplyRetrowave(Color color, float intensity)
    {
        // Purple/cyan 80s aesthetic
        float h, s, v;
        Color.RGBToHSV(color, out h, out s, out v);

        // Shift hue toward purple/cyan
        h = Mathf.Lerp(h, h < 0.5f ? 0.8f : 0.5f, intensity * 0.5f);
        s = Mathf.Clamp01(s * 1.5f);

        Color retrowave = Color.HSVToRGB(h, s, v);
        retrowave.a = color.a;
        return retrowave;
    }

    #endregion

    /// <summary>
    /// Gets the display name for a filter type.
    /// </summary>
    public static string GetFilterName(FilterType filterType)
    {
        switch (filterType)
        {
            case FilterType.Sepia: return "Sepia Tone";
            case FilterType.BlackAndWhite: return "Black & White";
            case FilterType.Vintage: return "Vintage";
            case FilterType.FilmNoir: return "Film Noir";
            case FilterType.Polaroid: return "Polaroid";
            case FilterType.OilPaint: return "Oil Paint";
            case FilterType.Watercolor: return "Watercolor";
            case FilterType.Sketch: return "Sketch";
            case FilterType.CelShading: return "Cel Shading";
            case FilterType.Impressionist: return "Impressionist";
            case FilterType.HDR: return "HDR";
            case FilterType.Bloom: return "Bloom";
            case FilterType.Vignette: return "Vignette";
            case FilterType.Sharpness: return "Sharpness";
            case FilterType.ColorPop: return "Color Pop";
            case FilterType.Fisheye: return "Fisheye";
            case FilterType.TiltShift: return "Tilt-Shift";
            case FilterType.ChromaticAberration: return "Chromatic Aberration";
            case FilterType.Glitch: return "Glitch";
            case FilterType.Retrowave: return "Retrowave";
            default: return filterType.ToString();
        }
    }
}

/// <summary>
/// Represents a photo filter with intensity.
/// </summary>
[System.Serializable]
public class PhotoFilter
{
    public FilterType type;
    public float intensity;

    public PhotoFilter(FilterType type, float intensity)
    {
        this.type = type;
        this.intensity = Mathf.Clamp01(intensity);
    }
}

/// <summary>
/// Available photo filter types.
/// </summary>
[System.Serializable]
public enum FilterType
{
    // Classic Filters
    Sepia,
    BlackAndWhite,
    Vintage,
    FilmNoir,
    Polaroid,

    // Artistic Filters
    OilPaint,
    Watercolor,
    Sketch,
    CelShading,
    Impressionist,

    // Enhancement Filters
    HDR,
    Bloom,
    Vignette,
    Sharpness,
    ColorPop,

    // Creative Filters
    Fisheye,
    TiltShift,
    ChromaticAberration,
    Glitch,
    Retrowave
}
