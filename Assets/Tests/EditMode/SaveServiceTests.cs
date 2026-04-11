using NUnit.Framework;
using Matchmancer.Achievements;
using Matchmancer.Boosters;
using Matchmancer.Progression;
using Matchmancer.Save;

namespace Matchmancer.Tests
{
    /// <summary>
    /// Unit tests for <see cref="SaveService"/> using the in-memory storage
    /// backend. Verifies snapshot building, profile handling, callbacks, and
    /// graceful behavior when subsystems are missing.
    /// </summary>
    public class SaveServiceTests
    {
        private InMemorySaveStorage _storage;
        private SaveService         _svc;

        [SetUp]
        public void SetUp()
        {
            _storage = new InMemorySaveStorage();
            _svc     = new SaveService(_storage, "main");
        }

        [Test]
        public void Ctor_NullStorageThrows()
        {
            Assert.Throws<System.ArgumentNullException>(
                () => new SaveService(null, "main"));
        }

        [Test]
        public void Ctor_DefaultsToMainProfile()
        {
            var svc = new SaveService(new InMemorySaveStorage());
            Assert.AreEqual("main", svc.ProfileId);
        }

        [Test]
        public void HasSave_FalseInitially()
        {
            Assert.IsFalse(_svc.HasSave);
        }

        [Test]
        public void Save_WritesToBackendAndIncrementsTelemetry()
        {
            _svc.Save();
            Assert.IsTrue(_svc.HasSave);
            Assert.AreEqual(1, _svc.SaveCount);
            Assert.AreEqual(1, _storage.WriteCount);
            Assert.IsTrue(_svc.LastSavedAtUnix > 0);
        }

        [Test]
        public void Save_FiresBeforeAndAfterCallbacks()
        {
            int before = 0, after = 0;
            _svc.OnBeforeSave += d => before++;
            _svc.OnAfterSave  += d => after++;
            _svc.Save();
            Assert.AreEqual(1, before);
            Assert.AreEqual(1, after);
        }

        [Test]
        public void Load_ReturnsNullWhenNothingSaved()
        {
            Assert.IsNull(_svc.Load());
            Assert.AreEqual(0, _svc.LoadCount);
        }

        [Test]
        public void Load_FiresAfterLoadCallback()
        {
            _svc.Save();
            int loaded = 0;
            _svc.OnAfterLoad += d => loaded++;
            var data = _svc.Load();
            Assert.IsNotNull(data);
            Assert.AreEqual(1, loaded);
            Assert.AreEqual(1, _svc.LoadCount);
        }

        [Test]
        public void Delete_RemovesSave()
        {
            _svc.Save();
            Assert.IsTrue(_svc.HasSave);
            _svc.Delete();
            Assert.IsFalse(_svc.HasSave);
        }

        [Test]
        public void SetProfile_SwitchesActiveProfile()
        {
            _svc.SetProfile("alt");
            Assert.AreEqual("alt", _svc.ProfileId);
            _svc.Save();
            Assert.IsTrue(_storage.Exists("alt"));
            Assert.IsFalse(_storage.Exists("main"));
        }

        [Test]
        public void SetProfile_NullOrEmptyIgnored()
        {
            _svc.SetProfile(null);
            Assert.AreEqual("main", _svc.ProfileId);
            _svc.SetProfile("");
            Assert.AreEqual("main", _svc.ProfileId);
        }

        [Test]
        public void BuildSaveData_AllNullSubsystems_ProducesEmptyShell()
        {
            var data = _svc.BuildSaveData();
            Assert.IsNotNull(data);
            Assert.AreEqual("main", data.ProfileId);
            Assert.AreEqual(SaveData.CurrentVersion, data.Version);
            Assert.IsTrue(data.IsEmpty);
        }

        [Test]
        public void BuildSaveData_OnlyBoosters_PopulatesBoosterSnapshotOnly()
        {
            var inv = new BoosterInventory();
            inv.RegisterDefinition(new BoosterDefinition("hammer", BoosterType.Hammer));
            inv.Purchase("hammer", 3);
            _svc.Boosters = inv;

            var data = _svc.BuildSaveData();
            Assert.IsNotNull(data.Boosters);
            Assert.IsNull(data.Progression);
            Assert.IsNull(data.Achievements);
            Assert.IsNull(data.Titles);
            Assert.IsNull(data.Gear);
            Assert.AreEqual(1, data.Boosters.BoosterIds.Length);
        }

        [Test]
        public void ApplySaveData_NullDataIsNoOp()
        {
            var inv = new BoosterInventory();
            inv.RegisterDefinition(new BoosterDefinition("h", BoosterType.Hammer));
            inv.Purchase("h", 5);
            _svc.Boosters = inv;
            _svc.ApplySaveData(null);
            Assert.AreEqual(5, inv.GetCount("h"));
        }

        [Test]
        public void Boosters_RoundTrip_PreservesCountsAndLifetime()
        {
            var inv = new BoosterInventory();
            inv.RegisterDefinition(new BoosterDefinition("hammer", BoosterType.Hammer)
                { UseContext = BoosterUseContext.InBattle });
            inv.RegisterDefinition(new BoosterDefinition("heal",  BoosterType.Heal)
                { UseContext = BoosterUseContext.Anywhere });

            inv.Purchase("hammer", 4);
            inv.Purchase("heal", 2);
            inv.Use("hammer", BoosterUseContext.InBattle);

            _svc.Boosters = inv;
            _svc.Save();

            // Replace inventory with a fresh one and reload.
            var fresh = new BoosterInventory();
            fresh.RegisterDefinition(new BoosterDefinition("hammer", BoosterType.Hammer)
                { UseContext = BoosterUseContext.InBattle });
            fresh.RegisterDefinition(new BoosterDefinition("heal",  BoosterType.Heal)
                { UseContext = BoosterUseContext.Anywhere });
            _svc.Boosters = fresh;

            var loaded = _svc.Load();
            Assert.IsNotNull(loaded);
            Assert.AreEqual(3, fresh.GetCount("hammer"));
            Assert.AreEqual(2, fresh.GetCount("heal"));
            Assert.AreEqual(1, fresh.LifetimeUsed);
            Assert.AreEqual(6, fresh.LifetimePurchased);
        }

        [Test]
        public void Achievements_RoundTrip_PreservesStatsAndUnlocks()
        {
            var tracker = new AchievementTracker();
            tracker.RegisterAchievement(new AchievementDefinition
            {
                Id = "ach.first_win",
                StatKey = AchievementStatKey.LevelsCompleted,
                TargetValue = 1,
            });
            tracker.IncrementStat(AchievementStatKey.LevelsCompleted, 1);
            tracker.IncrementStat(AchievementStatKey.TotalDamageDealt, 1500);
            Assert.IsTrue(tracker.IsUnlocked("ach.first_win"));

            _svc.Achievements = tracker;
            _svc.Save();

            var fresh = new AchievementTracker();
            fresh.RegisterAchievement(new AchievementDefinition
            {
                Id = "ach.first_win",
                StatKey = AchievementStatKey.LevelsCompleted,
                TargetValue = 1,
            });
            _svc.Achievements = fresh;
            _svc.Load();

            Assert.AreEqual(1,    fresh.GetStat(AchievementStatKey.LevelsCompleted));
            Assert.AreEqual(1500, fresh.GetStat(AchievementStatKey.TotalDamageDealt));
            Assert.IsTrue(fresh.IsUnlocked("ach.first_win"));
        }

        [Test]
        public void Titles_RoundTrip_PreservesOwnedAndEquipped()
        {
            var titles = new TitleSystem();
            titles.RegisterTitle(new TitleDefinition { Id = "t.hero" });
            titles.RegisterTitle(new TitleDefinition { Id = "t.legend" });
            titles.UnlockTitle("t.hero");
            titles.UnlockTitle("t.legend");
            titles.Equip("t.legend");

            _svc.Titles = titles;
            _svc.Save();

            var fresh = new TitleSystem();
            fresh.RegisterTitle(new TitleDefinition { Id = "t.hero" });
            fresh.RegisterTitle(new TitleDefinition { Id = "t.legend" });
            _svc.Titles = fresh;
            _svc.Load();

            Assert.IsTrue(fresh.HasTitle("t.hero"));
            Assert.IsTrue(fresh.HasTitle("t.legend"));
            Assert.AreEqual("t.legend", fresh.EquippedId);
        }

        [Test]
        public void Progression_RoundTrip_PreservesCompletionAndStars()
        {
            var prog = new ProgressionState();
            prog.RecordLevelCompletion(1, 3, true,  4.5f);
            prog.RecordLevelCompletion(2, 2, false, 2.0f);

            _svc.Progression = prog;
            _svc.Save();

            var fresh = new ProgressionState();
            _svc.Progression = fresh;
            _svc.Load();

            Assert.IsTrue(fresh.IsCompleted(1));
            Assert.IsTrue(fresh.IsCompleted(2));
            Assert.AreEqual(3, fresh.GetStars(1));
            Assert.AreEqual(2, fresh.GetStars(2));
            Assert.IsTrue(fresh.HasFiveStar(1));
            Assert.IsFalse(fresh.HasFiveStar(2));
            Assert.AreEqual(4.5f, fresh.GetBestCombo(1), 0.001f);
        }

        [Test]
        public void Save_LastSavedAtUnix_TracksMostRecent()
        {
            _svc.Save();
            long first = _svc.LastSavedAtUnix;
            // Don't sleep — just confirm second save updates the field
            // (BuildSaveData calls UtcNow each time).
            _svc.Save();
            Assert.IsTrue(_svc.LastSavedAtUnix >= first);
            Assert.AreEqual(2, _svc.SaveCount);
        }
    }
}
