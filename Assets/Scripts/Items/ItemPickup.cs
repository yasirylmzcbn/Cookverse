using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    //[Header("Aiming")]
    [Tooltip("Script should be placed on the item's Prefab.")]
    [SerializeField] public ItemData itemData;

    private void Awake()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        // GetComponent<Collider>().isTrigger = true;
    }

    public void Pickup(GameManager gameManager)
    {
        gameManager.AddInventoryItem(itemData);
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == null)
            return;
        if (!other.CompareTag("Pickup"))
            return;

        GameManager gameManager = FindAnyObjectByType<GameManager>();
        Pickup(gameManager);
    }
}