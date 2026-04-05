using System.Collections.Generic;
using UnityEngine;
using Matchmancer.Core;
using Matchmancer.Match;
using Matchmancer.Dice;
using Matchmancer.Objectives;

namespace Matchmancer.View
{
    /// <summary>
    /// Renders the 8x8 board and handles visual updates from BoardController events.
    /// </summary>
    public class BoardView : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _tilePrefab;
        [SerializeField] private GameObject _stoneBlockPrefab;

        [Header("Layout")]
        [SerializeField] private float _cellSize = 0.7f;
        [SerializeField] private Vector2 _boardOffset = new Vector2(-2.45f, -2.45f);

        private TileView[,] _tileViews;
        private Dictionary<GridPosition, StoneBlockView> _stoneViews = new();
        private BoardController _controller;

        public void Initialize(BoardController controller)
        {
            _controller = controller;
            _tileViews = new TileView[Board.Board.Rows, Board.Board.Cols];

            CreateBoardVisuals();
            SubscribeToEvents();
        }

        private void CreateBoardVisuals()
        {
            var board = _controller.Board;

            for (int r = 0; r < Board.Board.Rows; r++)
            {
                for (int c = 0; c < Board.Board.Cols; c++)
                {
                    var pos = new GridPosition(r, c);
                    var worldPos = GridToWorld(pos);

                    if (board.IsStoneBlock(r, c))
                    {
                        var stoneGO = Instantiate(_stoneBlockPrefab, worldPos, Quaternion.identity, transform);
                        var stoneView = stoneGO.GetComponent<StoneBlockView>();
                        _stoneViews[pos] = stoneView;
                        // HP is set from StoneBlockSystem data via controller
                    }
                    else
                    {
                        var tileGO = Instantiate(_tilePrefab, worldPos, Quaternion.identity, transform);
                        var tileView = tileGO.GetComponent<TileView>();
                        var tile = board[r, c];
                        tileView.Initialize(pos, tile.Type, tile.Sigil);
                        _tileViews[r, c] = tileView;
                    }
                }
            }
        }

        private void SubscribeToEvents()
        {
            _controller.OnSwapPerformed += HandleSwap;
            _controller.OnMatchesFound += HandleMatches;
            _controller.OnSigilCreated += HandleSigilCreated;
            _controller.OnSigilActivated += HandleSigilActivated;
            _controller.OnTilesDropped += HandleTilesDropped;
            _controller.OnTilesRefilled += HandleTilesRefilled;
            _controller.OnStonesDestroyed += HandleStonesDestroyed;
            _controller.OnDiceRolled += HandleDiceRolled;
            _controller.OnDiceEffectApplied += HandleDiceEffect;
            _controller.OnLevelComplete += HandleLevelComplete;
        }

        public Vector3 GridToWorld(GridPosition pos)
        {
            // Row 0 = top of board, Row 7 = bottom
            float x = _boardOffset.x + pos.Col * _cellSize;
            float y = _boardOffset.y + (Board.Board.Rows - 1 - pos.Row) * _cellSize;
            return new Vector3(x, y, 0);
        }

        public GridPosition? WorldToGrid(Vector3 worldPos)
        {
            int col = Mathf.RoundToInt((worldPos.x - _boardOffset.x) / _cellSize);
            int row = Board.Board.Rows - 1 - Mathf.RoundToInt((worldPos.y - _boardOffset.y) / _cellSize);

            if (row >= 0 && row < Board.Board.Rows && col >= 0 && col < Board.Board.Cols)
                return new GridPosition(row, col);
            return null;
        }

        /// <summary>
        /// Refresh all tile visuals from the current board state.
        /// </summary>
        public void RefreshAll()
        {
            var board = _controller.Board;
            for (int r = 0; r < Board.Board.Rows; r++)
            {
                for (int c = 0; c < Board.Board.Cols; c++)
                {
                    var view = _tileViews[r, c];
                    if (view == null) continue;
                    var tile = board[r, c];
                    view.UpdateVisual(tile.Type, tile.Sigil);
                    view.transform.position = GridToWorld(new GridPosition(r, c));
                }
            }
        }

        // === Event Handlers ===

        private void HandleSwap(GridPosition a, GridPosition b)
        {
            var viewA = _tileViews[a.Row, a.Col];
            var viewB = _tileViews[b.Row, b.Col];

            if (viewA != null && viewB != null)
            {
                // Swap view references
                _tileViews[a.Row, a.Col] = viewB;
                _tileViews[b.Row, b.Col] = viewA;

                viewA.PlayDropAnimation(GridToWorld(b), 0.15f);
                viewB.PlayDropAnimation(GridToWorld(a), 0.15f);
            }
        }

        private void HandleMatches(List<MatchInfo> matches)
        {
            foreach (var match in matches)
            {
                foreach (var pos in match.Positions)
                {
                    var view = _tileViews[pos.Row, pos.Col];
                    if (view != null) view.PlayMatchAnimation();
                }
            }
        }

        private void HandleSigilCreated(GridPosition pos, SigilType sigil)
        {
            var view = _tileViews[pos.Row, pos.Col];
            if (view != null)
            {
                var tile = _controller.Board[pos];
                view.UpdateVisual(tile.Type, sigil);
            }
        }

        private void HandleSigilActivated(GridPosition pos, SigilType sigil, List<GridPosition> cleared)
        {
            // Placeholder: flash effect on cleared tiles
            foreach (var p in cleared)
            {
                var view = _tileViews[p.Row, p.Col];
                if (view != null) view.PlayMatchAnimation();
            }
        }

        private void HandleTilesDropped(List<(GridPosition from, GridPosition to)> drops)
        {
            foreach (var (from, to) in drops)
            {
                var view = _tileViews[from.Row, from.Col];
                if (view != null)
                {
                    _tileViews[to.Row, to.Col] = view;
                    _tileViews[from.Row, from.Col] = null;
                    view.PlayDropAnimation(GridToWorld(to), 0.2f);
                }
            }
        }

        private void HandleTilesRefilled(List<GridPosition> positions)
        {
            var board = _controller.Board;
            foreach (var pos in positions)
            {
                var existing = _tileViews[pos.Row, pos.Col];
                if (existing != null)
                {
                    existing.UpdateVisual(board[pos].Type, board[pos].Sigil);
                }
                else
                {
                    // Spawn new tile above board and drop in
                    var spawnPos = GridToWorld(pos) + Vector3.up * 2f;
                    var tileGO = Instantiate(_tilePrefab, spawnPos, Quaternion.identity, transform);
                    var tileView = tileGO.GetComponent<TileView>();
                    tileView.Initialize(pos, board[pos].Type, board[pos].Sigil);
                    tileView.PlayDropAnimation(GridToWorld(pos), 0.25f);
                    _tileViews[pos.Row, pos.Col] = tileView;
                }
            }
        }

        private void HandleStonesDestroyed(List<GridPosition> positions)
        {
            foreach (var pos in positions)
            {
                if (_stoneViews.TryGetValue(pos, out var stoneView))
                {
                    stoneView.PlayDestroyEffect();
                    _stoneViews.Remove(pos);
                }
            }
        }

        private void HandleDiceRolled(DiceRollResult result)
        {
            Debug.Log($"[Dice] {result.EffectDescription}");
            // Placeholder — dice animation will go here
        }

        private void HandleDiceEffect(List<GridPosition> positions)
        {
            foreach (var pos in positions)
            {
                var view = _tileViews[pos.Row, pos.Col];
                if (view != null) view.PlayMatchAnimation();
            }
        }

        private void HandleLevelComplete(LevelResult result, int stars)
        {
            Debug.Log($"[Level] {result} — {stars} stars!");
        }

        private void OnDestroy()
        {
            if (_controller == null) return;
            _controller.OnSwapPerformed -= HandleSwap;
            _controller.OnMatchesFound -= HandleMatches;
            _controller.OnSigilCreated -= HandleSigilCreated;
            _controller.OnSigilActivated -= HandleSigilActivated;
            _controller.OnTilesDropped -= HandleTilesDropped;
            _controller.OnTilesRefilled -= HandleTilesRefilled;
            _controller.OnStonesDestroyed -= HandleStonesDestroyed;
            _controller.OnDiceRolled -= HandleDiceRolled;
            _controller.OnDiceEffectApplied -= HandleDiceEffect;
            _controller.OnLevelComplete -= HandleLevelComplete;
        }
    }
}
