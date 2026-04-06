using UnityEngine;
using TMPro;
using Matchmancer.Core;
using Matchmancer.Objectives;

namespace Matchmancer.View
{
    /// <summary>
    /// Displays score, moves remaining, and Magick Meter.
    /// Attach to a Canvas with child Text/Image elements.
    /// </summary>
    public class HUDView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _movesText;
        [SerializeField] private UnityEngine.UI.Image _meterFill;
        [SerializeField] private TextMeshProUGUI _meterText;
        [SerializeField] private TextMeshProUGUI _objectiveText;

        private BoardController _controller;

        public void Initialize(BoardController controller, string objectiveDescription)
        {
            _controller = controller;

            _controller.OnMoveDeducted += UpdateMoves;
            _controller.OnMeterChanged += UpdateMeter;
            _controller.OnMatchesFound += _ => UpdateScore();
            _controller.OnLevelComplete += HandleLevelComplete;

            if (_objectiveText != null)
                _objectiveText.text = objectiveDescription;

            UpdateScore();
            UpdateMoves(_controller.MovesRemaining);
            UpdateMeter(_controller.MeterCharge);
        }

        private void UpdateScore()
        {
            if (_scoreText != null)
                _scoreText.text = $"Score: {_controller.Score}";
        }

        private void UpdateMoves(int remaining)
        {
            if (_movesText != null)
                _movesText.text = $"Moves: {remaining}";
        }

        private void UpdateMeter(int charge)
        {
            if (_meterFill != null)
                _meterFill.fillAmount = (float)charge / _controller.MeterMax;

            if (_meterText != null)
                _meterText.text = $"Magick: {charge}/{_controller.MeterMax}";
        }

        private void HandleLevelComplete(LevelResult result, int stars)
        {
            // Placeholder — show result panel
            Debug.Log($"Level {(result == LevelResult.Victory ? "WON" : "LOST")} — {stars} stars");
        }

        private void OnDestroy()
        {
            if (_controller == null) return;
            _controller.OnMoveDeducted -= UpdateMoves;
            _controller.OnMeterChanged -= UpdateMeter;
            _controller.OnLevelComplete -= HandleLevelComplete;
        }
    }
}
