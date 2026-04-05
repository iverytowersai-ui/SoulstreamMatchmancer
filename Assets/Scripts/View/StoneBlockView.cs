using UnityEngine;

namespace Matchmancer.View
{
    /// <summary>
    /// Visual representation of a stone block.
    /// </summary>
    public class StoneBlockView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite _fullHPSprite;
        [SerializeField] private Sprite _crackedSprite;
        [SerializeField] private Sprite _heavilyCrackedSprite;

        private int _maxHP;
        private int _currentHP;

        public void Initialize(int hp)
        {
            _maxHP = hp;
            _currentHP = hp;
            UpdateVisual();
        }

        public void TakeDamage(int newHP)
        {
            _currentHP = newHP;
            UpdateVisual();
            PlayDamageEffect();
        }

        public void PlayDestroyEffect()
        {
            StartCoroutine(DestroyAnimation());
        }

        private void UpdateVisual()
        {
            if (_spriteRenderer == null) return;

            float hpRatio = (float)_currentHP / _maxHP;
            if (hpRatio > 0.66f)
                _spriteRenderer.sprite = _fullHPSprite;
            else if (hpRatio > 0.33f)
                _spriteRenderer.sprite = _crackedSprite;
            else
                _spriteRenderer.sprite = _heavilyCrackedSprite;
        }

        private void PlayDamageEffect()
        {
            StartCoroutine(ShakeEffect());
        }

        private System.Collections.IEnumerator ShakeEffect()
        {
            Vector3 origin = transform.position;
            float duration = 0.15f;
            float magnitude = 0.05f;
            float t = 0;
            while (t < duration)
            {
                float x = origin.x + Random.Range(-magnitude, magnitude);
                float y = origin.y + Random.Range(-magnitude, magnitude);
                transform.position = new Vector3(x, y, origin.z);
                t += Time.deltaTime;
                yield return null;
            }
            transform.position = origin;
        }

        private System.Collections.IEnumerator DestroyAnimation()
        {
            float t = 0;
            Vector3 original = transform.localScale;
            while (t < 0.2f)
            {
                transform.localScale = Vector3.Lerp(original, Vector3.zero, t / 0.2f);
                t += Time.deltaTime;
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
