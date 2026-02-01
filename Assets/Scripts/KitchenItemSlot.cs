using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class KitchenItemSlot : MonoBehaviour
{
    [Header("State")]
    [Tooltip("If true, ingredient cannot be removed.")]
    public bool isOn = false;

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

    private KitchenIngredientController currentIngredient;

    private GameObject _previewInstance;
    private KitchenIngredientController _previewSource;

    public bool HasIngredient() => currentIngredient != null;

    public bool CanRemoveIngredient() => !isOn;

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
            _previewInstance = BuildPreviewInstance(ingredient);
        }

        if (_previewInstance == null) return;

        Transform anchor = GetAnchor();
        _previewInstance.transform.SetParent(anchor, true);
        _previewInstance.transform.localPosition = Vector3.zero;
        _previewInstance.transform.localRotation = Quaternion.identity;
        _previewInstance.SetActive(true);
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

    private GameObject BuildPreviewInstance(KitchenIngredientController ingredient)
    {
        // Clone the ingredient visuals (same model) as a "ghost"
        Transform visualsRoot = ingredient.visualsRoot != null ? ingredient.visualsRoot : ingredient.transform;

        GameObject clone = Instantiate(visualsRoot.gameObject);
        clone.name = $"{visualsRoot.gameObject.name}_Preview";
        clone.layer = LayerMask.NameToLayer("Ignore Raycast");

        // Strip physics + scripts/colliders so it's purely visual
        foreach (var c in clone.GetComponentsInChildren<Collider>(true)) Destroy(c);
        foreach (var r in clone.GetComponentsInChildren<Rigidbody>(true)) Destroy(r);
        foreach (var mb in clone.GetComponentsInChildren<MonoBehaviour>(true)) Destroy(mb);
        foreach (var a in clone.GetComponentsInChildren<AudioSource>(true)) Destroy(a);

        // Make it look like a preview (no shadows + alpha via property block when possible)
        var renderers = clone.GetComponentsInChildren<Renderer>(true);
        var mpb = new MaterialPropertyBlock();

        foreach (var rend in renderers)
        {
            rend.shadowCastingMode = ShadowCastingMode.Off;
            rend.receiveShadows = false;

            // Try set alpha without instantiating materials
            mpb.Clear();

            var mat = rend.sharedMaterial;
            if (mat != null)
            {
                if (mat.HasProperty("_BaseColor"))
                {
                    Color c0 = mat.GetColor("_BaseColor");
                    c0.a = previewAlpha;
                    mpb.SetColor("_BaseColor", c0);
                    rend.SetPropertyBlock(mpb);
                }
                else if (mat.HasProperty("_Color"))
                {
                    Color c0 = mat.GetColor("_Color");
                    c0.a = previewAlpha;
                    mpb.SetColor("_Color", c0);
                    rend.SetPropertyBlock(mpb);
                }
            }
        }

        return clone;
    }
    private void OnDisable()
    {
        HidePreview();
    }
}