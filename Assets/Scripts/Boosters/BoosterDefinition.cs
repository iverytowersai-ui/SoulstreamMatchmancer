using System;

namespace Matchmancer.Boosters
{
    /// <summary>
    /// Pure C# definition of a booster — no UnityEngine references so it
    /// stays headlessly testable. <see cref="BoosterData"/> wraps this for
    /// inspector-side authoring.
    /// </summary>
    [Serializable]
    public class BoosterDefinition
    {
        public string Id;
        public string DisplayName;
        public string Description;

        public BoosterType Type;
        public BoosterUseContext UseContext;

        /// <summary>Magnitude of the effect (extra moves count, heal amount, bomb radius, etc.).</summary>
        public int EffectMagnitude;

        /// <summary>Optional secondary value (duration, AoE radius, etc.).</summary>
        public int SecondaryValue;

        /// <summary>Maximum stack count in inventory. 0 or negative = unlimited.</summary>
        public int MaxStack;

        /// <summary>Cost to purchase from the shop, in soft currency. 0 = unobtainable in shop.</summary>
        public int ShopPrice;

        public BoosterDefinition() { }

        public BoosterDefinition(string id, BoosterType type, int effectMagnitude = 1)
        {
            Id              = id;
            Type            = type;
            EffectMagnitude = effectMagnitude;
            UseContext      = BoosterUseContext.InBattle;
            MaxStack        = 99;
        }

        public BoosterDefinition Clone()
        {
            return new BoosterDefinition
            {
                Id              = Id,
                DisplayName     = DisplayName,
                Description     = Description,
                Type            = Type,
                UseContext      = UseContext,
                EffectMagnitude = EffectMagnitude,
                SecondaryValue  = SecondaryValue,
                MaxStack        = MaxStack,
                ShopPrice       = ShopPrice,
            };
        }
    }
}
