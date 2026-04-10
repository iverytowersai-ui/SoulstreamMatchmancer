using System;
using System.Collections.Generic;
using Matchmancer.Boosters;
using Matchmancer.Character;

namespace Matchmancer.Shop
{
    /// <summary>
    /// Pure C# purchase orchestrator. Validates funds, deducts gold, grants
    /// the item to the correct inventory, and tracks per-profile purchase
    /// counts for limited listings. No UnityEngine — fully headless-testable.
    ///
    /// Gear grant requires a <see cref="GearTuningResolver"/> delegate so
    /// the service can look up a <see cref="GearTuning"/> by id without
    /// holding a direct reference to ScriptableObjects.
    /// </summary>
    public class ShopService
    {
        private readonly ShopCatalog      _catalog;
        private readonly Wallet           _wallet;
        private readonly BoosterInventory _boosters;
        private readonly GearInventory    _gear;

        /// <summary>
        /// Resolves a gear tuning id → <see cref="GearTuning"/>. Null if
        /// the id is unknown. Set by the MonoBehaviour bridge that has
        /// access to ScriptableObject assets.
        /// </summary>
        public Func<string, GearTuning> GearTuningResolver { get; set; }

        /// <summary>Per-profile purchase count, keyed by shop listing id.</summary>
        private readonly Dictionary<string, int> _purchaseCounts =
            new Dictionary<string, int>();

        // ------------------------------------------------------------------
        // Events
        // ------------------------------------------------------------------

        /// <summary>Fires on every successful purchase. (listing, quantity granted)</summary>
        public event Action<ShopItemDefinition, int> OnPurchaseSuccess;

        /// <summary>Fires on every failed purchase. (listing or null, reason)</summary>
        public event Action<ShopItemDefinition, PurchaseResult> OnPurchaseFailed;

        // ------------------------------------------------------------------
        // Construction
        // ------------------------------------------------------------------

        /// <summary>
        /// All dependencies injected — no singletons touched.
        /// Pass null for <paramref name="gear"/> if gear isn't available yet.
        /// </summary>
        public ShopService(
            ShopCatalog      catalog,
            Wallet           wallet,
            BoosterInventory boosters,
            GearInventory    gear = null)
        {
            _catalog  = catalog  ?? throw new ArgumentNullException(nameof(catalog));
            _wallet   = wallet   ?? throw new ArgumentNullException(nameof(wallet));
            _boosters = boosters ?? throw new ArgumentNullException(nameof(boosters));
            _gear     = gear;
        }

        // ------------------------------------------------------------------
        // Purchase flow
        // ------------------------------------------------------------------

        /// <summary>
        /// Attempt a single purchase. Returns a result enum so the UI can
        /// show the right feedback. Does NOT touch the wallet if the
        /// purchase fails for any reason.
        /// </summary>
        public PurchaseResult TryPurchase(string listingId)
        {
            var item = _catalog.Get(listingId);
            if (item == null)
            {
                OnPurchaseFailed?.Invoke(null, PurchaseResult.ItemNotFound);
                return PurchaseResult.ItemNotFound;
            }

            // Limit check
            if (item.PurchaseLimit > 0)
            {
                int bought = GetPurchaseCount(listingId);
                if (bought >= item.PurchaseLimit)
                {
                    OnPurchaseFailed?.Invoke(item, PurchaseResult.LimitReached);
                    return PurchaseResult.LimitReached;
                }
            }

            // Funds check
            if (!_wallet.CanAfford(item.Price))
            {
                OnPurchaseFailed?.Invoke(item, PurchaseResult.InsufficientFunds);
                return PurchaseResult.InsufficientFunds;
            }

            // Grant the item
            int qty = item.Quantity < 1 ? 1 : item.Quantity;
            bool granted = Grant(item, qty);
            if (!granted)
            {
                OnPurchaseFailed?.Invoke(item, PurchaseResult.GrantFailed);
                return PurchaseResult.GrantFailed;
            }

            // Deduct gold (guaranteed to succeed — CanAfford was checked)
            _wallet.Spend(item.Price);

            // Track purchase count
            IncrementPurchaseCount(listingId);

            OnPurchaseSuccess?.Invoke(item, qty);
            return PurchaseResult.Success;
        }

        // ------------------------------------------------------------------
        // Grant logic — type-dependent routing
        // ------------------------------------------------------------------

        private bool Grant(ShopItemDefinition item, int qty)
        {
            switch (item.Type)
            {
                case ShopItemType.Booster:
                    return GrantBooster(item.RefId, qty);

                case ShopItemType.Gear:
                    return GrantGear(item.RefId);

                case ShopItemType.Gold:
                    _wallet.Earn(qty);
                    return true;

                default:
                    return false;
            }
        }

        private bool GrantBooster(string boosterId, int qty)
        {
            if (string.IsNullOrEmpty(boosterId)) return false;
            if (!_boosters.HasDefinition(boosterId)) return false;
            _boosters.Purchase(boosterId, qty);
            return true;
        }

        private bool GrantGear(string gearTuningId)
        {
            if (string.IsNullOrEmpty(gearTuningId)) return false;
            if (_gear == null) return false;

            GearTuning tuning = GearTuningResolver?.Invoke(gearTuningId);
            if (tuning == null) return false;

            var newItem = new GearItem(tuning);
            _gear.AddItem(newItem);
            return true;
        }

        // ------------------------------------------------------------------
        // Purchase-count tracking
        // ------------------------------------------------------------------

        public int GetPurchaseCount(string listingId)
        {
            if (string.IsNullOrEmpty(listingId)) return 0;
            _purchaseCounts.TryGetValue(listingId, out int c);
            return c;
        }

        private void IncrementPurchaseCount(string listingId)
        {
            if (!_purchaseCounts.ContainsKey(listingId))
                _purchaseCounts[listingId] = 0;
            _purchaseCounts[listingId]++;
        }

        // ------------------------------------------------------------------
        // Snapshot / restore
        // ------------------------------------------------------------------

        public ShopSnapshot CreateSnapshot()
        {
            var ids    = new List<string>(_purchaseCounts.Count);
            var counts = new List<int>(_purchaseCounts.Count);
            foreach (var kv in _purchaseCounts)
            {
                if (kv.Value <= 0) continue;
                ids.Add(kv.Key);
                counts.Add(kv.Value);
            }
            return new ShopSnapshot
            {
                ListingIds     = ids.ToArray(),
                PurchaseCounts = counts.ToArray(),
            };
        }

        public void LoadFromSnapshot(ShopSnapshot snapshot)
        {
            _purchaseCounts.Clear();
            if (snapshot == null) return;
            if (snapshot.ListingIds == null || snapshot.PurchaseCounts == null) return;
            int n = Math.Min(snapshot.ListingIds.Length, snapshot.PurchaseCounts.Length);
            for (int i = 0; i < n; i++)
            {
                string id = snapshot.ListingIds[i];
                if (string.IsNullOrEmpty(id)) continue;
                _purchaseCounts[id] = snapshot.PurchaseCounts[i];
            }
        }
    }

    [Serializable]
    public class ShopSnapshot
    {
        public string[] ListingIds;
        public int[]    PurchaseCounts;
    }
}
