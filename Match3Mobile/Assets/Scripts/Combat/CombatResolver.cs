using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Translates a list of matched tiles into combat effects.
/// Called by MatchResolver after each cascade wave, before tiles are removed.
///
/// Flow per wave:
///   1. Group tiles by TileType
///   2. Resolve Luck tiles first (they boost Damage calc this wave)
///   3. Resolve remaining types in order: Break → Debuff → Damage → Defense → Energy
///   4. Fire events — EnemyController and CharacterRuntime listen and apply effects
///   5. Report to CombatStats
/// </summary>
public class CombatResolver : MonoBehaviour
{
    #region Inspector Fields
    [SerializeField] private CombatConfig combatConfig;
    [SerializeField] private CharacterRuntime characterRuntime;
    #endregion

    #region Events
    /// <summary>Fired once per tile-type group per wave. Listeners apply the effect.</summary>
    public event Action<CombatEffect> OnEffectResolved;

    /// <summary>Fired after all effects in a wave are resolved.</summary>
    public event Action OnWaveResolved;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        // Forward Defense and Energy effects to CharacterRuntime
        if (characterRuntime != null)
            OnEffectResolved += characterRuntime.ApplyCombatEffect;
    }

    private void OnDestroy()
    {
        if (characterRuntime != null)
            OnEffectResolved -= characterRuntime.ApplyCombatEffect;
    }
    #endregion

    #region Public Entry Point
    /// <summary>
    /// Called by MatchResolver with the full matched tile list for one cascade wave.
    /// comboCount is the current cascade depth (1 = first match, 2 = first cascade, etc.)
    /// Pulls live attack/luck from CharacterRuntime if available.
    /// </summary>
    public void ResolveWave(List<Tile> matchedTiles, int comboCount,
                            float characterAttack = 10f, float characterLuck = 0f)
    {
        if (matchedTiles == null || matchedTiles.Count == 0) return;

        // Pull live stats if CharacterRuntime is available
        if (characterRuntime != null)
        {
            characterAttack = characterRuntime.Attack;
            characterLuck   = characterRuntime.Luck;
        }

        // 1 — Group by tile type
        Dictionary<TileType, List<Tile>> groups = GroupByType(matchedTiles);

        // 2 — Resolve Luck first so it can boost Damage calc
        float waveLuckCritBonus  = 0f;
        float waveLuckComboBonus = 0f;
        if (groups.ContainsKey(TileType.Luck))
        {
            CombatEffect luckEffect = ResolveLuck(groups[TileType.Luck], comboCount);
            waveLuckCritBonus  = luckEffect.LuckCritBonus;
            waveLuckComboBonus = luckEffect.LuckComboBonus;
            FireEffect(luckEffect);
        }

        // waveLuckCritBonus is already in crit-chance units (tiles * luckToCritRate);
        // it's passed through to RollCrit as a direct bonus — no unit conversion needed.
        // waveLuckComboBonus is a fractional boost to the combo multiplier applied in
        // ResolveDamage via extraComboMultiplier.

        // 3 — Resolve remaining types in priority order
        TileType[] resolveOrder = {
            TileType.Break,
            TileType.Debuff,
            TileType.Damage,
            TileType.Defense,
            TileType.Energy
        };

        foreach (TileType type in resolveOrder)
        {
            if (!groups.ContainsKey(type)) continue;

            CombatEffect effect = type switch
            {
                TileType.Break   => ResolveBreak(groups[type], comboCount),
                TileType.Debuff  => ResolveDebuff(groups[type], comboCount),
                TileType.Damage  => ResolveDamage(groups[type], comboCount,
                                        characterAttack, characterLuck,
                                        waveLuckCritBonus, waveLuckComboBonus),
                TileType.Defense => ResolveDefense(groups[type], comboCount),
                TileType.Energy  => ResolveEnergy(groups[type], comboCount),
                _                => default
            };

            FireEffect(effect);
        }

        // 4 — Update CombatStats combo
        CombatStats.Instance?.RecordCombo(comboCount);

        // 5 — Signal wave complete
        OnWaveResolved?.Invoke();
    }
    #endregion

    #region Private — Type Resolvers

    private CombatEffect ResolveDamage(List<Tile> tiles, int comboCount,
                                       float characterAttack, float luck,
                                       float bonusCritChance, float luckComboBonus)
    {
        int   matchSize = tiles.Count;

        // Base damage (pre-crit). Luck tiles boost the combo multiplier via
        // extraComboMultiplier = 1 + luckComboBonus (e.g. 0.15 = +15% per pair).
        float baseDmg = CombatFormula.CalculateDamage(
                            combatConfig, matchSize, characterAttack,
                            comboCount, luck, enemyDefense: 0f,
                            extraComboMultiplier: 1f + luckComboBonus);
                            // Enemy defense subtracted by EnemyController, not here.

        // Single crit roll: character luck + any wave-scoped Luck tile crit bonus.
        bool  isCrit   = CombatFormula.RollCrit(combatConfig, luck, bonusCritChance);
        float finalDmg = isCrit ? baseDmg * combatConfig.critDamageMultiplier : baseDmg;

        // Apply queued ultimate multiplier (returns 1f if not queued — no-op).
        // Done here (not in UltimateSystem) because CombatEffect is a struct and
        // event subscribers can't mutate it after the fact.
        float ultMult = characterRuntime != null
                        ? characterRuntime.ConsumeUltimateMultiplier()
                        : 1f;
        finalDmg *= ultMult;
        finalDmg  = Mathf.Max(0f, finalDmg);

        CombatStats.Instance?.RecordDamage(finalDmg, isCrit);

        return new CombatEffect
        {
            SourceType    = TileType.Damage,
            DamageDealt   = finalDmg,
            IsCriticalHit = isCrit,
            MatchSize     = matchSize,
            ComboCount    = comboCount
        };
    }

    private CombatEffect ResolveEnergy(List<Tile> tiles, int comboCount)
    {
        float energy = CombatFormula.CalculateEnergy(combatConfig, tiles.Count);
        CombatStats.Instance?.RecordEnergy(energy);

        return new CombatEffect
        {
            SourceType      = TileType.Energy,
            EnergyGenerated = energy,
            MatchSize       = tiles.Count,
            ComboCount      = comboCount
        };
    }

    private CombatEffect ResolveDefense(List<Tile> tiles, int comboCount)
    {
        float shield = CombatFormula.CalculateDefense(combatConfig, tiles.Count);
        CombatStats.Instance?.RecordShield(shield);

        return new CombatEffect
        {
            SourceType      = TileType.Defense,
            ShieldGenerated = shield,
            MatchSize       = tiles.Count,
            ComboCount      = comboCount
        };
    }

    private CombatEffect ResolveBreak(List<Tile> tiles, int comboCount)
    {
        float armorDmg = CombatFormula.CalculateArmorDamage(combatConfig, tiles.Count);

        return new CombatEffect
        {
            SourceType      = TileType.Break,
            ArmorDamageDealt = armorDmg,
            MatchSize       = tiles.Count,
            ComboCount      = comboCount
        };
    }

    private CombatEffect ResolveDebuff(List<Tile> tiles, int comboCount)
    {
        bool applyVuln = tiles.Count >= 2;

        return new CombatEffect
        {
            SourceType           = TileType.Debuff,
            AppliesPoison        = true,
            AppliesVulnerability = applyVuln,
            DebuffDuration       = combatConfig.poisonDuration,
            MatchSize            = tiles.Count,
            ComboCount           = comboCount
        };
    }

    private CombatEffect ResolveLuck(List<Tile> tiles, int comboCount)
    {
        float critBonus  = tiles.Count * combatConfig.luckToCritRate;
        float comboBonus = tiles.Count * 0.05f; // +5% combo mult per luck tile
        CombatStats.Instance?.RecordLuckTile();

        return new CombatEffect
        {
            SourceType      = TileType.Luck,
            LuckCritBonus   = critBonus,
            LuckComboBonus  = comboBonus,
            MatchSize       = tiles.Count,
            ComboCount      = comboCount
        };
    }
    #endregion

    #region Private — Helpers
    private void FireEffect(CombatEffect effect)
    {
        if (effect.SourceType == default && effect.MatchSize == 0) return;
        OnEffectResolved?.Invoke(effect);
    }

    private Dictionary<TileType, List<Tile>> GroupByType(List<Tile> tiles)
    {
        var groups = new Dictionary<TileType, List<Tile>>();
        foreach (Tile t in tiles)
        {
            if (!groups.ContainsKey(t.TileType))
                groups[t.TileType] = new List<Tile>();
            groups[t.TileType].Add(t);
        }
        return groups;
    }
    #endregion
}
