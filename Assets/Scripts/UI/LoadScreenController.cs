using UnityEngine;

namespace Matchmancer.UI
{
    /// <summary>
    /// Boot splash / loading screen. Shown while the game initialises
    /// singletons (save load, asset warmup, etc). When done it tells the
    /// navigator to advance to <see cref="ScreenId.Login"/> or
    /// <see cref="ScreenId.MainHub"/> depending on whether a session exists.
    ///
    /// MVP: no async loading — just waits one frame then pushes forward.
    /// </summary>
    public class LoadScreenController : ScreenController
    {
        [Header("References")]
        [SerializeField] private ScreenNavigatorController navigator;

        [Header("Config")]
        [Tooltip("Skip the Login screen and go straight to MainHub in MVP.")]
        [SerializeField] private bool skipLogin = false;

        private bool _doneOnce;

        protected override void OnShow()
        {
            _doneOnce = false;
        }

        private void Update()
        {
            if (!IsVisible || _doneOnce) return;
            _doneOnce = true;

            // MVP: advance immediately.
            if (navigator == null)
            {
                Debug.LogWarning($"[{nameof(LoadScreenController)}] No navigator assigned.", this);
                return;
            }

            ScreenId target = skipLogin ? ScreenId.MainHub : ScreenId.Login;
            navigator.Navigator.Replace(target);
        }
    }
}
