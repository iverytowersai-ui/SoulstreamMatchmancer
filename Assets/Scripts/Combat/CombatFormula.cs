using System;

namespace Matchmancer.Combat
{
    /// <summary>
    /// Stateless combat math. All formulas live here, nowhere else.
    /// Pure C# — no UnityEngine — so tests can call it with a seeded
    /// <see cref="System.Random"/> for deterministic crit outcomes.
    ///
    /// Canonical damage formula (from Matchmancer master spec):
    ///
    ///   FinalDamage = BaseTileValue × MatchSize
    ///               × MatchSizeMultiplier
    ///               × CharacterAttackModifier
    ///               × ComboMultiplier
    ///               × CritMultiplier
    ///               - EnemyDefense
    /// </summary>
    public static class CombatFormula
    {
        #region Damage / Energy / Defense / Break

        /// <summary>
        /// Calculate final damage from a Damage (SoulstreamShard) match.
        /// Returns a tuple so callers know whether the hit crit.
        /// </summary>
        /// <param name="tuning">Tuning constants.</param>
        /// <param name="matchSize">Number of tiles in the match (≥3).</param>
        /// <param name="characterAttack">Character attack stat. 10 = 1.0× modifier.</param>
        /// <param name="comboCount">Current cascade depth (0 on first wave).</param>
        /// <param name="luck">Character luck stat — raises crit chance.</param>
        /// <param name="enemyDefense">Flat damage reduction applied at the end.</param>
        /// <param name="rng">Random source — inject a seeded <c>System.Random</c> in tests.</param>
        public static DamageResult CalculateDamage(
            CombatTuning tuning,
            int          matchSize,
            float        characterAttack,
            int          comboCount,
            float        luck,
            float        enemyDefense,
            Random       rng)
        {
            if (tuning == null) throw new ArgumentNullException(nameof(tuning));
            if (rng    == null) throw new ArgumentNullException(nameof(rng));
            if (matchSize < 3)  return new DamageResult(0f, false);

            float baseValue = tuning.BaseTileValue * matchSize;
            float sizeMult  = GetMatchSizeMultiplier(tuning, matchSize);
            float charMod   = characterAttack / 10f;
            float comboMult = GetComboMultiplier(tuning, comboCount);

            bool  isCrit    = RollCrit(tuning, luck, rng);
            float critMult  = isCrit ? tuning.CritDamageMultiplier : 1f;

            float raw   = baseValue * sizeMult * charMod * comboMult * critMult;
            float final = Math.Max(0f, raw - enemyDefense);

            return new DamageResult(final, isCrit);
        }

        /// <summary>
        /// Energy gained from a Port Rune match.
        /// </summary>
        public static float CalculateEnergy(CombatTuning tuning, int matchSize)
        {
            if (matchSize < 3) return 0f;
            return tuning.BaseEnergyValue * matchSize * GetMatchSizeMultiplier(tuning, matchSize);
        }

        /// <summary>
        /// Shield generated from a Coven Seal match.
        /// </summary>
        public static float CalculateDefense(CombatTuning tuning, int matchSize)
        {
            if (matchSize < 3) return 0f;
            return tuning.DefensePerTile * matchSize;
        }

        /// <summary>
        /// Armor damage dealt from a Witchbreed Thorn match.
        /// </summary>
        public static float CalculateArmorDamage(CombatTuning tuning, int matchSize)
        {
            if (matchSize < 3) return 0f;
            return tuning.ArmorDamagePerTile * matchSize;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Returns the match-size multiplier (1.0× / 1.4× / 2.0×).
        /// </summary>
        public static float GetMatchSizeMultiplier(CombatTuning tuning, int matchSize)
        {
            if (matchSize >= 5) return tuning.Match5PlusMultiplier;
            if (matchSize == 4) return tuning.Match4Multiplier;
            return tuning.Match3Multiplier;
        }

        /// <summary>
        /// Returns the combo multiplier for the given cascade depth, clamped
        /// to <see cref="CombatTuning.MaxComboMultiplier"/>.
        /// </summary>
        public static float GetComboMultiplier(CombatTuning tuning, int comboCount)
        {
            float raw = 1f + comboCount * tuning.ComboMultiplierStep;
            return raw > tuning.MaxComboMultiplier ? tuning.MaxComboMultiplier : raw;
        }

        /// <summary>
        /// Final crit chance (clamped 0–1) for the given luck stat.
        /// </summary>
        public static float GetCritChance(CombatTuning tuning, float luck)
        {
            float chance = tuning.BaseCritChance + luck * tuning.LuckToCritRate;
            if (chance < 0f) return 0f;
            if (chance > 1f) return 1f;
            return chance;
        }

        /// <summary>
        /// Rolls a crit against <paramref name="rng"/>. Test-friendly — inject a seeded Random.
        /// </summary>
        public static bool RollCrit(CombatTuning tuning, float luck, Random rng)
        {
            return rng.NextDouble() < GetCritChance(tuning, luck);
        }

        #endregion

        #region Result Types

        /// <summary>
        /// Return value from <see cref="CalculateDamage"/>.
        /// </summary>
        public readonly struct DamageResult
        {
            public readonly float Amount;
            public readonly bool  IsCrit;

            public DamageResult(float amount, bool isCrit)
            {
                Amount = amount;
                IsCrit = isCrit;
            }
        }

        #endregion
    }
}
