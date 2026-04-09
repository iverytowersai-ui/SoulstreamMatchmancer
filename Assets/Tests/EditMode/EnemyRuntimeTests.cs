using System;
using NUnit.Framework;
using Matchmancer.Combat;
using Matchmancer.Core;
using Matchmancer.Enemy;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class EnemyRuntimeTests
    {
        private CombatTuning _tuning;

        [SetUp]
        public void SetUp()
        {
            _tuning = CombatTuning.Default();
            // Deterministic status values for tests
            _tuning.PoisonDamagePerTurn = 5f;
            _tuning.VulnerabilityMult   = 1.25f;
        }

        private EnemyRuntime MakeEnemy(
            int   maxHp      = 100,
            float defense    = 0f,
            int   maxArmor   = 0,
            float atk        = 10f,
            int   atkPerTurn = 1)
        {
            return new EnemyRuntime(
                displayName:     "TestFoe",
                maxHp:           maxHp,
                defense:         defense,
                maxArmor:        maxArmor,
                baseAttackPower: atk,
                attacksPerTurn:  atkPerTurn,
                tuning:          _tuning);
        }

        private static CombatEffect DamageEffect(float dmg) =>
            new CombatEffect(TileType.PortRune, CombatRole.Damage, 3, 1, damageDealt: dmg);

        private static CombatEffect BreakEffect(float armorDmg) =>
            new CombatEffect(TileType.CovenSeal, CombatRole.Break, 3, 1, armorDamageDealt: armorDmg);

        private static CombatEffect PoisonEffect(int duration) =>
            new CombatEffect(TileType.WitchbreedThorn, CombatRole.Debuff, 3, 1,
                appliesPoison: true, debuffDuration: duration);

        private static CombatEffect VulnerabilityEffect(int duration) =>
            new CombatEffect(TileType.WitchbreedThorn, CombatRole.Debuff, 3, 1,
                appliesVulnerability: true, debuffDuration: duration);

        // ------------------------------------------------------------------
        // Construction & initial state
        // ------------------------------------------------------------------

        [Test]
        public void Constructor_Defaults_InitialStateCorrect()
        {
            var e = MakeEnemy(maxHp: 80, maxArmor: 20);
            Assert.AreEqual("TestFoe", e.DisplayName);
            Assert.AreEqual(80, e.MaxHp);
            Assert.AreEqual(80, e.CurrentHp);
            Assert.AreEqual(20, e.MaxArmor);
            Assert.AreEqual(20, e.CurrentArmor);
            Assert.IsFalse(e.IsDefeated);
            Assert.IsFalse(e.IsPoisoned);
            Assert.IsFalse(e.IsVulnerable);
        }

        [Test]
        public void Constructor_NullTuning_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new EnemyRuntime("X", 100, 0f, 0, 10f, 1, null));
        }

        [Test]
        public void Constructor_ZeroHp_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new EnemyRuntime("X", 0, 0f, 0, 10f, 1, _tuning));
        }

        // ------------------------------------------------------------------
        // ApplyDamage — direct
        // ------------------------------------------------------------------

        [Test]
        public void ApplyDamage_Basic_ReducesHp()
        {
            var e = MakeEnemy(maxHp: 100);
            int dealt = e.ApplyDamage(30f);
            Assert.AreEqual(30, dealt);
            Assert.AreEqual(70, e.CurrentHp);
        }

        [Test]
        public void ApplyDamage_WithDefense_SubtractsFlat()
        {
            var e = MakeEnemy(maxHp: 100, defense: 5f);
            int dealt = e.ApplyDamage(30f);
            Assert.AreEqual(25, dealt);
            Assert.AreEqual(75, e.CurrentHp);
        }

        [Test]
        public void ApplyDamage_SmallerThanDefense_DealsZero()
        {
            var e = MakeEnemy(maxHp: 100, defense: 50f);
            int dealt = e.ApplyDamage(10f);
            Assert.AreEqual(0, dealt);
            Assert.AreEqual(100, e.CurrentHp);
        }

        [Test]
        public void ApplyDamage_OverkillClampsToZero()
        {
            var e = MakeEnemy(maxHp: 20);
            int dealt = e.ApplyDamage(999f);
            Assert.AreEqual(0, e.CurrentHp);
            Assert.IsTrue(e.IsDefeated);
            Assert.Greater(dealt, 0);
        }

        [Test]
        public void ApplyDamage_WithArmor_AbsorbsFirst()
        {
            var e = MakeEnemy(maxHp: 100, maxArmor: 20);
            int dealt = e.ApplyDamage(30f);
            Assert.AreEqual(0, e.CurrentArmor);
            Assert.AreEqual(90, e.CurrentHp); // 10 overflow
            Assert.AreEqual(10, dealt);
        }

        [Test]
        public void ApplyDamage_ArmorAbsorbsEntireHit()
        {
            var e = MakeEnemy(maxHp: 100, maxArmor: 50);
            int dealt = e.ApplyDamage(30f);
            Assert.AreEqual(20, e.CurrentArmor);
            Assert.AreEqual(100, e.CurrentHp);
            Assert.AreEqual(0, dealt);
        }

        [Test]
        public void ApplyDamage_Vulnerability_MultipliesDamage()
        {
            var e = MakeEnemy(maxHp: 100);
            e.AddVulnerability(3);
            // raw 40 * 1.25 = 50
            e.ApplyDamage(40f);
            Assert.AreEqual(50, e.CurrentHp);
        }

        [Test]
        public void ApplyDamage_FiresOnHpChanged()
        {
            var e = MakeEnemy(maxHp: 100);
            bool fired = false;
            int reportedOld = -1, reportedNew = -1, reportedAmount = 0;
            e.OnHpChanged += (o, n, a) =>
            {
                fired = true;
                reportedOld = o;
                reportedNew = n;
                reportedAmount = a;
            };
            e.ApplyDamage(25f);
            Assert.IsTrue(fired);
            Assert.AreEqual(100, reportedOld);
            Assert.AreEqual(75, reportedNew);
            Assert.AreEqual(-25, reportedAmount);
        }

        [Test]
        public void ApplyDamage_FiresOnDefeatedOnce()
        {
            var e = MakeEnemy(maxHp: 20);
            int defeatCount = 0;
            e.OnDefeated += () => defeatCount++;
            e.ApplyDamage(999f);
            Assert.AreEqual(1, defeatCount);
            // Dead enemy: no more damage, no more defeat events
            e.ApplyDamage(999f);
            Assert.AreEqual(1, defeatCount);
        }

        [Test]
        public void ApplyDamage_DefeatedEnemy_NoOp()
        {
            var e = MakeEnemy(maxHp: 20);
            e.ApplyDamage(999f);
            int dealt = e.ApplyDamage(50f);
            Assert.AreEqual(0, dealt);
            Assert.AreEqual(0, e.CurrentHp);
        }

        // ------------------------------------------------------------------
        // Armor damage
        // ------------------------------------------------------------------

        [Test]
        public void ApplyArmorDamage_ReducesArmorOnly()
        {
            var e = MakeEnemy(maxHp: 100, maxArmor: 30);
            int dealt = e.ApplyArmorDamage(10f);
            Assert.AreEqual(20, e.CurrentArmor);
            Assert.AreEqual(100, e.CurrentHp);
            Assert.AreEqual(10, dealt);
        }

        [Test]
        public void ApplyArmorDamage_NoHpSpillover()
        {
            var e = MakeEnemy(maxHp: 100, maxArmor: 10);
            e.ApplyArmorDamage(999f);
            Assert.AreEqual(0, e.CurrentArmor);
            Assert.AreEqual(100, e.CurrentHp);
        }

        [Test]
        public void ApplyArmorDamage_NoArmor_NoOp()
        {
            var e = MakeEnemy(maxHp: 100, maxArmor: 0);
            int dealt = e.ApplyArmorDamage(50f);
            Assert.AreEqual(0, dealt);
            Assert.AreEqual(100, e.CurrentHp);
        }

        // ------------------------------------------------------------------
        // ApplyCombatEffect routing
        // ------------------------------------------------------------------

        [Test]
        public void ApplyCombatEffect_Damage_RoutesToApplyDamage()
        {
            var e = MakeEnemy(maxHp: 100);
            e.ApplyCombatEffect(DamageEffect(20f));
            Assert.AreEqual(80, e.CurrentHp);
        }

        [Test]
        public void ApplyCombatEffect_Break_RoutesToArmor()
        {
            var e = MakeEnemy(maxHp: 100, maxArmor: 20);
            e.ApplyCombatEffect(BreakEffect(10f));
            Assert.AreEqual(10, e.CurrentArmor);
            Assert.AreEqual(100, e.CurrentHp);
        }

        [Test]
        public void ApplyCombatEffect_DebuffPoison_AddsStack()
        {
            var e = MakeEnemy(maxHp: 100);
            e.ApplyCombatEffect(PoisonEffect(3));
            Assert.IsTrue(e.IsPoisoned);
            Assert.AreEqual(1, e.PoisonStacks);
            Assert.AreEqual(3, e.PoisonTurnsRemaining);
        }

        [Test]
        public void ApplyCombatEffect_DebuffVulnerability_SetsFlag()
        {
            var e = MakeEnemy(maxHp: 100);
            e.ApplyCombatEffect(VulnerabilityEffect(2));
            Assert.IsTrue(e.IsVulnerable);
            Assert.AreEqual(2, e.VulnerabilityTurnsRemaining);
        }

        [Test]
        public void ApplyCombatEffect_Energy_Ignored()
        {
            var e = MakeEnemy(maxHp: 100);
            var energy = new CombatEffect(
                TileType.OzoneMark, CombatRole.Energy, 3, 1, energyGenerated: 50f);
            e.ApplyCombatEffect(energy);
            Assert.AreEqual(100, e.CurrentHp);
        }

        [Test]
        public void ApplyCombatEffect_Defense_Ignored()
        {
            var e = MakeEnemy(maxHp: 100);
            var shield = new CombatEffect(
                TileType.SoulstreamShard, CombatRole.Defense, 3, 1, shieldGenerated: 30f);
            e.ApplyCombatEffect(shield);
            Assert.AreEqual(100, e.CurrentHp);
        }

        [Test]
        public void ApplyCombatEffect_Luck_Ignored()
        {
            var e = MakeEnemy(maxHp: 100);
            var luck = new CombatEffect(
                TileType.PetshaCharm, CombatRole.Luck, 3, 1, luckCritBonus: 0.1f);
            e.ApplyCombatEffect(luck);
            Assert.AreEqual(100, e.CurrentHp);
        }

        // ------------------------------------------------------------------
        // Poison
        // ------------------------------------------------------------------

        [Test]
        public void AddPoisonStack_AccumulatesStacks()
        {
            var e = MakeEnemy();
            e.AddPoisonStack(3);
            e.AddPoisonStack(3);
            e.AddPoisonStack(3);
            Assert.AreEqual(3, e.PoisonStacks);
        }

        [Test]
        public void AddPoisonStack_RefreshesDurationWhenLonger()
        {
            var e = MakeEnemy();
            e.AddPoisonStack(2);
            e.AddPoisonStack(5);
            Assert.AreEqual(5, e.PoisonTurnsRemaining);
        }

        [Test]
        public void AddPoisonStack_DoesNotShortenDuration()
        {
            var e = MakeEnemy();
            e.AddPoisonStack(5);
            e.AddPoisonStack(2);
            Assert.AreEqual(5, e.PoisonTurnsRemaining);
        }

        // ------------------------------------------------------------------
        // OnPlayerTurnEnd
        // ------------------------------------------------------------------

        [Test]
        public void OnPlayerTurnEnd_PoisonTicksDamage()
        {
            var e = MakeEnemy(maxHp: 100);
            e.AddPoisonStack(3); // 1 stack × 5 dmg
            e.OnPlayerTurnEnd();
            Assert.AreEqual(95, e.CurrentHp);
            Assert.AreEqual(2, e.PoisonTurnsRemaining);
        }

        [Test]
        public void OnPlayerTurnEnd_PoisonBypassesDefenseAndArmor()
        {
            var e = MakeEnemy(maxHp: 100, defense: 50f, maxArmor: 50);
            e.AddPoisonStack(3);
            e.OnPlayerTurnEnd();
            // 1 stack × 5 dmg bypasses both
            Assert.AreEqual(95, e.CurrentHp);
            Assert.AreEqual(50, e.CurrentArmor);
        }

        [Test]
        public void OnPlayerTurnEnd_PoisonScalesWithStacks()
        {
            var e = MakeEnemy(maxHp: 100);
            e.AddPoisonStack(3);
            e.AddPoisonStack(3);
            e.AddPoisonStack(3); // 3 stacks × 5 = 15
            e.OnPlayerTurnEnd();
            Assert.AreEqual(85, e.CurrentHp);
        }

        [Test]
        public void OnPlayerTurnEnd_PoisonExpires_ClearsStacks()
        {
            var e = MakeEnemy(maxHp: 100);
            e.AddPoisonStack(1);
            e.OnPlayerTurnEnd();
            Assert.AreEqual(0, e.PoisonStacks);
            Assert.AreEqual(0, e.PoisonTurnsRemaining);
            Assert.IsFalse(e.IsPoisoned);
        }

        [Test]
        public void OnPlayerTurnEnd_VulnerabilityDecrements()
        {
            var e = MakeEnemy();
            e.AddVulnerability(2);
            e.OnPlayerTurnEnd();
            Assert.AreEqual(1, e.VulnerabilityTurnsRemaining);
            e.OnPlayerTurnEnd();
            Assert.AreEqual(0, e.VulnerabilityTurnsRemaining);
            Assert.IsFalse(e.IsVulnerable);
        }

        [Test]
        public void OnPlayerTurnEnd_PoisonKillsEnemy_FiresDefeated()
        {
            var e = MakeEnemy(maxHp: 5);
            int defeatCount = 0;
            e.OnDefeated += () => defeatCount++;
            e.AddPoisonStack(3); // 5 dmg → kills
            e.OnPlayerTurnEnd();
            Assert.AreEqual(0, e.CurrentHp);
            Assert.IsTrue(e.IsDefeated);
            Assert.AreEqual(1, defeatCount);
        }

        [Test]
        public void OnPlayerTurnEnd_FiresOnPoisonTicked()
        {
            var e = MakeEnemy(maxHp: 100);
            int tickDmg = 0;
            e.OnPoisonTicked += d => tickDmg = d;
            e.AddPoisonStack(3);
            e.OnPlayerTurnEnd();
            Assert.AreEqual(5, tickDmg);
        }

        // ------------------------------------------------------------------
        // RollAttack
        // ------------------------------------------------------------------

        [Test]
        public void RollAttack_ReturnsBaseAttackTimesAttacksPerTurn()
        {
            var e = MakeEnemy(atk: 12f, atkPerTurn: 3);
            Assert.AreEqual(36f, e.RollAttack());
        }

        [Test]
        public void RollAttack_FiresOnAttack()
        {
            var e = MakeEnemy(atk: 15f);
            float got = -1f;
            e.OnAttack += d => got = d;
            e.RollAttack();
            Assert.AreEqual(15f, got);
        }

        [Test]
        public void RollAttack_DefeatedEnemy_ReturnsZero()
        {
            var e = MakeEnemy(maxHp: 10, atk: 20f);
            e.ApplyDamage(999f);
            Assert.AreEqual(0f, e.RollAttack());
        }
    }
}
