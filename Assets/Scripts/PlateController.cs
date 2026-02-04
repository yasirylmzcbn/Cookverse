using Cookverse.Assets.Scripts;
using UnityEngine;

public class PlateController : MonoBehaviour, IDualAnchorIngredientSlot
{
    [Header("Placement")]
    [Tooltip("Where the protein ingredient snaps to (create an empty child and assign it).")]
    public Transform proteinAnchor;
    [Tooltip("Where the vegetable ingredient snaps to (create an empty child and assign it).")]
    public Transform vegetableAnchor;
    [Tooltip("How close an ingredient must be (to the anchor) to snap.")]
    public float snapRange = 0.8f;

    public Transform ProteinAnchor => proteinAnchor;
    public Transform VegetableAnchor => vegetableAnchor;
    public float SnapRange => snapRange;

    public Transform GetProteinAnchor() => proteinAnchor != null ? proteinAnchor : transform;
    public Transform GetVegetableAnchor() => vegetableAnchor != null ? vegetableAnchor : transform;

    public bool CanAcceptIngredient(KitchenIngredientController ingredient)
    {
        if (ingredient == null) return false;

        bool isProtein = ingredient.IsProteinIngredient;
        bool isVegetable = ingredient.IsVegetableIngredient;

        if (isProtein && !HasProteinIngredient()) return true;
        if (isVegetable && !HasVegetableIngredient()) return true;

        return false;
    }

    private bool HasProteinIngredient()
    {
        foreach (Transform child in proteinAnchor)
        {
            if (child.GetComponent<KitchenIngredientController>() != null)
                return true;
        }
        return false;
    }

    private bool HasVegetableIngredient()
    {
        foreach (Transform child in vegetableAnchor)
        {
            if (child.GetComponent<KitchenIngredientController>() != null)
                return true;
        }
        return false;
    }

    public float DistanceToAnchor(Vector3 worldPos, KitchenIngredientController ingredient)
    {
        if (ingredient.IsProteinIngredient)
        {
            return Vector3.Distance(worldPos, proteinAnchor.position);
        }
        else if (ingredient.IsVegetableIngredient)
        {
            return Vector3.Distance(worldPos, vegetableAnchor.position);
        }
        else
        {
            throw new System.InvalidOperationException("Ingredient is neither protein nor vegetable.");
        }
    }

    public bool IsWithinSnapRange(Vector3 ingredientWorldPos)
    {
        float proteinDistance = Vector3.Distance(ingredientWorldPos, proteinAnchor.position);
        float vegetableDistance = Vector3.Distance(ingredientWorldPos, vegetableAnchor.position);
        return proteinDistance <= snapRange || vegetableDistance <= snapRange;
    }

    public bool TryPlaceIngredient(KitchenIngredientController ingredient)
    {
        Debug.Log("Trying to place ingredient on plate..." + ingredient.name);
        if (ingredient == null) return false;

        bool isProtein = ingredient.IsProteinIngredient;
        bool isVegetable = ingredient.IsVegetableIngredient;

        if (isProtein && !HasProteinIngredient())
        {
            ingredient.transform.SetParent(proteinAnchor);
            ingredient.transform.localPosition = Vector3.zero;
            ingredient.transform.localRotation = Quaternion.identity;
            return true;
        }
        else if (isVegetable && !HasVegetableIngredient())
        {
            ingredient.transform.SetParent(vegetableAnchor);
            ingredient.transform.localPosition = Vector3.zero;
            ingredient.transform.localRotation = Quaternion.identity;
            return true;
        }

        return false;
    }
}
