using System;
using System.Collections.Generic;

namespace Matchmancer.Shop
{
    /// <summary>
    /// Pure C# shop catalog. Holds every <see cref="ShopItemDefinition"/>
    /// the Magickal Bazaar can sell. Provides lookup by id, filter by type,
    /// and sorted visible listing.
    ///
    /// Definitions are registered once at start-up from
    /// <see cref="ShopItemData"/> ScriptableObjects. The catalog is
    /// read-only at runtime — no procedural item generation in MVP.
    /// </summary>
    public class ShopCatalog
    {
        private readonly Dictionary<string, ShopItemDefinition> _items =
            new Dictionary<string, ShopItemDefinition>();

        public int Count => _items.Count;

        // ------------------------------------------------------------------
        // Registration
        // ------------------------------------------------------------------

        public void Register(ShopItemDefinition item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (string.IsNullOrEmpty(item.Id))
                throw new ArgumentException("ShopItemDefinition.Id cannot be empty.", nameof(item));
            _items[item.Id] = item;
        }

        public void RegisterAll(IEnumerable<ShopItemDefinition> items)
        {
            if (items == null) return;
            foreach (var item in items)
                if (item != null) Register(item);
        }

        // ------------------------------------------------------------------
        // Queries
        // ------------------------------------------------------------------

        public ShopItemDefinition Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            _items.TryGetValue(id, out var item);
            return item;
        }

        public bool Has(string id) =>
            !string.IsNullOrEmpty(id) && _items.ContainsKey(id);

        /// <summary>
        /// All visible listings of a given type, sorted by SortOrder.
        /// </summary>
        public List<ShopItemDefinition> GetVisibleByType(ShopItemType type)
        {
            var result = new List<ShopItemDefinition>();
            foreach (var item in _items.Values)
            {
                if (item.Type != type) continue;
                if (item.IsHidden) continue;
                result.Add(item);
            }
            result.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
            return result;
        }

        /// <summary>
        /// All visible listings across all types, sorted by SortOrder.
        /// </summary>
        public List<ShopItemDefinition> GetAllVisible()
        {
            var result = new List<ShopItemDefinition>();
            foreach (var item in _items.Values)
                if (!item.IsHidden) result.Add(item);
            result.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
            return result;
        }

        public IReadOnlyDictionary<string, ShopItemDefinition> All => _items;
    }
}
