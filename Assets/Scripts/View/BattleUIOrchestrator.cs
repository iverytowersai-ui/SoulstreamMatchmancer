using System.Collections.Generic;
using UnityEngine;
using Matchmancer.Core;
using Matchmancer.Combat;
using Matchmancer.Character;
using Matchmancer.Enemy;
using Matchmancer.Progression;
using Matchmancer.Objectives;

namespace Matchmancer.View
{
    /// <summary>
    /// Master scene-side wiring for the battle UI. Lives on a root UI
    /// GameObject and references every battle view component. On
    /// <see cref="InitializeBattle"/> it binds all views to the live
    /// runtime objects, subscribes to BoardController + combat events,
    /// and drives damage popups, combo banners, and the results overlay
    /// when the battle ends.
    ///
    /// Scene hierarchy suggestion:
    ///   BattleCanvas (Canvas — Screen Space Overlay, CanvasScaler: Scale With Screen Size)
    ///     ├─ EnemyPanel        (anchor top-center)     → EnemyDisplayView
    ///     ├─ CharacterPanel    (anchor bottom-left)    → CharacterDisplayView
    ///     ├─ UltimateButton    (anchor bottom-right)   → UltimateButtonView
    ///     ├─ ComboBanner       (anchor center)         → ComboBannerView
    ///     ├─ PopupContainer    (anchor center, RectTransform stretches to fill)
    ///     ├─ HUD               (anchor top-left)       → HUDView (existing)
    ///     └─ ResultsOverlay    (full-screen, starts inactive) → BattleResultsView
    ///
    /// How to wire:
    ///   1. Add this script to the Canvas (or an empty child).
    ///   2. Drag each view into the matching Inspector slot.
    ///   3. Drag the BoardController / EnemyTurnController /
    ///      CharacterBattleController scene objects into the "Runtime
    ///      Sources" section.
    ///   4. Press Play — the orchestrator auto-binds everything in
    ///      <see cref="OnEnable"/>.
    /// </summary>
    public class BattleUIOrchestrator : MonoBehaviour
    {
        // ------------------------------------------------------------------
        // Runtime sources
        // ------------------------------------------------------------------

        [Header("Runtime Sources")]
        [SerializeField] private BoardController          boardController;
        [SerializeField] private EnemyTurnController      enemyTurnController;
        [SerializeField] private CharacterBattleController characterBattleController;
        [SerializeField] private ResultsController        resultsController;

        // ------------------------------------------------------------------
        // View references
        // ------------------------------------------------------------------

        [Header("Views")]
        [SerializeField] private EnemyDisplayView     enemyDisplay;
        [SerializeField] private CharacterDisplayView characterDisplay;
        [SerializeField] private UltimateButtonView   ultimateButton;
        [SerializeField] private ComboBannerView      comboBanner;
        [SerializeField] private BattleResultsView    resultsView;

        [Header("Damage Popup")]
        [SerializeField] private DamagePopupView _popupPrefab;
        [SerializeField] private RectTransform   _popupContainer;

        [Header("Popup Colors")]
        [SerializeField] private Color _playerDamageColor = new Color(1f, 0.3f, 0.2f, 1f);
        [SerializeField] private Color _enemyDamageColor  = new Color(1f, 0.8f, 0.1f, 1f);
        [SerializeField] private Color _poisonColor       = new Color(0.6f, 0.2f, 0.8f, 1f);
        [SerializeField] private Color _armorBreakColor   = new Color(0.7f, 0.7f, 0.7f, 1f);

        // World-space anchor positions for popup spawn. Assign empty
        // GameObjects placed near the enemy/character visuals.
        [Header("Popup Anchors")]
        [SerializeField] private Transform _enemyPopupAnchor;
        [SerializeField] private Transform _characterPopupAnchor;

        private int _comboCounter;

        // ------------------------------------------------------------------
        // Lifecycle
        // ------------------------------------------------------------------

        private void OnEnable()
        {
            InitializeBattle();
        }

        private void OnDisable()
        {
            TeardownBattle();
        }

        /// <summary>
        /// Wire all views to the current runtime objects. Safe to call
        /// multiple times (e.g. between levels).
        /// </summary>
        public void InitializeBattle()
        {
            TeardownBattle();

            // Bind enemy
            if (enemyDisplay != null && enemyTurnController != null && enemyTurnController.Runtime != null)
                enemyDisplay.Bind(enemyTurnController.Runtime);

            // Bind character
            if (characterDisplay != null && characterBattleController != null && characterBattleController.Runtime != null)
                characterDisplay.Bind(characterBattleController.Runtime);

            // Bind ultimate button
            if (ultimateButton != null && characterBattleController != null && characterBattleController.Runtime != null)
                ultimateButton.Bind(characterBattleController.Runtime);

            // Subscribe to board events for combo tracking
            if (boardController != null)
            {
                boardController.OnCombatWaveResolved += HandleCombatWaveResolved;
                boardController.OnMoveDeducted       += HandleMoveDeducted;
                boardController.OnLevelComplete       += HandleLevelComplete;
            }

            // Subscribe to enemy damage events
            if (enemyTurnController != null)
            {
                if (enemyTurnController.Runtime != null)
                {
                    enemyTurnController.Runtime.OnHpChanged    += HandleEnemyHpChanged;
                    enemyTurnController.Runtime.OnArmorChanged += HandleEnemyArmorChanged;
                    enemyTurnController.Runtime.OnPoisonTicked += HandlePoisonTicked;
                }
                enemyTurnController.OnEnemyAttack += HandleEnemyAttack;
            }

            // Subscribe to character damage events
            if (characterBattleController != null && characterBattleController.Runtime != null)
                characterBattleController.Runtime.OnDamageTaken += HandleCharacterDamage;

            // Results view — subscribe via results controller
            if (resultsController != null)
                resultsController.OnResultsReady += HandleResultsReady;

            _comboCounter = 0;
        }

        public void TeardownBattle()
        {
            if (enemyDisplay != null) enemyDisplay.Unbind();
            if (characterDisplay != null) characterDisplay.Unbind();
            if (ultimateButton != null) ultimateButton.Unbind();

            if (boardController != null)
            {
                boardController.OnCombatWaveResolved -= HandleCombatWaveResolved;
                boardController.OnMoveDeducted       -= HandleMoveDeducted;
                boardController.OnLevelComplete       -= HandleLevelComplete;
            }

            if (enemyTurnController != null)
            {
                if (enemyTurnController.Runtime != null)
                {
                    enemyTurnController.Runtime.OnHpChanged    -= HandleEnemyHpChanged;
                    enemyTurnController.Runtime.OnArmorChanged -= HandleEnemyArmorChanged;
                    enemyTurnController.Runtime.OnPoisonTicked -= HandlePoisonTicked;
                }
                enemyTurnController.OnEnemyAttack -= HandleEnemyAttack;
            }

            if (characterBattleController != null && characterBattleController.Runtime != null)
                characterBattleController.Runtime.OnDamageTaken -= HandleCharacterDamage;

            if (resultsController != null)
                resultsController.OnResultsReady -= HandleResultsReady;
        }

        // ------------------------------------------------------------------
        // Board combat events
        // ------------------------------------------------------------------

        private void HandleCombatWaveResolved(IReadOnlyList<CombatEffect> effects)
        {
            _comboCounter++;

            // Show combo banner for cascades (2+)
            if (comboBanner != null)
                comboBanner.ShowCombo(_comboCounter);
        }

        private void HandleMoveDeducted(int movesRemaining)
        {
            // Reset combo counter each player move
            _comboCounter = 0;
        }

        private void HandleLevelComplete(Objectives.LevelResult result, int stars)
        {
            // Results view is driven by ResultsController, not this event
            // directly. But if there's no ResultsController, show a fallback.
            if (resultsController == null && resultsView != null)
            {
                resultsView.Show(
                    new BattleResult
                    {
                        Victory    = result == Objectives.LevelResult.Victory,
                        FinalScore = boardController != null ? boardController.Score : 0,
                    },
                    new StarResult { Stars = stars, FiveStar = stars >= 5, Victory = result == Objectives.LevelResult.Victory });
            }
        }

        // ------------------------------------------------------------------
        // Damage popup events
        // ------------------------------------------------------------------

        private void HandleEnemyHpChanged(int oldHp, int newHp, int delta)
        {
            if (delta < 0)
                SpawnPopup(-delta, _enemyPopupAnchor, _enemyDamageColor);
        }

        private void HandleEnemyArmorChanged(int oldArmor, int newArmor)
        {
            int delta = oldArmor - newArmor;
            if (delta > 0)
                SpawnPopup(delta, _enemyPopupAnchor, _armorBreakColor);
        }

        private void HandlePoisonTicked(int dot)
        {
            if (dot > 0)
                SpawnPopup(dot, _enemyPopupAnchor, _poisonColor);
        }

        private void HandleEnemyAttack(float rawDamage)
        {
            // The actual HP damage may differ after shield/defense. We show
            // the raw intent here; the character display updates on its own.
        }

        private void HandleCharacterDamage(float actualDamage)
        {
            if (actualDamage > 0f)
                SpawnPopup((int)actualDamage, _characterPopupAnchor, _playerDamageColor);
        }

        // ------------------------------------------------------------------
        // Results
        // ------------------------------------------------------------------

        private void HandleResultsReady(BattleResult result, StarResult stars, LevelConfig level)
        {
            if (resultsView != null)
                resultsView.Show(result, stars);
        }

        // ------------------------------------------------------------------
        // Popup spawning
        // ------------------------------------------------------------------

        private void SpawnPopup(int amount, Transform anchor, Color color)
        {
            if (_popupPrefab == null || _popupContainer == null) return;
            if (amount <= 0) return;

            var popup = Instantiate(_popupPrefab, _popupContainer);

            Vector3 worldPos = anchor != null ? anchor.position : Vector3.zero;

            // Slight random offset so stacked popups don't overlap exactly
            Vector2 jitter = new Vector2(
                Random.Range(-20f, 20f),
                Random.Range(-10f, 10f));

            var rect = popup.GetComponent<RectTransform>();
            if (rect != null)
            {
                // Convert world anchor to canvas local position
                if (anchor != null)
                {
                    Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
                        Camera.main, worldPos);
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        _popupContainer, screenPoint, null, out Vector2 localPos);
                    rect.anchoredPosition = localPos + jitter;
                }
                else
                {
                    rect.anchoredPosition = jitter;
                }
            }

            popup.Show(amount, worldPos, color);
        }
    }
}
