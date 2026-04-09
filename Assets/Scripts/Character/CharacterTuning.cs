using System;

namespace Matchmancer.Character
{
    /// <summary>
    /// Plain value object holding every tunable character stat.
    /// Pure C# so tests can construct one without Unity. The Unity-facing
    /// <see cref="CharacterData"/> ScriptableObject builds one of these
    /// from Inspector values via <c>ToTuning()</c>, and
    /// <see cref="CharacterRuntime"/> consumes it.
    ///
    /// Parallels <c>Matchmancer.Combat.CombatTuning</c> in style.
    /// </summary>
    public class CharacterTuning
    {
        // === Identity ===
        public string DisplayName { get; set; } = "Player";

        // === Base stats at level 1 ===
        public int   BaseMaxHp  { get; set; } = 150;
        public float BaseAttack { get; set; } = 10f;
        public float BaseDefense{ get; set; } = 5f;
        public float BaseLuck   { get; set; } = 0f;

        // === Growth per level ===
        public int   HpPerLevel      { get; set; } = 10;
        public float AttackPerLevel  { get; set; } = 1f;
        public float DefensePerLevel { get; set; } = 0.5f;
        public float LuckPerLevel    { get; set; } = 0.2f;

        // === Ultimate ===
        public string UltimateName             { get; set; } = "Soul Surge";
        public float  MaxEnergy                { get; set; } = 100f;
        public float  UltimateDamageMultiplier { get; set; } = 3f;

        // === XP curve ===
        /// <summary>
        /// XP required to reach level N+1. Index 0 = XP to reach level 2.
        /// Length + 1 is the hard level cap.
        /// </summary>
        public int[] XpPerLevel { get; set; } =
            { 100, 150, 220, 310, 420, 550, 700, 870, 1060 };

        /// <summary>Factory: returns a fresh tuning with all defaults.</summary>
        public static CharacterTuning Default() => new CharacterTuning();

        public CharacterTuning Clone()
        {
            var clone = new CharacterTuning
            {
                DisplayName              = DisplayName,
                BaseMaxHp                = BaseMaxHp,
                BaseAttack               = BaseAttack,
                BaseDefense              = BaseDefense,
                BaseLuck                 = BaseLuck,
                HpPerLevel               = HpPerLevel,
                AttackPerLevel           = AttackPerLevel,
                DefensePerLevel          = DefensePerLevel,
                LuckPerLevel             = LuckPerLevel,
                UltimateName             = UltimateName,
                MaxEnergy                = MaxEnergy,
                UltimateDamageMultiplier = UltimateDamageMultiplier,
            };
            clone.XpPerLevel = XpPerLevel != null ? (int[])XpPerLevel.Clone() : Array.Empty<int>();
            return clone;
        }
    }
}
