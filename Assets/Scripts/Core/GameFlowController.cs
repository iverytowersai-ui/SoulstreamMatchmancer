using UnityEngine;
using Matchmancer.UI;
using Matchmancer.Progression;
using Matchmancer.Character;
using Matchmancer.Save;

namespace Matchmancer.Core
{
    /// <summary>
    /// High-level flow controller that connects screen navigation to game actions.
    ///
    /// This is the bridge between the UI layer (ScreenNavigator) and the game
    /// systems (BattleTurnLoop, LevelProgressionManager, SaveManager). It
    /// handles the full player journey:
    ///
    ///   Loading → Login → MainHub → StageSelect → LevelSelect → Battle → Results → (loop)
    ///
    /// Attach to the same root GameObject as <see cref="ScreenNavigatorController"/>.
    /// Wire the Inspector references or let Awake find siblings.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameFlowController : MonoBehaviour
    {
        // =====================================================================
        // Inspector
        // =====================================================================

        [Header("Navigation")]
        [SerializeField] private ScreenNavigatorController screenNav;

        [Header("Battle")]
        [SerializeField] private BattleTurnLoop battleTurnLoop;

        [Header("Progression")]
        [SerializeField] private LevelProgressionManager progressionManager;

        [Header("Default Character (MVP — single character)")]
        [SerializeField] private CharacterData defaultCharacter;

        // =====================================================================
        // State
        // =====================================================================

        private LevelData _selectedLevel;
        private CharacterData _selectedCharacter;
        private int _selectedStageIndex;

        // =====================================================================
        // Properties
        // =====================================================================

        /// <summary>Currently selected level (null if none).</summary>
        public LevelData SelectedLevel => _selectedLevel;

        /// <summary>Currently selected character.</summary>
        public CharacterData SelectedCharacter => _selectedCharacter;

        /// <summary>Currently selected stage index (1-based).</summary>
        public int SelectedStageIndex => _selectedStageIndex;

        // =====================================================================
        // Lifecycle
        // =====================================================================

        private void Awake()
        {
            if (!screenNav)         screenNav         = GetComponent<ScreenNavigatorController>();
            if (!battleTurnLoop)    battleTurnLoop    = FindObjectOfType<BattleTurnLoop>();
            if (!progressionManager) progressionManager = FindObjectOfType<LevelProgressionManager>();

            _selectedCharacter = defaultCharacter;
        }

        private void OnEnable()
        {
            if (battleTurnLoop != null)
            {
                battleTurnLoop.OnBattleComplete += HandleBattleComplete;
            }
        }

        private void OnDisable()
        {
            if (battleTurnLoop != null)
            {
                battleTurnLoop.OnBattleComplete -= HandleBattleComplete;
            }
        }

        // =====================================================================
        // Boot sequence
        // =====================================================================

        /// <summary>
        /// Call from LoadScreenController when loading finishes.
        /// Decides whether to show login or skip to hub.
        /// </summary>
        public void OnLoadingComplete()
        {
            // MVP: skip login, go straight to hub
            screenNav.Navigator.Replace(ScreenId.MainHub);
        }

        /// <summary>
        /// Call from LoginScreenController after guest or auth flow completes.
        /// </summary>
        public void OnLoginComplete()
        {
            screenNav.Navigator.Replace(ScreenId.MainHub);
        }

        // =====================================================================
        // Hub → Stage → Level flow
        // =====================================================================

        /// <summary>Open the stage selection screen.</summary>
        public void OpenStageSelect()
        {
            screenNav.Navigator.Push(ScreenId.StageSelect);
        }

        /// <summary>
        /// Select a stage and open the level list.
        /// Called by StageSelectController when a stage node is tapped.
        /// </summary>
        public void SelectStage(int stageIndex)
        {
            _selectedStageIndex = stageIndex;
            screenNav.Navigator.Push(ScreenId.LevelSelect);
        }

        /// <summary>
        /// Select a level and launch the battle.
        /// Called by LevelSelectController when a level node is tapped.
        /// </summary>
        public void SelectLevelAndPlay(LevelData level)
        {
            if (level == null)
            {
                Debug.LogError("[GameFlowController] Cannot start battle — null LevelData.");
                return;
            }

            _selectedLevel = level;

            // Navigate to battle screen
            screenNav.Navigator.Push(ScreenId.Battle);

            // Start the battle
            if (battleTurnLoop != null && _selectedCharacter != null)
            {
                battleTurnLoop.StartBattle(level, _selectedCharacter);
                Debug.Log($"[GameFlowController] Launching {level.displayName} with {_selectedCharacter.displayName}");
            }
            else
            {
                Debug.LogError("[GameFlowController] Missing BattleTurnLoop or CharacterData.");
            }
        }

        // =====================================================================
        // Character selection (MVP: single character, future: roster screen)
        // =====================================================================

        /// <summary>
        /// Set the active character. Called from roster screen or character
        /// selection UI.
        /// </summary>
        public void SelectCharacter(CharacterData character)
        {
            if (character == null) return;
            _selectedCharacter = character;
            Debug.Log($"[GameFlowController] Selected character: {character.displayName}");
        }

        // =====================================================================
        // Battle completion
        // =====================================================================

        private void HandleBattleComplete(BattleResult result)
        {
            Debug.Log($"[GameFlowController] Battle complete: {result}");

            if (result.Victory)
            {
                // Save progress
                SaveVictory(result);
            }

            // Navigate to results screen
            screenNav.Navigator.Replace(ScreenId.Results);
        }

        /// <summary>
        /// Called from Results screen "Next Level" button.
        /// </summary>
        public void GoToNextLevel()
        {
            if (_selectedLevel == null || progressionManager == null)
            {
                ReturnToLevelSelect();
                return;
            }

            int nextGlobal = _selectedLevel.globalIndex + 1;
            LevelData nextLevel = progressionManager.GetLevel(nextGlobal);

            if (nextLevel != null)
            {
                // Go directly to next battle
                screenNav.Navigator.Replace(ScreenId.Battle);
                _selectedLevel = nextLevel;
                battleTurnLoop.StartBattle(nextLevel, _selectedCharacter);
            }
            else
            {
                // No more levels — return to hub
                ReturnToHub();
            }
        }

        /// <summary>
        /// Called from Results screen "Replay" button.
        /// </summary>
        public void ReplayCurrentLevel()
        {
            if (_selectedLevel == null || battleTurnLoop == null)
            {
                ReturnToLevelSelect();
                return;
            }

            screenNav.Navigator.Replace(ScreenId.Battle);
            battleTurnLoop.StartBattle(_selectedLevel, _selectedCharacter);
        }

        /// <summary>
        /// Called from Results screen "Back to Levels" button.
        /// </summary>
        public void ReturnToLevelSelect()
        {
            screenNav.Navigator.ClearTo(ScreenId.MainHub);
            screenNav.Navigator.Push(ScreenId.StageSelect);
            screenNav.Navigator.Push(ScreenId.LevelSelect);
        }

        // =====================================================================
        // Battle control passthrough
        // =====================================================================

        /// <summary>Pause the active battle and show pause overlay.</summary>
        public void PauseBattle()
        {
            battleTurnLoop?.Pause();
            screenNav.Navigator.Push(ScreenId.Pause);
        }

        /// <summary>Resume from pause.</summary>
        public void ResumeBattle()
        {
            screenNav.Navigator.Pop(); // remove Pause overlay
            battleTurnLoop?.Resume();
        }

        /// <summary>Quit battle from pause menu.</summary>
        public void QuitBattle()
        {
            battleTurnLoop?.Quit();
            screenNav.Navigator.ClearTo(ScreenId.MainHub);
        }

        /// <summary>Retry battle from pause menu or defeat screen.</summary>
        public void RetryBattle()
        {
            // Pop back to battle screen if we're on pause/results
            if (screenNav.Navigator.Current != ScreenId.Battle)
            {
                screenNav.Navigator.ClearTo(ScreenId.Battle);
            }
            battleTurnLoop?.Retry();
        }

        // =====================================================================
        // Hub menu actions
        // =====================================================================

        /// <summary>Return to main hub from anywhere.</summary>
        public void ReturnToHub()
        {
            screenNav.Navigator.ClearTo(ScreenId.MainHub);
        }

        /// <summary>Open the shop screen.</summary>
        public void OpenShop()
        {
            screenNav.Navigator.Push(ScreenId.Shop);
        }

        /// <summary>Open the lore/gallery screen.</summary>
        public void OpenLoreGallery()
        {
            screenNav.Navigator.Push(ScreenId.LoreGallery);
        }

        /// <summary>Open achievements screen.</summary>
        public void OpenAchievements()
        {
            screenNav.Navigator.Push(ScreenId.Achievements);
        }

        /// <summary>Open settings screen.</summary>
        public void OpenSettings()
        {
            screenNav.Navigator.Push(ScreenId.Settings);
        }

        /// <summary>Open character/gear screen.</summary>
        public void OpenCharacterGear()
        {
            screenNav.Navigator.Push(ScreenId.CharacterGear);
        }

        // =====================================================================
        // Save helpers
        // =====================================================================

        private void SaveVictory(BattleResult result)
        {
            int stars = 0;
            if (_selectedLevel != null)
            {
                if (result.FinalScore >= _selectedLevel.fiveStar) stars = 5;
                else if (result.FinalScore >= _selectedLevel.fourStar) stars = 4;
                else if (result.FinalScore >= _selectedLevel.threeStar) stars = 3;
                else if (result.FinalScore >= _selectedLevel.twoStar) stars = 2;
                else if (result.FinalScore >= _selectedLevel.oneStar) stars = 1;
            }

            Debug.Log($"[GameFlowController] Victory — Level {result.GlobalLevelIndex}, " +
                      $"Score {result.FinalScore}, Stars {stars}");

            if (SaveManager.Instance != null)
                SaveManager.Instance.TrySave();
        }
    }
}
