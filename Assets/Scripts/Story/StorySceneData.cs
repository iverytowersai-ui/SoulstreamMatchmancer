using System;
using System.Collections.Generic;
using UnityEngine;

namespace Matchmancer.Story
{
    /// <summary>
    /// ScriptableObject wrapper for StorySceneDefinition. Serializes the
    /// scene graph in the Inspector using StoryNodeEntry. The ToDefinition()
    /// method converts it to the pure C# definition used by DialogueRunner.
    /// </summary>
    [CreateAssetMenu(menuName = "Matchmancer/Story Scene Data")]
    public class StorySceneData : ScriptableObject
    {
        [Header("Identity")]
        public string id = "";
        public string title = "";

        [Header("Nodes")]
        public List<StoryNodeEntry> nodes = new List<StoryNodeEntry>();

        /// <summary>
        /// Serializable wrapper for a story node, compatible with Unity's
        /// serialization system for Inspector editing.
        /// </summary>
        [Serializable]
        public class StoryNodeEntry
        {
            [Header("Node Type")]
            public StoryNodeType type = StoryNodeType.Dialogue;

            [Header("Dialogue / Narration")]
            public string speakerName = "";
            public string speakerPortraitId = "";

            [TextArea(3, 5)]
            public string text = "";

            [Header("Choice (if type = Choice)")]
            public string[] choiceTexts = new string[0];
            public int[] choiceNextIndices = new int[0];

            [Header("Wait (if type = Wait)")]
            [Min(0)] public float waitDuration = 1f;

            [Header("Effect (if type = Effect)")]
            public string effectId = "";
        }

        /// <summary>
        /// Convert the serialized form to a pure C# definition.
        /// Validates the result and throws if invalid.
        /// </summary>
        public StorySceneDefinition ToDefinition()
        {
            if (nodes == null || nodes.Count == 0)
                throw new System.InvalidOperationException(
                    $"StorySceneData '{name}': nodes list is empty.");

            var storyNodes = new StoryNode[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                var entry = nodes[i];
                storyNodes[i] = new StoryNode
                {
                    Type = entry.type,
                    SpeakerName = entry.speakerName ?? "",
                    SpeakerPortraitId = entry.speakerPortraitId ?? "",
                    Text = entry.text ?? "",
                    ChoiceTexts = entry.choiceTexts != null ? new string[entry.choiceTexts.Length] : new string[0],
                    ChoiceNextIndices = entry.choiceNextIndices != null ? new int[entry.choiceNextIndices.Length] : new int[0],
                    WaitDuration = entry.waitDuration,
                    EffectId = entry.effectId ?? "",
                };

                if (entry.choiceTexts != null)
                    System.Array.Copy(entry.choiceTexts, storyNodes[i].ChoiceTexts, entry.choiceTexts.Length);

                if (entry.choiceNextIndices != null)
                    System.Array.Copy(entry.choiceNextIndices, storyNodes[i].ChoiceNextIndices, entry.choiceNextIndices.Length);
            }

            var definition = new StorySceneDefinition
            {
                Id = id,
                Title = title,
                Nodes = storyNodes,
            };

            definition.Validate();
            return definition;
        }
    }
}
