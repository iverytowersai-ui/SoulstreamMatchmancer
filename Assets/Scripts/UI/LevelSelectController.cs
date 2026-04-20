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
            if (levelButtonPrefab == null || contentParent == null || progressionManager == null) return;

            // Clear previous nodes
            foreach (Transform child in contentParent)
            {
                Destroy(child.gameObject);
            }

            int stateCurrentGlobal = progressionManager.CurrentGlobalIndex;
            float yPos = 50f;
            float spacingY = 220f;
            float colLeft = -250f;
            float colCenter = 0f;
            float colRight = 250f;

            for (int i = 0; i < 10; i++)
            {
                int levelIndex = i + 1;
                int globalIndex = LevelIndexing.ToGlobalIndex(stageIndex, levelIndex);

                // Math for S-Curve
                float xPos = colCenter;
                int pattern = i % 6;
                if (pattern == 1 || pattern == 2) xPos = colRight;
                else if (pattern == 4 || pattern == 5) xPos = colLeft;

                Vector2 pos = new Vector2(xPos, yPos + (i * spacingY));

                bool isLocked = !progressionManager.State.IsLevelUnlocked(globalIndex);
                bool isCurrent = globalIndex == stateCurrentGlobal;

                // Grab stars if completed (0..5)
                int bestStars = progressionManager.State.GetStars(globalIndex);

                var vm = new LevelNodeViewModel
                {
                    levelNumber = globalIndex, // Print global like 1-100
                    isLocked = isLocked,
                    isCurrent = isCurrent,
                    bestStars = bestStars
                };

                GameObject go = Instantiate(levelButtonPrefab, contentParent);
                LevelNodeController node = go.GetComponent<LevelNodeController>();
                if (node != null)
                {
                    node.Bind(vm, pos, OnNodeClicked);
                }
            }
            
            // Adjust content parent height to fit map
            RectTransform rt = contentParent.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, 100f + (10 * spacingY));
            }
        }

        private void OnNodeClicked(LevelNodeViewModel data)
        {
            if (data.isLocked || progressionManager == null || navigator == null) return;

            int stage = LevelIndexing.StageOf(data.levelNumber);
            int level = LevelIndexing.LevelOf(data.levelNumber);
            progressionManager.SetCurrentLevel(stage, level);
            navigator.Navigator.Push(ScreenId.Battle);
        }

        /// <summary>Called by instantiated level buttons (Legacy UnityEvent fallback).</summary>
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
