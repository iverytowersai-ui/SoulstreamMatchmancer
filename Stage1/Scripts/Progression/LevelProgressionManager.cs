using System;
using UnityEngine;

namespace Matchmancer.Progression
{
    /// <summary>
    /// Single source of truth for what is unlocked and what the player is currently playing.
    /// Owns all StageData assets and exposes unlock-state queries.
    /// </summary>
    public class LevelProgressionManager : MonoBehaviour
    {
        public static LevelProgressionManager Instance { get; private set; }

        [Tooltip("All 10 StageData assets in order. Stage 1 can be populated via Stage1Bootstrap.")]
        [SerializeField] private StageData[] stages;

        // Indexed by globalIndex (1–100)
        private bool[]  levelCompleted   = new bool[101];
        private int[]   levelStars       = new int[101];
        private bool[]  levelFiveStar    = new bool[101];
        private float[] levelBestCombo   = new float[101];

        private int currentStageIndex = 1;
        private int currentLevelIndex = 1;

        public event Action<int, int>      OnLevelCompleted;
        public event Action<int>           OnStageUnlocked;
        public event Action<int, int, int> OnStarsAwarded;

        private LevelData[] allLevels = new LevelData[101];

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildLevelIndex();
        }

        private void BuildLevelIndex()
        {
            if (stages == null) return;
            foreach (StageData stage in stages)
            {
                if (stage == null) continue;
                foreach (LevelData level in stage.levels)
                {
                    if (level == null) continue;
                    if (level.globalIndex >= 1 && level.globalIndex <= 100)
                        allLevels[level.globalIndex] = level;
                }
            }
        }

        public int        StageCount                            => stages?.Length ?? 0;
        public StageData  GetStage(int i)                       => (stages != null && i >= 1 && i <= stages.Length) ? stages[i - 1] : null;
        public LevelData  GetLevel(int g)                       => (g >= 1 && g <= 100) ? allLevels[g] : null;
        public LevelData  GetLevel(int sIdx, int lIdx)          => GetStage(sIdx)?.GetLevel(lIdx);
        public bool       IsLevelCompleted(int g)               => g >= 1 && g <= 100 && levelCompleted[g];
        public int        GetStars(int g)                       => (g >= 1 && g <= 100) ? levelStars[g] : 0;
        public bool       HasFiveStar(int g)                    => g >= 1 && g <= 100 && levelFiveStar[g];
        public float      GetBestCombo(int g)                   => (g >= 1 && g <= 100) ? levelBestCombo[g] : 0f;

        public bool IsStageUnlocked(int stageIndex)
        {
            if (stageIndex == 1) return true;
            StageData stage = GetStage(stageIndex);
            StageData prev  = GetStage(stageIndex - 1);
            if (stage == null || prev == null) return false;

            int completedInPrev = 0;
            foreach (LevelData lvl in prev.levels)
                if (lvl != null && IsLevelCompleted(lvl.globalIndex)) completedInPrev++;

            return completedInPrev >= stage.levelsRequiredToUnlock;
        }

        public bool IsLevelUnlocked(int globalIndex)
        {
            LevelData level = GetLevel(globalIndex);
            if (level == null) return false;
            if (!IsStageUnlocked(level.stageIndex)) return false;
            if (level.levelIndex == 1) return true;

            LevelData prev = GetLevel(globalIndex - 1);
            return prev != null && IsLevelCompleted(prev.globalIndex);
        }

        public int TotalCompletedLevels()
        {
            int count = 0;
            for (int i = 1; i <= 100; i++) if (levelCompleted[i]) count++;
            return count;
        }

        public int TotalStars()
        {
            int count = 0;
            for (int i = 1; i <= 100; i++) count += levelStars[i];
            return count;
        }

        public LevelData CurrentLevel      => GetLevel(currentStageIndex, currentLevelIndex);
        public int       CurrentStageIndex => currentStageIndex;
        public int       CurrentLevelIndex => currentLevelIndex;

        public void SetCurrentLevel(int stageIndex, int levelIndex)
        {
            currentStageIndex = stageIndex;
            currentLevelIndex = levelIndex;
        }

        public void SetCurrentLevel(LevelData level)
        {
            if (level == null) return;
            currentStageIndex = level.stageIndex;
            currentLevelIndex = level.levelIndex;
        }

        public void RecordLevelCompletion(int globalIndex, int stars, bool fiveStar, float bestCombo)
        {
            if (globalIndex < 1 || globalIndex > 100) return;

            bool wasNew = !levelCompleted[globalIndex];
            levelCompleted[globalIndex] = true;

            if (stars > levelStars[globalIndex])
            {
                levelStars[globalIndex] = stars;
                LevelData level = GetLevel(globalIndex);
                if (level != null)
                    OnStarsAwarded?.Invoke(level.stageIndex, level.levelIndex, stars);
            }

            if (fiveStar) levelFiveStar[globalIndex] = true;
            if (bestCombo > levelBestCombo[globalIndex]) levelBestCombo[globalIndex] = bestCombo;

            LevelData lvl = GetLevel(globalIndex);
            if (lvl != null)
            {
                OnLevelCompleted?.Invoke(lvl.stageIndex, lvl.levelIndex);
                int nextStage = lvl.stageIndex + 1;
                if (nextStage <= 10 && wasNew && IsStageUnlocked(nextStage))
                    OnStageUnlocked?.Invoke(nextStage);
            }
        }

        // Save/Load hooks — Skill 20 will wire these up
        public bool[]  GetCompletedFlags() => levelCompleted;
        public int[]   GetStarsArray()     => levelStars;
        public bool[]  GetFiveStarFlags()  => levelFiveStar;
        public float[] GetBestCombos()     => levelBestCombo;

        public void LoadProgressionData(bool[] completed, int[] stars, bool[] fiveStars, float[] bestCombos)
        {
            if (completed  != null) Array.Copy(completed,  levelCompleted,  Mathf.Min(completed.Length,  101));
            if (stars      != null) Array.Copy(stars,      levelStars,      Mathf.Min(stars.Length,      101));
            if (fiveStars  != null) Array.Copy(fiveStars,  levelFiveStar,   Mathf.Min(fiveStars.Length,  101));
            if (bestCombos != null) Array.Copy(bestCombos, levelBestCombo,  Mathf.Min(bestCombos.Length, 101));
        }
    }
}
