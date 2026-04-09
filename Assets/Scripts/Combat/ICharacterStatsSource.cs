namespace Matchmancer.Combat
{
    /// <summary>
    /// Minimal seam the BoardController reads from when calling
    /// <see cref="CombatResolver.ResolveWave"/>. Implemented by
    /// <c>CharacterBattleController</c> (Skill 14) and assigned on
    /// <see cref="Matchmancer.Core.BoardController.CharacterStatsSource"/>
    /// at battle start. If null, the resolver falls back to default
    /// attack = 10 and luck = 0.
    /// </summary>
    public interface ICharacterStatsSource
    {
        /// <summary>Current live attack stat (base + level growth + buffs).</summary>
        float Attack { get; }

        /// <summary>Current live luck stat (base + level growth + gear).</summary>
        float Luck { get; }
    }
}
