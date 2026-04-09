using System;
using UnityEngine;
using Matchmancer.Core;

namespace Matchmancer.Progression
{
    /// <summary>
    /// Scene-side glue for Skill 18. Listens for the battle-end signal,
    /// asks external sources to fill in a <see cref="BattleResult"/>, runs
    /// it through <see cref="StarRatingSystem"/>, and publishes the
    /// evaluated <see cref="StarResult"/> so the UI panel can bind to it.
    ///
    /// Build-it pattern (no scene yet): other systems (BoardController,
    /// ObjectiveChecker, CharacterBattleController) wire into this via
    /// <see cref="ReportBattleResult"/> once the battle loop is done.
    /// Until we wire full battle lifecycle in Skill 24, this component
    /// just provides the plumbing and events.
    /// </summary>
    [DisallowMultipleComponent]
    public class ResultsController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LevelProgressionManager progressionManager;

        /// <summary>
        /// Fires after a battle is processed. <paramref name="level"/> is
        /// the LevelConfig the battle ran against; may be null if the
        /// caller didn't supply one (evaluation is skipped in that case).
        /// </summary>
        public event Action<BattleResult, StarResult, LevelConfig> OnResultsReady;

        public BattleResult LastResult  { get; private set; }
        public StarResult   LastStars   { get; private set; }
        public LevelConfig  LastLevel   { get; private set; }

        private StarRatingSystem _system;

        private void Awake()
        {
            _system = new StarRatingSystem(progressionManager != null ? progressionManager.State : null);
            _system.OnBattleProcessed += HandleProcessed;
        }

        private void OnDestroy()
        {
            if (_system != null) _system.OnBattleProcessed -= HandleProcessed;
        }

        /// <summary>
        /// Called by battle code (BoardController / objective checker) at
        /// the moment the battle ends. Thin wrapper that drops into
        /// <see cref="StarRatingSystem.ProcessBattleResult"/>.
        /// </summary>
        public StarResult ReportBattleResult(BattleResult result, LevelConfig level)
        {
            if (_system == null)
                _system = new StarRatingSystem(progressionManager != null ? progressionManager.State : null);
            return _system.ProcessBattleResult(result, level);
        }

        private void HandleProcessed(BattleResult result, StarResult stars, LevelConfig level)
        {
            LastResult = result;
            LastStars  = stars;
            LastLevel  = level;
            OnResultsReady?.Invoke(result, stars, level);
        }
    }
}
