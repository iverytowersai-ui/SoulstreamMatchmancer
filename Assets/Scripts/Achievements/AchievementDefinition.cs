using System;

namespace Matchmancer.Achievements
{
    /// <summary>
    /// Pure C# definition of a single achievement. Lives in the runtime
    /// assembly so tests can build them without a ScriptableObject.
    /// <see cref="AchievementData"/> is the Inspector-facing wrapper.
    ///
    /// Unlock rule: when <c>Tracker.GetStat(StatKey) &gt;= TargetValue</c>,
    /// the tracker fires <see cref="AchievementTracker.OnAchievementUnlocked"/>
    /// exactly once for this definition.
    /// </summary>
    [Serializable]
    public class AchievementDefinition
    {
        /// <summary>Stable save id — never rename after shipping.</summary>
        public string Id;

        public string DisplayName;
        public string Description;

        /// <summary>Stat this achievement watches.</summary>
        public AchievementStatKey StatKey;

        /// <summary>The value the stat must reach (≥) to unlock.</summary>
        public long TargetValue;

        /// <summary>Optional title id to auto-grant when this unlocks.</summary>
        public string TitleIdReward;

        /// <summary>Hidden until unlocked (don't show in gallery).</summary>
        public bool IsSecret;

        /// <summary>Gold reward granted when unlocked (Skill 22 will pay out).</summary>
        public int GoldReward;

        public AchievementDefinition()
        {
            Id            = string.Empty;
            DisplayName   = string.Empty;
            Description   = string.Empty;
            StatKey       = AchievementStatKey.LevelsCompleted;
            TargetValue   = 1;
            TitleIdReward = string.Empty;
            IsSecret      = false;
            GoldReward    = 0;
        }

        public AchievementDefinition Clone()
        {
            return new AchievementDefinition
            {
                Id            = Id,
                DisplayName   = DisplayName,
                Description   = Description,
                StatKey       = StatKey,
                TargetValue   = TargetValue,
                TitleIdReward = TitleIdReward,
                IsSecret      = IsSecret,
                GoldReward    = GoldReward,
            };
        }
    }
}
