using UnityEngine;
using Matchmancer.Combat;
using Matchmancer.Character;

namespace Matchmancer.Progression
{
    /// <summary>
    /// Defines one battle level in full.
    /// Create via right-click → Create → Matchmancer → Level Data,
    /// or auto-populate via the Stage1Bootstrap editor tool.
    /// </summary>
    [CreateAssetMenu(fileName = "Level_S1_L1", menuName = "Matchmancer/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Identity")]
        public string  levelName        = "Level 1-1";
        public int     stageIndex       = 1;
        public int     levelIndex       = 1;
        public int     globalIndex      = 1;

        [Header("Board Config")]
        public int     boardWidth       = 8;
        public int     boardHeight      = 8;
        public bool[]  blockedCells;

        [Header("Combat")]
        public EnemyData enemy;
        public int     moveLimit        = 20;

        [Header("Tile Config")]
        [Tooltip("Empty = all 6 types active.")]
        public TileType[] allowedTileTypes;

        [Header("Star Thresholds")]
        public float   star1DamageThreshold  = 0f;
        public int     star2MoveThreshold    = 15;
        public int     star3MoveThreshold    = 10;

        [Header("5-Star Conditions")]
        public int     fiveStarMoveThreshold = 8;
        public float   fiveStarMinCombo      = 3f;
        public bool    fiveStarNoBoostersRequired = true;
        public bool    fiveStarNoDamageTaken = false;

        [Header("Rewards")]
        public int     goldReward       = 10;
        public int     xpReward         = 15;
        public GearData firstClearGearReward;

        [Header("Story")]
        public string  preStorySceneId  = "";
        public string  postStorySceneId = "";

        [Header("Difficulty Tags")]
        public bool    introducesNewMechanic = false;
        [TextArea] public string designNotes = "";
    }
}
