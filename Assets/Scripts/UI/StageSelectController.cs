using UnityEngine;
using UnityEngine.UI;
using Matchmancer.Progression;

namespace Matchmancer.UI
{
    /// <summary>
    /// Shows the 10 stages as a scrollable list. Each button shows:
    ///   • Stage name + art (from StageData)
    ///   • Lock/unlock state (from ProgressionState)
    ///   • Star count for all levels in the stage
    ///
    /// Tapping an unlocked stage pushes <see cref="ScreenId.LevelSelect"/>.
    /// Back button pops to MainHub.
    /// </summary>
    public class StageSelectController : ScreenController
    {
        [Header("References")]
        [SerializeField] private ScreenNavigatorController navigator;
        [SerializeField] private LevelProgressionManager   progressionManager;

        [Header("Prefab")]
        [Tooltip("Instantiated once per stage. Must have a Button + child text fields.")]
        [SerializeField] private GameObject stageButtonPrefab;

        [Header("Layout")]
        [SerializeField] private Transform contentParent;
        [SerializeField] private Button    backButton;

        /// <summary>Stage the player last selected. Read by LevelSelectController.</summary>
        public int SelectedStageIndex { get; private set; } = 1;

        private void OnEnable()
        {
            if (backButton != null)
                backButton.onClick.AddListener(HandleBack);
        }

        private void OnDisable()
        {
            if (backButton != null)
                backButton.onClick.RemoveListener(HandleBack);
        }

        protected override void OnShow()
        {
            // Future: rebuild stage buttons from progressionManager.
            // For now, each button calls HandleStageSelected(stageIndex).
        }

        /// <summary>Called by instantiated stage buttons.</summary>
        public void HandleStageSelected(int stageIndex)
        {
            if (progressionManager != null &&
                !progressionManager.State.IsStageUnlocked(stageIndex))
                return; // locked — ignore tap

            SelectedStageIndex = stageIndex;

            if (navigator != null)
                navigator.Navigator.Push(ScreenId.LevelSelect);
        }

        private void HandleBack()
        {
            if (navigator != null)
                navigator.Navigator.Pop();
        }
    }
}
