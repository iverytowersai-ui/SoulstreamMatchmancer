namespace Matchmancer.Boosters
{
    /// <summary>
    /// Where in the game flow a booster can be used. Gates consumption at the
    /// inventory layer so the UI just calls TryUse(id, context).
    /// </summary>
    public enum BoosterUseContext
    {
        OutOfBattle = 0, // pre-battle prep screen only
        InBattle    = 1, // during an active match
        Anywhere    = 2, // both contexts
    }
}
