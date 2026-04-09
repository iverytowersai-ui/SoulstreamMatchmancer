using UnityEngine;
using Matchmancer.Combat;

namespace Matchmancer.Enemy
{
    /// <summary>
    /// Unity-side asset that defines an enemy's static stats.
    /// Designers create these in Project view; runtime state lives in
    /// <see cref="EnemyRuntime"/> which is built from this data per battle.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Matchmancer/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        public string displayName = "Unknown Foe";
        [TextArea] public string loreDescription;
        public Sprite portrait;

        [Header("Defensive Stats")]
        [Min(1)]  public int   maxHp          = 100;
        [Min(0f)] public float defense        = 0f;
        [Min(0)]  public int   maxArmor       = 0;

        [Header("Offensive Stats")]
        [Min(0f)] public float baseAttackPower = 10f;
        [Min(1)]  public int   attacksPerTurn  = 1;

        /// <summary>Create a live runtime instance from this data.</summary>
        public EnemyRuntime CreateRuntime(CombatTuning tuning)
        {
            return new EnemyRuntime(
                displayName:     displayName,
                maxHp:           maxHp,
                defense:         defense,
                maxArmor:        maxArmor,
                baseAttackPower: baseAttackPower,
                attacksPerTurn:  attacksPerTurn,
                tuning:          tuning);
        }
    }
}
