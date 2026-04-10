using UnityEngine;

namespace Matchmancer.Shop
{
    /// <summary>
    /// Inspector-facing shop listing. One asset per listing under
    /// <c>Assets/Data/Shop/…</c>. Produces a pure-C#
    /// <see cref="ShopItemDefinition"/> via <see cref="ToDefinition"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "ShopItemData", menuName = "Matchmancer/Shop Item Data")]
    public class ShopItemData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Stable listing id — never rename after shipping.")]
        public string id;
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Type & Reference")]
        public ShopItemType type = ShopItemType.Booster;
        [Tooltip("BoosterId or GearTuningId. Ignored for Gold type.")]
        public string refId;
        [Min(1)] public int quantity = 1;

        [Header("Price")]
        [Min(0)] public int price = 100;

        [Header("Limits")]
        [Tooltip("0 = unlimited. Otherwise max times a profile can buy this.")]
        [Min(0)] public int purchaseLimit;

        [Header("Display")]
        public bool isHidden;
        public int  sortOrder;

        public ShopItemDefinition ToDefinition()
        {
            return new ShopItemDefinition
            {
                Id            = string.IsNullOrEmpty(id) ? name : id,
                DisplayName   = string.IsNullOrEmpty(displayName) ? name : displayName,
                Description   = description ?? string.Empty,
                Type          = type,
                RefId         = refId ?? string.Empty,
                Quantity      = quantity,
                Price         = price,
                PurchaseLimit = purchaseLimit,
                IsHidden      = isHidden,
                SortOrder     = sortOrder,
            };
        }
    }
}
