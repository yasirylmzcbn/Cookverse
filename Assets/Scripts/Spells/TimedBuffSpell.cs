using UnityEngine;

[CreateAssetMenu(menuName = "Cookverse/Spells/Timed Buff Spell", fileName = "TimedBuffSpell")]
public class TimedBuffSpell : SpellDefinition
{
    [Header("Duration")]
    [Min(0.05f)]
    public float durationSeconds = 5f;

    [Header("Move Speed")]
    [Tooltip("Multiplies player move speed for the duration.")]
    [Min(0f)]
    public float moveSpeedMultiplier = 1f;

    [Header("Fire Rate")]
    [Tooltip("Multiplies Potato_Shooter.shootCooldownDuration for the duration. < 1 = faster shooting.")]
    [Min(0f)]
    public float shootCooldownMultiplier = 1f;

    public override void Cast(in SpellCastContext context)
    {
        if (context.player == null) return;
        context.player.ApplyTimedBuff(moveSpeedMultiplier, shootCooldownMultiplier, durationSeconds);
    }
}
