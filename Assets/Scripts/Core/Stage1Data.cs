using System.Collections.Generic;
using Matchmancer.Objectives;

namespace Matchmancer.Core
{
    /// <summary>
    /// Stage 1: The Fractured Foundation — 10 levels with escalating Stone Blocks.
    /// Levels 1-3: No stones (learn core match-3).
    /// Levels 4-6: Single-hit stones.
    /// Levels 7-9: Multi-hit stones.
    /// Level 10: Boss — survive while stones spread.
    /// </summary>
    public static class Stage1Data
    {
        private static readonly Dictionary<int, LevelConfig> Levels = new()
        {
            // --- Levels 1-3: Learn the basics, no stones ---
            [1] = new LevelConfig
            {
                LevelNumber = 1,
                TotalMoves = 25,
                MeterCapacity = 10,
                OneStar = 300, TwoStar = 600, ThreeStar = 1000, FourStar = 1500, FiveStar = 2000,
                Objective = new ObjectiveConfig { Type = ObjectiveType.ReachScore, TargetScore = 300 },
                StoneBlocks = new List<StoneBlockConfig>()
            },
            [2] = new LevelConfig
            {
                LevelNumber = 2,
                TotalMoves = 22,
                MeterCapacity = 10,
                OneStar = 400, TwoStar = 800, ThreeStar = 1200, FourStar = 1800, FiveStar = 2500,
                Objective = new ObjectiveConfig { Type = ObjectiveType.ReachScore, TargetScore = 400 },
                StoneBlocks = new List<StoneBlockConfig>()
            },
            [3] = new LevelConfig
            {
                LevelNumber = 3,
                TotalMoves = 20,
                MeterCapacity = 10,
                OneStar = 500, TwoStar = 1000, ThreeStar = 1500, FourStar = 2200, FiveStar = 3000,
                Objective = new ObjectiveConfig { Type = ObjectiveType.ReachScore, TargetScore = 500 },
                StoneBlocks = new List<StoneBlockConfig>()
            },

            // --- Levels 4-6: Single-hit stones ---
            [4] = new LevelConfig
            {
                LevelNumber = 4,
                TotalMoves = 22,
                MeterCapacity = 10,
                OneStar = 500, TwoStar = 1000, ThreeStar = 1500, FourStar = 2200, FiveStar = 3000,
                Objective = new ObjectiveConfig { Type = ObjectiveType.ClearAllStones },
                StoneBlocks = new List<StoneBlockConfig>
                {
                    new(3, 3, 1), new(3, 4, 1)
                }
            },
            [5] = new LevelConfig
            {
                LevelNumber = 5,
                TotalMoves = 22,
                MeterCapacity = 10,
                OneStar = 600, TwoStar = 1200, ThreeStar = 1800, FourStar = 2500, FiveStar = 3500,
                Objective = new ObjectiveConfig { Type = ObjectiveType.ClearAllStones },
                StoneBlocks = new List<StoneBlockConfig>
                {
                    new(2, 2, 1), new(2, 5, 1), new(5, 2, 1), new(5, 5, 1)
                }
            },
            [6] = new LevelConfig
            {
                LevelNumber = 6,
                TotalMoves = 20,
                MeterCapacity = 10,
                OneStar = 700, TwoStar = 1400, ThreeStar = 2100, FourStar = 3000, FiveStar = 4000,
                Objective = new ObjectiveConfig { Type = ObjectiveType.ClearAllStones },
                StoneBlocks = new List<StoneBlockConfig>
                {
                    new(1, 3, 1), new(1, 4, 1),
                    new(3, 1, 1), new(3, 6, 1),
                    new(6, 3, 1), new(6, 4, 1)
                }
            },

            // --- Levels 7-9: Multi-hit stones ---
            [7] = new LevelConfig
            {
                LevelNumber = 7,
                TotalMoves = 25,
                MeterCapacity = 10,
                OneStar = 800, TwoStar = 1600, ThreeStar = 2400, FourStar = 3500, FiveStar = 4500,
                Objective = new ObjectiveConfig { Type = ObjectiveType.ClearAllStones },
                StoneBlocks = new List<StoneBlockConfig>
                {
                    new(3, 3, 2), new(3, 4, 2),
                    new(4, 3, 2), new(4, 4, 2)
                }
            },
            [8] = new LevelConfig
            {
                LevelNumber = 8,
                TotalMoves = 25,
                MeterCapacity = 10,
                OneStar = 900, TwoStar = 1800, ThreeStar = 2700, FourStar = 4000, FiveStar = 5000,
                Objective = new ObjectiveConfig { Type = ObjectiveType.ClearAllStones },
                StoneBlocks = new List<StoneBlockConfig>
                {
                    new(2, 2, 2), new(2, 5, 2),
                    new(4, 3, 3), new(4, 4, 3),
                    new(5, 2, 1), new(5, 5, 1)
                }
            },
            [9] = new LevelConfig
            {
                LevelNumber = 9,
                TotalMoves = 28,
                MeterCapacity = 10,
                OneStar = 1000, TwoStar = 2000, ThreeStar = 3000, FourStar = 4500, FiveStar = 6000,
                Objective = new ObjectiveConfig { Type = ObjectiveType.ClearAllStones },
                StoneBlocks = new List<StoneBlockConfig>
                {
                    new(1, 1, 2), new(1, 6, 2),
                    new(3, 3, 3), new(3, 4, 3),
                    new(4, 3, 3), new(4, 4, 3),
                    new(6, 1, 2), new(6, 6, 2)
                }
            },

            // --- Level 10: Boss — Survive while stones spread ---
            [10] = new LevelConfig
            {
                LevelNumber = 10,
                TotalMoves = 30,
                MeterCapacity = 10,
                OneStar = 1200, TwoStar = 2500, ThreeStar = 4000, FourStar = 5500, FiveStar = 7000,
                Objective = new ObjectiveConfig { Type = ObjectiveType.Survive, SurviveTurns = 15 },
                StoneBlocks = new List<StoneBlockConfig>
                {
                    // Starting stone cluster — center of board
                    new(3, 3, 2), new(3, 4, 2),
                    new(4, 3, 2), new(4, 4, 2)
                    // Boss mechanic: additional stones spawn every 3 turns (handled by BoardController extension)
                }
            }
        };

        public static LevelConfig GetLevel(int levelNumber)
        {
            return Levels.TryGetValue(levelNumber, out var config) ? config : null;
        }
    }
}
