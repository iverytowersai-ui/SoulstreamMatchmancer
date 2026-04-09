using System;
using NUnit.Framework;
using Matchmancer.Combat;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class CombatFormulaTests
    {
        private CombatTuning _tuning;

        [SetUp]
        public void SetUp()
        {
            _tuning = CombatTuning.Default();
        }

        // ------------------------------------------------------------------
        // Helpers — deterministic RNG
        // ------------------------------------------------------------------

        /// <summary>RNG whose NextDouble() always returns 0.999 — crit will never trigger.</summary>
        private static Random NoCritRng() => new Random(12345); // unused when BaseCritChance=0

        /// <summary>Force a no-crit path by zeroing crit chance in tuning.</summary>
        private void ForceNoCrit()
        {
            _tuning.BaseCritChance = 0f;
            _tuning.LuckToCritRate = 0f;
        }

        /// <summary>Force a guaranteed-crit path by maxing crit chance in tuning.</summary>
        private void ForceCrit()
        {
            _tuning.BaseCritChance = 1f;
            _tuning.LuckToCritRate = 0f;
        }

        // ------------------------------------------------------------------
        // Match size multipliers
        // ------------------------------------------------------------------

        [Test]
        public void GetMatchSizeMultiplier_Match3_Returns1()
        {
            Assert.AreEqual(1.0f, CombatFormula.GetMatchSizeMultiplier(_tuning, 3), 0.0001f);
        }

        [Test]
        public void GetMatchSizeMultiplier_Match4_Returns1_4()
        {
            Assert.AreEqual(1.4f, CombatFormula.GetMatchSizeMultiplier(_tuning, 4), 0.0001f);
        }

        [Test]
        public void GetMatchSizeMultiplier_Match5Plus_Returns2()
        {
            Assert.AreEqual(2.0f, CombatFormula.GetMatchSizeMultiplier(_tuning, 5), 0.0001f);
            Assert.AreEqual(2.0f, CombatFormula.GetMatchSizeMultiplier(_tuning, 9), 0.0001f);
        }

        // ------------------------------------------------------------------
        // Combo multipliers
        // ------------------------------------------------------------------

        [Test]
        public void GetComboMultiplier_Zero_Returns1()
        {
            Assert.AreEqual(1.0f, CombatFormula.GetComboMultiplier(_tuning, 0), 0.0001f);
        }

        [Test]
        public void GetComboMultiplier_Two_Returns1_2()
        {
            Assert.AreEqual(1.2f, CombatFormula.GetComboMultiplier(_tuning, 2), 0.0001f);
        }

        [Test]
        public void GetComboMultiplier_ClampsToMax()
        {
            // default max = 3.0, step = 0.1, so cap hits at combo=20
            Assert.AreEqual(3.0f, CombatFormula.GetComboMultiplier(_tuning, 50), 0.0001f);
        }

        // ------------------------------------------------------------------
        // Crit chance
        // ------------------------------------------------------------------

        [Test]
        public void GetCritChance_AddsLuck()
        {
            // base 0.05 + 10 luck * 0.005 = 0.10
            Assert.AreEqual(0.10f, CombatFormula.GetCritChance(_tuning, 10f), 0.0001f);
        }

        [Test]
        public void GetCritChance_ClampsAtOne()
        {
            Assert.AreEqual(1.0f, CombatFormula.GetCritChance(_tuning, 10000f), 0.0001f);
        }

        [Test]
        public void GetCritChance_ClampsAtZero()
        {
            _tuning.BaseCritChance = -0.5f;
            Assert.AreEqual(0f, CombatFormula.GetCritChance(_tuning, 0f), 0.0001f);
        }

        // ------------------------------------------------------------------
        // CalculateDamage
        // ------------------------------------------------------------------

        [Test]
        public void CalculateDamage_BelowMinMatchSize_ReturnsZero()
        {
            var result = CombatFormula.CalculateDamage(
                _tuning, matchSize: 2, characterAttack: 10f,
                comboCount: 0, luck: 0f, enemyDefense: 0f, rng: new Random(0));
            Assert.AreEqual(0f, result.Amount);
            Assert.IsFalse(result.IsCrit);
        }

        [Test]
        public void CalculateDamage_Match3_BaselineNoCrit()
        {
            ForceNoCrit();
            var result = CombatFormula.CalculateDamage(
                _tuning, matchSize: 3, characterAttack: 10f,
                comboCount: 0, luck: 0f, enemyDefense: 0f, rng: new Random(0));

            // 10 * 3 * 1.0 * (10/10) * 1.0 * 1.0 = 30
            Assert.AreEqual(30f, result.Amount, 0.0001f);
            Assert.IsFalse(result.IsCrit);
        }

        [Test]
        public void CalculateDamage_Match4_AppliesSizeMultiplier()
        {
            ForceNoCrit();
            var result = CombatFormula.CalculateDamage(
                _tuning, matchSize: 4, characterAttack: 10f,
                comboCount: 0, luck: 0f, enemyDefense: 0f, rng: new Random(0));

            // 10 * 4 * 1.4 * 1 * 1 * 1 = 56
            Assert.AreEqual(56f, result.Amount, 0.0001f);
        }

        [Test]
        public void CalculateDamage_Combo_AppliesComboMultiplier()
        {
            ForceNoCrit();
            var result = CombatFormula.CalculateDamage(
                _tuning, matchSize: 3, characterAttack: 10f,
                comboCount: 2, luck: 0f, enemyDefense: 0f, rng: new Random(0));

            // 30 * 1.2 = 36
            Assert.AreEqual(36f, result.Amount, 0.0001f);
        }

        [Test]
        public void CalculateDamage_Crit_AppliesCritMultiplier()
        {
            ForceCrit();
            var result = CombatFormula.CalculateDamage(
                _tuning, matchSize: 3, characterAttack: 10f,
                comboCount: 0, luck: 0f, enemyDefense: 0f, rng: new Random(0));

            // 30 * 1.5 = 45
            Assert.AreEqual(45f, result.Amount, 0.0001f);
            Assert.IsTrue(result.IsCrit);
        }

        [Test]
        public void CalculateDamage_Defense_SubtractsFlat()
        {
            ForceNoCrit();
            var result = CombatFormula.CalculateDamage(
                _tuning, matchSize: 3, characterAttack: 10f,
                comboCount: 0, luck: 0f, enemyDefense: 10f, rng: new Random(0));

            // 30 - 10 = 20
            Assert.AreEqual(20f, result.Amount, 0.0001f);
        }

        [Test]
        public void CalculateDamage_Defense_NeverNegative()
        {
            ForceNoCrit();
            var result = CombatFormula.CalculateDamage(
                _tuning, matchSize: 3, characterAttack: 10f,
                comboCount: 0, luck: 0f, enemyDefense: 9999f, rng: new Random(0));

            Assert.AreEqual(0f, result.Amount, 0.0001f);
        }

        [Test]
        public void CalculateDamage_CharacterAttack_ScalesLinearly()
        {
            ForceNoCrit();
            var result = CombatFormula.CalculateDamage(
                _tuning, matchSize: 3, characterAttack: 20f,
                comboCount: 0, luck: 0f, enemyDefense: 0f, rng: new Random(0));

            // charMod = 20/10 = 2.0, so 30 * 2 = 60
            Assert.AreEqual(60f, result.Amount, 0.0001f);
        }

        [Test]
        public void CalculateDamage_NullTuning_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                CombatFormula.CalculateDamage(null, 3, 10f, 0, 0f, 0f, new Random(0)));
        }

        [Test]
        public void CalculateDamage_NullRng_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                CombatFormula.CalculateDamage(_tuning, 3, 10f, 0, 0f, 0f, null));
        }

        // ------------------------------------------------------------------
        // Other match types
        // ------------------------------------------------------------------

        [Test]
        public void CalculateEnergy_Match3_ReturnsBaseTimesSize()
        {
            // 15 * 3 * 1.0 = 45
            Assert.AreEqual(45f, CombatFormula.CalculateEnergy(_tuning, 3), 0.0001f);
        }

        [Test]
        public void CalculateEnergy_Match4_AppliesSizeMult()
        {
            // 15 * 4 * 1.4 = 84
            Assert.AreEqual(84f, CombatFormula.CalculateEnergy(_tuning, 4), 0.0001f);
        }

        [Test]
        public void CalculateEnergy_BelowMin_ReturnsZero()
        {
            Assert.AreEqual(0f, CombatFormula.CalculateEnergy(_tuning, 2), 0.0001f);
        }

        [Test]
        public void CalculateDefense_Match3_ReturnsPerTileTimesSize()
        {
            // 8 * 3 = 24
            Assert.AreEqual(24f, CombatFormula.CalculateDefense(_tuning, 3), 0.0001f);
        }

        [Test]
        public void CalculateDefense_BelowMin_ReturnsZero()
        {
            Assert.AreEqual(0f, CombatFormula.CalculateDefense(_tuning, 2), 0.0001f);
        }

        [Test]
        public void CalculateArmorDamage_Match3_ReturnsPerTileTimesSize()
        {
            // 12 * 3 = 36
            Assert.AreEqual(36f, CombatFormula.CalculateArmorDamage(_tuning, 3), 0.0001f);
        }

        [Test]
        public void CalculateArmorDamage_BelowMin_ReturnsZero()
        {
            Assert.AreEqual(0f, CombatFormula.CalculateArmorDamage(_tuning, 2), 0.0001f);
        }
    }
}
