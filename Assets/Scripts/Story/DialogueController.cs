using UnityEngine;
using Matchmancer.UI;

namespace Matchmancer.Story
{
    /// <summary>
    /// MonoBehaviour bridge that wires DialogueRunner to the UI system and
    /// screen navigation. Owns a DialogueRunner instance and manages the
    /// story overlay screen lifecycle.
    /// </summary>
    public class DialogueController : MonoBehaviour
    {
        [SerializeField] private ScreenNavigator _screenNavigator;
        [SerializeField] private GameObject _dialoguePanel;

        private DialogueRunner _currentRunner;

        // ------------------------------------------------------------------
        // Events
        // ------------------------------------------------------------------

        public event System.Action<string> OnSceneStarted;
        public event System.Action<string> OnSceneEnded;

        // ------------------------------------------------------------------
        // Lifecycle
        // ------------------------------------------------------------------

        private void OnEnable()
        {
            if (_screenNavigator == null)
                _screenNavigator = FindObjectOfType<ScreenNavigator>();
        }

        // ------------------------------------------------------------------
        // Public
        // ------------------------------------------------------------------

        /// <summary>
        /// Start playing a story scene. Creates a new DialogueRunner,
        /// pushes the Story screen, and begins playback.
        /// </summary>
        public void PlayScene(StorySceneDefinition definition)
        {
            if (definition == null)
                return;

            // Stop any existing runner
            if (_currentRunner != null)
                _currentRunner = null;

            // Create and wire the new runner
            _currentRunner = new DialogueRunner(definition);

            _currentRunner.OnNodeReached += HandleNodeReached;
            _currentRunner.OnChoicePresented += HandleChoicePresented;
            _currentRunner.OnEffectTriggered += HandleEffectTriggered;
            _currentRunner.OnWaitStarted += HandleWaitStarted;
            _currentRunner.OnSceneComplete += HandleSceneComplete;

            // Push story overlay screen
            if (_screenNavigator != null)
                _screenNavigator.Push(ScreenId.StoryOverlay);

            // Show panel if available
            if (_dialoguePanel != null)
                _dialoguePanel.SetActive(true);

            // Begin playback
            _currentRunner.Start();
            OnSceneStarted?.Invoke(definition.Id);
        }

        /// <summary>
        /// Called by UI (e.g. tap on dialogue text) to advance the story.
        /// </summary>
        public void HandleAdvance()
        {
            if (_currentRunner == null || !_currentRunner.IsRunning)
                return;

            _currentRunner.Advance();
        }

        /// <summary>
        /// Called by choice buttons to pick a branch.
        /// </summary>
        public void HandleChoice(int choiceIndex)
        {
            if (_currentRunner == null || !_currentRunner.IsWaitingForChoice)
                return;

            _currentRunner.PickChoice(choiceIndex);
        }

        // ------------------------------------------------------------------
        // Runner Event Handlers
        // ------------------------------------------------------------------

        private void HandleNodeReached(StoryNode node, int index)
        {
            // Update UI with node content (to be implemented by UI system)
        }

        private void HandleChoicePresented(string[] choices)
        {
            // Display choice panel with buttons (to be implemented by UI system)
        }

        private void HandleEffectTriggered(string effectId)
        {
            // Trigger visual/audio effect (to be implemented by FX system)
        }

        private void HandleWaitStarted(float duration)
        {
            // Start timer for auto-advance (to be implemented by UI system)
        }

        private void HandleSceneComplete(string sceneId)
        {
            string completedSceneId = sceneId;

            // Hide panel
            if (_dialoguePanel != null)
                _dialoguePanel.SetActive(false);

            // Pop story screen
            if (_screenNavigator != null && _screenNavigator.CanPop)
                _screenNavigator.Pop();

            OnSceneEnded?.Invoke(completedSceneId);

            _currentRunner = null;
        }
    }
}
