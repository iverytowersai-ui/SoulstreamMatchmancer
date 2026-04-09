using System;

namespace Matchmancer.Progression
{
    /// <summary>
    /// Pure C# owner of all per-level progression data: completion flags,
    /// star ratings, 5-star bonus flags, and best combos. No Unity types —
    /// fully unit-testable.
    ///
    /// Star semantics:
    ///   • Star rating is 0–5 per level. Recording a lower rating never
    ///     downgrades a higher one.
    ///   • The dedicated 5-star flag tracks the special perfect clear
    ///     (no boosters / no damage taken / combo threshold met) that a
    ///     level's 5-star conditions define. Stored separately from the
    ///     rating so designers can track them independently.
    ///
    /// Unlock semantics:
    ///   • Stage 1 is always unlocked.
    ///   • Stage N (N &gt; 1) unlocks when the required number of levels in
    ///     Stage N-1 are completed. Default is "all 10".
    ///   • Within a stage, level N+1 unlocks after level N is completed.
    ///     Level 1 of any unlocked stage is always playable.
    /// </summary>
    public class ProgressionState
    {
        // Indices are 1..100 — slot 0 is unused for cleaner 1-based access.
        private readonly bool[]  _completed  = new bool [LevelIndexing.TotalLevels + 1];
        private readonly int[]   _stars      = new int  [LevelIndexing.TotalLevels + 1];
        private readonly bool[]  _fiveStar   = new bool [LevelIndexing.TotalLevels + 1];
        private readonly float[] _bestCombo  = new float[LevelIndexing.TotalLevels + 1];

        /// <summary>Required completions in stage N-1 before stage N unlocks.
        /// Length = LevelIndexing.StageCount. Index 0 (stage 1) is unused.
        /// Defaults to "all 10" for every stage.</summary>
        private readonly int[] _stageUnlockRequirement;

        // ------------------------------------------------------------------
        // Events
        // ------------------------------------------------------------------

        /// <summary>globalIndex, stars awarded</summary>
        public event Action<int, int> OnLevelCompleted;

        /// <summary>globalIndex, stars awarded (only fires when stars increase)</summary>
        public event Action<int, int> OnStarsImproved;

        /// <summary>stageIndex (2..10)</summary>
        public event Action<int> OnStageUnlocked;

        // ------------------------------------------------------------------
        // Construction
        // ------------------------------------------------------------------

        public ProgressionState()
        {
            _stageUnlockRequirement = new int[LevelIndexing.StageCount + 1];
            for (int s = 1; s <= LevelIndexing.StageCount; s++)
                _stageUnlockRequirement[s] = LevelIndexing.LevelsPerStage; // 10 = all
        }

        /// <summary>
        /// Override the default "all 10 of previous stage" unlock requirement
        /// for a specific stage. Stage 1 cannot be gated.
        /// </summary>
        public void SetStageUnlockRequirement(int stageIndex, int requiredCompletedInPrev)
        {
            if (!LevelIndexing.IsValidStageIndex(stageIndex) || stageIndex == 1) return;
            if (requiredCompletedInPrev < 0) requiredCompletedInPrev = 0;
            if (requiredCompletedInPrev > LevelIndexing.LevelsPerStage)
                requiredCompletedInPrev = LevelIndexing.LevelsPerStage;
            _stageUnlockRequirement[stageIndex] = requiredCompletedInPrev;
        }

        // ------------------------------------------------------------------
        // Queries — per level
        // ------------------------------------------------------------------

        public bool IsCompleted(int globalIndex)
            => LevelIndexing.IsValidGlobalIndex(globalIndex) && _completed[globalIndex];

        public int GetStars(int globalIndex)
            => LevelIndexing.IsValidGlobalIndex(globalIndex) ? _stars[globalIndex] : 0;

        public bool HasFiveStar(int globalIndex)
            => LevelIndexing.IsValidGlobalIndex(globalIndex) && _fiveStar[globalIndex];

        public float GetBestCombo(int globalIndex)
            => LevelIndexing.IsValidGlobalIndex(globalIndex) ? _bestCombo[globalIndex] : 0f;

        // ------------------------------------------------------------------
        // Queries — unlock gating
        // ------------------------------------------------------------------

        public bool IsStageUnlocked(int stageIndex)
        {
            if (!LevelIndexing.IsValidStageIndex(stageIndex)) return false;
            if (stageIndex == 1) return true;

            int required   = _stageUnlockRequirement[stageIndex];
            int prevStage  = stageIndex - 1;
            int completed  = 0;
            int firstGlobal = LevelIndexing.ToGlobalIndex(prevStage, 1);
            int lastGlobal  = LevelIndexing.ToGlobalIndex(prevStage, LevelIndexing.LevelsPerStage);
            for (int i = firstGlobal; i <= lastGlobal; i++)
                if (_completed[i]) completed++;
            return completed >= required;
        }

        public bool IsLevelUnlocked(int globalIndex)
        {
            if (!LevelIndexing.IsValidGlobalIndex(globalIndex)) return false;

            int stage = LevelIndexing.StageOf(globalIndex);
            int level = LevelIndexing.LevelOf(globalIndex);

            if (!IsStageUnlocked(stage)) return false;
            if (level == 1) return true;

            int prevGlobal = globalIndex - 1;
            return _completed[prevGlobal];
        }

        // ------------------------------------------------------------------
        // Queries — aggregate
        // ------------------------------------------------------------------

        public int TotalCompletedLevels()
        {
            int count = 0;
            for (int i = 1; i <= LevelIndexing.TotalLevels; i++)
                if (_completed[i]) count++;
            return count;
        }

        public int TotalStars()
        {
            int sum = 0;
            for (int i = 1; i <= LevelIndexing.TotalLevels; i++)
                sum += _stars[i];
            return sum;
        }

        public int TotalFiveStars()
        {
            int count = 0;
            for (int i = 1; i <= LevelIndexing.TotalLevels; i++)
                if (_fiveStar[i]) count++;
            return count;
        }

        public int CompletedInStage(int stageIndex)
        {
            if (!LevelIndexing.IsValidStageIndex(stageIndex)) return 0;
            int first = LevelIndexing.ToGlobalIndex(stageIndex, 1);
            int last  = LevelIndexing.ToGlobalIndex(stageIndex, LevelIndexing.LevelsPerStage);
            int count = 0;
            for (int i = first; i <= last; i++)
                if (_completed[i]) count++;
            return count;
        }

        // ------------------------------------------------------------------
        // Recording
        // ------------------------------------------------------------------

        /// <summary>
        /// Record the result of a cleared level. Never downgrades stars
        /// or best combo. Fires OnLevelCompleted every time. Fires
        /// OnStarsImproved only when star count strictly increases. Fires
        /// OnStageUnlocked when this completion pushes the next stage over
        /// its required threshold for the first time.
        /// </summary>
        public void RecordLevelCompletion(
            int   globalIndex,
            int   stars,
            bool  fiveStar,
            float bestCombo)
        {
            if (!LevelIndexing.IsValidGlobalIndex(globalIndex)) return;
            if (stars < 0) stars = 0;
            if (stars > 5) stars = 5;

            int stageIdx = LevelIndexing.StageOf(globalIndex);
            bool nextStageWasLocked = stageIdx < LevelIndexing.StageCount
                                      && !IsStageUnlocked(stageIdx + 1);

            _completed[globalIndex] = true;

            if (stars > _stars[globalIndex])
            {
                _stars[globalIndex] = stars;
                OnStarsImproved?.Invoke(globalIndex, stars);
            }

            if (fiveStar) _fiveStar[globalIndex] = true;

            if (bestCombo > _bestCombo[globalIndex])
                _bestCombo[globalIndex] = bestCombo;

            OnLevelCompleted?.Invoke(globalIndex, _stars[globalIndex]);

            if (nextStageWasLocked
                && stageIdx < LevelIndexing.StageCount
                && IsStageUnlocked(stageIdx + 1))
            {
                OnStageUnlocked?.Invoke(stageIdx + 1);
            }
        }

        // ------------------------------------------------------------------
        // Save/load
        // ------------------------------------------------------------------

        /// <summary>
        /// Returns a snapshot for the save system. Arrays are fresh copies —
        /// mutating them will not affect internal state.
        /// </summary>
        public Snapshot CreateSnapshot()
        {
            return new Snapshot
            {
                Completed = (bool[]) _completed.Clone(),
                Stars     = (int[])  _stars.Clone(),
                FiveStars = (bool[]) _fiveStar.Clone(),
                BestCombo = (float[])_bestCombo.Clone(),
            };
        }

        /// <summary>
        /// Replace all state from a loaded snapshot. Null or mis-sized arrays
        /// are ignored (keeping whatever is already in place).
        /// </summary>
        public void LoadFromSnapshot(Snapshot snapshot)
        {
            if (snapshot == null) return;
            int len = _completed.Length;
            CopyIfMatch(snapshot.Completed, _completed, len);
            CopyIfMatch(snapshot.Stars,     _stars,     len);
            CopyIfMatch(snapshot.FiveStars, _fiveStar,  len);
            CopyIfMatch(snapshot.BestCombo, _bestCombo, len);
        }

        private static void CopyIfMatch<T>(T[] src, T[] dst, int expectedLen)
        {
            if (src == null || dst == null) return;
            int n = Math.Min(src.Length, expectedLen);
            Array.Copy(src, dst, n);
        }

        /// <summary>Raw per-level state snapshot. Used by SaveSystem (Skill 20).</summary>
        public class Snapshot
        {
            public bool[]  Completed;
            public int[]   Stars;
            public bool[]  FiveStars;
            public float[] BestCombo;
        }
    }
}
