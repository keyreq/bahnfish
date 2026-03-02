using UnityEngine;

/// <summary>
/// Agent 8: Fish AI & Behavior Agent - NormalBehavior.cs
/// Standard swimming behavior for common fish species.
/// Smooth movement with depth preference and basic schooling.
/// </summary>
public class NormalBehavior : MonoBehaviour, IFishBehavior
{
    [Header("Movement Settings")]
    [SerializeField] private float swimSpeed = 2f;
    [SerializeField] private float turnSpeed = 2f;
    [SerializeField] private float depthOscillation = 0.5f;

    [Header("Schooling (Optional)")]
    [SerializeField] private bool enableSchooling = false;
    [SerializeField] private float schoolingRadius = 5f;
    [SerializeField] private float separationDistance = 2f;
    [SerializeField] private float alignmentWeight = 0.3f;
    [SerializeField] private float cohesionWeight = 0.3f;
    [SerializeField] private float separationWeight = 0.4f;

    private FishSpeciesData speciesData;
    private float depthTimer;
    private float preferredDepth;

    public void Initialize(FishSpeciesData data)
    {
        speciesData = data;
        swimSpeed *= data.speedMultiplier;
        preferredDepth = Random.Range(data.minDepth, data.maxDepth);
        depthTimer = Random.Range(0f, 10f);
    }

    public void UpdateMovement(FishAI fish, Rigidbody rb)
    {
        if (fish == null || rb == null) return;

        Vector3 movement = Vector3.zero;

        switch (fish.GetState())
        {
            case FishState.Idle:
                movement = IdleMovement(fish, rb);
                break;

            case FishState.Wandering:
                movement = WanderMovement(fish, rb);
                break;

            case FishState.Approaching:
                movement = ApproachMovement(fish, rb);
                break;

            case FishState.Fleeing:
                movement = FleeMovement(fish, rb);
                break;

            case FishState.Investigating:
                movement = InvestigateMovement(fish, rb);
                break;

            case FishState.Hooked:
                movement = HookedMovement(fish, rb);
                break;
        }

        // Apply schooling if enabled
        if (enableSchooling)
        {
            movement += CalculateSchooling(fish, rb);
        }

        // Apply movement
        rb.velocity = Vector3.Lerp(rb.velocity, movement, Time.fixedDeltaTime * 2f);

        // Rotate towards movement direction
        if (rb.velocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(rb.velocity.normalized);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
        }

        // Maintain depth with gentle oscillation
        MaintainDepthWithOscillation(fish, rb);
    }

    private Vector3 IdleMovement(FishAI fish, Rigidbody rb)
    {
        // Gentle drifting
        return rb.velocity * 0.95f;
    }

    private Vector3 WanderMovement(FishAI fish, Rigidbody rb)
    {
        Vector3 targetPosition = fish.GetWanderTarget();
        Vector3 direction = (targetPosition - fish.transform.position).normalized;
        return direction * swimSpeed;
    }

    private Vector3 ApproachMovement(FishAI fish, Rigidbody rb)
    {
        // Find nearest bait (mock implementation)
        GameObject bait = GameObject.FindGameObjectWithTag("Bait");
        if (bait != null)
        {
            Vector3 direction = (bait.transform.position - fish.transform.position).normalized;
            return direction * swimSpeed * 1.2f; // Faster when approaching bait
        }
        return WanderMovement(fish, rb);
    }

    private Vector3 FleeMovement(FishAI fish, Rigidbody rb)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 direction = (fish.transform.position - player.transform.position).normalized;
            return direction * swimSpeed * 1.5f; // Faster when fleeing
        }
        return WanderMovement(fish, rb);
    }

    private Vector3 InvestigateMovement(FishAI fish, Rigidbody rb)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Circle around player
            Vector3 toPlayer = player.transform.position - fish.transform.position;
            Vector3 perpendicular = Vector3.Cross(toPlayer, Vector3.up).normalized;
            return perpendicular * swimSpeed;
        }
        return WanderMovement(fish, rb);
    }

    private Vector3 HookedMovement(FishAI fish, Rigidbody rb)
    {
        // Struggle in random directions
        float stamina = fish.GetStamina();
        float struggleIntensity = stamina / 100f;

        Vector3 struggle = new Vector3(
            Mathf.Sin(Time.time * 5f) * struggleIntensity,
            Mathf.Cos(Time.time * 3f) * struggleIntensity,
            Mathf.Sin(Time.time * 4f) * struggleIntensity
        );

        return struggle * swimSpeed * 2f;
    }

    private Vector3 CalculateSchooling(FishAI fish, Rigidbody rb)
    {
        // Find nearby fish of same species
        Collider[] nearbyFish = Physics.OverlapSphere(fish.transform.position, schoolingRadius);

        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
        Vector3 separation = Vector3.zero;
        int neighborCount = 0;

        foreach (Collider col in nearbyFish)
        {
            FishAI otherFish = col.GetComponent<FishAI>();
            if (otherFish == null || otherFish == fish) continue;

            // Check if same species
            if (otherFish.GetSpeciesData()?.fishID == speciesData?.fishID)
            {
                neighborCount++;

                // Alignment: steer towards average heading
                alignment += otherFish.GetComponent<Rigidbody>().velocity;

                // Cohesion: steer towards average position
                cohesion += otherFish.transform.position;

                // Separation: steer away from close neighbors
                float distance = Vector3.Distance(fish.transform.position, otherFish.transform.position);
                if (distance < separationDistance)
                {
                    Vector3 away = fish.transform.position - otherFish.transform.position;
                    separation += away.normalized / distance;
                }
            }
        }

        if (neighborCount == 0)
            return Vector3.zero;

        // Average the values
        alignment /= neighborCount;
        cohesion = (cohesion / neighborCount) - fish.transform.position;

        // Apply weights
        Vector3 schoolingForce =
            alignment.normalized * alignmentWeight +
            cohesion.normalized * cohesionWeight +
            separation.normalized * separationWeight;

        return schoolingForce * swimSpeed * 0.5f;
    }

    private void MaintainDepthWithOscillation(FishAI fish, Rigidbody rb)
    {
        // Gentle oscillation around preferred depth
        depthTimer += Time.fixedDeltaTime;
        float oscillation = Mathf.Sin(depthTimer) * depthOscillation;

        float targetY = -(preferredDepth + oscillation);
        float currentY = fish.transform.position.y;

        if (Mathf.Abs(currentY - targetY) > 0.2f)
        {
            float verticalForce = (targetY - currentY) * 3f;
            rb.AddForce(Vector3.up * verticalForce, ForceMode.Acceleration);
        }
    }

    private void OnDrawGizmos()
    {
        if (!enableSchooling) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, schoolingRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, separationDistance);
    }
}
