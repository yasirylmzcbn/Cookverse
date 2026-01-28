using UnityEngine;
using UnityEngine.InputSystem;

public class KitchenIngredientController : MonoBehaviour
{
    [Header("Ingredient Visuals")]
    [Tooltip("Optional: assign the model root to clone for preview (ghost). If null, uses this transform.")]
    public Transform visualsRoot;

    [Header("Dragging")]
    [Tooltip("Fixed kitchen camera (assign in Inspector).")]
    public Camera stoveCamera;

    [Tooltip("What layers can be interacted with (ingredients + kitchen items).")]
    public LayerMask interactLayers = ~0;

    [Header("Drag Surface")]
    public LayerMask dragSurfaceLayers = ~0;
    public float surfaceRayDistance = 1000f;
    public bool useFixedDragY = false;
    public float fixedDragY = 0.9f;
    public float hoverYOffset = 0.02f;

    [SerializeField] private Rigidbody rb;

    [Header("Snapping Assist")]
    [Tooltip("Bigger = easier to 'hit' kitchen items with the cursor (sphere cast).")]
    public float snapSphereCastRadius = 0.25f;

    [Tooltip("How far we search for nearby slots while dragging (performance).")]
    public float slotSearchRadius = 2.0f;

    private readonly Collider[] _snapOverlapBuffer = new Collider[32];

    private bool isDragging;
    private Vector3 dragOffset;
    private float lockedY;

    private KitchenItemSlot currentSlot;
    private KitchenItemSlot hoverSlot;

    // Remembers where the ingredient was last sitting freely (counter/table/etc.)
    private struct FreeState
    {
        public bool hasValue;
        public Transform parent;
        public Vector3 position;
        public Quaternion rotation;
        public bool rbKinematic;
        public bool rbUseGravity;
        public RigidbodyConstraints rbConstraints;
    }

    private FreeState freeState;

    private void Awake()
    {
        if (rb == null) TryGetComponent(out rb);
        SaveFreeState();
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
            TryBeginDrag();

        if (isDragging)
            DragMove();

        if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging)
            EndDragAndTryPlace();
    }

    private void TryBeginDrag()
    {
        if (stoveCamera == null) return;

        // If currently placed in a slot, only allow pickup if slot is NOT on.
        // On removal, restore to last free state (do NOT start dragging immediately).
        if (currentSlot != null)
        {
            if (!currentSlot.CanRemoveIngredient())
                return;

            currentSlot.RemoveIngredient(this);
            currentSlot = null;
            return;
        }

        Ray ray = stoveCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 500f, interactLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider != null && hit.collider.gameObject == gameObject)
                BeginDragInternal();
        }
    }

    private void BeginDragInternal()
    {
        isDragging = true;

        lockedY = useFixedDragY ? fixedDragY : transform.position.y;

        if (TryGetPointerWorldPoint(out Vector3 pointerWorld))
            dragOffset = transform.position - pointerWorld;
        else
            dragOffset = Vector3.zero;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        transform.SetParent(null, true);
        SetHoverSlot(null);
    }

    private void DragMove()
    {
        if (!TryGetPointerWorldPoint(out Vector3 pointerWorld))
            return;

        Vector3 newPos = pointerWorld + dragOffset;
        newPos.y = lockedY + hoverYOffset;
        transform.position = newPos;

        UpdateHoverPreview();
    }

    private void UpdateHoverPreview()
    {
        // Prefer slot under cursor (for intent), else nearest in range
        KitchenItemSlot candidate = SphereCastSlotUnderMouse();

        if (candidate == null)
            candidate = FindNearestAcceptingSlotInRange(transform.position);

        SetHoverSlot(candidate);
    }

    private void EndDragAndTryPlace()
    {
        isDragging = false;

        // Try place into hovered slot if in its snap range
        if (hoverSlot != null && hoverSlot.IsWithinSnapRange(transform.position) && hoverSlot.TryPlaceIngredient(this))
        {
            currentSlot = hoverSlot;
            SetHoverSlot(null);
            return;
        }

        // Not placed: keep where dropped
        SetHoverSlot(null);

        if (rb != null)
            rb.isKinematic = false;

        SaveFreeState();
    }

    private void SetHoverSlot(KitchenItemSlot slot)
    {
        if (hoverSlot == slot) return;

        if (hoverSlot != null)
            hoverSlot.HidePreview();

        hoverSlot = slot;

        if (hoverSlot != null && hoverSlot.IsWithinSnapRange(transform.position) && hoverSlot.CanAcceptIngredient(this))
            hoverSlot.ShowPreviewFor(this);
    }

    private KitchenItemSlot SphereCastSlotUnderMouse()
    {
        if (stoveCamera == null) return null;

        Ray ray = stoveCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.SphereCast(ray, snapSphereCastRadius, out RaycastHit hit, 500f, interactLayers, QueryTriggerInteraction.Ignore))
            return hit.collider != null ? hit.collider.GetComponentInParent<KitchenItemSlot>() : null;

        return null;
    }

    private KitchenItemSlot FindNearestAcceptingSlotInRange(Vector3 fromWorldPos)
    {
        int count = Physics.OverlapSphereNonAlloc(fromWorldPos, slotSearchRadius, _snapOverlapBuffer, interactLayers, QueryTriggerInteraction.Ignore);

        KitchenItemSlot best = null;
        float bestDist = float.PositiveInfinity;

        for (int i = 0; i < count; i++)
        {
            Collider col = _snapOverlapBuffer[i];
            if (col == null) continue;

            KitchenItemSlot slot = col.GetComponentInParent<KitchenItemSlot>();
            if (slot == null) continue;
            if (!slot.CanAcceptIngredient(this)) continue;

            float d = slot.DistanceToAnchor(fromWorldPos);
            if (d <= slot.snapRange && d < bestDist)
            {
                bestDist = d;
                best = slot;
            }
        }

        return best;
    }

    private bool TryGetPointerWorldPoint(out Vector3 world)
    {
        world = default;
        if (stoveCamera == null || Mouse.current == null) return false;

        Ray ray = stoveCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, surfaceRayDistance, dragSurfaceLayers, QueryTriggerInteraction.Ignore))
        {
            world = hit.point;
            return true;
        }

        Plane plane = new Plane(Vector3.up, new Vector3(0f, lockedY, 0f));
        if (plane.Raycast(ray, out float enter))
        {
            world = ray.GetPoint(enter);
            return true;
        }

        return false;
    }

    private void SaveFreeState()
    {
        freeState.hasValue = true;
        freeState.parent = transform.parent;
        freeState.position = transform.position;
        freeState.rotation = transform.rotation;

        if (rb != null)
        {
            freeState.rbKinematic = rb.isKinematic;
            freeState.rbUseGravity = rb.useGravity;
            freeState.rbConstraints = rb.constraints;
        }
    }

    private void RestoreFreeState()
    {
        if (!freeState.hasValue) return;

        transform.SetParent(freeState.parent, true);
        transform.position = freeState.position;
        transform.rotation = freeState.rotation;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = freeState.rbKinematic;
            rb.useGravity = freeState.rbUseGravity;
            rb.constraints = freeState.rbConstraints;
        }
    }

    public void SnapInto(Transform anchor)
    {
        SaveFreeState();

        if (anchor != null)
        {
            transform.SetParent(anchor, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    public void OnRemovedFromSlot()
    {
        RestoreFreeState();
    }
}