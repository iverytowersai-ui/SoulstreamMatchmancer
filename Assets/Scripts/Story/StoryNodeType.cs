namespace Matchmancer.Story
{
    /// <summary>
    /// Discriminator for a single beat in a story scene. The
    /// <see cref="DialogueRunner"/> switches behavior per type; the UI reads
    /// the type to decide what to display (speech bubble, narration bar,
    /// choice panel, etc.).
    /// </summary>
    public enum StoryNodeType
    {
        /// <summary>Character speech — shows portrait + name + text.</summary>
        Dialogue  = 0,

        /// <summary>Narrator/environment text — no portrait, centered.</summary>
        Narration = 1,

        /// <summary>Branch point — displays 2..4 choices, waits for pick.</summary>
        Choice    = 2,

        /// <summary>Brief visual/audio beat — screen shake, flash, SFX.</summary>
        Effect    = 3,

        /// <summary>Auto-advance after a timed pause (cutscene pacing).</summary>
        Wait      = 4,

        /// <summary>Sentinel — marks the final node. DialogueRunner fires
        /// <see cref="DialogueRunner.OnSceneComplete"/> and stops.</summary>
        EndScene  = 5,
    }
}
