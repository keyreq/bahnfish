using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Main UI controller for the inventory system.
/// Manages visual grid rendering, cooler slots, equipment slots, and user interactions.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("Grid References")]
    [SerializeField] private RectTransform _gridContainer;
    [SerializeField] private GameObject _gridCellPrefab;
    [SerializeField] private float _cellSize = 50f;
    [SerializeField] private float _cellSpacing = 2f;

    [Header("Cooler Slots")]
    [SerializeField] private RectTransform _coolerContainer;
    [SerializeField] private GameObject _coolerSlotPrefab;

    [Header("Equipment Slots")]
    [SerializeField] private RectTransform _equipmentContainer;
    [SerializeField] private GameObject _equipmentSlotPrefab;

    [Header("UI Panels")]
    [SerializeField] private GameObject _inventoryPanel;
    [SerializeField] private TextMeshProUGUI _capacityText;
    [SerializeField] private TextMeshProUGUI _valueText;
    [SerializeField] private TextMeshProUGUI _efficiencyText;
    [SerializeField] private Button _closeButton;

    [Header("Rotation Controls")]
    [SerializeField] private Button _rotateButton;
    [SerializeField] private TextMeshProUGUI _rotateKeyHint;

    [Header("Drag & Drop")]
    [SerializeField] private DragDropHandler _dragDropHandler;

    // Grid cells
    private GridCell[,] _gridCells;
    private List<GameObject> _coolerSlots = new List<GameObject>();
    private List<GameObject> _equipmentSlots = new List<GameObject>();

    // State
    private bool _isOpen;

    // Properties
    public bool IsOpen => _isOpen;

    private void Awake()
    {
        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(CloseInventory);
        }

        if (_rotateButton != null)
        {
            _rotateButton.onClick.AddListener(OnRotateButtonClicked);
        }

        // Initialize as closed
        if (_inventoryPanel != null)
        {
            _inventoryPanel.SetActive(false);
        }
    }

    private void Start()
    {
        // Subscribe to inventory events
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += RefreshGrid;
        }

        // Initialize the grid
        InitializeGrid();
        InitializeCoolerSlots();
        InitializeEquipmentSlots();

        // Initial refresh
        RefreshGrid();
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= RefreshGrid;
        }
    }

    private void Update()
    {
        // Toggle inventory with Tab key
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }

        // Handle ESC to close
        if (Input.GetKeyDown(KeyCode.Escape) && _isOpen)
        {
            CloseInventory();
        }

        // Handle right-click for rotation
        if (Input.GetMouseButtonDown(1) && _isOpen)
        {
            HandleRightClick();
        }
    }

    /// <summary>
    /// Initializes the grid cells.
    /// </summary>
    private void InitializeGrid()
    {
        if (_gridContainer == null || _gridCellPrefab == null || InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryUI: Missing required references");
            return;
        }

        int width = InventoryManager.Instance.MainGrid.Width;
        int height = InventoryManager.Instance.MainGrid.Height;

        _gridCells = new GridCell[width, height];

        // Configure grid layout
        GridLayoutGroup gridLayout = _gridContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = _gridContainer.gameObject.AddComponent<GridLayoutGroup>();
        }

        gridLayout.cellSize = new Vector2(_cellSize, _cellSize);
        gridLayout.spacing = new Vector2(_cellSpacing, _cellSpacing);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = width;

        // Create cells
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject cellObj = Instantiate(_gridCellPrefab, _gridContainer);
                GridCell cell = cellObj.GetComponent<GridCell>();

                if (cell != null)
                {
                    cell.Initialize(x, y);
                    cell.OnCellHoverEnter += OnCellHoverEnter;
                    cell.OnCellHoverExit += OnCellHoverExit;

                    // Add click handler
                    Button cellButton = cellObj.GetComponent<Button>();
                    if (cellButton != null)
                    {
                        GridCell capturedCell = cell; // Capture for lambda
                        cellButton.onClick.AddListener(() => OnCellClicked(capturedCell));
                    }

                    _gridCells[x, y] = cell;
                }
            }
        }

        Debug.Log($"Initialized {width}x{height} inventory grid UI");
    }

    /// <summary>
    /// Initializes cooler slot UI elements.
    /// </summary>
    private void InitializeCoolerSlots()
    {
        if (_coolerContainer == null || _coolerSlotPrefab == null)
            return;

        // Create cooler slots based on InventoryManager capacity
        // For now, create a few placeholder slots
        for (int i = 0; i < 4; i++)
        {
            GameObject slotObj = Instantiate(_coolerSlotPrefab, _coolerContainer);
            _coolerSlots.Add(slotObj);
        }
    }

    /// <summary>
    /// Initializes equipment slot UI elements.
    /// </summary>
    private void InitializeEquipmentSlots()
    {
        if (_equipmentContainer == null || _equipmentSlotPrefab == null)
            return;

        // Create equipment slots
        for (int i = 0; i < 6; i++)
        {
            GameObject slotObj = Instantiate(_equipmentSlotPrefab, _equipmentContainer);
            _equipmentSlots.Add(slotObj);
        }
    }

    /// <summary>
    /// Refreshes the entire grid display.
    /// </summary>
    public void RefreshGrid()
    {
        if (_gridCells == null || InventoryManager.Instance == null)
            return;

        // Clear all cells first
        for (int x = 0; x < _gridCells.GetLength(0); x++)
        {
            for (int y = 0; y < _gridCells.GetLength(1); y++)
            {
                if (_gridCells[x, y] != null)
                {
                    _gridCells[x, y].SetOccupied(false);
                }
            }
        }

        // Update cells with current items
        List<InventoryItem> items = InventoryManager.Instance.GetAllItems();

        foreach (InventoryItem item in items)
        {
            List<Vector2Int> cells = item.Shape.GetOccupiedCells();

            foreach (Vector2Int cell in cells)
            {
                int x = item.GridX + cell.x;
                int y = item.GridY + cell.y;

                if (x >= 0 && x < _gridCells.GetLength(0) && y >= 0 && y < _gridCells.GetLength(1))
                {
                    _gridCells[x, y].SetOccupied(true, item);
                }
            }
        }

        // Update stats display
        UpdateStatsDisplay();
    }

    /// <summary>
    /// Updates the capacity, value, and efficiency displays.
    /// </summary>
    private void UpdateStatsDisplay()
    {
        if (InventoryManager.Instance == null)
            return;

        // Capacity
        if (_capacityText != null)
        {
            int occupied = InventoryManager.Instance.GetOccupiedSpace();
            int total = InventoryManager.Instance.GetTotalCapacity();
            _capacityText.text = $"Capacity: {occupied}/{total}";
        }

        // Total fish value
        if (_valueText != null)
        {
            float value = InventoryManager.Instance.GetTotalFishValue();
            _valueText.text = $"Total Value: ${value:F2}";
        }

        // Efficiency
        if (_efficiencyText != null)
        {
            var stats = InventoryManager.Instance.GetEfficiencyStats();
            _efficiencyText.text = $"Efficiency: {stats.utilizationPercent:F1}%\n" +
                                   $"Fragmentation: {stats.fragmentationScore:F2}";
        }
    }

    /// <summary>
    /// Highlights a specific cell.
    /// </summary>
    public void HighlightCell(int x, int y, bool isValid)
    {
        if (x < 0 || x >= _gridCells.GetLength(0) || y < 0 || y >= _gridCells.GetLength(1))
            return;

        if (_gridCells[x, y] != null)
        {
            _gridCells[x, y].SetHighlight(true, isValid);
        }
    }

    /// <summary>
    /// Clears all cell highlights.
    /// </summary>
    public void ClearHighlights()
    {
        if (_gridCells == null)
            return;

        for (int x = 0; x < _gridCells.GetLength(0); x++)
        {
            for (int y = 0; y < _gridCells.GetLength(1); y++)
            {
                if (_gridCells[x, y] != null)
                {
                    _gridCells[x, y].SetHighlight(false);
                }
            }
        }
    }

    /// <summary>
    /// Opens the inventory UI.
    /// </summary>
    public void OpenInventory()
    {
        if (_inventoryPanel != null)
        {
            _inventoryPanel.SetActive(true);
            _isOpen = true;

            RefreshGrid();

            // Pause game (optional)
            Time.timeScale = 0f;

            Debug.Log("Inventory opened");
        }
    }

    /// <summary>
    /// Closes the inventory UI.
    /// </summary>
    public void CloseInventory()
    {
        if (_inventoryPanel != null)
        {
            _inventoryPanel.SetActive(false);
            _isOpen = false;

            // Cancel any active drag
            if (_dragDropHandler != null && _dragDropHandler.IsDragging)
            {
                _dragDropHandler.CancelDrag();
            }

            // Resume game
            Time.timeScale = 1f;

            Debug.Log("Inventory closed");
        }
    }

    /// <summary>
    /// Toggles the inventory UI open/closed.
    /// </summary>
    public void ToggleInventory()
    {
        if (_isOpen)
        {
            CloseInventory();
        }
        else
        {
            OpenInventory();
        }
    }

    // ===== Event Handlers =====

    private void OnCellClicked(GridCell cell)
    {
        if (_dragDropHandler != null)
        {
            _dragDropHandler.OnCellClicked(cell);
        }
    }

    private void OnCellHoverEnter(GridCell cell)
    {
        // Could show tooltip here
    }

    private void OnCellHoverExit(GridCell cell)
    {
        // Hide tooltip
    }

    private void OnRotateButtonClicked()
    {
        if (_dragDropHandler != null && _dragDropHandler.IsDragging)
        {
            _dragDropHandler.RotateDraggedItem();
        }
    }

    private void HandleRightClick()
    {
        // Find cell under cursor
        Vector2 mousePos = Input.mousePosition;
        // This is simplified - proper implementation would use raycasting
        // For now, we'll delegate to drag handler
    }

    // ===== Public API =====

    /// <summary>
    /// Shows a notification message on the inventory UI.
    /// </summary>
    public void ShowNotification(string message)
    {
        Debug.Log($"[Inventory] {message}");
        // Could implement a proper notification UI element
    }

    /// <summary>
    /// Highlights optimal placement positions for an item.
    /// </summary>
    public void ShowOptimalPlacement(InventoryItem item)
    {
        if (item == null || InventoryManager.Instance == null)
            return;

        ClearHighlights();

        var hints = InventoryManager.Instance.GetPlacementHints(item);

        // Show top 3 best placements
        int count = Mathf.Min(3, hints.Count);
        for (int i = 0; i < count; i++)
        {
            var hint = hints[i];
            List<Vector2Int> cells = item.Shape.GetOccupiedCells();

            foreach (Vector2Int cell in cells)
            {
                int x = hint.gridX + cell.x;
                int y = hint.gridY + cell.y;

                HighlightCell(x, y, true);
            }
        }
    }

    /// <summary>
    /// Gets the grid cell at a specific position.
    /// </summary>
    public GridCell GetCell(int x, int y)
    {
        if (x < 0 || x >= _gridCells.GetLength(0) || y < 0 || y >= _gridCells.GetLength(1))
            return null;

        return _gridCells[x, y];
    }
}
