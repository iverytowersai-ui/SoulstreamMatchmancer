using UnityEngine;
using UnityEngine.UI;

namespace Matchmancer.UI
{
    /// <summary>
    /// MVP guest-only login. Shows a full-screen background with a
    /// "Play as Guest" button. Tap it → replace with MainHub.
    /// Future: add social sign-in buttons.
    /// </summary>
    public class LoginScreenController : ScreenController
    {
        [Header("References")]
        [SerializeField] private ScreenNavigatorController navigator;
        [SerializeField] private Button playAsGuestButton;

        [Header("Background")]
        [Tooltip("Full-screen background Image component. Assign the LoginScreen sprite to this Image's Source Image.")]
        [SerializeField] private Image backgroundImage;

        private void OnEnable()
        {
            if (playAsGuestButton != null)
                playAsGuestButton.onClick.AddListener(HandleGuestLogin);
        }

        private void OnDisable()
        {
            if (playAsGuestButton != null)
                playAsGuestButton.onClick.RemoveListener(HandleGuestLogin);
        }

        private void HandleGuestLogin()
        {
            if (navigator == null) return;
            navigator.Navigator.Replace(ScreenId.MainHub);
        }
    }
}
