using System;
using UnityEngine;

namespace Matchmancer.Boosters
{
    /// <summary>
    /// MonoBehaviour bridge that owns the headless <see cref="BoosterInventory"/>,
    /// seeds it from inspector-assigned BoosterData assets, and exposes a
    /// TryUse entry point for the UI. Effects are dispatched as events so
    /// other systems (board, character, etc.) can listen without Boosters
    /// taking a compile-time dependency on them.
    /// </summary>
    [DisallowMultipleComponent]
    public class BoosterController : MonoBehaviour
    {
        [Header("Booster Catalog")]
        [SerializeField] private BoosterData[] boosterAssets;
        [SerializeField] private BoosterStartingStock[] startingStock;

        public BoosterInventory Inventory { get; private set; }

        /// <summary>
        /// Fired after a booster is successfully consumed via <see cref="TryUse"/>.
        /// Listeners (board, character, ultimate meter, etc.) react to the
        /// definition's Type/EffectMagnitude.
        /// </summary>
        public event Action<BoosterDefinition> OnBoosterApplied;

        // ---------- Lifecycle ----------

        private void Awake()
        {
            Inventory = new BoosterInventory();

            if (boosterAssets != null)
            {
                foreach (var asset in boosterAssets)
                {
                    if (asset == null) continue;
                    Inventory.RegisterDefinition(asset.ToDefinition());
                }
            }

            if (startingStock != null)
            {
                foreach (var stock in startingStock)
                {
                    if (stock == null || string.IsNullOrEmpty(stock.boosterId) || stock.amount <= 0) continue;
                    if (Inventory.HasDefinition(stock.boosterId))
                        Inventory.Add(stock.boosterId, stock.amount);
                }
            }
        }

        // ---------- Public API for UI ----------

        /// <summary>
        /// Attempt to use the named booster in the given context. Returns
        /// true on success and fires <see cref="OnBoosterApplied"/>.
        /// </summary>
        public bool TryUse(string boosterId, BoosterUseContext context)
        {
            if (Inventory == null) return false;
            if (!Inventory.CanUse(boosterId, context)) return false;

            var def = Inventory.GetDefinition(boosterId);
            if (def == null) return false;

            // Consume first so failures inside the listener can't double-spend.
            if (!Inventory.Use(boosterId, context)) return false;

            OnBoosterApplied?.Invoke(def);
            return true;
        }

        // ---------- Save/Restore plumbing for SaveService ----------

        public BoosterInventorySnapshot CreateSnapshot() => Inventory?.CreateSnapshot();
        public void LoadSnapshot(BoosterInventorySnapshot snapshot) => Inventory?.LoadFromSnapshot(snapshot);

        [Serializable]
        public class BoosterStartingStock
        {
            public string boosterId;
            public int amount;
        }
    }
}
