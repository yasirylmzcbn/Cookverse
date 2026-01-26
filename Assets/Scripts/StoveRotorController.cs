using UnityEngine;
using UnityEngine.InputSystem;

public class StoveRotorController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StoveScript stove;
    [SerializeField] private StoveScript.BurnerSide burnerSide = StoveScript.BurnerSide.Left;
    [SerializeField] private SwitchCamera switchCamera;
    [SerializeField] private Camera stoveViewCamera;

    [Tooltip("Transform that actually rotates. Use an empty pivot at the knob center if your mesh pivot is off.")]
    [SerializeField] private Transform rotationTarget;

    [Header("Hit Testing")]
    [SerializeField] private LayerMask interactionLayers = ~0;

    [Header("Rotation")]
    [Tooltip("Degrees when burner is fully OFF (relative to the initial local rotation).")]
    [SerializeField] private float offAngle = 0f;

    [Tooltip("Degrees when burner is fully ON (relative to the initial local rotation).")]
    [SerializeField] private float onAngle = 270f;

    [Tooltip("How fast the knob turns from mouse movement (degrees per pixel).")]
    [SerializeField] private float degreesPerPixel = 0.25f;

    [Tooltip("Axis to rotate around (in local space). Y is common for stove knobs.")]
    [SerializeField] private Vector3 localRotationAxis = Vector3.up;

    private Quaternion initialLocalRotation;
    private bool dragging;
    private float currentAngle;

    private void Awake()
    {
        if (rotationTarget == null)
            rotationTarget = transform;

        initialLocalRotation = rotationTarget.localRotation;

        if (switchCamera == null)
            switchCamera = FindFirstObjectByType<SwitchCamera>();

        if (stove == null)
            stove = GetComponentInParent<StoveScript>();

        ResolveStoveCamera();

        // Initialize knob position from current burner level.
        if (stove != null)
        {
            float level01 = stove.GetBurnerLevel(burnerSide);
            currentAngle = Mathf.Lerp(offAngle, onAngle, level01);
            ApplyKnobRotation();
        }
        else
        {
            currentAngle = offAngle;
            ApplyKnobRotation();
        }
    }

    private void ResolveStoveCamera()
    {
        if (stoveViewCamera != null)
            return;

        if (switchCamera != null && switchCamera.stoveCamera != null)
        {
            stoveViewCamera = switchCamera.stoveCamera.GetComponentInChildren<Camera>(true);
        }
    }

    private void Update()
    {
        if (switchCamera == null || !switchCamera.IsInStoveCamera)
        {
            dragging = false;
            return;
        }

        if (Mouse.current == null)
            return;

        ResolveStoveCamera();
        if (stoveViewCamera == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = stoveViewCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactionLayers, QueryTriggerInteraction.Ignore))
            {
                // Support colliders on child objects.
                var rotor = hit.collider.GetComponentInParent<StoveRotorController>();
                dragging = (rotor == this);
            }
        }

        if (dragging && Mouse.current.leftButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            float deltaDegrees = delta.x * degreesPerPixel;
            currentAngle = Mathf.Clamp(currentAngle + deltaDegrees, Mathf.Min(offAngle, onAngle), Mathf.Max(offAngle, onAngle));

            float level01 = Mathf.InverseLerp(offAngle, onAngle, currentAngle);
            ApplyKnobRotation();

            if (stove != null)
                stove.SetBurnerLevel(burnerSide, level01);
        }

        if (dragging && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            dragging = false;
        }
    }

    private void ApplyKnobRotation()
    {
        Vector3 axis = localRotationAxis.sqrMagnitude > 0.0001f ? localRotationAxis.normalized : Vector3.up;
        rotationTarget.localRotation = initialLocalRotation * Quaternion.AngleAxis(currentAngle, axis);
    }
}
