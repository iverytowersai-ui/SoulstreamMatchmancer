/// <summary>
/// The resolved output of one tile-type group match.
/// Produced by CombatResolver, consumed by EnemyController, CharacterRuntime, and BattleUIController.
/// </summary>
public struct CombatEffect
{
    public TileType SourceType;

    // Damage
    public float DamageDealt;
    public bool  IsCriticalHit;

    // Defense
    public float ShieldGenerated;

    // Energy
    public float EnergyGenerated;

    // Break
    public float ArmorDamageDealt;

    // Debuff (applied to enemy)
    public bool  AppliesPoison;
    public bool  AppliesVulnerability;
    public int   DebuffDuration;

    // Luck support (affects next Damage tile calculation in same wave)
    public float LuckCritBonus;     // added to crit chance this wave
    public float LuckComboBonus;    // added to combo multiplier this wave

    // Metadata
    public int   MatchSize;
    public int   ComboCount;
}
