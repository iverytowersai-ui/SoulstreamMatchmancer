using System.Collections.Generic;
using UnityEngine;

namespace Matchmancer.Character
{
    /// <summary>
    /// Scene-side bridge between a <see cref="GearInventory"/> and a
    /// <see cref="CharacterBattleController"/>. One MonoBehaviour sits on the
    /// character / hub object, owns the pure-C# inventory, seeds it from
    /// Inspector-assigned <see cref="GearData"/> assets, and pushes total
    /// modifiers into the live <see cref="CharacterRuntime"/> whenever the
    /// loadout changes.
    ///
    /// Persistence of owned/equipped items is NOT implemented here — that
    /// lives in Skill 20 (Save System). This component only bridges runtime
    /// state to the battle character.
    /// </summary>
    [DisallowMultipleComponent]
    public class GearInventoryController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterBattleController characterBattleController;

        [Header("Starter Loadout")]
        [Tooltip("Items added to the inventory on Awake. First item per slot " +
                 "is auto-equipped.")]
        [SerializeField] private GearData[] starterItems;

        public GearInventory Inventory { get; private set; }

        // Map instanceId -> sourceData so the UI can re-resolve sprites etc.
        private readonly Dictionary<string, GearData> _source = new Dictionary<string, GearData>();

        // ------------------------------------------------------------------
        // Lifecycle
        // ------------------------------------------------------------------

        private void Awake()
        {
            Inventory = new GearInventory();
            Inventory.OnModifiersChanged += HandleModifiersChanged;

            if (starterItems == null) return;

            var seenSlots = new HashSet<GearSlot>();
            foreach (var data in starterItems)
            {
                if (data == null) continue;
                var item = data.CreateItem();
                _source[item.InstanceId] = data;
                Inventory.AddItem(item);

                if (!seenSlots.Contains(item.Slot))
                {
                    Inventory.Equip(item.InstanceId);
                    seenSlots.Add(item.Slot);
                }
            }
        }

        private void OnDestroy()
        {
            if (Inventory != null)
                Inventory.OnModifiersChanged -= HandleModifiersChanged;
        }

        // ------------------------------------------------------------------
        // Wiring
        // ------------------------------------------------------------------

        private void HandleModifiersChanged(GearStatModifiers mods)
        {
            if (characterBattleController == null)
            {
                Debug.LogWarning($"[{nameof(GearInventoryController)}] No CharacterBattleController assigned; gear bonuses will not apply.", this);
                return;
            }

            var runtime = characterBattleController.Runtime;
            if (runtime == null)
            {
                // Runtime may not be built yet (OnEnable order). Defer.
                return;
            }

            runtime.SetGearModifiers(mods);
        }

        /// <summary>
        /// Re-apply current inventory modifiers to whatever runtime the
        /// battle controller holds right now. Call this after
        /// <c>CharacterBattleController.RebuildRuntime()</c> so the new
        /// runtime picks up gear bonuses.
        /// </summary>
        public void ReapplyToCurrentRuntime()
        {
            var runtime = characterBattleController?.Runtime;
            if (runtime == null || Inventory == null) return;
            runtime.SetGearModifiers(Inventory.TotalEquippedModifiers);
        }

        /// <summary>Look up the source asset for an owned item, if any.</summary>
        public GearData GetSource(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId)) return null;
            _source.TryGetValue(instanceId, out var data);
            return data;
        }

        /// <summary>
        /// Resolve a tuning by id from the starter catalog plus anything seen
        /// since boot. Used by SaveService to rebuild gear on load. For full
        /// game coverage, also assign all GearData assets to <c>starterItems</c>
        /// (or expand this to a dedicated catalog field in a future skill).
        /// </summary>
        public GearTuning ResolveTuning(string tuningId)
        {
            if (string.IsNullOrEmpty(tuningId)) return null;

            if (starterItems != null)
            {
                foreach (var data in starterItems)
                {
                    if (data == null) continue;
                    var tuning = data.ToTuning();
                    if (tuning != null && tuning.Id == tuningId) return tuning;
                }
            }

            foreach (var data in _source.Values)
            {
                if (data == null) continue;
                var tuning = data.ToTuning();
                if (tuning != null && tuning.Id == tuningId) return tuning;
            }
            return null;
        }
    }
}
