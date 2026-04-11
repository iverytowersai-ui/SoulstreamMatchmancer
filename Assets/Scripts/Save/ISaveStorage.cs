namespace Matchmancer.Save
{
    /// <summary>
    /// Storage backend for <see cref="SaveData"/>. Implementations may use
    /// PlayerPrefs, JSON files, encrypted blobs, or in-memory dictionaries
    /// (for tests). The contract is intentionally synchronous and tiny —
    /// SaveService coordinates threading/atomicity if needed.
    /// </summary>
    public interface ISaveStorage
    {
        /// <summary>True if a save exists for the given profile.</summary>
        bool Exists(string profileId);

        /// <summary>Load a save, or null if none exists / data is corrupt.</summary>
        SaveData Load(string profileId);

        /// <summary>Persist a save. Implementations should be atomic where possible.</summary>
        void Save(string profileId, SaveData data);

        /// <summary>Delete the save for a profile. No-op if it doesn't exist.</summary>
        void Delete(string profileId);
    }
}
