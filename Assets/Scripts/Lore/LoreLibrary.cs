using System;
using System.Collections.Generic;

namespace Matchmancer.Lore
{
    /// <summary>
    /// Pure C# registry of all lore entries + unlocked tracking. No Unity —
    /// fully testable. <see cref="LoreController"/> is the Mono bridge.
    ///
    /// Unlock flow:
    ///   • All entries start locked.
    ///   • Call <see cref="Unlock"/> when a condition is met (progression
    ///     event, achievement unlock, level clear, etc.).
    ///   • <see cref="UnlockAll"/> available for dev/debug.
    ///   • Secret entries show as "???" in the gallery when locked.
    ///   • Non-secret entries show their title but grey-out the body.
    /// </summary>
    public class LoreLibrary
    {
        private readonly Dictionary<string, LoreEntry> _entries =
            new Dictionary<string, LoreEntry>();
        private readonly HashSet<string> _unlocked = new HashSet<string>();

        // ------------------------------------------------------------------
        // Events
        // ------------------------------------------------------------------

        /// <summary>Fires when an entry is registered (for UI rebuild).</summary>
        public event Action<LoreEntry> OnEntryRegistered;

        /// <summary>Fires when a previously-locked entry becomes readable.</summary>
        public event Action<LoreEntry> OnEntryUnlocked;

        // ------------------------------------------------------------------
        // Registration
        // ------------------------------------------------------------------

        public void Register(LoreEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (string.IsNullOrEmpty(entry.Id))
                throw new ArgumentException("LoreEntry.Id cannot be null/empty", nameof(entry));

            _entries[entry.Id] = entry;
            OnEntryRegistered?.Invoke(entry);
        }

        public void RegisterMany(IEnumerable<LoreEntry> entries)
        {
            if (entries == null) return;
            foreach (var e in entries) Register(e);
        }

        // ------------------------------------------------------------------
        // Queries
        // ------------------------------------------------------------------

        public LoreEntry Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            _entries.TryGetValue(id, out var entry);
            return entry;
        }

        public bool Has(string id) =>
            !string.IsNullOrEmpty(id) && _entries.ContainsKey(id);

        public bool IsUnlocked(string id) =>
            !string.IsNullOrEmpty(id) && _unlocked.Contains(id);

        public int TotalEntries => _entries.Count;
        public int UnlockedCount => _unlocked.Count;

        /// <summary>
        /// Return all entries in a given category, sorted by SortOrder.
        /// Includes locked entries (UI shows them as locked).
        /// </summary>
        public List<LoreEntry> GetByCategory(LoreCategory category)
        {
            var result = new List<LoreEntry>();
            foreach (var e in _entries.Values)
                if (e.Category == category) result.Add(e);
            result.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
            return result;
        }

        /// <summary>Return only unlocked entries in the given category.</summary>
        public List<LoreEntry> GetUnlockedByCategory(LoreCategory category)
        {
            var result = new List<LoreEntry>();
            foreach (var e in _entries.Values)
                if (e.Category == category && _unlocked.Contains(e.Id))
                    result.Add(e);
            result.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
            return result;
        }

        // ------------------------------------------------------------------
        // Unlock
        // ------------------------------------------------------------------

        /// <summary>
        /// Unlock an entry by id. Returns true if the entry was newly
        /// unlocked; false if already unlocked or unknown.
        /// </summary>
        public bool Unlock(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            if (!_entries.TryGetValue(id, out var entry)) return false;
            if (!_unlocked.Add(id)) return false;

            OnEntryUnlocked?.Invoke(entry);
            return true;
        }

        /// <summary>Unlock everything — dev/debug shortcut.</summary>
        public void UnlockAll()
        {
            foreach (var id in _entries.Keys)
            {
                if (_unlocked.Add(id))
                    OnEntryUnlocked?.Invoke(_entries[id]);
            }
        }

        // ------------------------------------------------------------------
        // Snapshot / Restore
        // ------------------------------------------------------------------

        public LoreSnapshot CreateSnapshot()
        {
            var ids = new string[_unlocked.Count];
            _unlocked.CopyTo(ids);
            return new LoreSnapshot { UnlockedIds = ids };
        }

        public void LoadFromSnapshot(LoreSnapshot snapshot)
        {
            if (snapshot == null) return;
            _unlocked.Clear();
            if (snapshot.UnlockedIds == null) return;
            foreach (var id in snapshot.UnlockedIds)
            {
                if (!string.IsNullOrEmpty(id) && _entries.ContainsKey(id))
                    _unlocked.Add(id);
            }
        }
    }

    /// <summary>Save data for LoreLibrary — just the set of unlocked ids.</summary>
    [Serializable]
    public class LoreSnapshot
    {
        public string[] UnlockedIds;
    }
}
