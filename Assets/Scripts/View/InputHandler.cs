using UnityEngine;
using Matchmancer.Core;

namespace Matchmancer.View
{
    /// <summary>
    /// Handles click/touch input for tile selection and swap.
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        private BoardController _controller;
        private BoardView _boardView;
        private GridPosition? _selectedTile;

        public void Initialize(BoardController controller, BoardView boardView)
        {
            _controller = controller;
            _boardView = boardView;
            _selectedTile = null;
        }

        private void Update()
        {
            if (_controller == null || _controller.IsBusy) return;

            if (Input.GetMouseButtonDown(0))
            {
                HandleClick(Input.mousePosition);
            }

            // Touch support
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    HandleClick(touch.position);
                }
            }
        }

        private void HandleClick(Vector3 screenPos)
        {
            var worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            worldPos.z = 0;
            var gridPos = _boardView.WorldToGrid(worldPos);

            if (!gridPos.HasValue) return;
            if (_controller.Board.IsStoneBlock(gridPos.Value.Row, gridPos.Value.Col)) return;

            if (!_selectedTile.HasValue)
            {
                // First selection
                _selectedTile = gridPos.Value;
                HighlightTile(gridPos.Value, true);
            }
            else if (_selectedTile.Value == gridPos.Value)
            {
                // Deselect
                HighlightTile(_selectedTile.Value, false);
                _selectedTile = null;
            }
            else if (_selectedTile.Value.IsAdjacentTo(gridPos.Value))
            {
                // Adjacent — attempt swap
                HighlightTile(_selectedTile.Value, false);
                _controller.TrySwap(_selectedTile.Value, gridPos.Value);
                _selectedTile = null;
            }
            else
            {
                // Non-adjacent — move selection
                HighlightTile(_selectedTile.Value, false);
                _selectedTile = gridPos.Value;
                HighlightTile(gridPos.Value, true);
            }
        }

        private void HighlightTile(GridPosition pos, bool highlight)
        {
            var tileViews = _boardView.GetComponentsInChildren<TileView>();
            foreach (var tv in tileViews)
            {
                if (tv.GridPosition == pos)
                {
                    tv.SetSelected(highlight);
                    break;
                }
            }
        }
    }
}
