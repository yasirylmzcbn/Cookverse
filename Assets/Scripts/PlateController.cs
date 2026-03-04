using Cookverse.Assets.Scripts;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PlateController : IngredientSlotBehaviour, IDualAnchorIngredientSlot
{
    [Header("Recipe")]
    [SerializeField] public Recipe recipe;
    private List<Ingredient> requiredIngredients;

    [Header("Placement")]
    [Tooltip("Where the protein ingredient snaps to (create an empty child and assign it).")]
    public Transform proteinAnchor;
    [Tooltip("Where the vegetable ingredient snaps to (create an empty child and assign it).")]
    public Transform vegetableAnchor;
    [Tooltip("How close an ingredient must be (to the anchor) to snap.")]
    public float snapRange = 0.8f;

    private KitchenIngredientController proteinIngredient;
    private KitchenIngredientController vegetableIngredient;

    [Header("Recipe Completion TMP")]
    [SerializeField] public TextMeshProUGUI completionText;

    [Header("Unlock + Spell Grant")]
    [Tooltip("Optional override; if left empty, this Plate will find the player's PlayerRecipeUnlocks in the scene.")]
    [SerializeField] private PlayerRecipeUnlocks playerRecipeUnlocks;
    [Tooltip("Optional. If set and a RecipeSpellDatabase is provided, the unlocked recipe's spell can be equipped.")]
    [SerializeField] private PlayerController playerController;

    [Tooltip("Optional database to map recipes -> spells.")]
    [SerializeField] private RecipeSpellDatabase recipeSpellDatabase;

    [Tooltip("If true, when a recipe is completed its mapped spell will be equipped (if available).")]
    [SerializeField] private bool autoEquipUnlockedSpell = true;

    private bool _unlockedFired;

    public Transform ProteinAnchor => proteinAnchor;
    public Transform VegetableAnchor => vegetableAnchor;
    public override float SnapRange => snapRange;

    public Transform GetProteinAnchor() => proteinAnchor != null ? proteinAnchor : transform;
    public Transform GetVegetableAnchor() => vegetableAnchor != null ? vegetableAnchor : transform;

    private void Awake()
    {
        if (playerRecipeUnlocks == null)
            playerRecipeUnlocks = PlayerRecipeUnlocks.Instance != null
                ? PlayerRecipeUnlocks.Instance
                : FindFirstObjectByType<PlayerRecipeUnlocks>();

        if (playerController == null)
            playerController = PlayerController.Instance != null
                ? PlayerController.Instance
                : FindFirstObjectByType<PlayerController>();
    }

    public void Start()
    {
        requiredIngredients = Recipes.RecipeIngredients[recipe];
    }
    public override bool CanAcceptIngredient(KitchenIngredientController ingredient)
    {
        if (ingredient == null) return false;
        if (!ingredient.IsCooked()) return false;

        bool isProtein = ingredient.IsProteinIngredient;
        bool isVegetable = ingredient.IsVegetableIngredient;

        if (isProtein && !HasProteinIngredient()) return true;
        if (isVegetable && !HasVegetableIngredient()) return true;

        return false;
    }

    private bool HasProteinIngredient()
    {
        return proteinIngredient != null;
    }

    private bool HasVegetableIngredient()
    {
        return vegetableIngredient != null;
    }

    public override float DistanceToAnchor(Vector3 worldPos, KitchenIngredientController ingredient)
    {
        if (ingredient == null) return float.PositiveInfinity;
        Transform anchor = ingredient.IsProteinIngredient ? GetProteinAnchor() : GetVegetableAnchor();
        return Vector3.Distance(worldPos, anchor.position);
    }

    public override bool IsWithinSnapRange(Vector3 ingredientWorldPos)
    {
        float proteinDistance = Vector3.Distance(ingredientWorldPos, GetProteinAnchor().position);
        float vegetableDistance = Vector3.Distance(ingredientWorldPos, GetVegetableAnchor().position);
        return proteinDistance <= snapRange || vegetableDistance <= snapRange;
    }

    public override bool TryPlaceIngredient(KitchenIngredientController ingredient)
    {
        Debug.Log("Trying to place ingredient: " + ingredient?.name);
        if (ingredient == null) return false;
        if (!ingredient.IsCooked()) return false;

        bool isProtein = ingredient.IsProteinIngredient;
        bool isVegetable = ingredient.IsVegetableIngredient;

        if (isProtein && !HasProteinIngredient())
        {
            proteinIngredient = ingredient;
            ingredient.SnapInto(GetProteinAnchor());
            IsRecipeComplete();
            return true;
        }
        else if (isVegetable && !HasVegetableIngredient())
        {
            vegetableIngredient = ingredient;
            ingredient.SnapInto(GetVegetableAnchor());
            IsRecipeComplete();
            return true;
        }

        return false;
    }

    public override bool CanRemoveIngredient()
    {
        // Allow removing from plate (useful while testing).
        return true;
    }

    public override bool RemoveIngredient(KitchenIngredientController ingredient)
    {
        if (ingredient == null) return false;
        if (!CanRemoveIngredient()) return false;

        if (proteinIngredient == ingredient)
        {
            proteinIngredient = null;
            ingredient.OnRemovedFromSlot();
            return true;
        }

        if (vegetableIngredient == ingredient)
        {
            vegetableIngredient = null;
            ingredient.OnRemovedFromSlot();
            return true;
        }

        return false;
    }

    public bool IsRecipeComplete()
    {
        if (proteinIngredient == null || vegetableIngredient == null) return false;
        bool hasRequiredProtein = requiredIngredients.Exists(ing => proteinIngredient != null && ing == proteinIngredient.IngredientType);
        bool hasRequiredVegetable = requiredIngredients.Exists(ing => vegetableIngredient != null && ing == vegetableIngredient.IngredientType);
        bool complete = hasRequiredProtein && hasRequiredVegetable;
        Debug.Log("req:" + hasRequiredProtein + " veg:" + hasRequiredVegetable);
        if (complete && !_unlockedFired)
        {
            _unlockedFired = true;
            Debug.Log("Recipe complete: " + recipe);

            if (playerRecipeUnlocks != null)
                playerRecipeUnlocks.Unlock(recipe);
            else
                Debug.LogWarning($"No PlayerRecipeUnlocks found. Recipe '{recipe}' won't be saved as unlocked.");

            if (autoEquipUnlockedSpell && recipeSpellDatabase != null && playerController != null)
            {
                var spell = recipeSpellDatabase.GetSpellOrNull(recipe);
            }

            if (completionText != null)
            {
                completionText.text = "You unlocked the " + recipe.ToString() + " recipe!";
                StartCoroutine(HideTextCoroutine(5f));

            }
        }
        return complete;
    }

    private System.Collections.IEnumerator HideTextCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (completionText != null)
            completionText.text = "";
    }
}

