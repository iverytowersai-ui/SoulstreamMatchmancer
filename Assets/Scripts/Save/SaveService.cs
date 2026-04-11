using System;
using Matchmancer.Achievements;
using Matchmancer.Boosters;
using Matchmancer.Character;
using Matchmancer.Progression;

namespace Matchmancer.Save
{
    /// <summary>
    /// Pure C# coordinator that knows how to snapshot and restore every
    /// persistent subsystem. SaveService takes a storage backend plus
    /// optional references to each subsystem — null subsystems are simply
    /// skipped, so partial scenes (e.g. a tests scene with only Boosters)
    /// still work.
    ///
    /// SaveService is intentionally NOT a MonoBehaviour. The
    /// <see cref="SaveManager"/> bridge collects the references from the
    /// scene and feeds them in.
    /// </summary>
    public class SaveService
    {
        private readonly ISaveStorage _storage;

        public string ProfileId { get; private set; } = "main";

        // Subsystem references — any may be null.
        public ProgressionState        Progression     { get; set; }
        public AchievementTracker      Achievements    { get; set; }
        public TitleSystem             Titles          { get; set; }
        public BoosterInventory        Boosters        { get; set; }
        public GearInventory           Gear            { get; set; }

        /// <summary>
        /// Resolver used by GearInventory.LoadFromSnapshot to map a tuning
        /// id back to a live GearTuning. Usually wired to
        /// <c>GearInventoryController.ResolveTuning</c>.
        /// </summary>
        public Func<string, GearTuning> GearTuningResolver { get; set; }

        // Telemetry
        public int  SaveCount { get; private set; }
        public int  LoadCount { get; private set; }
        public long LastSavedAtUnix { get; private set; }

        public event Action<SaveData> OnBeforeSave;
        public event Action<SaveData> OnAfterSave;
        public event Action<SaveData> OnAfterLoad;

        public SaveService(ISaveStorage storage, string profileId = "main")
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            if (!string.IsNullOrEmpty(profileId)) ProfileId = profileId;
        }

        // ---------- Snapshot building ----------

        /// <summary>
        /// Build a fresh <see cref="SaveData"/> from the currently-attached
        /// subsystems. Each subsystem snapshot is null if its reference is null.
        /// </summary>
        public SaveData BuildSaveData()
        {
            var data = new SaveData
            {
                Version     = SaveData.CurrentVersion,
                ProfileId   = ProfileId,
                SavedAtUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            };

            if (Progression  != null) data.Progression  = Progression.CreateSnapshot();
            if (Achievements != null) data.Achievements = Achievements.CreateSnapshot();
            if (Titles       != null) data.Titles       = Titles.CreateSnapshot();
            if (Boosters     != null) data.Boosters     = Boosters.CreateSnapshot();
            if (Gear         != null) data.Gear         = Gear.CreateSnapshot();

            return data;
        }

        /// <summary>
        /// Apply a previously-loaded <see cref="SaveData"/> to the attached
        /// subsystems. Missing sub-snapshots leave their subsystem untouched.
        /// </summary>
        public void ApplySaveData(SaveData data)
        {
            if (data == null) return;

            if (Progression  != null && data.Progression  != null) Progression.LoadFromSnapshot(data.Progression);
            if (Achievements != null && data.Achievements != null) Achievements.LoadFromSnapshot(data.Achievements);
            if (Titles       != null && data.Titles       != null) Titles.LoadFromSnapshot(data.Titles);
            if (Boosters     != null && data.Boosters     != null) Boosters.LoadFromSnapshot(data.Boosters);
            if (Gear         != null && data.Gear         != null && GearTuningResolver != null)
                Gear.LoadFromSnapshot(data.Gear, GearTuningResolver);
        }

        // ---------- Persistence ----------

        public bool HasSave => _storage.Exists(ProfileId);

        public void Save()
        {
            var data = BuildSaveData();
            OnBeforeSave?.Invoke(data);

            _storage.Save(ProfileId, data);

            SaveCount++;
            LastSavedAtUnix = data.SavedAtUnix;
            OnAfterSave?.Invoke(data);
        }

        /// <summary>
        /// Load the active profile and apply it to attached subsystems.
        /// Returns the loaded <see cref="SaveData"/>, or null if no save
        /// existed (or it was corrupt).
        /// </summary>
        public SaveData Load()
        {
            var data = _storage.Load(ProfileId);
            if (data == null) return null;

            ApplySaveData(data);
            LoadCount++;
            OnAfterLoad?.Invoke(data);
            return data;
        }

        public void Delete() => _storage.Delete(ProfileId);

        public void SetProfile(string profileId)
        {
            if (string.IsNullOrEmpty(profileId)) return;
            ProfileId = profileId;
        }
    }
}
