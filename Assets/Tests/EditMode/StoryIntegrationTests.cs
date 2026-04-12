using NUnit.Framework;
using Matchmancer.Story;

namespace Matchmancer.Tests
{
    /// <summary>
    /// Integration tests wiring DialogueRunner and StoryTriggerSystem together.
    /// Tests full scene playthrough, branching, and trigger lifecycle.
    /// </summary>
    [TestFixture]
    public class StoryIntegrationTests
    {
        private DialogueRunner _runner;
        private StoryTriggerSystem _triggerSystem;

        [SetUp]
        public void SetUp()
        {
            _runner = null;
            _triggerSystem = new StoryTriggerSystem();
        }

        // ==================================================================
        // Full Playthrough
        // ==================================================================

        [Test]
        public void FullPlaythrough_StartToEnd()
        {
            var def = CreateLinearScene();
            _triggerSystem.RegisterScene("linear", def);

            _runner = new DialogueRunner(def);
            string completedSceneId = null;
            _runner.OnSceneComplete += (id) => { completedSceneId = id; };

            _runner.Start();
            Assert.AreEqual(0, _runner.CurrentNodeIndex);

            _runner.Advance(); // Node 1: Narration
            Assert.AreEqual(1, _runner.CurrentNodeIndex);

            _runner.Advance(); // Skip Effect, land on Node 3: Dialogue
            Assert.AreEqual(3, _runner.CurrentNodeIndex);

            _runner.Advance(); // Node 4: EndScene
            Assert.IsTrue(_runner.IsComplete);
            Assert.AreEqual("linear", completedSceneId);
        }

        // ==================================================================
        // Choice Branching
        // ==================================================================

        [Test]
        public void ChoiceBranching_PathA()
        {
            var def = CreateBranchingScene();
            _runner = new DialogueRunner(def);

            _runner.Start();
            _runner.Advance(); // Reach Choice node at index 1
            _runner.PickChoice(0); // Pick "Path A" -> jumps to node 3

            Assert.AreEqual(3, _runner.CurrentNodeIndex);
            Assert.AreEqual("You took the left path.", _runner.CurrentNode.Text);
        }

        [Test]
        public void ChoiceBranching_PathB()
        {
            var def = CreateBranchingScene();
            _runner = new DialogueRunner(def);

            _runner.Start();
            _runner.Advance(); // Reach Choice node at index 1
            _runner.PickChoice(1); // Pick "Path B" -> jumps to node 4

            Assert.AreEqual(4, _runner.CurrentNodeIndex);
            Assert.AreEqual("You took the right path.", _runner.CurrentNode.Text);
        }

        // ==================================================================
        // Trigger Hooks
        // ==================================================================

        [Test]
        public void LevelPreTrigger_FiresBeforeLevel()
        {
            var preDef = CreateLinearScene();
            _triggerSystem.RegisterScene("level_1_pre", preDef);

            Assert.IsTrue(_triggerSystem.ShouldTrigger("level_1_pre"));

            // Simulate playing the scene
            _runner = new DialogueRunner(preDef);
            _runner.Start();
            while (!_runner.IsComplete)
                _runner.Advance();

            // Now mark it as seen
            _triggerSystem.MarkSeen("level_1_pre");

            // Next time, it should not trigger
            Assert.IsFalse(_triggerSystem.ShouldTrigger("level_1_pre"));
        }

        [Test]
        public void LevelPostTrigger_FiresAfterLevel()
        {
            var postDef = CreateLinearScene();
            _triggerSystem.RegisterScene("level_1_post", postDef);

            // Level hasn't completed yet, so post-trigger should be available
            Assert.IsTrue(_triggerSystem.ShouldTrigger("level_1_post"));

            // Play it
            _runner = new DialogueRunner(postDef);
            _runner.Start();
            while (!_runner.IsComplete)
                _runner.Advance();

            _triggerSystem.MarkSeen("level_1_post");

            // No longer available
            Assert.IsFalse(_triggerSystem.ShouldTrigger("level_1_post"));
        }

        [Test]
        public void StageIntreTrigger_FiresBeforeStage()
        {
            var introDef = CreateLinearScene();
            _triggerSystem.RegisterScene("stage_1_intro", introDef);

            Assert.IsTrue(_triggerSystem.ShouldTrigger("stage_1_intro"));

            _runner = new DialogueRunner(introDef);
            _runner.Start();
            while (!_runner.IsComplete)
                _runner.Advance();

            _triggerSystem.MarkSeen("stage_1_intro");

            Assert.IsFalse(_triggerSystem.ShouldTrigger("stage_1_intro"));
        }

        // ==================================================================
        // Replay After Seen
        // ==================================================================

        [Test]
        public void SeenScene_CanStillBePlayed()
        {
            var def = CreateLinearScene();
            _triggerSystem.RegisterScene("replay_scene", def);

            // Play it once
            _runner = new DialogueRunner(def);
            _runner.Start();
            while (!_runner.IsComplete)
                _runner.Advance();

            _triggerSystem.MarkSeen("replay_scene");

            // Even though it's seen, the runner can still play it
            _runner = new DialogueRunner(def);
            _runner.Start();
            Assert.IsTrue(_runner.IsRunning);

            // And can complete again
            while (!_runner.IsComplete)
                _runner.Advance();

            Assert.IsTrue(_runner.IsComplete);
        }

        [Test]
        public void ShouldTrigger_BlocksReplay()
        {
            var def = CreateLinearScene();
            _triggerSystem.RegisterScene("once_only", def);

            // Trigger fires before play
            Assert.IsTrue(_triggerSystem.ShouldTrigger("once_only"));

            // Play and mark seen
            _triggerSystem.MarkSeen("once_only");

            // Trigger no longer fires (gate prevents auto-playback)
            Assert.IsFalse(_triggerSystem.ShouldTrigger("once_only"));
        }

        // ==================================================================
        // Snapshot Persistence
        // ==================================================================

        [Test]
        public void SaveAndLoadSnapshot_PreservesProgressAcrossMultipleTriggers()
        {
            var level1Pre = CreateLinearScene();
            var level2Pre = CreateLinearScene();
            _triggerSystem.RegisterScene("level_1_pre", level1Pre);
            _triggerSystem.RegisterScene("level_2_pre", level2Pre);

            // Play and mark level 1 intro as seen
            _triggerSystem.MarkSeen("level_1_pre");

            // Snapshot
            var snapshot = _triggerSystem.CreateSnapshot();

            // Create new system and load snapshot
            var newSystem = new StoryTriggerSystem();
            newSystem.RegisterScene("level_1_pre", level1Pre);
            newSystem.RegisterScene("level_2_pre", level2Pre);
            newSystem.LoadFromSnapshot(snapshot);

            // Level 1 should still be seen
            Assert.IsFalse(newSystem.ShouldTrigger("level_1_pre"));

            // Level 2 should still be available
            Assert.IsTrue(newSystem.ShouldTrigger("level_2_pre"));
        }

        // ==================================================================
        // Helpers
        // ==================================================================

        private StorySceneDefinition CreateLinearScene()
        {
            return new StorySceneDefinition
            {
                Id = "linear",
                Title = "Linear Scene",
                Nodes = new[]
                {
                    new StoryNode { Type = StoryNodeType.Dialogue, Text = "Hello.", SpeakerName = "Hero" },
                    new StoryNode { Type = StoryNodeType.Narration, Text = "A tale unfolds." },
                    new StoryNode { Type = StoryNodeType.Effect, EffectId = "fade_out" },
                    new StoryNode { Type = StoryNodeType.Dialogue, Text = "Goodbye.", SpeakerName = "Hero" },
                    new StoryNode { Type = StoryNodeType.EndScene },
                },
            };
        }

        private StorySceneDefinition CreateBranchingScene()
        {
            return new StorySceneDefinition
            {
                Id = "branching",
                Title = "Branching Scene",
                Nodes = new[]
                {
                    new StoryNode { Type = StoryNodeType.Dialogue, Text = "Which path?", SpeakerName = "Guide" },
                    new StoryNode
                    {
                        Type = StoryNodeType.Choice,
                        Text = "Choose wisely.",
                        ChoiceTexts = new[] { "Path A", "Path B" },
                        ChoiceNextIndices = new[] { 3, 4 },
                    },
                    new StoryNode { Type = StoryNodeType.Narration, Text = "Placeholder" },
                    new StoryNode { Type = StoryNodeType.Narration, Text = "You took the left path." },
                    new StoryNode { Type = StoryNodeType.Narration, Text = "You took the right path." },
                    new StoryNode { Type = StoryNodeType.EndScene },
                },
            };
        }
    }
}
