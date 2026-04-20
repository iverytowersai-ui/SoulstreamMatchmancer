using UnityEngine;
using TMPro;

namespace Matchmancer.UI
{
    /// <summary>
    /// Banner that appears between chapters in the level select map.
    /// Shows chapter title, level count, and lock status.
    /// </summary>
    public class ChapterDividerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _subtitleText;
        [SerializeField] private GameObject      _lockOverlay;

        [Header("Layout")]
#pragma warning disable 0414 // Reserved for future divider self-layout; currently positioned by parent builder.
        [SerializeField] private float _nodeVSpacing = 140f;
        [SerializeField] private float _dividerVSpacing = 240f;
        [SerializeField] private float _headerOffset = 100f;
#pragma warning restore 0414

        /// <summary>
        /// Configures the divider and positions it.
        /// </summary>
        public void Bind(StageViewModel data, Vector2 anchoredPosition)
        {
            if (_titleText != null) _titleText.text = data.chapterTitle;
            if (_subtitleText != null) 
                _subtitleText.text = $"{data.levels.Length} Levels";

            if (_lockOverlay != null) _lockOverlay.SetActive(!data.isUnlocked);

            RectTransform rt = GetComponent<RectTransform>();
            if (rt != null) rt.anchoredPosition = anchoredPosition;
        }
    }
}
