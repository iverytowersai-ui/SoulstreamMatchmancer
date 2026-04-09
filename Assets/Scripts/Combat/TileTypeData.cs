using Matchmancer.Core;
using UnityEngine;

namespace Matchmancer.Combat
{
    /// <summary>
    /// Unity-facing visual + flavour metadata for a single tile type.
    /// One asset per <see cref="TileType"/>, stored under
    /// <c>Assets/ScriptableObjects/TileTypes/</c>.
    ///
    /// IMPORTANT — this is presentation-only. The combat role mapping
    /// lives in <see cref="CombatRoleMap"/> (pure C#), not here, so the
    /// combat math stays unit-testable without Unity.
    ///
    /// Use this asset to drive the view layer (TileView, HUD icons, lore pages).
    /// </summary>
    [CreateAssetMenu(
        fileName = "TileTypeData_",
        menuName = "Matchmancer/Tile Type Data",
        order    = 1)]
    public class TileTypeData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("The Soulstream tile identity. One asset per enum value.")]
        public TileType tileType;

        [Tooltip("Display name shown in UI, e.g. \"Soulstream Shard\".")]
        public string displayName;

        [Tooltip("Icon sprite used on the board and in the HUD.")]
        public Sprite icon;

        [Tooltip("Tint applied to the tile sprite.")]
        public Color tileColor = Color.white;

        [Header("Flavour")]
        [TextArea(3, 6)]
        public string loreDescription;

        /// <summary>
        /// Convenience: returns the combat role for this tile via <see cref="CombatRoleMap"/>.
        /// Throws if <c>tileType</c> is <see cref="TileType.None"/>.
        /// </summary>
        public CombatRole CombatRole => CombatRoleMap.GetRole(tileType);
    }
}
