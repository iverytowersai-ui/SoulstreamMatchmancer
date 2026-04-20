using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Soulstream.AI
{
    /// <summary>
    /// Unity-friendly Claude API client. Uses UnityWebRequest so it works on
    /// Editor, Standalone, Mobile, and WebGL without extra .NET dependencies.
    ///
    /// SECURITY WARNING: Do NOT ship a real API key inside a client build.
    /// For production, proxy through your own server. This class is intended
    /// for local development and prototyping.
    /// </summary>
    public class ClaudeManager : MonoBehaviour
    {
        private const string API_URL = "https://api.anthropic.com/v1/messages";
        private const string API_VERSION = "2023-06-01";

        [Header("API Config")]
        [Tooltip("Your Anthropic API key. For dev only. Leave empty to read ANTHROPIC_API_KEY env var.")]
        [SerializeField] private string apiKey = "";

        [Tooltip("Model to call. Current options: claude-opus-4-6, claude-sonnet-4-6, claude-haiku-4-5-20251001")]
        [SerializeField] private string model = "claude-sonnet-4-6";

        [Tooltip("Max tokens in the response.")]
        [SerializeField] private int maxTokens = 1024;

        [Tooltip("System prompt applied to every conversation (leave blank for none).")]
        [TextArea(3, 10)]
        [SerializeField] private string systemPrompt = "";

        public static ClaudeManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (string.IsNullOrEmpty(apiKey))
                apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY") ?? "";

            if (string.IsNullOrEmpty(apiKey))
                Debug.LogError("[ClaudeManager] No API key set. Assign one in the inspector or set ANTHROPIC_API_KEY.");
        }

        // --------- Public API ---------

        /// <summary>Send a single user prompt and receive the full text reply via callback.</summary>
        public void SendMessage(string userMessage, Action<string> onComplete, Action<string> onError = null)
        {
            var history = new List<ClaudeMessage> { new ClaudeMessage { role = "user", content = userMessage } };
            StartCoroutine(SendRequest(history, onComplete, onError));
        }

        /// <summary>Send a full multi-turn conversation. Each ClaudeMessage must have role="user" or "assistant".</summary>
        public void SendConversation(List<ClaudeMessage> history, Action<string> onComplete, Action<string> onError = null)
        {
            StartCoroutine(SendRequest(history, onComplete, onError));
        }

        // --------- Internals ---------

        private IEnumerator SendRequest(List<ClaudeMessage> history, Action<string> onComplete, Action<string> onError)
        {
            var payload = new ClaudeRequest
            {
                model = model,
                max_tokens = maxTokens,
                system = string.IsNullOrEmpty(systemPrompt) ? null : systemPrompt,
                messages = history
            };

            string json = JsonUtility.ToJson(payload);
            // JsonUtility can't skip null fields, so strip "system":null manually.
            if (payload.system == null)
                json = json.Replace(",\"system\":null", "").Replace("\"system\":null,", "");

            byte[] body = Encoding.UTF8.GetBytes(json);

            using (var req = new UnityWebRequest(API_URL, "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(body);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.SetRequestHeader("x-api-key", apiKey);
                req.SetRequestHeader("anthropic-version", API_VERSION);

                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    string msg = $"[ClaudeManager] HTTP {req.responseCode}: {req.error}\n{req.downloadHandler.text}";
                    Debug.LogError(msg);
                    onError?.Invoke(msg);
                    yield break;
                }

                try
                {
                    var resp = JsonUtility.FromJson<ClaudeResponse>(req.downloadHandler.text);
                    string text = (resp != null && resp.content != null && resp.content.Length > 0)
                        ? resp.content[0].text
                        : "";
                    onComplete?.Invoke(text);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ClaudeManager] Parse error: {ex.Message}\nRaw: {req.downloadHandler.text}");
                    onError?.Invoke(ex.Message);
                }
            }
        }

        // --------- DTOs (JsonUtility-compatible) ---------

        [Serializable]
        public class ClaudeMessage
        {
            public string role;    // "user" or "assistant"
            public string content; // plain text
        }

        [Serializable]
        private class ClaudeRequest
        {
            public string model;
            public int max_tokens;
            public string system;
            public List<ClaudeMessage> messages;
        }

        [Serializable]
        private class ClaudeResponse
        {
            public string id;
            public string role;
            public ContentBlock[] content;
            public string stop_reason;
            public Usage usage;
        }

        [Serializable]
        private class ContentBlock { public string type; public string text; }

        [Serializable]
        private class Usage { public int input_tokens; public int output_tokens; }
    }
}
