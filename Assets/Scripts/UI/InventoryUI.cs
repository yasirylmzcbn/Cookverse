using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public Transform content;
    public GameObject itemPrefab;
    private bool isRefreshing = false;
    private Hotbar _hotbar;
    [SerializeField] private TextMeshProUGUI feedbackText;

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

        if (content == null)
            ResolveContent();

        if (content == null)
        {
            isRefreshing = false;
            return;
        }

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
            itemui.BindInventoryUI(this);

            // ── Icon ──────────────────────────────────────────────────────────
            if (itemui.icon != null && item.icon != null)
                itemui.icon.sprite = item.icon;
        }

        Canvas.ForceUpdateCanvases();
        RectTransform rect = content.GetComponent<RectTransform>();
        if (rect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        isRefreshing = false;
    }

    private void ResolveContent()
    {
        var all = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (var t in all)
        {
            if (t != null && t.CompareTag("InventoryContent"))
            {
                content = t;
                return;
            }
        }

        content = null;
    }

    public bool TryQuickEquipToHotbar(ItemData item, int amount)
    {
        if (item == null) return false;

        if (_hotbar == null)
            _hotbar = FindFirstObjectByType<Hotbar>(FindObjectsInactive.Include);

        if (_hotbar == null)
        {
            SetFeedback("No hotbar found.");
            return false;
        }

        bool equipped = _hotbar.TryEquipFirstEmpty(item, amount);
        if (!equipped)
            SetFeedback("Hotbar is full. Drag an item to replace a slot.");

        return equipped;
    }

    private void SetFeedback(string message)
    {
        if (feedbackText != null)
            feedbackText.text = message;
    }
}