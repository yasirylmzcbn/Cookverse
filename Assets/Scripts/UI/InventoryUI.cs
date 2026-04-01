using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public Transform content;
    public GameObject itemPrefab;
    private bool isRefreshing = false;

    void Start()
    {
        isRefreshing = false;
        GameManager.Instance.OnInventoryChanged += RefreshUI;
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (isRefreshing) return;
        isRefreshing = true;
        content = GameObject.FindWithTag("InventoryContent")?.transform;
        Debug.Log(GameObject.FindWithTag("InventoryContent"));

        for (int i = content.childCount - 1; i >= 0; i--)
        {
            if (content.GetChild(i).gameObject != null) Destroy(content.GetChild(i).gameObject);
        }

        Debug.Log(GameManager.Instance?.GetInventoryItemsAsString());
        foreach (var (item, amount) in GameManager.Instance?.GetItems())
        {
            GameObject obj = Instantiate(itemPrefab, content);
            Debug.Log(obj);
            Debug.Log(obj == null);
            InventoryItemUI itemui = obj.GetComponent<InventoryItemUI>();
            itemui.itemName.text = item.itemName;
            itemui.itemAmount.text = "x" + amount.ToString();
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
        isRefreshing = false;
    }
}