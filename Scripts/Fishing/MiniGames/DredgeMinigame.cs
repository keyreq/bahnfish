using UnityEngine;

/// <summary>
/// Dredge minigame for recovering sunken objects and deep-sea catches.
/// Player navigates crane through underwater obstacles to retrieve items.
/// Inspired by Dredge's salvage mechanics.
/// </summary>
public class DredgeMinigame : BaseMinigame
{
    [Header("Dredge Settings")]
    private float gameTime = 20f;
    private float craneSpeed = 0.3f;
    private Vector2 cranePosition; // 0-1 normalized

    [Header("Target")]
    private Vector2 targetPosition;
    private float targetRadius = 40f;

    [Header("Obstacles")]
    private ObstacleData[] obstacles;
    private int obstacleCount = 5;

    [Header("State")]
    private float timeElapsed = 0f;
    private bool isActive = false;
    private bool hasCollided = false;
    private float collisionPenalty = 0f;

    private struct ObstacleData
    {
        public Vector2 position;
        public float radius;
        public bool isMoving;
        public Vector2 velocity;
    }

    public override void Initialize(Fish fish, BaseFishingTool tool, TensionSystem tensionSystem)
    {
        base.Initialize(fish, tool, tensionSystem);

        // Adjust difficulty
        switch (fish.rarity)
        {
            case FishRarity.Common:
                obstacleCount = 3;
                gameTime = 15f;
                break;
            case FishRarity.Uncommon:
                obstacleCount = 4;
                gameTime = 18f;
                break;
            case FishRarity.Rare:
                obstacleCount = 5;
                gameTime = 20f;
                break;
            case FishRarity.Legendary:
                obstacleCount = 7;
                gameTime = 25f;
                break;
            case FishRarity.Aberrant:
                obstacleCount = 8;
                gameTime = 30f;
                break;
        }
    }

    public override void OnMinigameStart()
    {
        isActive = true;
        timeElapsed = 0f;
        hasCollided = false;

        // Start crane at top center
        cranePosition = new Vector2(0.5f, 0.9f);

        // Target at bottom
        targetPosition = new Vector2(Random.Range(0.2f, 0.8f), Random.Range(0.1f, 0.3f));

        // Generate obstacles
        GenerateObstacles();

        Debug.Log("Dredge Minigame Started! Navigate to the target without hitting obstacles!");
    }

    public override void UpdateMinigame(float deltaTime)
    {
        if (!isActive)
            return;

        timeElapsed += deltaTime;

        // Apply collision penalty
        if (collisionPenalty > 0f)
        {
            collisionPenalty -= deltaTime;
            if (collisionPenalty <= 0f)
            {
                collisionPenalty = 0f;
                hasCollided = false;
            }
            return; // Can't move during penalty
        }

        // Timeout
        if (timeElapsed >= gameTime)
        {
            Debug.Log("Dredge failed - took too long!");
            EventSystem.Publish("OnLineBroken");
            OnMinigameEnd();
            return;
        }

        // Handle input
        HandleMovement(deltaTime);

        // Update moving obstacles
        UpdateObstacles(deltaTime);

        // Check collisions
        CheckCollisions();

        // Check if reached target
        CheckTargetReached();
    }

    private void HandleMovement(float deltaTime)
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector2 movement = new Vector2(horizontal, vertical) * craneSpeed * deltaTime;
        cranePosition += movement;

        // Clamp to screen
        cranePosition.x = Mathf.Clamp01(cranePosition.x);
        cranePosition.y = Mathf.Clamp01(cranePosition.y);
    }

    private void GenerateObstacles()
    {
        obstacles = new ObstacleData[obstacleCount];

        for (int i = 0; i < obstacleCount; i++)
        {
            ObstacleData obstacle = new ObstacleData();

            // Random position (avoid start and target areas)
            obstacle.position = new Vector2(
                Random.Range(0.15f, 0.85f),
                Random.Range(0.35f, 0.75f)
            );

            // Random size
            obstacle.radius = Random.Range(30f, 60f);

            // Some obstacles move (harder difficulty)
            if (fish.rarity >= FishRarity.Rare && Random.value < 0.4f)
            {
                obstacle.isMoving = true;
                obstacle.velocity = Random.insideUnitCircle.normalized * 0.1f;
            }
            else
            {
                obstacle.isMoving = false;
            }

            obstacles[i] = obstacle;
        }
    }

    private void UpdateObstacles(float deltaTime)
    {
        for (int i = 0; i < obstacles.Length; i++)
        {
            if (!obstacles[i].isMoving)
                continue;

            ObstacleData obstacle = obstacles[i];
            obstacle.position += obstacle.velocity * deltaTime;

            // Bounce off edges
            if (obstacle.position.x <= 0.1f || obstacle.position.x >= 0.9f)
            {
                obstacle.velocity.x *= -1f;
                obstacle.position.x = Mathf.Clamp(obstacle.position.x, 0.1f, 0.9f);
            }

            if (obstacle.position.y <= 0.1f || obstacle.position.y >= 0.9f)
            {
                obstacle.velocity.y *= -1f;
                obstacle.position.y = Mathf.Clamp(obstacle.position.y, 0.1f, 0.9f);
            }

            obstacles[i] = obstacle;
        }
    }

    private void CheckCollisions()
    {
        if (hasCollided)
            return;

        Vector2 screenCrane = cranePosition * new Vector2(Screen.width, Screen.height);
        float craneRadius = 25f;

        foreach (ObstacleData obstacle in obstacles)
        {
            Vector2 screenObstacle = obstacle.position * new Vector2(Screen.width, Screen.height);
            float distance = Vector2.Distance(screenCrane, screenObstacle);

            if (distance < craneRadius + obstacle.radius)
            {
                // Collision!
                hasCollided = true;
                collisionPenalty = 1.5f; // Frozen for 1.5 seconds

                Debug.Log("Hit obstacle! Crane stunned.");

                // Play collision effect
                break;
            }
        }
    }

    private void CheckTargetReached()
    {
        Vector2 screenCrane = cranePosition * new Vector2(Screen.width, Screen.height);
        Vector2 screenTarget = targetPosition * new Vector2(Screen.width, Screen.height);
        float distance = Vector2.Distance(screenCrane, screenTarget);

        if (distance < targetRadius + 25f)
        {
            // Success!
            Debug.Log("Successfully dredged item!");
            EventSystem.Publish("OnFishCaught", fish);
            OnMinigameEnd();
        }
    }

    public override void OnMinigameEnd()
    {
        isActive = false;
        Destroy(this);
    }

    #region Visual Display

    private void OnGUI()
    {
        if (!isActive)
            return;

        // Draw obstacles
        GUI.color = hasCollided ? Color.red : Color.gray;
        foreach (ObstacleData obstacle in obstacles)
        {
            Vector2 screenPos = obstacle.position * new Vector2(Screen.width, Screen.height);
            Rect rect = new Rect(screenPos.x - obstacle.radius, screenPos.y - obstacle.radius, obstacle.radius * 2, obstacle.radius * 2);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);

            // Show if moving
            if (obstacle.isMoving && Debug.isDebugBuild)
            {
                GUI.color = Color.yellow;
                Rect innerRect = new Rect(screenPos.x - 10, screenPos.y - 10, 20, 20);
                GUI.DrawTexture(innerRect, Texture2D.whiteTexture);
                GUI.color = hasCollided ? Color.red : Color.gray;
            }
        }

        // Draw target
        GUI.color = Color.green;
        Vector2 screenTarget = targetPosition * new Vector2(Screen.width, Screen.height);
        Rect targetRect = new Rect(screenTarget.x - targetRadius, screenTarget.y - targetRadius, targetRadius * 2, targetRadius * 2);
        GUI.DrawTexture(targetRect, Texture2D.whiteTexture);

        // Draw crane
        GUI.color = hasCollided ? new Color(1f, 0f, 0f, 0.5f) : Color.cyan;
        Vector2 screenCrane = cranePosition * new Vector2(Screen.width, Screen.height);
        float craneSize = 50f;
        Rect craneRect = new Rect(screenCrane.x - craneSize / 2, screenCrane.y - craneSize / 2, craneSize, craneSize);
        GUI.DrawTexture(craneRect, Texture2D.whiteTexture);

        // Draw crane cable (line to top)
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(screenCrane.x - 2, 0, 4, screenCrane.y), Texture2D.whiteTexture);

        // Instructions
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.UpperCenter;

        GUI.color = Color.white;
        GUI.Label(new Rect(0, 50, Screen.width, 30), $"Time: {(gameTime - timeElapsed):F1}s", style);

        if (hasCollided)
        {
            style.normal.textColor = Color.red;
            GUI.Label(new Rect(0, 80, Screen.width, 30), $"STUNNED! ({collisionPenalty:F1}s)", style);
        }
        else
        {
            GUI.Label(new Rect(0, 80, Screen.width, 30), "WASD: Navigate crane to green target", style);
        }
    }

    #endregion
}
