using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Matchmancer.Character;

namespace Matchmancer.View
{
    /// <summary>
    /// Controls the Ultimate button — pulse/glow when ready, press to queue,
    /// dim when not available.
    ///
    /// Setup:
    ///   1. Add a UI Button to the Canvas.
    ///   2. Add this script and wire the slot references.
    ///   3. The button's OnClick is wired in code (see <see cref="Bind"/>).
    ///
    /// Visual states:
    ///   • <b>Charging</b>  — button dimmed / not interactable.
    ///   • <b>Ready</b>     — glowing outline + interactable.
    ///   • <b>Queued</b>    — slightly pulsing to show it will fire next Damage wave.
    ///   • <b>Consumed</b>  — reverts to Charging after fire.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UltimateButtonView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image           _iconImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private GameObject      _glowOverlay;

        [Header("Visuals")]
        [SerializeField] private Color _readyColor  = new Color(1f, 0.85f, 0.2f, 1f);
        [SerializeField] private Color _dimColor    = new Color(0.4f, 0.4f, 0.4f, 1f);
        [SerializeField] private float _pulseSpeed  = 3f;
        [SerializeField] private float _pulseMin    = 0.7f;

        private Button            _button;
        private CharacterRuntime  _runtime;
        private bool              _isQueued;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);
            SetVisualState(false, false);
        }

        public void Bind(CharacterRuntime runtime)
        {
            Unbind();
            _runtime = runtime;
            if (_runtime == null) return;

            _runtime.OnUltimateReadyChanged += HandleReadyChanged;
            _runtime.OnEnergyChanged        += HandleEnergyChanged;

            if (_nameText != null) _nameText.text = _runtime.UltimateName ?? "Ultimate";

            _isQueued = _runtime.UltimateQueued;
            SetVisualState(_runtime.UltimateReady, _isQueued);
        }

        public void Unbind()
        {
            if (_runtime != null)
            {
                _runtime.OnUltimateReadyChanged -= HandleReadyChanged;
                _runtime.OnEnergyChanged        -= HandleEnergyChanged;
            }
            _runtime  = null;
            _isQueued = false;
            SetVisualState(false, false);
        }

        private void OnDestroy()
        {
            Unbind();
            if (_button != null) _button.onClick.RemoveListener(OnClick);
        }

        // ------------------------------------------------------------------
        // Input
        // ------------------------------------------------------------------

        private void OnClick()
        {
            if (_runtime == null) return;
            if (_runtime.QueueUltimate())
            {
                _isQueued = true;
                SetVisualState(true, true);
            }
        }

        // ------------------------------------------------------------------
        // Event handlers
        // ------------------------------------------------------------------

        private void HandleReadyChanged(bool ready)
        {
            if (!ready) _isQueued = false; // consumed
            SetVisualState(ready, _isQueued);
        }

        private void HandleEnergyChanged(float cur, float max)
        {
            // If ultimate was consumed (energy reset), update visuals.
            if (_runtime != null && !_runtime.UltimateReady)
            {
                _isQueued = false;
                SetVisualState(false, false);
            }
        }

        // ------------------------------------------------------------------
        // Visual state
        // ------------------------------------------------------------------

        private void SetVisualState(bool ready, bool queued)
        {
            _button.interactable = ready && !queued;

            if (_iconImage != null)
                _iconImage.color = ready ? _readyColor : _dimColor;

            if (_glowOverlay != null)
                _glowOverlay.SetActive(ready);
        }

        private void Update()
        {
            // Pulse the glow when queued to indicate "about to fire".
            if (_isQueued && _glowOverlay != null && _glowOverlay.activeSelf)
            {
                float t = Mathf.Lerp(_pulseMin, 1f,
                    (Mathf.Sin(Time.time * _pulseSpeed) + 1f) * 0.5f);
                var cg = _glowOverlay.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = t;
            }
        }
    }
}
