using System;

namespace Matchmancer.Story
{
    /// <summary>
    /// Pure C# state machine for executing a story scene. Manages playhead
    /// position, emits events for each node type, and orchestrates branching
    /// via choice nodes. No Unity dependencies — fully testable.
    /// </summary>
    public class DialogueRunner
    {
        private readonly StorySceneDefinition _definition;
        private int _currentNodeIndex;
        private bool _isRunning;
        private bool _isComplete;

        // ------------------------------------------------------------------
        // Events
        // ------------------------------------------------------------------

        /// <summary>Fires when a node is reached. (node, nodeIndex)</summary>
        public event Action<StoryNode, int> OnNodeReached;

        /// <summary>Fires when a Choice node is reached, blocking further advance. (choiceTexts)</summary>
        public event Action<string[]> OnChoicePresented;

        /// <summary>Fires when an Effect node is reached. (effectId)</summary>
        public event Action<string> OnEffectTriggered;

        /// <summary>Fires when a Wait node is reached. (duration)</summary>
        public event Action<float> OnWaitStarted;

        /// <summary>Fires when an EndScene node is reached. (sceneId)</summary>
        public event Action<string> OnSceneComplete;

        // ------------------------------------------------------------------
        // Properties
        // ------------------------------------------------------------------

        public int CurrentNodeIndex => _currentNodeIndex;
        public StoryNode CurrentNode => _currentNodeIndex >= 0 && _currentNodeIndex < _definition.Nodes.Length
            ? _definition.Nodes[_currentNodeIndex]
            : null;
        public bool IsRunning => _isRunning;
        public bool IsComplete => _isComplete;

        /// <summary>
        /// True if the current node is a Choice type and waiting for
        /// PickChoice() to be called.
        /// </summary>
        public bool IsWaitingForChoice => CurrentNode != null && CurrentNode.Type == StoryNodeType.Choice;

        // ------------------------------------------------------------------
        // Constructor
        // ------------------------------------------------------------------

        /// <summary>
        /// Initialize the runner with a story scene definition.
        /// Does not start playback — call Start() to begin.
        /// </summary>
        public DialogueRunner(StorySceneDefinition definition)
        {
            _definition = definition;
            _currentNodeIndex = 0;
            _isRunning = false;
            _isComplete = false;
        }

        // ------------------------------------------------------------------
        // Control
        // ------------------------------------------------------------------

        /// <summary>
        /// Begin playback at node 0. Fires OnNodeReached for the first node.
        /// </summary>
        public void Start()
        {
            if (_definition == null || _definition.Nodes == null || _definition.Nodes.Length == 0)
                return;

            _currentNodeIndex = 0;
            _isRunning = true;
            _isComplete = false;

            ProcessCurrentNode();
        }

        /// <summary>
        /// Advance to the next node. If waiting for a choice, this is a no-op
        /// until PickChoice() is called. If complete, this is a no-op.
        /// Auto-skips Effect and Wait nodes (they fire and advance immediately).
        /// </summary>
        public void Advance()
        {
            if (!_isRunning || _isComplete)
                return;

            // Don't advance if waiting for choice
            if (IsWaitingForChoice)
                return;

            // Move to next node
            _currentNodeIndex++;

            // Auto-skip Effect and Wait nodes
            while (_currentNodeIndex < _definition.Nodes.Length)
            {
                ProcessCurrentNode();

                // If we hit a node that requires user interaction or ends the scene, stop
                if (CurrentNode.Type == StoryNodeType.Dialogue ||
                    CurrentNode.Type == StoryNodeType.Narration ||
                    CurrentNode.Type == StoryNodeType.Choice ||
                    CurrentNode.Type == StoryNodeType.EndScene)
                    break;

                // Effect and Wait auto-advance
                _currentNodeIndex++;
            }
        }

        /// <summary>
        /// When at a Choice node, pick a branch by index into ChoiceNextIndices.
        /// Jumps the playhead to the chosen node and processes it.
        /// </summary>
        public void PickChoice(int choiceIndex)
        {
            if (!IsWaitingForChoice)
                return;

            StoryNode current = CurrentNode;
            if (choiceIndex < 0 || choiceIndex >= current.ChoiceNextIndices.Length)
                return;

            _currentNodeIndex = current.ChoiceNextIndices[choiceIndex];

            // Process the newly reached node
            if (_currentNodeIndex < _definition.Nodes.Length)
                ProcessCurrentNode();
        }

        /// <summary>
        /// Reset the runner to initial state, allowing replay of the scene.
        /// </summary>
        public void Reset()
        {
            _currentNodeIndex = 0;
            _isRunning = false;
            _isComplete = false;
        }

        // ------------------------------------------------------------------
        // Private
        // ------------------------------------------------------------------

        private void ProcessCurrentNode()
        {
            if (_currentNodeIndex < 0 || _currentNodeIndex >= _definition.Nodes.Length)
                return;

            StoryNode node = _definition.Nodes[_currentNodeIndex];
            OnNodeReached?.Invoke(node, _currentNodeIndex);

            switch (node.Type)
            {
                case StoryNodeType.Dialogue:
                case StoryNodeType.Narration:
                    // These nodes require user interaction; Advance() will be called by UI
                    break;

                case StoryNodeType.Choice:
                    OnChoicePresented?.Invoke(node.ChoiceTexts ?? new string[0]);
                    break;

                case StoryNodeType.Effect:
                    OnEffectTriggered?.Invoke(node.EffectId ?? "");
                    break;

                case StoryNodeType.Wait:
                    OnWaitStarted?.Invoke(node.WaitDuration);
                    break;

                case StoryNodeType.EndScene:
                    OnSceneComplete?.Invoke(_definition.Id);
                    _isComplete = true;
                    _isRunning = false;
                    break;
            }
        }
    }
}
