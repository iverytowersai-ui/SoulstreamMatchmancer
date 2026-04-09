using System;
using System.Collections.Generic;

namespace Matchmancer.Achievements
{
    /// <summary>
    /// Pure C# title collection + equipment tracker. Holds every title
    /// definition the game knows about, the set the player has unlocked,
    /// and which one is currently equipped.
    ///
    /// Wire it to an <see cref="AchievementTracker"/> via
    /// <see cref="WireToTracker"/> so achievement unlocks automatically
    /// grant their reward titles.
    ///
    /// Rules:
    ///   • A title must be registered before it can be unlocked.
    ///   • Unlocking is idempotent — second unlock is a no-op (no event).
    ///   • You can only equip a title you own.
    ///   • Equipping null / empty unequips the current title.
    ///   • Equipping the already-equipped title is a no-op.
    /// </summary>
    public class TitleSystem
    {
        private readonly Dictionary<string, TitleDefinition> _definitions =
            new Dictionary<string, TitleDefinition>();

        private readonly HashSet<string> _owned = new HashSet<string>();

        private string _equippedId;

        // Stored so we can unsubscribe cleanly if WireToTracker is called twice.
        private AchievementTracker _wiredTracker;
        private Action<AchievementDefinition> _trackerHandler;

        // ------------------------------------------------------------------
        // Events
        // ------------------------------------------------------------------

        public event Action<TitleDefinition> OnTitleUnlocked;
        public event Action<TitleDefinition> OnTitleEquipped;
        public event Action<TitleDefinition> OnTitleUnequipped;

        // ------------------------------------------------------------------
        // Definitions
        // ------------------------------------------------------------------

        public void RegisterTitle(TitleDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (string.IsNullOrEmpty(definition.Id))
                throw new ArgumentException("Title id must be non-empty.", nameof(definition));
            _definitions[definition.Id] = definition;
        }

        public void RegisterTitles(IEnumerable<TitleDefinition> definitions)
        {
            if (definitions == null) return;
            foreach (var d in definitions)
                if (d != null) RegisterTitle(d);
        }

        public bool HasDefinition(string id) =>
            !string.IsNullOrEmpty(id) && _definitions.ContainsKey(id);

        public TitleDefinition GetDefinition(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            _definitions.TryGetValue(id, out var def);
            return def;
        }

        public IReadOnlyDictionary<string, TitleDefinition> Definitions => _definitions;

        // ------------------------------------------------------------------
        // Ownership
        // ------------------------------------------------------------------

        /// <summary>
        /// Grant the player a title. Returns true if it was newly unlocked,
        /// false if already owned or unknown id.
        /// </summary>
        public bool UnlockTitle(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            if (!_definitions.TryGetValue(id, out var def)) return false;
            if (!_owned.Add(id)) return false;
            OnTitleUnlocked?.Invoke(def);
            return true;
        }

        public bool HasTitle(string id) =>
            !string.IsNullOrEmpty(id) && _owned.Contains(id);

        public IReadOnlyCollection<string> OwnedIds => _owned;
        public int OwnedCount => _owned.Count;

        // ------------------------------------------------------------------
        // Equipment
        // ------------------------------------------------------------------

        public string EquippedId => _equippedId;

        public TitleDefinition EquippedTitle =>
            string.IsNullOrEmpty(_equippedId) ? null : _definitions[_equippedId];

        /// <summary>
        /// Equip an owned title. Pass null or empty to unequip. Returns
        /// true if state changed. Throws if the id is non-empty but not owned.
        /// </summary>
        public bool Equip(string id)
        {
            // Unequip path
            if (string.IsNullOrEmpty(id))
            {
                if (_equippedId == null) return false;
                var prev = _definitions[_equippedId];
                _equippedId = null;
                OnTitleUnequipped?.Invoke(prev);
                return true;
            }

            if (!_owned.Contains(id))
                throw new InvalidOperationException(
                    $"Cannot equip title '{id}' — not owned.");

            if (_equippedId == id) return false; // already equipped

            // Swap out previous, swap in new
            if (_equippedId != null)
            {
                var prev = _definitions[_equippedId];
                _equippedId = null;
                OnTitleUnequipped?.Invoke(prev);
            }

            _equippedId = id;
            OnTitleEquipped?.Invoke(_definitions[id]);
            return true;
        }

        public void Unequip() => Equip(null);

        // ------------------------------------------------------------------
        // Achievement wiring
        // ------------------------------------------------------------------

        /// <summary>
        /// Subscribe to a tracker so achievement unlocks automatically
        /// grant matching titles. Calling twice unsubscribes from the
        /// previous tracker first.
        /// </summary>
        public void WireToTracker(AchievementTracker tracker)
        {
            if (_wiredTracker != null && _trackerHandler != null)
                _wiredTracker.OnAchievementUnlocked -= _trackerHandler;

            _wiredTracker  = tracker;
            _trackerHandler = HandleAchievementUnlocked;

            if (tracker != null)
                tracker.OnAchievementUnlocked += _trackerHandler;
        }

        private void HandleAchievementUnlocked(AchievementDefinition def)
        {
            if (def == null || string.IsNullOrEmpty(def.TitleIdReward)) return;
            UnlockTitle(def.TitleIdReward);
        }

        // ------------------------------------------------------------------
        // Snapshot / restore
        // ------------------------------------------------------------------

        public Snapshot CreateSnapshot()
        {
            var snap = new Snapshot
            {
                OwnedIds   = new string[_owned.Count],
                EquippedId = _equippedId,
            };
            int i = 0;
            foreach (var id in _owned) snap.OwnedIds[i++] = id;
            return snap;
        }

        public void LoadFromSnapshot(Snapshot snapshot)
        {
            if (snapshot == null) return;
            _owned.Clear();
            _equippedId = null;

            if (snapshot.OwnedIds != null)
                foreach (var id in snapshot.OwnedIds)
                    if (!string.IsNullOrEmpty(id)) _owned.Add(id);

            if (!string.IsNullOrEmpty(snapshot.EquippedId) && _owned.Contains(snapshot.EquippedId))
                _equippedId = snapshot.EquippedId;
        }

        [Serializable]
        public class Snapshot
        {
            public string[] OwnedIds;
            public string   EquippedId;
        }
    }
}
