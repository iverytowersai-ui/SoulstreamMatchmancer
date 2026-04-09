using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Matchmancer.Board;
using Matchmancer.Match;
using Matchmancer.Sigils;
using Matchmancer.Meter;
using Matchmancer.Dice;
using Matchmancer.StoneBlocks;
using Matchmancer.Objectives;
using Matchmancer.Combat;

namespace Matchmancer.Core
{
    /// <summary>
    /// Orchestrates the entire Core Game Loop. Attach to a GameObject in the scene.
    /// This is the single authority for turn order — no other script drives the loop.
    /// </summary>
    public class BoardController : MonoBehaviour
    {
        // === Inspector ===
        [Header("Combat")]
        [Tooltip("Optional tuning asset. If null, CombatTuning.Default() is used.")]
        [SerializeField] private CombatConfig _combatConfig;

        // === Systems ===
        private Board.Board _board;
        private MatchDetector _matchDetector;
        private SwapValidator _swapValidator;
        private SigilSystem _sigilSystem;
        private GravityHandler _gravityHandler;
        private MagickMeter _magickMeter;
        private DiceSystem _diceSystem;
        private StoneBlockSystem _stoneBlockSystem;
        private MoveTracker _moveTracker;
        private Scoring _scoring;
        private ObjectiveChecker _objectiveChecker;

        // === Combat (Skill 12) ===
        private CombatResolver _combatResolver;
        private CombatStats    _combatStats;

        private BossMechanic _bossMechanic; // null for non-boss levels
        private int _turnCount;
        private System.Random _rng;
        private bool _isTurnInProgress;

        // === Events for presentation layer ===
        public event Action<GridPosition, GridPosition> OnSwapPerformed;
        public event Action<GridPosition, GridPosition> OnSwapReversed;
        public event Action<List<MatchInfo>> OnMatchesFound;
        public event Action<GridPosition, SigilType> OnSigilCreated;
        public event Action<GridPosition, SigilType, List<GridPosition>> OnSigilActivated;
        public event Action<List<(GridPosition from, GridPosition to)>> OnTilesDropped;
        public event Action<List<GridPosition>> OnTilesRefilled;
        public event Action<List<GridPosition>> OnStonesDestroyed;
        public event Action<int> OnMeterChanged;
        public event Action<DiceRollResult> OnDiceRolled;
        public event Action<List<GridPosition>> OnDiceEffectApplied;
        public event Action<List<GridPosition>> OnBossStonesSpawned;
        public event Action<int> OnMoveDeducted;
        public event Action<LevelResult, int> OnLevelComplete; // result + stars

        // === Combat events (Skill 12) ===
        /// <summary>Fired for each CombatEffect produced by a match wave. Skill 13 will consume this.</summary>
        public event Action<CombatEffect> OnCombatEffect;
        /// <summary>Fired after all effects for one match wave have been dispatched.</summary>
        public event Action<IReadOnlyList<CombatEffect>> OnCombatWaveResolved;

        // === Character stats seam (Skill 14) ===
        /// <summary>
        /// Optional live player-stat provider. When set, its Attack/Luck are
        /// passed into <see cref="CombatResolver.ResolveWave"/> each wave.
        /// When null, the resolver falls back to its default (10, 0).
        /// Assigned by <c>CharacterBattleController</c> at battle start.
        /// </summary>
        public ICharacterStatsSource CharacterStatsSource { get; set; }

        // === Public State (read-only for UI) ===
        public Board.Board Board => _board;
        public int MovesRemaining => _moveTracker.MovesRemaining;
        public int Score => _scoring.Score;
        public int MeterCharge => _magickMeter.CurrentCharge;
        public int MeterMax => _magickMeter.MaxCapacity;
        public bool IsBusy => _isTurnInProgress;
        /// <summary>Live battle stats for the current level. Rebuilt on every InitializeLevel.</summary>
        public CombatStats CombatStats => _combatStats;

        /// <summary>
        /// Initialize all systems. Call this when loading a level.
        /// </summary>
        public void InitializeLevel(LevelConfig config)
        {
            _rng = new System.Random();
            _board = new Board.Board();
            _board.Initialize(() => GetRandomTileType());

            _matchDetector = new MatchDetector(_board);
            _swapValidator = new SwapValidator(_board, _matchDetector);
            _sigilSystem = new SigilSystem(_board);
            _gravityHandler = new GravityHandler(_board, () => GetRandomTileType());
            _magickMeter = new MagickMeter(config.MeterCapacity);
            _diceSystem = new DiceSystem(_board, _rng);
            _stoneBlockSystem = new StoneBlockSystem(_board);
            _moveTracker = new MoveTracker(config.TotalMoves);
            _scoring = new Scoring(config.OneStar, config.TwoStar, config.ThreeStar, config.FourStar, config.FiveStar);
            _objectiveChecker = new ObjectiveChecker(config.Objective, _scoring, _moveTracker, _stoneBlockSystem);

            // === Combat setup (Skill 12) ===
            _combatStats = new CombatStats();
            _combatStats.ResetForNewBattle();

            // Detach old resolver (if this is a level restart) so we don't leak events.
            if (_combatResolver != null)
            {
                _combatResolver.OnEffectResolved -= RaiseCombatEffect;
                _combatResolver.OnWaveResolved   -= RaiseCombatWaveResolved;
            }

            var tuning = _combatConfig != null ? _combatConfig.ToTuning() : CombatTuning.Default();
            _combatResolver = new CombatResolver(tuning, _combatStats, _rng);

            // Forward resolver events up to BoardController's public surface so
            // presentation-layer listeners (EnemyController in Skill 13, BattleUI, etc.)
            // can subscribe to BoardController directly and ignore the resolver's
            // internal lifecycle.
            _combatResolver.OnEffectResolved += RaiseCombatEffect;
            _combatResolver.OnWaveResolved   += RaiseCombatWaveResolved;

            // Place stone blocks from level config
            foreach (var stone in config.StoneBlocks)
            {
                _stoneBlockSystem.PlaceStone(stone.Row, stone.Col, stone.HP);
            }

            // Boss mechanic for Level 10 (Survive objective with stones)
            _turnCount = 0;
            if (config.Objective.Type == Objectives.ObjectiveType.Survive && config.StoneBlocks.Count > 0)
            {
                _bossMechanic = new BossMechanic(_board, _stoneBlockSystem, spreadInterval: 3, spreadHP: 1);
            }
            else
            {
                _bossMechanic = null;
            }

            // Clear any pre-existing matches so the board starts stable
            ClearInitialMatches();

            _isTurnInProgress = false;
        }

        /// <summary>
        /// Player input: attempt a swap. This is the ONLY entry point to the turn loop.
        /// </summary>
        public void TrySwap(GridPosition a, GridPosition b)
        {
            if (_isTurnInProgress) return;

            if (!_swapValidator.IsValidSwap(a, b))
            {
                // Invalid swap — animate reversal, cost 0 moves
                OnSwapReversed?.Invoke(a, b);
                return;
            }

            StartCoroutine(ExecuteTurn(a, b));
        }

        /// <summary>
        /// The authoritative turn sequence. Runs exactly in the order specified.
        /// </summary>
        private IEnumerator ExecuteTurn(GridPosition a, GridPosition b)
        {
            _isTurnInProgress = true;
            _scoring.ResetCascade();

            // Step 1: Perform the swap
            _board.SwapTiles(a, b);
            OnSwapPerformed?.Invoke(a, b);
            yield return new WaitForSeconds(0.2f); // swap animation window

            // Check if this swap activates a Sigil
            bool sigilSwap = false;
            var tileA = _board[a];
            var tileB = _board[b];

            if (tileA.IsSigil)
            {
                yield return ResolveSigilActivation(a);
                sigilSwap = true;
            }
            if (tileB.IsSigil)
            {
                yield return ResolveSigilActivation(b);
                sigilSwap = true;
            }

            if (sigilSwap)
            {
                // After sigil activation, run gravity + cascade
                yield return GravityRefillCascade();
            }
            else
            {
                // Step 2: Standard match resolution + cascade loop
                yield return ResolveBoardUntilStable();
            }

            // Step 2j: Deduct exactly 1 move
            _moveTracker.DeductMove();
            OnMoveDeducted?.Invoke(_moveTracker.MovesRemaining);

            // Step 3: Check Magick Meter (at most once per turn)
            if (_magickMeter.IsFull)
            {
                yield return ResolveDiceRollPhase();
            }

            // Step 4: Check win/lose
            _objectiveChecker.IncrementTurn();
            _turnCount++;

            // Boss mechanic: spread stones every N turns
            if (_bossMechanic != null)
            {
                var newStones = _bossMechanic.OnTurnEnd(_turnCount);
                if (newStones.Count > 0)
                {
                    OnBossStonesSpawned?.Invoke(newStones);
                    yield return new WaitForSeconds(0.3f);
                }
            }

            var result = _objectiveChecker.Evaluate();

            if (result != LevelResult.InProgress)
            {
                if (result == LevelResult.Victory)
                    _scoring.AddRemainingMovesBonus(_moveTracker.MovesRemaining);

                int stars = result == LevelResult.Victory ? _scoring.CalculateStars() : 0;
                OnLevelComplete?.Invoke(result, stars);
            }

            _isTurnInProgress = false;
        }

        /// <summary>
        /// Step 2a-i: Find matches, resolve them, gravity, refill, repeat until stable.
        /// </summary>
        private IEnumerator ResolveBoardUntilStable()
        {
            int waveIndex = 0;

            while (true)
            {
                // Step 2a-b: Find matches
                var matches = _matchDetector.FindAllMatches();
                if (matches.Count == 0) break;

                waveIndex++;
                OnMatchesFound?.Invoke(matches);

                // Skill 12: resolve combat effects for this wave BEFORE tiles are removed,
                // so presentation-layer listeners (Skill 13 enemy, VFX) still see valid
                // tile positions. Enemy defense is applied by the listener, not here.
                _combatResolver?.ResolveWave(
                    matches,
                    comboCount: waveIndex,
                    characterAttack: CharacterStatsSource?.Attack ?? 10f,
                    characterLuck:   CharacterStatsSource?.Luck   ?? 0f);

                yield return new WaitForSeconds(0.15f); // match highlight window

                foreach (var match in matches)
                {
                    // Step 2a: Remove matched tiles (except sigil spawn position)
                    var positionsToRemove = new List<GridPosition>(match.Positions);

                    // Step 2b: Create Sigils for qualifying patterns
                    if (match.Pattern != MatchPattern.ThreeInARow && match.SigilSpawnPosition.HasValue)
                    {
                        _sigilSystem.CreateSigilFromMatch(match);
                        positionsToRemove.Remove(match.SigilSpawnPosition.Value);
                        OnSigilCreated?.Invoke(match.SigilSpawnPosition.Value,
                            match.Pattern == MatchPattern.FourInARow ? SigilType.Line :
                            match.Pattern == MatchPattern.FiveInARow ? SigilType.Star : SigilType.Nova);
                    }

                    foreach (var pos in positionsToRemove)
                    {
                        // Check if clearing this tile activates a Sigil
                        if (_board[pos].IsSigil)
                        {
                            yield return ResolveSigilActivation(pos);
                        }
                        _board.ClearTile(pos);
                    }

                    // Step 2c-d: Score
                    _scoring.AddMatchScore(match.TileCount);

                    // Step 2e: Charge Magick Meter
                    _magickMeter.AddCharge(match.MeterCharge);
                    OnMeterChanged?.Invoke(_magickMeter.CurrentCharge);

                    // Step 2f: Damage adjacent Stone Blocks
                    var destroyed = _stoneBlockSystem.DamageAdjacentStones(match.Positions);
                    if (destroyed.Count > 0)
                    {
                        OnStonesDestroyed?.Invoke(destroyed);
                        // Clear destroyed stone positions so gravity can fill them
                        foreach (var pos in destroyed)
                            _board.ClearTile(pos);
                    }
                }

                yield return new WaitForSeconds(0.1f); // removal animation window

                // Step 2g-h: Gravity + refill
                yield return ApplyGravityAndRefill();

                // Step 2i: Cascade — increment cascade counter and loop
                _scoring.IncrementCascade();
            }

            _scoring.ResetCascade();
        }

        private IEnumerator ResolveSigilActivation(GridPosition pos)
        {
            var tile = _board[pos];
            if (!tile.IsSigil) yield break;

            var sigilType = tile.Sigil;
            var cleared = _sigilSystem.ActivateSigil(pos);

            // Clear the sigil tile itself
            _board.ClearTile(pos);

            OnSigilActivated?.Invoke(pos, sigilType, cleared);
            _scoring.AddSigilActivationScore();
            _magickMeter.AddSigilActivationCharge();
            OnMeterChanged?.Invoke(_magickMeter.CurrentCharge);

            // Clear affected tiles
            foreach (var target in cleared)
            {
                // Chain: if cleared tile is also a sigil, activate it too
                if (_board[target].IsSigil)
                {
                    yield return ResolveSigilActivation(target);
                }
                else
                {
                    _board.ClearTile(target);
                }
            }

            // Damage stones adjacent to all cleared positions
            var allCleared = new List<GridPosition>(cleared) { pos };
            var destroyedStones = _stoneBlockSystem.DamageAdjacentStones(allCleared);
            if (destroyedStones.Count > 0)
            {
                OnStonesDestroyed?.Invoke(destroyedStones);
                foreach (var stonePos in destroyedStones)
                    _board.ClearTile(stonePos);
            }

            yield return new WaitForSeconds(0.2f); // sigil effect animation
        }

        /// <summary>
        /// Gravity + refill + cascade until stable. Reused by both normal matches and dice effects.
        /// </summary>
        private IEnumerator GravityRefillCascade()
        {
            yield return ApplyGravityAndRefill();
            _scoring.IncrementCascade();
            yield return ResolveBoardUntilStable();
        }

        private IEnumerator ApplyGravityAndRefill()
        {
            var drops = _gravityHandler.ApplyGravity();
            if (drops.Count > 0)
            {
                OnTilesDropped?.Invoke(drops);
                yield return new WaitForSeconds(0.2f); // drop animation
            }

            var refilled = _gravityHandler.RefillBoard();
            if (refilled.Count > 0)
            {
                OnTilesRefilled?.Invoke(refilled);
                yield return new WaitForSeconds(0.15f); // refill animation
            }
        }

        /// <summary>
        /// Step 3: Dice Roll phase. Triggers at most once per turn.
        /// </summary>
        private IEnumerator ResolveDiceRollPhase()
        {
            // 3a: Roll
            var rollResult = _diceSystem.Roll();
            OnDiceRolled?.Invoke(rollResult);
            yield return new WaitForSeconds(0.5f); // dice animation

            // 3b: Apply effect — clear affected tiles
            foreach (var pos in rollResult.AffectedPositions)
            {
                if (_board.IsPlayable(pos.Row, pos.Col) && !_board[pos].IsEmpty)
                    _board.ClearTile(pos);
            }
            OnDiceEffectApplied?.Invoke(rollResult.AffectedPositions);
            yield return new WaitForSeconds(0.2f);

            // Damage stones adjacent to cleared positions
            var destroyedStones = _stoneBlockSystem.DamageAdjacentStones(rollResult.AffectedPositions);
            if (destroyedStones.Count > 0)
            {
                OnStonesDestroyed?.Invoke(destroyedStones);
                foreach (var pos in destroyedStones)
                    _board.ClearTile(pos);
            }

            // 3c: Gravity + cascade from dice effect
            yield return GravityRefillCascade();

            // 3d: Reset meter
            _magickMeter.Reset();
            OnMeterChanged?.Invoke(0);
        }

        private TileType GetRandomTileType()
        {
            var types = new[] {
                TileType.PortRune, TileType.OzoneMark, TileType.CovenSeal,
                TileType.WitchbreedThorn, TileType.SoulstreamShard, TileType.PetshaCharm
            };
            return types[_rng.Next(types.Length)];
        }

        // ------------------------------------------------------------------
        // Combat event forwarding (Skill 12)
        // ------------------------------------------------------------------

        private void RaiseCombatEffect(CombatEffect effect)
            => OnCombatEffect?.Invoke(effect);

        private void RaiseCombatWaveResolved(IReadOnlyList<CombatEffect> effects)
            => OnCombatWaveResolved?.Invoke(effects);

        private void OnDestroy()
        {
            if (_combatResolver != null)
            {
                _combatResolver.OnEffectResolved -= RaiseCombatEffect;
                _combatResolver.OnWaveResolved   -= RaiseCombatWaveResolved;
            }
        }

        /// <summary>
        /// Remove matches from the initial board so the player starts with no free cascades.
        /// </summary>
        private void ClearInitialMatches()
        {
            int safetyCounter = 0;
            while (safetyCounter < 100)
            {
                var matches = _matchDetector.FindAllMatches();
                if (matches.Count == 0) break;

                foreach (var match in matches)
                {
                    foreach (var pos in match.Positions)
                    {
                        if (!_board.IsStoneBlock(pos.Row, pos.Col))
                        {
                            _board.SetTile(pos, new Tile(GetRandomTileType(), pos));
                        }
                    }
                }
                safetyCounter++;
            }
        }
    }
}
