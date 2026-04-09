using System.Collections.Generic;
using NUnit.Framework;
using Matchmancer.Character;

namespace Matchmancer.Tests
{
    /// <summary>
    /// Pure C# tests for <see cref="GearInventory"/> + <see cref="GearStatModifiers"/>.
    /// No Unity, no scene, no ScriptableObject — construction is by POCO only.
    /// </summary>
    [TestFixture]
    public class GearInventoryTests
    {
        // ------------------------------------------------------------------
        // Test fixtures
        // ------------------------------------------------------------------

        private static GearItem MakeItem(
            string id,
            GearSlot slot,
            int flatHp = 0, float flatAtk = 0f, float flatDef = 0f, float flatLuck = 0f,
            float pctHp = 0f, float pctAtk = 0f, float pctDef = 0f, float pctLuck = 0f,
            GearRarity rarity = GearRarity.Common)
        {
            var tuning = new GearTuning
            {
                Id          = id,
                DisplayName = id,
                Slot        = slot,
                Rarity      = rarity,
                Modifiers   = new GearStatModifiers
                {
                    FlatMaxHp   = flatHp,
                    FlatAttack  = flatAtk,
                    FlatDefense = flatDef,
                    FlatLuck    = flatLuck,
                    PctMaxHp    = pctHp,
                    PctAttack   = pctAtk,
                    PctDefense  = pctDef,
                    PctLuck     = pctLuck,
                },
            };
            return new GearItem(tuning, instanceId: id + "_inst");
        }

        // ==================================================================
        // GearStatModifiers
        // ==================================================================

        [Test]
        public void Modifiers_Zero_IsEmpty()
        {
            var zero = GearStatModifiers.Zero;
            Assert.IsTrue(zero.IsZero);
        }

        [Test]
        public void Modifiers_Add_CombinesFlatAndPct()
        {
            var a = new GearStatModifiers { FlatAttack = 5f, PctAttack = 0.10f };
            var b = new GearStatModifiers { FlatAttack = 3f, PctAttack = 0.05f };
            var sum = a + b;
            Assert.AreEqual(8f,    sum.FlatAttack, 0.0001f);
            Assert.AreEqual(0.15f, sum.PctAttack,  0.0001f);
        }

        [Test]
        public void Modifiers_ApplyToMaxHp_PctThenFlat()
        {
            var m = new GearStatModifiers { PctMaxHp = 0.20f, FlatMaxHp = 10 };
            // 100 * 1.20 = 120  + 10 = 130
            Assert.AreEqual(130, m.ApplyToMaxHp(100));
        }

        [Test]
        public void Modifiers_ApplyToAttack_NeverNegative()
        {
            var m = new GearStatModifiers { FlatAttack = -999f };
            Assert.AreEqual(0f, m.ApplyToAttack(10f));
        }

        [Test]
        public void Modifiers_MaxHpFloor_AtLeastOne()
        {
            var m = new GearStatModifiers { FlatMaxHp = -999 };
            Assert.AreEqual(1, m.ApplyToMaxHp(100));
        }

        // ==================================================================
        // Ownership
        // ==================================================================

        [Test]
        public void NewInventory_Empty()
        {
            var inv = new GearInventory();
            Assert.AreEqual(0, inv.OwnedCount);
            Assert.IsTrue(inv.TotalEquippedModifiers.IsZero);
            foreach (GearSlot slot in System.Enum.GetValues(typeof(GearSlot)))
                Assert.IsNull(inv.GetEquipped(slot));
        }

        [Test]
        public void AddItem_IncrementsOwnedAndFiresEvent()
        {
            var inv = new GearInventory();
            GearItem added = null;
            inv.OnItemAdded += i => added = i;

            var sword = MakeItem("sword", GearSlot.Weapon, flatAtk: 5f);
            inv.AddItem(sword);

            Assert.AreEqual(1, inv.OwnedCount);
            Assert.AreSame(sword, added);
            Assert.IsTrue(inv.Owns(sword.InstanceId));
        }

        [Test]
        public void AddItem_Duplicate_Throws()
        {
            var inv = new GearInventory();
            var sword = MakeItem("sword", GearSlot.Weapon);
            inv.AddItem(sword);
            Assert.Throws<System.InvalidOperationException>(() => inv.AddItem(sword));
        }

        [Test]
        public void AddItem_Null_Throws()
        {
            var inv = new GearInventory();
            Assert.Throws<System.ArgumentNullException>(() => inv.AddItem(null));
        }

        [Test]
        public void RemoveItem_UnequipsAndFiresEvent()
        {
            var inv = new GearInventory();
            var sword = MakeItem("sword", GearSlot.Weapon, flatAtk: 10f);
            inv.AddItem(sword);
            inv.Equip(sword.InstanceId);

            bool removedFired = false;
            inv.OnItemRemoved += _ => removedFired = true;
            int modChangeCount = 0;
            inv.OnModifiersChanged += _ => modChangeCount++;

            bool ok = inv.RemoveItem(sword.InstanceId);

            Assert.IsTrue(ok);
            Assert.IsTrue(removedFired);
            Assert.AreEqual(0, inv.OwnedCount);
            Assert.IsNull(inv.GetEquipped(GearSlot.Weapon));
            Assert.AreEqual(1, modChangeCount, "Unequip-before-remove must fire modifier change exactly once.");
        }

        [Test]
        public void RemoveItem_Unknown_ReturnsFalse()
        {
            var inv = new GearInventory();
            Assert.IsFalse(inv.RemoveItem("ghost"));
            Assert.IsFalse(inv.RemoveItem(null));
        }

        // ==================================================================
        // Equip / unequip
        // ==================================================================

        [Test]
        public void Equip_NotOwned_Throws()
        {
            var inv = new GearInventory();
            Assert.Throws<System.InvalidOperationException>(() => inv.Equip("nope"));
        }

        [Test]
        public void Equip_FiresEquippedAndModifiersChanged()
        {
            var inv = new GearInventory();
            var sword = MakeItem("sword", GearSlot.Weapon, flatAtk: 7f);
            inv.AddItem(sword);

            GearSlot? equippedSlot = null;
            GearItem  equippedItem = null;
            inv.OnItemEquipped += (s, i) => { equippedSlot = s; equippedItem = i; };
            int modCalls = 0;
            GearStatModifiers lastMods = default;
            inv.OnModifiersChanged += m => { modCalls++; lastMods = m; };

            inv.Equip(sword.InstanceId);

            Assert.AreEqual(GearSlot.Weapon, equippedSlot);
            Assert.AreSame(sword, equippedItem);
            Assert.AreEqual(1, modCalls);
            Assert.AreEqual(7f, lastMods.FlatAttack);
            Assert.AreSame(sword, inv.GetEquipped(GearSlot.Weapon));
            Assert.IsTrue(inv.IsEquipped(sword.InstanceId));
        }

        [Test]
        public void Equip_SameItemTwice_IsNoOp()
        {
            var inv = new GearInventory();
            var sword = MakeItem("sword", GearSlot.Weapon, flatAtk: 5f);
            inv.AddItem(sword);
            inv.Equip(sword.InstanceId);

            int equipCount = 0;
            int modChangeCount = 0;
            inv.OnItemEquipped    += (_, _2) => equipCount++;
            inv.OnModifiersChanged += _ => modChangeCount++;

            inv.Equip(sword.InstanceId); // same again

            Assert.AreEqual(0, equipCount);
            Assert.AreEqual(0, modChangeCount);
        }

        [Test]
        public void Equip_SwapsPreviousItemInSameSlot()
        {
            var inv = new GearInventory();
            var rustBlade = MakeItem("rust",  GearSlot.Weapon, flatAtk: 3f);
            var soulBlade = MakeItem("soul",  GearSlot.Weapon, flatAtk: 9f);
            inv.AddItem(rustBlade);
            inv.AddItem(soulBlade);

            inv.Equip(rustBlade.InstanceId);

            GearSlot? unequippedSlot = null;
            GearItem  unequippedItem = null;
            inv.OnItemUnequipped += (s, i) => { unequippedSlot = s; unequippedItem = i; };

            inv.Equip(soulBlade.InstanceId);

            Assert.AreEqual(GearSlot.Weapon, unequippedSlot);
            Assert.AreSame(rustBlade, unequippedItem);
            Assert.AreSame(soulBlade, inv.GetEquipped(GearSlot.Weapon));
            Assert.IsTrue(inv.Owns(rustBlade.InstanceId), "Swapped-out item stays in the bag.");
            Assert.IsFalse(inv.IsEquipped(rustBlade.InstanceId));
        }

        [Test]
        public void Unequip_EmptySlot_IsNoOp()
        {
            var inv = new GearInventory();
            int modCalls = 0;
            inv.OnModifiersChanged += _ => modCalls++;
            var result = inv.Unequip(GearSlot.Relic);
            Assert.IsNull(result);
            Assert.AreEqual(0, modCalls);
        }

        [Test]
        public void Unequip_FiresEvents()
        {
            var inv = new GearInventory();
            var charm = MakeItem("charm", GearSlot.Talisman, flatLuck: 2f);
            inv.AddItem(charm);
            inv.Equip(charm.InstanceId);

            bool unequipFired = false;
            inv.OnItemUnequipped += (_, _2) => unequipFired = true;
            GearStatModifiers afterMods = new GearStatModifiers { FlatLuck = 99f };
            inv.OnModifiersChanged += m => afterMods = m;

            var returned = inv.Unequip(GearSlot.Talisman);

            Assert.AreSame(charm, returned);
            Assert.IsTrue(unequipFired);
            Assert.IsTrue(afterMods.IsZero, "Modifiers should zero out after last item leaves slot.");
        }

        // ==================================================================
        // Totals across multiple slots
        // ==================================================================

        [Test]
        public void TotalModifiers_SumsAcrossAllSlots()
        {
            var inv = new GearInventory();
            inv.AddItem(MakeItem("w", GearSlot.Weapon,   flatAtk: 10f, pctAtk: 0.10f));
            inv.AddItem(MakeItem("a", GearSlot.Armor,    flatDef: 5f,  flatHp: 20));
            inv.AddItem(MakeItem("t", GearSlot.Talisman, flatLuck: 3f, pctMaxHp: 0.05f));
            inv.AddItem(MakeItem("r", GearSlot.Relic,    flatAtk: 2f,  pctLuck: 0.20f));

            inv.Equip("w_inst");
            inv.Equip("a_inst");
            inv.Equip("t_inst");
            inv.Equip("r_inst");

            var total = inv.TotalEquippedModifiers;
            Assert.AreEqual(12f,   total.FlatAttack,  0.0001f);
            Assert.AreEqual(0.10f, total.PctAttack,   0.0001f);
            Assert.AreEqual(5f,    total.FlatDefense, 0.0001f);
            Assert.AreEqual(20,    total.FlatMaxHp);
            Assert.AreEqual(3f,    total.FlatLuck,    0.0001f);
            Assert.AreEqual(0.05f, total.PctMaxHp,    0.0001f);
            Assert.AreEqual(0.20f, total.PctLuck,     0.0001f);
        }

        [Test]
        public void UnequipAll_ClearsEverySlotAndFiresModifiersOnce()
        {
            var inv = new GearInventory();
            inv.AddItem(MakeItem("w", GearSlot.Weapon, flatAtk: 1f));
            inv.AddItem(MakeItem("a", GearSlot.Armor,  flatDef: 1f));
            inv.Equip("w_inst");
            inv.Equip("a_inst");

            int modCalls = 0;
            inv.OnModifiersChanged += _ => modCalls++;

            inv.UnequipAll();

            Assert.IsNull(inv.GetEquipped(GearSlot.Weapon));
            Assert.IsNull(inv.GetEquipped(GearSlot.Armor));
            Assert.IsTrue(inv.TotalEquippedModifiers.IsZero);
            Assert.AreEqual(1, modCalls, "UnequipAll should batch modifier changes into one event.");
        }

        [Test]
        public void UnequipAll_EmptyInventory_FiresNothing()
        {
            var inv = new GearInventory();
            int modCalls = 0;
            inv.OnModifiersChanged += _ => modCalls++;
            inv.UnequipAll();
            Assert.AreEqual(0, modCalls);
        }
    }
}
