using UnityEngine;

[CreateAssetMenu(menuName = "Cookverse/Spells/Heal Spell", fileName = "HealSpell")]
public class HealSpell : SpellDefinition
{
    [Header("Heal")]
    [Min(0)]
    public int healAmount = 25;

    public override void Cast(in SpellCastContext context)
    {
        if (context.player == null) return;
        context.player.Heal(healAmount);
    }
}
