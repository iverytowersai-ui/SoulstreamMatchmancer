using NUnit.Framework;
using Matchmancer.Boosters;

namespace Matchmancer.Tests
{
    /// <summary>
    /// Integration-style tests that exercise BoosterInventory across realistic
    /// flows: shop purchase → inventory → battle use → snapshot → reload.
    /// Headless — no MonoBehaviour, runs in EditMode.
    /// </summary>
    public class BoosterIntegrationTests
    {
        private BoosterInventory _inv;

        [SetUp]
        public void SetUp()
        {
            _inv = new BoosterInventory();
            _inv.RegisterDefinition(new BoosterDefinition("extra_moves", BoosterType.ExtraMoves, 5)
                { UseContext = BoosterUseContext.OutOfBattle, MaxStack = 10, ShopPrice = 100 });
            _inv.RegisterDefinition(new BoosterDefinition("hammer", BoosterType.Hammer, 1)
                { UseContext = BoosterUseContext.InBattle, MaxStack = 20, ShopPrice = 50 });
            _inv.RegisterDefinition(new BoosterDefinition("shuffle", BoosterType.Shuffle, 1)
                { UseContext = BoosterUseContext.InBattle, MaxStack = 5, ShopPrice = 75 });
            _inv.RegisterDefinition(new BoosterDefinition("heal", BoosterType.Heal, 50)
                { UseContext = BoosterUseContext.Anywhere, MaxStack = 10, ShopPrice = 60 });
            _inv.RegisterDefinition(new BoosterDefinition("ult", BoosterType.EnergySurge, 1)
                { UseContext = BoosterUseContext.InBattle, MaxStack = 3, ShopPrice = 200 });
        }

        [Test]
        public void ShopPurchaseFlow_PopulatesInventory()
        {
            _inv.Purchase("hammer", 5);
            _inv.Purchase("shuffle", 2);
            _inv.Purchase("heal", 3);
            Assert.AreEqual(5, _inv.GetCount("hammer"));
            Assert.AreEqual(2, _inv.GetCount("shuffle"));
            Assert.AreEqual(3, _inv.GetCount("heal"));
            Assert.AreEqual(10, _inv.LifetimePurchased);
        }

        [Test]
        public void BattleFlow_PreBattleAndInBattleSeparation()
        {
            _inv.Purchase("extra_moves", 1);
            _inv.Purchase("hammer", 3);

            Assert.IsTrue (_inv.Use("extra_moves", BoosterUseContext.OutOfBattle));
            Assert.IsFalse(_inv.Use("hammer", BoosterUseContext.OutOfBattle));

            Assert.IsFalse(_inv.Use("extra_moves", BoosterUseContext.InBattle));
            Assert.IsTrue (_inv.Use("hammer", BoosterUseContext.InBattle));
            Assert.IsTrue (_inv.Use("hammer", BoosterUseContext.InBattle));
            Assert.AreEqual(1, _inv.GetCount("hammer"));
        }

        [Test]
        public void HealBoosterAnywhere_BothContextsConsume()
        {
            _inv.Purchase("heal", 2);
            Assert.IsTrue(_inv.Use("heal", BoosterUseContext.OutOfBattle));
            Assert.IsTrue(_inv.Use("heal", BoosterUseContext.InBattle));
            Assert.AreEqual(0, _inv.GetCount("heal"));
            Assert.AreEqual(2, _inv.LifetimeUsed);
        }

        [Test]
        public void SaveLoad_RoundTripPreservesEverything()
        {
            _inv.Purchase("hammer", 4);
            _inv.Purchase("shuffle", 1);
            _inv.Use("hammer", BoosterUseContext.InBattle);

            var snap = _inv.CreateSnapshot();

            var fresh = new BoosterInventory();
            foreach (var kv in _inv.Definitions)
                fresh.RegisterDefinition(kv.Value.Clone());
            fresh.LoadFromSnapshot(snap);

            Assert.AreEqual(3, fresh.GetCount("hammer"));
            Assert.AreEqual(1, fresh.GetCount("shuffle"));
            Assert.AreEqual(1, fresh.LifetimeUsed);
            Assert.AreEqual(5, fresh.LifetimePurchased);
        }

        [Test]
        public void DefinitionRemoved_LifetimeStatsPreserved()
        {
            _inv.Purchase("hammer", 3);
            var snap = _inv.CreateSnapshot();

            var fresh = new BoosterInventory();
            fresh.RegisterDefinition(new BoosterDefinition("shuffle", BoosterType.Shuffle));
            fresh.LoadFromSnapshot(snap);

            Assert.AreEqual(0, fresh.GetCount("hammer"));
            Assert.AreEqual(3, fresh.LifetimePurchased);
        }

        [Test]
        public void EventReplay_AllConsumersFireOncePerAction()
        {
            int added = 0, used = 0, changed = 0;
            _inv.OnBoosterAdded += (d, a) => added++;
            _inv.OnBoosterUsed  += (d, a) => used++;
            _inv.OnCountChanged += (d, o, n) => changed++;

            _inv.Purchase("hammer", 1);
            _inv.Purchase("hammer", 1);
            _inv.Use("hammer", BoosterUseContext.InBattle);

            Assert.AreEqual(2, added);
            Assert.AreEqual(1, used);
            Assert.AreEqual(3, changed);
        }

        [Test]
        public void MaxStack_PreventsOverPurchase()
        {
            int firstAdd  = _inv.Purchase("shuffle", 3);
            int secondAdd = _inv.Purchase("shuffle", 5);
            Assert.AreEqual(3, firstAdd);
            Assert.AreEqual(2, secondAdd);
            Assert.AreEqual(5, _inv.GetCount("shuffle"));
            Assert.AreEqual(5, _inv.LifetimePurchased);
        }

        [Test]
        public void LifetimeStats_StableAcrossManyOperations()
        {
            for (int i = 0; i < 10; i++) _inv.Purchase("hammer", 1);
            for (int i = 0; i < 7;  i++) _inv.Use("hammer", BoosterUseContext.InBattle);
            Assert.AreEqual(3, _inv.GetCount("hammer"));
            Assert.AreEqual(10, _inv.LifetimePurchased);
            Assert.AreEqual(7,  _inv.LifetimeUsed);
        }

        [Test]
        public void ClearAll_LeavesLifetimeStatsAlone()
        {
            _inv.Purchase("hammer", 4);
            _inv.Use("hammer", BoosterUseContext.InBattle);
            _inv.ClearAll();
            Assert.AreEqual(0, _inv.GetCount("hammer"));
            Assert.AreEqual(4, _inv.LifetimePurchased);
            Assert.AreEqual(1, _inv.LifetimeUsed);
        }

        [Test]
        public void MultipleBoosterTypes_IndependentTracking()
        {
            _inv.Purchase("hammer", 3);
            _inv.Purchase("shuffle", 2);
            _inv.Purchase("ult", 1);

            _inv.Use("hammer", BoosterUseContext.InBattle);
            _inv.Use("ult", BoosterUseContext.InBattle);

            Assert.AreEqual(2, _inv.GetCount("hammer"));
            Assert.AreEqual(2, _inv.GetCount("shuffle"));
            Assert.AreEqual(0, _inv.GetCount("ult"));
            Assert.AreEqual(2, _inv.LifetimeUsed);
        }
    }
}
