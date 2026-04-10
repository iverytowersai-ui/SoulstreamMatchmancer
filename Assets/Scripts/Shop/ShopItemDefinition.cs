using System;

namespace Matchmancer.Shop
{
    /// <summary>
    /// Pure C# definition of a single shop listing. One per item/bundle
    /// that appears in the Magickal Bazaar. <see cref="ShopItemData"/> is
    /// the Inspector-facing ScriptableObject wrapper.
    ///
    /// <see cref="RefId"/> semantics depend on <see cref="Type"/>:
    ///   • Booster → the <c>BoosterDefinition.Id</c> to grant.
    ///   • Gear    → the <c>GearTuning.Id</c> to grant.
    ///   • Gold    → ignored (amount comes from <see cref="Quantity"/>).
    /// </summary>
    [Serializable]
    public class ShopItemDefinition
    {
        /// <summary>Stable id for this listing. Never rename after shipping.</summary>
        public string Id;

        public string DisplayName;
        public string Description;

        public ShopItemType Type;

        /// <summary>
        /// Reference to the granted item's definition id. Meaning varies
        /// by <see cref="Type"/> — see class summary.
        /// </summary>
        public string RefId;

        /// <summary>
        /// How many to grant on purchase. 1 for gear, N for booster packs
        /// or gold bundles.
        /// </summary>
        public int Quantity;

        /// <summary>Price in soft currency (gold). 0 = free.</summary>
        public int Price;

        /// <summary>
        /// Maximum times a single profile can buy this listing. 0 = unlimited.
        /// Useful for one-time gear purchases.
        /// </summary>
        public int PurchaseLimit;

        /// <summary>Hidden from the shelf until some external condition is met.</summary>
        public bool IsHidden;

        /// <summary>Sort order — lower appears first.</summary>
        public int SortOrder;

        public ShopItemDefinition()
        {
            Id          = string.Empty;
            DisplayName = string.Empty;
            Description = string.Empty;
            RefId       = string.Empty;
            Quantity    = 1;
        }

        public ShopItemDefinition Clone()
        {
            return new ShopItemDefinition
            {
                Id            = Id,
                DisplayName   = DisplayName,
                Description   = Description,
                Type          = Type,
                RefId         = RefId,
                Quantity      = Quantity,
                Price         = Price,
                PurchaseLimit = PurchaseLimit,
                IsHidden      = IsHidden,
                SortOrder     = SortOrder,
            };
        }
    }
}
