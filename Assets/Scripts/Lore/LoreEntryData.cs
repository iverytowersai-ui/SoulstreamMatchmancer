using UnityEngine;

namespace Matchmancer.Lore
{
    /// <summary>
    /// Inspector-facing wrapper for a single lore page. One asset per entry,
    /// stored under <c>Assets/Data/Lore/…</c>.
    /// </summary>
    [CreateAssetMenu(fileName = "LoreEntry_New", menuName = "Matchmancer/Lore Entry")]
    public class LoreEntryData : ScriptableObject
    {
        [Header("Identity")]
        public string id;
        public string title;
        [TextArea(4, 12)] public string bodyText;
        public LoreCategory category;
        public Sprite illustration;

        [Header("Unlock")]
        [Tooltip("Achievement or level id that must be completed to reveal this entry. Leave empty for always visible.")]
        public string unlockConditionId;
        public bool isSecret;

        [Header("Display")]
        [Min(0)] public int sortOrder;

        public LoreEntry ToDefinition()
        {
            return new LoreEntry
            {
                Id                = string.IsNullOrEmpty(id) ? name : id,
                Title             = string.IsNullOrEmpty(title) ? name : title,
                BodyText          = bodyText ?? string.Empty,
                Category          = category,
                ImageId           = illustration != null ? illustration.name : string.Empty,
                UnlockConditionId = unlockConditionId ?? string.Empty,
                SortOrder         = sortOrder,
                IsSecret          = isSecret,
            };
        }
    }
}
