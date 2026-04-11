using NUnit.Framework;
using Matchmancer.Achievements;
using Matchmancer.Boosters;
using Matchmancer.Progression;
using Matchmancer.Save;

namespace Matchmancer.Tests
{
    /// <summary>
    /// End-to-end save flows that wire <see cref="SaveService"/> to all
    /// snapshot-capable systems at once and verify a full round-trip
    /// (Progression + Achievements + Titles + Boosters) survives a fresh
    /// service instance backed by the same in-memory storage. Gear is
    /// covered separately in its own controller test (needs a tuning
    /// resolver wired through GearInventoryController).
    /// </summary>
    public class SaveSystemIntegrationTests
    {
        private InMemorySaveStorage _storage;

        [SetUp]
        public void SetUp()
        {
            _storage = new InMemorySaveStorage();
        }

        private static BoosterInventory MakeBoosterInventory()
        {
            var inv = new BoosterInventory();
            inv.RegisterDefinition(new BoosterDefinition("hammer", BoosterType.Hammer)
                { UseContext = BoosterUseContext.InBattle, MaxStack = 20, ShopPrice = 50 });
            inv.RegisterDefinition(new BoosterDefinition("shuffle", BoosterType.Shuffle)
                { UseContext = BoosterUseContext.InBattle, MaxStack = 10, ShopPrice = 75 });
            inv.RegisterDefinition(new BoosterDefinition("heal", BoosterType.Heal)
                { UseContext = BoosterUseContext.Anywhere, MaxStack = 10, ShopPrice = 60 });
            return inv;
        }

        private static AchievementTracker MakeTracker()
        {
            var t = new AchievementTracker();
            t.RegisterAchievement(new AchievementDefinition
            {
                Id = "ach.first_win",
                StatKey = AchievementStatKey.LevelsCompleted,
                TargetValue = 1,
                TitleIdReward = "t.starter",
            });
            t.RegisterAchievement(new AchievementDefinition
            {
                Id = "ach.veteran",
                StatKey = AchievementStatKey.LevelsCompleted,
                TargetValue = 10,
            });
            return t;
        }

        private static TitleSystem MakeTitles()
        {
            var ts = new TitleSystem();
            ts.RegisterTitle(new TitleDefinition { Id = "t.starter" });
            ts.RegisterTitle(new TitleDefinition { Id = "t.veteran" });
            return ts;
        }

        [Test]
        public void FullRoundTrip_AllSubsystemsRestore()
        {
            // ----- Build live state -----
            var prog     = new ProgressionState();
            var tracker  = MakeTracker();
            var titles   = MakeTitles();
            var boosters = MakeBoosterInventory();

            // Progression: complete two levels
            prog.RecordLevelCompletion(1, 3, true,  5.0f);
            prog.RecordLevelCompletion(2, 2, false, 2.5f);

            // Achievements: bump LevelsCompleted to 1 → unlocks ach.first_win
            tracker.IncrementStat(AchievementStatKey.LevelsCompleted, 1);
            tracker.IncrementStat(AchievementStatKey.TotalDamageDealt, 999);
            Assert.IsTrue(tracker.IsUnlocked("ach.first_win"));

            // Titles: manual unlock + equip
            titles.UnlockTitle("t.veteran");
            titles.Equip("t.veteran");

            // Boosters
            boosters.Purchase("hammer", 5);
            boosters.Purchase("heal", 2);
            boosters.Use("hammer", BoosterUseContext.InBattle);

            // ----- Save -----
            var svc = new SaveService(_storage, "main")
            {
                Progression  = prog,
                Achievements = tracker,
                Titles       = titles,
                Boosters     = boosters,
            };
            svc.Save();

            // ----- New session: fresh subsystems, same storage -----
            var prog2     = new ProgressionState();
            var tracker2  = MakeTracker();
            var titles2   = MakeTitles();
            var boosters2 = MakeBoosterInventory();

            var svc2 = new SaveService(_storage, "main")
            {
                Progression  = prog2,
                Achievements = tracker2,
                Titles       = titles2,
                Boosters     = boosters2,
            };
            var loaded = svc2.Load();
            Assert.IsNotNull(loaded);

            // Progression
            Assert.IsTrue(prog2.IsCompleted(1));
            Assert.AreEqual(3, prog2.GetStars(1));
            Assert.IsTrue(prog2.HasFiveStar(1));
            Assert.AreEqual(2, prog2.TotalCompletedLevels());

            // Achievements
            Assert.AreEqual(1,   tracker2.GetStat(AchievementStatKey.LevelsCompleted));
            Assert.AreEqual(999, tracker2.GetStat(AchievementStatKey.TotalDamageDealt));
            Assert.IsTrue(tracker2.IsUnlocked("ach.first_win"));

            // Titles
            Assert.IsTrue(titles2.HasTitle("t.veteran"));
            Assert.AreEqual("t.veteran", titles2.EquippedId);

            // Boosters
            Assert.AreEqual(4, boosters2.GetCount("hammer"));
            Assert.AreEqual(2, boosters2.GetCount("heal"));
            Assert.AreEqual(1, boosters2.LifetimeUsed);
            Assert.AreEqual(7, boosters2.LifetimePurchased);
        }

        [Test]
        public void NewProfile_StartsEmpty_DoesNotSeeOtherProfile()
        {
            var inv = MakeBoosterInventory();
            inv.Purchase("hammer", 3);

            var svcA = new SaveService(_storage, "A") { Boosters = inv };
            svcA.Save();

            var freshInv = MakeBoosterInventory();
            var svcB = new SaveService(_storage, "B") { Boosters = freshInv };
            Assert.IsFalse(svcB.HasSave);
            Assert.IsNull(svcB.Load());
            Assert.AreEqual(0, freshInv.GetCount("hammer"));
        }

        [Test]
        public void DeleteSave_RemovesPersistence_LiveStateUntouched()
        {
            var inv = MakeBoosterInventory();
            inv.Purchase("hammer", 3);

            var svc = new SaveService(_storage, "main") { Boosters = inv };
            svc.Save();
            Assert.IsTrue(svc.HasSave);

            svc.Delete();
            Assert.IsFalse(svc.HasSave);
            // Live inventory unchanged — Delete only wipes the storage record.
            Assert.AreEqual(3, inv.GetCount("hammer"));
        }

        [Test]
        public void TwoSavesInARow_SecondOverwritesFirst()
        {
            var inv = MakeBoosterInventory();
            var svc = new SaveService(_storage, "main") { Boosters = inv };

            inv.Purchase("hammer", 1);
            svc.Save();

            inv.Purchase("hammer", 4);
            svc.Save();

            // Reload into fresh inventory
            var fresh = MakeBoosterInventory();
            svc.Boosters = fresh;
            svc.Load();
            Assert.AreEqual(5, fresh.GetCount("hammer"));
        }

        [Test]
        public void PartialSave_OnlyAttachedSubsystemsRestore()
        {
            // Save with ONLY boosters attached
            var boosters = MakeBoosterInventory();
            boosters.Purchase("shuffle", 4);
            var saveSvc = new SaveService(_storage, "main") { Boosters = boosters };
            saveSvc.Save();

            // Load with progression also attached — its snapshot is null,
            // so it must be left untouched.
            var prog2     = new ProgressionState();
            prog2.RecordLevelCompletion(5, 2, false, 1.0f); // pre-existing live data
            var boosters2 = MakeBoosterInventory();

            var loadSvc = new SaveService(_storage, "main")
            {
                Progression = prog2,
                Boosters    = boosters2,
            };
            loadSvc.Load();

            Assert.AreEqual(4, boosters2.GetCount("shuffle"));
            // Progression untouched because save had no Progression snapshot.
            Assert.IsTrue(prog2.IsCompleted(5));
        }

        [Test]
        public void SaveData_VersionFieldMatchesCurrent()
        {
            var svc = new SaveService(_storage, "main");
            svc.Save();
            var data = svc.Load();
            Assert.AreEqual(SaveData.CurrentVersion, data.Version);
        }
    }
}
