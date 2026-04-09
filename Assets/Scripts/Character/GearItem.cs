using System;

namespace Matchmancer.Character
{
    /// <summary>
    /// Runtime instance of a piece of gear. Wraps a <see cref="GearTuning"/>
    /// plus a unique <see cref="InstanceId"/> so the inventory can hold
    /// multiple copies of the same definition and still tell them apart.
    ///
    /// Future: Skill 15.5 may add rolled sub-stats. That layer would live on
    /// the item, not the tuning, so the base definition stays immutable.
    /// </summary>
    public class GearItem
    {
        public string InstanceId { get; }
        public GearTuning Tuning { get; }

        public string Id          => Tuning.Id;
        public string DisplayName => Tuning.DisplayName;
        public GearSlot   Slot    => Tuning.Slot;
        public GearRarity Rarity  => Tuning.Rarity;
        public GearStatModifiers Modifiers => Tuning.Modifiers;

        public GearItem(GearTuning tuning, string instanceId = null)
        {
            Tuning     = tuning ?? throw new ArgumentNullException(nameof(tuning));
            InstanceId = string.IsNullOrEmpty(instanceId)
                ? Guid.NewGuid().ToString("N")
                : instanceId;
        }

        public override string ToString() => $"{DisplayName} ({Slot}/{Rarity}) [{InstanceId}]";
    }
}
