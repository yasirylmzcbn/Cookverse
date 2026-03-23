using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Attach this to the OvenDoor GameObject (the one with the door collider).
///
/// Supports two interaction modes:
///   Click  – left-click the door to toggle open/closed.
///   Drag   – click and drag down to open, drag up to close.
///            A small deadzone prevents accidental opens on plain clicks.
///
/// The door state is communicated to OvenSlot so the slot always knows
/// whether the door is open (useful if you later want to restrict placing
/// ingredients while the door is closed).
///
/// Inspector setup
/// ───────────────
///   Kitchen Camera  – the camera used to raycast clicks (same one KnobController uses)
///   Oven Slot       – the OvenSlot on the oven tray
///   Drag Threshold  – pixels of downward drag needed to trigger open (default 20)
///   Allow Drag      – uncheck to use click-only mode
/// </summary>
public class OvenDoorController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The kitchen camera used for raycasting.")]
    [SerializeField] private Camera kitchenCamera;

    [Tooltip("The OvenSlot this door belongs to.")]
    [SerializeField] private OvenSlot ovenSlot;

    [Header("Interaction")]
    [Tooltip("Allow dragging the door open/closed in addition to clicking.")]
    [SerializeField] private bool allowDrag = true;

    [Tooltip("How many pixels the mouse must move downward before a drag-open registers.")]
    [SerializeField] private float dragThreshold = 20f;

    // ── Internal state ───────────────────────────────────────────────────────
    private bool _isOpen = false;
    private bool _isDragging = false;
    private bool _dragConsumed = false;   // true once drag threshold was crossed this press
    private Vector2 _pressStartPos;

    private Mouse _mouse;

    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        _mouse = Mouse.current;
    }

    private void Update()
    {
        if (_mouse == null) return;

        // ── Press ────────────────────────────────────────────────────────────
        if (_mouse.leftButton.wasPressedThisFrame)
        {
            Ray ray = kitchenCamera.ScreenPointToRay(_mouse.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
            {
                _isDragging = true;
                _dragConsumed = false;
                _pressStartPos = _mouse.position.ReadValue();
            }
        }

        // ── Drag ─────────────────────────────────────────────────────────────
        if (_isDragging && allowDrag && !_dragConsumed)
        {
            Vector2 delta = _mouse.position.ReadValue() - _pressStartPos;

            // Drag DOWN  (negative Y in screen space) → open
            if (!_isOpen && delta.y < -dragThreshold)
            {
                Debug.Log($"OvenDoorController: Drag open detected (delta.y={delta.y})");
                Open();
                _dragConsumed = true;
            }
            // Drag UP → close
            else if (_isOpen && delta.y > dragThreshold)
            {
                Debug.Log($"OvenDoorController: Drag close detected (delta.y={delta.y})");
                Close();
                _dragConsumed = true;
            }
        }

        // ── Release ──────────────────────────────────────────────────────────
        if (_mouse.leftButton.wasReleasedThisFrame && _isDragging)
        {
            // If the mouse didn't move enough to count as a drag, treat it as a click toggle.
            if (!_dragConsumed)
            {
                Toggle();
            }

            _isDragging = false;
            _dragConsumed = false;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Door control
    // ─────────────────────────────────────────────────────────────────────────

    private void Toggle()
    {
        if (_isOpen) Close();
        else Open();
    }

    private void Open()
    {
        if (_isOpen) return;
        _isOpen = true;
        ovenSlot?.NotifyDragInSnapRange();   // reuses the existing door-open path in OvenSlot
    }

    private void Close()
    {
        if (!_isOpen) return;
        _isOpen = false;
        ovenSlot?.NotifyDragOutOfSnapRange(); // reuses the existing door-close path in OvenSlot
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Public API (optional – lets other scripts query or force door state)
    // ─────────────────────────────────────────────────────────────────────────

    public bool IsOpen => _isOpen;

    public void ForceOpen() => Open();
    public void ForceClose() => Close();
}