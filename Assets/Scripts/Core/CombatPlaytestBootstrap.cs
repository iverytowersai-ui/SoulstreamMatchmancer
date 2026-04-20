using System.Collections.Generic;
using UnityEngine;
using Matchmancer.Combat;
using Matchmancer.Core;
using Matchmancer.Enemy;
using Matchmancer.Match;

namespace Matchmancer.Core
{
    /// <summary>
    /// Zero-setup combat loop smoke test.
    ///
    /// Attach to any empty GameObject in a scene (e.g. BattleScene) and press
    /// Play. On Start it:
    ///   1. Builds a CombatTuning + CombatStats + CombatResolver.
    ///   2. Builds an EnemyRuntime (either from the assigned EnemyData, or from
    ///      a safe built-in test enemy if the slot is empty).
    ///   3. Fires a scripted wave of fake MatchInfo objects through the
    ///      resolver and feeds the resulting CombatEffects into the enemy.
    ///   4. Logs HP, armor, damage-per-effect, crits, and defeat state.
    ///
    /// This verifies Skill 11 (tile types), Skill 12 (combat resolver), and
    /// Skill 13 (enemy runtime) all wire together correctly — without needing
    /// a real board or player input.
    /// </summary>
    public class CombatPlaytestBootstrap : MonoBehaviour
    {
        [Header("Inputs (optional)")]
        [Tooltip("Leave null to use a built-in test enemy (120 HP, 2 defense, no armor).")]
        [SerializeField] private EnemyData _enemyAsset;

        [Tooltip("Leave null to use CombatTuning.Default().")]
        [SerializeField] private CombatConfig _combatConfig;

        [Header("Scripted Wave")]
        [Tooltip("Character attack stat passed to the resolver (10 = 1.0x modifier).")]
        [SerializeField] private float _characterAttack = 12f;

        [Tooltip("Character luck stat (0..1 raises crit chance).")]
        [SerializeField] private float _characterLuck = 0.1f;

        [Tooltip("Fire a second wave with combo multiplier after the first, to verify cascades.")]
        [SerializeField] private bool _fireSecondWave = true;

        [Header("Determinism")]
        [Tooltip("RNG seed so crit outcomes are reproducible.")]
        [SerializeField] private int _rngSeed = 12345;

        private void Start()
        {
            Debug.Log("<color=cyan>=== Matchmancer Combat Playtest ===</color>");

            // --- 1. Tuning ---
            CombatTuning tuning = (_combatConfig != null)
                ? _combatConfig.ToTuning()
                : CombatTuning.Default();
            Debug.Log($"[Playtest] Tuning ready. BaseTileValue={tuning.BaseTileValue}, " +
                      $"BaseCritChance={tuning.BaseCritChance}");

            // --- 2. Stats + Resolver ---
            var stats    = new CombatStats();
            stats.ResetForNewBattle();
            var rng      = new System.Random(_rngSeed);
            var resolver = new CombatResolver(tuning, stats, rng);

            // --- 3. Enemy ---
            EnemyRuntime enemy = (_enemyAsset != null)
                ? _enemyAsset.CreateRuntime(tuning)
                : BuildDefaultTestEnemy(tuning);
            Debug.Log($"[Playtest] Enemy '{enemy.DisplayName}' spawned. " +
                      $"HP={enemy.CurrentHp}/{enemy.MaxHp}, " +
                      $"Armor={enemy.CurrentArmor}/{enemy.MaxArmor}, " +
                      $"Defense={enemy.Defense}");

            // --- 4. Wire events ---
            resolver.OnEffectResolved += effect =>
            {
                Debug.Log($"[Resolver] Effect: role={effect.Role}, " +
                          $"matchSize={effect.MatchSize}, dmg={effect.DamageDealt}, " +
                          $"crit={effect.IsCriticalHit}, energy={effect.EnergyGenerated}, " +
                          $"shield={effect.ShieldGenerated}");
                enemy.ApplyCombatEffect(effect);
            };
            resolver.OnWaveResolved += effects =>
            {
                Debug.Log($"<color=yellow>[Resolver] Wave complete — {effects.Count} effect(s). " +
                          $"Enemy HP now {enemy.CurrentHp}/{enemy.MaxHp}</color>");
            };
            enemy.OnHpChanged += (oldHp, newHp, delta) =>
                Debug.Log($"[Enemy] HP {oldHp} → {newHp} (Δ{delta})");
            enemy.OnDefeated += () =>
                Debug.Log($"<color=red>[Enemy] DEFEATED</color>");

            // --- 5. Wave 1: a clean damage+energy+defense wave ---
            Debug.Log("<color=lime>--- Wave 1 (comboCount=1) ---</color>");
            var wave1 = new List<MatchInfo>
            {
                FakeMatch(TileType.SoulstreamShard, 3),   // Damage
                FakeMatch(TileType.PortRune,        3),   // Energy
                FakeMatch(TileType.CovenSeal,       4),   // Defense (4-match)
            };
            resolver.ResolveWave(wave1, comboCount: 1,
                                 characterAttack: _characterAttack,
                                 characterLuck:   _characterLuck);

            // --- 6. Wave 2: bigger match + break + luck to prove cascade combo ---
            if (_fireSecondWave && !enemy.IsDefeated)
            {
                Debug.Log("<color=lime>--- Wave 2 (comboCount=2, cascade) ---</color>");
                var wave2 = new List<MatchInfo>
                {
                    FakeMatch(TileType.PetshaCharm,      3), // Luck (boosts rest of wave)
                    FakeMatch(TileType.WitchbreedThorn,  3), // Break
                    FakeMatch(TileType.OzoneMark,        4), // Debuff
                    FakeMatch(TileType.SoulstreamShard,  5), // 5-match damage
                };
                resolver.ResolveWave(wave2, comboCount: 2,
                                     characterAttack: _characterAttack,
                                     characterLuck:   _characterLuck);
            }

            // --- 7. Summary ---
            Debug.Log("<color=cyan>=== Playtest Summary ===</color>");
            Debug.Log($"Total damage dealt   : {stats.TotalDamageDealt}");
            Debug.Log($"Max single hit       : {stats.MaxSingleHit}");
            Debug.Log($"Critical hits        : {stats.TotalCriticalHits}");
            Debug.Log($"Shield generated     : {stats.TotalShieldGenerated}");
            Debug.Log($"Energy generated     : {stats.TotalEnergyGenerated}");
            Debug.Log($"Enemy final HP       : {enemy.CurrentHp}/{enemy.MaxHp}");
            Debug.Log($"Enemy defeated       : {enemy.IsDefeated}");
            Debug.Log("<color=cyan>=========================</color>");
        }

        private static EnemyRuntime BuildDefaultTestEnemy(CombatTuning tuning)
        {
            return new EnemyRuntime(
                displayName:     "Training Dummy",
                maxHp:           120,
                defense:         2f,
                maxArmor:        0,
                baseAttackPower: 10f,
                attacksPerTurn:  1,
                tuning:          tuning);
        }

        private static MatchInfo FakeMatch(TileType type, int size)
        {
            var cells = new List<GridPosition>(size);
            for (int i = 0; i < size; i++) cells.Add(new GridPosition(0, i));
            MatchPattern pattern = size switch
            {
                3 => MatchPattern.ThreeInARow,
                4 => MatchPattern.FourInARow,
                _ => MatchPattern.FiveInARow,
            };
            return new MatchInfo(type, pattern, cells);
        }
    }
}
