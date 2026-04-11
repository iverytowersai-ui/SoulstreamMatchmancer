using System;

namespace Matchmancer.Boosters
{
    /// <summary>
    /// Plain-data save/load representation for <see cref="BoosterInventory"/>.
    /// Parallel arrays so it survives JsonUtility / Unity serialization.
    /// </summary>
    [Serializable]
    public class BoosterInventorySnapshot
    {
        public string[] BoosterIds;
        public int[]    Counts;
        public long     LifetimeUsed;
        public long     LifetimePurchased;

        public bool IsEmpty =>
            (BoosterIds == null || BoosterIds.Length == 0) &&
            LifetimeUsed == 0 &&
            LifetimePurchased == 0;
    }
}
