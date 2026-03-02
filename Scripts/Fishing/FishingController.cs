using UnityEngine;

/// <summary>
/// Main fishing controller that manages the fishing gameplay loop.
/// Implements IFishable interface and coordinates with mini-games, tension system, and tools.
/// Inspired by Cast n Chill's active combat system.
/// </summary>
public class FishingController : MonoBehaviour, IFishable
{
    [Header("Fishing Settings")]
    [SerializeField] private float fishingRange = 5f;
    [SerializeField] private float castTime = 1f;
    [SerializeField] private float biteWaitTimeMin = 2f;
    [SerializeField] private float biteWaitTimeMax = 8f;

    [Header("References")]
    [SerializeField] private Transform castPoint;
    [SerializeField] private BoatController boatController;
    [SerializeField] private TensionSystem tensionSystem;

    [Header("Current State")]
    private FishingState currentState = FishingState.Idle;
    private BaseFishingTool currentTool;
    private Fish hookedFish;
    private float stateTimer = 0f;
    private Vector3 castPosition;
    private BaseMinigame activeMinigame;

    // Fishing zone tracking
    private bool isInFishingZone = false;
    private FishingZone currentZone;

    #region Fishing State Machine

    public enum FishingState
    {
        Idle,           // Not fishing
        Casting,        // Casting line into water
        Waiting,        // Waiting for fish to bite
        Hooked,         // Fish hooked, starting minigame
        Reeling,        // Active minigame (player reeling)
        Caught,         // Successfully caught fish
        LineBroken      // Failed - line broke
    }

    #endregion

    private void Awake()
    {
        if (boatController == null)
            boatController = GetComponentInParent<BoatController>();

        if (tensionSystem == null)
            tensionSystem = GetComponent<TensionSystem>();

        if (tensionSystem == null)
        {
            tensionSystem = gameObject.AddComponent<TensionSystem>();
        }
    }

    private void Start()
    {
        // Subscribe to events
        EventSystem.Subscribe<Fish>("OnFishBite", OnFishBite);
        EventSystem.Subscribe<Fish>("OnFishCaught", OnFishCaughtInternal);
        EventSystem.Subscribe("OnLineBroken", OnLineBrokenInternal);
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        EventSystem.Unsubscribe<Fish>("OnFishBite", OnFishBite);
        EventSystem.Unsubscribe<Fish>("OnFishCaught", OnFishCaughtInternal);
        EventSystem.Unsubscribe("OnLineBroken", OnLineBrokenInternal);
    }

    private void Update()
    {
        // Handle input
        HandleInput();

        // Update state machine
        UpdateStateMachine();
    }

    private void HandleInput()
    {
        // Start fishing with fishing rod (E key or gamepad button)
        if (Input.GetKeyDown(KeyCode.E) && currentState == FishingState.Idle && isInFishingZone)
        {
            if (currentTool != null)
            {
                StartFishing();
            }
            else
            {
                Debug.LogWarning("No fishing tool equipped!");
            }
        }

        // Cancel fishing (Escape or back button)
        if (Input.GetKeyDown(KeyCode.Escape) && IsFishing())
        {
            StopFishing();
        }

        // During active reeling, pass input to minigame
        if (currentState == FishingState.Reeling && activeMinigame != null)
        {
            // Minigame handles its own input
        }
    }

    private void UpdateStateMachine()
    {
        stateTimer += Time.deltaTime;

        switch (currentState)
        {
            case FishingState.Idle:
                // Waiting for player to start fishing
                break;

            case FishingState.Casting:
                UpdateCasting();
                break;

            case FishingState.Waiting:
                UpdateWaiting();
                break;

            case FishingState.Hooked:
                UpdateHooked();
                break;

            case FishingState.Reeling:
                UpdateReeling();
                break;

            case FishingState.Caught:
                UpdateCaught();
                break;

            case FishingState.LineBroken:
                UpdateLineBroken();
                break;
        }
    }

    #region State Update Methods

    private void UpdateCasting()
    {
        if (stateTimer >= castTime)
        {
            // Cast complete, now waiting for bite
            ChangeState(FishingState.Waiting);

            // Request fish spawn from Fish Manager (Agent 8)
            // For now, use mock data - will integrate with Agent 8 later
            RequestFishSpawn();
        }
    }

    private void UpdateWaiting()
    {
        // Fish will bite after random delay (handled by Fish AI system)
        // When bite occurs, OnFishBite event will be triggered

        // Show waiting indicator/animation
        // Could add bobber movement animation here
    }

    private void UpdateHooked()
    {
        if (stateTimer >= 0.5f) // Brief hook delay
        {
            // Start the appropriate minigame based on tool and fish
            StartMinigame();
            ChangeState(FishingState.Reeling);
        }
    }

    private void UpdateReeling()
    {
        // Minigame is running - it will trigger events when complete
        if (activeMinigame != null)
        {
            activeMinigame.UpdateMinigame(Time.deltaTime);
        }
    }

    private void UpdateCaught()
    {
        if (stateTimer >= 1.5f)
        {
            // Add fish to inventory (will integrate with Agent 6)
            AddFishToInventory(hookedFish);

            // Reset to idle
            ChangeState(FishingState.Idle);
            hookedFish = null;
        }
    }

    private void UpdateLineBroken()
    {
        if (stateTimer >= 2f)
        {
            // Reset to idle
            ChangeState(FishingState.Idle);
            hookedFish = null;
        }
    }

    #endregion

    #region State Machine Management

    private void ChangeState(FishingState newState)
    {
        // Exit old state
        ExitState(currentState);

        // Change state
        currentState = newState;
        stateTimer = 0f;

        // Enter new state
        EnterState(newState);

        Debug.Log($"Fishing State Changed: {newState}");
    }

    private void EnterState(FishingState state)
    {
        switch (state)
        {
            case FishingState.Idle:
                // Re-enable boat controls
                if (boatController != null)
                    boatController.SetMovementEnabled(true);

                EventSystem.Publish("OnFishingEnded");
                break;

            case FishingState.Casting:
                // Disable boat controls during fishing
                if (boatController != null)
                    boatController.SetMovementEnabled(false);

                EventSystem.Publish("OnFishingStarted");

                // Calculate cast position
                castPosition = castPoint != null ? castPoint.position : transform.position + transform.forward * fishingRange;

                // Play cast animation/sound
                break;

            case FishingState.Waiting:
                // Show fishing indicator
                break;

            case FishingState.Hooked:
                // Play hook sound/animation
                // Show "Fish On!" indicator
                break;

            case FishingState.Reeling:
                // Initialize tension system
                if (tensionSystem != null && hookedFish != null)
                {
                    tensionSystem.Initialize(hookedFish, currentTool);
                }
                break;

            case FishingState.Caught:
                // Play success animation/sound
                Debug.Log($"Caught {hookedFish.name}!");
                break;

            case FishingState.LineBroken:
                // Play failure animation/sound
                Debug.Log("Line broke! Fish escaped.");
                break;
        }
    }

    private void ExitState(FishingState state)
    {
        switch (state)
        {
            case FishingState.Reeling:
                // Clean up minigame
                if (activeMinigame != null)
                {
                    activeMinigame.OnMinigameEnd();
                    activeMinigame = null;
                }

                // Reset tension system
                if (tensionSystem != null)
                {
                    tensionSystem.Reset();
                }
                break;
        }
    }

    #endregion

    #region Minigame Management

    private void StartMinigame()
    {
        if (currentTool == null || hookedFish == null)
            return;

        // Determine which minigame to use based on tool type
        if (currentTool is FishingRod)
        {
            activeMinigame = gameObject.AddComponent<ReelMinigame>();
        }
        else if (currentTool is Harpoon)
        {
            activeMinigame = gameObject.AddComponent<HarpoonMinigame>();
        }
        else if (currentTool.GetType().Name == "DredgeCrane") // For future implementation
        {
            activeMinigame = gameObject.AddComponent<DredgeMinigame>();
        }

        if (activeMinigame != null)
        {
            activeMinigame.Initialize(hookedFish, currentTool, tensionSystem);
            activeMinigame.OnMinigameStart();
        }
    }

    #endregion

    #region Event Handlers

    private void OnFishBite(Fish fish)
    {
        if (currentState == FishingState.Waiting)
        {
            hookedFish = fish;
            ChangeState(FishingState.Hooked);
        }
    }

    private void OnFishCaughtInternal(Fish fish)
    {
        if (currentState == FishingState.Reeling)
        {
            ChangeState(FishingState.Caught);

            // Publish public event for other systems
            EventSystem.Publish("FishCaught", fish);
        }
    }

    private void OnLineBrokenInternal()
    {
        if (currentState == FishingState.Reeling)
        {
            ChangeState(FishingState.LineBroken);

            // Publish line broken event
            EventSystem.Publish("LineBroken");
        }
    }

    #endregion

    #region Helper Methods

    private void RequestFishSpawn()
    {
        // This will integrate with Agent 8 (Fish AI)
        // For now, create mock bite after random delay
        float biteDelay = Random.Range(biteWaitTimeMin, biteWaitTimeMax);

        Invoke(nameof(SimulateFishBite), biteDelay);
    }

    private void SimulateFishBite()
    {
        // MOCK DATA - Will be replaced by Agent 8 integration
        Fish mockFish = new Fish
        {
            id = "bass_001",
            name = "Largemouth Bass",
            rarity = FishRarity.Common,
            baseValue = 25f,
            inventorySize = new Vector2Int(2, 1),
            weight = 2.5f,
            length = 35f
        };

        EventSystem.Publish("OnFishBite", mockFish);
    }

    private void AddFishToInventory(Fish fish)
    {
        // Will integrate with Agent 6 (Inventory)
        Debug.Log($"Adding {fish.name} to inventory (integration pending)");

        // For now, just add to money
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            GameState state = gameManager.CurrentGameState;
            state.money += fish.baseValue;
            gameManager.UpdateGameState(state);
        }
    }

    #endregion

    #region Fishing Zone Management

    private void OnTriggerEnter(Collider other)
    {
        FishingZone zone = other.GetComponent<FishingZone>();
        if (zone != null)
        {
            isInFishingZone = true;
            currentZone = zone;
            Debug.Log($"Entered fishing zone: {zone.zoneName}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        FishingZone zone = other.GetComponent<FishingZone>();
        if (zone != null && zone == currentZone)
        {
            isInFishingZone = false;
            currentZone = null;

            // Stop fishing if leaving zone
            if (IsFishing())
            {
                StopFishing();
            }
        }
    }

    #endregion

    #region IFishable Implementation

    public void StartFishing()
    {
        if (currentState != FishingState.Idle)
        {
            Debug.LogWarning("Already fishing!");
            return;
        }

        if (currentTool == null)
        {
            Debug.LogWarning("No fishing tool equipped!");
            return;
        }

        if (!isInFishingZone)
        {
            Debug.LogWarning("Not in a fishing zone!");
            return;
        }

        // Start fishing
        ChangeState(FishingState.Casting);
    }

    public void StopFishing()
    {
        if (currentState == FishingState.Idle)
            return;

        // Force return to idle
        ChangeState(FishingState.Idle);
        hookedFish = null;
    }

    public bool IsFishing()
    {
        return currentState != FishingState.Idle;
    }

    public object GetCurrentTool()
    {
        return currentTool;
    }

    #endregion

    #region Public API

    public void EquipTool(BaseFishingTool tool)
    {
        if (IsFishing())
        {
            Debug.LogWarning("Cannot change tools while fishing!");
            return;
        }

        currentTool = tool;
        Debug.Log($"Equipped {tool.toolName}");
    }

    public FishingState GetCurrentState() => currentState;

    public Fish GetHookedFish() => hookedFish;

    #endregion

    #region Debug Visualization

    private void OnDrawGizmos()
    {
        if (castPoint != null)
        {
            // Draw fishing range
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(castPoint.position, fishingRange);

            // Draw cast position when fishing
            if (currentState != FishingState.Idle)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(castPosition, 0.3f);
                Gizmos.DrawLine(castPoint.position, castPosition);
            }
        }
    }

    #endregion
}

/// <summary>
/// Base class for all fishing minigames
/// </summary>
public abstract class BaseMinigame : MonoBehaviour
{
    protected Fish fish;
    protected BaseFishingTool tool;
    protected TensionSystem tensionSystem;

    public virtual void Initialize(Fish fish, BaseFishingTool tool, TensionSystem tensionSystem)
    {
        this.fish = fish;
        this.tool = tool;
        this.tensionSystem = tensionSystem;
    }

    public abstract void OnMinigameStart();
    public abstract void UpdateMinigame(float deltaTime);
    public abstract void OnMinigameEnd();
}

/// <summary>
/// Base class for all fishing tools
/// </summary>
public abstract class BaseFishingTool : MonoBehaviour
{
    public string toolName;
    public float durability = 100f;
    public float power = 1f;

    public abstract bool CanCatchFish(Fish fish);
}

/// <summary>
/// Component for defining fishing zones in the world
/// </summary>
public class FishingZone : MonoBehaviour
{
    public string zoneName = "Fishing Spot";
    public string locationID = "starter_lake";
}
