using System;

namespace Matchmancer.Achievements
{
    /// <summary>
    /// Pure C# definition of a title (the flair label shown next to the
    /// player's name in the hub / results screen). Lives in the runtime
    /// assembly so tests can build them without a ScriptableObject.
    ///
    /// Titles unlock either:
    ///   • Automatically, when an achievement whose
    ///     <see cref="AchievementDefinition.TitleIdReward"/> matches this
    ///     id fires (wired by <see cref="TitleSystem.WireToTracker"/>).
    ///   • Manually, via <see cref="TitleSystem.UnlockTitle"/>.
    /// </summary>
    [Serializable]
    public class TitleDefinition
    {
        /// <summary>Stable save id — never rename after shipping.</summary>
        public string Id;
        public string DisplayName;
        public string Description;

        /// <summary>UI rarity bucket. Drives color/border, not stat math.</summary>
        public TitleRarity Rarity;

        /// <summary>
        /// Optional id of the achievement that unlocks this title. Empty
        /// means manual-unlock only (e.g. story rewards, shop purchases).
        /// </summary>
        public string SourceAchievementId;

        public TitleDefinition()
        {
            Id                  = string.Empty;
            DisplayName         = string.Empty;
            Description         = string.Empty;
            Rarity              = TitleRarity.Common;
            SourceAchievementId = string.Empty;
        }

        public TitleDefinition Clone()
        {
            return new TitleDefinition
            {
                Id                  = Id,
                DisplayName         = DisplayName,
                Description         = Description,
                Rarity              = Rarity,
                SourceAchievementId = SourceAchievementId,
            };
        }
    }

    /// <summary>UI bucket for titles. Mirrors <see cref="Matchmancer.Character.GearRarity"/>.</summary>
    public enum TitleRarity
    {
        Common    = 0,
        Uncommon  = 1,
        Rare      = 2,
        Epic      = 3,
        Legendary = 4,
    }
}
