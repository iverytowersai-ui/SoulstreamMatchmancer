namespace Matchmancer.Shop
{
    /// <summary>
    /// What kind of thing a shop listing sells. Determines which inventory
    /// receives the granted item after purchase.
    /// </summary>
    public enum ShopItemType
    {
        /// <summary>Grants a booster stack to <see cref="Boosters.BoosterInventory"/>.</summary>
        Booster = 0,

        /// <summary>Grants a gear piece to <see cref="Character.GearInventory"/>.</summary>
        Gear    = 1,

        /// <summary>Grants raw gold (for "gold pack" listings or convert-premium flows).</summary>
        Gold    = 2,
    }
}
