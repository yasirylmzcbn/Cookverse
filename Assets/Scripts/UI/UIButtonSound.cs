using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    private Selectable selectable;

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (UISoundManager.Instance != null && selectable != null && selectable.interactable)
        {
            UISoundManager.Instance.PlayHoverSound();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (UISoundManager.Instance != null && selectable != null && selectable.interactable)
        {
            UISoundManager.Instance.PlayClickSound();
        }
    }
}
