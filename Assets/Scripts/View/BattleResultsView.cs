using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Matchmancer.Core;
using Matchmancer.Progression;

namespace Matchmancer.View
{
    /// <summary>
    /// End-of-battle results overlay. Shows Victory / Defeat headline,
    /// star rating (1–5 filled icons), score, and Continue button.
    ///
    /// Setup:
    ///   1. Create a full-screen overlay panel (child of the Canvas),
    ///      default inactive.
    ///   2. Wire child elements to the slots below.
    ///   3. <see cref="BattleUIOrchestrator"/> calls
    ///      <see cref="Show(BattleResult, StarResult)"/> when the battle
    ///      ends.
    ///   4. The Continue button fires <see cref="OnContinueClicked"/> so
    ///      the orchestrator can transition to the level-select / hub.
    /// </summary>
    public class BattleResultsView : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject _panel;

        [Header("Outcome")]
        [SerializeField] private TextMeshProUGUI _outcomeText;
        [SerializeField] private Color _victoryColor = new Color(1f, 0.85f, 0.1f, 1f);
        [SerializeField] private Color _defeatColor  = new Color(0.8f, 0.15f, 0.15f, 1f);

        [Header("Stars (assign 5 Image elements in order)")]
        [SerializeField] private Image[] _starImages;
        [SerializeField] private Sprite  _starFilled;
        [SerializeField] private Sprite  _starEmpty;

        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _comboText;
        [SerializeField] private TextMeshProUGUI _hpText;

        [Header("Continue")]
        [SerializeField] private Button _continueButton;

        public event System.Action OnContinueClicked;

        private void Awake()
        {
            if (_panel != null) _panel.SetActive(false);
            if (_continueButton != null)
                _continueButton.onClick.AddListener(() => OnContinueClicked?.Invoke());
        }

        /// <summary>
        /// Reveal the results overlay. Pass the raw <see cref="BattleResult"/>
        /// and evaluated <see cref="StarResult"/> from the rating pipeline.
        /// </summary>
        public void Show(BattleResult result, StarResult stars)
        {
            if (_panel != null) _panel.SetActive(true);

            // Headline
            if (_outcomeText != null)
            {
                _outcomeText.text  = result.Victory ? "VICTORY" : "DEFEAT";
                _outcomeText.color = result.Victory ? _victoryColor : _defeatColor;
            }

            // Stars
            if (_starImages != null)
            {
                for (int i = 0; i < _starImages.Length; i++)
                {
                    if (_starImages[i] == null) continue;
                    bool filled = i < stars.Stars;
                    _starImages[i].sprite = filled ? _starFilled : _starEmpty;
                    _starImages[i].color  = filled
                        ? Color.white
                        : new Color(1f, 1f, 1f, 0.3f);
                }
            }

            // Stat lines
            if (_scoreText != null)
                _scoreText.text = $"Score: {result.FinalScore:N0}";
            if (_comboText != null)
                _comboText.text = $"Best Combo: ×{result.MaxComboAchieved}";
            if (_hpText != null)
                _hpText.text = result.Victory
                    ? $"HP: {result.HpRemaining}/{result.MaxHp}"
                    : "HP: 0";
        }

        /// <summary>Hide the panel (e.g. before starting the next level).</summary>
        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_continueButton != null) _continueButton.onClick.RemoveAllListeners();
        }
    }
}
