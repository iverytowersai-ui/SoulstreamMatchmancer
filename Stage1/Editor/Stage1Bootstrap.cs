#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Matchmancer.Combat;
using Matchmancer.Progression;

namespace Matchmancer.EditorTools
{
    /// <summary>
    /// One-click Stage 1 content generator.
    /// Menu: Matchmancer → Bootstrap Stage 1
    ///
    /// Creates:
    ///   Assets/ScriptableObjects/Enemies/Stage1/*.asset        (4 EnemyData)
    ///   Assets/ScriptableObjects/Levels/Stage1/Level_S1_L01..10.asset (10 LevelData)
    ///   Assets/ScriptableObjects/Stages/Stage_01.asset         (1 StageData)
    ///
    /// Values come from the Skill 17 difficulty ladder:
    ///   L1–L3  Shade Wraiths     | 8×8  | 25 moves | 4 tile types
    ///   L4–L7  Wraith Sentinels  | 8×8  | 22 moves | all 6 tile types
    ///   L8–L9  Armored Shades    | 8×8  | 20 moves | armor layer added
    ///   L10    The Hollow Judge  | 8×8  | 20 moves | boss, 300 HP, attacks every turn
    /// </summary>
    public static class Stage1Bootstrap
    {
        private const string ENEMY_DIR = "Assets/ScriptableObjects/Enemies/Stage1";
        private const string LEVEL_DIR = "Assets/ScriptableObjects/Levels/Stage1";
        private const string STAGE_DIR = "Assets/ScriptableObjects/Stages";

        [MenuItem("Matchmancer/Bootstrap Stage 1")]
        public static void Build()
        {
            EnsureDir(ENEMY_DIR);
            EnsureDir(LEVEL_DIR);
            EnsureDir(STAGE_DIR);

            // 1. Enemies
            EnemyData shadeWraith   = CreateEnemy("ShadeWraith",
                                                  "Shade Wraith",
                                                  hp: 80, armor: 0, atk: 8, cadence: 3,
                                                  elite: false, poison: false, regen: false,
                                                  lore: "A thin veil of hunger given shape by the Soulstream.");

            EnemyData wraithSentinel = CreateEnemy("WraithSentinel",
                                                  "Wraith Sentinel",
                                                  hp: 140, armor: 0, atk: 12, cadence: 3,
                                                  elite: false, poison: false, regen: false,
                                                  lore: "Bound to watch the Threshold. They do not blink.");

            EnemyData armoredShade   = CreateEnemy("ArmoredShade",
                                                  "Armored Shade",
                                                  hp: 180, armor: 30, atk: 14, cadence: 3,
                                                  elite: true, poison: false, regen: false,
                                                  lore: "Half-forged in the Coven's rituals, clad in still-cooling plate.");

            EnemyData hollowJudge    = CreateBoss("HollowJudge",
                                                  "The Hollow Judge",
                                                  hp: 300, armor: 0, atk: 18, cadence: 1,
                                                  lore: "Who presides at the Threshold, and who may pass.");

            // 2. Levels — all 10 of Stage 1
            LevelData[] lvls = new LevelData[10];

            // L1–L3 : Shade Wraiths, 4 tile types (teaches Damage/Energy/Defense/Luck)
            TileType[] fourTypes = { TileType.Damage, TileType.Energy, TileType.Defense, TileType.Luck };

            lvls[0] = CreateLevel(1, 1, 1,
                name: "The First Spark",
                enemy: shadeWraith, board: 8, moves: 25, types: fourTypes,
                s2: 20, s3: 16, fiveStarMoves: 14, fiveStarCombo: 2f,
                gold: 10, xp: 15, introMech: true,
                notes: "Tutorial level. Teaches swap, match, damage. No time pressure.");

            lvls[1] = CreateLevel(1, 2, 2,
                name: "Echoes in the Grey",
                enemy: shadeWraith, board: 8, moves: 25, types: fourTypes,
                s2: 18, s3: 14, fiveStarMoves: 12, fiveStarCombo: 2f,
                gold: 10, xp: 15,
                notes: "Introduces Energy tiles filling the ultimate meter visually.");

            lvls[2] = CreateLevel(1, 3, 3,
                name: "Threshold's Edge",
                enemy: shadeWraith, board: 8, moves: 25, types: fourTypes,
                s2: 18, s3: 14, fiveStarMoves: 12, fiveStarCombo: 2.5f,
                gold: 12, xp: 18,
                notes: "Last 4-tile-type level. Next level unlocks Debuff and Break.");

            // L4–L7 : Wraith Sentinels, all 6 tile types
            lvls[3] = CreateLevel(1, 4, 4,
                name: "The Sentinels Wake",
                enemy: wraithSentinel, board: 8, moves: 22, types: null,
                s2: 17, s3: 13, fiveStarMoves: 11, fiveStarCombo: 2.5f,
                gold: 15, xp: 22, introMech: true,
                notes: "Introduces Debuff and Break tiles. All 6 tile types active.");

            lvls[4] = CreateLevel(1, 5, 5,
                name: "Silent Watch",
                enemy: wraithSentinel, board: 8, moves: 22, types: null,
                s2: 16, s3: 12, fiveStarMoves: 10, fiveStarCombo: 3f,
                gold: 15, xp: 22,
                notes: "Standard combat. Combo pressure starts showing in 5-star requirement.");

            lvls[5] = CreateLevel(1, 6, 6,
                name: "Under Cold Lanterns",
                enemy: wraithSentinel, board: 8, moves: 22, types: null,
                s2: 16, s3: 12, fiveStarMoves: 10, fiveStarCombo: 3f,
                gold: 15, xp: 22,
                notes: "Higher HP. Teaches combo extension via Luck tiles.");

            lvls[6] = CreateLevel(1, 7, 7,
                name: "The Grey Procession",
                enemy: wraithSentinel, board: 8, moves: 22, types: null,
                s2: 15, s3: 11, fiveStarMoves: 9, fiveStarCombo: 3f,
                gold: 18, xp: 26,
                notes: "Sentinel cluster. Last 22-move level before armor introduces.");

            // L8–L9 : Armored Shades, armor layer
            lvls[7] = CreateLevel(1, 8, 8,
                name: "Forged in Coldfire",
                enemy: armoredShade, board: 8, moves: 20, types: null,
                s2: 15, s3: 11, fiveStarMoves: 9, fiveStarCombo: 3f,
                gold: 22, xp: 30, introMech: true,
                notes: "First armored enemy. Break tiles become essential.");

            lvls[8] = CreateLevel(1, 9, 9,
                name: "Plated Hunger",
                enemy: armoredShade, board: 8, moves: 20, types: null,
                s2: 14, s3: 10, fiveStarMoves: 8, fiveStarCombo: 3.5f,
                gold: 22, xp: 30,
                notes: "Harder armor variant. Reward first-clear gear here.");

            // L10 : Boss — The Hollow Judge
            lvls[9] = CreateLevel(1, 10, 10,
                name: "The Hollow Judge",
                enemy: hollowJudge, board: 8, moves: 20, types: null,
                s2: 16, s3: 12, fiveStarMoves: 10, fiveStarCombo: 4f,
                gold: 50, xp: 75, introMech: true,
                notes: "Stage 1 boss. 300 HP, attacks every turn. All 6 tile types. Signature first win.");

            // 3. Stage wrapper
            StageData stage = ScriptableObject.CreateInstance<StageData>();
            stage.stageName      = "Stage 1: The Threshold";
            stage.stageIndex     = 1;
            stage.stageColor     = new Color(0.35f, 0.32f, 0.45f);
            stage.stageSummary   = "The veil between the living world and the Soulstream is thinnest here. " +
                                   "Wraiths gather. Something older presides.";
            stage.levels         = lvls;
            stage.levelsRequiredToUnlock = 10;
            stage.bossLevelIndex = 10;
            stage.stageIntroSceneId = "story_s1_intro";
            stage.stageOutroSceneId = "story_s1_outro";
            stage.stageCompletionGoldBonus = 100;

            AssetDatabase.CreateAsset(stage, $"{STAGE_DIR}/Stage_01.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Stage1Bootstrap] Stage 1 populated: 4 enemies, 10 levels, 1 stage asset.");
            Selection.activeObject = stage;
            EditorGUIUtility.PingObject(stage);
        }

        // ---------- helpers ----------

        private static LevelData CreateLevel(int stage, int level, int global,
                                             string name, EnemyData enemy,
                                             int board, int moves, TileType[] types,
                                             int s2, int s3, int fiveStarMoves, float fiveStarCombo,
                                             int gold, int xp,
                                             bool introMech = false,
                                             string notes = "")
        {
            LevelData lvl = ScriptableObject.CreateInstance<LevelData>();
            lvl.levelName   = $"Level {stage}-{level}: {name}";
            lvl.stageIndex  = stage;
            lvl.levelIndex  = level;
            lvl.globalIndex = global;

            lvl.boardWidth  = board;
            lvl.boardHeight = board;

            lvl.enemy       = enemy;
            lvl.moveLimit   = moves;

            lvl.allowedTileTypes = types; // null = all 6 types

            lvl.star1DamageThreshold     = 0f;       // always awarded on victory
            lvl.star2MoveThreshold       = s2;
            lvl.star3MoveThreshold       = s3;

            lvl.fiveStarMoveThreshold    = fiveStarMoves;
            lvl.fiveStarMinCombo         = fiveStarCombo;
            lvl.fiveStarNoBoostersRequired = true;
            lvl.fiveStarNoDamageTaken    = false;

            lvl.goldReward  = gold;
            lvl.xpReward    = xp;

            lvl.introducesNewMechanic = introMech;
            lvl.designNotes = notes;

            string file = $"Level_S{stage:D1}_L{level:D2}.asset";
            AssetDatabase.CreateAsset(lvl, $"{LEVEL_DIR}/{file}");
            return lvl;
        }

        private static EnemyData CreateEnemy(string file, string displayName,
                                             int hp, int armor, int atk, int cadence,
                                             bool elite, bool poison, bool regen,
                                             string lore)
        {
            EnemyData e = ScriptableObject.CreateInstance<EnemyData>();
            e.enemyName          = displayName;
            e.maxHP              = hp;
            e.armor              = armor;
            e.attackPower        = atk;
            e.attackCadence      = cadence;
            e.isBoss             = false;
            e.isElite            = elite;
            e.appliesPoisonOnHit = poison;
            e.regeneratesArmor   = regen;
            e.loreBlurb          = lore;
            AssetDatabase.CreateAsset(e, $"{ENEMY_DIR}/{file}.asset");
            return e;
        }

        private static EnemyData CreateBoss(string file, string displayName,
                                            int hp, int armor, int atk, int cadence,
                                            string lore)
        {
            EnemyData e = ScriptableObject.CreateInstance<EnemyData>();
            e.enemyName          = displayName;
            e.maxHP              = hp;
            e.armor              = armor;
            e.attackPower        = atk;
            e.attackCadence      = cadence;  // 1 = every player move
            e.isBoss             = true;
            e.isElite            = true;
            e.loreBlurb          = lore;
            AssetDatabase.CreateAsset(e, $"{ENEMY_DIR}/{file}.asset");
            return e;
        }

        private static void EnsureDir(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string[] parts = path.Split('/');
            string current = parts[0]; // "Assets"
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif
