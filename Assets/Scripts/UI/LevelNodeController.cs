using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Matchmancer.UI
{
    /// <summary>
    /// Individual node in the level select map.
    /// Handles visual states (locked, unlocked, current, completed)
    /// and calculates its own S-curve position based on level index.
    /// </summary>
    public class LevelNodeController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image           _backgroundFill;
        [SerializeField] private Image           _borderRing;
        [SerializeField] private TextMeshProUGUI _numberText;
        [SerializeField] private GameObject      _glowOverlay;
        [SerializeField] private GameObject      _checkmarkIcon;
        [SerializeField] private GameObject      _lockIcon;
        [SerializeField] private GameObject[]    _starIcons; // Standard 3 stars

        [Header("Layout Tuning")]
        [SerializeField] private float _columnLeft   = 270f;
        [SerializeField] private float _columnCenter = 540f;
        [SerializeField] private float _columnRight  = 810f;
        [SerializeField] private float _vSpacing     = 140f;

        private LevelNodeViewModel _data;
        private Action<LevelNodeViewModel> _onClicked;

        /// <summary>
        /// Binds the node to data and positions it.
        /// </summary>
        public void Bind(LevelNodeViewModel data, Vector2 anchoredPosition, Action<LevelNodeViewModel> onClicked)
        {
            _data = data;
            _onClicked = onClicked;

            UpdateVisuals();
            
            RectTransform rt = GetComponent<RectTransform>();
            if (rt != null) rt.anchoredPosition = anchoredPosition;
        }

        private void UpdateVisuals()
        {
            if (_numberText != null) _numberText.text = _data.levelNumber.ToString();

            // Reset all
            if (_lockIcon != null) _lockIcon.SetActive(false);
            if (_checkmarkIcon != null) _checkmarkIcon.SetActive(false);
            if (_glowOverlay != null) _glowOverlay.SetActive(false);

            // Handle States
            if (_data.isLocked)
            {
                SetLockedState();
            }
            else if (_data.isCurrent)
            {
                SetCurrentState();
            }
            else if (_data.bestStars > 0)
            {
                SetCompletedState();
            }
            else
            {
                SetUnlockedState();
            }

            UpdateStars();
        }

        private void SetLockedState()
        {
            if (_lockIcon != null) _lockIcon.SetActive(true);
            // Visual tokens would be applied here (dim colors, etc.)
        }

        private void SetUnlockedState()
        {
            // Standard unlocked visuals
        }

        private void SetCurrentState()
        {
            if (_glowOverlay != null) _glowOverlay.SetActive(true);
            // Pulse logic would be in Update or a Tween
        }

        private void SetCompletedState()
        {
            if (_checkmarkIcon != null) _checkmarkIcon.SetActive(true);
        }

        private void UpdateStars()
        {
            if (_starIcons == null) return;
            for (int i = 0; i < _starIcons.Length; i++)
            {
                if (_data.isLocked)
                    _starIcons[i].SetActive(false);
                else
                    _starIcons[i].SetActive(true); // Outline/Fill handled by child components or tint
            }
        }

        public void OnPointerClick()
        {
            _onClicked?.Invoke(_data);
        }
    }
}
