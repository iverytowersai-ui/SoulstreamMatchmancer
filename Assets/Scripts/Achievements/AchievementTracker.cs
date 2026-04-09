using System;
using System.Collections.Generic;

namespace Matchmancer.Achievements
{
    /// <summary>
    /// Pure C# cumulative-stat tracker + achievement unlock engine. Lives
    /// outside Unity so headless tests drive it directly.
    ///
    /// Two stat update modes:
    ///   • <see cref="IncrementStat"/>   — adds to a cumulative counter
    ///     (e.g. EnemiesDefeated, TotalDamageDealt).
    ///   • <see cref="UpdateMaxStat"/>   — only sets if the new value is
    ///     greater (e.g. MaxComboEver, BiggestHitEver).
    ///
    /// Achievement unlock is automatic and idempotent: every stat write
    /// re-checks the watching achievements; each definition fires
    /// <see cref="OnAchievementUnlocked"/> at most once.
    ///
    /// Snapshot/restore goes through <see cref="CreateSnapshot"/> /
    /// <see cref="LoadFromSnapshot"/> for the save system (Skill 20).
    /// </summary>
    public class AchievementTracker
    {
        private readonly Dictionary<AchievementStatKey, long> _stats =
            new Dictionary<AchievementStatKey, long>();

        private readonly Dictionary<string, AchievementDefinition> _definitions =
            new Dictionary<string, AchievementDefinition>();

        private readonly HashSet<string> _unlocked = new HashSet<string>();

        // ------------------------------------------------------------------
        // Events
        // ------------------------------------------------------------------

        /// <summary>Fired whenever a stat value changes. (key, newValue)</summary>
        public event Action<AchievementStatKey, long> OnStatChanged;

        /// <summary>Fired exactly once per achievement when it crosses its threshold.</summary>
        public event Action<AchievementDefinition> OnAchievementUnlocked;

        // ------------------------------------------------------------------
        // Definitions
        // ------------------------------------------------------------------

        /// <summary>
        /// Register an achievement so the tracker watches it. Re-registering
        /// the same id replaces the previous definition (handy for hot-reload).
        /// Throws on null or empty id.
        /// </summary>
        public void RegisterAchievement(AchievementDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (string.IsNullOrEmpty(definition.Id))
                throw new ArgumentException("Achievement id must be non-empty.", nameof(definition));

            _definitions[definition.Id] = definition;

            // If the new threshold is already met, unlock immediately.
            if (!_unlocked.Contains(definition.Id) &&
                GetStat(definition.StatKey) >= definition.TargetValue)
            {
                Unlock(definition);
            }
        }

        public void RegisterAchievements(IEnumerable<AchievementDefinition> definitions)
        {
            if (definitions == null) return;
            foreach (var d in definitions)
                if (d != null) RegisterAchievement(d);
        }

        public bool HasDefinition(string id) =>
            !string.IsNullOrEmpty(id) && _definitions.ContainsKey(id);

        public AchievementDefinition GetDefinition(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            _definitions.TryGetValue(id, out var def);
            return def;
        }

        public IReadOnlyDictionary<string, AchievementDefinition> Definitions => _definitions;

        // ------------------------------------------------------------------
        // Stat reads / writes
        // ------------------------------------------------------------------

        public long GetStat(AchievementStatKey key)
        {
            _stats.TryGetValue(key, out long v);
            return v;
        }

        /// <summary>
        /// Add to a cumulative counter. Negative amounts are clamped to 0
        /// to keep counters monotonic. No-op when amount is 0.
        /// </summary>
        public void IncrementStat(AchievementStatKey key, long amount = 1)
        {
            if (amount <= 0) return;
            long current = GetStat(key);
            long next = current + amount;
            if (next < current) next = long.MaxValue; // overflow guard
            _stats[key] = next;
            OnStatChanged?.Invoke(key, next);
            CheckAchievementsForKey(key);
        }

        /// <summary>
        /// Set a high-water stat. Only mutates if the new value is strictly
        /// greater than the current value. Use this for "max combo ever",
        /// "biggest hit", "highest stage reached".
        /// </summary>
        public void UpdateMaxStat(AchievementStatKey key, long value)
        {
            long current = GetStat(key);
            if (value <= current) return;
            _stats[key] = value;
            OnStatChanged?.Invoke(key, value);
            CheckAchievementsForKey(key);
        }

        /// <summary>
        /// Direct overwrite — only used by save/load. Skips event firing
        /// and achievement checks; call <see cref="ReevaluateAll"/> after
        /// a bulk load if you need re-checks.
        /// </summary>
        public void SetStatRaw(AchievementStatKey key, long value)
        {
            _stats[key] = value;
        }

        // ------------------------------------------------------------------
        // Unlock state
        // ------------------------------------------------------------------

        public bool IsUnlocked(string id) =>
            !string.IsNullOrEmpty(id) && _unlocked.Contains(id);

        public int UnlockedCount => _unlocked.Count;

        public IReadOnlyCollection<string> UnlockedIds => _unlocked;

        /// <summary>
        /// Re-check every registered achievement against current stats.
        /// Used after bulk save loads. Idempotent — already-unlocked
        /// achievements stay unlocked but do not fire again.
        /// </summary>
        public void ReevaluateAll()
        {
            foreach (var def in _definitions.Values)
            {
                if (_unlocked.Contains(def.Id)) continue;
                if (GetStat(def.StatKey) >= def.TargetValue)
                    Unlock(def);
            }
        }

        // ------------------------------------------------------------------
        // Internal
        // ------------------------------------------------------------------

        private void CheckAchievementsForKey(AchievementStatKey key)
        {
            // Local copy to avoid "collection modified" if a listener
            // re-registers definitions while iterating.
            List<AchievementDefinition> toUnlock = null;
            foreach (var def in _definitions.Values)
            {
                if (def.StatKey != key) continue;
                if (_unlocked.Contains(def.Id)) continue;
                if (GetStat(key) >= def.TargetValue)
                {
                    if (toUnlock == null) toUnlock = new List<AchievementDefinition>();
                    toUnlock.Add(def);
                }
            }
            if (toUnlock == null) return;
            foreach (var def in toUnlock) Unlock(def);
        }

        private void Unlock(AchievementDefinition def)
        {
            if (_unlocked.Add(def.Id))
                OnAchievementUnlocked?.Invoke(def);
        }

        // ------------------------------------------------------------------
        // Snapshot / restore
        // ------------------------------------------------------------------

        public Snapshot CreateSnapshot()
        {
            var snap = new Snapshot
            {
                StatKeys     = new AchievementStatKey[_stats.Count],
                StatValues   = new long[_stats.Count],
                UnlockedIds  = new string[_unlocked.Count],
            };
            int i = 0;
            foreach (var kv in _stats)
            {
                snap.StatKeys[i]   = kv.Key;
                snap.StatValues[i] = kv.Value;
                i++;
            }
            int j = 0;
            foreach (var id in _unlocked) snap.UnlockedIds[j++] = id;
            return snap;
        }

        /// <summary>
        /// Replace internal state from a snapshot. Does NOT re-fire
        /// OnAchievementUnlocked for previously-unlocked achievements.
        /// Call <see cref="ReevaluateAll"/> after if you want missed
        /// thresholds re-checked.
        /// </summary>
        public void LoadFromSnapshot(Snapshot snapshot)
        {
            if (snapshot == null) return;
            _stats.Clear();
            _unlocked.Clear();

            if (snapshot.StatKeys != null && snapshot.StatValues != null)
            {
                int n = Math.Min(snapshot.StatKeys.Length, snapshot.StatValues.Length);
                for (int i = 0; i < n; i++)
                    _stats[snapshot.StatKeys[i]] = snapshot.StatValues[i];
            }

            if (snapshot.UnlockedIds != null)
            {
                foreach (var id in snapshot.UnlockedIds)
                    if (!string.IsNullOrEmpty(id)) _unlocked.Add(id);
            }
        }

        [Serializable]
        public class Snapshot
        {
            public AchievementStatKey[] StatKeys;
            public long[]               StatValues;
            public string[]             UnlockedIds;
        }
    }
}
