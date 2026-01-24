using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMove : MonoBehaviour
{
    private const float YMin = -50.0f;
    private const float YMax = 50.0f;

    [Header("References")]
    public Transform lookAt;
    public Transform Player;

    [Header("Camera")]
    public float distance = 10.0f;
    public float sensivity = 4.0f;

    [Tooltip("Mouse delta is in pixels; this scales it to feel closer to the old Input.GetAxis.")]
    [SerializeField] private float mouseDeltaMultiplier = 0.1f;

    [Header("Input (New Input System)")]
    [SerializeField] private InputAction lookAction;

    private float currentX = 0.0f;
    private float currentY = 0.0f;

    private void Awake()
    {
        EnsureActionsConfigured();
    }

    private void OnEnable()
    {
        lookAction?.Enable();
    }

    private void OnDisable()
    {
        lookAction?.Disable();
    }

    private void EnsureActionsConfigured()
    {
        if (lookAction != null && lookAction.bindings.Count > 0)
            return;

        lookAction = new InputAction("Look", InputActionType.Value);

        // Mouse delta (pixels per frame)
        lookAction.AddBinding("<Mouse>/delta");

        // Gamepad right stick (-1..1)
        lookAction.AddBinding("<Gamepad>/rightStick");
    }

    private void LateUpdate()
    {
        if (lookAt == null) return;

        Vector2 lookInput = lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero;

        // Mouse delta needs extra scaling vs stick.
        // (Stick is already normalized; mouse is pixels.)
        float scale = Time.deltaTime * sensivity;
        Vector2 scaled = lookInput * scale;

        if (Mouse.current != null && Mouse.current.delta.IsActuated())
            scaled *= mouseDeltaMultiplier;

        currentX += scaled.x;
        currentY += scaled.y;

        currentY = Mathf.Clamp(currentY, YMin, YMax);

        Vector3 direction = new Vector3(0f, 0f, -distance);
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0f);

        transform.position = lookAt.position + rotation * direction;
        transform.LookAt(lookAt.position);
    }
}