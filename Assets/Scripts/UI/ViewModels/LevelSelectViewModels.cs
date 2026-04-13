namespace Matchmancer.UI
{
    [System.Serializable]
    public struct LevelSelectViewModel
    {
        public int currentLevelIndex;          // 0-99
        public StageViewModel[] stages;        // 10 stages
    }

    [System.Serializable]
    public struct StageViewModel
    {
        public string chapterTitle;            // "THE HOLLOW GROVE"
        public int stageNumber;                // 1-10
        public bool isUnlocked;
        public bool isComingSoon;
        public LevelNodeViewModel[] levels;    // 10 levels
    }

    [System.Serializable]
    public struct LevelNodeViewModel
    {
        public int levelNumber;                // 1-100 (global) or 1-10 (local)
        public string enemyName;               // "Shadow Stalker"
        public bool isBoss;
        public bool isLocked;
        public bool isCurrent;
        public int bestStars;                  // 0-3 (0 = not completed)
        public int moveLimit;
        public int enemyHP;
        public int boardWidth;
        public int boardHeight;
        public string specialCondition;        // "Armor tiles", "Timed", etc.
    }
}
