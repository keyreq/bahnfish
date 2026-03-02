# 🚀 Bahnfish - Quick Start Guide

## Ready to Build? Start Here!

This guide shows you exactly how to launch the 22 development agents in parallel and begin building Bahnfish immediately.

---

## Prerequisites Checklist

Before launching agents, ensure you have:

- [ ] Read GAME_DESIGN.md (at least the summary)
- [ ] Read AGENTS_DESIGN.md (at least your agent's section)
- [ ] Unity 2022 LTS installed
- [ ] Git repository initialized
- [ ] Discord server set up for team communication
- [ ] Task tracking system ready (Jira/Linear/GitHub Projects)

---

## Phase 1: Launch First 4 Agents (Week 1)

### Agent 1: Core Architecture Agent

**Launch Command** (if using Task tool):
```
Launch Agent 1 to set up Unity project structure, core managers, and base interfaces
```

**Manual Setup**:
1. Create Unity 2022 LTS project named "Bahnfish"
2. Create folder structure:
   ```
   Assets/
   ├── Scripts/
   │   ├── Core/
   │   ├── Interfaces/
   │   ├── Fishing/
   │   ├── Inventory/
   │   ├── Sanity/
   │   ├── Fish/
   │   ├── Progression/
   │   ├── UI/
   │   ├── Audio/
   │   ├── Environment/
   │   ├── Player/
   │   ├── World/
   │   ├── Narrative/
   │   ├── Crafting/
   │   └── Utils/
   ├── Art/
   │   ├── Models/
   │   ├── Textures/
   │   ├── Materials/
   │   └── Animations/
   ├── Audio/
   │   ├── Music/
   │   ├── SFX/
   │   └── Ambient/
   ├── Prefabs/
   ├── Scenes/
   └── Data/
       └── ScriptableObjects/
   ```

3. Create base classes in `Scripts/Core/`:

```csharp
// GameManager.cs
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
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

    private void Initialize()
    {
        // Initialize all managers
        Debug.Log("GameManager initialized");
    }
}
```

```csharp
// EventSystem.cs
using System;
using System.Collections.Generic;

public class EventSystem
{
    private static Dictionary<string, Delegate> eventTable = new Dictionary<string, Delegate>();

    public static void Subscribe<T>(string eventName, Action<T> listener)
    {
        if (!eventTable.ContainsKey(eventName))
        {
            eventTable[eventName] = null;
        }
        eventTable[eventName] = (Action<T>)eventTable[eventName] + listener;
    }

    public static void Unsubscribe<T>(string eventName, Action<T> listener)
    {
        if (eventTable.ContainsKey(eventName))
        {
            eventTable[eventName] = (Action<T>)eventTable[eventName] - listener;
        }
    }

    public static void Publish<T>(string eventName, T data)
    {
        if (eventTable.ContainsKey(eventName) && eventTable[eventName] != null)
        {
            ((Action<T>)eventTable[eventName]).Invoke(data);
        }
    }
}
```

4. Create interfaces in `Scripts/Interfaces/`:

```csharp
// IFishable.cs
public interface IFishable
{
    void StartFishing();
    void StopFishing();
}

// IInventoryItem.cs
public interface IInventoryItem
{
    string ItemID { get; }
    string ItemName { get; }
    int Width { get; }
    int Height { get; }
}

// IUpgradeable.cs
public interface IUpgradeable
{
    string UpgradeID { get; }
    int CurrentLevel { get; }
    bool CanUpgrade();
    void Upgrade();
}

// IInteractable.cs
public interface IInteractable
{
    void Interact();
    string GetInteractionPrompt();
}

// ISaveSystem.cs
public interface ISaveSystem
{
    void SaveGame();
    void LoadGame();
    bool HasSaveData();
}
```

5. Create data types in `Scripts/Core/DataTypes.cs`:

```csharp
using UnityEngine;

[System.Serializable]
public enum TimeOfDay
{
    Day,
    Dusk,
    Night,
    Dawn
}

[System.Serializable]
public enum WeatherType
{
    Clear,
    Rain,
    Storm,
    Fog
}

[System.Serializable]
public enum FishRarity
{
    Common,
    Uncommon,
    Rare,
    Legendary,
    Aberrant
}

[System.Serializable]
public class Fish
{
    public string id;
    public string name;
    public FishRarity rarity;
    public float baseValue;
    public Vector2Int inventorySize;
    public bool isAberrant;
    public Sprite icon;
}

[System.Serializable]
public class GameState
{
    public float currentTime;
    public TimeOfDay timeOfDay;
    public WeatherType weather;
    public float sanity;
    public Vector3 playerPosition;
    public string currentLocationID;
}
```

**Deliverable**: Core Unity project with base architecture ready for other agents.

---

### Agent 4: Save/Load System Agent

**Launch Command**:
```
Launch Agent 4 to implement save/load system with JSON serialization
```

**Implementation** in `Scripts/Core/`:

```csharp
// SaveManager.cs
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour, ISaveSystem
{
    private static SaveManager _instance;
    public static SaveManager Instance => _instance;

    private string savePath;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, "savegame.json");
    }

    public void SaveGame()
    {
        SaveData data = new SaveData
        {
            playerPosition = Vector3.zero, // Will be filled by other systems
            money = 0,
            sanity = 100f,
            currentTime = 0f
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"Game saved to {savePath}");
    }

    public void LoadGame()
    {
        if (!HasSaveData()) return;

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // Other systems will load their data here via events
        EventSystem.Publish("GameLoaded", data);
        Debug.Log("Game loaded");
    }

    public bool HasSaveData()
    {
        return File.Exists(savePath);
    }
}

[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition;
    public float money;
    public float sanity;
    public float currentTime;
    // Other systems will add their data here
}
```

---

### Agent 2: Input & Player Controller Agent

**Launch Command**:
```
Launch Agent 2 to implement boat movement and camera controls
```

**Implementation** in `Scripts/Player/`:

```csharp
// BoatController.cs
using UnityEngine;

public class BoatController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 100f;
    public float acceleration = 2f;

    private Rigidbody rb;
    private float currentSpeed = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Rotation
        transform.Rotate(Vector3.up, horizontal * rotationSpeed * Time.deltaTime);

        // Movement
        float targetSpeed = vertical * moveSpeed;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

        Vector3 movement = transform.forward * currentSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + movement);

        // Publish movement event for other systems
        if (movement.magnitude > 0.01f)
        {
            EventSystem.Publish("PlayerMoved", transform.position);
        }
    }

    public Vector3 GetPosition() => transform.position;
}
```

---

### Agent 3: Time & Environment Agent

**Launch Command**:
```
Launch Agent 3 to implement day/night cycle and lighting system
```

**Implementation** in `Scripts/Environment/`:

```csharp
// TimeManager.cs
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [Header("Time Settings")]
    public float dayLengthMinutes = 15f; // Real-time minutes for full day
    public float currentTime = 0f; // 0-24 hour format

    [Header("Lighting")]
    public Light sunLight;
    public Gradient dayNightColorGradient;

    private float timeScale;
    private TimeOfDay currentTimeOfDay;

    private void Start()
    {
        timeScale = 24f / (dayLengthMinutes * 60f);
        if (sunLight == null)
        {
            sunLight = FindObjectOfType<Light>();
        }
    }

    private void Update()
    {
        UpdateTime();
        UpdateLighting();
        CheckTimeOfDay();
    }

    private void UpdateTime()
    {
        currentTime += Time.deltaTime * timeScale;
        if (currentTime >= 24f)
        {
            currentTime -= 24f;
        }
    }

    private void UpdateLighting()
    {
        float t = currentTime / 24f;

        // Rotate sun
        float sunAngle = (currentTime / 24f) * 360f - 90f;
        sunLight.transform.rotation = Quaternion.Euler(sunAngle, 0f, 0f);

        // Adjust intensity
        sunLight.intensity = Mathf.Clamp01(Mathf.Sin(t * Mathf.PI * 2f)) * 1.5f;

        // Adjust color
        sunLight.color = dayNightColorGradient.Evaluate(t);
    }

    private void CheckTimeOfDay()
    {
        TimeOfDay newTimeOfDay;

        if (currentTime >= 6f && currentTime < 8f)
            newTimeOfDay = TimeOfDay.Dawn;
        else if (currentTime >= 8f && currentTime < 18f)
            newTimeOfDay = TimeOfDay.Day;
        else if (currentTime >= 18f && currentTime < 20f)
            newTimeOfDay = TimeOfDay.Dusk;
        else
            newTimeOfDay = TimeOfDay.Night;

        if (newTimeOfDay != currentTimeOfDay)
        {
            currentTimeOfDay = newTimeOfDay;
            EventSystem.Publish("TimeOfDayChanged", currentTimeOfDay);
            Debug.Log($"Time of day changed to: {currentTimeOfDay}");
        }
    }

    public TimeOfDay GetCurrentTimeOfDay() => currentTimeOfDay;
    public float GetCurrentTime() => currentTime;
}
```

---

## Testing Phase 1 (End of Week 4)

After all 4 agents complete their work, test integration:

```csharp
// IntegrationTest.cs (Scripts/Tests/)
using UnityEngine;

public class IntegrationTest : MonoBehaviour
{
    private void Start()
    {
        TestCoreArchitecture();
        TestSaveLoad();
        TestPlayerMovement();
        TestTimeSystem();
    }

    private void TestCoreArchitecture()
    {
        Debug.Log("Testing Core Architecture...");
        Debug.Assert(GameManager.Instance != null, "GameManager not found");
        Debug.Log("✓ Core Architecture working");
    }

    private void TestSaveLoad()
    {
        Debug.Log("Testing Save/Load...");
        SaveManager.Instance.SaveGame();
        Debug.Assert(SaveManager.Instance.HasSaveData(), "Save file not created");
        SaveManager.Instance.LoadGame();
        Debug.Log("✓ Save/Load working");
    }

    private void TestPlayerMovement()
    {
        Debug.Log("Testing Player Movement...");
        BoatController boat = FindObjectOfType<BoatController>();
        Debug.Assert(boat != null, "BoatController not found");
        Vector3 startPos = boat.GetPosition();
        Debug.Log($"✓ Player at {startPos}");
    }

    private void TestTimeSystem()
    {
        Debug.Log("Testing Time System...");
        TimeManager timeManager = FindObjectOfType<TimeManager>();
        Debug.Assert(timeManager != null, "TimeManager not found");
        Debug.Log($"✓ Time: {timeManager.GetCurrentTime()}, Period: {timeManager.GetCurrentTimeOfDay()}");
    }
}
```

---

## Phase 2: Launch Gameplay Agents (Week 5-10)

Once Phase 1 is complete, launch these agents **in parallel**:

### Week 5-6: Fishing & Fish AI

```
Launch Agent 5 (Fishing Mechanics) and Agent 8 (Fish AI) in parallel
```

### Week 7-8: Inventory & Sanity

```
Launch Agent 6 (Inventory) and Agent 7 (Sanity) in parallel
```

### Week 9: UI

```
Launch Agent 11 (UI/UX) for HUD and basic menus
```

---

## Phase 3-6: Continue Sequential Launches

Follow the schedule in DEVELOPMENT_STRATEGY.md for launching the remaining agents.

---

## Parallel Launch Commands (Claude Code)

If using Claude Code with the Task tool, you can launch multiple agents simultaneously:

```
Launch the following agents in parallel:
- Agent 1: Core Architecture
- Agent 2: Player Controller
- Agent 3: Time & Environment
- Agent 4: Save/Load System

Each agent should implement their deliverables from AGENTS_DESIGN.md and coordinate through the EventSystem.
```

---

## Daily Workflow

### Morning (9 AM)
1. Pull latest changes from main branch
2. Check Discord for blockers
3. Review overnight CI build results
4. Plan today's tasks

### Development (9 AM - 5 PM)
1. Work on your agent's deliverables
2. Commit frequently with descriptive messages
3. Use feature flags for incomplete work
4. Document as you code

### Evening (5 PM)
1. Push your day's work to feature branch
2. Update agent progress in task tracker
3. Post update in Discord
4. Review integration build

### Weekly (Friday)
1. Merge feature branch to main (if ready)
2. Create pull request for review
3. Attend weekly sync meeting
4. Update documentation

---

## Success Checklist

### Week 1 ✓
- [ ] Unity project created
- [ ] Core managers implemented
- [ ] Interfaces defined
- [ ] Save/load functional

### Week 4 ✓
- [ ] Player can move boat
- [ ] Time cycles through day/night
- [ ] Game saves and loads
- [ ] All Phase 1 tests passing

### Week 10 ✓
- [ ] Player can catch fish
- [ ] Fish stored in inventory
- [ ] Sanity system working
- [ ] Core loop is playable

---

## Need Help?

1. **Check documentation**:
   - GAME_DESIGN.md for game mechanics
   - AGENTS_DESIGN.md for your agent's responsibilities
   - DEVELOPMENT_STRATEGY.md for overall strategy

2. **Ask in Discord**:
   - #agent-[your-number] for agent-specific questions
   - #integration for cross-agent issues
   - #general for general help

3. **Create GitHub issue**:
   - Tag with your agent number
   - Describe the problem clearly
   - Include error messages/logs

---

## Let's Build! 🚀

You now have everything you need to start developing Bahnfish. Pick your agent, set up your environment, and dive in!

**Remember**:
- Define interfaces first
- Use mock data to avoid blocking
- Commit daily
- Communicate often
- Have fun! 🎣
