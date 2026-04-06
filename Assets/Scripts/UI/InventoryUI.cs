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

        var all = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (var t in all)
        {
            if (t.CompareTag("InventoryContent"))
            {
                content = t;
                break;
            }
        }

        Debug.Log(content);

        for (int i = content.childCount - 1; i >= 0; i--)
        {
            if (content.GetChild(i).gameObject != null)
                Destroy(content.GetChild(i).gameObject);
        }

        Debug.Log(GameManager.Instance?.GetInventoryItemsAsString());

        foreach (var (item, amount) in GameManager.Instance?.GetItems())
        {
            GameObject obj = Instantiate(itemPrefab, content);
            InventoryItemUI itemui = obj.GetComponent<InventoryItemUI>();

            // ── Text display ──────────────────────────────────────────────────
            itemui.itemName.text = item.itemName;
            itemui.itemAmount.text = "x" + amount.ToString();

            // ── Data needed for drag-and-drop ─────────────────────────────────
            // These MUST be set so InventoryItemUI.OnBeginDrag can read them.
            itemui.itemData = item;
            itemui.amount = amount;

            // ── Icon ──────────────────────────────────────────────────────────
            if (itemui.icon != null && item.icon != null)
                itemui.icon.sprite = item.icon;
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
        isRefreshing = false;
    }
}