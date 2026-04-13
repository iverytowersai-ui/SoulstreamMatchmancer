/// <summary>
/// The six combat-relevant tile categories in Matchmancer.
/// Each type drives a distinct effect in CombatResolver.
/// </summary>
public enum TileType
{
    Damage,   // Soulstream Shard  — direct HP damage
    Energy,   // Port Rune         — fills ultimate meter
    Defense,  // Coven Seal        — generates shield/guard
    Debuff,   // OZONE Mark        — applies status (poison, vulnerability)
    Break,    // Witchbreed Thorn  — destroys armor/barriers
    Luck      // Petsha Charm      — crit support, combo boost, utility
}
