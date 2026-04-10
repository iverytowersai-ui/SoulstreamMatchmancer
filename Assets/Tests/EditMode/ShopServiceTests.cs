using NUnit.Framework;
using Matchmancer.Shop;
using Matchmancer.Boosters;
using Matchmancer.Character;

namespace Matchmancer.Tests
{
    /// <summary>
    /// Pure C# tests for <see cref="ShopService"/> + <see cref="ShopCatalog"/>.
    /// Wires a live <see cref="Wallet"/>, <see cref="BoosterInventory"/>,
    /// and <see cref="GearInventory"/> so grants are verifiable.
    /// </summary>
    [TestFixture]
    public class ShopServiceTests
    {
        private Wallet           _wallet;
        private BoosterInventory _boosters;
        private GearInventory    _gear;
        private ShopCatalog      _catalog;
        private ShopService      _service;

        [SetUp]
        public void SetUp()
        {
            _wallet   = new Wallet();
            _boosters = new BoosterInventory();
            _gear     = new GearInventory();
            _catalog  = new ShopCatalog();

            // Register one booster definition the shop can sell
            _boosters.RegisterDefinition(new BoosterDefinition("heal_pot", BoosterType.Heal, 50)
            {
                DisplayName = "Heal Potion",
                MaxStack    = 99,
                ShopPrice   = 100,
            });

            _service = new ShopService(_catalog, _wallet, _boosters, _gear);
            _service.GearTuningResolver = id =>
            {
                if (id == "rust_blade")
                    return new GearTuning
                    {
                        Id          = "rust_blade",
                        DisplayName = "Rust Blade",
                        Slot        = GearSlot.Weapon,
                        Modifiers   = new GearStatModifiers { FlatAttack = 5f },
                    };
                return null;
            };

            // Seed gold
            _wallet.Earn(1000);
        }

        private void AddBoosterListing(string id = "shop_heal", int price = 100, int qty = 1, int limit = 0)
        {
            _catalog.Register(new ShopItemDefinition
            {
                Id       = id,
                Type     = ShopItemType.Booster,
                RefId    = "heal_pot",
                Quantity = qty,
                Price    = price,
                PurchaseLimit = limit,
            });
        }

        private void AddGearListing(string id = "shop_blade", int price = 200, int limit = 0)
        {
            _catalog.Register(new ShopItemDefinition
            {
                Id       = id,
                Type     = ShopItemType.Gear,
                RefId    = "rust_blade",
                Quantity = 1,
                Price    = price,
                PurchaseLimit = limit,
            });
        }

        private void AddGoldListing(string id = "shop_gold_pack", int price = 0, int qty = 500)
        {
            _catalog.Register(new ShopItemDefinition
            {
                Id       = id,
                Type     = ShopItemType.Gold,
                Quantity = qty,
                Price    = price,
            });
        }

        // ==================================================================
        // Successful purchases
        // ==================================================================

        [Test]
        public void BuyBooster_DeductsGoldAndGrantsItem()
        {
            AddBoosterListing();
            var result = _service.TryPurchase("shop_heal");
            Assert.AreEqual(PurchaseResult.Success, result);
            Assert.AreEqual(900, _wallet.Gold);
            Assert.AreEqual(1, _boosters.GetCount("heal_pot"));
        }

        [Test]
        public void BuyBoosterPack_GrantsQuantity()
        {
            AddBoosterListing(qty: 5, price: 400);
            _service.TryPurchase("shop_heal");
            Assert.AreEqual(5, _boosters.GetCount("heal_pot"));
        }

        [Test]
        public void BuyGear_DeductsGoldAndAddsToInventory()
        {
            AddGearListing();
            var result = _service.TryPurchase("shop_blade");
            Assert.AreEqual(PurchaseResult.Success, result);
            Assert.AreEqual(800, _wallet.Gold);
            Assert.AreEqual(1, _gear.OwnedCount);
        }

        [Test]
        public void BuyGold_AddsToWallet()
        {
            AddGoldListing(price: 0, qty: 500);
            var result = _service.TryPurchase("shop_gold_pack");
            Assert.AreEqual(PurchaseResult.Success, result);
            // Started at 1000, earned 500 more, price was 0
            Assert.AreEqual(1500, _wallet.Gold);
        }

        // ==================================================================
        // Failure cases
        // ==================================================================

        [Test]
        public void BuyUnknownListing_ReturnsItemNotFound()
        {
            Assert.AreEqual(PurchaseResult.ItemNotFound, _service.TryPurchase("ghost"));
        }

        [Test]
        public void InsufficientFunds_ReturnsInsufficientFunds()
        {
            AddBoosterListing(price: 9999);
            var result = _service.TryPurchase("shop_heal");
            Assert.AreEqual(PurchaseResult.InsufficientFunds, result);
            Assert.AreEqual(1000, _wallet.Gold, "Gold must not change on failure.");
        }

        [Test]
        public void PurchaseLimit_ReturnsLimitReached()
        {
            AddBoosterListing(limit: 1);
            _service.TryPurchase("shop_heal");
            var second = _service.TryPurchase("shop_heal");
            Assert.AreEqual(PurchaseResult.LimitReached, second);
            Assert.AreEqual(1, _boosters.GetCount("heal_pot"),
                "Only the first purchase should have granted.");
        }

        [Test]
        public void GearGrantFailed_WhenResolverReturnsNull()
        {
            _catalog.Register(new ShopItemDefinition
            {
                Id    = "bad_gear",
                Type  = ShopItemType.Gear,
                RefId = "unknown_tuning_id",
                Price = 100,
            });
            var result = _service.TryPurchase("bad_gear");
            Assert.AreEqual(PurchaseResult.GrantFailed, result);
            Assert.AreEqual(1000, _wallet.Gold, "Gold must not change on grant failure.");
        }

        // ==================================================================
        // Events
        // ==================================================================

        [Test]
        public void OnPurchaseSuccess_FiresWithItem()
        {
            AddBoosterListing();
            ShopItemDefinition seen = null;
            int seenQty = 0;
            _service.OnPurchaseSuccess += (item, qty) => { seen = item; seenQty = qty; };

            _service.TryPurchase("shop_heal");

            Assert.IsNotNull(seen);
            Assert.AreEqual("shop_heal", seen.Id);
            Assert.AreEqual(1, seenQty);
        }

        [Test]
        public void OnPurchaseFailed_FiresOnInsufficientFunds()
        {
            AddBoosterListing(price: 9999);
            PurchaseResult seenReason = PurchaseResult.Success;
            _service.OnPurchaseFailed += (_, reason) => seenReason = reason;

            _service.TryPurchase("shop_heal");
            Assert.AreEqual(PurchaseResult.InsufficientFunds, seenReason);
        }

        // ==================================================================
        // Purchase count tracking
        // ==================================================================

        [Test]
        public void GetPurchaseCount_TracksPerListing()
        {
            AddBoosterListing();
            Assert.AreEqual(0, _service.GetPurchaseCount("shop_heal"));
            _service.TryPurchase("shop_heal");
            Assert.AreEqual(1, _service.GetPurchaseCount("shop_heal"));
            _service.TryPurchase("shop_heal");
            Assert.AreEqual(2, _service.GetPurchaseCount("shop_heal"));
        }

        // ==================================================================
        // Catalog
        // ==================================================================

        [Test]
        public void Catalog_GetVisibleByType_SortedBySortOrder()
        {
            _catalog.Register(new ShopItemDefinition { Id = "b", Type = ShopItemType.Booster, SortOrder = 2, RefId = "heal_pot" });
            _catalog.Register(new ShopItemDefinition { Id = "a", Type = ShopItemType.Booster, SortOrder = 1, RefId = "heal_pot" });
            _catalog.Register(new ShopItemDefinition { Id = "c", Type = ShopItemType.Gear,    SortOrder = 0, RefId = "rust_blade" });

            var boosters = _catalog.GetVisibleByType(ShopItemType.Booster);
            Assert.AreEqual(2, boosters.Count);
            Assert.AreEqual("a", boosters[0].Id);
            Assert.AreEqual("b", boosters[1].Id);
        }

        [Test]
        public void Catalog_HiddenItems_FilteredOut()
        {
            _catalog.Register(new ShopItemDefinition { Id = "vis", Type = ShopItemType.Booster, RefId = "heal_pot" });
            _catalog.Register(new ShopItemDefinition { Id = "hid", Type = ShopItemType.Booster, RefId = "heal_pot", IsHidden = true });

            var all = _catalog.GetAllVisible();
            Assert.AreEqual(1, all.Count);
            Assert.AreEqual("vis", all[0].Id);
        }

        // ==================================================================
        // Snapshot
        // ==================================================================

        [Test]
        public void Snapshot_RoundTrips_PurchaseCounts()
        {
            AddBoosterListing(limit: 5);
            _service.TryPurchase("shop_heal");
            _service.TryPurchase("shop_heal");

            var snap = _service.CreateSnapshot();

            // Build a fresh service with same catalog/wallet/boosters
            var fresh = new ShopService(_catalog, _wallet, _boosters, _gear);
            fresh.LoadFromSnapshot(snap);

            Assert.AreEqual(2, fresh.GetPurchaseCount("shop_heal"));
        }
    }
}
