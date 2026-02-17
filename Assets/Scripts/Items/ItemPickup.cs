using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    //[Header("Aiming")]
    [Tooltip("Script should be placed on the item's Prefab.")]
    [SerializeField] public ItemData itemData;

    private void Awake()
    {
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        GetComponent<Collider>().isTrigger = true;
    }

    public void Pickup(Inventory inventory)
    {
        inventory.AddItem(itemData);
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == null)
            return;

        var inventory = other.GetComponentInParent<Inventory>();
        if (inventory == null)
            return;

        Pickup(inventory);
    }
}