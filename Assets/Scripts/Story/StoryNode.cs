namespace Matchmancer.Story
{
    /// <summary>
    /// A single beat in a story scene: dialogue, narration, choice, effect,
    /// timed wait, or scene end. Pure C# POCO for testability.
    /// </summary>
    public class StoryNode
    {
        public StoryNodeType Type { get; set; }

        /// <summary>Name of the character speaking (for Dialogue nodes).</summary>
        public string SpeakerName { get; set; }

        /// <summary>Portrait ID for the character (for Dialogue nodes).</summary>
        public string SpeakerPortraitId { get; set; }

        /// <summary>Main text: dialogue, narration, or choice label.</summary>
        public string Text { get; set; }

        /// <summary>
        /// Array of choice texts (for Choice nodes only).
        /// Parallel to ChoiceNextIndices — index i is the text for choice i.
        /// </summary>
        public string[] ChoiceTexts { get; set; }

        /// <summary>
        /// Array of next node indices (for Choice nodes only).
        /// Parallel to ChoiceTexts — index i points to the node for choice i.
        /// </summary>
        public int[] ChoiceNextIndices { get; set; }

        /// <summary>Duration in seconds to wait before auto-advancing (for Wait nodes).</summary>
        public float WaitDuration { get; set; }

        /// <summary>ID of the effect to trigger (for Effect nodes).</summary>
        public string EffectId { get; set; }

        /// <summary>Deep copy constructor.</summary>
        public StoryNode Clone()
        {
            var cloned = new StoryNode
            {
                Type = Type,
                SpeakerName = SpeakerName,
                SpeakerPortraitId = SpeakerPortraitId,
                Text = Text,
                WaitDuration = WaitDuration,
                EffectId = EffectId,
            };

            if (ChoiceTexts != null)
            {
                cloned.ChoiceTexts = new string[ChoiceTexts.Length];
                System.Array.Copy(ChoiceTexts, cloned.ChoiceTexts, ChoiceTexts.Length);
            }

            if (ChoiceNextIndices != null)
            {
                cloned.ChoiceNextIndices = new int[ChoiceNextIndices.Length];
                System.Array.Copy(ChoiceNextIndices, cloned.ChoiceNextIndices, ChoiceNextIndices.Length);
            }

            return cloned;
        }
    }
}
