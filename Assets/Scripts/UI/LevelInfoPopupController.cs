using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Matchmancer.UI
{
    /// <summary>
    /// Bottom sheet popup that displays level details and allows the player
    /// to start a battle or replay a level.
    /// </summary>
    public class LevelInfoPopupController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _statsText; // "Moves: 25 | Enemy HP: 1400"
        [SerializeField] private TextMeshProUGUI _specialText; // "Special: Armor tiles"
        [SerializeField] private Image[]         _starIcons;
        
        [Header("Buttons")]
        [SerializeField] private Button _battleButton;
        [SerializeField] private Button _replayButton;
        [SerializeField] private Button _closeButton;

        [Header("Animation")]
        [SerializeField] private float _slideDuration = 0.3f;
        [SerializeField] private float _offscreenY     = -500f;
        [SerializeField] private float _onscreenY      = 0f;

        private LevelNodeViewModel _data;
        private Action<int> _onBattleStart; // globalIndex
        private RectTransform _rectTransform;
        private bool _isShowing;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _rectTransform.anchoredPosition = new Vector2(0, _offscreenY);
            gameObject.SetActive(false);

            if (_battleButton != null) _battleButton.onClick.AddListener(() => StartBattle(false));
            if (_replayButton != null) _replayButton.onClick.AddListener(() => StartBattle(true));
            if (_closeButton  != null) _closeButton.onClick.AddListener(Hide);
        }

        public void Show(LevelNodeViewModel data, Action<int> onBattleStart)
        {
            _data = data;
            _onBattleStart = onBattleStart;

            if (_titleText != null) _titleText.text = $"LEVEL {data.levelNumber} — {data.enemyName}";
            if (_statsText != null) _statsText.text = $"Moves: {data.moveLimit} | Enemy HP: {data.enemyHP}";
            if (_specialText != null) _specialText.text = $"Special: {data.specialCondition}";

            if (_replayButton != null) _replayButton.gameObject.SetActive(data.bestStars > 0);

            UpdateStars();
            
            gameObject.SetActive(true);
            _isShowing = true;
            StopAllCoroutines();
            StartCoroutine(SlideAnimation(_onscreenY));
        }

        public void Hide()
        {
            if (!_isShowing) return;
            _isShowing = false;
            StopAllCoroutines();
            StartCoroutine(SlideAnimation(_offscreenY, () => gameObject.SetActive(false)));
        }

        private void UpdateStars()
        {
            if (_starIcons == null) return;
            for (int i = 0; i < _starIcons.Length; i++)
            {
                // Star display logic (fill color based on bestStars)
                _starIcons[i].color = (i < _data.bestStars) ? Color.yellow : Color.gray;
            }
        }

        private void StartBattle(bool isReplay)
        {
            _onBattleStart?.Invoke(_data.levelNumber);
            Hide();
        }

        private System.Collections.IEnumerator SlideAnimation(float targetY, Action onComplete = null)
        {
            float startY = _rectTransform.anchoredPosition.y;
            float elapsed = 0;
            while (elapsed < _slideDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _slideDuration;
                // Simple ease-out
                t = 1f - Mathf.Pow(1f - t, 3f); 
                float y = Mathf.Lerp(startY, targetY, t);
                _rectTransform.anchoredPosition = new Vector2(0, y);
                yield return null;
            }
            _rectTransform.anchoredPosition = new Vector2(0, targetY);
            onComplete?.Invoke();
        }
    }
}
