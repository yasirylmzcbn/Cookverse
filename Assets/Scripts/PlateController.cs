using Cookverse.Assets.Scripts;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlateController : IngredientSlotBehaviour, IDualAnchorIngredientSlot
{
    [Header("Audio")]
    [SerializeField] private AudioClip placeOnPlateSfx;
    [SerializeField, Range(0f, 1f)] private float placeOnPlateSfxVolume = 1f;
    private AudioSource _audioSource;

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
        RebindReferences();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        EnsureAudioSource();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RebindReferences();
    }

    private void RebindReferences()
    {
        if (PlayerRecipeUnlocks.Instance != null)
            playerRecipeUnlocks = PlayerRecipeUnlocks.Instance;
        else if (playerRecipeUnlocks == null)
            playerRecipeUnlocks = FindFirstObjectByType<PlayerRecipeUnlocks>();

        if (PlayerController.Instance != null)
            playerController = PlayerController.Instance;
        else if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();
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
            PlayOneShot(placeOnPlateSfx, placeOnPlateSfxVolume);
            IsRecipeComplete();
            return true;
        }
        else if (isVegetable && !HasVegetableIngredient())
        {
            vegetableIngredient = ingredient;
            ingredient.SnapInto(GetVegetableAnchor());
            PlayOneShot(placeOnPlateSfx, placeOnPlateSfxVolume);
            IsRecipeComplete();
            return true;
        }

        return false;
    }

    public override bool RemoveIngredient(KitchenIngredientController ingredient)
    {
        if (ingredient == null) return false;


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
        RebindReferences();

        if (proteinIngredient == null || vegetableIngredient == null) return false;
        bool hasRequiredProtein = requiredIngredients.Exists(ing => proteinIngredient != null && ing == proteinIngredient.IngredientType);
        bool hasRequiredVegetable = requiredIngredients.Exists(ing => vegetableIngredient != null && ing == vegetableIngredient.IngredientType);
        bool complete = hasRequiredProtein && hasRequiredVegetable;
        Debug.Log("req:" + hasRequiredProtein + " veg:" + hasRequiredVegetable);
        if (complete && !_unlockedFired)
        {
            _unlockedFired = true;
            Debug.Log("Recipe complete: " + recipe);

            bool unlockedNow = false;

            if (playerRecipeUnlocks != null)
                unlockedNow = playerRecipeUnlocks.Unlock(recipe);
            else
                Debug.LogWarning($"No PlayerRecipeUnlocks found. Recipe '{recipe}' won't be saved as unlocked.");

            if (autoEquipUnlockedSpell && recipeSpellDatabase != null && playerController != null)
            {
                var spell = recipeSpellDatabase.GetSpellOrNull(recipe);
            }

            if (completionText != null && unlockedNow)
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

    private void EnsureAudioSource()
    {
        if (_audioSource != null)
            return;

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();

        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 1f;
    }

    private void PlayOneShot(AudioClip clip, float volume)
    {
        if (clip == null)
            return;

        EnsureAudioSource();
        if (_audioSource != null)
            _audioSource.PlayOneShot(clip, volume);
    }
}

