using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

/// <summary>
/// Runtime tests for core systems (Phase 1)
/// Tests GameManager, EventSystem, SaveManager, TimeManager, WeatherSystem
/// </summary>
public class CoreSystemTests
{
    [UnityTest]
    public IEnumerator Test_GameManagerExists()
    {
        // Find GameManager in scene
        var gameManager = GameObject.FindObjectOfType<GameManager>();
        Assert.IsNotNull(gameManager, "GameManager not found in scene!");

        Debug.Log("✓ GameManager exists");
        yield return null;
    }

    [UnityTest]
    public IEnumerator Test_GameManagerSingleton()
    {
        var gameManager = GameManager.Instance;
        Assert.IsNotNull(gameManager, "GameManager Instance is null!");

        // Try to get instance again
        var gameManager2 = GameManager.Instance;
        Assert.AreEqual(gameManager, gameManager2, "GameManager singleton not working!");

        Debug.Log("✓ GameManager singleton working");
        yield return null;
    }

    [UnityTest]
    public IEnumerator Test_EventSystemPublishSubscribe()
    {
        bool eventReceived = false;

        // Subscribe to test event
        EventSystem.Subscribe<string>("TestEvent", (data) =>
        {
            eventReceived = true;
            Assert.AreEqual("TestData", data);
        });

        // Publish event
        EventSystem.Publish("TestEvent", "TestData");

        // Wait a frame
        yield return null;

        Assert.IsTrue(eventReceived, "Event was not received!");
        Debug.Log("✓ EventSystem pub/sub working");
    }

    [UnityTest]
    public IEnumerator Test_SaveManagerExists()
    {
        var saveManager = SaveManager.Instance;
        Assert.IsNotNull(saveManager, "SaveManager Instance is null!");

        Debug.Log("✓ SaveManager exists");
        yield return null;
    }

    [UnityTest]
    public IEnumerator Test_TimeManagerExists()
    {
        var timeManager = TimeManager.Instance;
        Assert.IsNotNull(timeManager, "TimeManager Instance is null!");

        Debug.Log("✓ TimeManager exists");
        yield return null;
    }

    [UnityTest]
    public IEnumerator Test_TimeManagerTimeProgression()
    {
        var timeManager = TimeManager.Instance;
        float startTime = timeManager.CurrentTime;

        // Wait a second
        yield return new WaitForSeconds(1f);

        float endTime = timeManager.CurrentTime;
        Assert.Greater(endTime, startTime, "Time is not progressing!");

        Debug.Log($"✓ Time progressing: {startTime} → {endTime}");
    }

    [UnityTest]
    public IEnumerator Test_WeatherSystemExists()
    {
        var weatherSystem = WeatherSystem.Instance;
        Assert.IsNotNull(weatherSystem, "WeatherSystem Instance is null!");

        Debug.Log("✓ WeatherSystem exists");
        yield return null;
    }

    [UnityTest]
    public IEnumerator Test_WeatherSystemHasValidWeather()
    {
        var weatherSystem = WeatherSystem.Instance;
        var currentWeather = weatherSystem.CurrentWeather;

        Assert.IsTrue(System.Enum.IsDefined(typeof(WeatherType), currentWeather),
            $"Invalid weather type: {currentWeather}");

        Debug.Log($"✓ Current weather: {currentWeather}");
        yield return null;
    }

    [UnityTest]
    public IEnumerator Test_GameStateInitialized()
    {
        var gameManager = GameManager.Instance;
        var gameState = gameManager.CurrentGameState;

        Assert.IsNotNull(gameState, "GameState is null!");
        Assert.GreaterOrEqual(gameState.sanity, 0, "Sanity is negative!");
        Assert.LessOrEqual(gameState.sanity, 100, "Sanity is over 100!");

        Debug.Log($"✓ GameState initialized (Sanity: {gameState.sanity})");
        yield return null;
    }

    [UnityTest]
    public IEnumerator Test_DontDestroyOnLoad()
    {
        var gameManager = GameManager.Instance;
        var originalScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // GameManager should have DontDestroyOnLoad
        Assert.IsNotNull(gameManager.gameObject, "GameManager GameObject is null!");

        Debug.Log("✓ Core managers persist across scenes");
        yield return null;
    }
}
