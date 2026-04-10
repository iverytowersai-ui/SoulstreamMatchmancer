using System.Collections.Generic;
using UnityEngine;
using Matchmancer.Boosters;
using Matchmancer.Character;

namespace Matchmancer.Shop
{
    /// <summary>
    /// Scene-side bridge for the Magickal Bazaar. Owns the pure-C#
    /// <see cref="ShopService"/>, <see cref="Wallet"/>, and
    /// <see cref="ShopCatalog"/>. Seeds catalog from Inspector arrays,
    /// wires the gear tuning resolver through
    /// <see cref="GearInventoryController"/>, and exposes events the UI
    /// binds to.
    ///
    /// Persistence: Wallet snapshot + ShopService snapshot are added to
    /// <see cref="Save.SaveData"/> via Skill 20 save pipeline. This
    /// component calls Save/Load helpers but does not own the file I/O.
    /// </summary>
    [DisallowMultipleComponent]
    public class ShopController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BoosterController       boosterController;
        [SerializeField] private GearInventoryController gearInventoryController;

        [Header("Catalog")]
        [Tooltip("Every purchasable listing in the Bazaar.")]
        [SerializeField] private ShopItemData[] shopItems;

        [Header("Starting Gold")]
        [SerializeField] private int startingGold = 500;

        public Wallet       Wallet  { get; private set; }
        public ShopCatalog  Catalog { get; private set; }
        public ShopService  Service { get; private set; }

        // ------------------------------------------------------------------
        // Lifecycle
        // ------------------------------------------------------------------

        private void Awake()
        {
            Wallet  = new Wallet();
            Catalog = new ShopCatalog();

            if (shopItems != null)
                foreach (var data in shopItems)
                    if (data != null) Catalog.Register(data.ToDefinition());

            BoosterInventory boosters = boosterController != null
                ? boosterController.Inventory
                : new BoosterInventory();

            GearInventory gear = gearInventoryController != null
                ? gearInventoryController.Inventory
                : null;

            Service = new ShopService(Catalog, Wallet, boosters, gear);

            // Wire gear resolver
            if (gearInventoryController != null)
                Service.GearTuningResolver = gearInventoryController.ResolveTuning;

            // Seed starting gold (save-load will overwrite this later)
            if (startingGold > 0)
                Wallet.Earn(startingGold);
        }

        // ------------------------------------------------------------------
        // Public API for UI
        // ------------------------------------------------------------------

        public PurchaseResult Buy(string listingId) => Service.TryPurchase(listingId);

        /// <summary>Quick check for the UI "Buy" button active state.</summary>
        public bool CanBuy(string listingId)
        {
            var item = Catalog.Get(listingId);
            if (item == null) return false;
            if (item.PurchaseLimit > 0 && Service.GetPurchaseCount(listingId) >= item.PurchaseLimit) return false;
            return Wallet.CanAfford(item.Price);
        }

        /// <summary>Visible listings for a tab in the shop UI.</summary>
        public List<ShopItemDefinition> GetListings(ShopItemType type) =>
            Catalog.GetVisibleByType(type);
    }
}
