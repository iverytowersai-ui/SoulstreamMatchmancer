using System;

namespace Matchmancer.Character
{
    /// <summary>
    /// Plain-data save record for <see cref="GearInventory"/>. Stores owned
    /// items by tuning id + per-instance id, plus equipped slot map indexed
    /// by <c>(int)GearSlot</c>. JsonUtility-friendly.
    /// </summary>
    [Serializable]
    public class GearInventorySnapshot
    {
        /// <summary>Tuning ids of all owned items (looked up via catalog on load).</summary>
        public string[] OwnedTuningIds;

        /// <summary>Stable per-instance ids (parallel to OwnedTuningIds).</summary>
        public string[] OwnedInstanceIds;

        /// <summary>Equipped instance id per slot — index = (int)GearSlot.</summary>
        public string[] EquippedInstanceIds;
    }
}
