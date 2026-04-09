using UnityEngine;

namespace Matchmancer.Character
{
    /// <summary>
    /// Inspector-facing character definition. Creates the pure-C#
    /// <see cref="CharacterTuning"/> that <see cref="CharacterRuntime"/>
    /// consumes. One asset per playable character.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterData", menuName = "Matchmancer/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("Identity")]
        public string displayName = "The Arcanist";
        [TextArea] public string loreDescription;
        public Sprite portrait;
        public Sprite battleSprite;

        [Header("Base Stats (Level 1)")]
        [Min(1)]  public int   baseMaxHp  = 150;
        [Min(0f)] public float baseAttack = 10f;
        [Min(0f)] public float baseDefense= 5f;
        [Min(0f)] public float baseLuck   = 0f;

        [Header("Growth Per Level")]
        [Min(0)]  public int   hpPerLevel      = 10;
        [Min(0f)] public float attackPerLevel  = 1f;
        [Min(0f)] public float defensePerLevel = 0.5f;
        [Min(0f)] public float luckPerLevel    = 0.2f;

        [Header("Ultimate")]
        public string ultimateName = "Soul Surge";
        public Sprite ultimateIcon;
        [TextArea] public string ultimateDescription;
        [Min(1f)] public float maxEnergy = 100f;
        [Min(1f)] public float ultimateDamageMultiplier = 3f;

        [Header("XP Curve")]
        [Tooltip("XP required to reach level N+1. Index 0 = XP to reach level 2. " +
                 "Array length + 1 is the hard level cap.")]
        public int[] xpPerLevel =
            { 100, 150, 220, 310, 420, 550, 700, 870, 1060 };

        /// <summary>
        /// Convert Inspector values to pure-C# tuning that the runtime uses.
        /// Keeps CharacterRuntime testable without Unity.
        /// </summary>
        public CharacterTuning ToTuning()
        {
            return new CharacterTuning
            {
                DisplayName              = string.IsNullOrEmpty(displayName) ? "Player" : displayName,
                BaseMaxHp                = baseMaxHp,
                BaseAttack               = baseAttack,
                BaseDefense              = baseDefense,
                BaseLuck                 = baseLuck,
                HpPerLevel               = hpPerLevel,
                AttackPerLevel           = attackPerLevel,
                DefensePerLevel          = defensePerLevel,
                LuckPerLevel             = luckPerLevel,
                UltimateName             = string.IsNullOrEmpty(ultimateName) ? "Ultimate" : ultimateName,
                MaxEnergy                = maxEnergy,
                UltimateDamageMultiplier = ultimateDamageMultiplier,
                XpPerLevel               = xpPerLevel != null ? (int[])xpPerLevel.Clone() : new int[0],
            };
        }

        /// <summary>Factory: build a runtime from this asset.</summary>
        public CharacterRuntime CreateRuntime(int startingLevel = 1)
        {
            return new CharacterRuntime(ToTuning(), startingLevel);
        }
    }
}
