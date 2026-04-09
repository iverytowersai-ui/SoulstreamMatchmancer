using UnityEngine;

namespace Matchmancer.Progression
{
    /// <summary>
    /// Groups 10 <see cref="LevelData"/> assets into a single stage.
    /// One asset per stage, 10 total. Assigned to
    /// <see cref="LevelProgressionManager.stages"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "Stage_01", menuName = "Matchmancer/Stage Data")]
    public class StageData : ScriptableObject
    {
        [Header("Identity")]
        public string  stageName   = "Stage 1: The Threshold";
        [Min(1)] public int stageIndex = 1;
        public Sprite  stageArt;
        public Color   stageColor  = Color.white;
        [TextArea] public string stageSummary;

        [Header("Levels (must contain 10 in order)")]
        public LevelData[] levels = new LevelData[LevelIndexing.LevelsPerStage];

        [Header("Unlock")]
        [Tooltip("Completions of the previous stage required to unlock this stage. " +
                 "Ignored for Stage 1.")]
        [Range(0, 10)]
        public int levelsRequiredToUnlock = LevelIndexing.LevelsPerStage;

        [Header("Boss")]
        [Tooltip("Level index (1..10) that is the stage boss. Usually 10.")]
        [Range(1, 10)]
        public int bossLevelIndex = 10;

        [Header("Story Hooks")]
        public string stageIntroSceneId = "";
        public string stageOutroSceneId = "";

        [Header("Stage Rewards")]
        [Min(0)] public int stageCompletionGoldBonus = 50;

        // ------------------------------------------------------------------
        // Convenience
        // ------------------------------------------------------------------

        public LevelData GetLevel(int levelIndexOneBased)
        {
            if (levels == null) return null;
            if (levelIndexOneBased < 1 || levelIndexOneBased > levels.Length) return null;
            return levels[levelIndexOneBased - 1];
        }

        public LevelData GetBossLevel() => GetLevel(bossLevelIndex);

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (stageIndex < 1) stageIndex = 1;
            if (stageIndex > LevelIndexing.StageCount) stageIndex = LevelIndexing.StageCount;
            if (levels == null || levels.Length != LevelIndexing.LevelsPerStage)
            {
                var resized = new LevelData[LevelIndexing.LevelsPerStage];
                if (levels != null)
                {
                    int n = Mathf.Min(levels.Length, resized.Length);
                    for (int i = 0; i < n; i++) resized[i] = levels[i];
                }
                levels = resized;
            }
        }
#endif
    }
}
