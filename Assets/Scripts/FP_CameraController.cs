using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float sensX = 50f;
    public float sensY = 37f;

    [Header("Input (New Input System)")]
    [SerializeField] private InputAction lookAction;
    [SerializeField] private InputAction escapeAction;

    private float xRotation = 0f;

    public Transform playerBody;

    [Header("UI")]
    [SerializeField] private SpellMenuUI spellMenu;

    private void Awake()
    {
        EnsureActionsConfigured();
        if (spellMenu == null)
            spellMenu = FindFirstObjectByType<SpellMenuUI>();
    }

    private void OnEnable()
    {
        lookAction?.Enable();
        escapeAction?.Enable();
    }

    private void OnDisable()
    {
        lookAction?.Disable();
        escapeAction?.Disable();
    }

    private void EnsureActionsConfigured()
    {
        if (lookAction == null || lookAction.bindings.Count == 0)
        {
            lookAction = new InputAction("Look", InputActionType.Value);
            lookAction.AddBinding("<Mouse>/delta");
            lookAction.AddBinding("<Gamepad>/rightStick");
        }

        if (escapeAction == null || escapeAction.bindings.Count == 0)
        {
            escapeAction = new InputAction("Escape", InputActionType.Button);
            escapeAction.AddBinding("<Keyboard>/escape");
            escapeAction.AddBinding("<Gamepad>/start");
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        bool menuOpen = spellMenu != null && spellMenu.menuOpen;

        // Escape closes the menu
        if (menuOpen && escapeAction != null && escapeAction.WasPressedThisFrame())
        {
            spellMenu.SetMenuVisible(false);
            return;
        }

        // Block camera look while menu is open
        if (menuOpen) return;

        Vector2 lookInput = lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero;
        float mouseX = lookInput.x * sensX * Time.deltaTime;
        float mouseY = lookInput.y * sensY * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}