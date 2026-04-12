using UnityEngine;

namespace Matchmancer.Lore
{
    /// <summary>
    /// MonoBehaviour bridge that owns a <see cref="LoreLibrary"/>, seeds it
    /// from Inspector-assigned <see cref="LoreEntryData"/> assets, and
    /// exposes the API for the Lore Gallery UI panel.
    /// </summary>
    [DisallowMultipleComponent]
    public class LoreController : MonoBehaviour
    {
        [Header("Catalog")]
        [Tooltip("Every lore entry in the game. Order doesn't matter — " +
                 "SortOrder on each entry drives the in-gallery ordering.")]
        [SerializeField] private LoreEntryData[] catalog;

        public LoreLibrary Library { get; private set; }

        private void Awake()
        {
            Library = new LoreLibrary();

            if (catalog == null) return;
            foreach (var data in catalog)
            {
                if (data == null) continue;
                Library.Register(data.ToDefinition());
            }
        }

        // ------------------------------------------------------------------
        // Save / Restore wiring for SaveService
        // ------------------------------------------------------------------

        public LoreSnapshot CreateSnapshot() => Library?.CreateSnapshot();

        public void LoadSnapshot(LoreSnapshot snapshot)
        {
            Library?.LoadFromSnapshot(snapshot);
        }
    }
}
