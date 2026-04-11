using System;
using Matchmancer.Core;

namespace Matchmancer.Progression
{
    /// <summary>
    /// Pure C# orchestrator that ties together <see cref="BattleResult"/>,
    /// <see cref="StarEvaluator"/> and <see cref="ProgressionState"/>.
    ///
    /// Typical usage from scene code (see ResultsController):
    ///   var sys = new StarRatingSystem(progressionState);
    ///   var star = sys.ProcessBattleResult(result, levelConfig);
    ///   // show results UI using 'star'
    ///
    /// The system also fires a local event so UI can subscribe once and
    /// react to any battle completion — useful when several panels care.
    /// </summary>
    public class StarRatingSystem
    {
        private readonly ProgressionState _progression;

        /// <summary>
        /// Fired after a battle has been evaluated and (if a win) recorded
        /// into <see cref="ProgressionState"/>. Carries the raw result,
        /// the evaluated stars, and the level config used.
        /// </summary>
        public event Action<BattleResult, StarResult, LevelConfig> OnBattleProcessed;

        /// <summary>
        /// Optional — pass null if you just want to evaluate without
        /// recording (e.g. replay previews, test harnesses).
        /// </summary>
        public StarRatingSystem(ProgressionState progression)
        {
            _progression = progression;
        }

        /// <summary>
        /// Evaluate a single battle and, on victory, record it into
        /// progression. Returns the star result for UI to consume.
        /// Null level → throws; it's a programming error to report a
        /// battle that doesn't have a config.
        /// </summary>
        public StarResult ProcessBattleResult(BattleResult result, LevelConfig level)
        {
            if (level == null) throw new ArgumentNullException(nameof(level));

            StarResult stars = StarEvaluator.Evaluate(result, level);

            // Record the win to progression (if we have one).
            if (_progression != null && result.Victory && result.GlobalLevelIndex > 0)
            {
                _progression.RecordLevelCompletion(
                    globalIndex: result.GlobalLevelIndex,
                    stars:       stars.Stars,
                    fiveStar:    stars.FiveStar,
                    bestCombo:   result.MaxComboAchieved);
            }

            OnBattleProcessed?.Invoke(result, stars, level);
            return stars;
        }

        /// <summary>
        /// Dry run — evaluate stars without touching progression or firing
        /// events. Handy for "if I win right now, what would I get?" previews.
        /// </summary>
        public StarResult Preview(BattleResult result, LevelConfig level) =>
            StarEvaluator.Evaluate(result, level);
    }
}
