using UnityEngine;
using UnityEngine.UI;
using Matchmancer.Progression;

namespace Matchmancer.UI
{
    /// <summary>
    /// Shows the 10 levels inside the selected stage. Each button shows:
    ///   • Level number
    ///   • Star rating (0-5 filled stars)
    ///   • Lock / unlock indicator
    ///   • 5-star badge if achieved
    ///
    /// Tapping an unlocked level sets the progression manager's current
    /// level and pushes <see cref="ScreenId.Battle"/>.
    /// Back pops to StageSelect.
    /// </summary>
    public class LevelSelectController : ScreenController
    {
        [Header("References")]
        [SerializeField] private ScreenNavigatorController navigator;
        [SerializeField] private LevelProgressionManager   progressionManager;
        [SerializeField] private StageSelectController     stageSelect;

        [Header("Prefab")]
        [SerializeField] private GameObject levelButtonPrefab;

        [Header("Layout")]
        [SerializeField] private Transform contentParent;
        [SerializeField] private Button    backButton;

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
            int stage = stageSelect != null ? stageSelect.SelectedStageIndex : 1;
            RebuildLevelButtons(stage);
        }

        private void RebuildLevelButtons(int stageIndex)
        {
            // Future: instantiate / reuse 10 level buttons from prefab.
            // Each reads progressionManager.State for star count and lock state.
        }

        /// <summary>Called by instantiated level buttons.</summary>
        public void HandleLevelSelected(int stageIndex, int levelIndex)
        {
            if (progressionManager == null || navigator == null) return;

            int global = LevelIndexing.ToGlobalIndex(stageIndex, levelIndex);
            if (!progressionManager.State.IsLevelUnlocked(global)) return;

            progressionManager.SetCurrentLevel(stageIndex, levelIndex);
            navigator.Navigator.Push(ScreenId.Battle);
        }

        private void HandleBack()
        {
            if (navigator != null)
                navigator.Navigator.Pop();
        }
    }
}
