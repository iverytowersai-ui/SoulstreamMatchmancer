using NUnit.Framework;
using Matchmancer.Shop;
using Matchmancer.Boosters;
using Matchmancer.Character;
using Matchmancer.Achievements;

namespace Matchmancer.Tests
{
    /// <summary>
    /// Integration tests that exercise the full shop → wallet → inventory
    /// pipeline plus achievement gold payouts → wallet → shop loop.
    /// Pure C# — no Unity, no scene.
    /// </summary>
    [TestFixture]
    public class ShopIntegrationTests
    {
        private Wallet              _wallet;
        private BoosterInventory    _boosters;
        private GearInventory       _gear;
        private ShopCatalog         _catalog;
        private ShopService         _shop;
        private AchievementTracker  _tracker;

        [SetUp]
        public void SetUp()
        {
            _wallet   = new Wallet();
            _boosters = new BoosterInventory();
            _gear     = new GearInventory();
            _catalog  = new ShopCatalog();
            _tracker  = new AchievementTracker();

            // Register booster definition
            _boosters.RegisterDefinition(new BoosterDefinition("heal_pot", BoosterType.Heal, 50)
            {
                DisplayName = "Heal Potion",
                MaxStack    = 99,
                ShopPrice   = 100,
            });

            // Register gear tuning resolver
            _shop = new ShopService(_catalog, _wallet, _boosters, _gear);
            _shop.GearTuningResolver = id =>
            {
                if (id == "soul_blade")
                    return new GearTuning
                    {
                        Id          = "soul_blade",
                        DisplayName = "Soul Blade",
                        Slot        = GearSlot.Weapon,
                        Modifiers   = new GearStatModifiers { FlatAttack = 12f },
                    };
                return null;
            };

            // Wire achievement gold payouts into wallet
            _tracker.OnAchievementUnlocked += def =>
            {
                if (def.GoldReward > 0)
                    _wallet.Earn(def.GoldReward);
            };
        }

        // ==================================================================
        // Full purchase loop: earn → buy booster → use
        // ==================================================================

        [Test]
        public void EarnGoldFromBattle_BuyBooster_UseInBattle()
        {
            // Simulate level reward
            _wallet.Earn(200);
            Assert.AreEqual(200, _wallet.Gold);

            // Buy a heal potion
            _catalog.Register(new ShopItemDefinition
            {
                Id    = "shop_heal",
                Type  = ShopItemType.Booster,
                RefId = "heal_pot",
                Price = 100,
            });
            Assert.AreEqual(PurchaseResult.Success, _shop.TryPurchase("shop_heal"));
            Assert.AreEqual(100, _wallet.Gold);
            Assert.AreEqual(1, _boosters.GetCount("heal_pot"));

            // Use in battle
            Assert.IsTrue(_boosters.Use("heal_pot", BoosterUseContext.InBattle));
            Assert.AreEqual(0, _boosters.GetCount("heal_pot"));
        }

        // ==================================================================
        // Full purchase loop: earn → buy gear → equip → stats
        // ==================================================================

        [Test]
        public void BuyGear_EquipIt_StatsApply()
        {
            _wallet.Earn(500);

            _catalog.Register(new ShopItemDefinition
            {
                Id    = "shop_soul_blade",
                Type  = ShopItemType.Gear,
                RefId = "soul_blade",
                Price = 300,
            });
            Assert.AreEqual(PurchaseResult.Success, _shop.TryPurchase("shop_soul_blade"));
            Assert.AreEqual(200, _wallet.Gold);
            Assert.AreEqual(1, _gear.OwnedCount);

            // Find the purchased item and equip it
            GearItem bought = null;
            foreach (var item in _gear.OwnedItems.Values)
            {
                if (item.Id == "soul_blade") { bought = item; break; }
            }
            Assert.IsNotNull(bought);

            // Wire gear to a character runtime
            var tuning = new CharacterTuning
            {
                DisplayName = "Test Hero",
                BaseMaxHp   = 100,
                BaseAttack  = 10f,
                BaseDefense = 5f,
                BaseLuck    = 0f,
                MaxEnergy   = 100f,
                XpPerLevel  = new[] { 100 },
            };
            var runtime = new CharacterRuntime(tuning);
            _gear.OnModifiersChanged += mods => runtime.SetGearModifiers(mods);

            _gear.Equip(bought.InstanceId);
            Assert.AreEqual(22f, runtime.CurrentAttack, 0.001f); // 10 + 12 flat
        }

        // ==================================================================
        // Achievement gold → wallet → shop
        // ==================================================================

        [Test]
        public void AchievementUnlock_PaysGold_ThenPlayerBuys()
        {
            // Register an achievement that pays 300 gold
            _tracker.RegisterAchievement(new AchievementDefinition
            {
                Id          = "first_kill",
                StatKey     = AchievementStatKey.EnemiesDefeated,
                TargetValue = 1,
                GoldReward  = 300,
            });

            // No gold yet
            Assert.AreEqual(0, _wallet.Gold);

            // Simulate one enemy killed
            _tracker.IncrementStat(AchievementStatKey.EnemiesDefeated);
            Assert.IsTrue(_tracker.IsUnlocked("first_kill"));
            Assert.AreEqual(300, _wallet.Gold);

            // Spend the gold
            _catalog.Register(new ShopItemDefinition
            {
                Id    = "shop_heal",
                Type  = ShopItemType.Booster,
                RefId = "heal_pot",
                Price = 100,
            });
            Assert.AreEqual(PurchaseResult.Success, _shop.TryPurchase("shop_heal"));
            Assert.AreEqual(200, _wallet.Gold);
        }

        // ==================================================================
        // Purchase limit + multi-item interaction
        // ==================================================================

        [Test]
        public void LimitedGear_CannotBuyTwice()
        {
            _wallet.Earn(1000);
            _catalog.Register(new ShopItemDefinition
            {
                Id            = "unique_blade",
                Type          = ShopItemType.Gear,
                RefId         = "soul_blade",
                Price         = 200,
                PurchaseLimit = 1,
            });

            Assert.AreEqual(PurchaseResult.Success, _shop.TryPurchase("unique_blade"));
            Assert.AreEqual(PurchaseResult.LimitReached, _shop.TryPurchase("unique_blade"));
            Assert.AreEqual(1, _gear.OwnedCount);
            Assert.AreEqual(800, _wallet.Gold);
        }

        [Test]
        public void MultiplePurchases_TrackLifetimeSpending()
        {
            _wallet.Earn(500);
            _catalog.Register(new ShopItemDefinition
            {
                Id    = "cheap",
                Type  = ShopItemType.Booster,
                RefId = "heal_pot",
                Price = 50,
            });

            _shop.TryPurchase("cheap");
            _shop.TryPurchase("cheap");
            _shop.TryPurchase("cheap");

            Assert.AreEqual(3, _boosters.GetCount("heal_pot"));
            Assert.AreEqual(350, _wallet.Gold);
            Assert.AreEqual(150, _wallet.LifetimeGoldSpent);
        }

        // ==================================================================
        // Gear sell-back (GearTuning.SellPrice wiring preview)
        // ==================================================================

        [Test]
        public void SellGear_EarnsGoldBack()
        {
            // Buy gear
            _wallet.Earn(500);
            _catalog.Register(new ShopItemDefinition
            {
                Id    = "shop_blade",
                Type  = ShopItemType.Gear,
                RefId = "soul_blade",
                Price = 300,
            });
            _shop.TryPurchase("shop_blade");

            GearItem bought = null;
            foreach (var item in _gear.OwnedItems.Values)
            {
                if (item.Id == "soul_blade") { bought = item; break; }
            }
            Assert.IsNotNull(bought);

            // "Sell" it: remove from inventory, give gold
            int sellPrice = bought.Tuning.SellPrice; // 0 by default
            // For this test, simulate a sell price
            bought.Tuning.SellPrice = 100;
            _gear.RemoveItem(bought.InstanceId);
            _wallet.Earn(bought.Tuning.SellPrice);

            Assert.AreEqual(0, _gear.OwnedCount);
            Assert.AreEqual(300, _wallet.Gold); // 500 - 300 + 100 = 300
        }
    }
}
