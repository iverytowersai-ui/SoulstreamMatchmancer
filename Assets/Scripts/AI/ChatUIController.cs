using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Soulstream.AI
{
    /// <summary>
    /// Minimal chat UI. Hook up a TMP_InputField, a send Button, and a
    /// TextMeshProUGUI log in the inspector. Drop a ClaudeManager on a
    /// GameObject in the scene and assign it.
    /// </summary>
    public class ChatUIController : MonoBehaviour
    {
        [SerializeField] private ClaudeManager claudeManager;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button sendButton;
        [SerializeField] private TextMeshProUGUI chatLog;

        private readonly List<ClaudeManager.ClaudeMessage> history = new List<ClaudeManager.ClaudeMessage>();

        private void Start()
        {
            if (claudeManager == null) claudeManager = ClaudeManager.Instance;
            sendButton.onClick.AddListener(OnSend);
            inputField.onSubmit.AddListener(_ => OnSend());
        }

        private void OnSend()
        {
            string text = inputField.text?.Trim();
            if (string.IsNullOrEmpty(text)) return;

            AppendLog($"<color=#88f>You:</color> {text}");
            history.Add(new ClaudeManager.ClaudeMessage { role = "user", content = text });
            inputField.text = "";
            sendButton.interactable = false;

            claudeManager.SendConversation(history,
                reply =>
                {
                    history.Add(new ClaudeManager.ClaudeMessage { role = "assistant", content = reply });
                    AppendLog($"<color=#f8a>Claude:</color> {reply}");
                    sendButton.interactable = true;
                },
                err =>
                {
                    AppendLog($"<color=#f55>Error:</color> {err}");
                    sendButton.interactable = true;
                });
        }

        private void AppendLog(string line)
        {
            chatLog.text += (chatLog.text.Length > 0 ? "\n\n" : "") + line;
        }
    }
}
