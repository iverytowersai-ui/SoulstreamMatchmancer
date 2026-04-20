#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using Matchmancer.Progression;
using Matchmancer.Objectives;

namespace Matchmancer.Editor
{
    /// <summary>
    /// Generates all 100 Matchmancer LevelData assets (10 stages × 10 levels).
    ///
    /// Distinct from <see cref="LevelDataGenerator"/>, which only produces 25
    /// Chapter 1 levels. This one produces the full 100-level ladder with a
    /// stage-aware difficulty curve, star thresholds, reward scaling, and
    /// story scene hooks on the key levels (1, 5, 10 of each stage).
    ///
    /// Menu: <b>Matchmancer → Generate ALL 100 Levels</b>.
    /// Output: Assets/Data/Levels/Level_S{stage}_L{level:D2}.asset
    /// </summary>
    public static class LevelDataGenerator100
    {
        private const string LEVELS_FOLDER = "Assets/Data/Levels";
        private const int STAGES           = 10;
        private const int LEVELS_PER_STAGE = 10;

        [MenuItem("Matchmancer/Generate ALL 100 Levels")]
        public static void GenerateAll100()
        {
            if (!EditorUtility.DisplayDialog(
                "Generate 100 Levels",
                $"This will create/overwrite 100 LevelData assets under {LEVELS_FOLDER}.\n\n" +
                "Existing custom edits in those assets will be lost.\n\nContinue?",
                "Generate", "Cancel"))
            {
                return;
            }

            EnsureFolder(LEVELS_FOLDER);

            int created = 0;
            try
            {
                AssetDatabase.StartAssetEditing();

                for (int stage = 1; stage <= STAGES; stage++)
                {
                    for (int levelInStage = 1; levelInStage <= LEVELS_PER_STAGE; levelInStage++)
                    {
                        BuildOrUpdateLevel(stage, levelInStage);
                        created++;
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorUtility.DisplayDialog(
                "Done",
                $"Generated {created} LevelData assets under {LEVELS_FOLDER}.",
                "OK");
        }

        // --------------------------------------------------------------
        // One level
        // --------------------------------------------------------------

        private static void BuildOrUpdateLevel(int stage, int levelInStage)
        {
            int globalIndex = (stage - 1) * LEVELS_PER_STAGE + levelInStage;
            string fileName = $"Level_S{stage}_L{levelInStage:D2}.asset";
            string path     = $"{LEVELS_FOLDER}/{fileName}";

            // Load or create
            var data = AssetDatabase.LoadAssetAtPath<LevelData>(path);
            bool isNew = (data == null);
            if (isNew)
            {
                data = ScriptableObject.CreateInstance<LevelData>();
            }

            // Identity
            data.displayName = $"Stage {stage} - Level {levelInStage}";
            data.stageIndex  = stage;
            data.levelIndex  = levelInStage;
            data.globalIndex = globalIndex;

            // Moves & meter
            data.totalMoves    = ComputeMoves(stage, levelInStage);
            data.meterCapacity = ComputeMeterCapacity(stage);

            // Star thresholds (score-based, scale with stage)
            int baseOne = 1000 + (stage - 1) * 400;
            data.oneStar   = baseOne;
            data.twoStar   = (int)(baseOne * 1.6f);
            data.threeStar = (int)(baseOne * 2.4f);
            data.fourStar  = (int)(baseOne * 3.2f);
            data.fiveStar  = (int)(baseOne * 4.2f);

            // Objective — use ReachScore as default; boss levels (level 10) survive-turns
            if (levelInStage == LEVELS_PER_STAGE)
            {
                data.objective = new LevelObjective
                {
                    type          = ObjectiveType.Survive,
                    surviveTurns  = 8 + stage, // boss fight length
                    targetScore   = data.threeStar,
                };
                data.designNotes = $"BOSS: Stage {stage} boss. Survive {8 + stage} turns.";
            }
            else
            {
                data.objective = new LevelObjective
                {
                    type         = ObjectiveType.ReachScore,
                    targetScore  = data.threeStar,
                };
                data.designNotes = StageDesignNote(stage, levelInStage);
            }

            // Rewards scale with stage * levelInStage
            data.goldReward = 10 + (stage * 5) + (levelInStage * 2);
            data.xpReward   = 15 + (stage * 8) + (levelInStage * 3);

            // Story hooks: first level of each stage gets pre-story, last gets post-story
            data.preStorySceneId  = (levelInStage == 1) ? $"story_s{stage}_intro"      : "";
            data.postStorySceneId = (levelInStage == LEVELS_PER_STAGE) ? $"story_s{stage}_boss_post" : "";

            // Write
            if (isNew)
            {
                AssetDatabase.CreateAsset(data, path);
            }
            else
            {
                EditorUtility.SetDirty(data);
            }
        }

        // --------------------------------------------------------------
        // Curves
        // --------------------------------------------------------------

        private static int ComputeMoves(int stage, int levelInStage)
        {
            // Early stages are generous; later stages tighten moves.
            // Stage 1: 25-20. Stage 5: 22-17. Stage 10: 18-14.
            int stageBase    = 26 - stage;                  // 25, 24, ... 16
            int levelPenalty = (levelInStage - 1) / 2;      // 0,0,1,1,2,2,3,3,4,4
            int moves        = stageBase - levelPenalty;
            if (moves < 12) moves = 12;
            return moves;
        }

        private static int ComputeMeterCapacity(int stage)
        {
            // Ultimate meter fills faster early; feels slower late game.
            return 30 + (stage - 1) * 5;
        }

        private static string StageDesignNote(int stage, int levelInStage)
        {
            switch (stage)
            {
                case 1:  return "Tutorial — learn basic matching and tile types.";
                case 2:  return "Sigil mastery — combine 4/5 matches for sigils.";
                case 3:  return "Stone blocks appear — plan clearings.";
                case 4:  return "Poison enemies — keep offense up through debuffs.";
                case 5:  return "Armored enemies — Break tiles matter.";
                case 6:  return "Luck tiles in rotation — crit builds pay off.";
                case 7:  return "Dense stone blocks — map awareness required.";
                case 8:  return "High-defense foes — big combos only.";
                case 9:  return "Elite enemies with multi-attacks — use shields.";
                case 10: return "Endgame — mastery across all mechanics.";
                default: return "";
            }
        }

        // --------------------------------------------------------------
        // Folder helper
        // --------------------------------------------------------------

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            string leaf   = System.IO.Path.GetFileName(path);

            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}

#endif
