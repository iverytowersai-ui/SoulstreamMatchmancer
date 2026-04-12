using System;
using System.Collections.Generic;

namespace Matchmancer.Story
{
    /// <summary>
    /// Manages story scene triggers. Maps trigger points (level pre/post, stage
    /// intro/outro) to scene IDs, tracks which scenes have been viewed, and
    /// determines whether a trigger should fire (scene exists and not yet seen).
    /// Pure C# — fully testable.
    /// </summary>
    public class StoryTriggerSystem
    {
        private readonly Dictionary<string, StorySceneDefinition> _scenes =
            new Dictionary<string, StorySceneDefinition>();

        private readonly HashSet<string> _seenSceneIds = new HashSet<string>();

        // ------------------------------------------------------------------
        // Events
        // ------------------------------------------------------------------

        /// <summary>Fires when a scene is marked as seen.</summary>
        public event Action<string> OnSceneSeen;

        // ------------------------------------------------------------------
        // Registration
        // ------------------------------------------------------------------

        /// <summary>
        /// Register a story scene. Scenes can only be triggered if registered.
        /// </summary>
        public void RegisterScene(string sceneId, StorySceneDefinition definition)
        {
            if (string.IsNullOrEmpty(sceneId) || definition == null)
                return;

            _scenes[sceneId] = definition;
        }

        // ------------------------------------------------------------------
        // Queries
        // ------------------------------------------------------------------

        /// <summary>Get the definition for a scene ID, or null if not registered.</summary>
        public StorySceneDefinition GetSceneForTrigger(string triggerId)
        {
            if (string.IsNullOrEmpty(triggerId))
                return null;

            _scenes.TryGetValue(triggerId, out var def);
            return def;
        }

        /// <summary>
        /// Check if a scene has been viewed.
        /// </summary>
        public bool HasBeenSeen(string sceneId)
        {
            return _seenSceneIds.Contains(sceneId);
        }

        /// <summary>
        /// Check if a trigger should fire: scene exists AND not yet seen.
        /// </summary>
        public bool ShouldTrigger(string triggerId)
        {
            return GetSceneForTrigger(triggerId) != null && !HasBeenSeen(triggerId);
        }

        // ------------------------------------------------------------------
        // State Management
        // ------------------------------------------------------------------

        /// <summary>Mark a scene as seen. Fires OnSceneSeen event.</summary>
        public void MarkSeen(string sceneId)
        {
            if (string.IsNullOrEmpty(sceneId))
                return;

            if (_seenSceneIds.Add(sceneId))
                OnSceneSeen?.Invoke(sceneId);
        }

        /// <summary>Create a snapshot of the seen state for save/restore.</summary>
        public StoryTriggerSnapshot CreateSnapshot()
        {
            return new StoryTriggerSnapshot
            {
                SeenSceneIds = new string[_seenSceneIds.Count],
            };
        }

        /// <summary>Restore seen state from a snapshot.</summary>
        public void LoadFromSnapshot(StoryTriggerSnapshot snapshot)
        {
            _seenSceneIds.Clear();

            if (snapshot?.SeenSceneIds != null)
                foreach (var id in snapshot.SeenSceneIds)
                    _seenSceneIds.Add(id);
        }
    }

    /// <summary>
    /// Serializable snapshot of which scenes have been seen.
    /// Used for save/restore of story progression.
    /// </summary>
    [System.Serializable]
    public class StoryTriggerSnapshot
    {
        public string[] SeenSceneIds = new string[0];
    }
}
