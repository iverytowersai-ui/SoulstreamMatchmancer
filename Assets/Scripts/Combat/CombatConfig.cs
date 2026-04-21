using UnityEngine;

namespace Matchmancer.Combat
{
    /// <summary>
    /// Unity-facing tuning asset for all combat formula constants.
    /// Create once: right-click in Project → Create → Matchmancer → Combat Config,
    /// save as <c>Assets/ScriptableObjects/CombatConfig.asset</c>.
    ///
    /// Call <see cref="ToTuning"/> to convert to a pure-C# <see cref="CombatTuning"/>
    /// that <see cref="CombatFormula"/> consumes. This keeps the math layer
    /// testable without Unity.
    /// </summary>
    [CreateAssetMenu(
        fileName = "CombatConfig",
        menuName = "Matchmancer/Combat Config",
        order    = 0)]
    public class CombatConfig : ScriptableObject
    {
        [Header("Base values")]
        [Tooltip("Base damage contribution per tile in a match.")]
        public float baseTileValue = 10f;

        [Tooltip("Energy (ultimate meter) gain per tile in an Energy match.")]
        public float baseEnergyValue = 15f;

        [Tooltip("Shield granted per tile in a Defense match.")]
        public float baseDefenseValue = 8f;

        [Header("Match size multipliers")]
        public float match3Multiplier     = 1.0f;
        public float match4Multiplier     = 1.4f;
        public float match5PlusMultiplier = 2.0f;

        [Header("Combo")]
        [Tooltip("Multiplier added per cascade chain step. 0.1 = +10% per chain.")]
        public float comboMultiplierStep = 0.1f;

        [Tooltip("Hard cap on the combo multiplier.")]
        public float maxComboMultiplier = 3.0f;

        [Header("Critical hits")]
        [Range(0f, 1f)] public float baseCritChance = 0.05f;
        public float critDamageMultiplier = 1.5f;

        [Header("Luck")]
        [Tooltip("Crit chance added per point of Luck. 0.005 = 0.5% per point.")]
        public float luckToCritRate = 0.005f;

        [Header("Defense tile")]
        public float defensePerTile = 8f;

        [Header("Break tile")]
        public float armorDamagePerTile = 12f;

        [Header("Debuff tile")]
        public float poisonDamagePerTurn = 5f;
        public int   poisonDuration      = 3;
        public float vulnerabilityMult   = 1.25f;

        [Header("Fairness (distinctiveness guideline #2)")]
        [Tooltip("Enemy final crit chance is multiplied by this. 0.33 = enemy crits ~1/3 " +
                 "as often as the player, making the player feel lucky ~3× more often. " +
                 "Set 1.0 for symmetric combat, 0.0 to disable enemy crits entirely.")]
        [Range(0f, 1f)] public float enemyCritMultiplier = 0.33f;

        /// <summary>
        /// Project every Inspector field into a pure-C# <see cref="CombatTuning"/>
        /// so the formula layer never has to reference UnityEngine.
        /// </summary>
        public CombatTuning ToTuning()
        {
            return new CombatTuning
            {
                BaseTileValue        = baseTileValue,
                BaseEnergyValue      = baseEnergyValue,
                BaseDefenseValue     = baseDefenseValue,

                Match3Multiplier     = match3Multiplier,
                Match4Multiplier     = match4Multiplier,
                Match5PlusMultiplier = match5PlusMultiplier,

                ComboMultiplierStep  = comboMultiplierStep,
                MaxComboMultiplier   = maxComboMultiplier,

                BaseCritChance       = baseCritChance,
                CritDamageMultiplier = critDamageMultiplier,

                LuckToCritRate       = luckToCritRate,

                DefensePerTile       = defensePerTile,
                ArmorDamagePerTile   = armorDamagePerTile,

                PoisonDamagePerTurn  = poisonDamagePerTurn,
                PoisonDuration       = poisonDuration,
                VulnerabilityMult    = vulnerabilityMult,

                EnemyCritMultiplier  = enemyCritMultiplier,
            };
        }
    }
}
