using System;
using System.Collections.Generic;
using UnityEngine;
using Matchmancer.Combat;
using Matchmancer.Core;

namespace Matchmancer.Enemy
{
    /// <summary>
    /// Scene-side glue between <see cref="BoardController"/> and
    /// <see cref="EnemyRuntime"/>. Subscribes to the combat events the
    /// board already raises and drives the enemy's live state.
    ///
    /// Wiring:
    ///   - <see cref="BoardController.OnCombatWaveResolved"/>
    ///     → apply every effect in the wave to the enemy runtime.
    ///   - <see cref="BoardController.OnMoveDeducted"/>
    ///     → enemy's turn: tick debuffs, then roll an attack.
    ///
    /// Raises <see cref="OnEnemyAttack"/> with the damage total so the
    /// character system (Skill 14) can listen and take the hit.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyTurnController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BoardController boardController;
        [SerializeField] private EnemyData       enemyData;
        [SerializeField] private CombatConfig    combatConfig;

        public EnemyRuntime Runtime { get; private set; }

        /// <summary>Damage the enemy intends to deal to the character this turn.</summary>
        public event Action<float> OnEnemyAttack;
        /// <summary>Fired after all wave effects have been absorbed.</summary>
        public event Action OnWaveAbsorbed;
        /// <summary>Fired when the enemy dies.</summary>
        public event Action OnEnemyDefeated;

        private bool _alreadyDefeated;

        // ------------------------------------------------------------------
        // Lifecycle
        // ------------------------------------------------------------------

        private void OnEnable()
        {
            TryBuildRuntime();
            TrySubscribe();
        }

        private void OnDisable()
        {
            TryUnsubscribe();
        }

        /// <summary>
        /// Rebuild the runtime from <see cref="enemyData"/> + the current
        /// <see cref="combatConfig"/>. Call between levels if the enemy changes.
        /// </summary>
        public void RebuildRuntime()
        {
            DetachRuntime();
            TryBuildRuntime();
            _alreadyDefeated = false;
        }

        private void TryBuildRuntime()
        {
            if (enemyData == null)
            {
                Debug.LogWarning($"[{nameof(EnemyTurnController)}] No EnemyData assigned; enemy is inert.", this);
                return;
            }

            var tuning = combatConfig != null ? combatConfig.ToTuning() : CombatTuning.Default();
            Runtime = enemyData.CreateRuntime(tuning);
            Runtime.OnDefeated += HandleDefeated;
        }

        private void DetachRuntime()
        {
            if (Runtime != null)
                Runtime.OnDefeated -= HandleDefeated;
            Runtime = null;
        }

        private void TrySubscribe()
        {
            if (boardController == null)
            {
                Debug.LogWarning($"[{nameof(EnemyTurnController)}] No BoardController assigned; enemy won't react to matches.", this);
                return;
            }

            boardController.OnCombatWaveResolved += HandleWave;
            boardController.OnMoveDeducted       += HandleMoveDeducted;
        }

        private void TryUnsubscribe()
        {
            if (boardController == null) return;
            boardController.OnCombatWaveResolved -= HandleWave;
            boardController.OnMoveDeducted       -= HandleMoveDeducted;
        }

        // ------------------------------------------------------------------
        // Board event handlers
        // ------------------------------------------------------------------

        private void HandleWave(IReadOnlyList<CombatEffect> effects)
        {
            if (Runtime == null || Runtime.IsDefeated) return;
            if (effects == null) return;

            for (int i = 0; i < effects.Count; i++)
                Runtime.ApplyCombatEffect(effects[i]);

            OnWaveAbsorbed?.Invoke();
        }

        private void HandleMoveDeducted(int _movesRemaining)
        {
            if (Runtime == null) return;

            // Tick status effects first — poison can finish a weakened enemy.
            Runtime.OnPlayerTurnEnd();
            if (Runtime.IsDefeated) return;

            // Then attack.
            float dmg = Runtime.RollAttack();
            if (dmg > 0f)
                OnEnemyAttack?.Invoke(dmg);
        }

        private void HandleDefeated()
        {
            if (_alreadyDefeated) return;
            _alreadyDefeated = true;
            OnEnemyDefeated?.Invoke();
        }
    }
}
