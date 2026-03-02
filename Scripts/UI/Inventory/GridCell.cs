using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Represents a single cell in the inventory grid UI.
/// Handles visual state and mouse events.
/// </summary>
public class GridCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Visual Components")]
    [SerializeField] private Image _background;
    [SerializeField] private Image _itemIcon;
    [SerializeField] private Image _highlight;

    [Header("Colors")]
    [SerializeField] private Color _normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color _occupiedColor = new Color(0.4f, 0.4f, 0.4f, 0.8f);
    [SerializeField] private Color _hoverColor = new Color(0.3f, 0.3f, 0.5f, 0.8f);
    [SerializeField] private Color _validPlacementColor = new Color(0.2f, 0.8f, 0.2f, 0.6f);
    [SerializeField] private Color _invalidPlacementColor = new Color(0.8f, 0.2f, 0.2f, 0.6f);
    [SerializeField] private Color _coolerColor = new Color(0.2f, 0.5f, 0.8f, 0.8f);

    // Grid position
    private int _gridX;
    private int _gridY;

    // State
    private bool _isOccupied;
    private bool _isHighlighted;
    private bool _isCoolerSlot;
    private InventoryItem _containedItem;

    // Events
    public event System.Action<GridCell> OnCellHoverEnter;
    public event System.Action<GridCell> OnCellHoverExit;

    /// <summary>
    /// Grid X coordinate.
    /// </summary>
    public int GridX => _gridX;

    /// <summary>
    /// Grid Y coordinate.
    /// </summary>
    public int GridY => _gridY;

    /// <summary>
    /// Whether this cell is occupied by an item.
    /// </summary>
    public bool IsOccupied => _isOccupied;

    /// <summary>
    /// The item occupying this cell (if any).
    /// </summary>
    public InventoryItem ContainedItem => _containedItem;

    /// <summary>
    /// Whether this is a cooler slot.
    /// </summary>
    public bool IsCoolerSlot
    {
        get => _isCoolerSlot;
        set
        {
            _isCoolerSlot = value;
            UpdateVisuals();
        }
    }

    /// <summary>
    /// Initializes the grid cell with its position.
    /// </summary>
    public void Initialize(int gridX, int gridY)
    {
        _gridX = gridX;
        _gridY = gridY;
        _isOccupied = false;
        _isHighlighted = false;
        _isCoolerSlot = false;
        _containedItem = null;

        UpdateVisuals();
    }

    /// <summary>
    /// Sets the occupied state of this cell.
    /// </summary>
    public void SetOccupied(bool occupied, InventoryItem item = null)
    {
        _isOccupied = occupied;
        _containedItem = item;

        // Update icon
        if (_itemIcon != null)
        {
            if (occupied && item != null)
            {
                _itemIcon.sprite = item.Icon;
                _itemIcon.enabled = true;

                // Only show icon on the item's origin cell
                if (item.GridX != _gridX || item.GridY != _gridY)
                {
                    _itemIcon.enabled = false;
                }
            }
            else
            {
                _itemIcon.sprite = null;
                _itemIcon.enabled = false;
            }
        }

        UpdateVisuals();
    }

    /// <summary>
    /// Highlights this cell with a specific state.
    /// </summary>
    public void SetHighlight(bool highlighted, bool isValid = true)
    {
        _isHighlighted = highlighted;

        if (_highlight != null)
        {
            _highlight.enabled = highlighted;

            if (highlighted)
            {
                _highlight.color = isValid ? _validPlacementColor : _invalidPlacementColor;
            }
        }
    }

    /// <summary>
    /// Updates the visual appearance based on current state.
    /// </summary>
    private void UpdateVisuals()
    {
        if (_background == null)
            return;

        // Determine background color
        Color targetColor = _normalColor;

        if (_isCoolerSlot)
        {
            targetColor = _coolerColor;
        }
        else if (_isOccupied)
        {
            targetColor = _occupiedColor;
        }

        _background.color = targetColor;
    }

    /// <summary>
    /// Shows hover effect.
    /// </summary>
    public void ShowHover()
    {
        if (_background != null && !_isCoolerSlot)
        {
            _background.color = _hoverColor;
        }
    }

    /// <summary>
    /// Hides hover effect.
    /// </summary>
    public void HideHover()
    {
        UpdateVisuals();
    }

    // ===== Unity Event Handlers =====

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowHover();
        OnCellHoverEnter?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideHover();
        OnCellHoverExit?.Invoke(this);
    }

    /// <summary>
    /// Plays a visual feedback animation (e.g., when item is placed).
    /// </summary>
    public void PlayPlacementFeedback()
    {
        // Could implement a simple scale pulse or color flash
        // For now, we'll use a simple color flash
        if (_background != null)
        {
            StartCoroutine(FlashCoroutine());
        }
    }

    private System.Collections.IEnumerator FlashCoroutine()
    {
        Color original = _background.color;
        _background.color = Color.white;

        yield return new WaitForSeconds(0.1f);

        _background.color = original;
    }

    /// <summary>
    /// Plays an invalid placement shake animation.
    /// </summary>
    public void PlayInvalidFeedback()
    {
        StartCoroutine(ShakeCoroutine());
    }

    private System.Collections.IEnumerator ShakeCoroutine()
    {
        Vector3 originalPos = transform.localPosition;
        float shakeDuration = 0.3f;
        float shakeMagnitude = 5f;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            transform.localPosition = originalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }

    /// <summary>
    /// Gets the world position of this cell's center.
    /// </summary>
    public Vector3 GetCenterPosition()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            return rectTransform.position;
        }

        return transform.position;
    }

    /// <summary>
    /// Gets the RectTransform of this cell.
    /// </summary>
    public RectTransform GetRectTransform()
    {
        return GetComponent<RectTransform>();
    }
}
