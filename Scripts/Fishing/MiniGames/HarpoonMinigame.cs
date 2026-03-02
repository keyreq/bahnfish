using UnityEngine;

/// <summary>
/// Harpoon minigame for catching large or dangerous fish.
/// Player aims at moving target and times the throw for instant catch.
/// Success = instant catch, Miss = fish scared away.
/// </summary>
public class HarpoonMinigame : BaseMinigame
{
    [Header("Harpoon Settings")]
    private float gameTime = 10f; // Time to aim and throw
    private float chargeTime = 0f;
    private float maxChargeTime = 2f;
    private bool isCharging = false;
    private bool hasThrown = false;

    [Header("Target Movement")]
    private Vector2 targetPosition; // Screen position 0-1
    private Vector2 targetVelocity;
    private float targetSpeed = 0.3f;
    private float targetSize = 50f; // Pixels

    [Header("Cursor")]
    private Vector2 cursorPosition; // Screen position 0-1
    private float cursorSpeed = 0.5f;

    [Header("Hit Detection")]
    private float hitRadius = 60f; // Larger = easier

    private float timeElapsed = 0f;
    private bool isActive = false;

    public override void Initialize(Fish fish, BaseFishingTool tool, TensionSystem tensionSystem)
    {
        base.Initialize(fish, tool, tensionSystem);

        // Adjust difficulty based on fish
        switch (fish.rarity)
        {
            case FishRarity.Common:
                targetSpeed = 0.2f;
                hitRadius = 80f;
                break;
            case FishRarity.Uncommon:
                targetSpeed = 0.3f;
                hitRadius = 70f;
                break;
            case FishRarity.Rare:
                targetSpeed = 0.4f;
                hitRadius = 60f;
                break;
            case FishRarity.Legendary:
                targetSpeed = 0.5f;
                hitRadius = 50f;
                break;
            case FishRarity.Aberrant:
                targetSpeed = 0.6f;
                hitRadius = 40f;
                break;
        }

        // Larger fish are easier to hit but move unpredictably
        if (fish.weight > 10f)
        {
            targetSize = 80f;
            hitRadius += 20f;
        }
    }

    public override void OnMinigameStart()
    {
        isActive = true;
        timeElapsed = 0f;
        hasThrown = false;

        // Initialize target at random position
        targetPosition = new Vector2(Random.Range(0.3f, 0.7f), Random.Range(0.3f, 0.7f));
        targetVelocity = Random.insideUnitCircle.normalized * targetSpeed;

        // Initialize cursor at center
        cursorPosition = new Vector2(0.5f, 0.5f);

        Debug.Log("Harpoon Minigame Started! Aim at the fish and throw!");
    }

    public override void UpdateMinigame(float deltaTime)
    {
        if (!isActive || hasThrown)
            return;

        timeElapsed += deltaTime;

        // Timeout check
        if (timeElapsed >= gameTime)
        {
            // Missed - took too long
            MissedThrow();
            return;
        }

        // Update target movement
        UpdateTargetMovement(deltaTime);

        // Handle input
        HandleInput(deltaTime);

        // Check for throw
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Fire1"))
        {
            if (isCharging)
            {
                // Release throw
                ThrowHarpoon();
            }
        }

        // Update charge
        if (Input.GetKey(KeyCode.Space) || Input.GetButton("Fire1"))
        {
            isCharging = true;
            chargeTime += deltaTime;
            chargeTime = Mathf.Min(chargeTime, maxChargeTime);
        }
    }

    private void HandleInput(float deltaTime)
    {
        // Move cursor with WASD or arrow keys
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector2 moveDirection = new Vector2(horizontal, vertical);
        cursorPosition += moveDirection * cursorSpeed * deltaTime;

        // Clamp cursor to screen
        cursorPosition.x = Mathf.Clamp01(cursorPosition.x);
        cursorPosition.y = Mathf.Clamp01(cursorPosition.y);
    }

    private void UpdateTargetMovement(float deltaTime)
    {
        // Move target
        targetPosition += targetVelocity * deltaTime;

        // Bounce off edges
        if (targetPosition.x <= 0.1f || targetPosition.x >= 0.9f)
        {
            targetVelocity.x *= -1f;
            targetPosition.x = Mathf.Clamp(targetPosition.x, 0.1f, 0.9f);
        }

        if (targetPosition.y <= 0.1f || targetPosition.y >= 0.9f)
        {
            targetVelocity.y *= -1f;
            targetPosition.y = Mathf.Clamp(targetPosition.y, 0.1f, 0.9f);
        }

        // Aberrant fish move erratically
        if (fish.isAberrant && Random.value < 0.02f)
        {
            targetVelocity = Random.insideUnitCircle.normalized * targetSpeed;
        }
    }

    private void ThrowHarpoon()
    {
        hasThrown = true;

        // Calculate distance between cursor and target
        Vector2 screenCursor = new Vector2(cursorPosition.x * Screen.width, cursorPosition.y * Screen.height);
        Vector2 screenTarget = new Vector2(targetPosition.x * Screen.width, targetPosition.y * Screen.height);
        float distance = Vector2.Distance(screenCursor, screenTarget);

        // Check if hit
        bool isHit = distance <= hitRadius;

        // Perfect throw bonus (fully charged)
        bool isPerfect = chargeTime >= maxChargeTime - 0.1f;

        if (isHit)
        {
            // Success!
            Debug.Log($"Harpoon Hit! {(isPerfect ? "Perfect throw!" : "")}");
            EventSystem.Publish("OnFishCaught", fish);
            OnMinigameEnd();
        }
        else
        {
            // Missed
            MissedThrow();
        }
    }

    private void MissedThrow()
    {
        Debug.Log("Harpoon missed! Fish scared away.");
        EventSystem.Publish("OnLineBroken");
        OnMinigameEnd();
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

        // Draw target fish
        Vector2 screenTarget = new Vector2(targetPosition.x * Screen.width, targetPosition.y * Screen.height);
        Rect targetRect = new Rect(screenTarget.x - targetSize / 2, screenTarget.y - targetSize / 2, targetSize, targetSize);

        GUI.color = fish.isAberrant ? new Color(1f, 0f, 1f, 0.8f) : new Color(0f, 0.5f, 1f, 0.8f);
        GUI.DrawTexture(targetRect, Texture2D.whiteTexture);

        // Draw hit radius (for debugging)
        if (Debug.isDebugBuild)
        {
            GUI.color = new Color(1f, 1f, 0f, 0.2f);
            Rect hitCircle = new Rect(screenTarget.x - hitRadius, screenTarget.y - hitRadius, hitRadius * 2, hitRadius * 2);
            GUI.DrawTexture(hitCircle, Texture2D.whiteTexture);
        }

        // Draw cursor/crosshair
        Vector2 screenCursor = new Vector2(cursorPosition.x * Screen.width, cursorPosition.y * Screen.height);
        float crosshairSize = 40f;

        GUI.color = isCharging ? Color.red : Color.white;

        // Horizontal line
        GUI.DrawTexture(new Rect(screenCursor.x - crosshairSize / 2, screenCursor.y - 2, crosshairSize, 4), Texture2D.whiteTexture);
        // Vertical line
        GUI.DrawTexture(new Rect(screenCursor.x - 2, screenCursor.y - crosshairSize / 2, 4, crosshairSize), Texture2D.whiteTexture);

        // Charge indicator
        if (isCharging)
        {
            float chargeWidth = 200f;
            float chargeHeight = 20f;
            float chargeX = Screen.width / 2 - chargeWidth / 2;
            float chargeY = Screen.height - 100f;

            // Background
            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(chargeX - 2, chargeY - 2, chargeWidth + 4, chargeHeight + 4), Texture2D.whiteTexture);

            // Charge bar
            float chargeFill = (chargeTime / maxChargeTime) * chargeWidth;
            GUI.color = chargeTime >= maxChargeTime - 0.1f ? Color.green : Color.yellow;
            GUI.DrawTexture(new Rect(chargeX, chargeY, chargeFill, chargeHeight), Texture2D.whiteTexture);
        }

        // Instructions
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.UpperCenter;

        GUI.color = Color.white;
        GUI.Label(new Rect(0, 50, Screen.width, 30), $"Time: {(gameTime - timeElapsed):F1}s", style);
        GUI.Label(new Rect(0, 80, Screen.width, 30), "WASD: Move | SPACE: Charge & Throw", style);
    }

    #endregion
}
