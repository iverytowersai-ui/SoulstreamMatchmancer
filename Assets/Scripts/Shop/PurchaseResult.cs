namespace Matchmancer.Shop
{
    /// <summary>
    /// Outcome of a <see cref="ShopService.TryPurchase"/> call.
    /// </summary>
    public enum PurchaseResult
    {
        /// <summary>Purchase succeeded — gold deducted, item granted.</summary>
        Success           = 0,

        /// <summary>The listing id is not in the catalog.</summary>
        ItemNotFound      = 1,

        /// <summary>Player does not have enough gold.</summary>
        InsufficientFunds = 2,

        /// <summary>Player has hit the per-profile purchase limit.</summary>
        LimitReached      = 3,

        /// <summary>The booster or gear ref id could not be resolved.</summary>
        GrantFailed       = 4,
    }
}
