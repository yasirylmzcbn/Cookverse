using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public Transform content;
    public GameObject itemPrefab;
    private GameManager gameManager;
    private bool isRefreshing = false;

    void Start()
    {
        GameManager gameManager = FindAnyObjectByType<GameManager>();
        gameManager.OnInventoryChanged += RefreshUI;
        RefreshUI();
    }

    void RefreshUI()
    {
        if (isRefreshing) return;
        isRefreshing = true;

        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        foreach (var (item, amount) in GameManager.Instance.GetItems())
        {
            GameObject obj = Instantiate(itemPrefab, content);
            InventoryItemUI itemui = obj.GetComponent<InventoryItemUI>();
            itemui.itemName.text = item.itemName;
            itemui.itemAmount.text = "x" + amount.ToString();
        }

        isRefreshing = false;
    }
}