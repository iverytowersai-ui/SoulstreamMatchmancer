using UnityEngine;

namespace Matchmancer.Boosters
{
    /// <summary>
    /// Unity-side ScriptableObject wrapper around <see cref="BoosterDefinition"/>.
    /// Holds icon and serialized fields; <see cref="ToDefinition"/> converts to
    /// the pure C# definition used by the headless inventory system.
    /// </summary>
    [CreateAssetMenu(menuName = "Matchmancer/Booster Data", fileName = "Booster_New")]
    public class BoosterData : ScriptableObject
    {
        [Header("Identity")]
        public string id;
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Behavior")]
        public BoosterType type;
        public BoosterUseContext useContext = BoosterUseContext.InBattle;
        public int effectMagnitude = 1;
        public int secondaryValue;

        [Header("Economy")]
        public int maxStack = 99;
        public int shopPrice;

        public BoosterDefinition ToDefinition()
        {
            return new BoosterDefinition
            {
                Id              = string.IsNullOrEmpty(id) ? name : id,
                DisplayName     = displayName,
                Description     = description,
                Type            = type,
                UseContext      = useContext,
                EffectMagnitude = effectMagnitude,
                SecondaryValue  = secondaryValue,
                MaxStack        = maxStack,
                ShopPrice       = shopPrice,
            };
        }
    }
}
