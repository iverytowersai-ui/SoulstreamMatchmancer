using NUnit.Framework;
using Matchmancer.Boosters;

namespace Matchmancer.Tests
{
    public class BoosterInventoryTests
    {
        private static BoosterDefinition Make(
            string id,
            BoosterType type = BoosterType.ExtraMoves,
            BoosterUseContext ctx = BoosterUseContext.InBattle,
            int magnitude = 1,
            int maxStack = 99,
            int price = 0)
        {
            return new BoosterDefinition
            {
                Id              = id,
                DisplayName     = id,
                Type            = type,
                UseContext      = ctx,
                EffectMagnitude = magnitude,
                MaxStack        = maxStack,
                ShopPrice       = price,
            };
        }

        [Test]
        public void RegisterDefinition_NullThrows()
        {
            var inv = new BoosterInventory();
            Assert.Throws<System.ArgumentNullException>(() => inv.RegisterDefinition(null));
        }

        [Test]
        public void RegisterDefinition_EmptyIdThrows()
        {
            var inv = new BoosterInventory();
            Assert.Throws<System.ArgumentException>(() => inv.RegisterDefinition(Make("")));
        }

        [Test]
        public void RegisterDefinition_FiresEvent()
        {
            var inv = new BoosterInventory();
            BoosterDefinition seen = null;
            inv.OnBoosterRegistered += d => seen = d;
            var def = Make("hammer", BoosterType.Hammer);
            inv.RegisterDefinition(def);
            Assert.AreSame(def, seen);
            Assert.IsTrue(inv.HasDefinition("hammer"));
            Assert.AreEqual(0, inv.GetCount("hammer"));
        }

        [Test]
        public void HasDefinition_HandlesNullAndUnknown()
        {
            var inv = new BoosterInventory();
            Assert.IsFalse(inv.HasDefinition(null));
            Assert.IsFalse(inv.HasDefinition(""));
            Assert.IsFalse(inv.HasDefinition("ghost"));
        }

        [Test]
        public void Add_UnknownIdThrows()
        {
            var inv = new BoosterInventory();
            Assert.Throws<System.InvalidOperationException>(() => inv.Add("ghost", 1));
        }

        [Test]
        public void Add_NegativeOrZero_NoOp()
        {
            var inv = new BoosterInventory();
            inv.RegisterDefinition(Make("h"));
            Assert.AreEqual(0, inv.Add("h", 0));
            Assert.AreEqual(0, inv.Add("h", -3));
            Assert.AreEqual(0, inv.GetCount("h"));
        }

        [Test]
        public void Add_IncrementsCountAndFiresEvents()
        {
            var inv = new BoosterInventory();
            inv.RegisterDefinition(Make("h"));
            int oldSeen = -1, newSeen = -1, addedSeen = -1;
            inv.OnCountChanged += (d, o, n) => { oldSeen = o; newSeen = n; };
            inv.OnBoosterAdded += (d, a) => addedSeen = a;

            int added = inv.Add("h", 5);
            Assert.AreEqual(5, added);
            Assert.AreEqual(5, inv.GetCount("h"));
            Assert.AreEqual(0, oldSeen);
            Assert.AreEqual(5, newSeen);
            Assert.AreEqual(5, addedSeen);
        }

        [Test]
        public void Add_ClampsToMaxStack()
        {
            var inv = new BoosterInventory();
            inv.RegisterDefinition(Make("h", maxStack: 3));
            Assert.AreEqual(3, inv.Add("h", 10));
            Assert.AreEqual(3, inv.GetCount("h"));
            Assert.AreEqual(0, inv.Add("h", 5));
        }

        [Test]
        public void Add_ZeroMaxStackMeansUnlimited()
        {
            var inv = new BoosterInventory();
            inv.RegisterDefinition(Make("h", maxStack: 0));
            Assert.AreEqual(1000, inv.Add("h", 1000));
            Assert.AreEqual(1000, inv.GetCount("h"));
        }

        [Test]
        public void Use_DecrementsAndFiresEvents()
        {
            var inv = new BoosterInventory();
            inv.RegisterDefinition(Make("h", BoosterType.Hammer));
            inv.Add("h", 2);

            int usedSeen = 0;
            inv.OnBoosterUsed += (d, a) => usedSeen += a;

            Assert.IsTrue(inv.Use("h", BoosterUseContext.InBattle));
            Assert.AreEqual(1, inv.GetCount("h"));
            Assert.AreEqual(1, inv.LifetimeUsed);
            Assert.AreEqual(1, usedSeen);
        }

        [Test]
        public void Use_FailsWhenEmpty()
        {
            var inv = new BoosterInventory();
            inv.RegisterDefinition(Make("h"));
            Assert.IsFalse(inv.Use("h", BoosterUseContext.InBattle));
            Assert.AreEqual(0, inv.LifetimeUsed);
        }

        [Test]
        public void Use_FailsForUnknownId()
        {
            var inv = new BoosterInventory();
            Assert.IsFalse(inv.Use("ghost", BoosterUseContext.InBattle));
        }

        [Test]
        public void Use_RespectsContext()
        {
            var inv = new BoosterInventory();
            inv.RegisterDefinition(Make("hammer", BoosterType.Hammer, BoosterUseContext.InBattle));
            inv.Add("hammer", 1);

            Assert.IsFalse(inv.Use("hammer", BoosterUseContext.OutOfBattle));
            Assert.AreEqual(1, inv.GetCount("hammer"));

            Assert.IsTrue(inv.Use("hammer", BoosterUseContext.InBattle));
            Assert.AreEqual(0, inv.GetCount("hammer"));
        }

        [Test]
        public void Use_AnywhereContextWorksInBoth()
        {
            var inv = new BoosterInventory();
            inv.RegisterDefinition(Make("heal", BoosterType.Heal, BoosterUseContext.Anywhere));
            inv.Add("heal", 2);
            Assert.IsTrue(inv.Use("heal", BoosterUseContext.OutOfBattle));
            Assert.IsTrue(inv.Use("heal", BoosterUseContext.InBattle));
            Assert.AreEqual(0, inv.GetCount("heal"));
        }

        [Test]
        public void CanUse_ReportsCorrectly()
        {
            var inv = new BoosterInventory();
            inv.RegisterDefinition(Make("h", BoosterType.Hammer, BoosterUseContext.InBattle));
            Assert.IsFalse(inv.CanUse("h", BoosterUseContext.InBattle));
            inv.Add("h", 1);
            Assert.IsTrue(inv.CanUse("h", BoosterUseContext.InBattle));
            Assert.IsFalse(inv.CanUse("h", BoosterUseContext.OutOfBattle));
        }

        [Test]
        public void Purchase_AddsAndIncrementsLifetime()
        {
            var inv = new BoosterInventory();
            inv.RegisterDefinition(Make("h"));
            inv.Purchase("h", 3);
            Assert.AreEqual(3, inv.GetCount("h"));
            Assert.AreEqual(3, inv.LifetimePurchased);
        }

        [Test]
        public void Purchase_RespectsMaxStack_OnlyCountsAdded()
        {
            var inv = new BoosterInventory();
            inv.RegisterDefinition(Make("h", maxStack: 2));
            inv.Purchase("h", 5);
            Assert.AreEqual(2, inv.GetCount("h"));
            Assert.AreEqual(2, inv.LifetimePurchased);
        }

        [Test]
        public void GetCountByType_SumsAcrossDefinitions()
        {
            var inv = new BoosterInventory();
            inv.RegisterDefinition(Make("h1", BoosterType.Hammer));
            inv.RegisterDefinition(Make("h2", BoosterType.Hammer));
            inv.RegisterDefinition(Make("b1", BoosterType.Bomb));
            inv.Add("h1", 2);
            inv.Add("h2", 3);
            inv.Add("b1", 4);
            Assert.AreEqual(5, inv.GetCount(BoosterType.Hammer));
            Assert.AreEqual(4, inv.GetCount(BoosterType.Bomb));
        }

        [Test]
        public void SetCountRaw_ClampsAndFiresOnCountChangedOnly()
        {
            var inv = new BoosterInventory();
            inv.RegisterDefinition(Make("h", maxStack: 5));

            int addedFires = 0, usedFires = 0, changedFires = 0;
            inv.OnBoosterAdded += (d, a) => addedFires++;
            inv.OnBoosterUsed  += (d, a) => usedFires++;
            inv.OnCountChanged += (d, o, n) => changedFires++;

            inv.SetCountRaw("h", 100);
            Assert.AreEqual(5, inv.GetCount("h"));
            Assert.AreEqual(0, addedFires);
            Assert.AreEqual(0, usedFires);
            Assert.AreEqual(1, changedFires);

            inv.SetCountRaw("h", -10);
            Assert.AreEqual(0, inv.GetCount("h"));
        }

        [Test]
        public void SetCountRaw_NoChangeNoEvent()
        {
            var inv = new BoosterInventory();
            inv.RegisterDefinition(Make("h"));
            int fires = 0;
            inv.OnCountChanged += (d, o, n) => fires++;
            inv.SetCountRaw("h", 0);
            Assert.AreEqual(0, fires);
        }

        [Test]
        public void ClearAll_ZeroesEverything()
        {
            var inv = new BoosterInventory();
            inv.RegisterDefinition(Make("a"));
            inv.RegisterDefinition(Make("b"));
            inv.Add("a", 4);
            inv.Add("b", 7);
            inv.ClearAll();
            Assert.AreEqual(0, inv.GetCount("a"));
            Assert.AreEqual(0, inv.GetCount("b"));
        }

        [Test]
        public void Snapshot_RoundTripsCountsAndLifetime()
        {
            var inv = new BoosterInventory();
            inv.RegisterDefinition(Make("h"));
            inv.RegisterDefinition(Make("b", BoosterType.Bomb));
            inv.Purchase("h", 3);
            inv.Add("b", 2);
            inv.Use("h", BoosterUseContext.InBattle);

            var snap = inv.CreateSnapshot();

            var inv2 = new BoosterInventory();
            inv2.RegisterDefinition(Make("h"));
            inv2.RegisterDefinition(Make("b", BoosterType.Bomb));
            inv2.LoadFromSnapshot(snap);

            Assert.AreEqual(2, inv2.GetCount("h"));
            Assert.AreEqual(2, inv2.GetCount("b"));
            Assert.AreEqual(1, inv2.LifetimeUsed);
            Assert.AreEqual(3, inv2.LifetimePurchased);
        }

        [Test]
        public void Snapshot_NullLoadIsNoOp()
        {
            var inv = new BoosterInventory();
            inv.RegisterDefinition(Make("h"));
            inv.Add("h", 4);
            inv.LoadFromSnapshot(null);
            Assert.AreEqual(4, inv.GetCount("h"));
        }

        [Test]
        public void Snapshot_DropsUnknownIdsOnLoad()
        {
            var snap = new BoosterInventorySnapshot
            {
                BoosterIds = new[] { "h", "ghost" },
                Counts     = new[] { 3, 99 },
            };
            var inv = new BoosterInventory();
            inv.RegisterDefinition(Make("h"));
            inv.LoadFromSnapshot(snap);
            Assert.AreEqual(3, inv.GetCount("h"));
            Assert.AreEqual(0, inv.GetCount("ghost"));
        }

        [Test]
        public void Snapshot_ExcludesZeroCounts()
        {
            var inv = new BoosterInventory();
            inv.RegisterDefinition(Make("a"));
            inv.RegisterDefinition(Make("b"));
            inv.Add("a", 2);
            var snap = inv.CreateSnapshot();
            Assert.AreEqual(1, snap.BoosterIds.Length);
            Assert.AreEqual("a", snap.BoosterIds[0]);
            Assert.AreEqual(2, snap.Counts[0]);
        }
    }
}
