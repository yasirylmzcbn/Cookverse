using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SpellSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public SpellDefinition Spell { get; private set; }

    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.3f, 0.9f);
    [SerializeField] private Color hoveredColor = new Color(0.4f, 0.4f, 0.6f, 1f);
    [SerializeField] private Color equippedColor = new Color(0.2f, 0.5f, 0.3f, 1f);

    private SpellMenuUI _menu;

    public void Init(SpellDefinition spell, SpellMenuUI menu)
    {
        Spell = spell;
        _menu = menu;
        if (iconImage != null && spell.icon != null)
            iconImage.sprite = spell.icon;
        if (backgroundImage != null)
            backgroundImage.color = normalColor;
    }

    public void SetEquippedVisual(bool equipped)
    {
        if (backgroundImage != null)
            backgroundImage.color = equipped ? equippedColor : normalColor;
    }

    public void OnPointerEnter(PointerEventData e)
    {
        _menu.OnSpellHovered(Spell);
        if (backgroundImage != null) backgroundImage.color = hoveredColor;
    }

    public void OnPointerExit(PointerEventData e)
    {
        _menu.OnSpellHovered(null);
        SetEquippedVisual(_menu.IsEquipped(Spell));
    }

    public void OnPointerClick(PointerEventData e)
    {
        _menu.OnSpellClicked(Spell);
    }
}