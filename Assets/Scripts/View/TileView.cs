using UnityEngine;
using Matchmancer.Core;

namespace Matchmancer.View
{
    /// <summary>
    /// Visual representation of a single tile on the board.
    /// Attach to a prefab with a SpriteRenderer.
    /// </summary>
    public class TileView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private SpriteRenderer _sigilOverlay;
        [SerializeField] private GameObject _selectionHighlight;

        private GridPosition _gridPosition;
        private TileType _tileType;
        private SigilType _sigilType;

        // Color map matching the spec hex codes
        private static readonly Color PortRuneColor = new Color32(0x00, 0xD4, 0xFF, 0xFF);      // #00D4FF
        private static readonly Color OzoneMarkColor = new Color32(0xFF, 0xD1, 0x00, 0xFF);      // #FFD100
        private static readonly Color CovenSealColor = new Color32(0x7B, 0x2C, 0xBF, 0xFF);      // #7B2CBF
        private static readonly Color WitchbreedThornColor = new Color32(0xC4, 0x1E, 0x3A, 0xFF); // #C41E3A
        private static readonly Color SoulstreamShardColor = new Color32(0xC0, 0xC0, 0xC0, 0xFF); // #C0C0C0
        private static readonly Color PetshaCharmColor = new Color32(0xD4, 0xA0, 0x17, 0xFF);     // #D4A017

        // Sigil indicator colors
        private static readonly Color LineSigilColor = new Color32(0x00, 0xFF, 0x88, 0xFF);
        private static readonly Color StarSigilColor = new Color32(0xFF, 0xFF, 0x00, 0xFF);
        private static readonly Color NovaSigilColor = new Color32(0xFF, 0x44, 0x00, 0xFF);

        public GridPosition GridPosition => _gridPosition;

        public void Initialize(GridPosition pos, TileType type, SigilType sigil = SigilType.None)
        {
            _gridPosition = pos;
            UpdateVisual(type, sigil);
        }

        public void UpdateVisual(TileType type, SigilType sigil)
        {
            _tileType = type;
            _sigilType = sigil;

            if (_spriteRenderer != null)
            {
                string spritePath = GetSpritePath(type);
                if (!string.IsNullOrEmpty(spritePath))
                {
                    var loadedSprite = Resources.Load<Sprite>(spritePath);
                    if (loadedSprite != null)
                    {
                        _spriteRenderer.sprite = loadedSprite;
                        _spriteRenderer.color = Color.white; // Drop the tint since placeholders are pre-colored
                    }
                    else
                    {
                        _spriteRenderer.color = GetTileColor(type);
                    }
                }
                else
                {
                    _spriteRenderer.color = GetTileColor(type);
                }
                _spriteRenderer.enabled = type != TileType.None;
            }

            if (_sigilOverlay != null)
            {
                _sigilOverlay.enabled = sigil != SigilType.None;
                if (sigil != SigilType.None)
                    _sigilOverlay.color = GetSigilColor(sigil);
            }

            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            if (_selectionHighlight != null)
                _selectionHighlight.SetActive(selected);
        }

        public void PlayMatchAnimation()
        {
            // Placeholder — scale punch + fade
            StartCoroutine(ScalePunch());
        }

        public void PlayDropAnimation(Vector3 targetPos, float duration)
        {
            StartCoroutine(SmoothMove(targetPos, duration));
        }

        private System.Collections.IEnumerator ScalePunch()
        {
            Vector3 original = transform.localScale;
            Vector3 big = original * 1.3f;
            float t = 0;
            while (t < 0.15f)
            {
                transform.localScale = Vector3.Lerp(original, big, t / 0.15f);
                t += Time.deltaTime;
                yield return null;
            }
            t = 0;
            while (t < 0.1f)
            {
                transform.localScale = Vector3.Lerp(big, Vector3.zero, t / 0.1f);
                t += Time.deltaTime;
                yield return null;
            }
            transform.localScale = original;
        }

        private System.Collections.IEnumerator SmoothMove(Vector3 target, float duration)
        {
            Vector3 start = transform.position;
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                float ease = t / duration;
                ease = ease * ease * (3f - 2f * ease); // smoothstep
                transform.position = Vector3.Lerp(start, target, ease);
                yield return null;
            }
            transform.position = target;
        }

        private static Color GetTileColor(TileType type)
        {
            return type switch
            {
                TileType.PortRune => PortRuneColor,
                TileType.OzoneMark => OzoneMarkColor,
                TileType.CovenSeal => CovenSealColor,
                TileType.WitchbreedThorn => WitchbreedThornColor,
                TileType.SoulstreamShard => SoulstreamShardColor,
                TileType.PetshaCharm => PetshaCharmColor,
                _ => Color.gray
            };
        }

        private static Color GetSigilColor(SigilType sigil)
        {
            return sigil switch
            {
                SigilType.Line => LineSigilColor,
                SigilType.Star => StarSigilColor,
                SigilType.Nova => NovaSigilColor,
                _ => Color.clear
            };
        }

        private static string GetSpritePath(TileType type)
        {
            return type switch
            {
                TileType.PortRune => "Tiles/ph_port_rune",
                TileType.OzoneMark => "Tiles/ph_ozone_mark",
                TileType.CovenSeal => "Tiles/ph_coven_seal",
                TileType.WitchbreedThorn => "Tiles/ph_witchbreed_thorn",
                TileType.SoulstreamShard => "Tiles/ph_soulstream_shard",
                TileType.PetshaCharm => "Tiles/ph_petsha_charm",
                _ => null
            };
        }
    }
}
