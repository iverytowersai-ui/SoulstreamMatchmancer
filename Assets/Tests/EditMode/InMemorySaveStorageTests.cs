using NUnit.Framework;
using Matchmancer.Save;

namespace Matchmancer.Tests
{
    /// <summary>
    /// Storage-contract tests for <see cref="InMemorySaveStorage"/>. Round-trip
    /// is exercised through JsonUtility (the in-memory backend stores serialized
    /// JSON, not live object references) so these tests double as a smoke test
    /// for SaveData's serializability.
    /// </summary>
    public class InMemorySaveStorageTests
    {
        [Test]
        public void Exists_FalseWhenNoSave()
        {
            var s = new InMemorySaveStorage();
            Assert.IsFalse(s.Exists("main"));
            Assert.IsFalse(s.Exists(null));
            Assert.IsFalse(s.Exists(""));
        }

        [Test]
        public void Load_ReturnsNullForMissing()
        {
            var s = new InMemorySaveStorage();
            Assert.IsNull(s.Load("ghost"));
            Assert.IsNull(s.Load(null));
        }

        [Test]
        public void SaveLoad_RoundTripsTopLevelFields()
        {
            var s = new InMemorySaveStorage();
            var data = new SaveData
            {
                Version     = SaveData.CurrentVersion,
                ProfileId   = "main",
                SavedAtUnix = 1234567890,
            };
            s.Save("main", data);
            Assert.IsTrue(s.Exists("main"));

            var loaded = s.Load("main");
            Assert.IsNotNull(loaded);
            Assert.AreEqual(SaveData.CurrentVersion, loaded.Version);
            Assert.AreEqual("main", loaded.ProfileId);
            Assert.AreEqual(1234567890, loaded.SavedAtUnix);
        }

        [Test]
        public void Save_NullDataIsNoOp()
        {
            var s = new InMemorySaveStorage();
            s.Save("main", null);
            Assert.IsFalse(s.Exists("main"));
            Assert.AreEqual(0, s.WriteCount);
        }

        [Test]
        public void Save_NullProfileIsNoOp()
        {
            var s = new InMemorySaveStorage();
            s.Save(null, new SaveData());
            Assert.AreEqual(0, s.WriteCount);
        }

        [Test]
        public void Delete_RemovesAndReportsExistence()
        {
            var s = new InMemorySaveStorage();
            s.Save("main", new SaveData { ProfileId = "main" });
            Assert.IsTrue(s.Exists("main"));
            s.Delete("main");
            Assert.IsFalse(s.Exists("main"));
            Assert.AreEqual(1, s.DeleteCount);
        }

        [Test]
        public void Delete_MissingIsNoOp()
        {
            var s = new InMemorySaveStorage();
            s.Delete("ghost");
            Assert.AreEqual(0, s.DeleteCount);
        }

        [Test]
        public void MultipleProfiles_AreIndependent()
        {
            var s = new InMemorySaveStorage();
            s.Save("a", new SaveData { ProfileId = "a", SavedAtUnix = 1 });
            s.Save("b", new SaveData { ProfileId = "b", SavedAtUnix = 2 });

            Assert.AreEqual(1, s.Load("a").SavedAtUnix);
            Assert.AreEqual(2, s.Load("b").SavedAtUnix);

            s.Delete("a");
            Assert.IsFalse(s.Exists("a"));
            Assert.IsTrue(s.Exists("b"));
        }

        [Test]
        public void Clear_RemovesEverything()
        {
            var s = new InMemorySaveStorage();
            s.Save("a", new SaveData());
            s.Save("b", new SaveData());
            s.Clear();
            Assert.IsFalse(s.Exists("a"));
            Assert.IsFalse(s.Exists("b"));
        }

        [Test]
        public void WriteCount_IncrementsPerSave()
        {
            var s = new InMemorySaveStorage();
            s.Save("main", new SaveData());
            s.Save("main", new SaveData());
            s.Save("main", new SaveData());
            Assert.AreEqual(3, s.WriteCount);
        }
    }
}
