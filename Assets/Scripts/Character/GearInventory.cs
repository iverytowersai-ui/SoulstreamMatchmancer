using System;
using System.Collections.Generic;

namespace Matchmancer.Character
{
    /// <summary>
    /// Pure C# gear collection + slot equipment tracker. Lives outside
    /// Unity so tests drive it directly. Wire it to a
    /// <see cref="CharacterRuntime"/> by subscribing to
    /// <see cref="OnModifiersChanged"/> and calling
    /// <c>CharacterRuntime.SetGearModifiers(inv.TotalEquippedModifiers)</c>.
    ///
    /// Rules:
    ///   • An owned item can be equipped or unequipped any time out of battle.
    ///   • Equipping an item while another one is already in that slot
    ///     automatically unequips the previous one (no swap failure path).
    ///   • You cannot equip an item you don't own.
    ///   • Removing an owned item auto-unequips it first.
    /// </summary>
    public class GearInventory
    {
        // Owned items keyed by instanceId for O(1) lookup.
        private readonly Dictionary<string, GearItem> _owned = new Dictionary<string, GearItem>();

        // Slot -> currently equipped instanceId (or null for empty).
        private readonly Dictionary<GearSlot, string> _equipped = new Dictionary<GearSlot, string>();

        public IReadOnlyDictionary<string, GearItem> OwnedItems => _owned;
        public int OwnedCount => _owned.Count;

        // ------------------------------------------------------------------
        // Events
        // ------------------------------------------------------------------

        /// <summary>Item added to the bag.</summary>
        public event Action<GearItem> OnItemAdded;
        /// <summary>Item removed from the bag (unequipped first if needed).</summary>
        public event Action<GearItem> OnItemRemoved;
        /// <summary>Item was equipped into a slot.</summary>
        public event Action<GearSlot, GearItem> OnItemEquipped;
        /// <summary>Item was unequipped from a slot.</summary>
        public event Action<GearSlot, GearItem> OnItemUnequipped;
        /// <summary>
        /// Fired after any change that alters <see cref="TotalEquippedModifiers"/>.
        /// Subscribe here to push the new totals into a
        /// <see cref="CharacterRuntime"/>.
        /// </summary>
        public event Action<GearStatModifiers> OnModifiersChanged;

        public GearInventory()
        {
            foreach (GearSlot slot in Enum.GetValues(typeof(GearSlot)))
                _equipped[slot] = null;
        }

        // ------------------------------------------------------------------
        // Ownership
        // ------------------------------------------------------------------

        /// <summary>Add a new item to the bag. Does not equip it.</summary>
        public void AddItem(GearItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (_owned.ContainsKey(item.InstanceId))
                throw new InvalidOperationException(
                    $"Gear item '{item.InstanceId}' is already owned.");

            _owned[item.InstanceId] = item;
            OnItemAdded?.Invoke(item);
        }

        /// <summary>
        /// Remove an item from the bag. If it is equipped, it is
        /// unequipped first (which fires the modifier event once). Returns
        /// <c>false</c> if the instance is unknown.
        /// </summary>
        public bool RemoveItem(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId)) return false;
            if (!_owned.TryGetValue(instanceId, out var item)) return false;

            // Unequip first if needed
            if (_equipped[item.Slot] == instanceId)
                Unequip(item.Slot);

            _owned.Remove(instanceId);
            OnItemRemoved?.Invoke(item);
            return true;
        }

        public bool Owns(string instanceId) =>
            !string.IsNullOrEmpty(instanceId) && _owned.ContainsKey(instanceId);

        public GearItem GetItem(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId)) return null;
            _owned.TryGetValue(instanceId, out var item);
            return item;
        }

        // ------------------------------------------------------------------
        // Equip / unequip
        // ------------------------------------------------------------------

        /// <summary>
        /// Equip an owned item into its slot. Any item currently in that
        /// slot is unequipped first (still owned, just removed from the
        /// slot). Throws if the item is not owned.
        /// </summary>
        public void Equip(string instanceId)
        {
            if (!_owned.TryGetValue(instanceId, out var item))
                throw new InvalidOperationException(
                    $"Cannot equip '{instanceId}' — item not owned.");

            GearSlot slot = item.Slot;

            // Already equipped? No-op.
            if (_equipped[slot] == instanceId) return;

            // Kick out whatever's in the slot.
            if (_equipped[slot] != null)
            {
                GearItem previous = _owned[_equipped[slot]];
                _equipped[slot] = null;
                OnItemUnequipped?.Invoke(slot, previous);
            }

            _equipped[slot] = instanceId;
            OnItemEquipped?.Invoke(slot, item);
            OnModifiersChanged?.Invoke(TotalEquippedModifiers);
        }

        /// <summary>
        /// Unequip whatever is in the given slot. No-op if the slot is
        /// empty. Returns the item that was removed, or null.
        /// </summary>
        public GearItem Unequip(GearSlot slot)
        {
            string id = _equipped[slot];
            if (id == null) return null;

            GearItem item = _owned[id];
            _equipped[slot] = null;
            OnItemUnequipped?.Invoke(slot, item);
            OnModifiersChanged?.Invoke(TotalEquippedModifiers);
            return item;
        }

        public GearItem GetEquipped(GearSlot slot)
        {
            string id = _equipped[slot];
            if (id == null) return null;
            return _owned[id];
        }

        public bool IsEquipped(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId)) return false;
            foreach (var kv in _equipped)
                if (kv.Value == instanceId) return true;
            return false;
        }

        // ------------------------------------------------------------------
        // Stat totals
        // ------------------------------------------------------------------

        /// <summary>
        /// Sum of <see cref="GearStatModifiers"/> across every equipped slot.
        /// This is the single value a <see cref="CharacterRuntime"/> needs.
        /// </summary>
        public GearStatModifiers TotalEquippedModifiers
        {
            get
            {
                GearStatModifiers total = GearStatModifiers.Zero;
                foreach (var kv in _equipped)
                {
                    if (kv.Value == null) continue;
                    total += _owned[kv.Value].Modifiers;
                }
                return total;
            }
        }

        /// <summary>
        /// Unequip everything. Fires one <see cref="OnModifiersChanged"/>
        /// at the end. Useful for tests and for full gear-rebuild flows.
        /// </summary>
        public void UnequipAll()
        {
            bool any = false;
            foreach (var slot in new List<GearSlot>(_equipped.Keys))
            {
                if (_equipped[slot] == null) continue;
                GearItem item = _owned[_equipped[slot]];
                _equipped[slot] = null;
                OnItemUnequipped?.Invoke(slot, item);
                any = true;
            }
            if (any) OnModifiersChanged?.Invoke(TotalEquippedModifiers);
        }
    }
}
