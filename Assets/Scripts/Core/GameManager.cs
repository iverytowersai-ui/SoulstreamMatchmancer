using UnityEngine;
using Matchmancer.View;

namespace Matchmancer.Core
{
    /// <summary>
    /// Scene bootstrap. Wires up all systems and loads a level.
    /// Attach to a root GameObject in the scene.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BoardController _boardController;
        [SerializeField] private BoardView _boardView;
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private HUDView _hudView;

        [Header("Level")]
        [SerializeField] private int _levelToLoad = 1;

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

            _boardController.InitializeLevel(config);
            _boardView.Initialize(_boardController);
            _inputHandler.Initialize(_boardController, _boardView);

            string objectiveDesc = config.Objective.Type switch
            {
                Objectives.ObjectiveType.ReachScore => $"Reach {config.Objective.TargetScore} points",
                Objectives.ObjectiveType.ClearAllStones => "Clear all Stone Blocks",
                Objectives.ObjectiveType.Survive => $"Survive {config.Objective.SurviveTurns} turns",
                _ => "Complete the objective"
            };

            _hudView.Initialize(_boardController, objectiveDesc);

            _boardController.OnLevelComplete += OnLevelComplete;
        }

        private void OnLevelComplete(Objectives.LevelResult result, int stars)
        {
            _boardController.OnLevelComplete -= OnLevelComplete;

            if (result == Objectives.LevelResult.Victory)
            {
                Debug.Log($"Victory! {stars} stars. Next level: {_levelToLoad + 1}");
                // TODO: Show result screen, then load next level
            }
            else
            {
                Debug.Log("Defeat. Try again.");
                // TODO: Show retry screen
            }
        }
    }
}
