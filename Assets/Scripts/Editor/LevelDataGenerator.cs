#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Matchmancer.Progression;
using Matchmancer.Objectives;

namespace Matchmancer.Editor
{
    public class LevelDataGenerator
    {
        private const string LEVELS_FOLDER = "Assets/Data/Levels";
        private const int TOTAL_LEVELS = 25;
        private const int BOARD_SIZE = 8;

        [MenuItem("Matchmancer/Generate Chapter 1 Levels")]
        public static void GenerateChapter1Levels()
        {
            if (!EditorUtility.DisplayDialog(
                "Generate Chapter 1 Levels",
                $"This will create {TOTAL_LEVELS} LevelData assets in {LEVELS_FOLDER}.\n\nContinue?",
                "Generate", "Cancel"))
            {
                return;
            }

            // Ensure folder exists
            EnsureLevelsFolderExists();

            // Generate all 25 levels
            for (int levelNum = 1; levelNum <= TOTAL_LEVELS; levelNum++)
            {
                GenerateSingleLevel(levelNum);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Success",
                $"Generated {TOTAL_LEVELS} levels in {LEVELS_FOLDER}",
                "OK");
        }

        private static void EnsureLevelsFolderExists()
        {
            if (!AssetDatabase.IsValidFolder(LEVELS_FOLDER))
            {
                string parentFolder = "Assets/Data";
                if (!AssetDatabase.IsValidFolder(parentFolder))
                {
                    AssetDatabase.CreateFolder("Assets", "Data");
                }
                AssetDatabase.CreateFolder(parentFolder, "Levels");
            }
        }

        private static void GenerateSingleLevel(int levelNum)
        {
            LevelData level = ScriptableObject.CreateInstance<LevelData>();

            // Stage and level index mapping
            int stageIndex = (levelNum - 1) / 10 + 1;  // 1-3
            int levelIndex = ((levelNum - 1) % 10) + 1; // 1-10
            int globalIndex = levelNum;

            level.displayName = $"Chapter 1 - Level {levelNum}";
            level.stageIndex = stageIndex;
            level.levelIndex = levelIndex;
            level.globalIndex = globalIndex;
            level.enemy = null; // Leave for future use

            // Configure based on difficulty curve
            ConfigureLevelDifficulty(level, levelNum);
            ConfigureLevelObjective(level, levelNum);
            ConfigureStoneBlocks(level, levelNum);
            ConfigureStarThresholds(level, levelNum);
            ConfigureRewards(level, levelNum);
            ConfigureStoryScenes(level, levelNum);

            // Save asset
            string fileName = $"Level_S{stageIndex}_L{levelIndex:D2}";
            string assetPath = $"{LEVELS_FOLDER}/{fileName}.asset";
            AssetDatabase.CreateAsset(level, assetPath);
        }

        private static void ConfigureLevelDifficulty(LevelData level, int levelNum)
        {
            if (levelNum <= 5)
            {
                // Tutorial: Generous moves, no complexity
                level.totalMoves = 30 - ((levelNum - 1) * 1); // 30, 29, 28, 27, 26
                level.meterCapacity = 30;
                level.designNotes = "Tutorial level. Focus on learning match mechanics.";
            }
            else if (levelNum <= 10)
            {
                // Sigil mastery: Introduce sigil mechanics
                level.totalMoves = 25 - ((levelNum - 6) * 1); // 25, 24, 23, 22, 21
                level.meterCapacity = 35;
                level.designNotes = "Sigil mastery phase. Learn special tile combinations.";
            }
            else if (levelNum <= 15)
            {
                // Glass/light blockers: Introduce stone blocks
                level.totalMoves = 22 - ((levelNum - 11) * 1); // 22, 21, 20, 19, 18
                level.meterCapacity = 40;
                level.designNotes = "Glass blockers introduced. Stone blocks impede progress.";
            }
            else if (levelNum <= 20)
            {
                // Chain blockers: Chains limit mobility
                level.totalMoves = 20 - ((levelNum - 16) * 1); // 20, 19, 18, 17, 16
                level.meterCapacity = 45;
                level.designNotes = "Chain obstacles limit movement. Plan sequences carefully.";
            }
            else if (levelNum <= 24)
            {
                // Full challenge: Tight constraints
                level.totalMoves = 18 - ((levelNum - 21) * 1); // 18, 17, 16, 15
                level.meterCapacity = 50;
                level.designNotes = "Full difficulty. Multiple obstacle types. Tight move count.";
            }
            else
            {
                // Boss level: Survive objective
                level.totalMoves = 18;
                level.meterCapacity = 60;
                level.designNotes = "Boss level. Survive escalating threats. Story climax.";
            }
        }

        private static void ConfigureLevelObjective(LevelData level, int levelNum)
        {
            LevelObjective objective = new LevelObjective();

            if (levelNum <= 5)
            {
                // Tutorial: Pure score reach
                objective.type = ObjectiveType.ReachScore;
                objective.targetScore = 5000 + (levelNum * 2000); // 7k, 9k, 11k, 13k, 15k
                objective.targetStoneClears = 0;
                objective.surviveTurns = 0;
            }
            else if (levelNum <= 10)
            {
                // Sigil mastery: Score reach with higher targets
                objective.type = ObjectiveType.ReachScore;
                objective.targetScore = 15000 + ((levelNum - 6) * 2500); // 15k, 17.5k, 20k, 22.5k, 25k
                objective.targetStoneClears = 0;
                objective.surviveTurns = 0;
            }
            else if (levelNum <= 15)
            {
                // Mix of ReachScore and ClearAllStones
                if (levelNum % 2 == 0)
                {
                    objective.type = ObjectiveType.ClearAllStones;
                    objective.targetScore = 0;
                    objective.targetStoneClears = 3 + ((levelNum - 11) / 2); // 3, 4, 5
                }
                else
                {
                    objective.type = ObjectiveType.ReachScore;
                    objective.targetScore = 25000 + ((levelNum - 11) * 1500); // 25k, 26.5k, 28k, 29.5k, 31k
                    objective.targetStoneClears = 0;
                }
                objective.surviveTurns = 0;
            }
            else if (levelNum <= 20)
            {
                // ClearAllStones primary, ReachScore secondary
                if (levelNum % 2 == 0)
                {
                    objective.type = ObjectiveType.ReachScore;
                    objective.targetScore = 30000 + ((levelNum - 16) * 2000); // 30k, 32k, 34k, 36k, 38k
                    objective.targetStoneClears = 0;
                }
                else
                {
                    objective.type = ObjectiveType.ClearAllStones;
                    objective.targetScore = 0;
                    objective.targetStoneClears = 5 + ((levelNum - 17) / 2); // 5, 5, 6, 6, 7
                }
                objective.surviveTurns = 0;
            }
            else if (levelNum <= 24)
            {
                // Full mix: ReachScore, ClearAllStones, CollectTiles
                int mod = (levelNum - 21) % 4;
                if (mod == 0)
                {
                    objective.type = ObjectiveType.ReachScore;
                    objective.targetScore = 35000 + ((levelNum - 21) * 2000); // 35k, 37k, 39k, 41k
                    objective.targetStoneClears = 0;
                }
                else if (mod == 1)
                {
                    objective.type = ObjectiveType.ClearAllStones;
                    objective.targetScore = 0;
                    objective.targetStoneClears = 7 + (levelNum - 21) / 2; // 7, 7, 8, 8
                }
                else
                {
                    objective.type = ObjectiveType.CollectTiles;
                    objective.targetScore = 12 + (levelNum - 21); // 12, 13, 14, 15
                    objective.targetStoneClears = 0;
                }
                objective.surviveTurns = 0;
            }
            else
            {
                // Level 25: Boss - Survive
                objective.type = ObjectiveType.Survive;
                objective.targetScore = 0;
                objective.targetStoneClears = 0;
                objective.surviveTurns = 8;
            }

            level.objective = objective;
        }

        private static void ConfigureStoneBlocks(LevelData level, int levelNum)
        {
            level.stoneBlocks = new List<LevelStoneBlock>();

            int blockCount = 0;
            int blockHP = 1;

            if (levelNum <= 5)
            {
                // Tutorial: No stone blocks
                blockCount = 0;
            }
            else if (levelNum <= 10)
            {
                // Sigil mastery: 0-2 blocks, 1hp each
                blockCount = (levelNum - 6) / 2; // 0, 0, 1, 1, 2
                blockHP = 1;
            }
            else if (levelNum <= 15)
            {
                // Glass/light: 3-5 blocks, 1-2hp
                blockCount = 3 + (levelNum - 11); // 3, 4, 5, 6, 7
                blockHP = 1 + ((levelNum - 11) / 3); // 1, 1, 1, 2, 2
            }
            else if (levelNum <= 20)
            {
                // Chain: 4-7 blocks, 2-3hp
                blockCount = 4 + (levelNum - 16); // 4, 5, 6, 7, 8
                blockHP = 2 + ((levelNum - 16) / 2); // 2, 2, 2, 3, 3
            }
            else if (levelNum <= 24)
            {
                // Full challenge: 5-9 blocks, 2-3hp
                blockCount = 5 + (levelNum - 21); // 5, 6, 7, 8, 9
                blockHP = 2 + ((levelNum - 21) / 2); // 2, 2, 3, 3, 3
            }
            else
            {
                // Level 25: Boss - 6 blocks, 3hp
                blockCount = 6;
                blockHP = 3;
            }

            // Place blocks randomly, avoiding center positions for early levels
            Random.InitState(levelNum * 12345); // Deterministic seed per level
            int placed = 0;
            int attempts = 0;
            int maxAttempts = blockCount * 10;

            while (placed < blockCount && attempts < maxAttempts)
            {
                int row = Random.Range(0, BOARD_SIZE);
                int col = Random.Range(0, BOARD_SIZE);

                // Avoid center positions for early levels (1-10)
                if (levelNum <= 10)
                {
                    if (row >= 2 && row <= 5 && col >= 2 && col <= 5)
                    {
                        attempts++;
                        continue;
                    }
                }

                // Check for duplicates
                bool duplicate = false;
                foreach (var block in level.stoneBlocks)
                {
                    if (block.row == row && block.col == col)
                    {
                        duplicate = true;
                        break;
                    }
                }

                if (!duplicate)
                {
                    LevelStoneBlock newBlock = new LevelStoneBlock
                    {
                        row = row,
                        col = col,
                        hp = blockHP
                    };
                    level.stoneBlocks.Add(newBlock);
                    placed++;
                }

                attempts++;
            }
        }

        private static void ConfigureStarThresholds(LevelData level, int levelNum)
        {
            // Star thresholds scale with level and difficulty
            int baseScore = 5000;

            if (levelNum <= 5)
            {
                // Tutorial: Generous thresholds
                level.oneStar = baseScore * 1;
                level.twoStar = baseScore * 2;
                level.threeStar = baseScore * 3;
                level.fourStar = baseScore * 4;
                level.fiveStar = baseScore * 5;
            }
            else if (levelNum <= 10)
            {
                // Sigil mastery
                baseScore = 8000;
                level.oneStar = baseScore * 1;
                level.twoStar = baseScore * 2;
                level.threeStar = baseScore * 3;
                level.fourStar = baseScore * 4;
                level.fiveStar = baseScore * 5;
            }
            else if (levelNum <= 15)
            {
                // Glass/light
                baseScore = 10000;
                level.oneStar = baseScore * 1;
                level.twoStar = baseScore * 2;
                level.threeStar = baseScore * 3;
                level.fourStar = baseScore * 4;
                level.fiveStar = baseScore * 5;
            }
            else if (levelNum <= 20)
            {
                // Chain
                baseScore = 12000;
                level.oneStar = baseScore * 1;
                level.twoStar = baseScore * 2;
                level.threeStar = baseScore * 3;
                level.fourStar = baseScore * 4;
                level.fiveStar = baseScore * 5;
            }
            else if (levelNum <= 24)
            {
                // Full challenge
                baseScore = 15000;
                level.oneStar = baseScore * 1;
                level.twoStar = baseScore * 2;
                level.threeStar = baseScore * 3;
                level.fourStar = baseScore * 4;
                level.fiveStar = baseScore * 5;
            }
            else
            {
                // Level 25: Boss
                baseScore = 20000;
                level.oneStar = baseScore * 1;
                level.twoStar = baseScore * 2;
                level.threeStar = baseScore * 3;
                level.fourStar = baseScore * 4;
                level.fiveStar = baseScore * 5;
            }
        }

        private static void ConfigureRewards(LevelData level, int levelNum)
        {
            // Gold and XP scale with level
            level.goldReward = 100 + (levelNum * 25); // 125, 150, 175... 725
            level.xpReward = 50 + (levelNum * 15); // 65, 80, 95... 425

            // Boss level bonus
            if (levelNum == 25)
            {
                level.goldReward = 900;
                level.xpReward = 450;
            }
        }

        private static void ConfigureStoryScenes(LevelData level, int levelNum)
        {
            level.preStorySceneId = null;
            level.postStorySceneId = null;

            if (levelNum == 1)
            {
                level.preStorySceneId = "ch1_intro";
            }

            if (levelNum == 25)
            {
                level.preStorySceneId = "ch1_climax";
                level.postStorySceneId = "ch1_epilogue";
            }
        }
    }
}

#endif
