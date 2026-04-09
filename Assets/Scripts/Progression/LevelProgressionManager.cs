using System;
using UnityEngine;

namespace Matchmancer.Progression
{
    /// <summary>
    /// Single scene-side owner of progression. Wraps the pure-C#
    /// <see cref="ProgressionState"/> and the Unity-side
    /// <see cref="StageData"/> assets.
    ///
    /// Responsibilities:
    ///   • Build a lookup from globalIndex (1..100) to <see cref="LevelData"/>.
    ///   • Track which level the player is currently playing.
    ///   • Expose unlock queries + completion recording.
    ///   • Persist/restore via <see cref="ProgressionState.Snapshot"/>.
    ///
    /// The manager is a DontDestroyOnLoad singleton — one instance lives
    /// across scene loads so the hub/menu/battle scenes all see the same
    /// data.
    /// </summary>
    public class LevelProgressionManager : MonoBehaviour
    {
        // ------------------------------------------------------------------
        // Singleton
        // ------------------------------------------------------------------

        public static LevelProgressionManager Instance { get; private set; }

        // ------------------------------------------------------------------
        // Inspector
        // ------------------------------------------------------------------

        [Header("Stages")]
        [Tooltip("Must contain exactly 10 StageData assets in order (Stage 1..Stage 10).")]
        [SerializeField] private StageData[] stages = new StageData[LevelIndexing.StageCount];

        // ------------------------------------------------------------------
        // Runtime
        // ------------------------------------------------------------------

        private readonly LevelData[] _allLevels = new LevelData[LevelIndexing.TotalLevels + 1];

        private int _currentStageIndex = 1;
        private int _currentLevelIndex = 1;

        /// <summary>Pure C# state. Exposed so UI/save can subscribe directly.</summary>
        public ProgressionState State { get; private set; }

        public int  CurrentStageIndex => _currentStageIndex;
        public int  CurrentLevelIndex => _currentLevelIndex;
        public int  CurrentGlobalIndex => LevelIndexing.ToGlobalIndex(_currentStageIndex, _currentLevelIndex);

        // Forwarded events so listeners can bind once to the manager.
        public event Action<int, int> OnLevelCompleted;   // globalIndex, stars
        public event Action<int, int> OnStarsImproved;    // globalIndex, stars
        public event Action<int>      OnStageUnlocked;    // stageIndex

        // ------------------------------------------------------------------
        // Lifecycle
        // ------------------------------------------------------------------

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            State = new ProgressionState();
            State.OnLevelCompleted += ForwardLevelCompleted;
            State.OnStarsImproved  += ForwardStarsImproved;
            State.OnStageUnlocked  += ForwardStageUnlocked;

            ApplyStageUnlockRequirementsFromAssets();
            BuildLevelIndex();
        }

        private void OnDestroy()
        {
            if (State != null)
            {
                State.OnLevelCompleted -= ForwardLevelCompleted;
                State.OnStarsImproved  -= ForwardStarsImproved;
                State.OnStageUnlocked  -= ForwardStageUnlocked;
            }
            if (Instance == this) Instance = null;
        }

        private void ForwardLevelCompleted(int g, int s) => OnLevelCompleted?.Invoke(g, s);
        private void ForwardStarsImproved (int g, int s) => OnStarsImproved?.Invoke(g, s);
        private void ForwardStageUnlocked (int s)        => OnStageUnlocked?.Invoke(s);

        // ------------------------------------------------------------------
        // Index build
        // ------------------------------------------------------------------

        private void ApplyStageUnlockRequirementsFromAssets()
        {
            if (stages == null) return;
            for (int i = 0; i < stages.Length; i++)
            {
                var stage = stages[i];
                if (stage == null) continue;
                if (stage.stageIndex <= 1) continue;
                State.SetStageUnlockRequirement(stage.stageIndex, stage.levelsRequiredToUnlock);
            }
        }

        private void BuildLevelIndex()
        {
            if (stages == null) return;
            for (int i = 0; i < stages.Length; i++)
            {
                var stage = stages[i];
                if (stage == null || stage.levels == null) continue;
                for (int j = 0; j < stage.levels.Length; j++)
                {
                    var level = stage.levels[j];
                    if (level == null) continue;
                    if (LevelIndexing.IsValidGlobalIndex(level.globalIndex))
                        _allLevels[level.globalIndex] = level;
                }
            }
        }

        // ------------------------------------------------------------------
        // Queries
        // ------------------------------------------------------------------

        public int       StageCount                => stages != null ? stages.Length : 0;

        public StageData GetStage(int stageIndex)
        {
            if (stages == null) return null;
            if (!LevelIndexing.IsValidStageIndex(stageIndex)) return null;
            if (stageIndex - 1 >= stages.Length) return null;
            return stages[stageIndex - 1];
        }

        public LevelData GetLevel(int globalIndex)
            => LevelIndexing.IsValidGlobalIndex(globalIndex) ? _allLevels[globalIndex] : null;

        public LevelData GetLevel(int stageIndex, int levelIndex)
            => GetLevel(LevelIndexing.ToGlobalIndex(stageIndex, levelIndex));

        public bool  IsStageUnlocked(int stageIndex) => State.IsStageUnlocked(stageIndex);
        public bool  IsLevelUnlocked(int globalIndex) => State.IsLevelUnlocked(globalIndex);
        public bool  IsLevelCompleted(int globalIndex) => State.IsCompleted(globalIndex);
        public int   GetStars(int globalIndex) => State.GetStars(globalIndex);
        public bool  HasFiveStar(int globalIndex) => State.HasFiveStar(globalIndex);
        public float GetBestCombo(int globalIndex) => State.GetBestCombo(globalIndex);
        public int   TotalCompletedLevels() => State.TotalCompletedLevels();
        public int   TotalStars() => State.TotalStars();

        // ------------------------------------------------------------------
        // Current level pointer
        // ------------------------------------------------------------------

        public LevelData CurrentLevel => GetLevel(_currentStageIndex, _currentLevelIndex);

        public void SetCurrentLevel(int stageIndex, int levelIndex)
        {
            _currentStageIndex = Mathf.Clamp(stageIndex, 1, LevelIndexing.StageCount);
            _currentLevelIndex = Mathf.Clamp(levelIndex, 1, LevelIndexing.LevelsPerStage);
        }

        public void SetCurrentLevel(LevelData level)
        {
            if (level == null) return;
            SetCurrentLevel(level.stageIndex, level.levelIndex);
        }

        // ------------------------------------------------------------------
        // Recording
        // ------------------------------------------------------------------

        public void RecordLevelCompletion(int globalIndex, int stars, bool fiveStar, float bestCombo)
            => State.RecordLevelCompletion(globalIndex, stars, fiveStar, bestCombo);

        // ------------------------------------------------------------------
        // Save / Load
        // ------------------------------------------------------------------

        public ProgressionState.Snapshot CreateSnapshot() => State.CreateSnapshot();
        public void LoadFromSnapshot(ProgressionState.Snapshot snapshot)
            => State.LoadFromSnapshot(snapshot);
    }
}
