using UnityEngine;

namespace Matchmancer.Achievements
{
    /// <summary>
    /// Inspector-facing title asset. One .asset per title under
    /// <c>Assets/Data/Titles/…</c>. Projects to a pure-C#
    /// <see cref="TitleDefinition"/> via <see cref="ToDefinition"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "TitleData", menuName = "Matchmancer/Title Data")]
    public class TitleData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Stable save id. Never rename after the title ships.")]
        public string id;
        public string displayName;
        [TextArea] public string description;

        [Header("Display")]
        public TitleRarity rarity = TitleRarity.Common;

        [Header("Unlock Source")]
        [Tooltip("Achievement id whose unlock will auto-grant this title. " +
                 "Empty means manual-unlock only.")]
        public string sourceAchievementId;

        public TitleDefinition ToDefinition()
        {
            return new TitleDefinition
            {
                Id                  = string.IsNullOrEmpty(id)          ? name : id,
                DisplayName         = string.IsNullOrEmpty(displayName) ? name : displayName,
                Description         = description ?? string.Empty,
                Rarity              = rarity,
                SourceAchievementId = sourceAchievementId ?? string.Empty,
            };
        }
    }
}
