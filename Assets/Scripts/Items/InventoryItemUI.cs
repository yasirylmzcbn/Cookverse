using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventoryItemUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerClickHandler
{
    [Header("Display")]
    public Image icon;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemAmount;

    // Set by InventoryUI.RefreshUI()
    [HideInInspector] public ItemData itemData;
    [HideInInspector] public int amount;

    // ── Drag state ────────────────────────────────────────────────────────────
    private static GameObject _dragGhost;
    private Canvas _rootCanvas;
    private InventoryUI _inventoryUI;

    private Canvas RootCanvas
    {
        get
        {
            if (_rootCanvas == null)
                _rootCanvas = GetComponentInParent<Canvas>();
            return _rootCanvas;
        }
    }

    // ── IBeginDragHandler ────────────────────────────────────────────────────
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"[InventoryItemUI] OnBeginDrag — itemData={(itemData != null ? itemData.itemName : "NULL")}, amount={amount}");

        if (itemData == null)
        {
            Debug.LogError("[InventoryItemUI] itemData is null — drag cancelled. Make sure InventoryUI sets itemui.itemData.");
            return;
        }

        if (RootCanvas == null)
        {
            Debug.LogError("[InventoryItemUI] Could not find a parent Canvas — drag cancelled.");
            return;
        }

        // Destroy any leftover ghost from an interrupted drag
        if (_dragGhost != null) Destroy(_dragGhost);

        _dragGhost = new GameObject("InventoryDragGhost");
        _dragGhost.transform.SetParent(RootCanvas.transform, false);
        _dragGhost.transform.SetAsLastSibling();

        Image ghostImg = _dragGhost.AddComponent<Image>();
        ghostImg.sprite = (icon != null) ? icon.sprite : null;
        ghostImg.color = new Color(1f, 1f, 1f, 0.75f);
        ghostImg.raycastTarget = false;     // MUST be false — let events pass through to slots

        // If there's no sprite, make it a bright yellow square so you can see it
        if (ghostImg.sprite == null)
        {
            ghostImg.color = new Color(1f, 1f, 0f, 0.75f);
            Debug.LogWarning("[InventoryItemUI] No sprite on icon — ghost will appear as a yellow square.");
        }

        RectTransform rt = _dragGhost.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(60f, 60f);

        // Put DragDropItem on THIS object — Unity sets pointerDrag to the object
        // that received OnBeginDrag, so HotbarSlot.OnDrop reads it from here,
        // not from the ghost.
        DragDropItem payload = gameObject.GetComponent<DragDropItem>();
        if (payload == null) payload = gameObject.AddComponent<DragDropItem>();
        payload.itemData = itemData;
        payload.amount = amount;
        payload.sourceSlot = null;   // came from inventory

        Debug.Log($"[InventoryItemUI] Ghost created. Canvas={RootCanvas.name}, sprite={(ghostImg.sprite != null ? ghostImg.sprite.name : "none")}");

        MoveDragGhost(eventData);
    }

    // ── IDragHandler ─────────────────────────────────────────────────────────
    public void OnDrag(PointerEventData eventData)
    {
        MoveDragGhost(eventData);
    }

    // ── IEndDragHandler ──────────────────────────────────────────────────────
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"[InventoryItemUI] OnEndDrag — pointerEnter={(eventData.pointerEnter != null ? eventData.pointerEnter.name : "null")}");

        if (_dragGhost != null)
        {
            Destroy(_dragGhost);
            _dragGhost = null;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (itemData == null) return;

        _inventoryUI?.TryQuickEquipToHotbar(itemData, amount);
    }

    public void BindInventoryUI(InventoryUI inventoryUI)
    {
        _inventoryUI = inventoryUI;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private void MoveDragGhost(PointerEventData eventData)
    {
        if (_dragGhost == null) return;

        RectTransform canvasRT = RootCanvas.GetComponent<RectTransform>();
        Camera cam = RootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : eventData.pressEventCamera;

        bool ok = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRT, eventData.position, cam, out Vector2 localPoint);

        if (ok)
            _dragGhost.GetComponent<RectTransform>().localPosition = localPoint;
    }
}