using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CookwareSlot : MonoBehaviour
{
    [Header("State")]
    [Tooltip("If true, ingredient cannot be removed.")]
    [SerializeField] private bool isOn = false;

    public bool IsOn
    {
        get => isOn;
        set => isOn = value;
    }

    [Header("Placement")]
    [Tooltip("Where the ingredient snaps to (create an empty child and assign it).")]
    public Transform ingredientAnchor;

    [Tooltip("How close an ingredient must be (to the anchor) to snap.")]
    public float snapRange = 0.8f;

    [Header("Preview (while dragging)")]
    [Tooltip("Show a ghost preview of the ingredient at the anchor when in snap range.")]
    public bool showPreview = true;

    [Range(0.05f, 1f)]
    public float previewAlpha = 0.35f;

    [Header("Cooking")]
    [Tooltip("How fast cookLevel increases per second while this slot is On and has an ingredient.")]
    [SerializeField] private float cookRatePerSecond = 0.2f;

    private KitchenIngredientController currentIngredient;

    public KitchenIngredientController CurrentIngredient => currentIngredient;

    private GameObject _previewInstance;
    private KitchenIngredientController _previewSource;

    private void Update()
    {
        if (!isOn) return;
        if (currentIngredient == null) return;
        if (currentIngredient.IsCooked()) return;

        currentIngredient.cookLevel = Mathf.Clamp01(currentIngredient.cookLevel + cookRatePerSecond * Time.deltaTime);

        if (currentIngredient.cookLevel >= 1)
        {
            currentIngredient.SetToCookedForm();
        }
    }

    public bool HasIngredient() => currentIngredient != null;

    public bool CanRemoveIngredient() => !IsOn || currentIngredient.IsCooked();

    public Transform GetAnchor() => ingredientAnchor != null ? ingredientAnchor : transform;

    public bool CanAcceptIngredient(KitchenIngredientController ingredient)
    {
        if (ingredient == null) return false;
        if (HasIngredient()) return false;
        return true;
    }

    public float DistanceToAnchor(Vector3 worldPos)
    {
        return Vector3.Distance(worldPos, GetAnchor().position);
    }

    public bool IsWithinSnapRange(Vector3 ingredientWorldPos)
    {
        return DistanceToAnchor(ingredientWorldPos) <= snapRange;
    }

    public bool TryPlaceIngredient(KitchenIngredientController ingredient)
    {
        if (!CanAcceptIngredient(ingredient)) return false;

        currentIngredient = ingredient;

        ingredient.SnapInto(GetAnchor());

        HidePreview();
        return true;
    }

    public bool RemoveIngredient(KitchenIngredientController ingredient)
    {
        Debug.Log($"RemoveIngredient called on slot {gameObject.name}, isOn={isOn}");
        if (ingredient == null) return false;
        if (currentIngredient != ingredient) return false;
        if (!CanRemoveIngredient()) return false;

        currentIngredient = null;
        ingredient.OnRemovedFromSlot();
        return true;
    }

    public void ShowPreviewFor(KitchenIngredientController ingredient)
    {
        if (!showPreview) return;
        if (ingredient == null) return;
        if (HasIngredient()) { HidePreview(); return; }

        // Rebuild preview if source changed
        if (_previewInstance == null || _previewSource != ingredient)
        {
            HidePreview();
            _previewSource = ingredient;
        }
    }

    public void HidePreview()
    {
        if (_previewInstance != null)
        {
            Destroy(_previewInstance);
            _previewInstance = null;
        }
        _previewSource = null;
    }

    private void OnDisable()
    {
        HidePreview();
    }
}