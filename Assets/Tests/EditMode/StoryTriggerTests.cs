using NUnit.Framework;
using Matchmancer.Story;

namespace Matchmancer.Tests
{
    /// <summary>
    /// Pure C# tests for <see cref="StoryTriggerSystem"/>. No Unity, no scene.
    /// Covers scene registration, seen tracking, trigger logic, and snapshot
    /// save/restore.
    /// </summary>
    [TestFixture]
    public class StoryTriggerTests
    {
        private StoryTriggerSystem _system;
        private string _onSceneSeenLastId;

        [SetUp]
        public void SetUp()
        {
            _system = new StoryTriggerSystem();
            _onSceneSeenLastId = null;
            _system.OnSceneSeen += (id) => { _onSceneSeenLastId = id; };
        }

        // ==================================================================
        // Registration
        // ==================================================================

        [Test]
        public void RegisterScene_StoresScene()
        {
            var def = CreateTestDefinition("scene_1");
            _system.RegisterScene("scene_1", def);

            var retrieved = _system.GetSceneForTrigger("scene_1");
            Assert.AreEqual(def, retrieved);
        }

        [Test]
        public void RegisterScene_NullId_NoOp()
        {
            var def = CreateTestDefinition("scene_1");
            _system.RegisterScene(null, def);

            var retrieved = _system.GetSceneForTrigger("scene_1");
            Assert.IsNull(retrieved);
        }

        [Test]
        public void RegisterScene_NullDefinition_NoOp()
        {
            _system.RegisterScene("scene_1", null);

            var retrieved = _system.GetSceneForTrigger("scene_1");
            Assert.IsNull(retrieved);
        }

        // ==================================================================
        // GetSceneForTrigger
        // ==================================================================

        [Test]
        public void GetSceneForTrigger_WithRegisteredScene_ReturnsDefinition()
        {
            var def = CreateTestDefinition("opening");
            _system.RegisterScene("opening", def);

            var result = _system.GetSceneForTrigger("opening");

            Assert.AreEqual(def, result);
        }

        [Test]
        public void GetSceneForTrigger_WithUnregisteredScene_ReturnsNull()
        {
            var result = _system.GetSceneForTrigger("nonexistent");

            Assert.IsNull(result);
        }

        [Test]
        public void GetSceneForTrigger_WithEmptyId_ReturnsNull()
        {
            var result = _system.GetSceneForTrigger("");

            Assert.IsNull(result);
        }

        // ==================================================================
        // HasBeenSeen
        // ==================================================================

        [Test]
        public void HasBeenSeen_BeforeMarking_ReturnsFalse()
        {
            Assert.IsFalse(_system.HasBeenSeen("scene_1"));
        }

        [Test]
        public void HasBeenSeen_AfterMarking_ReturnsTrue()
        {
            _system.MarkSeen("scene_1");

            Assert.IsTrue(_system.HasBeenSeen("scene_1"));
        }

        [Test]
        public void HasBeenSeen_WithEmptyId_ReturnsFalse()
        {
            _system.MarkSeen("");

            Assert.IsFalse(_system.HasBeenSeen(""));
        }

        // ==================================================================
        // MarkSeen
        // ==================================================================

        [Test]
        public void MarkSeen_FiresOnSceneSeenEvent()
        {
            _system.MarkSeen("level_1_intro");

            Assert.AreEqual("level_1_intro", _onSceneSeenLastId);
        }

        [Test]
        public void MarkSeen_SameSceneMultipleTimes_FiresOnce()
        {
            int callCount = 0;
            _system.OnSceneSeen += (id) => { callCount++; };

            _system.MarkSeen("level_1_intro");
            _system.MarkSeen("level_1_intro");

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void MarkSeen_NullId_NoOp()
        {
            int callCount = 0;
            _system.OnSceneSeen += (id) => { callCount++; };

            _system.MarkSeen(null);

            Assert.AreEqual(0, callCount);
        }

        // ==================================================================
        // ShouldTrigger
        // ==================================================================

        [Test]
        public void ShouldTrigger_WithUnregisteredScene_ReturnsFalse()
        {
            Assert.IsFalse(_system.ShouldTrigger("nonexistent"));
        }

        [Test]
        public void ShouldTrigger_WithRegisteredAndUnseen_ReturnsTrue()
        {
            var def = CreateTestDefinition("intro");
            _system.RegisterScene("intro", def);

            Assert.IsTrue(_system.ShouldTrigger("intro"));
        }

        [Test]
        public void ShouldTrigger_WithRegisteredAndSeen_ReturnsFalse()
        {
            var def = CreateTestDefinition("intro");
            _system.RegisterScene("intro", def);
            _system.MarkSeen("intro");

            Assert.IsFalse(_system.ShouldTrigger("intro"));
        }

        // ==================================================================
        // Snapshot Save/Restore
        // ==================================================================

        [Test]
        public void CreateSnapshot_CapturesSeenState()
        {
            _system.MarkSeen("scene_1");
            _system.MarkSeen("scene_2");

            var snapshot = _system.CreateSnapshot();

            Assert.IsNotNull(snapshot.SeenSceneIds);
            Assert.AreEqual(2, snapshot.SeenSceneIds.Length);
            Assert.Contains("scene_1", snapshot.SeenSceneIds);
            Assert.Contains("scene_2", snapshot.SeenSceneIds);
        }

        [Test]
        public void LoadFromSnapshot_RestoresSeenState()
        {
            var snapshot = new StoryTriggerSnapshot
            {
                SeenSceneIds = new[] { "level_1_intro", "level_1_outro" },
            };

            _system.LoadFromSnapshot(snapshot);

            Assert.IsTrue(_system.HasBeenSeen("level_1_intro"));
            Assert.IsTrue(_system.HasBeenSeen("level_1_outro"));
            Assert.IsFalse(_system.HasBeenSeen("level_2_intro"));
        }

        [Test]
        public void LoadFromSnapshot_ClearsExistingState()
        {
            _system.MarkSeen("old_scene");
            Assert.IsTrue(_system.HasBeenSeen("old_scene"));

            var snapshot = new StoryTriggerSnapshot
            {
                SeenSceneIds = new[] { "new_scene" },
            };
            _system.LoadFromSnapshot(snapshot);

            Assert.IsFalse(_system.HasBeenSeen("old_scene"));
            Assert.IsTrue(_system.HasBeenSeen("new_scene"));
        }

        [Test]
        public void Snapshot_IsDetachedCopy()
        {
            var snapshot = _system.CreateSnapshot();
            _system.MarkSeen("later_scene");

            // Snapshot should not change
            Assert.IsFalse(snapshot.SeenSceneIds.Length == 1 && snapshot.SeenSceneIds[0] == "later_scene");
        }

        [Test]
        public void LoadFromSnapshot_WithNullSnapshot_NoOp()
        {
            _system.MarkSeen("scene_1");
            _system.LoadFromSnapshot(null);

            // Should be cleared, not errored
            Assert.IsFalse(_system.HasBeenSeen("scene_1"));
        }

        [Test]
        public void LoadFromSnapshot_WithNullSeenSceneIds_Clears()
        {
            _system.MarkSeen("scene_1");

            var snapshot = new StoryTriggerSnapshot { SeenSceneIds = null };
            _system.LoadFromSnapshot(snapshot);

            Assert.IsFalse(_system.HasBeenSeen("scene_1"));
        }

        // ==================================================================
        // Helpers
        // ==================================================================

        private StorySceneDefinition CreateTestDefinition(string sceneId)
        {
            return new StorySceneDefinition
            {
                Id = sceneId,
                Title = $"Test Scene: {sceneId}",
                Nodes = new[]
                {
                    new StoryNode { Type = StoryNodeType.Dialogue, Text = "Test", SpeakerName = "Test" },
                    new StoryNode { Type = StoryNodeType.EndScene },
                },
            };
        }
    }
}
