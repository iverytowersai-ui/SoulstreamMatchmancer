using NUnit.Framework;
using Matchmancer.Achievements;

namespace Matchmancer.Tests
{
    /// <summary>
    /// Pure C# tests for <see cref="TitleSystem"/> + the
    /// <see cref="AchievementTracker"/> wiring path.
    /// </summary>
    [TestFixture]
    public class TitleSystemTests
    {
        private TitleSystem _titles;

        [SetUp]
        public void SetUp()
        {
            _titles = new TitleSystem();
        }

        private static TitleDefinition Title(string id, string source = null)
        {
            return new TitleDefinition
            {
                Id                  = id,
                DisplayName         = id,
                SourceAchievementId = source ?? string.Empty,
            };
        }

        // ==================================================================
        // Registration
        // ==================================================================

        [Test]
        public void RegisterTitle_NullOrEmptyId_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(() => _titles.RegisterTitle(null));
            Assert.Throws<System.ArgumentException>(() =>
                _titles.RegisterTitle(new TitleDefinition { Id = string.Empty }));
        }

        [Test]
        public void HasDefinition_AfterRegister_True()
        {
            _titles.RegisterTitle(Title("blade"));
            Assert.IsTrue(_titles.HasDefinition("blade"));
            Assert.IsFalse(_titles.HasDefinition("ghost"));
        }

        // ==================================================================
        // Unlock
        // ==================================================================

        [Test]
        public void UnlockTitle_FirstTime_FiresEvent()
        {
            _titles.RegisterTitle(Title("blade"));

            int calls = 0;
            TitleDefinition unlocked = null;
            _titles.OnTitleUnlocked += d => { calls++; unlocked = d; };

            bool ok = _titles.UnlockTitle("blade");

            Assert.IsTrue(ok);
            Assert.AreEqual(1, calls);
            Assert.AreEqual("blade", unlocked.Id);
            Assert.IsTrue(_titles.HasTitle("blade"));
        }

        [Test]
        public void UnlockTitle_Twice_NoOpSecondTime()
        {
            _titles.RegisterTitle(Title("blade"));
            _titles.UnlockTitle("blade");

            int calls = 0;
            _titles.OnTitleUnlocked += _ => calls++;
            bool ok = _titles.UnlockTitle("blade");

            Assert.IsFalse(ok);
            Assert.AreEqual(0, calls);
        }

        [Test]
        public void UnlockTitle_Unknown_ReturnsFalse()
        {
            Assert.IsFalse(_titles.UnlockTitle("ghost"));
            Assert.IsFalse(_titles.UnlockTitle(null));
        }

        // ==================================================================
        // Equip
        // ==================================================================

        [Test]
        public void Equip_NotOwned_Throws()
        {
            _titles.RegisterTitle(Title("blade"));
            Assert.Throws<System.InvalidOperationException>(() => _titles.Equip("blade"));
        }

        [Test]
        public void Equip_Owned_FiresEquipped()
        {
            _titles.RegisterTitle(Title("blade"));
            _titles.UnlockTitle("blade");

            TitleDefinition equipped = null;
            _titles.OnTitleEquipped += d => equipped = d;

            bool changed = _titles.Equip("blade");

            Assert.IsTrue(changed);
            Assert.AreEqual("blade", equipped.Id);
            Assert.AreEqual("blade", _titles.EquippedId);
        }

        [Test]
        public void Equip_SameTitleTwice_NoOp()
        {
            _titles.RegisterTitle(Title("blade"));
            _titles.UnlockTitle("blade");
            _titles.Equip("blade");

            int calls = 0;
            _titles.OnTitleEquipped += _ => calls++;
            bool changed = _titles.Equip("blade");

            Assert.IsFalse(changed);
            Assert.AreEqual(0, calls);
        }

        [Test]
        public void Equip_DifferentTitle_FiresUnequipThenEquip()
        {
            _titles.RegisterTitle(Title("a"));
            _titles.RegisterTitle(Title("b"));
            _titles.UnlockTitle("a");
            _titles.UnlockTitle("b");
            _titles.Equip("a");

            string lastUnequipped = null;
            string lastEquipped   = null;
            _titles.OnTitleUnequipped += d => lastUnequipped = d.Id;
            _titles.OnTitleEquipped   += d => lastEquipped   = d.Id;

            _titles.Equip("b");

            Assert.AreEqual("a", lastUnequipped);
            Assert.AreEqual("b", lastEquipped);
            Assert.AreEqual("b", _titles.EquippedId);
        }

        [Test]
        public void Equip_NullOrEmpty_Unequips()
        {
            _titles.RegisterTitle(Title("a"));
            _titles.UnlockTitle("a");
            _titles.Equip("a");

            string unequippedId = null;
            _titles.OnTitleUnequipped += d => unequippedId = d.Id;

            bool changed = _titles.Equip(null);

            Assert.IsTrue(changed);
            Assert.AreEqual("a", unequippedId);
            Assert.IsNull(_titles.EquippedId);
        }

        [Test]
        public void Unequip_WhenNothingEquipped_NoOp()
        {
            int calls = 0;
            _titles.OnTitleUnequipped += _ => calls++;
            _titles.Unequip();
            Assert.AreEqual(0, calls);
        }

        // ==================================================================
        // Achievement wiring
        // ==================================================================

        [Test]
        public void WireToTracker_AchievementUnlock_GrantsTitle()
        {
            var tracker = new AchievementTracker();
            _titles.RegisterTitle(Title("first_kill_title"));
            _titles.WireToTracker(tracker);

            tracker.RegisterAchievement(new AchievementDefinition
            {
                Id            = "first_kill",
                StatKey       = AchievementStatKey.EnemiesDefeated,
                TargetValue   = 1,
                TitleIdReward = "first_kill_title",
            });

            tracker.IncrementStat(AchievementStatKey.EnemiesDefeated);

            Assert.IsTrue(_titles.HasTitle("first_kill_title"));
        }

        [Test]
        public void WireToTracker_AchievementWithoutReward_NoTitleGranted()
        {
            var tracker = new AchievementTracker();
            _titles.RegisterTitle(Title("blade"));
            _titles.WireToTracker(tracker);

            tracker.RegisterAchievement(new AchievementDefinition
            {
                Id          = "no_reward",
                StatKey     = AchievementStatKey.LevelsCompleted,
                TargetValue = 1,
            });

            tracker.IncrementStat(AchievementStatKey.LevelsCompleted);

            Assert.IsFalse(_titles.HasTitle("blade"));
        }

        [Test]
        public void WireToTracker_RewardingUnregisteredTitle_GracefullySkipped()
        {
            var tracker = new AchievementTracker();
            _titles.WireToTracker(tracker);

            tracker.RegisterAchievement(new AchievementDefinition
            {
                Id            = "x",
                StatKey       = AchievementStatKey.LevelsCompleted,
                TargetValue   = 1,
                TitleIdReward = "missing_title",
            });

            // Should not throw — unknown title is silently ignored.
            tracker.IncrementStat(AchievementStatKey.LevelsCompleted);
            Assert.IsFalse(_titles.HasTitle("missing_title"));
        }

        [Test]
        public void WireToTracker_Twice_OnlyLatestTrackerIsListened()
        {
            var trackerA = new AchievementTracker();
            var trackerB = new AchievementTracker();

            _titles.RegisterTitle(Title("title_a"));
            _titles.RegisterTitle(Title("title_b"));

            _titles.WireToTracker(trackerA);
            _titles.WireToTracker(trackerB);

            trackerA.RegisterAchievement(new AchievementDefinition
            {
                Id = "a", StatKey = AchievementStatKey.LevelsCompleted, TargetValue = 1, TitleIdReward = "title_a",
            });
            trackerB.RegisterAchievement(new AchievementDefinition
            {
                Id = "b", StatKey = AchievementStatKey.LevelsCompleted, TargetValue = 1, TitleIdReward = "title_b",
            });

            trackerA.IncrementStat(AchievementStatKey.LevelsCompleted);
            trackerB.IncrementStat(AchievementStatKey.LevelsCompleted);

            Assert.IsFalse(_titles.HasTitle("title_a"),
                "TitleSystem should have unsubscribed from trackerA when wired to trackerB.");
            Assert.IsTrue(_titles.HasTitle("title_b"));
        }

        // ==================================================================
        // Snapshot / restore
        // ==================================================================

        [Test]
        public void Snapshot_RoundTrip_PreservesOwnedAndEquipped()
        {
            _titles.RegisterTitle(Title("a"));
            _titles.RegisterTitle(Title("b"));
            _titles.UnlockTitle("a");
            _titles.UnlockTitle("b");
            _titles.Equip("b");

            var snap = _titles.CreateSnapshot();

            var restored = new TitleSystem();
            restored.RegisterTitle(Title("a"));
            restored.RegisterTitle(Title("b"));
            restored.LoadFromSnapshot(snap);

            Assert.IsTrue(restored.HasTitle("a"));
            Assert.IsTrue(restored.HasTitle("b"));
            Assert.AreEqual("b", restored.EquippedId);
        }

        [Test]
        public void LoadSnapshot_EquippedNotOwned_DropsEquipped()
        {
            var snap = new TitleSystem.Snapshot
            {
                OwnedIds   = new[] { "a" },
                EquippedId = "ghost",
            };

            _titles.RegisterTitle(Title("a"));
            _titles.LoadFromSnapshot(snap);

            Assert.IsTrue(_titles.HasTitle("a"));
            Assert.IsNull(_titles.EquippedId);
        }
    }
}
