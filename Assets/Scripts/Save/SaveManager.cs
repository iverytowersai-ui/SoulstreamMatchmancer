using System.IO;
using UnityEngine;
using Matchmancer.Achievements;
using Matchmancer.Boosters;
using Matchmancer.Character;
using Matchmancer.Progression;

namespace Matchmancer.Save
{
    /// <summary>
    /// Scene-side bridge that constructs a <see cref="SaveService"/>, wires
    /// it to the live subsystems found in the scene, and exposes Save/Load
    /// to the rest of the game. DontDestroyOnLoad singleton — persists
    /// across scene loads alongside <see cref="LevelProgressionManager"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        [Header("Profile")]
        [SerializeField] private string profileId = "main";

        [Header("Subsystem References (drag from scene)")]
        [SerializeField] private LevelProgressionManager  progressionManager;
        [SerializeField] private AchievementController    achievementController;
        [SerializeField] private BoosterController        boosterController;
        [SerializeField] private GearInventoryController  gearInventoryController;

        [Header("Behavior")]
        [Tooltip("Auto-load the save (if any) on Awake.")]
        [SerializeField] private bool autoLoadOnAwake = true;
        [Tooltip("Auto-save when the application is paused or quits.")]
        [SerializeField] private bool autoSaveOnLifecycle = true;

        public SaveService Service { get; private set; }

        // ----- Lifecycle -----

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            var rootDir = Path.Combine(Application.persistentDataPath, "Saves");
            var storage = new JsonFileSaveStorage(rootDir);
            Service = new SaveService(storage, profileId);

            WireSubsystems();

            if (autoLoadOnAwake) TryLoad();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && autoSaveOnLifecycle && Service != null) Service.Save();
        }

        private void OnApplicationQuit()
        {
            if (autoSaveOnLifecycle && Service != null) Service.Save();
        }

        // ----- Wiring -----

        private void WireSubsystems()
        {
            if (Service == null) return;

            if (progressionManager != null)
                Service.Progression = progressionManager.State;

            if (achievementController != null)
            {
                Service.Achievements = achievementController.Tracker;
                Service.Titles       = achievementController.Titles;
            }

            if (boosterController != null)
                Service.Boosters = boosterController.Inventory;

            if (gearInventoryController != null)
            {
                Service.Gear = gearInventoryController.Inventory;
                Service.GearTuningResolver = gearInventoryController.ResolveTuning;
            }
        }

        // ----- Public API -----

        public void TrySave()
        {
            if (Service == null) return;
            Service.Save();
        }

        public SaveData TryLoad()
        {
            if (Service == null) return null;
            return Service.Load();
        }

        public void DeleteSave()
        {
            if (Service == null) return;
            Service.Delete();
        }

        public bool HasSave => Service != null && Service.HasSave;
    }
}
