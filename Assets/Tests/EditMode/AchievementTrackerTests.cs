using NUnit.Framework;
using Matchmancer.Achievements;

namespace Matchmancer.Tests
{
    /// <summary>
    /// Pure C# tests for <see cref="AchievementTracker"/>. No Unity, no SO.
    /// </summary>
    [TestFixture]
    public class AchievementTrackerTests
    {
        private AchievementTracker _tracker;

        [SetUp]
        public void SetUp()
        {
            _tracker = new AchievementTracker();
        }

        private static AchievementDefinition Def(string id, AchievementStatKey key, long target, string titleReward = null)
        {
            return new AchievementDefinition
            {
                Id            = id,
                DisplayName   = id,
                StatKey       = key,
                TargetValue   = target,
                TitleIdReward = titleReward ?? string.Empty,
            };
        }

        // ==================================================================
        // Stat counters
        // ==================================================================

        [Test]
        public void NewTracker_AllStatsZero()
        {
            Assert.AreEqual(0L, _tracker.GetStat(AchievementStatKey.LevelsCompleted));
            Assert.AreEqual(0L, _tracker.GetStat(AchievementStatKey.TotalDamageDealt));
            Assert.AreEqual(0, _tracker.UnlockedCount);
        }

        [Test]
        public void IncrementStat_Adds()
        {
            _tracker.IncrementStat(AchievementStatKey.EnemiesDefeated, 3);
            _tracker.IncrementStat(AchievementStatKey.EnemiesDefeated);   // default 1
            Assert.AreEqual(4L, _tracker.GetStat(AchievementStatKey.EnemiesDefeated));
        }

        [Test]
        public void IncrementStat_FiresOnStatChanged()
        {
            int calls = 0;
            long lastValue = 0;
            _tracker.OnStatChanged += (k, v) => { calls++; lastValue = v; };

            _tracker.IncrementStat(AchievementStatKey.LevelsCompleted, 5);

            Assert.AreEqual(1, calls);
            Assert.AreEqual(5L, lastValue);
        }

        [Test]
        public void IncrementStat_NegativeOrZero_Ignored()
        {
            _tracker.IncrementStat(AchievementStatKey.EnemiesDefeated, 0);
            _tracker.IncrementStat(AchievementStatKey.EnemiesDefeated, -10);
            Assert.AreEqual(0L, _tracker.GetStat(AchievementStatKey.EnemiesDefeated));
        }

        [Test]
        public void UpdateMaxStat_OnlyRaises()
        {
            _tracker.UpdateMaxStat(AchievementStatKey.MaxComboEver, 5);
            _tracker.UpdateMaxStat(AchievementStatKey.MaxComboEver, 3); // ignored
            _tracker.UpdateMaxStat(AchievementStatKey.MaxComboEver, 8);
            Assert.AreEqual(8L, _tracker.GetStat(AchievementStatKey.MaxComboEver));
        }

        [Test]
        public void UpdateMaxStat_NoEventOnIgnore()
        {
            _tracker.UpdateMaxStat(AchievementStatKey.MaxComboEver, 5);

            int calls = 0;
            _tracker.OnStatChanged += (_, _2) => calls++;
            _tracker.UpdateMaxStat(AchievementStatKey.MaxComboEver, 3);

            Assert.AreEqual(0, calls);
        }

        // ==================================================================
        // Achievement registration + unlock
        // ==================================================================

        [Test]
        public void RegisterAchievement_NullOrEmptyId_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(() => _tracker.RegisterAchievement(null));
            Assert.Throws<System.ArgumentException>(() =>
                _tracker.RegisterAchievement(new AchievementDefinition { Id = string.Empty }));
        }

        [Test]
        public void IncrementCrossingThreshold_FiresUnlockOnce()
        {
            _tracker.RegisterAchievement(Def("first_blood", AchievementStatKey.EnemiesDefeated, 1));

            int calls = 0;
            AchievementDefinition unlocked = null;
            _tracker.OnAchievementUnlocked += d => { calls++; unlocked = d; };

            _tracker.IncrementStat(AchievementStatKey.EnemiesDefeated);
            _tracker.IncrementStat(AchievementStatKey.EnemiesDefeated); // shouldn't refire

            Assert.AreEqual(1, calls);
            Assert.AreEqual("first_blood", unlocked.Id);
            Assert.IsTrue(_tracker.IsUnlocked("first_blood"));
            Assert.AreEqual(1, _tracker.UnlockedCount);
        }

        [Test]
        public void RegisterAchievement_AfterStatAlreadyMetThreshold_UnlocksImmediately()
        {
            _tracker.IncrementStat(AchievementStatKey.LevelsCompleted, 10);

            int calls = 0;
            _tracker.OnAchievementUnlocked += _ => calls++;

            _tracker.RegisterAchievement(Def("ten_levels", AchievementStatKey.LevelsCompleted, 10));

            Assert.AreEqual(1, calls);
            Assert.IsTrue(_tracker.IsUnlocked("ten_levels"));
        }

        [Test]
        public void MultipleAchievementsOnSameKey_AllUnlockAtCorrectThresholds()
        {
            _tracker.RegisterAchievement(Def("kills_5",  AchievementStatKey.EnemiesDefeated, 5));
            _tracker.RegisterAchievement(Def("kills_10", AchievementStatKey.EnemiesDefeated, 10));
            _tracker.RegisterAchievement(Def("kills_50", AchievementStatKey.EnemiesDefeated, 50));

            _tracker.IncrementStat(AchievementStatKey.EnemiesDefeated, 5);
            Assert.IsTrue(_tracker.IsUnlocked("kills_5"));
            Assert.IsFalse(_tracker.IsUnlocked("kills_10"));

            _tracker.IncrementStat(AchievementStatKey.EnemiesDefeated, 5);
            Assert.IsTrue(_tracker.IsUnlocked("kills_10"));
            Assert.IsFalse(_tracker.IsUnlocked("kills_50"));

            _tracker.IncrementStat(AchievementStatKey.EnemiesDefeated, 40);
            Assert.IsTrue(_tracker.IsUnlocked("kills_50"));
            Assert.AreEqual(3, _tracker.UnlockedCount);
        }

        [Test]
        public void OneIncrement_CanCrossMultipleThresholdsAtOnce()
        {
            _tracker.RegisterAchievement(Def("a", AchievementStatKey.LevelsCompleted, 1));
            _tracker.RegisterAchievement(Def("b", AchievementStatKey.LevelsCompleted, 5));
            _tracker.RegisterAchievement(Def("c", AchievementStatKey.LevelsCompleted, 10));

            int calls = 0;
            _tracker.OnAchievementUnlocked += _ => calls++;

            _tracker.IncrementStat(AchievementStatKey.LevelsCompleted, 100);

            Assert.AreEqual(3, calls);
            Assert.AreEqual(3, _tracker.UnlockedCount);
        }

        [Test]
        public void DefinitionsForOtherKeys_NotChecked()
        {
            _tracker.RegisterAchievement(Def("dmg",  AchievementStatKey.TotalDamageDealt, 100));
            _tracker.RegisterAchievement(Def("kill", AchievementStatKey.EnemiesDefeated,  1));

            _tracker.IncrementStat(AchievementStatKey.TotalDamageDealt, 200);

            Assert.IsTrue(_tracker.IsUnlocked("dmg"));
            Assert.IsFalse(_tracker.IsUnlocked("kill"));
        }

        // ==================================================================
        // Reevaluate
        // ==================================================================

        [Test]
        public void ReevaluateAll_UnlocksMissedAchievements()
        {
            _tracker.RegisterAchievement(Def("a", AchievementStatKey.LevelsCompleted, 5));
            _tracker.SetStatRaw(AchievementStatKey.LevelsCompleted, 10);
            // No event fired by SetStatRaw, so the achievement is still locked.
            Assert.IsFalse(_tracker.IsUnlocked("a"));

            _tracker.ReevaluateAll();
            Assert.IsTrue(_tracker.IsUnlocked("a"));
        }

        // ==================================================================
        // Snapshot / restore
        // ==================================================================

        [Test]
        public void Snapshot_RoundTrip_PreservesStatsAndUnlocks()
        {
            _tracker.RegisterAchievement(Def("a", AchievementStatKey.LevelsCompleted, 1));
            _tracker.IncrementStat(AchievementStatKey.LevelsCompleted, 3);
            _tracker.IncrementStat(AchievementStatKey.TotalDamageDealt, 250);

            var snap = _tracker.CreateSnapshot();

            var restored = new AchievementTracker();
            restored.RegisterAchievement(Def("a", AchievementStatKey.LevelsCompleted, 1));
            restored.LoadFromSnapshot(snap);

            Assert.AreEqual(3L,   restored.GetStat(AchievementStatKey.LevelsCompleted));
            Assert.AreEqual(250L, restored.GetStat(AchievementStatKey.TotalDamageDealt));
            Assert.IsTrue(restored.IsUnlocked("a"));
        }

        [Test]
        public void Snapshot_NullLoad_NoOp()
        {
            _tracker.IncrementStat(AchievementStatKey.LevelsCompleted, 5);
            _tracker.LoadFromSnapshot(null);
            Assert.AreEqual(5L, _tracker.GetStat(AchievementStatKey.LevelsCompleted));
        }
    }
}
