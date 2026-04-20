namespace Matchmancer.Combat
{
    /// <summary>
    /// The six Matchmancer tile types. See Skill 11 for full combat behavior.
    /// Included here as a minimal enum so Stage 1 LevelData compiles without Skill 11.
    /// If Skill 11 already defines TileType, delete this file.
    /// </summary>
    public enum TileType
    {
        Damage  = 0, // Soulstream Shard
        Energy  = 1, // Port Rune
        Defense = 2, // Coven Seal
        Debuff  = 3, // OZONE Mark
        Break   = 4, // Witchbreed Thorn
        Luck    = 5  // Petsha Charm
    }
}
