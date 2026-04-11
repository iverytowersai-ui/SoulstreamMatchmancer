using UnityEngine;
using UnityEngine.UI;

namespace Matchmancer.UI
{
    /// <summary>
    /// Central hub screen. Shows buttons for:
    ///   • Play (→ StageSelect)
    ///   • Gear (→ CharacterGear)
    ///   • Shop (→ Shop)
    ///   • Achievements (→ Achievements)
    ///   • Lore (→ LoreGallery)
    ///   • Settings (→ Settings)
    ///
    /// Also displays the player's current title, gold balance, and
    /// character portrait (wired in Skill 24 when art exists).
    /// </summary>
    public class MainHubController : ScreenController
    {
        [Header("References")]
        [SerializeField] private ScreenNavigatorController navigator;

        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button gearButton;
        [SerializeField] private Button shopButton;
        [SerializeField] private Button achievementsButton;
        [SerializeField] private Button loreButton;
        [SerializeField] private Button settingsButton;

        private void OnEnable()
        {
            Bind(playButton,         ScreenId.StageSelect);
            Bind(gearButton,         ScreenId.CharacterGear);
            Bind(shopButton,         ScreenId.Shop);
            Bind(achievementsButton, ScreenId.Achievements);
            Bind(loreButton,         ScreenId.LoreGallery);
            Bind(settingsButton,     ScreenId.Settings);
        }

        private void OnDisable()
        {
            Unbind(playButton);
            Unbind(gearButton);
            Unbind(shopButton);
            Unbind(achievementsButton);
            Unbind(loreButton);
            Unbind(settingsButton);
        }

        private void Bind(Button btn, ScreenId target)
        {
            if (btn == null || navigator == null) return;
            btn.onClick.AddListener(() => navigator.Navigator.Push(target));
        }

        private void Unbind(Button btn)
        {
            if (btn != null) btn.onClick.RemoveAllListeners();
        }

        protected override void OnShow()
        {
            // Refresh gold / title / portrait here when those systems exist.
        }
    }
}
