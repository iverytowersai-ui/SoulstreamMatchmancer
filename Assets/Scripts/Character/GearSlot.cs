namespace Matchmancer.Character
{
    /// <summary>
    /// The four gear slots a character can equip. One item per slot at a time.
    /// Order matters only for iteration — don't rely on numeric values for save
    /// data; serialize by name.
    /// </summary>
    public enum GearSlot
    {
        Weapon   = 0,
        Armor    = 1,
        Talisman = 2,
        Relic    = 3,
    }
}
