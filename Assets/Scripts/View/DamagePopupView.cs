using UnityEngine;
using TMPro;

namespace Matchmancer.View
{
    /// <summary>
    /// Lightweight floating damage number. Spawned by
    /// <see cref="BattleUIOrchestrator"/> when the enemy or character takes
    /// a hit. Animates upward and fades out, then self-destructs.
    ///
    /// For mobile performance, use an object pool and call
    /// <see cref="Show(int, Vector3, Color)"/> instead of Instantiate/Destroy
    /// in a real build. This script works either way.
    /// </summary>
    public class DamagePopupView : MonoBehaviour
    {
        [Header("Animation")]
        [SerializeField] private float _riseSpeed    = 80f;
        [SerializeField] private float _lifetime     = 0.9f;
        [SerializeField] private float _fadeStart    = 0.4f;
        [SerializeField] private float _scalePopTime = 0.15f;
        [SerializeField] private float _scalePopSize = 1.3f;

        private TextMeshProUGUI _text;
        private RectTransform   _rect;
        private Color           _baseColor;
        private float           _elapsed;

        private void Awake()
        {
            _text = GetComponentInChildren<TextMeshProUGUI>();
            _rect = GetComponent<RectTransform>();
        }

        /// <summary>
        /// Configure and begin the popup animation.
        /// <paramref name="worldAnchor"/> is the world-space position the
        /// popup starts from; the Canvas's RectTransformUtility will convert
        /// it. If you're spawning in overlay space, pass the screen position
        /// directly and skip the conversion.
        /// </summary>
        public void Show(int amount, Vector3 worldAnchor, Color color)
        {
            if (_text == null) return;

            _text.text      = amount > 0 ? $"+{amount}" : amount.ToString();
            _baseColor      = color;
            _text.color     = color;
            _elapsed        = 0f;

            if (_rect != null)
                _rect.localScale = Vector3.one;

            gameObject.SetActive(true);
        }

        /// <summary>Overload for heal popups (positive).</summary>
        public void ShowHeal(int amount, Vector3 worldAnchor) =>
            Show(amount, worldAnchor, new Color(0.3f, 1f, 0.3f, 1f));

        /// <summary>Overload for shield popups.</summary>
        public void ShowShield(int amount, Vector3 worldAnchor) =>
            Show(amount, worldAnchor, new Color(0.3f, 0.6f, 1f, 1f));

        private void Update()
        {
            if (_text == null) return;

            _elapsed += Time.deltaTime;
            if (_elapsed >= _lifetime)
            {
                gameObject.SetActive(false);
                Destroy(gameObject);
                return;
            }

            // Rise
            if (_rect != null)
                _rect.anchoredPosition += Vector2.up * (_riseSpeed * Time.deltaTime);

            // Scale pop: grow then shrink
            if (_elapsed < _scalePopTime)
            {
                float t = _elapsed / _scalePopTime;
                float s = Mathf.Lerp(1f, _scalePopSize, t);
                if (_rect != null) _rect.localScale = Vector3.one * s;
            }
            else if (_rect != null && _rect.localScale.x > 1.01f)
            {
                float s = Mathf.Lerp(_rect.localScale.x, 1f, Time.deltaTime * 10f);
                _rect.localScale = Vector3.one * s;
            }

            // Fade
            if (_elapsed > _fadeStart)
            {
                float fadeProgress = (_elapsed - _fadeStart) / (_lifetime - _fadeStart);
                float alpha = Mathf.Lerp(1f, 0f, fadeProgress);
                _text.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, alpha);
            }
        }
    }
}
