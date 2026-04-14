using UnityEngine;

/// <summary>
/// Stateless formula calculator. All combat math goes through here.
/// Pass CombatConfig for tuning values — never hardcode constants.
/// </summary>
public static class CombatFormula
{
    /// <summary>
    /// Calculate pre-crit damage from a Damage tile match. Callers are responsible
    /// for rolling crit and applying the crit multiplier — this keeps crit state
    /// consistent with what gets reported in CombatEffect / CombatStats.
    ///
    /// <paramref name="extraComboMultiplier"/> is an additional multiplicative
    /// factor applied on top of the base combo multiplier (e.g. from Luck tiles).
    /// Defaults to 1.0 (no extra boost).
    /// </summary>
    public static float CalculateDamage(
        CombatConfig config,
        int          matchSize,
        float        characterAttack,
        int          comboCount,
        float        luck,
        float        enemyDefense,
        float        extraComboMultiplier = 1f)
    {
        float baseValue = config.baseTileValue * matchSize;
        float sizeMult  = GetMatchSizeMultiplier(config, matchSize);
        float charMod   = characterAttack / 10f;   // normalised: 10 attack = 1.0×
        float comboMult = Mathf.Min(
                              1f + comboCount * config.comboMultiplierStep,
                              config.maxComboMultiplier)
                          * Mathf.Max(0f, extraComboMultiplier);

        // NOTE: no crit roll here — callers apply crit (see CombatResolver).
        // Luck parameter retained for future formula use; currently unused here.
        _ = luck;

        float raw    = baseValue * sizeMult * charMod * comboMult;
        float final_ = Mathf.Max(0f, raw - enemyDefense);

        return final_;
    }

    /// <summary>
    /// Calculate energy gained from an Energy tile match.
    /// </summary>
    public static float CalculateEnergy(CombatConfig config, int matchSize)
    {
        return config.baseEnergyValue * matchSize * GetMatchSizeMultiplier(config, matchSize);
    }

    /// <summary>
    /// Calculate shield generated from a Defense tile match.
    /// </summary>
    public static float CalculateDefense(CombatConfig config, int matchSize)
    {
        return config.defensePerTile * matchSize;
    }

    /// <summary>
    /// Calculate armor damage from a Break tile match.
    /// </summary>
    public static float CalculateArmorDamage(CombatConfig config, int matchSize)
    {
        return config.armorDamagePerTile * matchSize;
    }

    /// <summary>
    /// Returns true if a critical hit should occur based on base chance + luck modifier.
    /// <paramref name="bonusCritChance"/> is added directly to the final crit chance
    /// (already in crit-chance units, 0..1). Use this for wave-scoped bonuses such
    /// as Luck tile effects whose contribution is already in crit-chance units.
    /// </summary>
    public static bool RollCrit(CombatConfig config, float luck, float bonusCritChance = 0f)
    {
        float critChance = Mathf.Clamp01(
            config.baseCritChance + luck * config.luckToCritRate + bonusCritChance);
        return Random.value < critChance;
    }

    /// <summary>
    /// Returns the match size damage multiplier.
    /// </summary>
    public static float GetMatchSizeMultiplier(CombatConfig config, int matchSize)
    {
        if (matchSize >= 5) return config.match5PlusMultiplier;
        if (matchSize == 4) return config.match4Multiplier;
        return config.match3Multiplier;
    }

    /// <summary>
    /// Returns the combo multiplier for the given cascade depth.
    /// </summary>
    public static float GetComboMultiplier(CombatConfig config, int comboCount)
    {
        return Mathf.Min(1f + comboCount * config.comboMultiplierStep, config.maxComboMultiplier);
    }
}
