using UnityEngine;

/// <summary>
/// ScriptableObject defining an enemy's stats, attack pattern, and visual data.
/// Create via right-click → Create → Matchmancer → Enemy Data.
/// </summary>
[CreateAssetMenu(fileName = "EnemyData_Default", menuName = "Matchmancer/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string     enemyName   = "Shadow Thrall";
    public Sprite     portrait;
    public Sprite     battleSprite;

    [Header("Stats")]
    public float      maxHP       = 100f;
    public float      defense     = 0f;      // flat damage reduction
    public float      armor       = 0f;      // barrier that must be broken first (Break tiles)

    [Header("Attack Pattern")]
    [Tooltip("Enemy cycles through these actions in order, then loops.")]
    public EnemyActionData[] actionPattern = new EnemyActionData[]
    {
        new EnemyActionData { actionType = EnemyActionType.Attack, value = 15f }
    };

    [Header("Rewards")]
    public int        xpReward    = 50;
    public int        goldReward  = 25;

    [Header("Scaling")]
    [Tooltip("HP multiplier per stage beyond stage 1. e.g. 1.15 = +15% HP per stage.")]
    public float      hpScalePerStage      = 1.15f;
    public float      damageScalePerStage  = 1.10f;

    [Header("Flavour")]
    [TextArea] public string loreDescription;
}
