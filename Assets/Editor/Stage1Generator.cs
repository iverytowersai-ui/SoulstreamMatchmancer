#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Matchmancer.Enemy;
using Matchmancer.Objectives;
using Matchmancer.Progression;

namespace Matchmancer.Editor
{
    /// <summary>
    /// Populates Stage 1 content (3 enemies + 10 levels + 1 stage asset) as a
    /// one-click menu action. Idempotent: re-running updates tunings in place
    /// rather than duplicating assets. Asset GUIDs are preserved across runs
    /// so references in scenes and prefabs don't break.
    ///
    /// Stage 1 design — "The Threshold":
    ///   Tone: Soulstream initiate steps through the first Ward. Low pressure,
    ///         every level teaches or reinforces a single tile role, closing
    ///         on a guardian who combines all six.
    ///   Curve: HP+atk grow gently across levels 1..9, then the boss spikes.
    ///          Every 2-3 levels the enemy archetype changes (basic → armored →
    ///          boss) so the break tile matters by level 6.
    ///
    /// After running:
    ///   • Assign the generated Stage_01 asset to LevelProgressionManager.stages[0].
    ///   • Tests still green — this only touches Editor-time asset generation.
    /// </summary>
    public static class Stage1Generator
    {
        private const string DataFolder      = "Assets/Data/Stage1";
        private const string EnemiesFolder   = "Assets/Data/Stage1/Enemies";
        private const string LevelsFolder    = "Assets/Data/Stage1/Levels";
        private const string StageAssetPath  = "Assets/Data/Stage1/Stage_01.asset";

        // ------------------------------------------------------------------
        // Menu
        // ------------------------------------------------------------------

        [MenuItem("Matchmancer/Stage 1/Generate Stage 1 Assets", priority = 20)]
        public static void Generate()
        {
            EnsureFolder(DataFolder);
            EnsureFolder(EnemiesFolder);
            EnsureFolder(LevelsFolder);

            // 1. Enemies
            var basic   = UpsertBasicFoe();
            var armored = UpsertArmoredFoe();
            var boss    = UpsertThresholdWarden();

            // 2. Levels
            var levels = new LevelData[10];
            for (int i = 0; i < 10; i++)
            {
                int levelIdx = i + 1;
                var spec = GetLevelSpec(levelIdx, basic, armored, boss);
                levels[i] = UpsertLevel(spec);
            }

            // 3. Stage
            var stage = UpsertStage(levels);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Stage 1 Generated",
                "Stage 1 content is ready at:\n\n" +
                $"{EnemiesFolder}\n{LevelsFolder}\n{StageAssetPath}\n\n" +
                "Next: assign Stage_01 to LevelProgressionManager.stages[0].",
                "OK");

            Selection.activeObject = stage;
            EditorGUIUtility.PingObject(stage);
        }

        [MenuItem("Matchmancer/Stage 1/Delete Stage 1 Assets", priority = 21)]
        public static void Delete()
        {
            if (!EditorUtility.DisplayDialog(
                "Delete Stage 1?",
                $"This permanently deletes the folder:\n\n{DataFolder}\n\nAre you sure?",
                "Delete", "Cancel"))
                return;

            if (AssetDatabase.IsValidFolder(DataFolder))
                AssetDatabase.DeleteAsset(DataFolder);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // ------------------------------------------------------------------
        // Enemies
        // ------------------------------------------------------------------

        private static EnemyData UpsertBasicFoe()
        {
            var e = LoadOrCreate<EnemyData>(EnemiesFolder + "/Enemy_Stage1_Initiate.asset");
            e.displayName     = "Veiled Initiate";
            e.loreDescription = "A novice of the Threshold. Still learning to read the Soulstream; afraid of the Ward.";
            e.maxHp           = 80;
            e.defense         = 0f;
            e.maxArmor        = 0;
            e.baseAttackPower = 8f;
            e.attacksPerTurn  = 1;
            EditorUtility.SetDirty(e);
            return e;
        }

        private static EnemyData UpsertArmoredFoe()
        {
            var e = LoadOrCreate<EnemyData>(EnemiesFolder + "/Enemy_Stage1_Warden.asset");
            e.displayName     = "Ward Sentinel";
            e.loreDescription = "Plated in a lattice of petrified Coven Seal. Slow, deliberate, and nearly unswayed by simple strikes.";
            e.maxHp           = 120;
            e.defense         = 2f;
            e.maxArmor        = 20;
            e.baseAttackPower = 10f;
            e.attacksPerTurn  = 1;
            EditorUtility.SetDirty(e);
            return e;
        }

        private static EnemyData UpsertThresholdWarden()
        {
            var e = LoadOrCreate<EnemyData>(EnemiesFolder + "/Enemy_Stage1_Boss_ThresholdWarden.asset");
            e.displayName     = "Cael, the Threshold Warden";
            e.loreDescription = "Keeper of the first Ward. What lives behind Cael's plated glare is older than the Bazaar itself.";
            e.maxHp           = 250;
            e.defense         = 3f;
            e.maxArmor        = 40;
            e.baseAttackPower = 14f;
            e.attacksPerTurn  = 1;
            EditorUtility.SetDirty(e);
            return e;
        }

        // ------------------------------------------------------------------
        // Levels
        // ------------------------------------------------------------------

        private struct LevelSpec
        {
            public int    levelIndex;
            public string displayName;
            public string designNotes;
            public int    totalMoves;
            public EnemyData enemy;
            public int[]  starThresholds; // 1..5
            public ObjectiveType objectiveType;
            public int    objectiveTarget;
            public int    goldReward;
            public int    xpReward;
        }

        private static LevelSpec GetLevelSpec(int lv, EnemyData basic, EnemyData armored, EnemyData boss)
        {
            switch (lv)
            {
                case 1: return new LevelSpec {
                    levelIndex = 1, displayName = "First Touch",
                    designNotes = "Tutorial feel. Only damage + energy tiles meaningful. Teach the swap → match loop.",
                    totalMoves = 30, enemy = basic,
                    starThresholds = new[] { 600, 1200, 2000, 3000, 4200 },
                    objectiveType = ObjectiveType.ReachScore, objectiveTarget = 2000,
                    goldReward = 15, xpReward = 20,
                };
                case 2: return new LevelSpec {
                    levelIndex = 2, displayName = "Shard Study",
                    designNotes = "Introduce Soulstream Shard (damage) as the primary clear. Gentle HP bump.",
                    totalMoves = 28, enemy = basic,
                    starThresholds = new[] { 800, 1600, 2600, 3800, 5200 },
                    objectiveType = ObjectiveType.ReachScore, objectiveTarget = 2600,
                    goldReward = 18, xpReward = 22,
                };
                case 3: return new LevelSpec {
                    levelIndex = 3, displayName = "Port Practice",
                    designNotes = "Encourage Port Rune (energy) combos to fill the ultimate meter at least once.",
                    totalMoves = 27, enemy = basic,
                    starThresholds = new[] { 900, 1800, 2900, 4200, 5800 },
                    objectiveType = ObjectiveType.ReachScore, objectiveTarget = 2900,
                    goldReward = 20, xpReward = 25,
                };
                case 4: return new LevelSpec {
                    levelIndex = 4, displayName = "Shield Up",
                    designNotes = "Coven Seal (defense) shines. Enemy hits a little harder; shielding tanks the first attack.",
                    totalMoves = 26, enemy = basic,
                    starThresholds = new[] { 1000, 2000, 3200, 4600, 6400 },
                    objectiveType = ObjectiveType.ReachScore, objectiveTarget = 3200,
                    goldReward = 22, xpReward = 28,
                };
                case 5: return new LevelSpec {
                    levelIndex = 5, displayName = "First Mark",
                    designNotes = "OZONE Mark (debuff/poison) becomes relevant. Teach stacking poison.",
                    totalMoves = 26, enemy = basic,
                    starThresholds = new[] { 1100, 2200, 3500, 5000, 7000 },
                    objectiveType = ObjectiveType.ReachScore, objectiveTarget = 3500,
                    goldReward = 25, xpReward = 30,
                };
                case 6: return new LevelSpec {
                    levelIndex = 6, displayName = "Thornfall",
                    designNotes = "First armored foe. Witchbreed Thorn (break) is required to crack the Sentinel's plating.",
                    totalMoves = 25, enemy = armored,
                    starThresholds = new[] { 1200, 2400, 3800, 5400, 7500 },
                    objectiveType = ObjectiveType.ReachScore, objectiveTarget = 3800,
                    goldReward = 28, xpReward = 35,
                };
                case 7: return new LevelSpec {
                    levelIndex = 7, displayName = "Lucky Draw",
                    designNotes = "Petsha Charm (luck) drives crits. Reward cascades heavily via crit multiplier.",
                    totalMoves = 25, enemy = basic,
                    starThresholds = new[] { 1300, 2600, 4100, 5800, 8000 },
                    objectiveType = ObjectiveType.ReachScore, objectiveTarget = 4100,
                    goldReward = 30, xpReward = 38,
                };
                case 8: return new LevelSpec {
                    levelIndex = 8, displayName = "Gauntlet of Wards",
                    designNotes = "Second Sentinel with buffed stats. All six tile roles contribute.",
                    totalMoves = 24, enemy = armored,
                    starThresholds = new[] { 1400, 2800, 4400, 6200, 8500 },
                    objectiveType = ObjectiveType.ReachScore, objectiveTarget = 4400,
                    goldReward = 35, xpReward = 42,
                };
                case 9: return new LevelSpec {
                    levelIndex = 9, displayName = "The Last Step",
                    designNotes = "Pre-boss. Armored foe, tight move budget. Previews boss pacing.",
                    totalMoves = 23, enemy = armored,
                    starThresholds = new[] { 1500, 3000, 4700, 6600, 9000 },
                    objectiveType = ObjectiveType.ReachScore, objectiveTarget = 4700,
                    goldReward = 40, xpReward = 50,
                };
                case 10: return new LevelSpec {
                    levelIndex = 10, displayName = "Cael, the Threshold Warden",
                    designNotes = "BOSS. High HP + armor. Requires break tiles to crack, then burst. Worth ~3x normal rewards.",
                    totalMoves = 22, enemy = boss,
                    starThresholds = new[] { 2000, 3800, 5600, 7800, 11000 },
                    objectiveType = ObjectiveType.ReachScore, objectiveTarget = 5600,
                    goldReward = 100, xpReward = 150,
                };
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(lv));
            }
        }

        private static LevelData UpsertLevel(LevelSpec spec)
        {
            string path = $"{LevelsFolder}/Level_S1_L{spec.levelIndex:D2}.asset";
            var level = LoadOrCreate<LevelData>(path);

            level.displayName  = $"1-{spec.levelIndex}: {spec.displayName}";
            level.stageIndex   = 1;
            level.levelIndex   = spec.levelIndex;
            level.globalIndex  = spec.levelIndex; // stage 1 → global = level
            level.enemy        = spec.enemy;
            level.totalMoves   = spec.totalMoves;
            level.meterCapacity = 100;

            level.oneStar   = spec.starThresholds[0];
            level.twoStar   = spec.starThresholds[1];
            level.threeStar = spec.starThresholds[2];
            level.fourStar  = spec.starThresholds[3];
            level.fiveStar  = spec.starThresholds[4];

            level.objective = new LevelObjective
            {
                type              = spec.objectiveType,
                targetScore       = spec.objectiveTarget,
                targetStoneClears = 0,
                surviveTurns      = 0,
            };

            if (level.stoneBlocks == null)
                level.stoneBlocks = new System.Collections.Generic.List<LevelStoneBlock>();
            else
                level.stoneBlocks.Clear();

            level.goldReward = spec.goldReward;
            level.xpReward   = spec.xpReward;

            level.preStorySceneId  = "";
            level.postStorySceneId = spec.levelIndex == 10 ? "stage1_outro" : "";
            level.designNotes = spec.designNotes;

            EditorUtility.SetDirty(level);
            return level;
        }

        // ------------------------------------------------------------------
        // Stage
        // ------------------------------------------------------------------

        private static StageData UpsertStage(LevelData[] levels)
        {
            var stage = LoadOrCreate<StageData>(StageAssetPath);
            stage.stageName   = "Stage 1: The Threshold";
            stage.stageIndex  = 1;
            stage.stageColor  = new Color(0.42f, 0.35f, 0.62f, 1f); // dusky amethyst
            stage.stageSummary =
                "The first Ward of the Soulstream. Every role of the match is taught here, " +
                "and at the edge of the Threshold, Cael waits.";
            stage.levels      = levels;
            stage.levelsRequiredToUnlock = 0; // stage 1 is always open
            stage.bossLevelIndex         = 10;
            stage.stageIntroSceneId      = "stage1_intro";
            stage.stageOutroSceneId      = "stage1_outro";
            stage.stageCompletionGoldBonus = 150;

            EditorUtility.SetDirty(stage);
            return stage;
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }
            return asset;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath)) return;

            // Walk up building parents.
            string parent = Path.GetDirectoryName(folderPath).Replace('\\', '/');
            string leaf   = Path.GetFileName(folderPath);
            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
#endif
