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
    [Header("Audio")]
    [SerializeField] private AudioClip placeIngredientSfx;
    [SerializeField, Range(0f, 1f)] private float placeIngredientSfxVolume = 1f;
    [SerializeField] protected AudioClip cookingSizzleSfx;
    [SerializeField] protected AudioClip cookingReadySfx;
    [SerializeField] protected AudioClip cookingBurntSfx;
    [SerializeField, Range(0f, 1f)] protected float cookingSfxVolume = 1f;
    private AudioSource _audioSource;
    private KitchenIngredientController _trackedCookingIngredient;
    private bool _sizzlePlayedForCurrentIngredient;
    private bool _readyPlayedForCurrentIngredient;
    private bool _burntPlayedForCurrentIngredient;
    private const string DefaultSizzlePath = "Assets/Audio/Sizzle.wav";
    private const string DefaultReadyPath = "Assets/Audio/Ready.wav";
    private const string DefaultBurntPath = "Assets/Audio/Burnt.wav";

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
        if (!isOn)
        {
            if (_trackedCookingIngredient != null)
                _sizzlePlayedForCurrentIngredient = false;
            return;
        }

        if (currentIngredient == null)
        {
            ResetCookingSfxState();
            return;
        }

        TrackCookingSfxState(currentIngredient);
        if (!_sizzlePlayedForCurrentIngredient)
        {
            PlayCookingSfx(cookingSizzleSfx);
            _sizzlePlayedForCurrentIngredient = true;
        }

        float previousCookLevel = currentIngredient.cookLevel;
        currentIngredient.cookLevel = currentIngredient.cookLevel + cookRatePerSecond * Time.deltaTime;
        if (!_readyPlayedForCurrentIngredient
            && previousCookLevel < GV.REQUIRED_COOK_LEVEL
            && currentIngredient.cookLevel >= GV.REQUIRED_COOK_LEVEL)
        {
            PlayCookingSfx(cookingReadySfx);
            _readyPlayedForCurrentIngredient = true;
        }

        if (!_burntPlayedForCurrentIngredient
            && previousCookLevel < GV.REQUIRED_BURN_LEVEL
            && currentIngredient.cookLevel >= GV.REQUIRED_BURN_LEVEL)
        {
            PlayCookingSfx(cookingBurntSfx);
            _burntPlayedForCurrentIngredient = true;
        }

        if (currentIngredient.IsCooked())
        {
            currentIngredient.SetToCookedForm();
        }
        if (currentIngredient.IsBurnt())
        {
            currentIngredient.SetToBurntForm();
        }
    }

    private void Awake()
    {
        EnsureAudioSource();
    }

#if UNITY_EDITOR
    private void Reset()
    {
        TryAssignDefaultCookingSfx();
    }

    private void OnValidate()
    {
        TryAssignDefaultCookingSfx();
    }

    private void TryAssignDefaultCookingSfx()
    {
        if (cookingSizzleSfx == null)
            cookingSizzleSfx = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(DefaultSizzlePath);

        if (cookingReadySfx == null)
            cookingReadySfx = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(DefaultReadyPath);

        if (cookingBurntSfx == null)
            cookingBurntSfx = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(DefaultBurntPath);
    }
#endif


    public bool HasIngredient() => currentIngredient != null;

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
        PlayPlaceIngredientSfx();
        return true;
    }

    public override bool RemoveIngredient(KitchenIngredientController ingredient)
    {
        if (ingredient == null) return false;
        if (currentIngredient != ingredient) return false;

        currentIngredient = null;
        ResetCookingSfxState();
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

    protected void PlayPlaceIngredientSfx()
    {
        if (placeIngredientSfx == null)
            return;

        EnsureAudioSource();
        if (_audioSource != null)
            _audioSource.PlayOneShot(placeIngredientSfx, placeIngredientSfxVolume);
    }

    private void EnsureAudioSource()
    {
        if (_audioSource != null)
            return;

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();

        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 0f;
    }

    protected void PlayCookingSfx(AudioClip clip)
    {
        if (clip == null)
            return;

        EnsureAudioSource();
        if (_audioSource != null)
            _audioSource.PlayOneShot(clip, cookingSfxVolume);
    }

    private void TrackCookingSfxState(KitchenIngredientController ingredient)
    {
        if (_trackedCookingIngredient == ingredient)
            return;

        _trackedCookingIngredient = ingredient;
        _sizzlePlayedForCurrentIngredient = false;
        _readyPlayedForCurrentIngredient = false;
        _burntPlayedForCurrentIngredient = false;
    }

    protected void ResetCookingSfxState()
    {
        _trackedCookingIngredient = null;
        _sizzlePlayedForCurrentIngredient = false;
        _readyPlayedForCurrentIngredient = false;
        _burntPlayedForCurrentIngredient = false;
    }

    protected void HandleCookingMilestoneSfx(KitchenIngredientController ingredient, float previousCookLevel)
    {
        if (ingredient == null)
            return;

        TrackCookingSfxState(ingredient);

        if (!_sizzlePlayedForCurrentIngredient)
        {
            PlayCookingSfx(cookingSizzleSfx);
            _sizzlePlayedForCurrentIngredient = true;
        }

        if (!_readyPlayedForCurrentIngredient
            && previousCookLevel < GV.REQUIRED_COOK_LEVEL
            && ingredient.cookLevel >= GV.REQUIRED_COOK_LEVEL)
        {
            PlayCookingSfx(cookingReadySfx);
            _readyPlayedForCurrentIngredient = true;
        }

        if (!_burntPlayedForCurrentIngredient
            && previousCookLevel < GV.REQUIRED_BURN_LEVEL
            && ingredient.cookLevel >= GV.REQUIRED_BURN_LEVEL)
        {
            PlayCookingSfx(cookingBurntSfx);
            _burntPlayedForCurrentIngredient = true;
        }
    }
}