using UnityEngine;

/// <summary>
/// Handles water physics for the boat including buoyancy, water resistance, and momentum.
/// Simulates realistic boat behavior on water with configurable physics parameters.
/// Can be attached to any object that should float and interact with water.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class WaterPhysics : MonoBehaviour
{
    [Header("Water Settings")]
    [SerializeField] private float waterLevel = 0f;
    [SerializeField] private float waterDensity = 1000f;

    [Header("Buoyancy Settings")]
    [SerializeField] private float buoyancyForce = 15f;
    [SerializeField] private float buoyancyDamping = 3f;
    [SerializeField] private Transform[] floatPoints;
    [SerializeField] private bool autoCreateFloatPoints = true;
    [SerializeField] private int autoFloatPointCount = 4;

    [Header("Water Resistance")]
    [SerializeField] private float waterDrag = 2f;
    [SerializeField] private float waterAngularDrag = 1f;
    [SerializeField] private bool applyWaterResistance = true;

    [Header("Wave Settings")]
    [SerializeField] private bool enableWaves = false;
    [SerializeField] private float waveHeight = 0.5f;
    [SerializeField] private float waveFrequency = 0.5f;
    [SerializeField] private float waveSpeed = 1f;

    [Header("Splash Effects")]
    [SerializeField] private bool enableSplashEffects = true;
    [SerializeField] private ParticleSystem splashParticles;
    [SerializeField] private float splashThreshold = 2f;

    [Header("Debug")]
    [SerializeField] private bool drawDebugGizmos = true;

    private Rigidbody rb;
    private float originalDrag;
    private float originalAngularDrag;
    private bool isInWater = false;
    private float timeOffset;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        timeOffset = Random.Range(0f, 100f); // Random offset for waves

        // Store original drag values
        originalDrag = rb.drag;
        originalAngularDrag = rb.angularDrag;

        // Auto-create float points if needed
        if (autoCreateFloatPoints && (floatPoints == null || floatPoints.Length == 0))
        {
            CreateFloatPoints();
        }
    }

    private void FixedUpdate()
    {
        ApplyBuoyancy();

        if (applyWaterResistance)
        {
            ApplyWaterResistance();
        }

        CheckSplash();
    }

    private void CreateFloatPoints()
    {
        // Create float points at corners of the object
        GameObject floatPointParent = new GameObject("FloatPoints");
        floatPointParent.transform.SetParent(transform);
        floatPointParent.transform.localPosition = Vector3.zero;

        Bounds bounds = GetComponentInChildren<Collider>()?.bounds ?? new Bounds(transform.position, Vector3.one * 2f);
        Vector3 size = bounds.size;

        floatPoints = new Transform[autoFloatPointCount];

        switch (autoFloatPointCount)
        {
            case 4:
                // Four corners
                floatPoints[0] = CreateFloatPoint(floatPointParent.transform, new Vector3(-size.x / 2, 0, -size.z / 2));
                floatPoints[1] = CreateFloatPoint(floatPointParent.transform, new Vector3(size.x / 2, 0, -size.z / 2));
                floatPoints[2] = CreateFloatPoint(floatPointParent.transform, new Vector3(-size.x / 2, 0, size.z / 2));
                floatPoints[3] = CreateFloatPoint(floatPointParent.transform, new Vector3(size.x / 2, 0, size.z / 2));
                break;

            case 8:
                // Eight points (corners + midpoints)
                floatPoints[0] = CreateFloatPoint(floatPointParent.transform, new Vector3(-size.x / 2, 0, -size.z / 2));
                floatPoints[1] = CreateFloatPoint(floatPointParent.transform, new Vector3(size.x / 2, 0, -size.z / 2));
                floatPoints[2] = CreateFloatPoint(floatPointParent.transform, new Vector3(-size.x / 2, 0, size.z / 2));
                floatPoints[3] = CreateFloatPoint(floatPointParent.transform, new Vector3(size.x / 2, 0, size.z / 2));
                floatPoints[4] = CreateFloatPoint(floatPointParent.transform, new Vector3(0, 0, -size.z / 2));
                floatPoints[5] = CreateFloatPoint(floatPointParent.transform, new Vector3(0, 0, size.z / 2));
                floatPoints[6] = CreateFloatPoint(floatPointParent.transform, new Vector3(-size.x / 2, 0, 0));
                floatPoints[7] = CreateFloatPoint(floatPointParent.transform, new Vector3(size.x / 2, 0, 0));
                break;

            default:
                floatPoints = new Transform[1];
                floatPoints[0] = CreateFloatPoint(floatPointParent.transform, Vector3.zero);
                break;
        }

        Debug.Log($"WaterPhysics: Created {floatPoints.Length} float points");
    }

    private Transform CreateFloatPoint(Transform parent, Vector3 localPosition)
    {
        GameObject fp = new GameObject("FloatPoint");
        fp.transform.SetParent(parent);
        fp.transform.localPosition = localPosition;
        return fp.transform;
    }

    private void ApplyBuoyancy()
    {
        if (floatPoints == null || floatPoints.Length == 0) return;

        isInWater = false;
        int submergedPoints = 0;

        foreach (Transform floatPoint in floatPoints)
        {
            if (floatPoint == null) continue;

            float pointHeight = floatPoint.position.y;
            float currentWaterLevel = GetWaterLevelAtPosition(floatPoint.position);

            // Check if this point is underwater
            if (pointHeight < currentWaterLevel)
            {
                isInWater = true;
                submergedPoints++;

                // Calculate submersion depth
                float submersionDepth = currentWaterLevel - pointHeight;

                // Calculate buoyancy force
                float pointBuoyancy = buoyancyForce * submersionDepth;

                // Apply upward force at this point
                Vector3 buoyancyVector = Vector3.up * pointBuoyancy;
                rb.AddForceAtPosition(buoyancyVector, floatPoint.position, ForceMode.Force);

                // Apply damping to reduce oscillation
                Vector3 pointVelocity = rb.GetPointVelocity(floatPoint.position);
                Vector3 dampingForce = -pointVelocity * buoyancyDamping * submersionDepth;
                rb.AddForceAtPosition(dampingForce, floatPoint.position, ForceMode.Force);
            }
        }

        // Adjust drag based on submersion
        if (isInWater)
        {
            float submersionRatio = (float)submergedPoints / floatPoints.Length;
            rb.drag = Mathf.Lerp(originalDrag, waterDrag, submersionRatio);
            rb.angularDrag = Mathf.Lerp(originalAngularDrag, waterAngularDrag, submersionRatio);
        }
        else
        {
            rb.drag = originalDrag;
            rb.angularDrag = originalAngularDrag;
        }
    }

    private void ApplyWaterResistance()
    {
        if (!isInWater) return;

        // Apply resistance opposite to velocity
        Vector3 resistanceForce = -rb.velocity * waterDrag * 0.5f;
        rb.AddForce(resistanceForce, ForceMode.Force);

        // Apply angular resistance
        Vector3 angularResistance = -rb.angularVelocity * waterAngularDrag * 0.5f;
        rb.AddTorque(angularResistance, ForceMode.Force);
    }

    private float GetWaterLevelAtPosition(Vector3 position)
    {
        if (!enableWaves)
        {
            return waterLevel;
        }

        // Simple sine wave
        float wave = Mathf.Sin((position.x + Time.time * waveSpeed + timeOffset) * waveFrequency) +
                    Mathf.Sin((position.z + Time.time * waveSpeed * 0.7f + timeOffset) * waveFrequency * 1.3f);

        return waterLevel + (wave * waveHeight * 0.5f);
    }

    private void CheckSplash()
    {
        if (!enableSplashEffects || splashParticles == null) return;

        // Check if boat is impacting water surface
        if (isInWater && rb.velocity.magnitude > splashThreshold)
        {
            float velocityY = Mathf.Abs(rb.velocity.y);
            if (velocityY > splashThreshold * 0.5f)
            {
                // Play splash effect
                if (!splashParticles.isPlaying)
                {
                    splashParticles.Play();
                }
            }
        }
    }

    #region Public API

    /// <summary>
    /// Check if object is currently in water
    /// </summary>
    public bool IsInWater()
    {
        return isInWater;
    }

    /// <summary>
    /// Set water level
    /// </summary>
    public void SetWaterLevel(float level)
    {
        waterLevel = level;
    }

    /// <summary>
    /// Get current water level at a position
    /// </summary>
    public float GetWaterLevel(Vector3 position)
    {
        return GetWaterLevelAtPosition(position);
    }

    /// <summary>
    /// Set buoyancy force multiplier
    /// </summary>
    public void SetBuoyancyForce(float force)
    {
        buoyancyForce = force;
    }

    /// <summary>
    /// Enable or disable wave simulation
    /// </summary>
    public void SetWavesEnabled(bool enabled)
    {
        enableWaves = enabled;
    }

    /// <summary>
    /// Set wave properties
    /// </summary>
    public void SetWaveProperties(float height, float frequency, float speed)
    {
        waveHeight = height;
        waveFrequency = frequency;
        waveSpeed = speed;
    }

    #endregion

    private void OnDrawGizmos()
    {
        if (!drawDebugGizmos) return;

        // Draw water level plane
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
        Gizmos.DrawCube(new Vector3(transform.position.x, waterLevel, transform.position.z), new Vector3(20f, 0.1f, 20f));

        // Draw float points
        if (floatPoints != null)
        {
            foreach (Transform floatPoint in floatPoints)
            {
                if (floatPoint == null) continue;

                float currentWaterLevel = GetWaterLevelAtPosition(floatPoint.position);
                bool isUnderwater = floatPoint.position.y < currentWaterLevel;

                Gizmos.color = isUnderwater ? Color.blue : Color.red;
                Gizmos.DrawWireSphere(floatPoint.position, 0.2f);

                // Draw line to water surface
                if (isUnderwater)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(floatPoint.position, new Vector3(floatPoint.position.x, currentWaterLevel, floatPoint.position.z));
                }
            }
        }

        // Draw center of mass
        if (rb != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(rb.worldCenterOfMass, 0.3f);
        }
    }
}
