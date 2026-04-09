namespace Matchmancer.Progression
{
    /// <summary>
    /// Canonical mapping between (stage, level) pairs and the flat global
    /// 1..100 index used by all progression state. Convention:
    ///   globalIndex = (stageIndex - 1) * LevelsPerStage + levelIndex
    ///
    /// Pure static — no Unity types — so progression logic can be unit tested
    /// without any ScriptableObjects.
    /// </summary>
    public static class LevelIndexing
    {
        public const int StageCount     = 10;
        public const int LevelsPerStage = 10;
        public const int TotalLevels    = StageCount * LevelsPerStage; // 100

        public static int ToGlobalIndex(int stageIndex, int levelIndex)
            => (stageIndex - 1) * LevelsPerStage + levelIndex;

        public static int StageOf(int globalIndex)
            => ((globalIndex - 1) / LevelsPerStage) + 1;

        public static int LevelOf(int globalIndex)
            => ((globalIndex - 1) % LevelsPerStage) + 1;

        public static bool IsValidGlobalIndex(int globalIndex)
            => globalIndex >= 1 && globalIndex <= TotalLevels;

        public static bool IsValidStageIndex(int stageIndex)
            => stageIndex >= 1 && stageIndex <= StageCount;

        public static bool IsValidLevelIndex(int levelIndex)
            => levelIndex >= 1 && levelIndex <= LevelsPerStage;
    }
}
