namespace Matchmancer.Character
{
    /// <summary>
    /// Cosmetic / tuning bucket. Does not drive stat math directly — each
    /// <see cref="GearTuning"/> carries its own <see cref="GearStatModifiers"/>.
    /// Rarity is for UI framing (border color, drop rate, sort order).
    /// </summary>
    public enum GearRarity
    {
        Common    = 0,
        Uncommon  = 1,
        Rare      = 2,
        Epic      = 3,
        Legendary = 4,
    }
}
