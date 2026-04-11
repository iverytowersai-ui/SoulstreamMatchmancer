using UnityEngine;

namespace Matchmancer.UI
{
    /// <summary>
    /// Base class for every screen in the game. Subclasses implement
    /// <see cref="OnShow"/> / <see cref="OnHide"/> for their specific
    /// setup/teardown. The <see cref="ScreenNavigatorController"/> calls
    /// <see cref="Show"/> / <see cref="Hide"/> based on nav events.
    ///
    /// Convention: each screen's root GameObject starts disabled.
    /// <see cref="Show"/> activates it; <see cref="Hide"/> deactivates.
    /// Override <see cref="OnShow"/> to refresh data every time the screen
    /// becomes visible (e.g. re-read progression after a battle).
    /// </summary>
    public abstract class ScreenController : MonoBehaviour
    {
        [Header("Screen Identity")]
        [SerializeField] private ScreenId screenId;

        public ScreenId ScreenId => screenId;

        /// <summary>True while this screen's GameObject is active.</summary>
        public bool IsVisible { get; private set; }

        public void Show()
        {
            gameObject.SetActive(true);
            IsVisible = true;
            OnShow();
        }

        public void Hide()
        {
            OnHide();
            IsVisible = false;
            gameObject.SetActive(false);
        }

        /// <summary>Called after the screen becomes visible. Override to refresh data.</summary>
        protected virtual void OnShow() { }

        /// <summary>Called before the screen is hidden. Override to clean up.</summary>
        protected virtual void OnHide() { }
    }
}
