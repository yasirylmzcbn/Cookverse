using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

interface IInteractable
{
    public bool Interact();
}

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    public CharacterController controller;
    public float speed = 10;
    public float gravity = -9.81f * 2;
    public float jumpHeight = 3f;

    public int maxHealth = 100;
    public int currentHealth;

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
    [SerializeField] private InputAction reloadAction;

    [Header("Spell Input (New Input System)")]
    [SerializeField] private InputAction spell1Action;
    [SerializeField] private InputAction spell2Action;
    [SerializeField] private InputAction spell3Action;
    [SerializeField] private InputAction spell4Action;

    [Header("Spells")]
    [Tooltip("4-slot spell loadout. Create spells via Assets > Create > Cookverse > Spells and assign them here.")]
    [SerializeField] private SpellDefinition[] spellLoadout = new SpellDefinition[4];
    [Tooltip("Optional database used to auto-equip unlocked spells after scene loads.")]
    [SerializeField] private RecipeSpellDatabase recipeSpellDatabase;
    [Tooltip("If true, equips any unlocked recipe spells when a scene loads.")]
    [SerializeField] private bool autoEquipUnlockedSpellsOnSceneLoad = true;

    [Tooltip("Optional cast origin for spells (e.g., hand or staff tip). If null, uses InteractorSource then transform.")]
    [SerializeField] private Transform spellCastOrigin;

    private readonly float[] _nextSpellTimes = new float[4];

    Vector3 velocity;
    bool isGrounded;

    public Transform InteractorSource;
    public float InteractDistance = 3f;

    bool currentlyInteracting = false;

    SwitchCamera switchCamera;

    private float _moveSpeedMultiplier = 1f;
    private Coroutine _buffCoroutine;
    private bool _buffActive;
    private float _buffPrevMoveMultiplier = 1f;
    private float _buffPrevShootCooldownDuration;
    private bool _buffHadShooter;

    private Potato_Shooter _potatoShooter;
    private float _baseShootCooldownDuration;
    private bool _cachedShooterBase;

    private PlayerRecipeUnlocks _recipeUnlocks;

    private void ResolveRecipeUnlocksIfNeeded()
    {
        if (_recipeUnlocks != null) return;

        // Prefer same object.
        if (TryGetComponent(out PlayerRecipeUnlocks self))
            _recipeUnlocks = self;

        // Common prefab setups put different scripts on parent/child objects.
        if (_recipeUnlocks == null)
            _recipeUnlocks = GetComponentInParent<PlayerRecipeUnlocks>(true);

        if (_recipeUnlocks == null)
            _recipeUnlocks = GetComponentInChildren<PlayerRecipeUnlocks>(true);

        // Fallbacks.
        if (_recipeUnlocks == null)
            _recipeUnlocks = PlayerRecipeUnlocks.Instance;

        if (_recipeUnlocks == null)
            _recipeUnlocks = FindFirstObjectByType<PlayerRecipeUnlocks>();
    }

    public bool IsSpellOnCooldown(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _nextSpellTimes.Length) return false;
        return Time.time < _nextSpellTimes[slotIndex];
    }

    public SpellDefinition GetSpell(int slotIndex)
    {
        if (spellLoadout == null) return null;
        if (slotIndex < 0 || slotIndex >= spellLoadout.Length) return null;
        return spellLoadout[slotIndex];
    }

    public bool IsRecipeUnlocked(Recipe recipe)
    {
        ResolveRecipeUnlocksIfNeeded();
        return _recipeUnlocks != null && _recipeUnlocks.IsUnlocked(recipe);
    }

    public bool IsSpellUnlocked(int slotIndex)
    {
        SpellDefinition spell = GetSpell(slotIndex);
        if (spell == null) return false;
        if (!spell.requiresRecipeUnlock) return true;
        return IsRecipeUnlocked(spell.requiredRecipe);
    }

    public bool TryEquipSpell(SpellDefinition spell, int slotIndex = -1)
    {
        if (spell == null) return false;
        if (spellLoadout == null || spellLoadout.Length != 4)
            spellLoadout = new SpellDefinition[4];

        if (slotIndex >= 0 && slotIndex < spellLoadout.Length)
        {
            spellLoadout[slotIndex] = spell;
            return true;
        }

        for (int i = 0; i < spellLoadout.Length; i++)
        {
            if (spellLoadout[i] == null)
            {
                spellLoadout[i] = spell;
                return true;
            }
        }

        return false;
    }

    void Start()
    {
        switchCamera = FindFirstObjectByType<SwitchCamera>();
        currentHealth = maxHealth;
        ResolveRecipeUnlocksIfNeeded();
        AutoEquipUnlockedSpells();
    }

    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        Debug.Log(currentHealth + "/" + maxHealth);
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        Debug.Log(currentHealth + "/" + maxHealth);
    }

    public void ApplyTimedBuff(float moveSpeedMultiplier, float shootCooldownMultiplier, float durationSeconds)
    {
        ClearActiveBuff(stopCoroutine: true);
        _buffCoroutine = StartCoroutine(BuffRoutine(moveSpeedMultiplier, shootCooldownMultiplier, durationSeconds));
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
        // Ensure only one PlayerController exists across scene loads.
        // This prevents double-input (controlling 2 players) and fixes UI/spells binding to the wrong player.
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Duplicate PlayerController detected. Keeping '{Instance.gameObject.name}', destroying '{gameObject.name}'.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureActionsConfigured();
        _potatoShooter = GetComponentInChildren<Potato_Shooter>(true);
        CacheShooterBaseIfNeeded();
        ResolveRecipeUnlocksIfNeeded();

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Scene cameras and other refs live per-scene.
        switchCamera = FindFirstObjectByType<SwitchCamera>();
        ResolveRecipeUnlocksIfNeeded();
        _potatoShooter = GetComponentInChildren<Potato_Shooter>(true);
        CacheShooterBaseIfNeeded();
        AutoEquipUnlockedSpells();
    }

    private void OnEnable()
    {
        moveAction?.Enable();
        jumpAction?.Enable();
        shootAction?.Enable();
        reloadAction?.Enable();

        spell1Action?.Enable();
        spell2Action?.Enable();
        spell3Action?.Enable();
        spell4Action?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        jumpAction?.Disable();
        shootAction?.Disable();
        reloadAction?.Disable();

        spell1Action?.Disable();
        spell2Action?.Disable();
        spell3Action?.Disable();
        spell4Action?.Disable();
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
        if (reloadAction == null || reloadAction.bindings.Count == 0)
        {
            reloadAction = new InputAction("Shoot", InputActionType.Button);
            reloadAction.AddBinding("<Keyboard>/r");
        }

        // Spells (4 slots). Defaults support both keyboard and gamepad at the same time.
        if (spell1Action == null || spell1Action.bindings.Count == 0)
        {
            spell1Action = new InputAction("Spell1", InputActionType.Button);
            spell1Action.AddBinding("<Keyboard>/1");
            spell1Action.AddBinding("<Keyboard>/numpad1");
            spell1Action.AddBinding("<Gamepad>/dpad/up");
        }
        if (spell2Action == null || spell2Action.bindings.Count == 0)
        {
            spell2Action = new InputAction("Spell2", InputActionType.Button);
            spell2Action.AddBinding("<Keyboard>/2");
            spell2Action.AddBinding("<Keyboard>/numpad2");
            spell2Action.AddBinding("<Gamepad>/dpad/right");
        }
        if (spell3Action == null || spell3Action.bindings.Count == 0)
        {
            spell3Action = new InputAction("Spell3", InputActionType.Button);
            spell3Action.AddBinding("<Keyboard>/3");
            spell3Action.AddBinding("<Keyboard>/numpad3");
            spell3Action.AddBinding("<Gamepad>/dpad/down");
        }
        if (spell4Action == null || spell4Action.bindings.Count == 0)
        {
            spell4Action = new InputAction("Spell4", InputActionType.Button);
            spell4Action.AddBinding("<Keyboard>/4");
            spell4Action.AddBinding("<Keyboard>/numpad4");
            spell4Action.AddBinding("<Gamepad>/dpad/left");
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
        Vector3 moveVelocity = move * (speed * _moveSpeedMultiplier);
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

        if (spell1Action != null && spell1Action.WasPressedThisFrame()) TryCastSpell(0);
        if (spell2Action != null && spell2Action.WasPressedThisFrame()) TryCastSpell(1);
        if (spell3Action != null && spell3Action.WasPressedThisFrame()) TryCastSpell(2);
        if (spell4Action != null && spell4Action.WasPressedThisFrame()) TryCastSpell(3);

        // Add a shoot cooldown later
        if (shootAction != null && shootAction.IsPressed())
        {
            var potatoShooter = GetPotatoShooter();
            if (potatoShooter != null) potatoShooter.Shoot();
        }

        if (reloadAction != null && reloadAction.IsPressed())
        {
            Debug.Log("reload pressed");
            var potatoShooter = GetPotatoShooter();
            if (potatoShooter != null) potatoShooter.TryReload();
        }
    }
    private bool IsSpellEquipped(SpellDefinition spell)
    {
        if (spell == null || spellLoadout == null) return false;
        for (int i = 0; i < spellLoadout.Length; i++)
        {
            if (spellLoadout[i] == spell)
                return true;
        }

        return false;
    }

    private void AutoEquipUnlockedSpells()
    {
        if (!autoEquipUnlockedSpellsOnSceneLoad) return;
        if (recipeSpellDatabase == null) return;

        var entries = recipeSpellDatabase.GetEntries();
        if (entries == null) return;

        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            if (entry.spell == null) continue;
            if (!IsRecipeUnlocked(entry.recipe)) continue;
            if (IsSpellEquipped(entry.spell)) continue;

            TryEquipSpell(entry.spell);
        }
    }

    private void TryCastSpell(int slotIndex)
    {
        SpellDefinition spell = GetSpell(slotIndex);
        if (spell == null)
        {
            Debug.LogWarning($"No spell equipped in slot {slotIndex}.");
            return;
        }

        bool unlocked = IsSpellUnlocked(slotIndex);
        if (!unlocked)
        {
            Debug.LogWarning($"Spell '{spell.displayName}' in slot {slotIndex} is locked. RequiresRecipe={spell.requiresRecipeUnlock}, RequiredRecipe={spell.requiredRecipe}, HasUnlock={IsRecipeUnlocked(spell.requiredRecipe)}");
            return;
        }
        Debug.LogWarning($"{spell.displayName} is unlocked on slot " + slotIndex);
        if (IsSpellOnCooldown(slotIndex)) return;

        Transform origin = spellCastOrigin != null
            ? spellCastOrigin
            : (InteractorSource != null ? InteractorSource : transform);

        Vector3 forward = origin != null ? origin.forward : transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f)
            forward = transform.forward;
        forward.Normalize();

        var context = new SpellCastContext(this, origin, forward);
        if (!spell.CanCast(in context)) return;

        spell.Cast(in context);
        float cd = Mathf.Max(0f, spell.cooldownSeconds);
        _nextSpellTimes[slotIndex] = Time.time + cd;
    }

    private Potato_Shooter GetPotatoShooter()
    {
        if (_potatoShooter == null)
            _potatoShooter = GetComponentInChildren<Potato_Shooter>(true);
        CacheShooterBaseIfNeeded();
        return _potatoShooter;
    }

    private void CacheShooterBaseIfNeeded()
    {
        if (_cachedShooterBase) return;
        if (_potatoShooter == null) return;
        _baseShootCooldownDuration = _potatoShooter.shootCooldownDuration;
        _cachedShooterBase = true;
    }

    private IEnumerator BuffRoutine(float moveSpeedMultiplier, float shootCooldownMultiplier, float durationSeconds)
    {
        moveSpeedMultiplier = Mathf.Max(0f, moveSpeedMultiplier);
        shootCooldownMultiplier = Mathf.Max(0f, shootCooldownMultiplier);

        _buffPrevMoveMultiplier = _moveSpeedMultiplier;
        _moveSpeedMultiplier = _buffPrevMoveMultiplier * moveSpeedMultiplier;

        var shooter = GetPotatoShooter();
        _buffHadShooter = shooter != null;
        if (_buffHadShooter)
        {
            CacheShooterBaseIfNeeded();
            _buffPrevShootCooldownDuration = shooter.shootCooldownDuration;
            shooter.shootCooldownDuration = _baseShootCooldownDuration * shootCooldownMultiplier;
        }

        _buffActive = true;

        yield return new WaitForSeconds(durationSeconds);

        ClearActiveBuff(stopCoroutine: false);
    }

    private void ClearActiveBuff(bool stopCoroutine)
    {
        if (stopCoroutine && _buffCoroutine != null)
        {
            StopCoroutine(_buffCoroutine);
            _buffCoroutine = null;
        }

        if (!_buffActive) return;

        _moveSpeedMultiplier = _buffPrevMoveMultiplier;

        if (_buffHadShooter)
        {
            var shooter = GetPotatoShooter();
            if (shooter != null)
                shooter.shootCooldownDuration = _buffPrevShootCooldownDuration;
        }

        _buffActive = false;
        _buffHadShooter = false;
        _buffPrevMoveMultiplier = 1f;
        _buffPrevShootCooldownDuration = 0f;
        _buffCoroutine = null;
    }
}