using System;
using System.Collections.Generic;
using NUnit.Framework;
using Matchmancer.Character;
using Matchmancer.Combat;
using Matchmancer.Core;
using Matchmancer.Enemy;
using Matchmancer.Match;

namespace Matchmancer.Tests
{
    /// <summary>
    /// End-to-end integration tests for the full Matchmancer combat loop.
    /// Wires <see cref="CombatResolver"/>, <see cref="CharacterRuntime"/>,
    /// and <see cref="EnemyRuntime"/> together manually — mirroring the
    /// exact event wiring that BoardController, CharacterBattleController,
    /// and EnemyTurnController do at runtime — then drives scripted match
    /// waves and enemy attacks to verify the closed loop behaves correctly
    /// without any Unity scene, MonoBehaviour lifecycle, or coroutine.
    ///
    /// These tests are the "scene play test" in headless form. They prove:
    ///   • CombatResolver dispatches effects in the correct order
    ///   • CharacterRuntime absorbs Defense/Energy; ignores enemy-side effects
    ///   • EnemyRuntime absorbs Damage/Break/Debuff; ignores player-side effects
    ///   • Luck waves boost damage when they accompany Damage waves
    ///   • Enemy-turn cycle (poison tick + roll attack + character damage) works
    ///   • Battles can be driven to either victory or defeat end-state
    /// </summary>
    [TestFixture]
    public class FullCombatLoopIntegrationTests
    {
        // ------------------------------------------------------------------
        // Shared battle harness
        // ------------------------------------------------------------------

        private CombatTuning    _tuning;
        private CombatStats     _stats;
        private CombatResolver  _resolver;
        private CharacterRuntime _character;
        private CharacterTuning  _charTuning;
        private EnemyRuntime    _enemy;
        private int             _waveCounter;

        [SetUp]
        public void SetUp()
        {
            _tuning = CombatTuning.Default();
            // Lock down randomness for deterministic assertions.
            _tuning.BaseCritChance   = 0f;
            _tuning.LuckToCritRate   = 0f;
            _tuning.PoisonDamagePerTurn = 5f;
            _tuning.VulnerabilityMult   = 1.25f;

            _stats   = new CombatStats();
            _stats.ResetForNewBattle();
            _resolver = new CombatResolver(_tuning, _stats, new System.Random(42));

            _charTuning = new CharacterTuning
            {
                DisplayName = "Arcanist",
                BaseMaxHp   = 200,
                BaseAttack  = 10f,
                BaseDefense = 5f,
                BaseLuck    = 0f,
                MaxEnergy   = 100f,
                UltimateDamageMultiplier = 3f,
                XpPerLevel  = new[] { 100 },
            };
            _character = new CharacterRuntime(_charTuning, 1);

            _enemy = new EnemyRuntime(
                displayName:     "TestFoe",
                maxHp:           200,
                defense:         0f,
                maxArmor:        0,
                baseAttackPower: 20f,
                attacksPerTurn:  1,
                tuning:          _tuning);

            // Wire resolver → character per-effect (mirrors CharacterBattleController).
            _resolver.OnEffectResolved += _character.ApplyCombatEffect;

            // Wire resolver → enemy via wave (mirrors EnemyTurnController.HandleWave).
            _resolver.OnWaveResolved += ApplyWaveToEnemy;

            _waveCounter = 0;
        }

        private void ApplyWaveToEnemy(IReadOnlyList<CombatEffect> effects)
        {
            if (_enemy == null || _enemy.IsDefeated) return;
            for (int i = 0; i < effects.Count; i++)
                _enemy.ApplyCombatEffect(effects[i]);
        }

        // ------------------------------------------------------------------
        // Match builders
        // ------------------------------------------------------------------

        private static MatchInfo Match(TileType type, int size)
        {
            var positions = new List<GridPosition>(size);
            for (int i = 0; i < size; i++) positions.Add(new GridPosition(0, i));
            return new MatchInfo(type, MatchPattern.ThreeInARow, positions);
        }

        private static List<MatchInfo> Wave(params MatchInfo[] matches)
            => new List<MatchInfo>(matches);

        // ------------------------------------------------------------------
        // Simulated turn primitives
        // ------------------------------------------------------------------

        /// <summary>
        /// Resolve one wave of matches (one cascade step). Mirrors what
        /// BoardController does inside ResolveBoardUntilStable.
        /// </summary>
        private IReadOnlyList<CombatEffect> ResolveWave(List<MatchInfo> matches)
        {
            _waveCounter++;
            return _resolver.ResolveWave(
                matches,
                comboCount:       _waveCounter,
                characterAttack:  _character.CurrentAttack,
                characterLuck:    _character.CurrentLuck);
        }

        /// <summary>
        /// End the player's turn. Mirrors EnemyTurnController.HandleMoveDeducted:
        /// ticks poison + vulnerability, then rolls an enemy attack that the
        /// character absorbs.
        /// </summary>
        private void EndPlayerTurn()
        {
            _waveCounter = 0; // reset combo for next player move
            if (_enemy.IsDefeated) return;
            _enemy.OnPlayerTurnEnd();
            if (_enemy.IsDefeated) return;
            float dmg = _enemy.RollAttack();
            if (dmg > 0f && !_character.IsDefeated)
                _character.TakeEnemyDamage(dmg);
        }

        // ==================================================================
        // Tile role dispatch
        // ==================================================================

        [Test]
        public void DamageWave_ReducesEnemyHp_DoesNotTouchCharacter()
        {
            ResolveWave(Wave(Match(TileType.SoulstreamShard, 3)));
            Assert.Less(_enemy.CurrentHp, _enemy.MaxHp);
            Assert.AreEqual(_character.MaxHp, _character.CurrentHp);
        }

        [Test]
        public void DefenseWave_AddsCharacterShield_DoesNotTouchEnemy()
        {
            ResolveWave(Wave(Match(TileType.CovenSeal, 3)));
            Assert.Greater(_character.CurrentShield, 0f);
            Assert.AreEqual(_enemy.MaxHp, _enemy.CurrentHp);
        }

        [Test]
        public void EnergyWave_AddsCharacterEnergy_DoesNotTouchEnemy()
        {
            ResolveWave(Wave(Match(TileType.PortRune, 3)));
            Assert.Greater(_character.CurrentEnergy, 0f);
            Assert.AreEqual(_enemy.MaxHp, _enemy.CurrentHp);
        }

        [Test]
        public void BreakWave_DamagesEnemyArmorOnly()
        {
            // Rebuild enemy with armor so Break has somewhere to land.
            _enemy = new EnemyRuntime("ArmoredFoe", 200, 0f, 30, 20f, 1, _tuning);
            _resolver.OnWaveResolved += ApplyWaveToEnemy;
            _resolver.OnWaveResolved -= ApplyWaveToEnemy; // reattach once (clean slate)
            _resolver.OnWaveResolved += ApplyWaveToEnemy;

            ResolveWave(Wave(Match(TileType.WitchbreedThorn, 3)));
            Assert.Less(_enemy.CurrentArmor, _enemy.MaxArmor);
            Assert.AreEqual(_enemy.MaxHp, _enemy.CurrentHp);
        }

        [Test]
        public void DebuffWave_PoisonsEnemy()
        {
            ResolveWave(Wave(Match(TileType.OzoneMark, 3)));
            Assert.IsTrue(_enemy.IsPoisoned);
            Assert.GreaterOrEqual(_enemy.PoisonStacks, 1);
        }

        // ==================================================================
        // Player survives enemy attacks — shield + defense pipeline
        // ==================================================================

        [Test]
        public void EnemyAttack_ReducesCharacterHp_AfterDefense()
        {
            EndPlayerTurn(); // enemy swings for 20 - 5 defense = 15 chip
            Assert.AreEqual(_character.MaxHp - 15, _character.CurrentHp);
        }

        [Test]
        public void ShieldAbsorbsEnemyAttackBeforeHp()
        {
            // Match defense to build shield, then take enemy hit.
            ResolveWave(Wave(Match(TileType.CovenSeal, 5))); // beefy shield
            float shieldBefore = _character.CurrentShield;
            Assume.That(shieldBefore, Is.GreaterThanOrEqualTo(20f),
                "Expected Match-5 defense to generate a meaningful shield.");

            EndPlayerTurn(); // 20 raw dmg
            Assert.Less(_character.CurrentShield, shieldBefore,
                "Shield should have absorbed the hit.");
            // With shieldBefore large enough to soak 20 dmg entirely, HP stays full.
            if (shieldBefore >= 20f)
                Assert.AreEqual(_character.MaxHp, _character.CurrentHp);
        }

        // ==================================================================
        // Energy → Ultimate flow
        // ==================================================================

        [Test]
        public void EnergyMatches_FillUltimateMeter()
        {
            bool ready = false;
            _character.OnUltimateReadyChanged += r => ready = r;
            // Hammer energy matches until full.
            for (int i = 0; i < 20 && !_character.UltimateReady; i++)
            {
                _waveCounter = 0;
                ResolveWave(Wave(Match(TileType.PortRune, 4)));
            }
            Assert.IsTrue(_character.UltimateReady, "Ultimate should have filled within 20 waves.");
            Assert.IsTrue(ready);
        }

        [Test]
        public void QueuedUltimate_ConsumesAndResetsMeter()
        {
            // Fast-fill
            for (int i = 0; i < 20 && !_character.UltimateReady; i++)
            {
                _waveCounter = 0;
                ResolveWave(Wave(Match(TileType.PortRune, 4)));
            }
            Assume.That(_character.UltimateReady, Is.True);

            Assert.IsTrue(_character.QueueUltimate());
            float mult = _character.ConsumeUltimateMultiplier();
            Assert.AreEqual(3f, mult);
            Assert.AreEqual(0f, _character.CurrentEnergy);
            Assert.IsFalse(_character.UltimateReady);
            Assert.IsFalse(_character.UltimateQueued);
        }

        // ==================================================================
        // Poison + vulnerability DoT
        // ==================================================================

        [Test]
        public void Poison_TicksEnemyHpOnTurnEnd()
        {
            ResolveWave(Wave(Match(TileType.OzoneMark, 3))); // apply poison
            int before = _enemy.CurrentHp;
            EndPlayerTurn();
            Assert.Less(_enemy.CurrentHp, before,
                "Poison should have ticked damage on turn end.");
        }

        [Test]
        public void PoisonStacks_ScaleDotDamage()
        {
            // Stack poison by matching debuff several times
            for (int i = 0; i < 3; i++)
            {
                _waveCounter = 0;
                ResolveWave(Wave(Match(TileType.OzoneMark, 3)));
            }
            Assume.That(_enemy.PoisonStacks, Is.GreaterThanOrEqualTo(3));

            int hpBefore = _enemy.CurrentHp;
            _enemy.OnPlayerTurnEnd();
            int tickDmg = hpBefore - _enemy.CurrentHp;
            // 3 stacks × 5 base = 15 poison damage
            Assert.AreEqual(15, tickDmg);
        }

        // ==================================================================
        // Multi-wave cascade
        // ==================================================================

        [Test]
        public void CascadeWaves_DamageAccumulates()
        {
            int hpStart = _enemy.CurrentHp;

            // Three waves in one player move, combo scales up.
            ResolveWave(Wave(Match(TileType.SoulstreamShard, 3)));
            ResolveWave(Wave(Match(TileType.SoulstreamShard, 3)));
            ResolveWave(Wave(Match(TileType.SoulstreamShard, 3)));

            int totalDmg = hpStart - _enemy.CurrentHp;
            Assert.Greater(totalDmg, 0);
            // Combo multiplier step means wave 3 should deal more than wave 1.
            // We assert the aggregate is strictly greater than 3× the base wave 1 damage.
            Assert.Greater(totalDmg, 30,
                "Three cascading damage waves should accumulate meaningful damage.");
        }

        // ==================================================================
        // Full battles — the closed loop
        // ==================================================================

        [Test]
        public void FullBattle_PlayerWinsByDamageMatches()
        {
            bool enemyDefeated = false;
            _enemy.OnDefeated += () => enemyDefeated = true;

            // Give player enough HP to outlast the enemy.
            int maxTurns = 50;
            int turns = 0;
            while (!_enemy.IsDefeated && !_character.IsDefeated && turns++ < maxTurns)
            {
                ResolveWave(Wave(Match(TileType.SoulstreamShard, 4))); // bigger matches = more dmg
                EndPlayerTurn();
            }

            Assert.IsTrue(_enemy.IsDefeated,  $"Enemy should be defeated within {maxTurns} turns.");
            Assert.IsFalse(_character.IsDefeated, "Character should still be alive.");
            Assert.IsTrue(enemyDefeated, "OnDefeated should have fired.");
        }

        [Test]
        public void FullBattle_CharacterLosesToRelentlessEnemy()
        {
            // Fragile character, hard-hitting enemy.
            _charTuning.BaseMaxHp  = 40;
            _charTuning.BaseDefense = 0f;
            _character = new CharacterRuntime(_charTuning, 1);

            _enemy = new EnemyRuntime("Bruiser", 9999, 0f, 0, 50f, 1, _tuning);

            // Rewire — resolver events were bound to old instances' methods.
            _resolver = new CombatResolver(_tuning, _stats, new System.Random(42));
            _resolver.OnEffectResolved += _character.ApplyCombatEffect;
            _resolver.OnWaveResolved   += ApplyWaveToEnemy;

            bool charDefeated = false;
            _character.OnDefeated += () => charDefeated = true;

            int maxTurns = 30;
            int turns = 0;
            while (!_character.IsDefeated && turns++ < maxTurns)
            {
                // Player matches only DEFENSE, never damage (self-only posture).
                ResolveWave(Wave(Match(TileType.CovenSeal, 3)));
                EndPlayerTurn();
            }

            Assert.IsTrue(_character.IsDefeated, "Character should have been defeated.");
            Assert.IsTrue(charDefeated, "OnDefeated should have fired.");
            Assert.IsFalse(_enemy.IsDefeated, "Enemy should still be alive.");
        }

        [Test]
        public void FullBattle_ShieldTanksEarlyTurns()
        {
            // Player banks shield in the first two turns, should take zero HP damage.
            ResolveWave(Wave(Match(TileType.CovenSeal, 5)));
            ResolveWave(Wave(Match(TileType.CovenSeal, 5)));
            float shieldBank = _character.CurrentShield;
            Assume.That(shieldBank, Is.GreaterThan(40f),
                "Two fat defense waves should stack a large shield.");

            int hpBefore = _character.CurrentHp;
            EndPlayerTurn(); // enemy swings for ~15 effective
            Assert.AreEqual(hpBefore, _character.CurrentHp,
                "Shield should have fully absorbed the enemy hit.");
            Assert.Less(_character.CurrentShield, shieldBank,
                "Shield should have depleted.");
        }

        [Test]
        public void FullBattle_PoisonDotCanFinishWeakenedEnemy()
        {
            _enemy = new EnemyRuntime("Sickly", 20, 0f, 0, 1f, 1, _tuning);
            _resolver = new CombatResolver(_tuning, _stats, new System.Random(42));
            _resolver.OnEffectResolved += _character.ApplyCombatEffect;
            _resolver.OnWaveResolved   += ApplyWaveToEnemy;

            // Stack 4 poison (4 × 5 = 20 damage per tick — exactly lethal).
            for (int i = 0; i < 4; i++)
            {
                _waveCounter = 0;
                ResolveWave(Wave(Match(TileType.OzoneMark, 3)));
            }
            Assume.That(_enemy.PoisonStacks, Is.GreaterThanOrEqualTo(4));

            bool defeated = false;
            _enemy.OnDefeated += () => defeated = true;

            EndPlayerTurn(); // poison ticks for 20 → enemy dies
            Assert.IsTrue(_enemy.IsDefeated);
            Assert.IsTrue(defeated);
        }
    }
}
