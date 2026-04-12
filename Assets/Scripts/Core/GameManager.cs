using UnityEngine;
using Matchmancer.View;
using Matchmancer.Enemy;
using Matchmancer.Character;
using Matchmancer.Progression;
using Matchmancer.Objectives;

namespace Matchmancer.Core
{
    /// <summary>
    /// Scene bootstrap. Wires up all systems and loads a level.
    /// Handles the full battle lifecycle: board → combat → enemy turn →
    /// win/lose → results screen → next level / retry.
    /// Attach to a root GameObject in the scene.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Board")]
        [SerializeField] private BoardController _boardController;
        [SerializeField] private BoardView       _boardView;
        [SerializeField] private InputHandler    _inputHandler;
        [SerializeField] private HUDView         _hudView;

        [Header("Combat")]
        [SerializeField] private EnemyTurnController      _enemyTurnController;
        [SerializeField] private CharacterBattleController _characterBattleController;
        [SerializeField] private GearInventoryController   _gearInventoryController;

        [Header("Results")]
        [SerializeField] private ResultsController _resultsController;
        [SerializeField] private BattleUIOrchestrator _battleUIOrchestrator;

        [Header("Level")]
        [SerializeField] private int _levelToLoad = 1;

        public int CurrentLevel => _levelToLoad;

        private void Start()
        {
            LoadLevel(_levelToLoad);
        }

        public void LoadLevel(int levelNumber)
        {
            var config = Stage1Data.GetLevel(levelNumber);
            if (config == null)
            {
                Debug.LogError($"Level {levelNumber} not found in Stage 1 data.");
                return;
            }

            _levelToLoad = levelNumber;

            // ── Board + visuals ──────────────────────────────────
            _boardController.InitializeLevel(config);
            _boardView.Initialize(_boardController);
            _inputHandler.Initialize(_boardController, _boardView);

            string objectiveDesc = config.Objective.Type switch
            {
                ObjectiveType.ReachScore     => $"Reach {config.Objective.TargetScore} points",
                ObjectiveType.ClearAllStones => "Clear all Stone Blocks",
                ObjectiveType.Survive        => $"Survive {config.Objective.SurviveTurns} turns",
                _                            => "Complete the objective"
            };
            _hudView.Initialize(_boardController, objectiveDesc);

            // ── Combat controllers ───────────────────────────────
            // These self-wire to BoardController in their OnEnable(),
            // but we need to rebuild their runtimes for the new level.
            if (_enemyTurnController != null)
                _enemyTurnController.RebuildRuntime();
            if (_characterBattleController != null)
                _characterBattleController.RebuildRuntime();
            if (_gearInventoryController != null)
                _gearInventoryController.ReapplyToCurrentRuntime();

            // ── Battle UI ────────────────────────────────────────
            if (_battleUIOrchestrator != null)
                _battleUIOrchestrator.InitializeBattle();

            // ── Subscribe to battle end ──────────────────────────
            _boardController.OnLevelComplete -= OnLevelComplete;
            _boardController.OnLevelComplete += OnLevelComplete;

            if (_enemyTurnController != null)
            {
                _enemyTurnController.OnEnemyDefeated -= OnEnemyDefeated;
                _enemyTurnController.OnEnemyDefeated += OnEnemyDefeated;
            }

            Debug.Log($"<color=#00CCFF>[Matchmancer]</color> Level {levelNumber} loaded — {objectiveDesc}");
        }

        // =================================================================
        // Battle end handlers
        // =================================================================

        private void OnLevelComplete(LevelResult result, int stars)
        {
            // Board objective resolved (score / stones / survive).
            // The results controller (if present) will evaluate stars
            // and record progression; the BattleResultsView shows the overlay.
            if (_resultsController != null)
            {
                var battleResult = BuildBattleResult(result == LevelResult.Victory, stars);
                var levelConfig  = Stage1Data.GetLevel(_levelToLoad);
                if (levelConfig != null)
                    _resultsController.ReportBattleResult(battleResult, levelConfig);
            }

            if (result == LevelResult.Victory)
            {
                Debug.Log($"<color=#00FF88>[Matchmancer]</color> Victory! {stars} star(s). " +
                          $"Next level: {_levelToLoad + 1}");
            }
            else
            {
                Debug.Log("<color=#FF4444>[Matchmancer]</color> Defeat. Try again.");
            }
        }

        private void OnEnemyDefeated()
        {
            // Enemy HP hit 0 via combat effects — treat as victory.
            // BoardController's objective check may not catch this (it checks
            // score / stones / survive), so we handle it here as a supplemental
            // win condition for the RPG combat layer.
            Debug.Log("<color=#00FF88>[Matchmancer]</color> Enemy defeated!");

            if (_resultsController != null)
            {
                var battleResult = BuildBattleResult(true, 0);
                var levelConfig  = Stage1Data.GetLevel(_levelToLoad);
                if (levelConfig != null)
                {
                    var starResult = _resultsController.ReportBattleResult(battleResult, levelConfig);
                    Debug.Log($"<color=#00FF88>[Matchmancer]</color> {starResult.Stars} star(s)!");
                }
            }
        }

        private BattleResult BuildBattleResult(bool victory, int fallbackStars)
        {
            var result = new BattleResult
            {
                GlobalLevelIndex = _levelToLoad,
                Victory          = victory,
                FinalScore       = _boardController != null ? _boardController.Score : 0,
            };

            if (_characterBattleController != null && _characterBattleController.Runtime != null)
            {
                result.HpRemaining = _characterBattleController.Runtime.CurrentHp;
                result.MaxHp       = _characterBattleController.Runtime.MaxHp;
            }

            if (_boardController?.CombatStats != null)
            {
                result.MaxComboAchieved = _boardController.CombatStats.MaxCombo;
                result.DamageDealt      = (int)_boardController.CombatStats.TotalDamageDealt;
            }

            result.MovesRemaining = _boardController != null ? _boardController.MovesRemaining : 0;

            return result;
        }

        // =================================================================
        // Public controls (for results screen buttons)
        // =================================================================

        /// <summary>Retry the current level.</summary>
        public void RetryLevel()
        {
            LoadLevel(_levelToLoad);
        }

        /// <summary>Advance to the next level.</summary>
        public void NextLevel()
        {
            LoadLevel(_levelToLoad + 1);
        }
    }
}
