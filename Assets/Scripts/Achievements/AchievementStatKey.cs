namespace Matchmancer.Achievements
{
    /// <summary>
    /// Every stat the achievement tracker is aware of. Add new keys at the
    /// end — never reorder — so save data built against older versions still
    /// maps correctly. Values are serialised by name via
    /// <see cref="System.Enum.Parse(System.Type, string)"/>.
    ///
    /// Cumulative keys (most) grow monotonically with
    /// <c>IncrementStat</c>. High-water keys (suffix "Ever") only grow via
    /// <c>UpdateMaxStat</c>.
    /// </summary>
    public enum AchievementStatKey
    {
        LevelsCompleted      = 0,
        StagesCompleted      = 1,
        TotalStarsEarned     = 2,
        FiveStarsEarned      = 3,

        EnemiesDefeated      = 4,
        BossesDefeated       = 5,
        FlawlessBattles      = 6,

        TotalDamageDealt     = 7,
        TotalDamageTaken     = 8,
        TotalMatchesMade     = 9,
        TotalTilesCleared    = 10,
        UltimatesUsed        = 11,

        // High-water keys — set via UpdateMaxStat only
        MaxComboEver         = 100,
        HighestStageReached  = 101,
        BiggestHitEver       = 102,
    }
}
