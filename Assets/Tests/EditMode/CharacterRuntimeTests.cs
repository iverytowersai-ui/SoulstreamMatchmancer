using System;
using NUnit.Framework;
using Matchmancer.Character;
using Matchmancer.Combat;
using Matchmancer.Core;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class CharacterRuntimeTests
    {
        private CharacterTuning _tuning;

        [SetUp]
        public void SetUp()
        {
            _tuning = new CharacterTuning
            {
                DisplayName              = "Arcanist",
                BaseMaxHp                = 100,
                BaseAttack               = 10f,
                BaseDefense              = 5f,
                BaseLuck                 = 2f,
                HpPerLevel               = 10,
                AttackPerLevel           = 1f,
                DefensePerLevel          = 0.5f,
                LuckPerLevel             = 0.2f,
                UltimateName             = "Soul Surge",
                MaxEnergy                = 100f,
                UltimateDamageMultiplier = 3f,
                XpPerLevel               = new[] { 100, 150, 200, 300 },
            };
        }

        private CharacterRuntime Make(int level = 1) => new CharacterRuntime(_tuning, level);

        private static CombatEffect ShieldEffect(float amount) =>
            new CombatEffect(TileType.SoulstreamShard, CombatRole.Defense, 3, 1, shieldGenerated: amount);

        private static CombatEffect EnergyEffect(float amount) =>
            new CombatEffect(TileType.OzoneMark, CombatRole.Energy, 3, 1, energyGenerated: amount);

        private static CombatEffect DamageEffect(float amount) =>
            new CombatEffect(TileType.PortRune, CombatRole.Damage, 3, 1, damageDealt: amount);

        private static CombatEffect LuckEffect() =>
            new CombatEffect(TileType.PetshaCharm, CombatRole.Luck, 3, 1, luckCritBonus: 0.1f);

        // ==================================================================
        // Construction
        // ==================================================================

        [Test]
        public void Constructor_Level1_InitialState()
        {
            var c = Make(1);
            Assert.AreEqual("Arcanist", c.DisplayName);
            Assert.AreEqual(1, c.Level);
            Assert.AreEqual(100, c.MaxHp);
            Assert.AreEqual(100, c.CurrentHp);
            Assert.AreEqual(10f, c.CurrentAttack);
            Assert.AreEqual(5f,  c.CurrentDefense);
            Assert.AreEqual(2f,  c.CurrentLuck);
            Assert.AreEqual(0f,  c.CurrentShield);
            Assert.AreEqual(0f,  c.CurrentEnergy);
            Assert.AreEqual(100f, c.MaxEnergy);
            Assert.IsFalse(c.UltimateReady);
            Assert.IsFalse(c.UltimateQueued);
            Assert.IsFalse(c.IsDefeated);
        }

        [Test]
        public void Constructor_Level3_StatsScaleWithGrowth()
        {
            var c = Make(3);
            Assert.AreEqual(3, c.Level);
            Assert.AreEqual(120, c.MaxHp);        // 100 + 10*2
            Assert.AreEqual(120, c.CurrentHp);
            Assert.AreEqual(12f, c.CurrentAttack); // 10 + 1*2
            Assert.AreEqual(6f,  c.CurrentDefense);// 5 + 0.5*2
            Assert.AreEqual(2.4f, c.CurrentLuck, 0.0001f); // 2 + 0.2*2
        }

        [Test]
        public void Constructor_NullTuning_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new CharacterRuntime(null, 1));
        }

        [Test]
        public void Constructor_ZeroLevel_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterRuntime(_tuning, 0));
        }

        [Test]
        public void Constructor_ZeroBaseMaxHp_Throws()
        {
            _tuning.BaseMaxHp = 0;
            Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterRuntime(_tuning, 1));
        }

        [Test]
        public void Constructor_StartingLevelAboveCap_ClampsToCap()
        {
            var c = new CharacterRuntime(_tuning, 999);
            Assert.AreEqual(c.MaxLevel, c.Level);
            Assert.IsTrue(c.IsMaxLevel);
        }

        // ==================================================================
        // Incoming damage
        // ==================================================================

        [Test]
        public void TakeEnemyDamage_WithDefense_SubtractsFlat()
        {
            var c = Make();
            int dealt = c.TakeEnemyDamage(30f);
            // 30 - 5 defense = 25
            Assert.AreEqual(25, dealt);
            Assert.AreEqual(75, c.CurrentHp);
        }

        [Test]
        public void TakeEnemyDamage_SmallerThanDefense_MinOneChip()
        {
            var c = Make();
            int dealt = c.TakeEnemyDamage(2f);
            Assert.AreEqual(1, dealt);
            Assert.AreEqual(99, c.CurrentHp);
        }

        [Test]
        public void TakeEnemyDamage_Shield_AbsorbsFirst()
        {
            var c = Make();
            c.AddShield(20f);
            int dealt = c.TakeEnemyDamage(10f);
            Assert.AreEqual(10f, c.CurrentShield);
            Assert.AreEqual(100, c.CurrentHp);
            Assert.AreEqual(0, dealt);
        }

        [Test]
        public void TakeEnemyDamage_Shield_Overflow_GoesToHp()
        {
            var c = Make();
            c.AddShield(10f);
            int dealt = c.TakeEnemyDamage(30f);
            // Shield eats 10, remaining 20, minus 5 defense = 15
            Assert.AreEqual(0f, c.CurrentShield);
            Assert.AreEqual(85, c.CurrentHp);
            Assert.AreEqual(15, dealt);
        }

        [Test]
        public void TakeEnemyDamage_ZeroOrNegative_NoOp()
        {
            var c = Make();
            c.TakeEnemyDamage(0f);
            c.TakeEnemyDamage(-5f);
            Assert.AreEqual(100, c.CurrentHp);
        }

        [Test]
        public void TakeEnemyDamage_Overkill_ClampsToZeroAndDefeats()
        {
            var c = Make();
            int defeats = 0;
            c.OnDefeated += () => defeats++;
            c.TakeEnemyDamage(9999f);
            Assert.AreEqual(0, c.CurrentHp);
            Assert.IsTrue(c.IsDefeated);
            Assert.AreEqual(1, defeats);
        }

        [Test]
        public void TakeEnemyDamage_DefeatedCharacter_NoOp()
        {
            var c = Make();
            c.TakeEnemyDamage(9999f);
            int dealt = c.TakeEnemyDamage(50f);
            Assert.AreEqual(0, dealt);
            Assert.AreEqual(0, c.CurrentHp);
        }

        [Test]
        public void TakeEnemyDamage_FiresOnHpChangedAndOnDamageTaken()
        {
            var c = Make();
            int oldHp = -1, newHp = -1, delta = 0;
            float taken = -1f;
            c.OnHpChanged    += (o, n, d) => { oldHp = o; newHp = n; delta = d; };
            c.OnDamageTaken  += d => taken = d;
            c.TakeEnemyDamage(20f);
            Assert.AreEqual(100, oldHp);
            Assert.AreEqual(85,  newHp);
            Assert.AreEqual(-15, delta);
            Assert.AreEqual(15f, taken);
        }

        // ==================================================================
        // ApplyCombatEffect — player-facing routing
        // ==================================================================

        [Test]
        public void ApplyCombatEffect_Defense_AddsShield()
        {
            var c = Make();
            c.ApplyCombatEffect(ShieldEffect(25f));
            Assert.AreEqual(25f, c.CurrentShield);
        }

        [Test]
        public void ApplyCombatEffect_Energy_AddsToMeter()
        {
            var c = Make();
            c.ApplyCombatEffect(EnergyEffect(30f));
            Assert.AreEqual(30f, c.CurrentEnergy);
        }

        [Test]
        public void ApplyCombatEffect_Damage_Ignored()
        {
            var c = Make();
            c.ApplyCombatEffect(DamageEffect(50f));
            Assert.AreEqual(100, c.CurrentHp);
        }

        [Test]
        public void ApplyCombatEffect_Luck_Ignored()
        {
            var c = Make();
            c.ApplyCombatEffect(LuckEffect());
            Assert.AreEqual(0f, c.CurrentEnergy);
            Assert.AreEqual(0f, c.CurrentShield);
        }

        // ==================================================================
        // Energy & Ultimate
        // ==================================================================

        [Test]
        public void AddEnergy_CapsAtMaxEnergy()
        {
            var c = Make();
            c.AddEnergy(150f);
            Assert.AreEqual(100f, c.CurrentEnergy);
        }

        [Test]
        public void AddEnergy_FillsToMax_FiresUltimateReady()
        {
            var c = Make();
            bool ready = false;
            c.OnUltimateReadyChanged += r => ready = r;
            c.AddEnergy(100f);
            Assert.IsTrue(c.UltimateReady);
            Assert.IsTrue(ready);
        }

        [Test]
        public void AddEnergy_OnUltimateReadyFiresOnce()
        {
            var c = Make();
            int fireCount = 0;
            c.OnUltimateReadyChanged += _ => fireCount++;
            c.AddEnergy(100f);
            c.AddEnergy(50f); // already full
            Assert.AreEqual(1, fireCount);
        }

        [Test]
        public void QueueUltimate_NotReady_ReturnsFalse()
        {
            var c = Make();
            Assert.IsFalse(c.QueueUltimate());
            Assert.IsFalse(c.UltimateQueued);
        }

        [Test]
        public void QueueUltimate_Ready_ReturnsTrueAndQueues()
        {
            var c = Make();
            c.AddEnergy(100f);
            Assert.IsTrue(c.QueueUltimate());
            Assert.IsTrue(c.UltimateQueued);
        }

        [Test]
        public void ConsumeUltimateMultiplier_NotQueued_ReturnsOne()
        {
            var c = Make();
            Assert.AreEqual(1f, c.ConsumeUltimateMultiplier());
        }

        [Test]
        public void ConsumeUltimateMultiplier_Queued_ReturnsMultAndResets()
        {
            var c = Make();
            c.AddEnergy(100f);
            c.QueueUltimate();
            float mult = c.ConsumeUltimateMultiplier();
            Assert.AreEqual(3f, mult);
            Assert.AreEqual(0f, c.CurrentEnergy);
            Assert.IsFalse(c.UltimateReady);
            Assert.IsFalse(c.UltimateQueued);
        }

        [Test]
        public void ConsumeUltimateMultiplier_FiresOnUltimateReadyFalse()
        {
            var c = Make();
            c.AddEnergy(100f);
            c.QueueUltimate();
            bool? lastState = null;
            c.OnUltimateReadyChanged += r => lastState = r;
            c.ConsumeUltimateMultiplier();
            Assert.AreEqual(false, lastState);
        }

        // ==================================================================
        // Healing
        // ==================================================================

        [Test]
        public void Heal_ClampsToMaxHp()
        {
            var c = Make();
            c.TakeEnemyDamage(50f);
            c.Heal(9999);
            Assert.AreEqual(c.MaxHp, c.CurrentHp);
        }

        [Test]
        public void Heal_ZeroAmount_NoOp()
        {
            var c = Make();
            c.TakeEnemyDamage(30f);
            int before = c.CurrentHp;
            c.Heal(0);
            Assert.AreEqual(before, c.CurrentHp);
        }

        [Test]
        public void Heal_DefeatedNoOp()
        {
            var c = Make();
            c.TakeEnemyDamage(9999f);
            c.Heal(50);
            Assert.AreEqual(0, c.CurrentHp);
        }

        // ==================================================================
        // Shield
        // ==================================================================

        [Test]
        public void AddShield_Additive()
        {
            var c = Make();
            c.AddShield(10f);
            c.AddShield(15f);
            Assert.AreEqual(25f, c.CurrentShield);
        }

        [Test]
        public void AddShield_FiresEvent()
        {
            var c = Make();
            float latest = -1f;
            c.OnShieldChanged += s => latest = s;
            c.AddShield(12f);
            Assert.AreEqual(12f, latest);
        }

        // ==================================================================
        // XP & Level Up
        // ==================================================================

        [Test]
        public void AwardXp_BelowThreshold_NoLevelUp()
        {
            var c = Make();
            c.AwardXp(50);
            Assert.AreEqual(1, c.Level);
            Assert.AreEqual(50, c.CurrentXp);
        }

        [Test]
        public void AwardXp_CrossesThreshold_LevelsUpOnce()
        {
            var c = Make();
            int newLevel = -1;
            c.OnLevelUp += l => newLevel = l;
            c.AwardXp(100); // exactly threshold
            Assert.AreEqual(2, c.Level);
            Assert.AreEqual(0, c.CurrentXp);
            Assert.AreEqual(2, newLevel);
            Assert.AreEqual(110, c.MaxHp);   // recalculated
            Assert.AreEqual(11f, c.CurrentAttack);
        }

        [Test]
        public void AwardXp_MultiLevelJump_RollsOver()
        {
            var c = Make();
            int levelUps = 0;
            c.OnLevelUp += _ => levelUps++;
            // Thresholds (by current level): L1→L2=100, L2→L3=150, L3→L4=200, L4→L5=300.
            // Feed 300: L1→L2 (−100=200), L2→L3 (−150=50), L3 needs 200 → stop.
            c.AwardXp(300);
            Assert.AreEqual(3, c.Level);
            Assert.AreEqual(2, levelUps);
            Assert.AreEqual(50, c.CurrentXp);
        }

        [Test]
        public void AwardXp_LevelUpHealsForHpGrowth()
        {
            var c = Make();
            c.TakeEnemyDamage(40f); // 100 → 65 (35 taken)
            int beforeHp = c.CurrentHp;
            c.AwardXp(100); // L1 → L2, MaxHp 100→110, heal +10
            Assert.AreEqual(beforeHp + 10, c.CurrentHp);
        }

        [Test]
        public void AwardXp_PastCap_ClampsAndZeroesXp()
        {
            var c = Make();
            // Feed enough to blow through all 4 thresholds (100+150+200+300 = 750)
            c.AwardXp(10_000);
            Assert.AreEqual(c.MaxLevel, c.Level);
            Assert.IsTrue(c.IsMaxLevel);
            Assert.AreEqual(0, c.CurrentXp);
        }

        [Test]
        public void AwardXp_AtMaxLevel_NoOp()
        {
            var c = new CharacterRuntime(_tuning, 5); // cap
            Assert.IsTrue(c.IsMaxLevel);
            c.AwardXp(500);
            Assert.AreEqual(0, c.CurrentXp);
            Assert.AreEqual(5, c.Level);
        }

        [Test]
        public void AwardXp_ZeroOrNegative_NoOp()
        {
            var c = Make();
            c.AwardXp(0);
            c.AwardXp(-100);
            Assert.AreEqual(1, c.Level);
            Assert.AreEqual(0, c.CurrentXp);
        }

        [Test]
        public void XpToNextLevel_Level1_Returns100()
        {
            var c = Make();
            Assert.AreEqual(100, c.XpToNextLevel());
        }

        [Test]
        public void XpToNextLevel_AtCap_ReturnsZero()
        {
            var c = new CharacterRuntime(_tuning, 5);
            Assert.AreEqual(0, c.XpToNextLevel());
        }
    }
}
