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
    /// Populates Stage 2 content (3 enemies + 10 levels + 1 stage asset) as a
    /// one-click menu action. Idempotent: re-running updates tunings in place.
    ///
    /// Stage 2 design — "The Sigil Bazaar":
    ///   Tone: The Ward is broken; the player crosses into the Bazaar's lower
    ///         galleries where sigil merchants and rogue conjurers dwell.
    ///         Sigil mastery (4- and 5-match specials) is the core teaching.
    ///   Curve: Enemies are faster and trickier than Stage 1. The basic foe
    ///          has multi-attack from level 5 onward. Armored foe introduces
    ///          higher defense. Boss demands sustained sigil chains.
    /// </summary>
    public static class Stage2Generator
    {
        private const string DataFolder    = "Assets/Data/Stage2";
        private const string EnemiesFolder = "Assets/Data/Stage2/Enemies";
        private const string LevelsFolder  = "Assets/Data/Stage2/Levels";
        private const string StageAssetPath = "Assets/Data/Stage2/Stage_02.asset";

        // ------------------------------------------------------------------
        // Menu
        // ------------------------------------------------------------------

        [MenuItem("Matchmancer/Stage 2/Generate Stage 2 Assets", priority = 30)]
        public static void Generate()
        {
            EnsureFolder(DataFolder);
            EnsureFolder(EnemiesFolder);
            EnsureFolder(LevelsFolder);

            var swift   = UpsertSwiftFoe();
            var warded  = UpsertWardedFoe();
            var boss    = UpsertBazaarKeeper();

            var levels = new LevelData[10];
            for (int i = 0; i < 10; i++)
            {
                int levelIdx = i + 1;
                var spec = GetLevelSpec(levelIdx, swift, warded, boss);
                levels[i] = UpsertLevel(spec);
            }

            var stage = UpsertStage(levels);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Stage 2 Generated",
                "Stage 2 content is ready at:\n\n" +
                $"{EnemiesFolder}\n{LevelsFolder}\n{StageAssetPath}\n\n" +
                "Next: assign Stage_02 to LevelProgressionManager.stages[1].",
                "OK");

            Selection.activeObject = stage;
            EditorGUIUtility.PingObject(stage);
        }

        [MenuItem("Matchmancer/Stage 2/Delete Stage 2 Assets", priority = 31)]
        public static void Delete()
        {
            if (!EditorUtility.DisplayDialog(
                "Delete Stage 2?",
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

        private static EnemyData UpsertSwiftFoe()
        {
            var e = LoadOrCreate<EnemyData>(EnemiesFolder + "/Enemy_Stage2_Conjurer.asset");
            e.displayName     = "Bazaar Conjurer";
            e.loreDescription = "A street-level sigil dealer who weaves quick curses between sales. Fast hands, thin skin.";
            e.maxHp           = 100;
            e.defense         = 1f;
            e.maxArmor        = 0;
            e.baseAttackPower = 12f;
            e.attacksPerTurn  = 1;
            EditorUtility.SetDirty(e);
            return e;
        }

        private static EnemyData UpsertWardedFoe()
        {
            var e = LoadOrCreate<EnemyData>(EnemiesFolder + "/Enemy_Stage2_Wardwright.asset");
            e.displayName     = "Wardwright";
            e.loreDescription = "Armored artisan who inscribes protective glyphs onto living stone. Each broken ward reforms stronger.";
            e.maxHp           = 150;
            e.defense         = 3f;
            e.maxArmor        = 30;
            e.baseAttackPower = 11f;
            e.attacksPerTurn  = 1;
            EditorUtility.SetDirty(e);
            return e;
        }

        private static EnemyData UpsertBazaarKeeper()
        {
            var e = LoadOrCreate<EnemyData>(EnemiesFolder + "/Enemy_Stage2_Boss_Mirael.asset");
            e.displayName     = "Mirael, the Sigil Broker";
            e.loreDescription = "She reads the Soulstream like a ledger. Every sigil in the Bazaar bears her mark — cross her and the whole gallery turns hostile.";
            e.maxHp           = 320;
            e.defense         = 4f;
            e.maxArmor        = 50;
            e.baseAttackPower = 16f;
            e.attacksPerTurn  = 2;
            EditorUtility.SetDirty(e);
            return e;
        }

        // ------------------------------------------------------------------
        // Levels
        // ------------------------------------------------------------------

        private struct LevelSpec
        {
            public int       levelIndex;
            public string    displayName;
            public string    designNotes;
            public int       totalMoves;
            public EnemyData enemy;
            public int[]     starThresholds;
            public ObjectiveType objectiveType;
            public int       objectiveTarget;
            public int       goldReward;
            public int       xpReward;
        }

        private static LevelSpec GetLevelSpec(int lv, EnemyData swift, EnemyData warded, EnemyData boss)
        {
            switch (lv)
            {
                case 1: return new LevelSpec {
                    levelIndex = 1, displayName = "Gallery Gate",
                    designNotes = "Intro to Stage 2. Familiar match loop but the Conjurer hits harder than Stage 1 foes. Ease in.",
                    totalMoves = 28, enemy = swift,
                    starThresholds = new[] { 1200, 2400, 3800, 5400, 7500 },
                    objectiveType = ObjectiveType.ReachScore, objectiveTarget = 3800,
                    goldReward = 25, xpReward = 35,
                };
                case 2: return new LevelSpec {
                    levelIndex = 2, displayName = "Sigil Spark",
                    designNotes = "Teach Line Sigils: a 4-match creates a line clear. Encourage horizontal/vertical planning.",
                    totalMoves = 27, enemy = swift,
                    starThresholds = new[] { 1400, 2800, 4400, 6200, 8500 },
                    objectiveType = ObjectiveType.ReachScore, objectiveTarget = 4400,
                    goldReward = 28, xpReward = 38,
                };
                case 3: return new LevelSpec {
                    levelIndex = 3, displayName = "Star Forge",
                    designNotes = "Teach Star Sigils: an L/T-match spawns a star that clears a cross. Reward creative placement.",
                    totalMoves = 26, enemy = swift,
                    starThresholds = new[] { 1600, 3200, 5000, 7000, 9600 },
                    objectiveType = ObjectiveType.ReachScore, objectiveTarget = 5000,
                    goldReward = 30, xpReward = 42,
                };
                case 4: return new LevelSpec {
                    levelIndex = 4, displayName = "Nova Well",
                    designNotes = "Teach Nova Sigils: a 5-match creates a nova that clears all tiles of that type. Big payoff for setup.",
                    totalMoves = 25, enemy = swift,
                    starThresholds = new[] { 1800, 3600, 5600, 7800, 10800 },
                    objectiveType = ObjectiveType.ReachScore, objectiveTarget = 5600,
                    goldReward = 35, xpReward = 48,
                };
                case 5: return new LevelSpec {
                    levelIndex = 5, displayName = "Double Bind",
                    designNotes = "Conjurer gains multi-attack. Player must balance offense and defense. Sigils become essential for burst.",
                    totalMoves = 25, enemy = swift,
                    starThresholds = new[] { 2000, 3800, 6000, 8400, 11500 },
                    objectiveType = ObjectiveType.ReachScore, objectiveTarget = 6000,
                    goldReward = 38, xpReward = 52,
                };
                case 6: return new LevelSpec {
                    levelIndex = 6, displayName = "Warded Path",
                    designNotes = "First Wardwright. Heavy armor requires break tiles + sigil combos to crack efficiently.",
                    totalMoves = 24, enemy = warded,
                    starThresholds = new[] { 2200, 4200, 6400, 9000, 12500 },
                    objectiveType = ObjectiveType.ReachScore, objectiveTarget = 6400,
                    goldReward = 42, xpReward = 58,
                };
                case 7: return new LevelSpec {
                    levelIndex = 7, displayName = "Chain Reaction",
                    designNotes = "Combo focus level. Sigil + cascade chains are the path to high scores. Reward multi-step setups.",
                    totalMoves = 24, enemy = swift,
                    starThresholds = new[] { 2400, 4600, 7000, 9800, 13500 },
                    objectiveType = ObjectiveType.ReachScore, objectiveTarget = 7000,
                    goldReward = 45, xpReward = 62,
                };
                case 8: return new LevelSpec {
                    levelIndex = 8, displayName = "Glyph Maze",
                    designNotes = "Wardwright with tighter budget. Every move must count. Sigils are the only way to burst through armor fast enough.",
                    totalMoves = 23, enemy = warded,
                    starThresholds = new[] { 2600, 5000, 7600, 10600, 14500 },
                    objectiveType = ObjectiveType.ReachScore, objectiveTarget = 7600,
                    goldReward = 50, xpReward = 68,
                };
                case 9: return new LevelSpec {
                    levelIndex = 9, displayName = "The Broker's Ledger",
                    designNotes = "Pre-boss. Wardwright with high stats. Tests all sigil types. Sets the pace for Mirael.",
                    totalMoves = 22, enemy = warded,
                    starThresholds = new[] { 2800, 5400, 8200, 11400, 15500 },
                    objectiveType = ObjectiveType.ReachScore, objectiveTarget = 8200,
                    goldReward = 55, xpReward = 75,
                };
                case 10: return new LevelSpec {
                    levelIndex = 10, displayName = "Mirael, the Sigil Broker",
                    designNotes = "BOSS. Dual-attack, heavy armor, high HP. Demands sustained sigil chains and smart defensive play. ~3x rewards.",
                    totalMoves = 22, enemy = boss,
                    starThresholds = new[] { 3500, 6000, 9000, 12500, 17000 },
                    objectiveType = ObjectiveType.ReachScore, objectiveTarget = 9000,
                    goldReward = 150, xpReward = 200,
                };
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(lv));
            }
        }

        private static LevelData UpsertLevel(LevelSpec spec)
        {
            string path = $"{LevelsFolder}/Level_S2_L{spec.levelIndex:D2}.asset";
            var level = LoadOrCreate<LevelData>(path);

            level.displayName   = $"2-{spec.levelIndex}: {spec.displayName}";
            level.stageIndex    = 2;
            level.levelIndex    = spec.levelIndex;
            level.globalIndex   = 10 + spec.levelIndex; // stage 2 → global 11..20

            level.enemy         = spec.enemy;
            level.totalMoves    = spec.totalMoves;
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

            level.preStorySceneId  = spec.levelIndex == 1 ? "stage2_intro" : "";
            level.postStorySceneId = spec.levelIndex == 10 ? "stage2_outro" : "";
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
            stage.stageName    = "Stage 2: The Sigil Bazaar";
            stage.stageIndex   = 2;
            stage.stageColor   = new Color(0.28f, 0.55f, 0.68f, 1f); // deep teal
            stage.stageSummary =
                "Beyond the Threshold lies the Bazaar — a labyrinth of sigil merchants, " +
                "rogue conjurers, and warded corridors. Master the sigils or be consumed by them.";
            stage.levels                   = levels;
            stage.levelsRequiredToUnlock   = 7; // need 7 of 10 Stage 1 levels
            stage.bossLevelIndex           = 10;
            stage.stageIntroSceneId        = "stage2_intro";
            stage.stageOutroSceneId        = "stage2_outro";
            stage.stageCompletionGoldBonus = 200;

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

            string parent = Path.GetDirectoryName(folderPath).Replace('\\', '/');
            string leaf   = Path.GetFileName(folderPath);
            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
#endif
