using System.Collections.Generic;
using UnityEngine;

namespace Matchmancer.Save
{
    /// <summary>
    /// In-memory storage backend used by EditMode tests and as a fallback
    /// during early scene boot. Round-trips through JsonUtility so tests
    /// catch serialization bugs the same way the file backend would.
    /// </summary>
    public class InMemorySaveStorage : ISaveStorage
    {
        private readonly Dictionary<string, string> _store =
            new Dictionary<string, string>();

        public int WriteCount  { get; private set; }
        public int DeleteCount { get; private set; }

        public bool Exists(string profileId) =>
            !string.IsNullOrEmpty(profileId) && _store.ContainsKey(profileId);

        public SaveData Load(string profileId)
        {
            if (string.IsNullOrEmpty(profileId)) return null;
            if (!_store.TryGetValue(profileId, out var json)) return null;
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                return JsonUtility.FromJson<SaveData>(json);
            }
            catch
            {
                return null;
            }
        }

        public void Save(string profileId, SaveData data)
        {
            if (string.IsNullOrEmpty(profileId) || data == null) return;
            _store[profileId] = JsonUtility.ToJson(data);
            WriteCount++;
        }

        public void Delete(string profileId)
        {
            if (string.IsNullOrEmpty(profileId)) return;
            if (_store.Remove(profileId)) DeleteCount++;
        }

        public void Clear()
        {
            _store.Clear();
        }
    }
}
