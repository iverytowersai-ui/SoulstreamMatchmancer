using System;

namespace Matchmancer.Character
{
    /// <summary>
    /// Pure C# definition of a single piece of gear. Lives in the
    /// <c>Matchmancer.Runtime</c> assembly so headless tests can build gear
    /// without a ScriptableObject. <see cref="GearData"/> is the Unity-facing
    /// wrapper that projects to this via <c>ToTuning()</c>.
    /// </summary>
    [Serializable]
    public class GearTuning
    {
        /// <summary>Stable id for save data (e.g. "gear_rust_blade"). Never rename.</summary>
        public string Id;

        /// <summary>Player-facing name.</summary>
        public string DisplayName;

        /// <summary>Short flavor line shown under the name.</summary>
        public string FlavorText;

        public GearSlot    Slot;
        public GearRarity  Rarity;

        /// <summary>Stat bonuses this piece grants while equipped.</summary>
        public GearStatModifiers Modifiers;

        /// <summary>Optional gold sell price (Skill 22 will use this).</summary>
        public int SellPrice;

        public GearTuning()
        {
            Id          = string.Empty;
            DisplayName = string.Empty;
            FlavorText  = string.Empty;
            Slot        = GearSlot.Weapon;
            Rarity      = GearRarity.Common;
            Modifiers   = GearStatModifiers.Zero;
            SellPrice   = 0;
        }

        /// <summary>Deep copy — safe to mutate without touching the source.</summary>
        public GearTuning Clone()
        {
            return new GearTuning
            {
                Id          = Id,
                DisplayName = DisplayName,
                FlavorText  = FlavorText,
                Slot        = Slot,
                Rarity      = Rarity,
                Modifiers   = Modifiers, // struct — copied by value
                SellPrice   = SellPrice,
            };
        }
    }
}
