using UnityEngine;

namespace Soulstream.AI
{
    /// <summary>
    /// Sanity check. Add to any GameObject in a scene that also has ClaudeManager.
    /// Press Play — the response prints to the Console.
    /// </summary>
    public class ClaudeTest : MonoBehaviour
    {
        [SerializeField] private ClaudeManager claudeManager;
        [TextArea(2, 5)]
        [SerializeField] private string prompt = "In one short sentence, describe a dark-fantasy match-3 combat tile.";

        private void Start()
        {
            if (claudeManager == null) claudeManager = ClaudeManager.Instance;

            claudeManager.SendMessage(prompt,
                reply => Debug.Log($"[Claude] {reply}"),
                err   => Debug.LogError($"[Claude ERROR] {err}"));
        }
    }
}
