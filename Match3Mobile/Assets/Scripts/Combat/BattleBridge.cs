using UnityEngine;

/// <summary>
/// Temporary listener that logs combat effects to the Console.
/// Replace this with real EnemyController + CharacterRuntime wiring in Skills 13–14.
/// Safe to leave in the scene — it becomes a no-op once real listeners are attached.
/// </summary>
public class BattleBridge : MonoBehaviour
{
    [SerializeField] private CombatResolver combatResolver;

    private void OnEnable()
    {
        if (combatResolver != null)
            combatResolver.OnEffectResolved += LogEffect;
    }

    private void OnDisable()
    {
        if (combatResolver != null)
            combatResolver.OnEffectResolved -= LogEffect;
    }

    private void LogEffect(CombatEffect e)
    {
        switch (e.SourceType)
        {
            case TileType.Damage:
                Debug.Log($"[Combat] DAMAGE {e.DamageDealt:F1}" +
                          $"{(e.IsCriticalHit ? " CRIT!" : "")} " +
                          $"(match {e.MatchSize}, combo {e.ComboCount})");
                break;
            case TileType.Energy:
                Debug.Log($"[Combat] ENERGY +{e.EnergyGenerated:F1}");
                break;
            case TileType.Defense:
                Debug.Log($"[Combat] SHIELD +{e.ShieldGenerated:F1}");
                break;
            case TileType.Break:
                Debug.Log($"[Combat] ARMOR DMG {e.ArmorDamageDealt:F1}");
                break;
            case TileType.Debuff:
                Debug.Log($"[Combat] DEBUFF — poison:{e.AppliesPoison} " +
                          $"vuln:{e.AppliesVulnerability} dur:{e.DebuffDuration}");
                break;
            case TileType.Luck:
                Debug.Log($"[Combat] LUCK — crit+{e.LuckCritBonus:P1} " +
                          $"combo+{e.LuckComboBonus:P1}");
                break;
        }
    }
}
