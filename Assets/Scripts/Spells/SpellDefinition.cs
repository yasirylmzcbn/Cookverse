using UnityEngine;

public readonly struct SpellCastContext
{
    public readonly PlayerController player;
    public readonly Transform origin;
    public readonly Vector3 forwardFlat;

    public SpellCastContext(PlayerController player, Transform origin, Vector3 forwardFlat)
    {
        this.player = player;
        this.origin = origin;
        this.forwardFlat = forwardFlat;
    }
}

public abstract class SpellDefinition : ScriptableObject
{
    [Header("UI")]
    public string displayName = "Spell";
    public Sprite icon;

    [Header("Unlock")]
    [Tooltip("If enabled, the player must unlock the recipe below before this spell can be cast.")]
    public bool requiresRecipeUnlock;

    public Recipe requiredRecipe;

    [Header("Cooldown")]
    [Min(0f)]
    public float cooldownSeconds = 10f;

    public virtual bool CanCast(in SpellCastContext context) => true;

    public abstract void Cast(in SpellCastContext context);
}
