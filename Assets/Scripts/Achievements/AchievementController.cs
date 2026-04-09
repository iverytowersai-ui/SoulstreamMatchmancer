using UnityEngine;
using Matchmancer.Core;
using Matchmancer.Progression;

namespace Matchmancer.Achievements
{
    /// <summary>
    /// Scene-side bridge that owns the pure-C#
    /// <see cref="AchievementTracker"/> + <see cref="TitleSystem"/>, seeds
    /// them from Inspector-assigned ScriptableObject assets, and listens to
    /// <see cref="ResultsController.OnResultsReady"/> to convert each
    /// finished battle into stat increments.
    ///
    /// Persistence is NOT implemented here — Skill 20 (Save System) will
    /// snapshot/restore the tracker and title system. This component only
    /// bridges the runtime layer.
    /// </summary>
    [DisallowMultipleComponent]
    public class AchievementController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ResultsController results;

        [Header("Definitions")]
        [SerializeField] private AchievementData[] achievementAssets;
        [SerializeField] private TitleData[]       titleAssets;

        public AchievementTracker Tracker { get; private set; }
        public TitleSystem        Titles  { get; private set; }

        private void Awake()
        {
            Tracker = new AchievementTracker();
            Titles  = new TitleSystem();

            if (achievementAssets != null)
                foreach (var data in achievementAssets)
                    if (data != null) Tracker.RegisterAchievement(data.ToDefinition());

            if (titleAssets != null)
                foreach (var data in titleAssets)
                    if (data != null) Titles.RegisterTitle(data.ToDefinition());

            // Wire titles to auto-unlock from achievements that grant them.
            Titles.WireToTracker(Tracker);
        }

        private void OnEnable()
        {
            if (results != null) results.OnResultsReady += HandleResults;
        }

        private void OnDisable()
        {
            if (results != null) results.OnResultsReady -= HandleResults;
        }

        // ------------------------------------------------------------------
        // Battle → stats translation
        // ------------------------------------------------------------------

        private void HandleResults(BattleResult result, StarResult stars, LevelConfig level)
        {
            if (Tracker == null) return;

            // Cumulative damage stats apply win or lose.
            if (result.DamageDealt > 0)
                Tracker.IncrementStat(AchievementStatKey.TotalDamageDealt, result.DamageDealt);
            if (result.DamageTaken > 0)
                Tracker.IncrementStat(AchievementStatKey.TotalDamageTaken, result.DamageTaken);

            // High-water marks
            if (result.MaxComboAchieved > 0)
                Tracker.UpdateMaxStat(AchievementStatKey.MaxComboEver, result.MaxComboAchieved);

            if (!result.Victory) return;

            // Win-only stats
            Tracker.IncrementStat(AchievementStatKey.LevelsCompleted);
            Tracker.IncrementStat(AchievementStatKey.EnemiesDefeated);

            if (stars.Stars > 0)
                Tracker.IncrementStat(AchievementStatKey.TotalStarsEarned, stars.Stars);
            if (stars.FiveStar)
                Tracker.IncrementStat(AchievementStatKey.FiveStarsEarned);
            if (result.Flawless)
                Tracker.IncrementStat(AchievementStatKey.FlawlessBattles);

            if (result.GlobalLevelIndex > 0)
                Tracker.UpdateMaxStat(AchievementStatKey.HighestStageReached,
                    LevelIndexing.StageOf(result.GlobalLevelIndex));
        }
    }
}
