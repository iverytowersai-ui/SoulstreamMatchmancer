using UnityEngine;

namespace Matchmancer.Achievements
{
    /// <summary>
    /// Inspector-facing achievement asset. Drop one .asset per achievement
    /// under <c>Assets/Data/Achievements/…</c>. Projects to a pure-C#
    /// <see cref="AchievementDefinition"/> via <see cref="ToDefinition"/>
    /// which the runtime + tests consume.
    /// </summary>
    [CreateAssetMenu(fileName = "AchievementData", menuName = "Matchmancer/Achievement Data")]
    public class AchievementData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Stable save id. Never rename after the achievement ships.")]
        public string id;
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Unlock Condition")]
        public AchievementStatKey statKey = AchievementStatKey.LevelsCompleted;
        [Min(1)] public long targetValue = 1;

        [Header("Reward")]
        [Tooltip("Optional title id to grant when this unlocks. Leave empty for no title.")]
        public string titleIdReward;
        [Min(0)] public int goldReward;

        [Header("Display")]
        [Tooltip("Hide name + description until unlocked.")]
        public bool isSecret;

        public AchievementDefinition ToDefinition()
        {
            return new AchievementDefinition
            {
                Id            = string.IsNullOrEmpty(id)          ? name : id,
                DisplayName   = string.IsNullOrEmpty(displayName) ? name : displayName,
                Description   = description ?? string.Empty,
                StatKey       = statKey,
                TargetValue   = targetValue < 1 ? 1 : targetValue,
                TitleIdReward = titleIdReward ?? string.Empty,
                GoldReward    = goldReward,
                IsSecret      = isSecret,
            };
        }
    }
}
