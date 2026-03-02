using UnityEngine;

/// <summary>
/// Central game manager implementing Singleton pattern.
/// Manages game initialization, state transitions, and core lifecycle events.
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    /// <summary>
    /// Singleton instance accessor. Creates a new instance if none exists.
    /// </summary>
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                }
            }
            return _instance;
        }
    }

    [Header("Game State")]
    [SerializeField] private GameState currentGameState;

    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogging = true;

    /// <summary>
    /// Gets the current game state.
    /// </summary>
    public GameState CurrentGameState => currentGameState;

    /// <summary>
    /// Initializes the GameManager singleton and ensures it persists across scenes.
    /// </summary>
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        Initialize();
    }

    /// <summary>
    /// Initializes all core game systems and managers.
    /// </summary>
    private void Initialize()
    {
        currentGameState = new GameState
        {
            currentTime = 8f, // Start at 8 AM
            timeOfDay = TimeOfDay.Day,
            weather = WeatherType.Clear,
            sanity = 100f,
            playerPosition = Vector3.zero,
            currentLocationID = "starter_lake"
        };

        if (enableDebugLogging)
        {
            Debug.Log("[GameManager] Initialized successfully");
        }

        // Notify all systems that the game is ready
        EventSystem.Publish("GameInitialized", this);
    }

    /// <summary>
    /// Updates the current game state and publishes update event.
    /// </summary>
    /// <param name="newState">The new game state to apply</param>
    public void UpdateGameState(GameState newState)
    {
        currentGameState = newState;
        EventSystem.Publish("GameStateUpdated", currentGameState);
    }

    /// <summary>
    /// Pauses the game.
    /// </summary>
    public void PauseGame()
    {
        Time.timeScale = 0f;
        EventSystem.Publish("GamePaused", true);

        if (enableDebugLogging)
        {
            Debug.Log("[GameManager] Game paused");
        }
    }

    /// <summary>
    /// Resumes the game from paused state.
    /// </summary>
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        EventSystem.Publish("GamePaused", false);

        if (enableDebugLogging)
        {
            Debug.Log("[GameManager] Game resumed");
        }
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void QuitGame()
    {
        if (enableDebugLogging)
        {
            Debug.Log("[GameManager] Quitting game");
        }

        EventSystem.Publish("GameQuitting", this);

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
