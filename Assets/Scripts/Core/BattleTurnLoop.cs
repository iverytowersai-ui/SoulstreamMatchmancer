using System;
using System.Collections;
using UnityEngine;
using Matchmancer.Progression;
using Matchmancer.Character;
using Matchmancer.Objectives;
using Matchmancer.Combat;
using Matchmancer.Enemy;

namespace Matchmancer.Core
{
    /// <summary>
    /// Master state machine that orchestrates a full Matchmancer battle.
    ///
    /// The heavy lifting is already done by event-driven wiring:
    ///   • BoardController owns the cascade loop, combat resolver, scoring, objectives
    ///   • EnemyTurnController subscribes to BoardController.OnCombatWaveResolved
    ///     and BoardController.OnMoveDeducted to auto-apply effects and attack
    ///   • CharacterBattleController subscribes to BoardController.OnCombatEffect
    ///     and EnemyTurnController.OnEnemyAttack to auto-apply shield/damage
    ///
    /// This script adds:
    ///   1. A readable state enum so UI can react (idle, player turn, resolving, etc.)
    ///   2. Level initialization that configures all sub-systems
    ///   3. Battle result packaging when the level ends
    ///   4. Pause/resume/retry/quit flow
    ///   5. Events for the GameFlowController to consume
    /// </summary>
    public class BattleTurnLoop : MonoBehaviour
    {
        // =====================================================================
        // State
        // =====================================================================

        public enum BattleState
        {
            Idle,
            Initializing,
            PlayerTurn,
            Resolving,
            EnemyTurn,
            UltimateActive,
            Victory,
            Defeat,
            Paused
        }

        // =====================================================================
        // Inspector refs
        // =====================================================================

        [Header("Battle Systems (auto-found if null)")]
        [SerializeField] private BoardController         boardController;
        [SerializeField] private EnemyTurnController     enemyTurnController;
        [SerializeField] private CharacterBattleController characterBattleController;

        [Header("Timing")]
        [SerializeField] private float victoryDelay = 0.8f;
        [SerializeField] private float defeatDelay  = 0.5f;

        // =====================================================================
        // Runtime
        // =====================================================================

        private BattleState _state = BattleState.Idle;
        private BattleState _stateBeforePause;
        private int         _turnNumber;
        private LevelData   _currentLevel;
        private CharacterData _currentCharacter;
        private Coroutine   _activeRoutine;
        private bool        _subscribed;

        // =====================================================================
        // Events
        // =====================================================================

        /// <summary>Fires after every state transition.</summary>
        public event Action<BattleState> OnStateChanged;

        /// <summary>Fires on Victory or Defeat with the final BattleResult.</summary>
        public event Action<BattleResult> OnBattleComplete;

        /// <summary>Fires after each move is deducted (turn counter).</summary>
        public event Action<int> OnTurnComplete;

        // =====================================================================
        // Properties
        // =====================================================================

        public BattleState State => _state;
        public int TurnNumber => _turnNumber;
        public LevelData CurrentLevel => _currentLevel;

        public bool IsActive => _state != BattleState.Idle
                             && _state != BattleState.Victory
                             && _state != BattleState.Defeat
                             && _state != BattleState.Paused;

        // =====================================================================
        // Lifecycle
        // =====================================================================

        private void Awake()
        {
            if (!boardController)           boardController           = GetComponent<BoardController>();
            if (!enemyTurnController)       enemyTurnController       = GetComponent<EnemyTurnController>();
            if (!characterBattleController) characterBattleController = GetComponent<CharacterBattleController>();
        }

        private void OnDestroy() => Unsub();

        // =====================================================================
        // Public API
        // =====================================================================

        /// <summary>
        /// Initialize and start a battle. Sets up board, enemy, character,
        /// then transitions to PlayerTurn.
        /// </summary>
        public void StartBattle(LevelData level, CharacterData character)
        {
            if (level == null || character == null)
            {
                Debug.LogError("[BattleTurnLoop] Null level or character data.");
                return;
            }

            _currentLevel     = level;
            _currentCharacter = character;
            _turnNumber       = 0;

            SetState(BattleState.Initializing);

            // 1. Initialize board via the existing pipeline
            var config = level.ToLevelConfig();
            boardController.InitializeLevel(config);

            // 2. Rebuild enemy runtime from level's EnemyData
            //    (EnemyTurnController auto-subscribes to BoardController in OnEnable,
            //     but RebuildRuntime resets HP for a new fight)
            if (enemyTurnController != null)
                enemyTurnController.RebuildRuntime();

            // 3. Wire character stats into combat resolver
            //    (CharacterBattleController builds its own runtime in OnEnable,
            //     but we ensure boardController uses live stats)
            if (characterBattleController != null)
                boardController.CharacterStatsSource = characterBattleController;

            Sub();

            Debug.Log($"[BattleTurnLoop] Battle started — {level.displayName} with {character.displayName}");
            SetState(BattleState.PlayerTurn);
        }

        /// <summary>Pause during an active battle.</summary>
        public void Pause()
        {
            if (!IsActive) return;
            _stateBeforePause = _state;
            SetState(BattleState.Paused);
        }

        /// <summary>Resume from pause.</summary>
        public void Resume()
        {
            if (_state != BattleState.Paused) return;
            SetState(_stateBeforePause);
        }

        /// <summary>Retry the current level.</summary>
        public void Retry()
        {
            Cleanup();
            StartBattle(_currentLevel, _currentCharacter);
        }

        /// <summary>Quit the battle.</summary>
        public void Quit()
        {
            var result = BuildResult(false);
            Cleanup();
            SetState(BattleState.Idle);
            OnBattleComplete?.Invoke(result);
        }

        // =====================================================================
        // State machine
        // =====================================================================

        private void SetState(BattleState next)
        {
            if (_state == next) return;
            var prev = _state;
            _state = next;
            Debug.Log($"[BattleTurnLoop] {prev} → {next}");
            OnStateChanged?.Invoke(next);

            switch (next)
            {
                case BattleState.Victory:
                    _activeRoutine = StartCoroutine(EndSequence(true));
                    break;
                case BattleState.Defeat:
                    _activeRoutine = StartCoroutine(EndSequence(false));
                    break;
            }
        }

        // =====================================================================
        // Event wiring
        // =====================================================================

        private void Sub()
        {
            if (_subscribed) Unsub();
            _subscribed = true;

            // BoardController fires OnLevelComplete when ObjectiveChecker says Victory/Defeat
            if (boardController != null)
            {
                boardController.OnLevelComplete += HandleLevelComplete;
                boardController.OnMoveDeducted  += HandleMoveDeducted;
                boardController.OnSwapPerformed += HandleSwapStarted;
            }

            // EnemyTurnController fires OnEnemyDefeated when HP reaches 0
            if (enemyTurnController != null)
                enemyTurnController.OnEnemyDefeated += HandleEnemyDefeated;
        }

        private void Unsub()
        {
            if (!_subscribed) return;
            _subscribed = false;

            if (boardController != null)
            {
                boardController.OnLevelComplete -= HandleLevelComplete;
                boardController.OnMoveDeducted  -= HandleMoveDeducted;
                boardController.OnSwapPerformed -= HandleSwapStarted;
            }

            if (enemyTurnController != null)
                enemyTurnController.OnEnemyDefeated -= HandleEnemyDefeated;
        }

        // =====================================================================
        // Event handlers
        // =====================================================================

        private void HandleSwapStarted(GridPosition a, GridPosition b)
        {
            if (_state == BattleState.PlayerTurn)
                SetState(BattleState.Resolving);
        }

        private void HandleMoveDeducted(int movesRemaining)
        {
            _turnNumber++;
            OnTurnComplete?.Invoke(_turnNumber);

            // After the move is deducted, EnemyTurnController auto-attacks
            // via its own subscription to OnMoveDeducted. We just track state.
            if (_state == BattleState.Resolving)
                SetState(BattleState.EnemyTurn);

            // Check if character was killed by enemy attack
            if (characterBattleController?.Runtime != null
                && characterBattleController.Runtime.IsDefeated)
            {
                SetState(BattleState.Defeat);
                return;
            }

            // Return to player turn if battle still active
            if (_state == BattleState.EnemyTurn)
                SetState(BattleState.PlayerTurn);
        }

        private void HandleLevelComplete(LevelResult result, int starCount)
        {
            if (result == LevelResult.Victory)
                SetState(BattleState.Victory);
            else if (result == LevelResult.Defeat)
                SetState(BattleState.Defeat);
        }

        private void HandleEnemyDefeated()
        {
            if (_state != BattleState.Victory && _state != BattleState.Defeat)
                SetState(BattleState.Victory);
        }

        // =====================================================================
        // End sequence
        // =====================================================================

        private IEnumerator EndSequence(bool victory)
        {
            yield return new WaitForSeconds(victory ? victoryDelay : defeatDelay);
            var result = BuildResult(victory);
            Debug.Log($"[BattleTurnLoop] {(victory ? "Victory" : "Defeat")} — {result}");
            OnBattleComplete?.Invoke(result);
        }

        // =====================================================================
        // Helpers
        // =====================================================================

        private BattleResult BuildResult(bool victory)
        {
            var stats = boardController?.CombatStats;
            var rt    = characterBattleController?.Runtime;

            return new BattleResult
            {
                GlobalLevelIndex = _currentLevel != null ? _currentLevel.globalIndex : 0,
                Victory          = victory,
                FinalScore       = boardController?.Score ?? 0,
                MovesUsed        = _currentLevel != null
                    ? _currentLevel.totalMoves - (boardController?.MovesRemaining ?? 0) : 0,
                MovesRemaining   = boardController?.MovesRemaining ?? 0,
                TurnsTaken       = _turnNumber,
                HpRemaining      = rt?.CurrentHp ?? 0,
                MaxHp            = rt?.MaxHp ?? 0,
                MaxComboAchieved = stats?.MaxCombo ?? 0,
                DamageDealt      = (int)(stats?.TotalDamageDealt ?? 0),
                DamageTaken      = 0,
            };
        }

        private void Cleanup()
        {
            Unsub();
            if (_activeRoutine != null)
            {
                StopCoroutine(_activeRoutine);
                _activeRoutine = null;
            }
        }
    }
}
