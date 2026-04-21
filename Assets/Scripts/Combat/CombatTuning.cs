namespace Matchmancer.Combat
{
    /// <summary>
    /// Plain value object holding every tunable combat constant.
    /// Pure C# so tests can construct one without Unity. The Unity-facing
    /// <see cref="CombatConfig"/> ScriptableObject builds one of these from
    /// Inspector values, and <see cref="CombatFormula"/> consumes it.
    ///
    /// Starter values match the Matchmancer master spec formula.
    /// </summary>
    public class CombatTuning
    {
        // === Base values ===
        public float BaseTileValue       { get; set; } = 10f;
        public float BaseEnergyValue     { get; set; } = 15f;
        public float BaseDefenseValue    { get; set; } = 8f;

        // === Match size multipliers ===
        public float Match3Multiplier     { get; set; } = 1.0f;
        public float Match4Multiplier     { get; set; } = 1.4f;
        public float Match5PlusMultiplier { get; set; } = 2.0f;

        // === Combo ===
        /// <summary>Multiplier added per cascade step. 0.1 = +10% per chain.</summary>
        public float ComboMultiplierStep { get; set; } = 0.1f;
        public float MaxComboMultiplier  { get; set; } = 3.0f;

        // === Critical hits ===
        /// <summary>Base chance (0–1). 0.05 = 5%.</summary>
        public float BaseCritChance       { get; set; } = 0.05f;
        public float CritDamageMultiplier { get; set; } = 1.5f;

        // === Luck ===
        /// <summary>Crit chance added per point of Luck. 0.005 = 0.5%.</summary>
        public float LuckToCritRate { get; set; } = 0.005f;

        // === Defense ===
        public float DefensePerTile { get; set; } = 8f;

        // === Break ===
        public float ArmorDamagePerTile { get; set; } = 12f;

        // === Debuff ===
        public float PoisonDamagePerTurn { get; set; } = 5f;
        public int   PoisonDuration      { get; set; } = 3;
        public float VulnerabilityMult   { get; set; } = 1.25f;

        // === Fairness (distinctiveness guideline #2) ===
        /// <summary>
        /// Multiplier applied to the enemy's final crit chance. Default 0.33 means
        /// the enemy crits at roughly one-third the rate the player does, making the
        /// player feel "lucky" about 3× as often as the AI. This is a shipped Puzzle
        /// Quest finding — symmetric luck feels rigged to players even when it isn't.
        /// Only applied when <see cref="CombatSide.Enemy"/> is passed to
        /// <see cref="CombatFormula.GetCritChance"/> / <see cref="CombatFormula.RollCrit"/>.
        /// Set to 1.0 to make combat symmetric; 0.0 to make the enemy never crit.
        /// </summary>
        public float EnemyCritMultiplier { get; set; } = 0.33f;

        /// <summary>Factory: returns a fresh tuning with all defaults.</summary>
        public static CombatTuning Default() => new CombatTuning();
    }
}
