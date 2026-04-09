using System;
using System.Collections.Generic;
using Matchmancer.Core;
using Matchmancer.Match;

namespace Matchmancer.Combat
{
    /// <summary>
    /// Translates matched tiles into combat effects.
    ///
    /// BoardController calls <see cref="ResolveWave"/> once per cascade wave
    /// with the full list of <see cref="MatchInfo"/> that wave produced.
    /// The resolver:
    ///   1. Resolves Luck matches first (they boost Damage crit this wave).
    ///   2. Resolves remaining matches in role order:
    ///      Break → Debuff → Damage → Defense → Energy.
    ///   3. Returns the list of <see cref="CombatEffect"/> and fires
    ///      <see cref="OnEffectResolved"/> per effect + <see cref="OnWaveResolved"/>
    ///      once at the end.
    ///
    /// Pure C# — no UnityEngine — so it can be unit-tested with a seeded
    /// <see cref="System.Random"/>.
    ///
    /// Enemy defense is NOT subtracted here. Skill 13 (EnemyController) will
    /// listen to <see cref="OnEffectResolved"/> and apply its own defense to
    /// <see cref="CombatEffect.DamageDealt"/>. This keeps the resolver
    /// enemy-agnostic and lets tests run without an enemy at all.
    /// </summary>
    public class CombatResolver
    {
        private readonly CombatTuning _tuning;
        private readonly CombatStats  _stats;
        private readonly Random       _rng;

        /// <summary>Fired once per match in the wave.</summary>
        public event Action<CombatEffect> OnEffectResolved;

        /// <summary>Fired once after all matches in the wave are resolved.</summary>
        public event Action<IReadOnlyList<CombatEffect>> OnWaveResolved;

        public CombatResolver(CombatTuning tuning, CombatStats stats, Random rng)
        {
            _tuning = tuning ?? throw new ArgumentNullException(nameof(tuning));
            _stats  = stats  ?? throw new ArgumentNullException(nameof(stats));
            _rng    = rng    ?? throw new ArgumentNullException(nameof(rng));
        }

        /// <summary>
        /// Resolve all matches from one cascade wave.
        /// </summary>
        /// <param name="matches">All MatchInfo objects from this wave.</param>
        /// <param name="comboCount">Cascade depth (1 = first wave, 2+ = cascade chains).</param>
        /// <param name="characterAttack">Live attack stat. Skill 14 supplies; defaults to 10.</param>
        /// <param name="characterLuck">Live luck stat. Skill 14 supplies; defaults to 0.</param>
        public IReadOnlyList<CombatEffect> ResolveWave(
            IReadOnlyList<MatchInfo> matches,
            int   comboCount,
            float characterAttack = 10f,
            float characterLuck   = 0f)
        {
            var effects = new List<CombatEffect>();
            if (matches == null || matches.Count == 0)
            {
                OnWaveResolved?.Invoke(effects);
                return effects;
            }

            // --- Pass 1: Luck matches boost the rest of this wave ---
            float waveLuckCritBonus  = 0f;
            float waveLuckComboBonus = 0f;

            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                if (!CombatRoleMap.TryGetRole(match.TileType, out var role)) continue;
                if (role != CombatRole.Luck) continue;

                var luckEffect = ResolveLuck(match, comboCount);
                waveLuckCritBonus  += luckEffect.LuckCritBonus;
                waveLuckComboBonus += luckEffect.LuckComboBonus;
                effects.Add(luckEffect);
                OnEffectResolved?.Invoke(luckEffect);
            }

            // --- Pass 2: Non-Luck matches in priority order ---
            // Break first so armor is cracked before Damage lands.
            // Debuff before Damage so vulnerability multiplier (future) can apply.
            // Damage before Defense/Energy so the big numbers are dispatched first.
            CombatRole[] order =
            {
                CombatRole.Break,
                CombatRole.Debuff,
                CombatRole.Damage,
                CombatRole.Defense,
                CombatRole.Energy,
            };

            for (int o = 0; o < order.Length; o++)
            {
                var targetRole = order[o];

                for (int i = 0; i < matches.Count; i++)
                {
                    var match = matches[i];
                    if (!CombatRoleMap.TryGetRole(match.TileType, out var role)) continue;
                    if (role != targetRole) continue;

                    CombatEffect effect = targetRole switch
                    {
                        CombatRole.Break   => ResolveBreak  (match, comboCount),
                        CombatRole.Debuff  => ResolveDebuff (match, comboCount),
                        CombatRole.Damage  => ResolveDamage (match, comboCount,
                                                             characterAttack,
                                                             characterLuck,
                                                             waveLuckCritBonus,
                                                             waveLuckComboBonus),
                        CombatRole.Defense => ResolveDefense(match, comboCount),
                        CombatRole.Energy  => ResolveEnergy (match, comboCount),
                        _                  => default,
                    };

                    effects.Add(effect);
                    OnEffectResolved?.Invoke(effect);
                }
            }

            // --- Bookkeeping ---
            _stats.RecordCombo(comboCount);
            OnWaveResolved?.Invoke(effects);
            return effects;
        }

        // ------------------------------------------------------------------
        // Role resolvers
        // ------------------------------------------------------------------

        private CombatEffect ResolveDamage(
            MatchInfo match,
            int   comboCount,
            float characterAttack,
            float characterLuck,
            float luckCritBonus,
            float luckComboBonus)
        {
            // Luck bonus is additive to base crit chance. Scale back to "luck units"
            // using LuckToCritRate so CombatFormula.GetCritChance produces the bonus.
            float effectiveLuck = characterLuck;
            if (_tuning.LuckToCritRate > 0f && luckCritBonus > 0f)
                effectiveLuck += luckCritBonus / _tuning.LuckToCritRate;

            // Combo bonus from luck is a flat additive to the base combo multiplier.
            // We simulate by inflating comboCount: each 0.1 bonus ≈ one extra combo step.
            int effectiveCombo = comboCount;
            if (_tuning.ComboMultiplierStep > 0f && luckComboBonus > 0f)
                effectiveCombo += (int)Math.Round(luckComboBonus / _tuning.ComboMultiplierStep);

            var result = CombatFormula.CalculateDamage(
                _tuning,
                matchSize: match.TileCount,
                characterAttack: characterAttack,
                comboCount: effectiveCombo,
                luck: effectiveLuck,
                enemyDefense: 0f,     // applied later by EnemyController (Skill 13)
                rng: _rng);

            _stats.RecordDamage(result.Amount, result.IsCrit);

            return new CombatEffect(
                sourceTile: match.TileType,
                role: CombatRole.Damage,
                matchSize: match.TileCount,
                comboCount: comboCount,
                damageDealt: result.Amount,
                isCriticalHit: result.IsCrit);
        }

        private CombatEffect ResolveEnergy(MatchInfo match, int comboCount)
        {
            float energy = CombatFormula.CalculateEnergy(_tuning, match.TileCount);
            _stats.RecordEnergy(energy);

            return new CombatEffect(
                sourceTile: match.TileType,
                role: CombatRole.Energy,
                matchSize: match.TileCount,
                comboCount: comboCount,
                energyGenerated: energy);
        }

        private CombatEffect ResolveDefense(MatchInfo match, int comboCount)
        {
            float shield = CombatFormula.CalculateDefense(_tuning, match.TileCount);
            _stats.RecordShield(shield);

            return new CombatEffect(
                sourceTile: match.TileType,
                role: CombatRole.Defense,
                matchSize: match.TileCount,
                comboCount: comboCount,
                shieldGenerated: shield);
        }

        private CombatEffect ResolveBreak(MatchInfo match, int comboCount)
        {
            float armorDmg = CombatFormula.CalculateArmorDamage(_tuning, match.TileCount);
            _stats.RecordBreakTriggered();

            return new CombatEffect(
                sourceTile: match.TileType,
                role: CombatRole.Break,
                matchSize: match.TileCount,
                comboCount: comboCount,
                armorDamageDealt: armorDmg);
        }

        private CombatEffect ResolveDebuff(MatchInfo match, int comboCount)
        {
            // Any debuff match applies poison. A match of 4+ also applies vulnerability.
            bool applyVuln = match.TileCount >= 4;
            _stats.RecordDebuffApplied();

            return new CombatEffect(
                sourceTile: match.TileType,
                role: CombatRole.Debuff,
                matchSize: match.TileCount,
                comboCount: comboCount,
                appliesPoison: true,
                appliesVulnerability: applyVuln,
                debuffDuration: _tuning.PoisonDuration);
        }

        private CombatEffect ResolveLuck(MatchInfo match, int comboCount)
        {
            // Each Luck tile in the match adds crit chance and combo mult for this wave.
            float critBonus  = match.TileCount * _tuning.LuckToCritRate;
            float comboBonus = match.TileCount * _tuning.ComboMultiplierStep * 0.5f;
            _stats.RecordLuckTile(match.TileCount);

            return new CombatEffect(
                sourceTile: match.TileType,
                role: CombatRole.Luck,
                matchSize: match.TileCount,
                comboCount: comboCount,
                luckCritBonus: critBonus,
                luckComboBonus: comboBonus);
        }
    }
}
