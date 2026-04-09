using UnityEngine;

namespace Matchmancer.Character
{
    /// <summary>
    /// Inspector-facing gear definition. Drop one .asset per equippable item
    /// under <c>Assets/Data/Gear/…</c>. Produces a pure-C# <see cref="GearTuning"/>
    /// via <see cref="ToTuning"/> which the runtime + tests consume.
    /// </summary>
    [CreateAssetMenu(fileName = "GearData", menuName = "Matchmancer/Gear Data")]
    public class GearData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Stable save id. Never rename after the item ships.")]
        public string id;
        public string displayName;
        [TextArea] public string flavorText;
        public Sprite icon;

        [Header("Classification")]
        public GearSlot   slot   = GearSlot.Weapon;
        public GearRarity rarity = GearRarity.Common;

        [Header("Flat Bonuses")]
        public int   flatMaxHp;
        public float flatAttack;
        public float flatDefense;
        public float flatLuck;

        [Header("Percent Bonuses (0.15 = +15%)")]
        [Range(-0.5f, 2f)] public float pctMaxHp;
        [Range(-0.5f, 2f)] public float pctAttack;
        [Range(-0.5f, 2f)] public float pctDefense;
        [Range(-0.5f, 2f)] public float pctLuck;

        [Header("Economy")]
        [Min(0)] public int sellPrice;

        /// <summary>
        /// Project Inspector values into the pure-C# tuning the runtime uses.
        /// Falls back to the asset name when <see cref="id"/> is blank so tests
        /// and early dev assets never explode on empty strings.
        /// </summary>
        public GearTuning ToTuning()
        {
            return new GearTuning
            {
                Id          = string.IsNullOrEmpty(id)          ? name : id,
                DisplayName = string.IsNullOrEmpty(displayName) ? name : displayName,
                FlavorText  = flavorText ?? string.Empty,
                Slot        = slot,
                Rarity      = rarity,
                SellPrice   = sellPrice,
                Modifiers   = new GearStatModifiers
                {
                    FlatMaxHp   = flatMaxHp,
                    FlatAttack  = flatAttack,
                    FlatDefense = flatDefense,
                    FlatLuck    = flatLuck,
                    PctMaxHp    = pctMaxHp,
                    PctAttack   = pctAttack,
                    PctDefense  = pctDefense,
                    PctLuck     = pctLuck,
                },
            };
        }

        /// <summary>Factory: build a fresh runtime item from this asset.</summary>
        public GearItem CreateItem()
        {
            return new GearItem(ToTuning());
        }
    }
}
