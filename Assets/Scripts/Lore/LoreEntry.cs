using System;

namespace Matchmancer.Lore
{
    /// <summary>
    /// Pure C# definition of a single lore page. Immutable once built.
    /// <see cref="LoreEntryData"/> is the Inspector-side wrapper.
    /// </summary>
    [Serializable]
    public class LoreEntry
    {
        /// <summary>Stable save id — never rename.</summary>
        public string Id;
        public string Title;
        public string BodyText;
        public LoreCategory Category;

        /// <summary>Sprite key the UI uses to look up the illustration.</summary>
        public string ImageId;

        /// <summary>
        /// Optional — id of the achievement or level whose completion reveals
        /// this entry. Empty string = always visible once unlocked.
        /// </summary>
        public string UnlockConditionId;

        /// <summary>Sort order within its category tab.</summary>
        public int SortOrder;

        /// <summary>If true, shown as "???" until unlocked.</summary>
        public bool IsSecret;

        public LoreEntry()
        {
            Id               = string.Empty;
            Title            = string.Empty;
            BodyText         = string.Empty;
            Category         = LoreCategory.Character;
            ImageId          = string.Empty;
            UnlockConditionId= string.Empty;
            SortOrder        = 0;
            IsSecret         = false;
        }

        public LoreEntry Clone()
        {
            return new LoreEntry
            {
                Id                = Id,
                Title             = Title,
                BodyText          = BodyText,
                Category          = Category,
                ImageId           = ImageId,
                UnlockConditionId = UnlockConditionId,
                SortOrder         = SortOrder,
                IsSecret          = IsSecret,
            };
        }
    }
}
