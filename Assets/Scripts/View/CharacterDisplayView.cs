using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Matchmancer.Character;

namespace Matchmancer.View
{
    /// <summary>
    /// Presents the player character's live state: HP bar, shield overlay,
    /// energy meter, name/portrait, and level badge.
    ///
    /// Setup:
    ///   1. Create a UI panel anchored bottom-left or bottom-right of the
    ///      Canvas.
    ///   2. Wire child Image/Text elements to the slots below.
    ///   3. <see cref="BattleUIOrchestrator"/> calls
    ///      <see cref="Bind(CharacterRuntime)"/> at battle start.
    ///
    /// The HP bar uses a layered approach: _hpFill (green) sits behind
    /// _shieldFill (blue overlay). When shield is active, its fill sits on
    /// top of the HP bar to show absorbed-before-HP semantics visually.
    /// </summary>
    public class CharacterDisplayView : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private Image           _portrait;
        [SerializeField] private TextMeshProUGUI _levelText;

        [Header("HP")]
        [SerializeField] private Image           _hpFill;
        [SerializeField] private TextMeshProUGUI _hpText;

        [Header("Shield")]
        [SerializeField] private Image           _shieldFill;
        [SerializeField] private TextMeshProUGUI _shieldText;
        [SerializeField] private GameObject      _shieldGroup;

        [Header("Energy / Ultimate")]
        [SerializeField] private Image           _energyFill;
        [SerializeField] private TextMeshProUGUI _energyText;

        private CharacterRuntime _runtime;

        // ------------------------------------------------------------------
        // Bind / unbind
        // ------------------------------------------------------------------

        public void Bind(CharacterRuntime runtime)
        {
            Unbind();
            _runtime = runtime;
            if (_runtime == null) return;

            _runtime.OnHpChanged            += HandleHpChanged;
            _runtime.OnShieldChanged        += HandleShieldChanged;
            _runtime.OnEnergyChanged        += HandleEnergyChanged;
            _runtime.OnUltimateReadyChanged += HandleUltimateReady;
            _runtime.OnDefeated             += HandleDefeated;
            _runtime.OnLevelUp              += HandleLevelUp;

            // Initial paint
            if (_nameText  != null) _nameText.text  = _runtime.DisplayName;
            if (_levelText != null) _levelText.text  = $"Lv.{_runtime.Level}";
            RefreshHp();
            RefreshShield();
            RefreshEnergy();
        }

        public void Unbind()
        {
            if (_runtime == null) return;
            _runtime.OnHpChanged            -= HandleHpChanged;
            _runtime.OnShieldChanged        -= HandleShieldChanged;
            _runtime.OnEnergyChanged        -= HandleEnergyChanged;
            _runtime.OnUltimateReadyChanged -= HandleUltimateReady;
            _runtime.OnDefeated             -= HandleDefeated;
            _runtime.OnLevelUp              -= HandleLevelUp;
            _runtime = null;
        }

        private void OnDestroy() => Unbind();

        // ------------------------------------------------------------------
        // Event handlers
        // ------------------------------------------------------------------

        private void HandleHpChanged(int old, int cur, int delta) => RefreshHp();
        private void HandleShieldChanged(float shield)            => RefreshShield();
        private void HandleEnergyChanged(float cur, float max)    => RefreshEnergy();
        private void HandleDefeated()                             => RefreshHp();

        private void HandleUltimateReady(bool ready)
        {
            // The UltimateButtonView handles glow/pulse. Energy bar still
            // fills to full so the player sees it hit 100%.
            RefreshEnergy();
        }

        private void HandleLevelUp(int newLevel)
        {
            if (_levelText != null) _levelText.text = $"Lv.{newLevel}";
            RefreshHp();
        }

        // ------------------------------------------------------------------
        // Refresh
        // ------------------------------------------------------------------

        private void RefreshHp()
        {
            if (_runtime == null) return;

            float frac = _runtime.MaxHp > 0
                ? Mathf.Clamp01((float)_runtime.CurrentHp / _runtime.MaxHp)
                : 0f;

            if (_hpFill != null) _hpFill.fillAmount = frac;
            if (_hpText != null) _hpText.text = $"{_runtime.CurrentHp}/{_runtime.MaxHp}";
        }

        private void RefreshShield()
        {
            if (_runtime == null) return;

            bool hasShield = _runtime.CurrentShield > 0f;
            if (_shieldGroup != null) _shieldGroup.SetActive(hasShield);

            if (hasShield)
            {
                // Shield fills relative to MaxHp so you can see how many HP
                // it's "worth".
                float frac = _runtime.MaxHp > 0
                    ? Mathf.Clamp01(_runtime.CurrentShield / _runtime.MaxHp)
                    : 0f;

                if (_shieldFill != null) _shieldFill.fillAmount = frac;
                if (_shieldText != null)
                    _shieldText.text = $"+{Mathf.FloorToInt(_runtime.CurrentShield)}";
            }
        }

        private void RefreshEnergy()
        {
            if (_runtime == null) return;

            float frac = _runtime.MaxEnergy > 0f
                ? Mathf.Clamp01(_runtime.CurrentEnergy / _runtime.MaxEnergy)
                : 0f;

            if (_energyFill != null) _energyFill.fillAmount = frac;
            if (_energyText != null) _energyText.text = $"{(int)_runtime.CurrentEnergy}/{(int)_runtime.MaxEnergy}";
        }
    }
}
