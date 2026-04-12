using System.Collections.Generic;
using NUnit.Framework;
using Matchmancer.Lore;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class LoreLibraryTests
    {
        private LoreLibrary _lib;

        private static LoreEntry Make(string id, LoreCategory cat = LoreCategory.Character,
            int sort = 0, bool secret = false, string unlock = "")
        {
            return new LoreEntry
            {
                Id                = id,
                Title             = id + " Title",
                BodyText          = id + " body text.",
                Category          = cat,
                SortOrder         = sort,
                IsSecret          = secret,
                UnlockConditionId = unlock,
            };
        }

        [SetUp]
        public void SetUp()
        {
            _lib = new LoreLibrary();
        }

        // ==================================================================
        // Registration
        // ==================================================================

        [Test]
        public void Register_AddsEntry()
        {
            _lib.Register(Make("blue"));
            Assert.IsTrue(_lib.Has("blue"));
            Assert.AreEqual(1, _lib.TotalEntries);
        }

        [Test]
        public void Register_NullThrows()
        {
            Assert.Throws<System.ArgumentNullException>(() => _lib.Register(null));
        }

        [Test]
        public void Register_EmptyIdThrows()
        {
            var e = Make("x");
            e.Id = "";
            Assert.Throws<System.ArgumentException>(() => _lib.Register(e));
        }

        [Test]
        public void Register_FiresEvent()
        {
            LoreEntry seen = null;
            _lib.OnEntryRegistered += entry => seen = entry;
            var e = Make("blue");
            _lib.Register(e);
            Assert.AreSame(e, seen);
        }

        [Test]
        public void RegisterMany_AddsAll()
        {
            _lib.RegisterMany(new[] { Make("a"), Make("b"), Make("c") });
            Assert.AreEqual(3, _lib.TotalEntries);
        }

        // ==================================================================
        // Queries
        // ==================================================================

        [Test]
        public void Get_ReturnsEntry()
        {
            var e = Make("blue");
            _lib.Register(e);
            Assert.AreSame(e, _lib.Get("blue"));
        }

        [Test]
        public void Get_UnknownReturnsNull()
        {
            Assert.IsNull(_lib.Get("ghost"));
            Assert.IsNull(_lib.Get(null));
        }

        [Test]
        public void GetByCategory_SortsBySortOrder()
        {
            _lib.Register(Make("c", LoreCategory.Enemy, sort: 3));
            _lib.Register(Make("a", LoreCategory.Enemy, sort: 1));
            _lib.Register(Make("b", LoreCategory.Enemy, sort: 2));
            _lib.Register(Make("x", LoreCategory.World, sort: 0));

            var enemies = _lib.GetByCategory(LoreCategory.Enemy);
            Assert.AreEqual(3, enemies.Count);
            Assert.AreEqual("a", enemies[0].Id);
            Assert.AreEqual("b", enemies[1].Id);
            Assert.AreEqual("c", enemies[2].Id);
        }

        [Test]
        public void GetUnlockedByCategory_FiltersLocked()
        {
            _lib.Register(Make("a", LoreCategory.Enemy));
            _lib.Register(Make("b", LoreCategory.Enemy));
            _lib.Unlock("a");

            var unlocked = _lib.GetUnlockedByCategory(LoreCategory.Enemy);
            Assert.AreEqual(1, unlocked.Count);
            Assert.AreEqual("a", unlocked[0].Id);
        }

        // ==================================================================
        // Unlock
        // ==================================================================

        [Test]
        public void NewLibrary_AllLocked()
        {
            _lib.Register(Make("blue"));
            Assert.IsFalse(_lib.IsUnlocked("blue"));
            Assert.AreEqual(0, _lib.UnlockedCount);
        }

        [Test]
        public void Unlock_MarksAsUnlocked()
        {
            _lib.Register(Make("blue"));
            bool ok = _lib.Unlock("blue");
            Assert.IsTrue(ok);
            Assert.IsTrue(_lib.IsUnlocked("blue"));
            Assert.AreEqual(1, _lib.UnlockedCount);
        }

        [Test]
        public void Unlock_AlreadyUnlocked_ReturnsFalse()
        {
            _lib.Register(Make("blue"));
            _lib.Unlock("blue");
            Assert.IsFalse(_lib.Unlock("blue"));
        }

        [Test]
        public void Unlock_UnknownId_ReturnsFalse()
        {
            Assert.IsFalse(_lib.Unlock("ghost"));
        }

        [Test]
        public void Unlock_FiresEvent()
        {
            _lib.Register(Make("blue"));
            LoreEntry seen = null;
            _lib.OnEntryUnlocked += e => seen = e;
            _lib.Unlock("blue");
            Assert.AreEqual("blue", seen.Id);
        }

        [Test]
        public void UnlockAll_UnlocksEverything()
        {
            _lib.Register(Make("a"));
            _lib.Register(Make("b"));
            _lib.Register(Make("c"));
            _lib.UnlockAll();
            Assert.AreEqual(3, _lib.UnlockedCount);
        }

        [Test]
        public void UnlockAll_FiresEventPerEntry()
        {
            _lib.Register(Make("a"));
            _lib.Register(Make("b"));
            int fires = 0;
            _lib.OnEntryUnlocked += _ => fires++;
            _lib.UnlockAll();
            Assert.AreEqual(2, fires);
        }

        // ==================================================================
        // Snapshot
        // ==================================================================

        [Test]
        public void Snapshot_RoundTripsUnlocked()
        {
            _lib.Register(Make("a"));
            _lib.Register(Make("b"));
            _lib.Register(Make("c"));
            _lib.Unlock("a");
            _lib.Unlock("c");

            var snap = _lib.CreateSnapshot();

            var lib2 = new LoreLibrary();
            lib2.Register(Make("a"));
            lib2.Register(Make("b"));
            lib2.Register(Make("c"));
            lib2.LoadFromSnapshot(snap);

            Assert.IsTrue(lib2.IsUnlocked("a"));
            Assert.IsFalse(lib2.IsUnlocked("b"));
            Assert.IsTrue(lib2.IsUnlocked("c"));
        }

        [Test]
        public void Snapshot_DropsUnknownIds()
        {
            var snap = new LoreSnapshot { UnlockedIds = new[] { "a", "ghost" } };
            _lib.Register(Make("a"));
            _lib.LoadFromSnapshot(snap);
            Assert.IsTrue(_lib.IsUnlocked("a"));
            Assert.AreEqual(1, _lib.UnlockedCount);
        }

        [Test]
        public void Snapshot_NullIsNoOp()
        {
            _lib.Register(Make("a"));
            _lib.Unlock("a");
            _lib.LoadFromSnapshot(null);
            Assert.IsTrue(_lib.IsUnlocked("a"));
        }

        [Test]
        public void LoreEntry_Clone_IsIndependent()
        {
            var original = Make("x");
            var copy = original.Clone();
            copy.Title = "changed";
            Assert.AreEqual("x Title", original.Title);
        }
    }
}
