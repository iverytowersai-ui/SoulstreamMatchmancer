using UnityEngine;

/// <summary>
/// Initialises the battle at scene start.
/// Wires CharacterRuntime, EnemyController, and EnemyTurnController
/// with their ScriptableObject data and starts the combat loop.
/// </summary>
public class BattleInitializer : MonoBehaviour
{
    [Header("Data Assets")]
    [SerializeField] private CharacterData    characterData;
    [SerializeField] private EnemyData        enemyData;

    [Header("Runtime References")]
    [SerializeField] private CharacterRuntime    characterRuntime;
    [SerializeField] private EnemyController     enemyController;
    [SerializeField] private EnemyTurnController enemyTurnController;

    [Header("Battle Settings")]
    [SerializeField] private int playerLevel = 1;
    [SerializeField] private int stageNumber = 1;

    private void Start()
    {
        InitialiseBattle();
    }

    public void InitialiseBattle()
    {
        // Reset combat stats
        CombatStats.Instance?.ResetForNewBattle();

        // Initialise player
        if (characterRuntime != null && characterData != null)
        {
            characterRuntime.InitialiseForBattle(characterData, playerLevel);
            Debug.Log($"[Battle] {characterData.characterName} ready — " +
                      $"HP:{characterRuntime.MaxHP} ATK:{characterRuntime.Attack}");
        }

        // Initialise enemy
        if (enemyController != null && enemyData != null)
        {
            enemyController.Initialise(enemyData, stageNumber);
            Debug.Log($"[Battle] {enemyData.enemyName} ready — HP:{enemyController.MaxHP}");
        }

        // Start enemy turn cycle
        if (enemyTurnController != null)
        {
            enemyTurnController.StartBattle(stageNumber);
        }

        // Listen for battle end
        if (enemyController != null)
            enemyController.OnEnemyDefeated += HandleVictory;
        if (characterRuntime != null)
            characterRuntime.OnPlayerDefeated += HandleDefeat;
    }

    private void HandleVictory()
    {
        enemyTurnController?.StopBattle();
        CombatStats.Instance?.RecordVictory(true, CombatStats.Instance.BattleDuration);

        // Award XP from defeated enemy
        if (enemyData != null)
            characterRuntime?.AwardXP(enemyData.xpReward);

        Debug.Log("[Battle] VICTORY!");
    }

    private void HandleDefeat()
    {
        enemyTurnController?.StopBattle();
        CombatStats.Instance?.RecordVictory(false, CombatStats.Instance.BattleDuration);
        Debug.Log("[Battle] DEFEAT.");
    }

    private void OnDestroy()
    {
        if (enemyController != null)
            enemyController.OnEnemyDefeated -= HandleVictory;
        if (characterRuntime != null)
            characterRuntime.OnPlayerDefeated -= HandleDefeat;
    }
}
