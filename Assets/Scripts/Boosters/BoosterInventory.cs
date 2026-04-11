using System;
using System.Collections.Generic;

namespace Matchmancer.Boosters
{
    /// <summary>
    /// Pure C# booster inventory: registers definitions, tracks counts,
    /// validates use-context, fires events, snapshots for save system.
    /// No UnityEngine references — fully headless-testable.
    /// </summary>
    public class BoosterInventory
    {
        private readonly Dictionary<string, BoosterDefinition> _definitions =
            new Dictionary<string, BoosterDefinition>();
        private readonly Dictionary<string, int> _counts =
            new Dictionary<string, int>();

        /// <summary>Total uses across the lifetime of this save.</summary>
        public long LifetimeUsed { get; private set; }

        /// <summary>Total purchases across the lifetime of this save.</summary>
        public long LifetimePurchased { get; private set; }

        // Events
        public event Action<BoosterDefinition, int, int> OnCountChanged;   // def, oldCount, newCount
        public event Action<BoosterDefinition, int>      OnBoosterAdded;   // def, amountAdded
        public event Action<BoosterDefinition, int>      OnBoosterUsed;    // def, amountUsed
        public event Action<BoosterDefinition>           OnBoosterRegistered;

        public IReadOnlyDictionary<string, BoosterDefinition> Definitions => _definitions;

        // ---------- Registration ----------

        public void RegisterDefinition(BoosterDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (string.IsNullOrEmpty(definition.Id))
                throw new ArgumentException("BoosterDefinition.Id cannot be null/empty", nameof(definition));

            _definitions[definition.Id] = definition;
            if (!_counts.ContainsKey(definition.Id))
                _counts[definition.Id] = 0;

            OnBoosterRegistered?.Invoke(definition);
        }

        public bool HasDefinition(string id) =>
            !string.IsNullOrEmpty(id) && _definitions.ContainsKey(id);

        public BoosterDefinition GetDefinition(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            _definitions.TryGetValue(id, out var def);
            return def;
        }

        // ---------- Counts ----------

        public int GetCount(string id)
        {
            if (string.IsNullOrEmpty(id)) return 0;
            return _counts.TryGetValue(id, out var c) ? c : 0;
        }

        public int GetCount(BoosterType type)
        {
            int total = 0;
            foreach (var kv in _definitions)
                if (kv.Value.Type == type)
                    total += GetCount(kv.Key);
            return total;
        }

        /// <summary>
        /// Add boosters to the stack. Respects MaxStack (0/negative = unlimited).
        /// Returns the number actually added (may be less if cap reached).
        /// </summary>
        public int Add(string id, int amount)
        {
            if (amount <= 0) return 0;
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Booster id cannot be null/empty", nameof(id));
            if (!_definitions.TryGetValue(id, out var def))
                throw new InvalidOperationException($"Unknown booster id '{id}' — register before adding.");

            int oldCount = GetCount(id);
            int newCount = oldCount + amount;
            if (def.MaxStack > 0 && newCount > def.MaxStack)
                newCount = def.MaxStack;

            int added = newCount - oldCount;
            if (added <= 0) return 0;

            _counts[id] = newCount;
            OnCountChanged?.Invoke(def, oldCount, newCount);
            OnBoosterAdded?.Invoke(def, added);
            return added;
        }

        /// <summary>
        /// Record a shop purchase. Increments LifetimePurchased and adds to inventory.
        /// </summary>
        public int Purchase(string id, int amount = 1)
        {
            int added = Add(id, amount);
            if (added > 0) LifetimePurchased += added;
            return added;
        }

        public bool CanUse(string id, BoosterUseContext context)
        {
            if (string.IsNullOrEmpty(id)) return false;
            if (!_definitions.TryGetValue(id, out var def)) return false;
            if (GetCount(id) <= 0) return false;
            if (def.UseContext == BoosterUseContext.Anywhere) return true;
            return def.UseContext == context;
        }

        /// <summary>
        /// Consume one booster of the given id. Returns true on success.
        /// Validates the use context — using an InBattle booster outside battle returns false.
        /// </summary>
        public bool Use(string id, BoosterUseContext context)
        {
            if (!CanUse(id, context)) return false;

            var def = _definitions[id];
            int oldCount = _counts[id];
            int newCount = oldCount - 1;
            _counts[id] = newCount;
            LifetimeUsed += 1;

            OnCountChanged?.Invoke(def, oldCount, newCount);
            OnBoosterUsed?.Invoke(def, 1);
            return true;
        }

        /// <summary>
        /// Cheat / debug / save-load: forcibly set the count without firing
        /// OnBoosterAdded/Used. OnCountChanged still fires so UI updates.
        /// </summary>
        public void SetCountRaw(string id, int count)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (!_definitions.TryGetValue(id, out var def)) return;
            if (count < 0) count = 0;
            if (def.MaxStack > 0 && count > def.MaxStack) count = def.MaxStack;

            int oldCount = GetCount(id);
            if (oldCount == count) return;
            _counts[id] = count;
            OnCountChanged?.Invoke(def, oldCount, count);
        }

        public void ClearAll()
        {
            var keys = new List<string>(_counts.Keys);
            foreach (var k in keys) SetCountRaw(k, 0);
        }

        // ---------- Snapshot / Restore ----------

        public BoosterInventorySnapshot CreateSnapshot()
        {
            var ids    = new List<string>(_counts.Count);
            var counts = new List<int>(_counts.Count);
            foreach (var kv in _counts)
            {
                if (kv.Value <= 0) continue;
                ids.Add(kv.Key);
                counts.Add(kv.Value);
            }
            return new BoosterInventorySnapshot
            {
                BoosterIds        = ids.ToArray(),
                Counts            = counts.ToArray(),
                LifetimeUsed      = LifetimeUsed,
                LifetimePurchased = LifetimePurchased,
            };
        }

        public void LoadFromSnapshot(BoosterInventorySnapshot snapshot)
        {
            if (snapshot == null) return;

            // Reset all current counts (silent)
            var keys = new List<string>(_counts.Keys);
            foreach (var k in keys) _counts[k] = 0;

            LifetimeUsed      = snapshot.LifetimeUsed;
            LifetimePurchased = snapshot.LifetimePurchased;

            if (snapshot.BoosterIds == null || snapshot.Counts == null) return;
            int n = Math.Min(snapshot.BoosterIds.Length, snapshot.Counts.Length);
            for (int i = 0; i < n; i++)
            {
                string id = snapshot.BoosterIds[i];
                int count = snapshot.Counts[i];
                if (string.IsNullOrEmpty(id)) continue;
                if (!_definitions.ContainsKey(id)) continue; // dropped if def removed
                SetCountRaw(id, count);
            }
        }
    }
}
