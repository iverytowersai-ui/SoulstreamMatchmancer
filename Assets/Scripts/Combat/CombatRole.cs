namespace Matchmancer.Combat
{
    /// <summary>
    /// The six combat ROLES a tile can fulfil in Matchmancer.
    /// Distinct from <see cref="Matchmancer.Core.TileType"/> (which is the
    /// Soulstream-flavoured identity of a tile). A single TileType maps to
    /// exactly one CombatRole — see <see cref="CombatRoleMap"/>.
    ///
    /// Design note: we keep the role enum separate from TileType so the
    /// combat system can be tuned / extended without touching any of the
    /// board, matching, sigil, dice or stone-block code.
    /// </summary>
    public enum CombatRole
    {
        /// <summary>Direct HP damage to the active enemy.</summary>
        Damage,

        /// <summary>Fills the character's ultimate / energy meter.</summary>
        Energy,

        /// <summary>Generates shield / guard on the character.</summary>
        Defense,

        /// <summary>Applies a status effect (poison, vulnerability, etc.).</summary>
        Debuff,

        /// <summary>Destroys enemy armor / barriers.</summary>
        Break,

        /// <summary>Crit support, combo boost, and utility effects.</summary>
        Luck
    }
}
