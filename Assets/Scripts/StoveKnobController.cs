using UnityEngine;
using UnityEngine.InputSystem;

public class KnobController : MonoBehaviour
{
    [Header("Camera Reference")]
    [SerializeField] public Camera stoveCam;

    public enum BurnerSide { Left, Right }

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private float minAngle = 0f;
    [SerializeField] private float maxAngle = 180f;

    [Header("State")]
    [SerializeField] public BurnerSide side;
    [SerializeField] private bool isOn = false;
    [SerializeField] private float onThreshold = 30f; // Angle at which stove turns on

    private bool isDragging = false;
    private float currentAngle = 0f;
    private Vector2 lastMousePos;
    private Mouse mouse;
    private StoveScript stove;

    void Start()
    {
        mouse = Mouse.current;
        stove = GetComponentInParent<StoveScript>();
    }

    void Update()
    {
        if (mouse == null) return;

        // Check for mouse click on this object
        if (mouse.leftButton.wasPressedThisFrame)
        {
            Ray ray = stoveCam.ScreenPointToRay(mouse.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    Debug.Log("Started dragging knob");
                    isDragging = true;
                    lastMousePos = mouse.position.ReadValue();
                }
            }
        }

        // Check for mouse release
        if (mouse.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }

        // Handle dragging
        if (isDragging)
        {
            Vector2 currentMousePos = mouse.position.ReadValue();
            Vector2 mouseDelta = currentMousePos - lastMousePos;

            // Calculate rotation based on horizontal mouse movement
            float rotationAmount = mouseDelta.x * rotationSpeed;
            currentAngle += rotationAmount;

            // Clamp the angle to min/max range
            currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

            // Apply rotation (rotating around local Z-axis, adjust if needed)
            transform.localRotation = Quaternion.Euler(0, currentAngle, 0);

            // Update burner visuals continuously so heat level can reach 1.0
            if (stove != null)
            {
                stove.ApplyVisuals(side, GetHeatLevel());
            }

            // Check if stove should be on or off
            bool wasOn = isOn;
            isOn = currentAngle >= onThreshold;

            // Optional: Trigger events when state changes
            if (wasOn != isOn)
            {
                OnStateChanged(isOn);
            }

            lastMousePos = currentMousePos;
        }
    }

    // Override this or use Unity Events to communicate with your stove
    protected virtual void OnStateChanged(bool newState)
    {
        Debug.Log($"Knob {gameObject.name} is now: {(newState ? "ON" : "OFF")}");
        // Keep this for ON/OFF logic (sound, VFX enable/disable, etc.).
    }

    // Public method to get current state
    public bool IsOn()
    {
        return isOn;
    }

    // Public method to get current angle (useful for heat levels)
    public float GetCurrentAngle()
    {
        return currentAngle;
    }

    // Get normalized heat level (0 to 1)
    public float GetHeatLevel()
    {
        return Mathf.InverseLerp(minAngle, maxAngle, currentAngle);
    }
}