using System;
using UnityEngine;

/// <summary>
/// Controls the enemy's turn: cycles through the action pattern defined in EnemyData,
/// telegraphs the next action, and executes it after the player's move resolves.
/// </summary>
public class EnemyTurnController : MonoBehaviour
{
    #region Inspector Fields
    [SerializeField] private EnemyController  enemyController;
    [SerializeField] private MatchResolver    matchResolver;
    #endregion

    #region Events
    /// <summary>
    /// Fired when the enemy executes an action.
    /// Listeners: CharacterRuntime (to take damage), BattleUIController (to show effects).
    /// </summary>
    public event Action<EnemyActionData, float> OnEnemyActionExecuted;

    /// <summary>Fired to telegraph next action before it happens.</summary>
    public event Action<EnemyActionData> OnNextActionTelegraphed;
    #endregion

    #region Runtime State
    private int  currentActionIndex;
    private bool battleActive;
    private int  stageNumber = 1;
    #endregion

    #region Properties
    /// <summary>The enemy's next action (for UI intent display).</summary>
    public EnemyActionData NextAction
    {
        get
        {
            if (enemyController?.Data == null) return null;
            var pattern = enemyController.Data.actionPattern;
            if (pattern == null || pattern.Length == 0) return null;
            return pattern[currentActionIndex % pattern.Length];
        }
    }
    #endregion

    #region Unity Lifecycle
    private void OnEnable()
    {
        if (matchResolver != null)
            matchResolver.OnAllChainsResolved += ExecuteEnemyTurn;
    }

    private void OnDisable()
    {
        if (matchResolver != null)
            matchResolver.OnAllChainsResolved -= ExecuteEnemyTurn;
    }
    #endregion

    #region Public
    /// <summary>Call to begin the battle. Resets action index and telegraphs first action.</summary>
    public void StartBattle(int stage = 1)
    {
        stageNumber        = stage;
        currentActionIndex = 0;
        battleActive       = true;
        TelegraphNextAction();
    }

    public void StopBattle()
    {
        battleActive = false;
    }
    #endregion

    #region Private
    private void ExecuteEnemyTurn()
    {
        if (!battleActive) return;
        if (enemyController == null || enemyController.IsDefeated) return;

        // Tick status effects (poison, etc.) at the start of enemy turn
        enemyController.TickStatusEffects();
        if (enemyController.IsDefeated) return;

        // Get current action from pattern
        var pattern = enemyController.Data.actionPattern;
        if (pattern == null || pattern.Length == 0) return;

        EnemyActionData action = pattern[currentActionIndex % pattern.Length];

        // Scale damage by stage
        float scaledValue = action.value *
            Mathf.Pow(enemyController.Data.damageScalePerStage, stageNumber - 1);

        // Execute
        ExecuteAction(action, scaledValue);

        // Advance pattern
        currentActionIndex++;
        TelegraphNextAction();
    }

    private void ExecuteAction(EnemyActionData action, float scaledValue)
    {
        switch (action.actionType)
        {
            case EnemyActionType.Attack:
                Debug.Log($"[Enemy Turn] {enemyController.Data.enemyName} attacks for {scaledValue:F1}");
                break;

            case EnemyActionType.Defend:
                Debug.Log($"[Enemy Turn] {enemyController.Data.enemyName} defends");
                break;

            case EnemyActionType.Debuff:
                Debug.Log($"[Enemy Turn] {enemyController.Data.enemyName} debuffs player");
                break;

            case EnemyActionType.Heal:
                Debug.Log($"[Enemy Turn] {enemyController.Data.enemyName} heals for {scaledValue:F1}");
                break;

            case EnemyActionType.Charge:
                Debug.Log($"[Enemy Turn] {enemyController.Data.enemyName} is charging...");
                break;
        }

        OnEnemyActionExecuted?.Invoke(action, scaledValue);
    }

    private void TelegraphNextAction()
    {
        if (enemyController?.Data == null) return;
        var pattern = enemyController.Data.actionPattern;
        if (pattern == null || pattern.Length == 0) return;

        EnemyActionData nextAction = pattern[currentActionIndex % pattern.Length];
        OnNextActionTelegraphed?.Invoke(nextAction);
    }
    #endregion
}
