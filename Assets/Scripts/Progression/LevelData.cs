using System.Collections.Generic;
using UnityEngine;
using Matchmancer.Core;
using Matchmancer.Enemy;
using Matchmancer.Objectives;

namespace Matchmancer.Progression
{
    /// <summary>
    /// Serializable stone block definition. Mirrors
    /// <see cref="StoneBlockConfig"/> but as a Unity-visible struct so
    /// level designers can edit per-level layouts in the Inspector.
    /// </summary>
    [System.Serializable]
    public struct LevelStoneBlock
    {
        public int row;
        public int col;
        [Min(1)] public int hp;
    }

    /// <summary>
    /// Serializable objective definition. Mirrors
    /// <see cref="ObjectiveConfig"/> but with public fields so Unity's
    /// serializer picks it up.
    /// </summary>
    [System.Serializable]
    public struct LevelObjective
    {
        public ObjectiveType type;
        [Min(0)] public int targetScore;
        [Min(0)] public int targetStoneClears;
        [Min(0)] public int surviveTurns;
    }

    /// <summary>
    /// The Matchmancer-level definition of a single battle. One asset per
    /// level, 100 total. Carries identity, enemy, board/move config, star
    /// thresholds, rewards, and story hooks.
    ///
    /// <see cref="ToLevelConfig"/> converts the Inspector-friendly fields
    /// into the plain-C# <see cref="LevelConfig"/> that
    /// <c>BoardController.InitializeLevel</c> already consumes — so the
    /// board pipeline needs no changes.
    /// </summary>
    [CreateAssetMenu(fileName = "Level_S1_L1", menuName = "Matchmancer/Level Data")]
    public class LevelData : ScriptableObject
    {
        // ------------------------------------------------------------------
        // Identity
        // ------------------------------------------------------------------

        [Header("Identity")]
        public string displayName = "Level 1-1";

        [Tooltip("1..10")]
        [Min(1)] public int stageIndex  = 1;

        [Tooltip("1..10 within the stage")]
        [Min(1)] public int levelIndex  = 1;

        [Tooltip("1..100, flat global key used by save data. " +
                 "By convention = (stageIndex-1)*10 + levelIndex.")]
        [Min(1)] public int globalIndex = 1;

        // ------------------------------------------------------------------
        // Combat
        // ------------------------------------------------------------------

        [Header("Combat")]
        public EnemyData enemy;

        // ------------------------------------------------------------------
        // Board / Moves
        // ------------------------------------------------------------------

        [Header("Moves & Meter")]
        [Min(1)] public int totalMoves    = 25;
        [Min(1)] public int meterCapacity = 100;

        // ------------------------------------------------------------------
        // Star thresholds (score-based — consumed by Scoring system)
        // ------------------------------------------------------------------

        [Header("Star Score Thresholds")]
        [Min(0)] public int oneStar   = 1000;
        [Min(0)] public int twoStar   = 2500;
        [Min(0)] public int threeStar = 5000;
        [Min(0)] public int fourStar  = 7500;
        [Min(0)] public int fiveStar  = 10000;

        // ------------------------------------------------------------------
        // Objective + stones
        // ------------------------------------------------------------------

        [Header("Objective")]
        public LevelObjective objective = new LevelObjective
        {
            type         = ObjectiveType.ReachScore,
            targetScore  = 5000,
        };

        [Header("Stone Blocks")]
        public List<LevelStoneBlock> stoneBlocks = new List<LevelStoneBlock>();

        // ------------------------------------------------------------------
        // Rewards
        // ------------------------------------------------------------------

        [Header("Rewards")]
        [Min(0)] public int goldReward = 10;
        [Min(0)] public int xpReward   = 15;

        // ------------------------------------------------------------------
        // Story hooks
        // ------------------------------------------------------------------

        [Header("Story Hooks")]
        public string preStorySceneId  = "";
        public string postStorySceneId = "";

        // ------------------------------------------------------------------
        // Design notes
        // ------------------------------------------------------------------

        [Header("Design Notes (internal)")]
        [TextArea] public string designNotes = "";

        // ------------------------------------------------------------------
        // Conversion
        // ------------------------------------------------------------------

        /// <summary>
        /// Build the plain-C# <see cref="LevelConfig"/> that BoardController
        /// consumes. Lets the existing engine code stay untouched.
        /// </summary>
        public LevelConfig ToLevelConfig()
        {
            var config = new LevelConfig
            {
                LevelNumber   = globalIndex,
                TotalMoves    = totalMoves,
                MeterCapacity = meterCapacity,
                OneStar       = oneStar,
                TwoStar       = twoStar,
                ThreeStar     = threeStar,
                FourStar      = fourStar,
                FiveStar      = fiveStar,
                Objective = new ObjectiveConfig
                {
                    Type              = objective.type,
                    TargetScore       = objective.targetScore,
                    TargetStoneClears = objective.targetStoneClears,
                    SurviveTurns      = objective.surviveTurns,
                },
                StoneBlocks = new List<StoneBlockConfig>(stoneBlocks.Count),
            };

            foreach (var s in stoneBlocks)
                config.StoneBlocks.Add(new StoneBlockConfig(s.row, s.col, s.hp));

            return config;
        }

        // ------------------------------------------------------------------
        // Editor validation
        // ------------------------------------------------------------------

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (stageIndex  < 1) stageIndex  = 1;
            if (stageIndex  > LevelIndexing.StageCount) stageIndex = LevelIndexing.StageCount;
            if (levelIndex  < 1) levelIndex  = 1;
            if (levelIndex  > LevelIndexing.LevelsPerStage) levelIndex = LevelIndexing.LevelsPerStage;
            // Auto-compute global index if it looks stale.
            int expected = LevelIndexing.ToGlobalIndex(stageIndex, levelIndex);
            if (globalIndex != expected) globalIndex = expected;
        }
#endif
    }
}
