using UnityEngine;

namespace Matchmancer.Combat
{
    /// <summary>
    /// Minimal EnemyData ScriptableObject — just enough for Stage 1 to compile and play.
    /// Replace or extend when Skill 13 (Enemy System) is built.
    /// Create via right-click → Create → Matchmancer → Enemy Data.
    /// </summary>
    [CreateAssetMenu(fileName = "Enemy_New", menuName = "Matchmancer/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        public string enemyName = "Shade Wraith";
        public Sprite portrait;
        [TextArea] public string loreBlurb;

        [Header("Stats")]
        public int   maxHP        = 100;
        public int   armor        = 0;
        public int   attackPower  = 10;

        [Header("Turn Behavior")]
        [Tooltip("How many player moves between each enemy attack. 1 = every move.")]
        public int   attackCadence = 3;

        [Header("Difficulty Tags")]
        public bool  isBoss             = false;
        public bool  isElite            = false;
        public bool  appliesPoisonOnHit = false;
        public bool  regeneratesArmor   = false;

        [Header("Multi-Phase (bosses)")]
        public bool  isMultiPhase       = false;
        [Range(0f, 1f)] public float phase2HpThreshold = 0.66f;
        [Range(0f, 1f)] public float phase3HpThreshold = 0.33f;
    }
}
