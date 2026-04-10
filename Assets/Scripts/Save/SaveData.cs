using System;
using Matchmancer.Progression;
using Matchmancer.Achievements;
using Matchmancer.Boosters;
using Matchmancer.Character;
using Matchmancer.Shop;

namespace Matchmancer.Save
{
    /// <summary>
    /// Top-level save record. Aggregates the existing nested Snapshot types
    /// from every persistent system. Add new sub-snapshot fields here as
    /// systems gain save support — bump <see cref="CurrentVersion"/> whenever
    /// the layout changes in a non-additive way.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        /// <summary>
        /// Bump on every breaking change to the save layout. Additive fields
        /// (new arrays/values defaulted to zero/null) do NOT need a version
        /// bump because JsonUtility tolerates missing fields.
        /// </summary>
        public const int CurrentVersion = 1;

        public int    Version = CurrentVersion;
        public long   SavedAtUnix;
        public string ProfileId = "main";

        // ----- Sub-snapshots (nested types from each system) -----
        public ProgressionState.Snapshot   Progression;
        public AchievementTracker.Snapshot Achievements;
        public TitleSystem.Snapshot        Titles;
        public BoosterInventorySnapshot    Boosters;
        public GearInventorySnapshot       Gear;
        public WalletSnapshot              Wallet;
        public ShopSnapshot                Shop;

        public bool IsEmpty =>
            Progression  == null &&
            Achievements == null &&
            Titles       == null &&
            Boosters     == null &&
            Gear         == null &&
            Wallet       == null &&
            Shop         == null;

        public static SaveData CreateEmpty(string profileId = "main")
        {
            return new SaveData
            {
                Version     = CurrentVersion,
                ProfileId   = profileId,
                SavedAtUnix = 0,
            };
        }
    }
}
