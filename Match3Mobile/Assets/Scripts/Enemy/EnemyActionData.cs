using UnityEngine;

/// <summary>
/// Defines a single enemy action in its pattern sequence.
/// Stored in EnemyData's action pattern array.
/// </summary>
[System.Serializable]
public class EnemyActionData
{
    public EnemyActionType actionType = EnemyActionType.Attack;
    public float           value      = 20f;   // damage amount, heal amount, etc.
    public string          intentIcon;          // UI hint: "sword", "shield", "skull"

    [TextArea]
    public string          telegraphText;       // "The Vampyl prepares to strike..."
}
