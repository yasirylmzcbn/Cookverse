using System.Collections.Generic;
using Cookverse.Assets.Scripts;
using UnityEngine;
using UnityEngine.Rendering;

public enum CookwareType
{
    None,
    Pot,
    Pan,
    Fryer,
    Oven,
    Other
}

public class CookwareSlot : IngredientSlotBehaviour, ISingleAnchorIngredientSlot
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

    [Header("Lid Animation")]
    [Tooltip("Animator controlling the pot lid (assign in Inspector).")]
    [SerializeField] private Animator lidAnimator;

    [Tooltip("Trigger name to play the lid opening animation.")]
    [SerializeField] private string lidOpenTrigger = "Open";

    [Tooltip("Trigger name to play the lid closing animation.")]
    [SerializeField] private string lidCloseTrigger = "Close";

    [Tooltip("If true, resets the opposite trigger before setting the new one.")]
    [SerializeField] private bool resetOppositeTrigger = true;

    private bool _lidIsOpen;

    private KitchenIngredientController currentIngredient;

    public KitchenIngredientController CurrentIngredient => currentIngredient;

    private GameObject _previewInstance;
    private KitchenIngredientController _previewSource;

    private void Update()
    {
        if (!isOn) return;
        if (currentIngredient == null) return;

        currentIngredient.cookLevel = currentIngredient.cookLevel + cookRatePerSecond * Time.deltaTime;
        if (currentIngredient.IsCooked())
        {
            currentIngredient.SetToCookedForm();
        }
        if (currentIngredient.IsBurnt())
        {
            currentIngredient.SetToBurntForm();
        }
    }


    public bool HasIngredient() => currentIngredient != null;

    public override bool CanRemoveIngredient() => !IsOn || (currentIngredient != null && currentIngredient.IsCooked());
    public override float SnapRange => snapRange;
    public Transform IngredientAnchor => ingredientAnchor;
    public Transform GetAnchor() => IngredientAnchor != null ? IngredientAnchor : transform;

    public override bool CanAcceptIngredient(KitchenIngredientController ingredient)
    {
        if (ingredient == null) return false;
        if (HasIngredient()) return false;
        return true;
    }

    public float DistanceToAnchor(Vector3 worldPos)
    {
        return Vector3.Distance(worldPos, GetAnchor().position);
    }

    public override float DistanceToAnchor(Vector3 worldPos, KitchenIngredientController ingredient)
    {
        return DistanceToAnchor(worldPos);
    }

    public override bool IsWithinSnapRange(Vector3 ingredientWorldPos)
    {
        return DistanceToAnchor(ingredientWorldPos) <= snapRange;
    }

    public override bool TryPlaceIngredient(KitchenIngredientController ingredient)
    {
        if (!CanAcceptIngredient(ingredient)) return false;

        currentIngredient = ingredient;

        ingredient.SnapInto(GetAnchor());

        NotifyDragOutOfSnapRangeOrDropped();

        HidePreview();
        return true;
    }

    public override bool RemoveIngredient(KitchenIngredientController ingredient)
    {
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

    protected void OnDisable()
    {
        HidePreview();
        NotifyDragOutOfSnapRangeOrDropped();
    }

    public void NotifyDragInSnapRange()
    {
        if (lidAnimator == null) return;
        if (_lidIsOpen) return;

        if (resetOppositeTrigger && !string.IsNullOrWhiteSpace(lidCloseTrigger))
            lidAnimator.ResetTrigger(lidCloseTrigger);

        if (!string.IsNullOrWhiteSpace(lidOpenTrigger))
            lidAnimator.SetTrigger(lidOpenTrigger);

        _lidIsOpen = true;
    }

    public void NotifyDragOutOfSnapRangeOrDropped()
    {
        if (lidAnimator == null) return;
        if (!_lidIsOpen) return;

        if (resetOppositeTrigger && !string.IsNullOrWhiteSpace(lidOpenTrigger))
            lidAnimator.ResetTrigger(lidOpenTrigger);

        if (!string.IsNullOrWhiteSpace(lidCloseTrigger))
            lidAnimator.SetTrigger(lidCloseTrigger);

        _lidIsOpen = false;
    }
}