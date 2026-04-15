using NUnit.Framework;
using Matchmancer.Story;

namespace Matchmancer.Tests
{
    /// <summary>
    /// Pure C# tests for <see cref="DialogueRunner"/>. No Unity, no scene.
    /// Covers node progression, choice branching, effect/wait auto-skip,
    /// and scene completion.
    /// </summary>
    [TestFixture]
    public class DialogueRunnerTests
    {
        private DialogueRunner _runner;
        private StoryNode _nodeReachedLastNode;
        private int _nodeReachedLastIndex;
        private string[] _choicePresentedLastChoices;
        private string _effectTriggeredLastId;
        private float _waitStartedLastDuration;
        private string _sceneCompleteLastId;

        [SetUp]
        public void SetUp()
        {
            _nodeReachedLastNode = null;
            _nodeReachedLastIndex = -1;
            _choicePresentedLastChoices = null;
            _effectTriggeredLastId = null;
            _waitStartedLastDuration = 0f;
            _sceneCompleteLastId = null;
        }

        // ==================================================================
        // Start / Initial State
        // ==================================================================

        [Test]
        public void Start_WithValidDefinition_SetsRunningAndNodeIndex()
        {
            var def = CreateSimpleDefinition();
            _runner = new DialogueRunner(def);

            _runner.Start();

            Assert.IsTrue(_runner.IsRunning);
            Assert.AreEqual(0, _runner.CurrentNodeIndex);
            Assert.IsNotNull(_runner.CurrentNode);
        }

        [Test]
        public void Start_FiresOnNodeReachedForFirstNode()
        {
            var def = CreateSimpleDefinition();
            _runner = new DialogueRunner(def);
            _runner.OnNodeReached += (node, idx) =>
            {
                _nodeReachedLastNode = node;
                _nodeReachedLastIndex = idx;
            };

            _runner.Start();

            Assert.AreEqual(0, _nodeReachedLastIndex);
            Assert.AreEqual(StoryNodeType.Dialogue, _nodeReachedLastNode.Type);
        }

        [Test]
        public void Start_WithNullDefinition_NoOp()
        {
            _runner = new DialogueRunner(null);
            _runner.Start();

            Assert.IsFalse(_runner.IsRunning);
        }

        // ==================================================================
        // Dialogue / Narration Nodes
        // ==================================================================

        [Test]
        public void Advance_ThroughDialogueNodes_StepsSequentially()
        {
            var def = CreateSimpleDefinition();
            _runner = new DialogueRunner(def);
            _runner.Start();

            Assert.AreEqual(0, _runner.CurrentNodeIndex);
            Assert.AreEqual(StoryNodeType.Dialogue, _runner.CurrentNode.Type);

            _runner.Advance();
            Assert.AreEqual(1, _runner.CurrentNodeIndex);
            Assert.AreEqual(StoryNodeType.Narration, _runner.CurrentNode.Type);
        }

        [Test]
        public void Advance_FiresOnNodeReachedForEachNode()
        {
            var def = CreateSimpleDefinition();
            _runner = new DialogueRunner(def);
            int callCount = 0;
            _runner.OnNodeReached += (node, idx) => callCount++;

            _runner.Start(); // Count 1
            _runner.Advance(); // Count 2

            Assert.AreEqual(2, callCount);
        }

        // ==================================================================
        // Choice Nodes
        // ==================================================================

        [Test]
        public void Choice_BlocksAdvanceUntilPickChoice()
        {
            var def = CreateChoiceDefinition();
            _runner = new DialogueRunner(def);
            _runner.Start();
            _runner.Advance(); // Reach the Choice node

            Assert.IsTrue(_runner.IsWaitingForChoice);

            // Calling Advance should be a no-op
            int originalIndex = _runner.CurrentNodeIndex;
            _runner.Advance();
            Assert.AreEqual(originalIndex, _runner.CurrentNodeIndex);
        }

        [Test]
        public void Choice_FiresOnChoicePresentedWithTexts()
        {
            var def = CreateChoiceDefinition();
            _runner = new DialogueRunner(def);
            _runner.OnChoicePresented += (choices) => { _choicePresentedLastChoices = choices; };

            _runner.Start();
            _runner.Advance();

            Assert.IsNotNull(_choicePresentedLastChoices);
            Assert.AreEqual(2, _choicePresentedLastChoices.Length);
            Assert.AreEqual("Choice A", _choicePresentedLastChoices[0]);
            Assert.AreEqual("Choice B", _choicePresentedLastChoices[1]);
        }

        [Test]
        public void PickChoice_JumpsToCorrectBranch()
        {
            var def = CreateChoiceDefinition();
            _runner = new DialogueRunner(def);
            _runner.Start();
            _runner.Advance(); // Reach Choice node at index 2

            int choiceNodeIndex = _runner.CurrentNodeIndex;
            _runner.PickChoice(0); // Pick first choice

            // Should have jumped to the node indicated by ChoiceNextIndices[0]
            int expectedNextIndex = def.Nodes[choiceNodeIndex].ChoiceNextIndices[0];
            Assert.AreEqual(expectedNextIndex, _runner.CurrentNodeIndex);
        }

        [Test]
        public void PickChoice_InvalidIndex_NoOp()
        {
            var def = CreateChoiceDefinition();
            _runner = new DialogueRunner(def);
            _runner.Start();
            _runner.Advance();

            int currentIndex = _runner.CurrentNodeIndex;
            _runner.PickChoice(999);

            Assert.AreEqual(currentIndex, _runner.CurrentNodeIndex);
        }

        // ==================================================================
        // Effect Nodes
        // ==================================================================

        [Test]
        public void Effect_FiresOnEffectTriggered()
        {
            var def = CreateEffectDefinition();
            _runner = new DialogueRunner(def);
            _runner.OnEffectTriggered += (id) => { _effectTriggeredLastId = id; };

            _runner.Start();
            _runner.Advance();

            Assert.AreEqual("screen_shake_01", _effectTriggeredLastId);
        }

        [Test]
        public void Effect_AutoSkipsAfterFiring()
        {
            var def = CreateEffectDefinition();
            _runner = new DialogueRunner(def);
            _runner.Start();

            // Node 0: Dialogue. Node 1: Effect. Node 2: Narration. Node 3: EndScene.
            Assert.AreEqual(0, _runner.CurrentNodeIndex);

            _runner.Advance();
            // Should skip the Effect node and land on the Narration
            Assert.AreEqual(2, _runner.CurrentNodeIndex);
        }

        // ==================================================================
        // Wait Nodes
        // ==================================================================

        [Test]
        public void Wait_FiresOnWaitStarted()
        {
            var def = CreateWaitDefinition();
            _runner = new DialogueRunner(def);
            _runner.OnWaitStarted += (duration) => { _waitStartedLastDuration = duration; };

            _runner.Start();
            _runner.Advance();

            Assert.AreEqual(2.5f, _waitStartedLastDuration);
        }

        [Test]
        public void Wait_AutoSkipsAfterFiring()
        {
            var def = CreateWaitDefinition();
            _runner = new DialogueRunner(def);
            _runner.Start();

            // Node 0: Dialogue. Node 1: Wait. Node 2: Narration. Node 3: EndScene.
            Assert.AreEqual(0, _runner.CurrentNodeIndex);

            _runner.Advance();
            // Should skip the Wait node and land on the Narration
            Assert.AreEqual(2, _runner.CurrentNodeIndex);
        }

        // ==================================================================
        // EndScene Node
        // ==================================================================

        [Test]
        public void EndScene_FiresOnSceneComplete()
        {
            var def = CreateSimpleDefinition();
            _runner = new DialogueRunner(def);
            _runner.OnSceneComplete += (id) => { _sceneCompleteLastId = id; };

            _runner.Start();
            _runner.Advance();
            _runner.Advance();

            Assert.AreEqual("test_scene", _sceneCompleteLastId);
        }

        [Test]
        public void EndScene_SetsIsComplete()
        {
            var def = CreateSimpleDefinition();
            _runner = new DialogueRunner(def);
            _runner.Start();
            _runner.Advance();
            _runner.Advance();

            Assert.IsTrue(_runner.IsComplete);
            Assert.IsFalse(_runner.IsRunning);
        }

        [Test]
        public void Advance_WhenComplete_IsNoOp()
        {
            var def = CreateSimpleDefinition();
            _runner = new DialogueRunner(def);
            _runner.Start();
            _runner.Advance();
            _runner.Advance();

            int indexBeforeAdvance = _runner.CurrentNodeIndex;
            _runner.Advance();

            Assert.AreEqual(indexBeforeAdvance, _runner.CurrentNodeIndex);
        }

        // ==================================================================
        // Reset
        // ==================================================================

        [Test]
        public void Reset_AllowsReplay()
        {
            var def = CreateSimpleDefinition();
            _runner = new DialogueRunner(def);
            _runner.Start();
            _runner.Advance();
            _runner.Advance();

            _runner.Reset();

            Assert.IsFalse(_runner.IsRunning);
            Assert.IsFalse(_runner.IsComplete);
            Assert.AreEqual(0, _runner.CurrentNodeIndex);
        }

        // ==================================================================
        // Helpers
        // ==================================================================

        private StorySceneDefinition CreateSimpleDefinition()
        {
            return new StorySceneDefinition
            {
                Id = "test_scene",
                Title = "Test Scene",
                Nodes = new[]
                {
                    new StoryNode { Type = StoryNodeType.Dialogue, Text = "Hello", SpeakerName = "Hero" },
                    new StoryNode { Type = StoryNodeType.Narration, Text = "The end." },
                    new StoryNode { Type = StoryNodeType.EndScene },
                },
            };
        }

        private StorySceneDefinition CreateChoiceDefinition()
        {
            return new StorySceneDefinition
            {
                Id = "choice_scene",
                Title = "Choice Scene",
                Nodes = new[]
                {
                    new StoryNode { Type = StoryNodeType.Dialogue, Text = "Pick one.", SpeakerName = "NPC" },
                    new StoryNode
                    {
                        Type = StoryNodeType.Choice,
                        Text = "What do you choose?",
                        ChoiceTexts = new[] { "Choice A", "Choice B" },
                        ChoiceNextIndices = new[] { 2, 3 },
                    },
                    new StoryNode { Type = StoryNodeType.Narration, Text = "You chose A." },
                    new StoryNode { Type = StoryNodeType.Narration, Text = "You chose B." },
                    new StoryNode { Type = StoryNodeType.EndScene },
                },
            };
        }

        private StorySceneDefinition CreateEffectDefinition()
        {
            return new StorySceneDefinition
            {
                Id = "effect_scene",
                Title = "Effect Scene",
                Nodes = new[]
                {
                    new StoryNode { Type = StoryNodeType.Dialogue, Text = "Watch this!", SpeakerName = "Wizard" },
                    new StoryNode { Type = StoryNodeType.Effect, EffectId = "screen_shake_01" },
                    new StoryNode { Type = StoryNodeType.Narration, Text = "Whoa!" },
                    new StoryNode { Type = StoryNodeType.EndScene },
                },
            };
        }

        private StorySceneDefinition CreateWaitDefinition()
        {
            return new StorySceneDefinition
            {
                Id = "wait_scene",
                Title = "Wait Scene",
                Nodes = new[]
                {
                    new StoryNode { Type = StoryNodeType.Dialogue, Text = "Loading...", SpeakerName = "System" },
                    new StoryNode { Type = StoryNodeType.Wait, WaitDuration = 2.5f },
                    new StoryNode { Type = StoryNodeType.Narration, Text = "Done!" },
                    new StoryNode { Type = StoryNodeType.EndScene },
                },
            };
        }
    }
}
