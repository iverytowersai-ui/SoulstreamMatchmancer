using UnityEngine;

/// <summary>
/// UI glue between the Ultimate button and CharacterRuntime.
/// The actual damage multiplier is consumed inside CombatResolver.ResolveDamage
/// (because CombatEffect is a struct and can't be mutated by event subscribers
/// after it's been emitted).
///
/// Usage: player taps Ultimate button → calls TryActivateUltimate()
/// On the next Damage tile wave, CombatResolver calls
/// CharacterRuntime.ConsumeUltimateMultiplier() and applies the multiplier.
/// </summary>
public class UltimateSystem : MonoBehaviour
{
    [SerializeField] private CharacterRuntime characterRuntime;

    /// <summary>Called by the Ultimate UI button.</summary>
    public void TryActivateUltimate()
    {
        characterRuntime?.QueueUltimate();
    }
}
