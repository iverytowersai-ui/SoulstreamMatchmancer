using System.Collections.Generic;
using UnityEngine;

namespace Matchmancer.UI
{
    /// <summary>
    /// Scene-side owner of the pure-C# <see cref="ScreenNavigator"/>.
    /// Maps <see cref="ScreenId"/> → <see cref="ScreenController"/> and
    /// calls <c>Show()</c> / <c>Hide()</c> when the nav state changes.
    ///
    /// Setup:
    ///   1. Drop this on a root Canvas or manager object.
    ///   2. Assign every screen controller to <see cref="screens"/>.
    ///   3. Set <see cref="initialScreen"/> (usually Loading).
    ///   4. Each screen's GameObject should start disabled.
    ///
    /// At Awake the controller builds the lookup, pushes the initial
    /// screen, and begins responding to navigation events.
    /// </summary>
    [DisallowMultipleComponent]
    public class ScreenNavigatorController : MonoBehaviour
    {
        [Header("Screens")]
        [Tooltip("Every ScreenController in the game. Order does not matter.")]
        [SerializeField] private ScreenController[] screens;

        [Header("Boot")]
        [SerializeField] private ScreenId initialScreen = ScreenId.Loading;

        public ScreenNavigator Navigator { get; private set; }

        private readonly Dictionary<ScreenId, ScreenController> _map =
            new Dictionary<ScreenId, ScreenController>();

        private ScreenId _visibleId = ScreenId.None;

        // ------------------------------------------------------------------
        // Lifecycle
        // ------------------------------------------------------------------

        private void Awake()
        {
            Navigator = new ScreenNavigator();

            if (screens != null)
            {
                foreach (var sc in screens)
                {
                    if (sc == null) continue;
                    if (_map.ContainsKey(sc.ScreenId))
                    {
                        Debug.LogWarning(
                            $"[{nameof(ScreenNavigatorController)}] Duplicate ScreenId '{sc.ScreenId}' — keeping first.",
                            sc);
                        continue;
                    }
                    _map[sc.ScreenId] = sc;
                    sc.gameObject.SetActive(false);
                }
            }

            Navigator.OnScreenChanged += HandleScreenChanged;
        }

        private void Start()
        {
            if (initialScreen != ScreenId.None)
                Navigator.Push(initialScreen);
        }

        private void OnDestroy()
        {
            if (Navigator != null)
                Navigator.OnScreenChanged -= HandleScreenChanged;
        }

        // ------------------------------------------------------------------
        // Navigation handler
        // ------------------------------------------------------------------

        private void HandleScreenChanged(ScreenId previous, ScreenId next)
        {
            if (previous != ScreenId.None && _map.TryGetValue(previous, out var prevCtrl))
                prevCtrl.Hide();

            if (next != ScreenId.None && _map.TryGetValue(next, out var nextCtrl))
                nextCtrl.Show();
            else if (next != ScreenId.None)
                Debug.LogWarning(
                    $"[{nameof(ScreenNavigatorController)}] No controller for ScreenId '{next}'.",
                    this);

            _visibleId = next;
        }

        // ------------------------------------------------------------------
        // Convenience
        // ------------------------------------------------------------------

        /// <summary>Return to the main hub from any depth.</summary>
        public void GoToHub() => Navigator.ClearTo(ScreenId.MainHub);

        /// <summary>Navigate back one screen.</summary>
        public void GoBack() => Navigator.Pop();

        /// <summary>Look up a controller by id (or null if not registered).</summary>
        public ScreenController GetScreen(ScreenId id)
        {
            _map.TryGetValue(id, out var ctrl);
            return ctrl;
        }
    }
}
