using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class EquippedDiamondUI : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("0=Up, 1=Right, 2=Down, 3=Left")]
    public int slotIndex;

    [SerializeField] private Image iconImage;
    [SerializeField] private Image diamondBackground;
    [SerializeField] private TextMeshProUGUI slotLabel;
    [SerializeField] private Color emptyColor = new Color(0.15f, 0.15f, 0.2f, 0.8f);
    [SerializeField] private Color filledColor = new Color(0.3f, 0.6f, 0.9f, 1f);
    [SerializeField] private Color selectedColor = new Color(0.9f, 0.7f, 0.1f, 1f);

    private SpellMenuUI _menu;

    public void Init(SpellMenuUI menu)
    {
        _menu = menu;
    }

    public void Refresh(SpellDefinition spell, bool isSelected)
    {
        bool filled = spell != null;
        if (diamondBackground != null)
            diamondBackground.color = isSelected ? selectedColor : (filled ? filledColor : emptyColor);
        if (iconImage != null)
        {
            iconImage.enabled = filled && spell.icon != null;
            if (filled && spell.icon != null)
                iconImage.sprite = spell.icon;
        }
        if (slotLabel != null)
            slotLabel.text = filled ? spell.displayName : GetDirectionLabel(slotIndex);
    }

    public void OnPointerClick(PointerEventData e)
    {
        Debug.Log($"Diamond slot {slotIndex} clicked");
        _menu.OnDiamondClicked(slotIndex);
    }

    private string GetDirectionLabel(int idx) => idx switch
    {
        0 => "↑",
        1 => "→",
        2 => "↓",
        3 => "←",
        _ => "?"
    };
}