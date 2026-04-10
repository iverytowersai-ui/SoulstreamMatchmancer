using UnityEngine;
using TMPro;

namespace Matchmancer.View
{
    /// <summary>
    /// Brief "Combo x3!" banner that pops in, holds, then fades out.
    /// <see cref="BattleUIOrchestrator"/> calls <see cref="ShowCombo(int)"/>
    /// after each cascade wave.
    /// </summary>
    public class ComboBannerView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _comboText;
        [SerializeField] private CanvasGroup     _canvasGroup;

        [Header("Timing")]
        [SerializeField] private float _holdDuration = 0.8f;
        [SerializeField] private float _fadeDuration = 0.4f;

        [Header("Scale Punch")]
        [SerializeField] private float _punchScale   = 1.4f;
        [SerializeField] private float _punchSpeed   = 10f;

        private float  _timer;
        private bool   _showing;

        private void Awake()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            Hide();
        }

        public void ShowCombo(int comboCount)
        {
            if (comboCount < 2) return; // Don't banner a 1-combo.

            if (_comboText != null)
                _comboText.text = $"Combo ×{comboCount}!";

            if (_canvasGroup != null) _canvasGroup.alpha = 1f;
            transform.localScale = Vector3.one * _punchScale;
            gameObject.SetActive(true);

            _timer   = 0f;
            _showing = true;
        }

        private void Update()
        {
            if (!_showing) return;

            _timer += Time.deltaTime;

            // Scale punch settle
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                Vector3.one,
                Time.deltaTime * _punchSpeed);

            // Fade out phase
            float totalDuration = _holdDuration + _fadeDuration;
            if (_timer > _holdDuration && _canvasGroup != null)
            {
                float fadeProgress = (_timer - _holdDuration) / _fadeDuration;
                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, fadeProgress);
            }

            if (_timer >= totalDuration)
                Hide();
        }

        private void Hide()
        {
            _showing = false;
            gameObject.SetActive(false);
        }
    }
}
