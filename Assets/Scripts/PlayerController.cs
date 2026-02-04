using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;

interface IInteractable
{
    public bool Interact();
}

public class PlayerController : MonoBehaviour
{
    public CharacterController controller;
    public float speed = 10;
    public float gravity = -9.81f * 2;
    public float jumpHeight = 3f;

    [Header("Movement Reference")]
    [Tooltip("If set, movement will be relative to this transform (usually the active camera). If left empty, uses the active SwitchCamera camera, else Camera.main.")]
    [SerializeField] private Transform movementReference;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Input (New Input System)")]
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction jumpAction;
    [SerializeField] private InputAction shootAction;

    Vector3 velocity;
    bool isGrounded;

    public Transform InteractorSource;
    public float InteractDistance = 3f;

    bool currentlyInteracting = false;

    SwitchCamera switchCamera;

    void Start()
    {
        switchCamera = FindFirstObjectByType<SwitchCamera>();
    }

    private Transform GetMovementReference()
    {
        if (movementReference != null)
            return movementReference;

        if (switchCamera != null)
        {
            if (switchCamera.kitchenCamera != null && switchCamera.kitchenCamera.activeInHierarchy)
                return switchCamera.kitchenCamera.transform;

            if (switchCamera.thirdPersonCamera != null && switchCamera.thirdPersonCamera.activeInHierarchy)
                return switchCamera.thirdPersonCamera.transform;

            if (switchCamera.firstPersonCamera != null && switchCamera.firstPersonCamera.activeInHierarchy)
                return switchCamera.firstPersonCamera.transform;
        }

        return Camera.main != null ? Camera.main.transform : null;
    }

    private void Awake()
    {
        EnsureActionsConfigured();
    }

    private void OnEnable()
    {
        moveAction?.Enable();
        jumpAction?.Enable();
        shootAction?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        jumpAction?.Disable();
        shootAction?.Disable();
    }

    private void EnsureActionsConfigured()
    {
        if (moveAction == null || moveAction.bindings.Count == 0)
        {
            moveAction = new InputAction("Move", InputActionType.Value);

            // Keyboard WASD / arrows
            var composite = moveAction.AddCompositeBinding("2DVector");
            composite.With("Up", "<Keyboard>/w");
            composite.With("Down", "<Keyboard>/s");
            composite.With("Left", "<Keyboard>/a");
            composite.With("Right", "<Keyboard>/d");

            composite = moveAction.AddCompositeBinding("2DVector");
            composite.With("Up", "<Keyboard>/upArrow");
            composite.With("Down", "<Keyboard>/downArrow");
            composite.With("Left", "<Keyboard>/leftArrow");
            composite.With("Right", "<Keyboard>/rightArrow");

            moveAction.AddBinding("<Gamepad>/leftStick");
        }

        if (jumpAction == null || jumpAction.bindings.Count == 0)
        {
            jumpAction = new InputAction("Jump", InputActionType.Button);
            jumpAction.AddBinding("<Keyboard>/space");
            jumpAction.AddBinding("<Gamepad>/buttonSouth");
        }

        if (shootAction == null || shootAction.bindings.Count == 0)
        {
            shootAction = new InputAction("Shoot", InputActionType.Button);
            shootAction.AddBinding("<Mouse>/leftButton");
        }
    }

    void Update()
    {
        if (currentlyInteracting)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame)
            {
                currentlyInteracting = false;
                switchCamera.ExitKitchenCamera();
            }
            return;
        }

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        Vector2 moveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        float x = moveInput.x;
        float z = moveInput.y;

        Transform reference = GetMovementReference();
        Vector3 referenceForward = reference != null ? reference.forward : transform.forward;
        Vector3 referenceRight = reference != null ? reference.right : transform.right;
        referenceForward.y = 0f;
        referenceRight.y = 0f;
        referenceForward = referenceForward.sqrMagnitude > 0.0001f ? referenceForward.normalized : transform.forward;
        referenceRight = referenceRight.sqrMagnitude > 0.0001f ? referenceRight.normalized : transform.right;

        Vector3 move = (referenceRight * x) + (referenceForward * z);
        move = Vector3.ClampMagnitude(move, 1f);
        Vector3 moveVelocity = move * speed;
        controller.Move(moveVelocity * Time.deltaTime);

        if (jumpAction != null && jumpAction.WasPressedThisFrame() && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * 2f * -gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            Ray r = new Ray(InteractorSource.position, InteractorSource.forward);
            if (Physics.Raycast(r, out RaycastHit hit, InteractDistance))
            {
                Debug.Log("Hit " + hit.collider.gameObject.name);
                IInteractable interactable = null;

                // 1) Exact collider object
                hit.collider.gameObject.TryGetComponent(out interactable);

                // 2) Parent chain (common when collider is on a child)
                if (interactable == null)
                    interactable = hit.collider.GetComponentInParent<IInteractable>();

                // 3) Rigidbody root (common for compound colliders)
                if (interactable == null && hit.rigidbody != null)
                    interactable = hit.rigidbody.GetComponentInParent<IInteractable>();

                if (interactable != null)
                    currentlyInteracting = interactable.Interact();
                Debug.Log("Interactable: " + interactable);
            }
        }

        // Add a shoot cooldown later
        if (shootAction != null && shootAction.IsPressed())
        {
            Debug.Log("player shoot");
            Potato_Shooter potatoShooter = GetComponentInChildren<Potato_Shooter>();
            if (potatoShooter != null)
            {
                potatoShooter.Shoot();
            }
        }
    }
}