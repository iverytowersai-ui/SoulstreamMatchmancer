using System;
using System.IO;
using UnityEngine;

namespace Matchmancer.Save
{
    /// <summary>
    /// JSON-on-disk storage. Writes to <c>{rootDir}/{profileId}.save.json</c>
    /// using a temp-file + File.Replace pattern so a crash mid-write cannot
    /// leave a half-written save. Pure C# / System.IO — safe to construct
    /// outside of MonoBehaviour land.
    /// </summary>
    public class JsonFileSaveStorage : ISaveStorage
    {
        private const string Extension = ".save.json";
        private const string TempExt   = ".save.tmp";

        private readonly string _rootDir;

        public string RootDirectory => _rootDir;

        /// <summary>
        /// Construct with an explicit root directory (used by tests). Pass
        /// <c>Application.persistentDataPath</c> at runtime.
        /// </summary>
        public JsonFileSaveStorage(string rootDirectory)
        {
            if (string.IsNullOrEmpty(rootDirectory))
                throw new ArgumentException("rootDirectory cannot be null/empty", nameof(rootDirectory));
            _rootDir = rootDirectory;
            Directory.CreateDirectory(_rootDir);
        }

        private string PathFor(string profileId) =>
            Path.Combine(_rootDir, profileId + Extension);

        private string TempPathFor(string profileId) =>
            Path.Combine(_rootDir, profileId + TempExt);

        public bool Exists(string profileId)
        {
            if (string.IsNullOrEmpty(profileId)) return false;
            return File.Exists(PathFor(profileId));
        }

        public SaveData Load(string profileId)
        {
            if (string.IsNullOrEmpty(profileId)) return null;
            var path = PathFor(profileId);
            if (!File.Exists(path)) return null;
            try
            {
                var json = File.ReadAllText(path);
                if (string.IsNullOrEmpty(json)) return null;
                return JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[JsonFileSaveStorage] Failed to load '{path}': {ex.Message}");
                return null;
            }
        }

        public void Save(string profileId, SaveData data)
        {
            if (string.IsNullOrEmpty(profileId) || data == null) return;
            var finalPath = PathFor(profileId);
            var tempPath  = TempPathFor(profileId);

            try
            {
                var json = JsonUtility.ToJson(data);
                File.WriteAllText(tempPath, json);

                if (File.Exists(finalPath))
                {
                    // Atomic swap on platforms that support it; falls back
                    // to delete+move if the OS can't do an in-place replace.
                    File.Replace(tempPath, finalPath, null);
                }
                else
                {
                    File.Move(tempPath, finalPath);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JsonFileSaveStorage] Failed to save '{finalPath}': {ex.Message}");
                try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch { /* swallow */ }
                throw;
            }
        }

        public void Delete(string profileId)
        {
            if (string.IsNullOrEmpty(profileId)) return;
            var path = PathFor(profileId);
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[JsonFileSaveStorage] Failed to delete '{path}': {ex.Message}");
            }
        }
    }
}
