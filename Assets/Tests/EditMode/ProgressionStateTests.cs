using NUnit.Framework;
using Matchmancer.Progression;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class ProgressionStateTests
    {
        private ProgressionState _state;

        [SetUp]
        public void SetUp()
        {
            _state = new ProgressionState();
        }

        private static int G(int stage, int level) => LevelIndexing.ToGlobalIndex(stage, level);

        /// <summary>Clear all 10 levels of a stage with 3 stars each.</summary>
        private void ClearStage(int stageIndex)
        {
            for (int i = 1; i <= LevelIndexing.LevelsPerStage; i++)
                _state.RecordLevelCompletion(G(stageIndex, i), stars: 3, fiveStar: false, bestCombo: 0f);
        }

        // ==================================================================
        // LevelIndexing sanity
        // ==================================================================

        [Test]
        public void LevelIndexing_RoundTrip()
        {
            for (int s = 1; s <= 10; s++)
            for (int l = 1; l <= 10; l++)
            {
                int g = LevelIndexing.ToGlobalIndex(s, l);
                Assert.AreEqual(s, LevelIndexing.StageOf(g));
                Assert.AreEqual(l, LevelIndexing.LevelOf(g));
            }
        }

        [Test]
        public void LevelIndexing_TotalIs100()
        {
            Assert.AreEqual(100, LevelIndexing.TotalLevels);
            Assert.AreEqual(1,   LevelIndexing.ToGlobalIndex(1, 1));
            Assert.AreEqual(100, LevelIndexing.ToGlobalIndex(10, 10));
            Assert.AreEqual(50,  LevelIndexing.ToGlobalIndex(5, 10));
            Assert.AreEqual(51,  LevelIndexing.ToGlobalIndex(6, 1));
        }

        // ==================================================================
        // Initial state
        // ==================================================================

        [Test]
        public void InitialState_NothingCompleted()
        {
            Assert.AreEqual(0, _state.TotalCompletedLevels());
            Assert.AreEqual(0, _state.TotalStars());
            Assert.AreEqual(0, _state.TotalFiveStars());
            for (int i = 1; i <= 100; i++)
            {
                Assert.IsFalse(_state.IsCompleted(i));
                Assert.AreEqual(0, _state.GetStars(i));
            }
        }

        [Test]
        public void InitialState_Stage1Unlocked()
        {
            Assert.IsTrue(_state.IsStageUnlocked(1));
        }

        [Test]
        public void InitialState_OtherStagesLocked()
        {
            for (int s = 2; s <= 10; s++)
                Assert.IsFalse(_state.IsStageUnlocked(s), $"Stage {s}");
        }

        [Test]
        public void InitialState_OnlyLevel1Of1Unlocked()
        {
            Assert.IsTrue (_state.IsLevelUnlocked(G(1, 1)));
            Assert.IsFalse(_state.IsLevelUnlocked(G(1, 2)));
            Assert.IsFalse(_state.IsLevelUnlocked(G(2, 1)));
        }

        // ==================================================================
        // Recording basic completion
        // ==================================================================

        [Test]
        public void RecordCompletion_MarksCompletedAndStars()
        {
            _state.RecordLevelCompletion(G(1, 1), 3, false, 2.5f);
            Assert.IsTrue(_state.IsCompleted(G(1, 1)));
            Assert.AreEqual(3, _state.GetStars(G(1, 1)));
            Assert.AreEqual(2.5f, _state.GetBestCombo(G(1, 1)));
            Assert.AreEqual(1, _state.TotalCompletedLevels());
            Assert.AreEqual(3, _state.TotalStars());
        }

        [Test]
        public void RecordCompletion_UnlocksNextLevel()
        {
            Assert.IsFalse(_state.IsLevelUnlocked(G(1, 2)));
            _state.RecordLevelCompletion(G(1, 1), 1, false, 0f);
            Assert.IsTrue(_state.IsLevelUnlocked(G(1, 2)));
        }

        [Test]
        public void RecordCompletion_StarsNeverDowngrade()
        {
            _state.RecordLevelCompletion(G(1, 1), 3, false, 0f);
            _state.RecordLevelCompletion(G(1, 1), 1, false, 0f);
            Assert.AreEqual(3, _state.GetStars(G(1, 1)));
        }

        [Test]
        public void RecordCompletion_StarsCanImprove()
        {
            _state.RecordLevelCompletion(G(1, 1), 1, false, 0f);
            _state.RecordLevelCompletion(G(1, 1), 3, false, 0f);
            Assert.AreEqual(3, _state.GetStars(G(1, 1)));
        }

        [Test]
        public void RecordCompletion_BestComboNeverDowngrade()
        {
            _state.RecordLevelCompletion(G(1, 1), 3, false, 5f);
            _state.RecordLevelCompletion(G(1, 1), 3, false, 2f);
            Assert.AreEqual(5f, _state.GetBestCombo(G(1, 1)));
        }

        [Test]
        public void RecordCompletion_FiveStarFlagSticks()
        {
            _state.RecordLevelCompletion(G(1, 1), 3, true, 0f);
            Assert.IsTrue(_state.HasFiveStar(G(1, 1)));
            // A subsequent non-5-star clear should not clear the flag.
            _state.RecordLevelCompletion(G(1, 1), 3, false, 0f);
            Assert.IsTrue(_state.HasFiveStar(G(1, 1)));
        }

        [Test]
        public void RecordCompletion_StarsClampedToFive()
        {
            _state.RecordLevelCompletion(G(1, 1), 99, false, 0f);
            Assert.AreEqual(5, _state.GetStars(G(1, 1)));
        }

        [Test]
        public void RecordCompletion_NegativeStarsClampedToZero()
        {
            _state.RecordLevelCompletion(G(1, 1), -3, false, 0f);
            Assert.AreEqual(0, _state.GetStars(G(1, 1)));
            Assert.IsTrue(_state.IsCompleted(G(1, 1)));
        }

        [Test]
        public void RecordCompletion_InvalidIndexNoOp()
        {
            _state.RecordLevelCompletion(0,    3, false, 0f);
            _state.RecordLevelCompletion(101,  3, false, 0f);
            _state.RecordLevelCompletion(-5,   3, false, 0f);
            Assert.AreEqual(0, _state.TotalCompletedLevels());
        }

        // ==================================================================
        // Events
        // ==================================================================

        [Test]
        public void OnLevelCompleted_FiresWithArgs()
        {
            int gotIndex = -1, gotStars = -1;
            _state.OnLevelCompleted += (g, s) => { gotIndex = g; gotStars = s; };
            _state.RecordLevelCompletion(G(1, 1), 2, false, 0f);
            Assert.AreEqual(G(1, 1), gotIndex);
            Assert.AreEqual(2, gotStars);
        }

        [Test]
        public void OnStarsImproved_FiresOnlyWhenStarsIncrease()
        {
            int fires = 0;
            _state.OnStarsImproved += (g, s) => fires++;
            _state.RecordLevelCompletion(G(1, 1), 2, false, 0f); // 0 → 2 (fire)
            _state.RecordLevelCompletion(G(1, 1), 2, false, 0f); // 2 → 2 (no fire)
            _state.RecordLevelCompletion(G(1, 1), 1, false, 0f); // 1 ignored (no fire)
            _state.RecordLevelCompletion(G(1, 1), 3, false, 0f); // 2 → 3 (fire)
            Assert.AreEqual(2, fires);
        }

        [Test]
        public void OnStageUnlocked_FiresWhenPrevStageCleared()
        {
            int unlocked = -1;
            _state.OnStageUnlocked += s => unlocked = s;
            ClearStage(1);
            Assert.AreEqual(2, unlocked);
            Assert.IsTrue(_state.IsStageUnlocked(2));
        }

        [Test]
        public void OnStageUnlocked_FiresOnceOnly()
        {
            int count = 0;
            _state.OnStageUnlocked += _ => count++;
            ClearStage(1);
            // Re-clear level 5 — should not re-fire.
            _state.RecordLevelCompletion(G(1, 5), 3, false, 0f);
            Assert.AreEqual(1, count);
        }

        // ==================================================================
        // Unlock gating
        // ==================================================================

        [Test]
        public void StageUnlock_DefaultNeedsAllTen()
        {
            for (int i = 1; i <= 9; i++)
                _state.RecordLevelCompletion(G(1, i), 3, false, 0f);
            Assert.IsFalse(_state.IsStageUnlocked(2));
            _state.RecordLevelCompletion(G(1, 10), 3, false, 0f);
            Assert.IsTrue(_state.IsStageUnlocked(2));
        }

        [Test]
        public void StageUnlock_CustomRequirement()
        {
            _state.SetStageUnlockRequirement(2, 5);
            for (int i = 1; i <= 5; i++)
                _state.RecordLevelCompletion(G(1, i), 1, false, 0f);
            Assert.IsTrue(_state.IsStageUnlocked(2));
        }

        [Test]
        public void StageUnlock_Stage1CannotBeGated()
        {
            _state.SetStageUnlockRequirement(1, 5);
            Assert.IsTrue(_state.IsStageUnlocked(1));
        }

        [Test]
        public void LevelUnlock_LinearWithinStage()
        {
            _state.RecordLevelCompletion(G(1, 1), 1, false, 0f);
            Assert.IsTrue(_state.IsLevelUnlocked(G(1, 2)));
            Assert.IsFalse(_state.IsLevelUnlocked(G(1, 3)));
            _state.RecordLevelCompletion(G(1, 2), 1, false, 0f);
            Assert.IsTrue(_state.IsLevelUnlocked(G(1, 3)));
        }

        [Test]
        public void LevelUnlock_FirstLevelOfUnlockedStage()
        {
            ClearStage(1);
            Assert.IsTrue(_state.IsLevelUnlocked(G(2, 1)));
            Assert.IsFalse(_state.IsLevelUnlocked(G(2, 2)));
        }

        [Test]
        public void LevelUnlock_LockedStage_AllLevelsLocked()
        {
            for (int l = 1; l <= 10; l++)
                Assert.IsFalse(_state.IsLevelUnlocked(G(3, l)));
        }

        // ==================================================================
        // Aggregate
        // ==================================================================

        [Test]
        public void CompletedInStage_CountsCorrectly()
        {
            _state.RecordLevelCompletion(G(1, 1), 3, false, 0f);
            _state.RecordLevelCompletion(G(1, 2), 3, false, 0f);
            _state.RecordLevelCompletion(G(2, 1), 3, false, 0f); // won't count — stage 2 locked but state still records
            Assert.AreEqual(2, _state.CompletedInStage(1));
            Assert.AreEqual(1, _state.CompletedInStage(2));
        }

        [Test]
        public void TotalCompletedLevels_SumsAcrossStages()
        {
            ClearStage(1);
            ClearStage(2);
            Assert.AreEqual(20, _state.TotalCompletedLevels());
            Assert.AreEqual(60, _state.TotalStars());
        }

        [Test]
        public void TotalFiveStars_Counts()
        {
            _state.RecordLevelCompletion(G(1, 1), 3, true,  0f);
            _state.RecordLevelCompletion(G(1, 2), 3, true,  0f);
            _state.RecordLevelCompletion(G(1, 3), 3, false, 0f);
            Assert.AreEqual(2, _state.TotalFiveStars());
        }

        // ==================================================================
        // Snapshot save/load
        // ==================================================================

        [Test]
        public void Snapshot_RoundTripPreservesState()
        {
            _state.RecordLevelCompletion(G(1, 1), 3, true,  4.5f);
            _state.RecordLevelCompletion(G(1, 2), 2, false, 1.0f);

            var snap = _state.CreateSnapshot();

            var fresh = new ProgressionState();
            fresh.LoadFromSnapshot(snap);

            Assert.IsTrue (fresh.IsCompleted(G(1, 1)));
            Assert.IsTrue (fresh.IsCompleted(G(1, 2)));
            Assert.AreEqual(3, fresh.GetStars(G(1, 1)));
            Assert.AreEqual(2, fresh.GetStars(G(1, 2)));
            Assert.IsTrue (fresh.HasFiveStar(G(1, 1)));
            Assert.IsFalse(fresh.HasFiveStar(G(1, 2)));
            Assert.AreEqual(4.5f, fresh.GetBestCombo(G(1, 1)));
        }

        [Test]
        public void Snapshot_IsADetachedCopy()
        {
            _state.RecordLevelCompletion(G(1, 1), 3, false, 0f);
            var snap = _state.CreateSnapshot();
            _state.RecordLevelCompletion(G(1, 2), 3, false, 0f);
            // Snap should not reflect the later mutation.
            Assert.IsFalse(snap.Completed[G(1, 2)]);
        }

        [Test]
        public void Snapshot_NullIsNoOp()
        {
            _state.RecordLevelCompletion(G(1, 1), 3, false, 0f);
            _state.LoadFromSnapshot(null);
            Assert.IsTrue(_state.IsCompleted(G(1, 1)));
        }
    }
}
