using NUnit.Framework;
using Matchmancer.Character;

namespace Matchmancer.Tests
{
    /// <summary>
    /// Integration tests that wire <see cref="GearInventory"/> to a live
    /// <see cref="CharacterRuntime"/> the same way
    /// <see cref="GearInventoryController"/> does in a real scene — by
    /// subscribing to <c>OnModifiersChanged</c> and pushing totals into
    /// <c>CharacterRuntime.SetGearModifiers</c>.
    ///
    /// Purpose: prove the full loadout → stat pipeline works without a scene.
    /// </summary>
    [TestFixture]
    public class GearCharacterIntegrationTests
    {
        private CharacterTuning _tuning;

        [SetUp]
        public void SetUp()
        {
            _tuning = new CharacterTuning
            {
                DisplayName              = "Test Hero",
                BaseMaxHp                = 100,
                BaseAttack               = 10f,
                BaseDefense              = 5f,
                BaseLuck                 = 2f,
                HpPerLevel               = 10,
                AttackPerLevel           = 1f,
                DefensePerLevel          = 0.5f,
                LuckPerLevel             = 0.2f,
                UltimateName             = "Test Ult",
                MaxEnergy                = 100f,
                UltimateDamageMultiplier = 3f,
                XpPerLevel               = new[] { 100, 150, 200, 300 },
            };
        }

        private static CharacterRuntime BuildRuntimeWiredTo(GearInventory inv, CharacterTuning tuning, int startingLevel = 1)
        {
            var runtime = new CharacterRuntime(tuning, startingLevel);
            inv.OnModifiersChanged += mods => runtime.SetGearModifiers(mods);
            return runtime;
        }

        // ==================================================================
        // Baseline + single-slot application
        // ==================================================================

        [Test]
        public void NoGear_UsesBaseStats()
        {
            var runtime = new CharacterRuntime(_tuning);
            Assert.AreEqual(100, runtime.MaxHp);
            Assert.AreEqual(10f, runtime.CurrentAttack, 0.0001f);
            Assert.AreEqual(5f,  runtime.CurrentDefense, 0.0001f);
            Assert.AreEqual(2f,  runtime.CurrentLuck, 0.0001f);
            Assert.IsTrue(runtime.GearModifiers.IsZero);
        }

        [Test]
        public void EquipFlatAttackWeapon_RaisesAttack()
        {
            var inv = new GearInventory();
            var runtime = BuildRuntimeWiredTo(inv, _tuning);

            var sword = new GearItem(new GearTuning
            {
                Id = "rust_blade", DisplayName = "Rust Blade",
                Slot = GearSlot.Weapon,
                Modifiers = new GearStatModifiers { FlatAttack = 5f },
            });
            inv.AddItem(sword);
            inv.Equip(sword.InstanceId);

            Assert.AreEqual(15f, runtime.CurrentAttack, 0.0001f);
        }

        [Test]
        public void EquipPctHpArmor_ScalesMaxHpAndFullHealsOnBuild()
        {
            // 100 base * 1.20 = 120
            var inv = new GearInventory();
            var armor = new GearItem(new GearTuning
            {
                Id = "vitality_vest",
                Slot = GearSlot.Armor,
                Modifiers = new GearStatModifiers { PctMaxHp = 0.20f },
            });
            inv.AddItem(armor);
            inv.Equip(armor.InstanceId);

            // Build runtime AFTER equipping and manually seed the gear.
            var runtime = new CharacterRuntime(_tuning);
            runtime.SetGearModifiers(inv.TotalEquippedModifiers);

            Assert.AreEqual(120, runtime.MaxHp);
            // HP ratio preservation from full: should still be full.
            Assert.AreEqual(120, runtime.CurrentHp);
        }

        [Test]
        public void UnequipItem_RevertsStats()
        {
            var inv = new GearInventory();
            var runtime = BuildRuntimeWiredTo(inv, _tuning);

            var sword = new GearItem(new GearTuning
            {
                Id = "sword", Slot = GearSlot.Weapon,
                Modifiers = new GearStatModifiers { FlatAttack = 7f },
            });
            inv.AddItem(sword);
            inv.Equip(sword.InstanceId);
            Assert.AreEqual(17f, runtime.CurrentAttack, 0.0001f);

            inv.Unequip(GearSlot.Weapon);
            Assert.AreEqual(10f, runtime.CurrentAttack, 0.0001f);
            Assert.IsTrue(runtime.GearModifiers.IsZero);
        }

        // ==================================================================
        // Multi-slot stacking
        // ==================================================================

        [Test]
        public void FullLoadout_StacksAllFourSlots()
        {
            var inv = new GearInventory();
            var runtime = BuildRuntimeWiredTo(inv, _tuning);

            inv.AddItem(new GearItem(new GearTuning { Id = "w", Slot = GearSlot.Weapon,
                Modifiers = new GearStatModifiers { FlatAttack = 5f, PctAttack = 0.10f } }));
            inv.AddItem(new GearItem(new GearTuning { Id = "a", Slot = GearSlot.Armor,
                Modifiers = new GearStatModifiers { FlatDefense = 3f, FlatMaxHp = 20 } }));
            inv.AddItem(new GearItem(new GearTuning { Id = "t", Slot = GearSlot.Talisman,
                Modifiers = new GearStatModifiers { FlatLuck = 3f } }));
            inv.AddItem(new GearItem(new GearTuning { Id = "r", Slot = GearSlot.Relic,
                Modifiers = new GearStatModifiers { PctMaxHp = 0.10f } }));

            foreach (var item in inv.OwnedItems.Values)
                inv.Equip(item.InstanceId);

            // Attack: 10 * 1.10 + 5 = 16
            Assert.AreEqual(16f, runtime.CurrentAttack, 0.0001f);
            // Defense: 5 + 3 = 8
            Assert.AreEqual(8f,  runtime.CurrentDefense, 0.0001f);
            // Luck: 2 + 3 = 5
            Assert.AreEqual(5f,  runtime.CurrentLuck, 0.0001f);
            // MaxHp: 100 * 1.10 + 20 = 130
            Assert.AreEqual(130, runtime.MaxHp);
        }

        [Test]
        public void SwappingWeapon_UpdatesStatsInPlace()
        {
            var inv = new GearInventory();
            var runtime = BuildRuntimeWiredTo(inv, _tuning);

            var rust = new GearItem(new GearTuning { Id = "rust", Slot = GearSlot.Weapon,
                Modifiers = new GearStatModifiers { FlatAttack = 3f } });
            var soul = new GearItem(new GearTuning { Id = "soul", Slot = GearSlot.Weapon,
                Modifiers = new GearStatModifiers { FlatAttack = 12f } });

            inv.AddItem(rust);
            inv.AddItem(soul);

            inv.Equip(rust.InstanceId);
            Assert.AreEqual(13f, runtime.CurrentAttack, 0.0001f);

            inv.Equip(soul.InstanceId);
            Assert.AreEqual(22f, runtime.CurrentAttack, 0.0001f);
        }

        // ==================================================================
        // HP ratio preservation across modifier changes
        // ==================================================================

        [Test]
        public void GearMaxHpIncrease_PreservesHpRatio()
        {
            var inv = new GearInventory();
            var runtime = BuildRuntimeWiredTo(inv, _tuning);

            // Take damage: 100 -> 50 (50% ratio)
            runtime.TakeEnemyDamage(55f); // 55 - 5 def = 50
            Assert.AreEqual(50, runtime.CurrentHp);

            // Equip +100% HP -> MaxHp 200, CurrentHp should become 100 (50%).
            var vest = new GearItem(new GearTuning { Id = "vest", Slot = GearSlot.Armor,
                Modifiers = new GearStatModifiers { PctMaxHp = 1.00f } });
            inv.AddItem(vest);
            inv.Equip(vest.InstanceId);

            Assert.AreEqual(200, runtime.MaxHp);
            Assert.AreEqual(100, runtime.CurrentHp);
        }

        [Test]
        public void GearMaxHpDecrease_NeverDropsToZero()
        {
            var inv = new GearInventory();
            var runtime = BuildRuntimeWiredTo(inv, _tuning);

            // Take damage down to 1 HP
            runtime.TakeEnemyDamage(9999f);
            Assert.AreEqual(1, runtime.CurrentHp);
            Assert.IsFalse(runtime.IsDefeated);

            // Equip a cursed relic that halves MaxHp — ratio would be ~0.5,
            // but a rounding trap could zero it out. Must clamp to >=1.
            var cursed = new GearItem(new GearTuning { Id = "cursed", Slot = GearSlot.Relic,
                Modifiers = new GearStatModifiers { PctMaxHp = -0.50f } });
            inv.AddItem(cursed);
            inv.Equip(cursed.InstanceId);

            Assert.AreEqual(50, runtime.MaxHp);
            Assert.GreaterOrEqual(runtime.CurrentHp, 1,
                "Gear should never kill a live character.");
            Assert.IsFalse(runtime.IsDefeated);
        }

        // ==================================================================
        // Interaction with level-up
        // ==================================================================

        [Test]
        public void LevelUp_KeepsGearBonusesActive()
        {
            var inv = new GearInventory();
            var runtime = BuildRuntimeWiredTo(inv, _tuning);

            var sword = new GearItem(new GearTuning { Id = "sword", Slot = GearSlot.Weapon,
                Modifiers = new GearStatModifiers { PctAttack = 0.50f } });
            inv.AddItem(sword);
            inv.Equip(sword.InstanceId);

            // L1: (10 * 1.5) = 15
            Assert.AreEqual(15f, runtime.CurrentAttack, 0.0001f);

            runtime.AwardXp(100); // L1 -> L2

            // L2 raw = 10 + 1 = 11; with +50% = 16.5
            Assert.AreEqual(2, runtime.Level);
            Assert.AreEqual(16.5f, runtime.CurrentAttack, 0.0001f);
        }

        [Test]
        public void DefeatedCharacter_IgnoresSetGearModifiers()
        {
            var runtime = new CharacterRuntime(_tuning);
            runtime.TakeEnemyDamage(9999f);
            // Push past 1 HP: take another hit to actually defeat.
            // (Chip damage leaves us at 1; hit again.)
            while (!runtime.IsDefeated && runtime.CurrentHp > 0)
                runtime.TakeEnemyDamage(9999f);

            Assert.IsTrue(runtime.IsDefeated);
            int maxBefore = runtime.MaxHp;

            runtime.SetGearModifiers(new GearStatModifiers { FlatMaxHp = 500 });

            Assert.AreEqual(maxBefore, runtime.MaxHp, "Defeated runtime should ignore gear changes.");
        }

        // ==================================================================
        // GearData bridge — smoke test via ToTuning shape
        // ==================================================================

        [Test]
        public void GearTuning_Clone_IsIndependent()
        {
            var original = new GearTuning
            {
                Id = "x", DisplayName = "X",
                Slot = GearSlot.Weapon,
                Rarity = GearRarity.Epic,
                Modifiers = new GearStatModifiers { FlatAttack = 10f },
            };
            var copy = original.Clone();
            copy.DisplayName = "Y";
            copy.Modifiers = new GearStatModifiers { FlatAttack = 999f };

            Assert.AreEqual("X", original.DisplayName);
            Assert.AreEqual(10f, original.Modifiers.FlatAttack, 0.0001f);
        }
    }
}
