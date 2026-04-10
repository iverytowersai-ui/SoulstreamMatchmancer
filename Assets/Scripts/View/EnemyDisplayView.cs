using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Matchmancer.Enemy;

namespace Matchmancer.View
{
    /// <summary>
    /// Presents the enemy's live state during battle: name, portrait, HP
    /// bar, armor bar, and status-effect icons (poison, vulnerability).
    ///
    /// Setup:
    ///   1. Create a UI panel (Image background) anchored top-center of the
    ///      Canvas.
    ///   2. Add child elements for each [SerializeField] slot below.
    ///   3. Drag this script onto the panel and wire the references.
    ///   4. <see cref="BattleUIOrchestrator"/> calls
    ///      <see cref="Bind(EnemyRuntime)"/> once when the battle starts.
    ///
    /// All fields are optional — leave any null and that part simply won't
    /// render. This lets you bring the UI online in stages.
    /// </summary>
    public class EnemyDisplayView : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private Image           _portrait;

        [Header("HP")]
        [SerializeField] private Image           _hpFill;
        [SerializeField] private TextMeshProUGUI _hpText;

        [Header("Armor")]
        [SerializeField] private Image           _armorFill;
        [SerializeField] private TextMeshProUGUI _armorText;
        [SerializeField] private GameObject      _armorGroup;

        [Header("Status Icons")]
        [SerializeField] private GameObject      _poisonIcon;
        [SerializeField] private TextMeshProUGUI _poisonStacksText;
        [SerializeField] private GameObject      _vulnerableIcon;

        private EnemyRuntime _runtime;

        // ------------------------------------------------------------------
        // Bind / unbind
        // ------------------------------------------------------------------

        public void Bind(EnemyRuntime runtime)
        {
            Unbind();
            _runtime = runtime;
            if (_runtime == null) return;

            _runtime.OnHpChanged    += HandleHpChanged;
            _runtime.OnArmorChanged += HandleArmorChanged;
            _runtime.OnPoisonTicked += HandlePoisonTicked;
            _runtime.OnDefeated     += HandleDefeated;

            // Initial paint
            if (_nameText != null) _nameText.text = _runtime.DisplayName;
            RefreshHp();
            RefreshArmor();
            RefreshStatus();
        }

        public void Unbind()
        {
            if (_runtime == null) return;
            _runtime.OnHpChanged    -= HandleHpChanged;
            _runtime.OnArmorChanged -= HandleArmorChanged;
            _runtime.OnPoisonTicked -= HandlePoisonTicked;
            _runtime.OnDefeated     -= HandleDefeated;
            _runtime = null;
        }

        private void OnDestroy() => Unbind();

        // ------------------------------------------------------------------
        // Event handlers
        // ------------------------------------------------------------------

        private void HandleHpChanged(int oldHp, int newHp, int delta)
        {
            RefreshHp();
            RefreshStatus();
        }

        private void HandleArmorChanged(int oldArmor, int newArmor) => RefreshArmor();

        private void HandlePoisonTicked(int dot)
        {
            RefreshHp();
            RefreshStatus();
        }

        private void HandleDefeated()
        {
            RefreshHp();
            RefreshStatus();
        }

        // ------------------------------------------------------------------
        // Refresh helpers
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

        private void RefreshArmor()
        {
            if (_runtime == null) return;

            bool hasArmor = _runtime.MaxArmor > 0;
            if (_armorGroup != null) _armorGroup.SetActive(hasArmor);

            if (!hasArmor) return;

            float frac = Mathf.Clamp01((float)_runtime.CurrentArmor / _runtime.MaxArmor);
            if (_armorFill != null) _armorFill.fillAmount = frac;
            if (_armorText != null) _armorText.text = $"{_runtime.CurrentArmor}/{_runtime.MaxArmor}";
        }

        private void RefreshStatus()
        {
            if (_runtime == null) return;

            bool poisoned = _runtime.IsPoisoned;
            if (_poisonIcon != null) _poisonIcon.SetActive(poisoned);
            if (_poisonStacksText != null)
            {
                _poisonStacksText.gameObject.SetActive(poisoned);
                _poisonStacksText.text = poisoned ? $"×{_runtime.PoisonStacks}" : "";
            }

            bool vuln = _runtime.IsVulnerable;
            if (_vulnerableIcon != null) _vulnerableIcon.SetActive(vuln);
        }
    }
}
