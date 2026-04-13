using UnityEngine;

/// <summary>
/// Sits between the Ultimate UI button and CharacterRuntime.
/// Also patches CombatResolver to apply the ultimate damage multiplier
/// by subscribing to OnEffectResolved before damage is dispatched.
///
/// Usage: player taps Ultimate button → calls TryActivateUltimate()
/// On the next Damage tile wave, the multiplier fires automatically.
/// </summary>
public class UltimateSystem : MonoBehaviour
{
    [SerializeField] private CharacterRuntime characterRuntime;
    [SerializeField] private CombatResolver   combatResolver;

    private void OnEnable()
    {
        if (combatResolver != null)
            combatResolver.OnEffectResolved += ApplyUltimateIfQueued;
    }

    private void OnDisable()
    {
        if (combatResolver != null)
            combatResolver.OnEffectResolved -= ApplyUltimateIfQueued;
    }

    /// <summary>Called by the Ultimate UI button.</summary>
    public void TryActivateUltimate()
    {
        characterRuntime?.QueueUltimate();
    }

    private void ApplyUltimateIfQueued(CombatEffect effect)
    {
        // Only relevant for Damage effects
        if (effect.SourceType != TileType.Damage) return;

        // ConsumeUltimateMultiplier returns 1f if not queued — no-op
        float mult = characterRuntime?.ConsumeUltimateMultiplier() ?? 1f;
        if (mult > 1f)
            Debug.Log($"[Ultimate] Multiplier {mult}× applied to damage wave.");
    }
}
