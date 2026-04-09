using Matchmancer.Core;

namespace Matchmancer.Combat
{
    /// <summary>
    /// Static lookup between <see cref="TileType"/> (Soulstream identity)
    /// and <see cref="CombatRole"/> (mechanical effect). This is the single
    /// source of truth for the mapping defined in the Matchmancer master spec:
    ///
    ///   SoulstreamShard  → Damage
    ///   PortRune         → Energy
    ///   CovenSeal        → Defense
    ///   OzoneMark        → Debuff
    ///   WitchbreedThorn  → Break
    ///   PetshaCharm      → Luck
    ///
    /// Pure C#. No UnityEngine. Safe to call from tests.
    /// </summary>
    public static class CombatRoleMap
    {
        /// <summary>
        /// Returns the combat role for the given tile type.
        /// <see cref="TileType.None"/> throws — callers must filter empty cells first.
        /// </summary>
        public static CombatRole GetRole(TileType tileType)
        {
            return tileType switch
            {
                TileType.SoulstreamShard => CombatRole.Damage,
                TileType.PortRune        => CombatRole.Energy,
                TileType.CovenSeal       => CombatRole.Defense,
                TileType.OzoneMark       => CombatRole.Debuff,
                TileType.WitchbreedThorn => CombatRole.Break,
                TileType.PetshaCharm     => CombatRole.Luck,
                _ => throw new System.ArgumentException(
                    $"TileType.{tileType} has no combat role. " +
                    "Did you forget to filter out TileType.None before calling CombatRoleMap?")
            };
        }

        /// <summary>
        /// Safe variant — returns <c>true</c> and sets <paramref name="role"/>
        /// when the tile has a mapped combat role. Returns <c>false</c> for
        /// <see cref="TileType.None"/> without throwing.
        /// </summary>
        public static bool TryGetRole(TileType tileType, out CombatRole role)
        {
            switch (tileType)
            {
                case TileType.SoulstreamShard: role = CombatRole.Damage;  return true;
                case TileType.PortRune:        role = CombatRole.Energy;  return true;
                case TileType.CovenSeal:       role = CombatRole.Defense; return true;
                case TileType.OzoneMark:       role = CombatRole.Debuff;  return true;
                case TileType.WitchbreedThorn: role = CombatRole.Break;   return true;
                case TileType.PetshaCharm:     role = CombatRole.Luck;    return true;
                default:                       role = default;            return false;
            }
        }
    }
}
