using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SpellMenuUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerRecipeUnlocks playerRecipeUnlocks;
    [SerializeField] private RecipeSpellDatabase recipeSpellDatabase;

    [Header("Panels")]
    [SerializeField] private GameObject inventoryPanel;   // top half — wire up later
    [SerializeField] private GameObject spellMenuRoot;    // bottom half container

    [Header("Description Panel (left 25%)")]
    [SerializeField] private TextMeshProUGUI spellNameText;
    [SerializeField] private TextMeshProUGUI spellDescText;
    [SerializeField] private Image spellPreviewIcon;

    [Header("Spell List Panel (left-mid 25%)")]
    [SerializeField] private Transform spellListContainer;  // Vertical Layout Group parent
    [SerializeField] private GameObject spellSlotPrefab;    // prefab with SpellSlotUI

    [Header("Equipped Diamond Slots (right 25%)")]
    [SerializeField] private EquippedDiamondUI[] diamondSlots = new EquippedDiamondUI[4];
    // Assign in Inspector: 0=Up, 1=Right, 2=Down, 3=Left

    public bool menuOpen;
    private int _selectedDiamondSlot = -1;  // which diamond the player is assigning TO
    private List<SpellSlotUI> _spellSlotUIs = new();

    private void Awake()
    {
        if (playerController == null) playerController = PlayerController.Instance ?? FindFirstObjectByType<PlayerController>();
        if (playerRecipeUnlocks == null) playerRecipeUnlocks = PlayerRecipeUnlocks.Instance ?? FindFirstObjectByType<PlayerRecipeUnlocks>();
    }

    private void Start()
    {
        for (int i = 0; i < diamondSlots.Length; i++)
            diamondSlots[i]?.Init(this);

        SetMenuVisible(false);
    }

    public void SetMenuVisible(bool visible)
    {

        menuOpen = visible;
        spellMenuRoot.SetActive(visible);
        if (inventoryPanel != null)
            inventoryPanel.SetActive(visible);

        if (visible)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            RebuildSpellList();
            RefreshDiamonds();
            ClearDescription();
            inventoryPanel.GetComponent<InventoryUI>().RefreshUI();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _selectedDiamondSlot = -1;
        }
    }

    // ── Spell List ────────────────────────────────────────────

    private void RebuildSpellList()
    {
        Debug.Log("Rebuilding spell list UI");
        foreach (var ui in _spellSlotUIs)
            if (ui != null) Destroy(ui.gameObject);
        _spellSlotUIs.Clear();

        if (recipeSpellDatabase == null) return;

        var entries = recipeSpellDatabase.GetEntries();
        Debug.Log("Entries: " + entries);
        foreach (var entry in entries)
        {
            Debug.Log("Checking spell entry: " + entry.spell + " requires recipe: " + entry.recipe);
            if (entry.spell == null) continue;
            Debug.Log("Is recipe unlocked?" + (playerRecipeUnlocks != null ? playerRecipeUnlocks.IsUnlocked(entry.recipe).ToString() : "No playerRecipeUnlocks reference"));
            if (playerRecipeUnlocks == null || !playerRecipeUnlocks.IsUnlocked(entry.recipe)) continue;

            var go = Instantiate(spellSlotPrefab, spellListContainer);
            var ui = go.GetComponent<SpellSlotUI>();
            ui.Init(entry.spell, this);
            ui.SetEquippedVisual(IsEquipped(entry.spell));
            _spellSlotUIs.Add(ui);
        }
    }

    // ── Diamond Slots ─────────────────────────────────────────

    public void OnDiamondClicked(int slotIndex)
    {
        Debug.Log($"Diamond slot {slotIndex} clicked in SpellMenuUI");
        // Toggle selection — clicking a diamond means "I want to assign a spell here"
        _selectedDiamondSlot = (_selectedDiamondSlot == slotIndex) ? -1 : slotIndex;
        RefreshDiamonds();
    }

    private void RefreshDiamonds()
    {
        for (int i = 0; i < diamondSlots.Length; i++)
        {
            var spell = playerController.GetSpell(i);
            diamondSlots[i]?.Refresh(spell, i == _selectedDiamondSlot);
        }
        // Update equipped highlights on list
        foreach (var ui in _spellSlotUIs)
            ui.SetEquippedVisual(IsEquipped(ui.Spell));
    }

    // ── Spell Click (assign to selected diamond) ──────────────

    public void OnSpellClicked(SpellDefinition spell)
    {
        if (_selectedDiamondSlot < 0)
        {
            // No slot selected: auto-pick first empty slot, or do nothing
            for (int i = 0; i < 4; i++)
            {
                if (playerController.GetSpell(i) == null)
                {
                    playerController.TryEquipSpell(spell, i);
                    RefreshDiamonds();
                    return;
                }
            }
            // All slots full — show hint in description area
            if (spellDescText != null)
                spellDescText.text = "Select a diamond slot first, then click a spell to assign it.";
            return;
        }

        // If this spell is already in another slot, remove it from there first
        var loadout = playerController.GetLoadout();
        for (int i = 0; i < loadout.Length; i++)
        {
            if (loadout[i] == spell && i != _selectedDiamondSlot)
                playerController.UnequipSpell(i);
        }

        playerController.TryEquipSpell(spell, _selectedDiamondSlot);
        _selectedDiamondSlot = -1;  // deselect after assigning
        RefreshDiamonds();
    }

    // ── Hover description ─────────────────────────────────────

    public void OnSpellHovered(SpellDefinition spell)
    {
        if (spell == null) { ClearDescription(); return; }
        if (spellNameText != null) spellNameText.text = spell.displayName;
        if (spellDescText != null) spellDescText.text = spell.description;
        if (spellPreviewIcon != null)
        {
            spellPreviewIcon.enabled = spell.icon != null;
            if (spell.icon != null) spellPreviewIcon.sprite = spell.icon;
        }
    }

    private void ClearDescription()
    {
        if (spellNameText != null) spellNameText.text = "";
        if (spellDescText != null) spellDescText.text = "Hover a spell to see details";
        if (spellPreviewIcon != null) spellPreviewIcon.enabled = false;
    }

    // ── Helpers ───────────────────────────────────────────────

    public bool IsEquipped(SpellDefinition spell)
    {
        if (spell == null) return false;
        var loadout = playerController.GetLoadout();
        for (int i = 0; i < loadout.Length; i++)
            if (loadout[i] == spell) return true;
        return false;
    }
}