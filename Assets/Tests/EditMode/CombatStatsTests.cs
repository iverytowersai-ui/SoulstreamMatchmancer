using NUnit.Framework;
using Matchmancer.Combat;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class CombatStatsTests
    {
        private CombatStats _stats;

        [SetUp]
        public void SetUp()
        {
            _stats = new CombatStats();
            _stats.ResetForNewBattle();
        }

        // ------------------------------------------------------------------
        // Reset
        // ------------------------------------------------------------------

        [Test]
        public void ResetForNewBattle_ClearsAllCounters()
        {
            _stats.RecordDamage(50f, true);
            _stats.RecordCombo(5);
            _stats.RecordShield(20f);
            _stats.RecordEnergy(15f);
            _stats.RecordMove();
            _stats.RecordLuckTile(3);
            _stats.RecordDebuffApplied();
            _stats.RecordBreakTriggered();
            _stats.RecordUltimate();
            _stats.FinalizeBattle(true, 42f);

            _stats.ResetForNewBattle();

            Assert.AreEqual(0f, _stats.TotalDamageDealt);
            Assert.AreEqual(0f, _stats.MaxSingleHit);
            Assert.AreEqual(0,  _stats.TotalCriticalHits);
            Assert.AreEqual(0,  _stats.CurrentCombo);
            Assert.AreEqual(0,  _stats.MaxCombo);
            Assert.AreEqual(0,  _stats.TotalMoves);
            Assert.AreEqual(0f, _stats.TotalShieldGenerated);
            Assert.AreEqual(0f, _stats.TotalEnergyGenerated);
            Assert.AreEqual(0,  _stats.UltimatesActivated);
            Assert.AreEqual(0,  _stats.LuckTilesMatched);
            Assert.AreEqual(0,  _stats.DebuffsApplied);
            Assert.AreEqual(0,  _stats.BreaksTriggered);
            Assert.IsFalse(_stats.IsVictory);
            Assert.AreEqual(0f, _stats.BattleDuration);
        }

        // ------------------------------------------------------------------
        // Damage
        // ------------------------------------------------------------------

        [Test]
        public void RecordDamage_AccumulatesTotal()
        {
            _stats.RecordDamage(10f, false);
            _stats.RecordDamage(25f, false);
            Assert.AreEqual(35f, _stats.TotalDamageDealt);
        }

        [Test]
        public void RecordDamage_TracksMaxSingleHit()
        {
            _stats.RecordDamage(50f, false);
            _stats.RecordDamage(30f, false);
            _stats.RecordDamage(80f, false);
            _stats.RecordDamage(20f, false);
            Assert.AreEqual(80f, _stats.MaxSingleHit);
        }

        [Test]
        public void RecordDamage_CritIncrementsCritCounter()
        {
            _stats.RecordDamage(50f, false);
            _stats.RecordDamage(30f, true);
            _stats.RecordDamage(40f, true);
            Assert.AreEqual(2, _stats.TotalCriticalHits);
        }

        [Test]
        public void RecordDamage_NegativeClampsToZero()
        {
            _stats.RecordDamage(-10f, false);
            Assert.AreEqual(0f, _stats.TotalDamageDealt);
            Assert.AreEqual(0f, _stats.MaxSingleHit);
        }

        [Test]
        public void RecordDamage_FiresEvent()
        {
            float receivedAmount = 0f;
            bool  receivedCrit   = false;
            _stats.OnDamageDealt += (amt, crit) => { receivedAmount = amt; receivedCrit = crit; };

            _stats.RecordDamage(42f, true);

            Assert.AreEqual(42f, receivedAmount);
            Assert.IsTrue(receivedCrit);
        }

        // ------------------------------------------------------------------
        // Combos
        // ------------------------------------------------------------------

        [Test]
        public void RecordCombo_UpdatesCurrentAndMax()
        {
            _stats.RecordCombo(3);
            Assert.AreEqual(3, _stats.CurrentCombo);
            Assert.AreEqual(3, _stats.MaxCombo);

            _stats.RecordCombo(7);
            Assert.AreEqual(7, _stats.CurrentCombo);
            Assert.AreEqual(7, _stats.MaxCombo);
        }

        [Test]
        public void RecordCombo_MaxDoesNotRegress()
        {
            _stats.RecordCombo(10);
            _stats.RecordCombo(4);
            Assert.AreEqual(4,  _stats.CurrentCombo);
            Assert.AreEqual(10, _stats.MaxCombo);
        }

        [Test]
        public void ResetCombo_ClearsCurrentButKeepsMax()
        {
            _stats.RecordCombo(8);
            _stats.ResetCombo();
            Assert.AreEqual(0, _stats.CurrentCombo);
            Assert.AreEqual(8, _stats.MaxCombo);
        }

        [Test]
        public void RecordCombo_NegativeClampsToZero()
        {
            _stats.RecordCombo(-5);
            Assert.AreEqual(0, _stats.CurrentCombo);
        }

        // ------------------------------------------------------------------
        // Shield / Energy
        // ------------------------------------------------------------------

        [Test]
        public void RecordShield_AccumulatesPositiveOnly()
        {
            _stats.RecordShield(10f);
            _stats.RecordShield(15f);
            _stats.RecordShield(-5f);
            _stats.RecordShield(0f);
            Assert.AreEqual(25f, _stats.TotalShieldGenerated);
        }

        [Test]
        public void RecordEnergy_AccumulatesPositiveOnly()
        {
            _stats.RecordEnergy(12f);
            _stats.RecordEnergy(8f);
            _stats.RecordEnergy(-3f);
            Assert.AreEqual(20f, _stats.TotalEnergyGenerated);
        }

        // ------------------------------------------------------------------
        // Move counters
        // ------------------------------------------------------------------

        [Test]
        public void MoveCounters_Increment()
        {
            _stats.RecordMove();
            _stats.RecordMove();
            _stats.RecordMove();
            _stats.RecordInventoryMove();
            _stats.RecordClutchMove();
            _stats.RecordUltimate();
            _stats.RecordUltimate();

            Assert.AreEqual(3, _stats.TotalMoves);
            Assert.AreEqual(1, _stats.InventoryMovesUsed);
            Assert.AreEqual(1, _stats.ClutchMoves);
            Assert.AreEqual(2, _stats.UltimatesActivated);
        }

        // ------------------------------------------------------------------
        // Luck / Debuff / Break
        // ------------------------------------------------------------------

        [Test]
        public void RecordLuckTile_DefaultAddsOne()
        {
            _stats.RecordLuckTile();
            _stats.RecordLuckTile();
            Assert.AreEqual(2, _stats.LuckTilesMatched);
        }

        [Test]
        public void RecordLuckTile_BulkAdds()
        {
            _stats.RecordLuckTile(4);
            _stats.RecordLuckTile(3);
            Assert.AreEqual(7, _stats.LuckTilesMatched);
        }

        [Test]
        public void RecordDebuffAndBreak_Increment()
        {
            _stats.RecordDebuffApplied();
            _stats.RecordDebuffApplied();
            _stats.RecordBreakTriggered();
            Assert.AreEqual(2, _stats.DebuffsApplied);
            Assert.AreEqual(1, _stats.BreaksTriggered);
        }

        // ------------------------------------------------------------------
        // Finalize
        // ------------------------------------------------------------------

        [Test]
        public void FinalizeBattle_SetsVictoryAndDuration()
        {
            _stats.FinalizeBattle(true, 123.5f);
            Assert.IsTrue(_stats.IsVictory);
            Assert.AreEqual(123.5f, _stats.BattleDuration, 0.0001f);
        }

        [Test]
        public void FinalizeBattle_LossKeepsDuration()
        {
            _stats.FinalizeBattle(false, 60f);
            Assert.IsFalse(_stats.IsVictory);
            Assert.AreEqual(60f, _stats.BattleDuration, 0.0001f);
        }
    }
}
