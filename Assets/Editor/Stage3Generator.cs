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
    /// Populates Stage 3 content (3 enemies + 10 levels + 1 stage asset).
    /// Idempotent: re-running updates tunings in place.
    ///
    /// Stage 3 design — "The Quarry of Stones":
    ///   Tone: Raw Soulstream ore is mined here, and the galleries are
    ///         riddled with petrified stone blocks. Players learn to plan
    ///         around obstacles and clear paths through cramped boards.
    ///   Curve: Stone blocks appear from level 1. Enemy HP/armor continue
    ///          to climb. Boss has stone-spawning attacks (conceptually —
    ///          the data sets high HP and defense to simulate pressure).
    /// </summary>
    public static class Stage3Generator
    {
        private const string DataFolder    = "Assets/Data/Stage3";
        private const string EnemiesFolder = "Assets/Data/Stage3/Enemies";
        private const string LevelsFolder  = "Assets/Data/Stage3/Levels";
        private const string StageAssetPath = "Assets/Data/Stage3/Stage_03.asset";

        [MenuItem("Matchmancer/Stage 3/Generate Stage 3 Assets", priority = 40)]
        public static void Generate()
        {
            EnsureFolder(DataFolder);
            EnsureFolder(EnemiesFolder);
            EnsureFolder(LevelsFolder);

            var miner   = UpsertMinerFoe();
            var golem   = UpsertGolemFoe();
            var boss    = UpsertQuarryBoss();

            var levels = new LevelData[10];
            for (int i = 0; i < 10; i++)
            {
                int levelIdx = i + 1;
                var spec = GetLevelSpec(levelIdx, miner, golem, boss);
                levels[i] = UpsertLevel(spec);
            }

            var stage = UpsertStage(levels);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Stage 3 Generated",
                "Stage 3 content is ready at:\n\n" +
                $"{EnemiesFolder}\n{LevelsFolder}\n{StageAssetPath}\n\n" +
                "Next: assign Stage_03 to LevelProgressionManager.stages[2].",
                "OK");

            Selection.activeObject = stage;
            EditorGUIUtility.PingObject(stage);
        }

        [MenuItem("Matchmancer/Stage 3/Delete Stage 3 Assets", priority = 41)]
        public static void Delete()
        {
            if (!EditorUtility.DisplayDialog(
                "Delete Stage 3?",
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

        private static EnemyData UpsertMinerFoe()
        {
            var e = LoadOrCreate<EnemyData>(EnemiesFolder + "/Enemy_Stage3_OreDigger.asset");
            e.displayName     = "Ore Digger";
            e.loreDescription = "A hunched laborer who pries raw Soulstream ore from the walls. Weak alone, but the stones protect him.";
            e.maxHp           = 120;
            e.defense         = 2f;
            e.maxArmor        = 0;
            e.baseAttackPower = 14f;
            e.attacksPerTurn  = 1;
            EditorUtility.SetDirty(e);
            return e;
        }

        private static EnemyData UpsertGolemFoe()
        {
            var e = LoadOrCreate<EnemyData>(EnemiesFolder + "/Enemy_Stage3_StoneGolem.asset");
            e.displayName     = "Stone Golem";
            e.loreDescription = "Animated from quarry tailings. Its plating reforms after every few turns — break it fast or fight forever.";
            e.maxHp           = 200;
            e.defense         = 4f;
            e.maxArmor        = 40;
            e.baseAttackPower = 13f;
            e.attacksPerTurn  = 1;
            EditorUtility.SetDirty(e);
            return e;
        }

        private static EnemyData UpsertQuarryBoss()
        {
            var e = LoadOrCreate<EnemyData>(EnemiesFolder + "/Enemy_Stage3_Boss_Gravus.asset");
            e.displayName     = "Gravus, the Living Quarry";
            e.loreDescription = "The deepest vein of the quarry gained sentience. Gravus IS the stone — every block on the board is an extension of his will.";
            e.maxHp           = 400;
            e.defense         = 5f;
            e.maxArmor        = 60;
            e.baseAttackPower = 18f;
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
            public LevelStoneBlock[] stones;
        }

        private static LevelSpec GetLevelSpec(int lv, EnemyData miner, EnemyData golem, EnemyData boss)
        {
            switch (lv)
            {
                case 1: return new LevelSpec {
                    levelIndex = 1, displayName = "Rubble Entry",
                    designNotes = "Intro to stone blocks. Two stones in corners teach the mechanic without punishing. Match around them.",
                    totalMoves = 26, enemy = miner,
                    starThresholds = new[] { 1800, 3400, 5200, 7200, 10000 },
                    objectiveType = ObjectiveType.ReachScore, objectiveTarget = 5200,
                    goldReward = 35, xpReward = 48,
                    stones = new[] {
                        new LevelStoneBlock { row = 0, col = 0, hp = 1 },
                        new LevelStoneBlock { row = 0, col = 6, hp = 1 },
                    },
                };
                case 2: return new LevelSpec {
                    levelIndex = 2, displayName = "Narrow Vein",
                    designNotes = "Column of stones creates a narrow playable lane. Forces vertical match planning.",
                    totalMoves = 25, enemy = miner,
                    starThresholds = new[] { 2000, 3800, 5800, 8000, 11000 },
                    objectiveType = ObjectiveType.ReachScore, objectiveTarget = 5800,
                    goldReward = 38, xpReward = 52,
                    stones = new[] {
                        new LevelStoneBlock { row = 1, col = 3, hp = 2 },
                        new LevelStoneBlock { row = 2, col = 3, hp = 2 },
                        new LevelStoneBlock { row = 3, col = 3, hp = 2 },
                    },
                };
                case 3: return new LevelSpec {
                    levelIndex = 3, displayName = "Scattered Ore",
                    designNotes = "Random-feeling stone placement. Teaches awareness of stone-adjacent matching for ClearStone objective.",
                    totalMoves = 25, enemy = miner,
                    starThresholds = new[] { 2200, 4200, 6400, 8800, 12000 },
                    objectiveType = ObjectiveType.ClearAllStones, objectiveTarget = 4,
                    goldReward = 42, xpReward = 55,
                    stones = new[] {
                        new LevelStoneBlock { row = 1, col = 1, hp = 1 },
                        new LevelStoneBlock { row = 1, col = 5, hp = 1 },
                        new LevelStoneBlock { row = 4, col = 2, hp = 1 },
                        new LevelStoneBlock { row = 4, col = 4, hp = 1 },
                    },
                };
                case 4: return new LevelSpec {
                    levelIndex = 4, displayName = "Hard Rock",
                    designNotes = "HP-2 stones require two adjacent matches to clear. Teach multi-hit stone breaking.",
                    totalMoves = 24, enemy = miner,
                    starThresholds = new[] { 2400, 4600, 7000, 9600, 13000 },
                    objectiveType = ObjectiveType.ClearAllStones, objectiveTarget = 3,
                    goldReward = 45, xpReward = 60,
                    stones = new[] {
                        new LevelStoneBlock { row = 2, col = 1, hp = 2 },
                        new LevelStoneBlock { row = 2, col = 5, hp = 2 },
                        new LevelStoneBlock { row = 3, col = 3, hp = 2 },
                    },
                };
                case 5: return new LevelSpec {
                    levelIndex = 5, displayName = "Golem Wake",
                    designNotes = "First Stone Golem. Heavy armor + stones on board. Break tiles and sigils needed to crack through.",
                    totalMoves = 24, enemy = golem,
                    starThresholds = new[] { 2600, 5000, 7600, 10400, 14000 },
                    objectiveType = ObjectiveType.ReachScore, objectiveTarget = 7600,
                    goldReward = 48, xpReward = 65,
                    stones = new[] {
                        new LevelStoneBlock { row = 0, col = 3, hp = 2 },
                        new LevelStoneBlock { row = 6, col = 3, hp = 2 },
                    },
                };
                case 6: return new LevelSpec {
                    levelIndex = 6, displayName = "Walled Garden",
                    designNotes = "Ring of stones around center. Forces outside-in strategy. Sigil novas can bypass the wall.",
                    totalMoves = 23, enemy = miner,
                    starThresholds = new[] { 2800, 5400, 8200, 11200, 15000 },
                    objectiveType = ObjectiveType.ReachScore, objectiveTarget = 8200,
                    goldReward = 52, xpReward = 70,
                    stones = new[] {
                        new LevelStoneBlock { row = 2, col = 2, hp = 1 },
                        new LevelStoneBlock { row = 2, col = 4, hp = 1 },
                        new LevelStoneBlock { row = 4, col = 2, hp = 1 },
                        new LevelStoneBlock { row = 4, col = 4, hp = 1 },
                    },
                };
                case 7: return new LevelSpec {
                    levelIndex = 7, displayName = "Deep Shaft",
                    designNotes = "Tall stone columns. Limited horizontal space forces creative vertical play and sigil use.",
                    totalMoves = 23, enemy = golem,
                    starThresholds = new[] { 3000, 5800, 8800, 12000, 16000 },
                    objectiveType = ObjectiveType.ReachScore, objectiveTarget = 8800,
                    goldReward = 55, xpReward = 75,
                    stones = new[] {
                        new LevelStoneBlock { row = 0, col = 1, hp = 2 },
                        new LevelStoneBlock { row = 1, col = 1, hp = 2 },
                        new LevelStoneBlock { row = 0, col = 5, hp = 2 },
                        new LevelStoneBlock { row = 1, col = 5, hp = 2 },
                    },
                };
                case 8: return new LevelSpec {
                    levelIndex = 8, displayName = "Fault Line",
                    designNotes = "Diagonal stone barrier. Tests spatial planning. Every match must be intentional.",
                    totalMoves = 22, enemy = golem,
                    starThresholds = new[] { 3200, 6200, 9400, 12800, 17000 },
                    objectiveType = ObjectiveType.ReachScore, objectiveTarget = 9400,
                    goldReward = 60, xpReward = 82,
                    stones = new[] {
                        new LevelStoneBlock { row = 1, col = 1, hp = 2 },
                        new LevelStoneBlock { row = 2, col = 2, hp = 2 },
                        new LevelStoneBlock { row = 3, col = 3, hp = 3 },
                        new LevelStoneBlock { row = 4, col = 4, hp = 2 },
                        new LevelStoneBlock { row = 5, col = 5, hp = 2 },
                    },
                };
                case 9: return new LevelSpec {
                    levelIndex = 9, displayName = "The Core",
                    designNotes = "Pre-boss gauntlet. Dense stones + armored Golem. Must clear stones for board space while damaging enemy.",
                    totalMoves = 21, enemy = golem,
                    starThresholds = new[] { 3500, 6600, 10000, 13600, 18000 },
                    objectiveType = ObjectiveType.ClearAllStones, objectiveTarget = 6,
                    goldReward = 65, xpReward = 90,
                    stones = new[] {
                        new LevelStoneBlock { row = 0, col = 0, hp = 2 },
                        new LevelStoneBlock { row = 0, col = 6, hp = 2 },
                        new LevelStoneBlock { row = 3, col = 2, hp = 3 },
                        new LevelStoneBlock { row = 3, col = 4, hp = 3 },
                        new LevelStoneBlock { row = 6, col = 0, hp = 2 },
                        new LevelStoneBlock { row = 6, col = 6, hp = 2 },
                    },
                };
                case 10: return new LevelSpec {
                    levelIndex = 10, displayName = "Gravus, the Living Quarry",
                    designNotes = "BOSS. Massive HP + armor + dual attack. Board starts dense with stones. ~3x rewards.",
                    totalMoves = 22, enemy = boss,
                    starThresholds = new[] { 4000, 7500, 11000, 15000, 20000 },
                    objectiveType = ObjectiveType.ReachScore, objectiveTarget = 11000,
                    goldReward = 200, xpReward = 280,
                    stones = new[] {
                        new LevelStoneBlock { row = 1, col = 1, hp = 3 },
                        new LevelStoneBlock { row = 1, col = 5, hp = 3 },
                        new LevelStoneBlock { row = 3, col = 3, hp = 3 },
                        new LevelStoneBlock { row = 5, col = 1, hp = 3 },
                        new LevelStoneBlock { row = 5, col = 5, hp = 3 },
                    },
                };
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(lv));
            }
        }

        private static LevelData UpsertLevel(LevelSpec spec)
        {
            string path = $"{LevelsFolder}/Level_S3_L{spec.levelIndex:D2}.asset";
            var level = LoadOrCreate<LevelData>(path);

            level.displayName   = $"3-{spec.levelIndex}: {spec.displayName}";
            level.stageIndex    = 3;
            level.levelIndex    = spec.levelIndex;
            level.globalIndex   = 20 + spec.levelIndex; // stage 3 → global 21..30

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
                targetStoneClears = spec.objectiveType == ObjectiveType.ClearAllStones ? spec.objectiveTarget : 0,
                surviveTurns      = 0,
            };

            if (level.stoneBlocks == null)
                level.stoneBlocks = new System.Collections.Generic.List<LevelStoneBlock>();
            else
                level.stoneBlocks.Clear();

            if (spec.stones != null)
                level.stoneBlocks.AddRange(spec.stones);

            level.goldReward = spec.goldReward;
            level.xpReward   = spec.xpReward;

            level.preStorySceneId  = spec.levelIndex == 1 ? "stage3_intro" : "";
            level.postStorySceneId = spec.levelIndex == 10 ? "stage3_outro" : "";
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
            stage.stageName    = "Stage 3: The Quarry of Stones";
            stage.stageIndex   = 3;
            stage.stageColor   = new Color(0.58f, 0.45f, 0.32f, 1f); // quarry amber
            stage.stageSummary =
                "The deeper galleries where raw Soulstream ore is mined. Stone blocks " +
                "choke the board, and the workers here fight to protect what they've unearthed.";
            stage.levels                   = levels;
            stage.levelsRequiredToUnlock   = 7; // need 7 of 10 Stage 2 levels
            stage.bossLevelIndex           = 10;
            stage.stageIntroSceneId        = "stage3_intro";
            stage.stageOutroSceneId        = "stage3_outro";
            stage.stageCompletionGoldBonus = 250;

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
