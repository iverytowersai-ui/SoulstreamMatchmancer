using System;
using System.Collections.Generic;
using NUnit.Framework;
using Matchmancer.Combat;
using Matchmancer.Core;
using Matchmancer.Match;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class CombatResolverTests
    {
        private CombatTuning _tuning;
        private CombatStats  _stats;
        private CombatResolver _resolver;

        [SetUp]
        public void SetUp()
        {
            _tuning = CombatTuning.Default();
            // Deterministic: kill crit randomness for most tests.
            _tuning.BaseCritChance = 0f;
            _tuning.LuckToCritRate = 0f;

            _stats = new CombatStats();
            _stats.ResetForNewBattle();

            _resolver = new CombatResolver(_tuning, _stats, new Random(0));
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static MatchInfo MakeMatch(TileType type, int tileCount,
                                           MatchPattern pattern = MatchPattern.ThreeInARow)
        {
            var positions = new List<GridPosition>();
            for (int i = 0; i < tileCount; i++)
                positions.Add(new GridPosition(0, i));
            return new MatchInfo(type, pattern, positions);
        }

        private static IReadOnlyList<MatchInfo> Wave(params MatchInfo[] matches) => matches;

        // ------------------------------------------------------------------
        // Constructor guards
        // ------------------------------------------------------------------

        [Test]
        public void Constructor_NullTuning_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new CombatResolver(null, _stats, new Random(0)));
        }

        [Test]
        public void Constructor_NullStats_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new CombatResolver(_tuning, null, new Random(0)));
        }

        [Test]
        public void Constructor_NullRng_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new CombatResolver(_tuning, _stats, null));
        }

        // ------------------------------------------------------------------
        // Empty / null input
        // ------------------------------------------------------------------

        [Test]
        public void ResolveWave_NullMatches_ReturnsEmpty_FiresWaveEventOnce()
        {
            int waveEvents = 0;
            _resolver.OnWaveResolved += _ => waveEvents++;

            var result = _resolver.ResolveWave(null, comboCount: 1);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
            Assert.AreEqual(1, waveEvents);
        }

        [Test]
        public void ResolveWave_EmptyMatches_ReturnsEmpty()
        {
            var result = _resolver.ResolveWave(new List<MatchInfo>(), comboCount: 1);
            Assert.AreEqual(0, result.Count);
        }

        // ------------------------------------------------------------------
        // Damage (SoulstreamShard)
        // ------------------------------------------------------------------

        [Test]
        public void ResolveWave_Match3Damage_ProducesDamageEffect()
        {
            var match = MakeMatch(TileType.SoulstreamShard, 3);
            var effects = _resolver.ResolveWave(Wave(match), comboCount: 1,
                                                characterAttack: 10f, characterLuck: 0f);

            Assert.AreEqual(1, effects.Count);
            var e = effects[0];
            Assert.AreEqual(CombatRole.Damage, e.Role);
            Assert.AreEqual(TileType.SoulstreamShard, e.SourceTile);
            Assert.AreEqual(3, e.MatchSize);
            // 10 * 3 * 1.0 * 1 * 1.1 (combo=1) * 1 = 33
            Assert.AreEqual(33f, e.DamageDealt, 0.0001f);
            Assert.IsFalse(e.IsCriticalHit);
        }

        [Test]
        public void ResolveWave_Match4Damage_AppliesSizeMultiplier()
        {
            var match = MakeMatch(TileType.SoulstreamShard, 4, MatchPattern.FourInARow);
            var effects = _resolver.ResolveWave(Wave(match), comboCount: 1);

            // 10 * 4 * 1.4 * 1 * 1.1 = 61.6
            Assert.AreEqual(61.6f, effects[0].DamageDealt, 0.001f);
        }

        [Test]
        public void ResolveWave_DamageRecordsToStats()
        {
            var match = MakeMatch(TileType.SoulstreamShard, 3);
            _resolver.ResolveWave(Wave(match), comboCount: 1);

            Assert.AreEqual(33f, _stats.TotalDamageDealt, 0.0001f);
            Assert.AreEqual(33f, _stats.MaxSingleHit,     0.0001f);
            Assert.AreEqual(0,   _stats.TotalCriticalHits);
        }

        [Test]
        public void ResolveWave_ForcedCrit_RecordsCritAndAppliesMultiplier()
        {
            _tuning.BaseCritChance = 1f; // guaranteed crit
            _resolver = new CombatResolver(_tuning, _stats, new Random(0));

            var match = MakeMatch(TileType.SoulstreamShard, 3);
            var effects = _resolver.ResolveWave(Wave(match), comboCount: 1);

            Assert.IsTrue(effects[0].IsCriticalHit);
            // 33 * 1.5 = 49.5
            Assert.AreEqual(49.5f, effects[0].DamageDealt, 0.001f);
            Assert.AreEqual(1, _stats.TotalCriticalHits);
        }

        // ------------------------------------------------------------------
        // Energy (PortRune)
        // ------------------------------------------------------------------

        [Test]
        public void ResolveWave_EnergyMatch_GeneratesEnergy()
        {
            var match = MakeMatch(TileType.PortRune, 3);
            var effects = _resolver.ResolveWave(Wave(match), comboCount: 1);

            Assert.AreEqual(1, effects.Count);
            var e = effects[0];
            Assert.AreEqual(CombatRole.Energy, e.Role);
            Assert.AreEqual(45f, e.EnergyGenerated, 0.0001f); // 15 * 3 * 1.0
            Assert.AreEqual(45f, _stats.TotalEnergyGenerated, 0.0001f);
        }

        // ------------------------------------------------------------------
        // Defense (CovenSeal)
        // ------------------------------------------------------------------

        [Test]
        public void ResolveWave_DefenseMatch_GeneratesShield()
        {
            var match = MakeMatch(TileType.CovenSeal, 3);
            var effects = _resolver.ResolveWave(Wave(match), comboCount: 1);

            Assert.AreEqual(CombatRole.Defense, effects[0].Role);
            Assert.AreEqual(24f, effects[0].ShieldGenerated, 0.0001f); // 8 * 3
            Assert.AreEqual(24f, _stats.TotalShieldGenerated, 0.0001f);
        }

        // ------------------------------------------------------------------
        // Break (WitchbreedThorn)
        // ------------------------------------------------------------------

        [Test]
        public void ResolveWave_BreakMatch_DealsArmorDamage_IncrementsBreakCounter()
        {
            var match = MakeMatch(TileType.WitchbreedThorn, 3);
            var effects = _resolver.ResolveWave(Wave(match), comboCount: 1);

            Assert.AreEqual(CombatRole.Break, effects[0].Role);
            Assert.AreEqual(36f, effects[0].ArmorDamageDealt, 0.0001f); // 12 * 3
            Assert.AreEqual(1, _stats.BreaksTriggered);
        }

        // ------------------------------------------------------------------
        // Debuff (OzoneMark)
        // ------------------------------------------------------------------

        [Test]
        public void ResolveWave_Match3Debuff_AppliesPoisonOnly()
        {
            var match = MakeMatch(TileType.OzoneMark, 3);
            var e = _resolver.ResolveWave(Wave(match), comboCount: 1)[0];

            Assert.AreEqual(CombatRole.Debuff, e.Role);
            Assert.IsTrue(e.AppliesPoison);
            Assert.IsFalse(e.AppliesVulnerability);
            Assert.AreEqual(_tuning.PoisonDuration, e.DebuffDuration);
            Assert.AreEqual(1, _stats.DebuffsApplied);
        }

        [Test]
        public void ResolveWave_Match4Debuff_AppliesPoisonAndVulnerability()
        {
            var match = MakeMatch(TileType.OzoneMark, 4, MatchPattern.FourInARow);
            var e = _resolver.ResolveWave(Wave(match), comboCount: 1)[0];

            Assert.IsTrue(e.AppliesPoison);
            Assert.IsTrue(e.AppliesVulnerability);
        }

        // ------------------------------------------------------------------
        // Luck (PetshaCharm)
        // ------------------------------------------------------------------

        [Test]
        public void ResolveWave_LuckMatch_IncrementsLuckTileCounter()
        {
            // SetUp zeros LuckToCritRate to kill crit randomness, so restore
            // a real value for this test specifically since it verifies the
            // luck-bonus math itself.
            _tuning.LuckToCritRate = 0.005f;
            _resolver = new CombatResolver(_tuning, _stats, new Random(0));

            var match = MakeMatch(TileType.PetshaCharm, 3);
            var effects = _resolver.ResolveWave(Wave(match), comboCount: 1);

            Assert.AreEqual(CombatRole.Luck, effects[0].Role);
            Assert.Greater(effects[0].LuckCritBonus,  0f);
            Assert.Greater(effects[0].LuckComboBonus, 0f);
            Assert.AreEqual(3, _stats.LuckTilesMatched);
        }

        [Test]
        public void ResolveWave_LuckBoostsDamageCrit_WhenPairedWithDamageMatch()
        {
            // Arrange: make crit chance non-zero so luck can push it to 100%
            _tuning.BaseCritChance = 0.5f;
            _tuning.LuckToCritRate = 1f; // every luck point = +100% crit
            _resolver = new CombatResolver(_tuning, _stats, new Random(0));

            var luckMatch   = MakeMatch(TileType.PetshaCharm,     3);
            var damageMatch = MakeMatch(TileType.SoulstreamShard, 3);

            var effects = _resolver.ResolveWave(Wave(luckMatch, damageMatch), comboCount: 1);

            // Find the damage effect (order may be luck-first per our pass order)
            CombatEffect? damage = null;
            foreach (var e in effects)
                if (e.Role == CombatRole.Damage) damage = e;

            Assert.IsNotNull(damage);
            Assert.IsTrue(damage.Value.IsCriticalHit, "Luck tiles should guarantee crit here");
        }

        // ------------------------------------------------------------------
        // Resolve order
        // ------------------------------------------------------------------

        [Test]
        public void ResolveWave_OrdersLuckFirstThenBreakDebuffDamageDefenseEnergy()
        {
            // One of each role, submitted in random order.
            var matches = new List<MatchInfo>
            {
                MakeMatch(TileType.PortRune,         3), // Energy
                MakeMatch(TileType.SoulstreamShard,  3), // Damage
                MakeMatch(TileType.PetshaCharm,      3), // Luck
                MakeMatch(TileType.CovenSeal,        3), // Defense
                MakeMatch(TileType.WitchbreedThorn,  3), // Break
                MakeMatch(TileType.OzoneMark,        3), // Debuff
            };

            var effects = _resolver.ResolveWave(matches, comboCount: 1);
            Assert.AreEqual(6, effects.Count);

            var order = new List<CombatRole>();
            foreach (var e in effects) order.Add(e.Role);

            Assert.AreEqual(CombatRole.Luck,    order[0]);
            Assert.AreEqual(CombatRole.Break,   order[1]);
            Assert.AreEqual(CombatRole.Debuff,  order[2]);
            Assert.AreEqual(CombatRole.Damage,  order[3]);
            Assert.AreEqual(CombatRole.Defense, order[4]);
            Assert.AreEqual(CombatRole.Energy,  order[5]);
        }

        // ------------------------------------------------------------------
        // Events
        // ------------------------------------------------------------------

        [Test]
        public void ResolveWave_FiresOnEffectResolved_PerMatch()
        {
            int eventCount = 0;
            _resolver.OnEffectResolved += _ => eventCount++;

            _resolver.ResolveWave(Wave(
                MakeMatch(TileType.SoulstreamShard, 3),
                MakeMatch(TileType.PortRune, 3),
                MakeMatch(TileType.CovenSeal, 3)
            ), comboCount: 1);

            Assert.AreEqual(3, eventCount);
        }

        [Test]
        public void ResolveWave_FiresOnWaveResolved_Once()
        {
            int waveCount = 0;
            _resolver.OnWaveResolved += _ => waveCount++;

            _resolver.ResolveWave(Wave(
                MakeMatch(TileType.SoulstreamShard, 3),
                MakeMatch(TileType.PortRune, 3)
            ), comboCount: 1);

            Assert.AreEqual(1, waveCount);
        }

        // ------------------------------------------------------------------
        // Combo tracking
        // ------------------------------------------------------------------

        [Test]
        public void ResolveWave_RecordsCombo_ToStats()
        {
            _resolver.ResolveWave(Wave(MakeMatch(TileType.SoulstreamShard, 3)), comboCount: 3);
            Assert.AreEqual(3, _stats.CurrentCombo);
            Assert.AreEqual(3, _stats.MaxCombo);
        }

        [Test]
        public void ResolveWave_HigherComboDealsMoreDamage()
        {
            var match = MakeMatch(TileType.SoulstreamShard, 3);

            var d1 = _resolver.ResolveWave(Wave(match), comboCount: 1)[0].DamageDealt;
            // Fresh resolver so stats don't accumulate into formula
            _resolver = new CombatResolver(_tuning, new CombatStats(), new Random(0));
            var d5 = _resolver.ResolveWave(Wave(match), comboCount: 5)[0].DamageDealt;

            Assert.Greater(d5, d1);
        }
    }
}
