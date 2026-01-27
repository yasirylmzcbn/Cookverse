using UnityEngine;
using UnityEngine.InputSystem;

public class DraggableObject : MonoBehaviour, IMoveableItem
{
    public Camera stoveCam;
    [SerializeField] private Rigidbody rb; // Optional: auto-filled from this GameObject if left empty
    private bool isDragging = false;
    private float zDistance;
    private Vector3 offset;
    private float lockedY;

    private void Awake()
    {
        // Makes rb optional in the Inspector.
        if (rb == null)
        {
            TryGetComponent(out rb);
        }
    }

    void Update()
    {
        // Check for mouse click
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryPickup();
        }

        // Drag the object
        if (isDragging)
        {
            Vector3 newPosition = GetMouseWorldPosition() + offset;
            newPosition.y = lockedY;
            transform.position = newPosition;
        }

        // Check for mouse release
        if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging)
        {
            isDragging = false;
            OnDrop();
        }
    }

    private void TryPickup()
    {
        Ray ray = stoveCam.ScreenPointToRay(Mouse.current.position.ReadValue());
        Debug.Log("Trying to pick up object with raycast.");
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log($"Hit object: {hit.collider.gameObject.name}");
            if (hit.collider.gameObject == gameObject)
            {
                Debug.Log("Picked up the object.");
                lockedY = transform.position.y;
                zDistance = stoveCam.WorldToScreenPoint(transform.position).z;
                offset = transform.position - GetMouseWorldPosition();
                isDragging = true;
                OnPickup();
            }
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePoint = Mouse.current.position.ReadValue();
        mousePoint.z = zDistance;
        return stoveCam.ScreenToWorldPoint(mousePoint);
    }

    public void OnPickup()
    {
        // Optional: Add visual feedback
        // if (rb != null) rb.isKinematic = true;
    }

    public void OnDrop()
    {
        // Optional: Re-enable physics
        // if (rb != null) rb.isKinematic = false;
    }
}