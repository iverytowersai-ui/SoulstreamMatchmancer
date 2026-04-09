using Matchmancer.Core;

namespace Matchmancer.Combat
{
    /// <summary>
    /// The resolved output of one match's combat effect. Produced by
    /// <see cref="CombatResolver"/>, consumed by the enemy controller
    /// (Skill 13), character runtime (Skill 14), and battle UI.
    ///
    /// Pure data — no logic, no Unity types. Use <c>Role</c> to branch,
    /// not <c>SourceTile</c>, because the enemy cares about combat role
    /// not tile identity.
    /// </summary>
    public readonly struct CombatEffect
    {
        /// <summary>The raw Soulstream tile type that produced this effect.</summary>
        public TileType SourceTile { get; }

        /// <summary>The combat role this tile plays (Damage, Energy, etc.).</summary>
        public CombatRole Role { get; }

        /// <summary>Number of tiles in the originating match (always ≥3).</summary>
        public int MatchSize { get; }

        /// <summary>Cascade depth when this match resolved (1 = first wave, 2+ = cascades).</summary>
        public int ComboCount { get; }

        // ---- Damage ----
        public float DamageDealt { get; }
        public bool  IsCriticalHit { get; }

        // ---- Defense / Energy / Break ----
        public float ShieldGenerated  { get; }
        public float EnergyGenerated  { get; }
        public float ArmorDamageDealt { get; }

        // ---- Debuff ----
        public bool AppliesPoison        { get; }
        public bool AppliesVulnerability { get; }
        public int  DebuffDuration       { get; }

        // ---- Luck (applied to the WAVE, not the enemy) ----
        public float LuckCritBonus  { get; }
        public float LuckComboBonus { get; }

        public CombatEffect(
            TileType   sourceTile,
            CombatRole role,
            int        matchSize,
            int        comboCount,
            float      damageDealt         = 0f,
            bool       isCriticalHit       = false,
            float      shieldGenerated     = 0f,
            float      energyGenerated     = 0f,
            float      armorDamageDealt    = 0f,
            bool       appliesPoison       = false,
            bool       appliesVulnerability = false,
            int        debuffDuration      = 0,
            float      luckCritBonus       = 0f,
            float      luckComboBonus      = 0f)
        {
            SourceTile           = sourceTile;
            Role                 = role;
            MatchSize            = matchSize;
            ComboCount           = comboCount;
            DamageDealt          = damageDealt;
            IsCriticalHit        = isCriticalHit;
            ShieldGenerated      = shieldGenerated;
            EnergyGenerated      = energyGenerated;
            ArmorDamageDealt     = armorDamageDealt;
            AppliesPoison        = appliesPoison;
            AppliesVulnerability = appliesVulnerability;
            DebuffDuration       = debuffDuration;
            LuckCritBonus        = luckCritBonus;
            LuckComboBonus       = luckComboBonus;
        }
    }
}
