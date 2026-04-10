using NUnit.Framework;
using Matchmancer.Shop;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class WalletTests
    {
        private Wallet _wallet;

        [SetUp]
        public void SetUp()
        {
            _wallet = new Wallet();
        }

        // ==================================================================
        // Initial state
        // ==================================================================

        [Test]
        public void NewWallet_ZeroBalance()
        {
            Assert.AreEqual(0, _wallet.Gold);
            Assert.AreEqual(0, _wallet.LifetimeGoldEarned);
            Assert.AreEqual(0, _wallet.LifetimeGoldSpent);
        }

        // ==================================================================
        // Earn
        // ==================================================================

        [Test]
        public void Earn_IncreasesBalance()
        {
            _wallet.Earn(100);
            Assert.AreEqual(100, _wallet.Gold);
            Assert.AreEqual(100, _wallet.LifetimeGoldEarned);
        }

        [Test]
        public void Earn_MultipleAdds()
        {
            _wallet.Earn(50);
            _wallet.Earn(75);
            Assert.AreEqual(125, _wallet.Gold);
        }

        [Test]
        public void Earn_Zero_NoOp()
        {
            int calls = 0;
            _wallet.OnGoldChanged += (_, _2, _3) => calls++;
            _wallet.Earn(0);
            Assert.AreEqual(0, _wallet.Gold);
            Assert.AreEqual(0, calls);
        }

        [Test]
        public void Earn_Negative_NoOp()
        {
            _wallet.Earn(-50);
            Assert.AreEqual(0, _wallet.Gold);
        }

        [Test]
        public void Earn_FiresEvent()
        {
            long seenOld = -1, seenNew = -1, seenDelta = 0;
            _wallet.OnGoldChanged += (o, n, d) => { seenOld = o; seenNew = n; seenDelta = d; };
            _wallet.Earn(200);
            Assert.AreEqual(0,   seenOld);
            Assert.AreEqual(200, seenNew);
            Assert.AreEqual(200, seenDelta);
        }

        // ==================================================================
        // Spend
        // ==================================================================

        [Test]
        public void Spend_DeductsBalance()
        {
            _wallet.Earn(500);
            bool ok = _wallet.Spend(200);
            Assert.IsTrue(ok);
            Assert.AreEqual(300, _wallet.Gold);
            Assert.AreEqual(200, _wallet.LifetimeGoldSpent);
        }

        [Test]
        public void Spend_ExactBalance_Succeeds()
        {
            _wallet.Earn(100);
            Assert.IsTrue(_wallet.Spend(100));
            Assert.AreEqual(0, _wallet.Gold);
        }

        [Test]
        public void Spend_InsufficientFunds_Fails()
        {
            _wallet.Earn(50);
            Assert.IsFalse(_wallet.Spend(100));
            Assert.AreEqual(50, _wallet.Gold, "Balance must not change on failure.");
        }

        [Test]
        public void Spend_Zero_AlwaysSucceeds()
        {
            Assert.IsTrue(_wallet.Spend(0));
        }

        [Test]
        public void Spend_Negative_Fails()
        {
            _wallet.Earn(100);
            Assert.IsFalse(_wallet.Spend(-10));
        }

        [Test]
        public void Spend_FiresEvent()
        {
            _wallet.Earn(300);
            long seenDelta = 0;
            _wallet.OnGoldChanged += (_, _2, d) => seenDelta = d;
            _wallet.Spend(120);
            Assert.AreEqual(-120, seenDelta);
        }

        // ==================================================================
        // CanAfford
        // ==================================================================

        [Test]
        public void CanAfford_True_WhenEnough()
        {
            _wallet.Earn(500);
            Assert.IsTrue(_wallet.CanAfford(500));
            Assert.IsTrue(_wallet.CanAfford(1));
        }

        [Test]
        public void CanAfford_False_WhenNot()
        {
            _wallet.Earn(50);
            Assert.IsFalse(_wallet.CanAfford(51));
        }

        [Test]
        public void CanAfford_Negative_False()
        {
            _wallet.Earn(100);
            Assert.IsFalse(_wallet.CanAfford(-1));
        }

        // ==================================================================
        // Snapshot
        // ==================================================================

        [Test]
        public void Snapshot_RoundTrips()
        {
            _wallet.Earn(1000);
            _wallet.Spend(250);
            var snap = _wallet.CreateSnapshot();

            var loaded = new Wallet();
            loaded.LoadFromSnapshot(snap);

            Assert.AreEqual(750,  loaded.Gold);
            Assert.AreEqual(1000, loaded.LifetimeGoldEarned);
            Assert.AreEqual(250,  loaded.LifetimeGoldSpent);
        }

        [Test]
        public void Snapshot_NullNoOp()
        {
            _wallet.Earn(100);
            _wallet.LoadFromSnapshot(null);
            Assert.AreEqual(100, _wallet.Gold, "Null snapshot should be a no-op.");
        }

        [Test]
        public void SetBalanceRaw_ClampsNegative()
        {
            _wallet.SetBalanceRaw(-999);
            Assert.AreEqual(0, _wallet.Gold);
        }
    }
}
