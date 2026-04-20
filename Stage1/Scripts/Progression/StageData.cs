using UnityEngine;
using Matchmancer.Character;

namespace Matchmancer.Progression
{
    /// <summary>
    /// Groups 10 levels into one stage.
    /// Create via right-click → Create → Matchmancer → Stage Data.
    /// </summary>
    [CreateAssetMenu(fileName = "Stage_01", menuName = "Matchmancer/Stage Data")]
    public class StageData : ScriptableObject
    {
        [Header("Identity")]
        public string    stageName      = "Stage 1: The Threshold";
        public int       stageIndex     = 1;
        public Sprite    stageArt;
        public Color     stageColor     = Color.white;
        [TextArea] public string stageSummary;

        [Header("Levels")]
        [Tooltip("Must contain exactly 10 LevelData assets in order.")]
        public LevelData[] levels       = new LevelData[10];

        [Header("Unlock")]
        public int       levelsRequiredToUnlock = 10;

        [Header("Boss")]
        public int       bossLevelIndex  = 10;

        [Header("Story")]
        public string    stageIntroSceneId = "";
        public string    stageOutroSceneId = "";

        [Header("Rewards")]
        public GearData  stageCompletionGearReward;
        public int       stageCompletionGoldBonus = 50;

        public LevelData GetLevel(int levelIndex)
        {
            if (levelIndex < 1 || levelIndex > levels.Length) return null;
            return levels[levelIndex - 1];
        }

        public LevelData GetBossLevel() => GetLevel(bossLevelIndex);
    }
}
