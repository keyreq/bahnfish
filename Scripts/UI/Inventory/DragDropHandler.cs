using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Handles drag and drop interactions for inventory items.
/// Manages mouse/touch input, visual feedback, and item placement validation.
/// </summary>
public class DragDropHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryUI _inventoryUI;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private GraphicRaycaster _raycaster;

    [Header("Drag Visual")]
    [SerializeField] private GameObject _dragPreviewPrefab;
    [SerializeField] private float _dragAlpha = 0.7f;

    [Header("Audio")]
    [SerializeField] private AudioClip _pickupSound;
    [SerializeField] private AudioClip _placeSound;
    [SerializeField] private AudioClip _invalidSound;
    [SerializeField] private AudioClip _rotateSound;

    // State
    private InventoryItem _draggedItem;
    private GameObject _dragPreview;
    private Vector2Int _dragStartPosition;
    private int _dragStartRotation;
    private bool _isDragging;

    // Input
    private PointerEventData _pointerEventData;
    private EventSystem _eventSystem;

    // Current hover cell
    private GridCell _currentHoverCell;

    /// <summary>
    /// Whether an item is currently being dragged.
    /// </summary>
    public bool IsDragging => _isDragging;

    /// <summary>
    /// The item currently being dragged.
    /// </summary>
    public InventoryItem DraggedItem => _draggedItem;

    private void Awake()
    {
        _eventSystem = EventSystem.current;

        if (_canvas == null)
            _canvas = GetComponentInParent<Canvas>();

        if (_raycaster == null)
            _raycaster = GetComponentInParent<GraphicRaycaster>();
    }

    private void Update()
    {
        // Handle rotation input
        if (_isDragging && Input.GetKeyDown(KeyCode.R))
        {
            RotateDraggedItem();
        }

        // Handle drag movement
        if (_isDragging)
        {
            UpdateDragPosition();
            UpdatePlacementPreview();
        }
    }

    /// <summary>
    /// Starts dragging an item from a grid cell.
    /// </summary>
    public void StartDrag(InventoryItem item, GridCell sourceCell)
    {
        if (item == null || _isDragging)
            return;

        _draggedItem = item;
        _dragStartPosition = new Vector2Int(item.GridX, item.GridY);
        _dragStartRotation = item.GetRotation();
        _isDragging = true;

        // Remove item from grid temporarily
        InventoryManager.Instance.RemoveItem(item);

        // Create drag preview
        CreateDragPreview();

        // Play sound
        PlaySound(_pickupSound);

        Debug.Log($"Started dragging {item.ItemName}");
    }

    /// <summary>
    /// Ends the drag operation, attempting to place the item.
    /// </summary>
    public void EndDrag()
    {
        if (!_isDragging || _draggedItem == null)
            return;

        // Find the cell under the cursor
        GridCell targetCell = GetCellUnderCursor();

        bool placed = false;

        if (targetCell != null)
        {
            // Attempt to place at target cell
            placed = InventoryManager.Instance.AddItemAt(_draggedItem, targetCell.GridX, targetCell.GridY);
        }

        if (!placed)
        {
            // Try to place anywhere in the grid
            placed = InventoryManager.Instance.AddItem(_draggedItem);
        }

        if (!placed)
        {
            // Return to original position
            _draggedItem.SetRotation(_dragStartRotation);
            bool returnedToOrigin = InventoryManager.Instance.AddItemAt(
                _draggedItem,
                _dragStartPosition.x,
                _dragStartPosition.y
            );

            if (!returnedToOrigin)
            {
                // Emergency: just try to add it anywhere
                InventoryManager.Instance.AddItem(_draggedItem);
            }

            // Play invalid sound and shake
            PlaySound(_invalidSound);
            if (targetCell != null)
            {
                targetCell.PlayInvalidFeedback();
            }

            Debug.LogWarning($"Could not place {_draggedItem.ItemName}, returned to origin");
        }
        else
        {
            // Successfully placed
            PlaySound(_placeSound);

            if (targetCell != null)
            {
                targetCell.PlayPlacementFeedback();
            }

            Debug.Log($"Placed {_draggedItem.ItemName} at ({_draggedItem.GridX}, {_draggedItem.GridY})");
        }

        // Cleanup
        CleanupDrag();
    }

    /// <summary>
    /// Cancels the drag operation, returning item to original position.
    /// </summary>
    public void CancelDrag()
    {
        if (!_isDragging || _draggedItem == null)
            return;

        // Return to original position and rotation
        _draggedItem.SetRotation(_dragStartRotation);
        InventoryManager.Instance.AddItemAt(_draggedItem, _dragStartPosition.x, _dragStartPosition.y);

        Debug.Log($"Cancelled drag of {_draggedItem.ItemName}");

        CleanupDrag();
    }

    /// <summary>
    /// Rotates the currently dragged item 90 degrees.
    /// </summary>
    public void RotateDraggedItem()
    {
        if (!_isDragging || _draggedItem == null)
            return;

        _draggedItem.Rotate();

        // Update preview
        UpdateDragPreview();

        PlaySound(_rotateSound);

        Debug.Log($"Rotated {_draggedItem.ItemName} to {_draggedItem.GetRotation()}°");
    }

    /// <summary>
    /// Creates the visual preview of the dragged item.
    /// </summary>
    private void CreateDragPreview()
    {
        if (_dragPreviewPrefab != null)
        {
            _dragPreview = Instantiate(_dragPreviewPrefab, _canvas.transform);
        }
        else
        {
            // Create a simple preview
            _dragPreview = new GameObject("DragPreview");
            _dragPreview.transform.SetParent(_canvas.transform);

            Image previewImage = _dragPreview.AddComponent<Image>();
            previewImage.sprite = _draggedItem.Icon;
            previewImage.raycastTarget = false;

            // Set alpha
            Color color = previewImage.color;
            color.a = _dragAlpha;
            previewImage.color = color;
        }

        // Set size based on item dimensions
        RectTransform rectTransform = _dragPreview.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // Size based on grid cells (assuming each cell is ~50 pixels)
            float cellSize = 50f;
            rectTransform.sizeDelta = new Vector2(
                _draggedItem.GetWidth() * cellSize,
                _draggedItem.GetHeight() * cellSize
            );
        }

        UpdateDragPosition();
    }

    /// <summary>
    /// Updates the drag preview position to follow the cursor.
    /// </summary>
    private void UpdateDragPosition()
    {
        if (_dragPreview == null)
            return;

        Vector2 screenPosition = Input.mousePosition;
        _dragPreview.transform.position = screenPosition;
    }

    /// <summary>
    /// Updates the drag preview visual based on item rotation.
    /// </summary>
    private void UpdateDragPreview()
    {
        if (_dragPreview == null)
            return;

        // Update rotation
        RectTransform rectTransform = _dragPreview.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.rotation = Quaternion.Euler(0, 0, -_draggedItem.GetRotation());

            // Update size based on rotation
            float cellSize = 50f;
            rectTransform.sizeDelta = new Vector2(
                _draggedItem.GetWidth() * cellSize,
                _draggedItem.GetHeight() * cellSize
            );
        }
    }

    /// <summary>
    /// Updates the placement preview on the grid.
    /// </summary>
    private void UpdatePlacementPreview()
    {
        GridCell targetCell = GetCellUnderCursor();

        if (targetCell != null && targetCell != _currentHoverCell)
        {
            // Clear old highlights
            _inventoryUI?.ClearHighlights();

            // Check if item can be placed here
            bool canPlace = InventoryManager.Instance.MainGrid.CanPlaceItem(
                _draggedItem,
                targetCell.GridX,
                targetCell.GridY
            );

            // Highlight cells that would be occupied
            List<Vector2Int> occupiedCells = _draggedItem.Shape.GetOccupiedCells();
            foreach (Vector2Int cell in occupiedCells)
            {
                int x = targetCell.GridX + cell.x;
                int y = targetCell.GridY + cell.y;

                _inventoryUI?.HighlightCell(x, y, canPlace);
            }

            _currentHoverCell = targetCell;
        }
        else if (targetCell == null && _currentHoverCell != null)
        {
            // Cursor left the grid
            _inventoryUI?.ClearHighlights();
            _currentHoverCell = null;
        }
    }

    /// <summary>
    /// Gets the grid cell currently under the cursor.
    /// </summary>
    private GridCell GetCellUnderCursor()
    {
        if (_raycaster == null || _eventSystem == null)
            return null;

        _pointerEventData = new PointerEventData(_eventSystem)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        _raycaster.Raycast(_pointerEventData, results);

        foreach (RaycastResult result in results)
        {
            GridCell cell = result.gameObject.GetComponent<GridCell>();
            if (cell != null)
            {
                return cell;
            }
        }

        return null;
    }

    /// <summary>
    /// Cleans up the drag operation.
    /// </summary>
    private void CleanupDrag()
    {
        _isDragging = false;
        _draggedItem = null;
        _currentHoverCell = null;

        if (_dragPreview != null)
        {
            Destroy(_dragPreview);
            _dragPreview = null;
        }

        // Clear highlights
        _inventoryUI?.ClearHighlights();

        // Refresh UI
        _inventoryUI?.RefreshGrid();
    }

    /// <summary>
    /// Plays a sound effect.
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            // Use AudioManager if available, otherwise simple playback
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(clip);
            }
            else
            {
                AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
            }
        }
    }

    // ===== Public API for UI Integration =====

    /// <summary>
    /// Called when a grid cell is clicked.
    /// </summary>
    public void OnCellClicked(GridCell cell)
    {
        if (_isDragging)
        {
            EndDrag();
        }
        else if (cell.IsOccupied && cell.ContainedItem != null)
        {
            StartDrag(cell.ContainedItem, cell);
        }
    }

    /// <summary>
    /// Called when right-click is detected (for rotation).
    /// </summary>
    public void OnRightClick(GridCell cell)
    {
        if (cell.IsOccupied && cell.ContainedItem != null && !_isDragging)
        {
            // Rotate item in place
            InventoryItem item = cell.ContainedItem;
            int oldRotation = item.GetRotation();

            item.Rotate();

            // Check if new rotation fits
            if (!InventoryManager.Instance.MainGrid.CanPlaceItem(item, item.GridX, item.GridY))
            {
                // Revert rotation
                item.SetRotation(oldRotation);
                PlaySound(_invalidSound);
            }
            else
            {
                // Refresh grid to show new rotation
                _inventoryUI?.RefreshGrid();
                PlaySound(_rotateSound);
            }
        }
    }
}
