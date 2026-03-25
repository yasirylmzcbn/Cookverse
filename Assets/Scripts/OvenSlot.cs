using UnityEngine;

/// <summary>
/// Oven variant of CookwareSlot.
///
/// Key differences from the base CookwareSlot
/// ────────────────────────────────────────────
///   • Two ingredient anchors on a single tray (ingredientAnchor inherited +
///     ingredientAnchor2 added here).
///   • Door open/close animation mirrors the pot-lid pattern: the door opens
///     whenever any ingredient is dragged into snap range of either anchor, and
///     closes when every dragged ingredient is dropped or moved away.
///   • Oven lights are toggled externally by OvenKnobController via SetLights().
///   • IsOn / cooking logic is inherited from CookwareSlot for both anchors.
///
/// Inspector setup
/// ───────────────
///   Ingredient Anchor   - first snap point  (empty child GameObject)
///   Ingredient Anchor 2 - second snap point (empty child GameObject)
///   Door Animator       - Animator that has the door open/close clips
///   Door Open Trigger   - Animator trigger parameter  (default "Open")
///   Door Close Trigger  - Animator trigger parameter  (default "Close")
///   Lights Animator     - Animator that has the light on/off clips
///                         (can be the same Animator as the door)
///   Lights Bool Param   - Animator bool parameter     (default "isOn")
/// </summary>
public class OvenSlot : CookwareSlot
{
    // ── Second anchor ────────────────────────────────────────────────────────

    [Header("Oven - Second Anchor")]
    [Tooltip("Second ingredient snap point on the oven tray.")]
    public Transform ingredientAnchor2;

    private KitchenIngredientController _ingredient2;
    public KitchenIngredientController CurrentIngredient2 => _ingredient2;

    // ── Door animation ───────────────────────────────────────────────────────

    [Header("Oven - Door Animation")]
    [Tooltip("Animator that controls the oven door.")]
    [SerializeField] private Animator doorAnimator;

    [Tooltip("Trigger name that opens the oven door.")]
    [SerializeField] private string doorOpenTrigger = "Open";

    [Tooltip("Trigger name that closes the oven door.")]
    [SerializeField] private string doorCloseTrigger = "Close";

    [Tooltip("Reset the opposite trigger before firing the new one (prevents queued clips).")]
    [SerializeField] private bool resetOppositeDoorTrigger = true;

    // Counter of active drags currently inside snap range.
    // Using a counter (not a bool) lets two simultaneous drags both work correctly.
    private int _dragsInRange;
    private bool _doorIsOpen;

    // ── Lights ───────────────────────────────────────────────────────────────

    [Header("Oven - Lights")]
    [Tooltip("Animator that controls the two oven indicator lights. " +
             "Can be the same Animator as the door, or a separate one.")]
    [SerializeField] private Animator lightsAnimator;

    [Tooltip("Bool parameter name in the lights Animator (true = lights on).")]
    [SerializeField] private string lightsBoolParam = "isOn";

    // ── Cook rate ────────────────────────────────────────────────────────────
    // cookRatePerSecond is private in the base class, so we shadow it here
    // with the same default so the oven tray cooks at the same speed.

    [Header("Oven - Cook Rate Override")]
    [Tooltip("Cook rate for the second anchor. " +
             "Set to match the base cookRatePerSecond (default 0.2) " +
             "or customise per-oven.")]
    [SerializeField] private float ovenCookRatePerSecond = 0.2f;

    // ─────────────────────────────────────────────────────────────────────────
    // Second-anchor helpers
    // ─────────────────────────────────────────────────────────────────────────

    public bool HasIngredient2() => _ingredient2 != null;

    public Transform GetAnchor2() =>
        ingredientAnchor2 != null ? ingredientAnchor2 : transform;

    public float DistanceToAnchor2(Vector3 worldPos) =>
        Vector3.Distance(worldPos, GetAnchor2().position);

    public override float DistanceToAnchor(Vector3 worldPos, KitchenIngredientController ingredient)
    {
        bool anchor1Available = !HasIngredient() || CurrentIngredient == ingredient;
        bool anchor2Available = !HasIngredient2() || _ingredient2 == ingredient;

        if (anchor1Available && anchor2Available)
            return Mathf.Min(DistanceToAnchor(worldPos), DistanceToAnchor2(worldPos));

        if (anchor1Available)
            return DistanceToAnchor(worldPos);

        if (anchor2Available)
            return DistanceToAnchor2(worldPos);

        return Mathf.Min(DistanceToAnchor(worldPos), DistanceToAnchor2(worldPos));
    }

    /// <summary>True if worldPos is within snap range of either anchor.</summary>
    public bool IsWithinSnapRangeOfEither(Vector3 worldPos) =>
        IsWithinSnapRange(worldPos) ||
        DistanceToAnchor2(worldPos) <= SnapRange;

    public override bool IsWithinSnapRange(Vector3 ingredientWorldPos)
    {
        return DistanceToAnchor(ingredientWorldPos) <= SnapRange
               || DistanceToAnchor2(ingredientWorldPos) <= SnapRange;
    }

    public Transform GetPreviewAnchor(Vector3 worldPos, KitchenIngredientController ingredient)
    {
        bool anchor1Available = !HasIngredient() || CurrentIngredient == ingredient;
        bool anchor2Available = !HasIngredient2() || _ingredient2 == ingredient;

        if (anchor1Available && !anchor2Available)
            return GetAnchor();

        if (!anchor1Available && anchor2Available)
            return GetAnchor2();

        return DistanceToAnchor(worldPos) <= DistanceToAnchor2(worldPos)
            ? GetAnchor()
            : GetAnchor2();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Placement overrides
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Accepts if at least one anchor is free.</summary>
    public override bool CanAcceptIngredient(KitchenIngredientController ingredient)
    {
        if (ingredient == null) return false;
        return !HasIngredient() || !HasIngredient2();
    }

    public override bool CanRemoveIngredient()
    {
        bool anchor1Removable = CurrentIngredient != null && (!IsOn || CurrentIngredient.IsCooked());
        bool anchor2Removable = _ingredient2 != null && (!IsOn || _ingredient2.IsCooked());
        return anchor1Removable || anchor2Removable;
    }

    /// <summary>
    /// Snaps the ingredient to the nearest free anchor.
    /// If both are free, picks the closer one; otherwise fills the empty slot.
    /// </summary>
    public override bool TryPlaceIngredient(KitchenIngredientController ingredient)
    {
        if (!CanAcceptIngredient(ingredient)) return false;

        bool anchor1Free = !HasIngredient();
        bool anchor2Free = !HasIngredient2();

        bool useAnchor1;
        if (anchor1Free && anchor2Free)
        {
            float d1 = DistanceToAnchor(ingredient.transform.position);
            float d2 = DistanceToAnchor2(ingredient.transform.position);
            useAnchor1 = (d1 <= d2);
        }
        else
        {
            useAnchor1 = anchor1Free;
        }

        if (useAnchor1)
        {
            // Delegate fully to the base class (handles CurrentIngredient field,
            // SnapInto, HidePreview, and its own door-close notification).
            return base.TryPlaceIngredient(ingredient);
        }
        else
        {
            _ingredient2 = ingredient;
            ingredient.SnapInto(GetAnchor2());
            // Ingredient has been placed - treat as "dropped outside range"
            // so the door-close counter is decremented correctly.
            HidePreview();
            return true;
        }
    }

    /// <summary>Removes the ingredient from whichever anchor holds it.</summary>
    public override bool RemoveIngredient(KitchenIngredientController ingredient)
    {
        if (ingredient == null) return false;

        bool canRemoveTarget = !IsOn || ingredient.IsCooked();
        if (!canRemoveTarget) return false;

        if (_ingredient2 == ingredient)
        {
            _ingredient2 = null;
            ingredient.OnRemovedFromSlot();
            return true;
        }

        return base.RemoveIngredient(ingredient);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Update - cook both anchor slots
    // ─────────────────────────────────────────────────────────────────────────

    // NOTE: Unity does not call base.Update() automatically; we must do it
    // ourselves so slot-1 cooking still works.
    private void Update()
    {
        // Slot 1 - handled by CookwareSlot.Update (private, so we call it via
        // the "new" keyword shadowing; Unity will call this Update, not base).
        // We reproduce the base logic here to keep everything in one place.
        CookSlot1();
        CookSlot2();
    }

    // Replicates the cooking logic from CookwareSlot.Update for slot 1
    private void CookSlot1()
    {
        if (!IsOn) return;
        var ing = CurrentIngredient;
        if (ing == null || ing.IsCooked()) return;

        ing.cookLevel = Mathf.Clamp01(ing.cookLevel + ovenCookRatePerSecond * Time.deltaTime);
        if (ing.cookLevel >= 1f)
            ing.SetToCookedForm();
        if (ing.cookLevel >= 1.75f)
        {
            ing.SetToBurntForm();
        }
    }

    private void CookSlot2()
    {
        if (!IsOn) return;
        if (_ingredient2 == null || _ingredient2.IsCooked()) return;

        _ingredient2.cookLevel = Mathf.Clamp01(
            _ingredient2.cookLevel + ovenCookRatePerSecond * Time.deltaTime);

        if (_ingredient2.cookLevel >= 1f)
            _ingredient2.SetToCookedForm();
    }

    public new void NotifyDragInSnapRange()
    {
        _dragsInRange++;
        if (_doorIsOpen) return;
        OpenDoor();
    }

    public void NotifyDragOutOfSnapRange()
    {
        _dragsInRange = Mathf.Max(0, _dragsInRange - 1);
        if (_dragsInRange > 0 || !_doorIsOpen) return;
        CloseDoor();
    }

    private void OpenDoor()
    {
        if (doorAnimator == null) return;
        if (resetOppositeDoorTrigger && !string.IsNullOrWhiteSpace(doorCloseTrigger))
            doorAnimator.ResetTrigger(doorCloseTrigger);
        if (!string.IsNullOrWhiteSpace(doorOpenTrigger))
            doorAnimator.SetTrigger(doorOpenTrigger);
        _doorIsOpen = true;
    }

    private void CloseDoor()
    {
        if (doorAnimator == null) return;
        if (resetOppositeDoorTrigger && !string.IsNullOrWhiteSpace(doorOpenTrigger))
            doorAnimator.ResetTrigger(doorOpenTrigger);
        if (!string.IsNullOrWhiteSpace(doorCloseTrigger))
            doorAnimator.SetTrigger(doorCloseTrigger);
        _doorIsOpen = false;
    }

    public void SetLights(bool on)
    {
        if (lightsAnimator == null) return;
        lightsAnimator.SetBool(lightsBoolParam, on);
    }
    private new void OnDisable()
    {
        base.OnDisable(); // clears base preview + triggers base close-notification
        _dragsInRange = 0;
        CloseDoor();
    }
}