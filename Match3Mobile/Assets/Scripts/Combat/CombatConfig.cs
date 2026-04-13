using UnityEngine;

/// <summary>
/// Central tuning asset for all combat formula constants.
/// Create one instance: Assets/ScriptableObjects/CombatConfig.asset
/// </summary>
[CreateAssetMenu(fileName = "CombatConfig", menuName = "Matchmancer/Combat Config")]
public class CombatConfig : ScriptableObject
{
    [Header("Base Values")]
    public float baseTileValue       = 10f;
    public float baseEnergyValue     = 15f;
    public float baseDefenseValue    = 8f;

    [Header("Match Size Multipliers")]
    public float match3Multiplier    = 1.0f;
    public float match4Multiplier    = 1.4f;
    public float match5PlusMultiplier = 2.0f;

    [Header("Combo")]
    [Tooltip("Multiplier added per cascade chain step. e.g. 0.1 = +10% per chain.")]
    public float comboMultiplierStep = 0.1f;
    public float maxComboMultiplier  = 3.0f;

    [Header("Critical Hits")]
    public float baseCritChance      = 0.05f;   // 5%
    public float critDamageMultiplier = 1.5f;

    [Header("Luck")]
    [Tooltip("Each point of luck adds this to crit chance.")]
    public float luckToCritRate      = 0.005f;  // 0.5% per luck point

    [Header("Defense")]
    [Tooltip("Flat damage reduction from defense tile matches.")]
    public float defensePerTile      = 8f;

    [Header("Break")]
    [Tooltip("Armor/barrier damage per break tile in a match.")]
    public float armorDamagePerTile  = 12f;

    [Header("Debuff")]
    public float poisonDamagePerTurn = 5f;
    public int   poisonDuration      = 3;       // turns
    public float vulnerabilityMult   = 1.25f;   // incoming damage boost
}
