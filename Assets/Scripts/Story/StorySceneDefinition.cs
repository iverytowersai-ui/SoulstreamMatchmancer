namespace Matchmancer.Story
{
    /// <summary>
    /// A complete story scene definition: a sequence of nodes that play in
    /// order, with choices allowing branching. Pure C# POCO for testability.
    /// </summary>
    public class StorySceneDefinition
    {
        public string Id { get; set; }
        public string Title { get; set; }

        /// <summary>
        /// Array of nodes that make up this scene. Must end with an EndScene
        /// node (validated by Validate()).
        /// </summary>
        public StoryNode[] Nodes { get; set; }

        /// <summary>
        /// Validates that the scene definition is well-formed:
        /// - Nodes array is not null or empty
        /// - Nodes array ends with an EndScene type
        /// Throws System.InvalidOperationException if validation fails.
        /// </summary>
        public void Validate()
        {
            if (Nodes == null || Nodes.Length == 0)
                throw new System.InvalidOperationException(
                    $"StorySceneDefinition '{Id}': Nodes array is null or empty.");

            if (Nodes[Nodes.Length - 1].Type != StoryNodeType.EndScene)
                throw new System.InvalidOperationException(
                    $"StorySceneDefinition '{Id}': Last node must be of type EndScene.");
        }

        /// <summary>
        /// Deep copy: clones the definition and all nodes within it.
        /// </summary>
        public StorySceneDefinition Clone()
        {
            var cloned = new StorySceneDefinition
            {
                Id = Id,
                Title = Title,
            };

            if (Nodes != null)
            {
                cloned.Nodes = new StoryNode[Nodes.Length];
                for (int i = 0; i < Nodes.Length; i++)
                    cloned.Nodes[i] = Nodes[i]?.Clone();
            }

            return cloned;
        }
    }
}
