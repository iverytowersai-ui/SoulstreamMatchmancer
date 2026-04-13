/// <summary>
/// Types of actions an enemy can take during their turn.
/// </summary>
public enum EnemyActionType
{
    Attack,     // Deal direct damage to the player
    Defend,     // Raise own armor/barrier
    Debuff,     // Reduce player stats (attack, etc.)
    Heal,       // Restore own HP
    Charge      // Charging for a heavy attack next turn
}
