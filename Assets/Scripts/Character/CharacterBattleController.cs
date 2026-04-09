using System.Collections.Generic;
using UnityEngine;
using Matchmancer.Combat;
using Matchmancer.Core;
using Matchmancer.Enemy;

namespace Matchmancer.Character
{
    /// <summary>
    /// Scene-side glue between <see cref="BoardController"/>,
    /// <see cref="EnemyTurnController"/> and <see cref="CharacterRuntime"/>.
    ///
    /// Wiring:
    ///   • Builds <see cref="CharacterRuntime"/> from <see cref="CharacterData"/>.
    ///   • Subscribes to <see cref="BoardController.OnCombatEffect"/> to apply
    ///     Defense (shield) and Energy (ultimate meter) tile effects.
    ///   • Subscribes to <see cref="EnemyTurnController.OnEnemyAttack"/> to
    ///     absorb incoming damage.
    ///   • Exposes live Attack/Luck to the board via
    ///     <see cref="ICharacterStatsSource"/> so the resolver reads real
    ///     stats instead of its hardcoded defaults.
    /// </summary>
    [DisallowMultipleComponent]
    public class CharacterBattleController : MonoBehaviour, ICharacterStatsSource
    {
        [Header("References")]
        [SerializeField] private BoardController       boardController;
        [SerializeField] private EnemyTurnController   enemyTurnController;
        [SerializeField] private CharacterData         characterData;

        [Header("Starting State")]
        [Tooltip("Level to initialise the runtime at. Will be replaced by the " +
                 "save system in Skill 10/20.")]
        [Min(1)]
        [SerializeField] private int startingLevel = 1;

        public CharacterRuntime Runtime { get; private set; }

        // ICharacterStatsSource — live reads, resilient to null runtime
        public float Attack => Runtime?.CurrentAttack ?? 10f;
        public float Luck   => Runtime?.CurrentLuck   ?? 0f;

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

        /// <summary>Rebuild runtime (e.g. between levels).</summary>
        public void RebuildRuntime()
        {
            Runtime = null;
            TryBuildRuntime();
        }

        private void TryBuildRuntime()
        {
            if (characterData == null)
            {
                Debug.LogWarning($"[{nameof(CharacterBattleController)}] No CharacterData assigned; character is inert.", this);
                return;
            }
            Runtime = characterData.CreateRuntime(startingLevel);
        }

        private void TrySubscribe()
        {
            if (boardController != null)
            {
                boardController.OnCombatEffect += HandleCombatEffect;
                boardController.CharacterStatsSource = this;
            }
            else
            {
                Debug.LogWarning($"[{nameof(CharacterBattleController)}] No BoardController assigned; stats/effects won't wire.", this);
            }

            if (enemyTurnController != null)
                enemyTurnController.OnEnemyAttack += HandleEnemyAttack;
        }

        private void TryUnsubscribe()
        {
            if (boardController != null)
            {
                boardController.OnCombatEffect -= HandleCombatEffect;
                if (ReferenceEquals(boardController.CharacterStatsSource, this))
                    boardController.CharacterStatsSource = null;
            }

            if (enemyTurnController != null)
                enemyTurnController.OnEnemyAttack -= HandleEnemyAttack;
        }

        // ------------------------------------------------------------------
        // Event handlers
        // ------------------------------------------------------------------

        private void HandleCombatEffect(CombatEffect effect)
        {
            if (Runtime == null || Runtime.IsDefeated) return;
            Runtime.ApplyCombatEffect(effect);
        }

        private void HandleEnemyAttack(float rawDamage)
        {
            if (Runtime == null || Runtime.IsDefeated) return;
            Runtime.TakeEnemyDamage(rawDamage);
        }

        // ------------------------------------------------------------------
        // External input (UI buttons)
        // ------------------------------------------------------------------

        /// <summary>Called by the Ultimate button UI.</summary>
        public bool TryQueueUltimate() => Runtime?.QueueUltimate() ?? false;
    }
}
