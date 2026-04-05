using System;
using Matchmancer.Core;

namespace Matchmancer.Board
{
    public class Board
    {
        public const int Rows = 8;
        public const int Cols = 8;

        private readonly Tile[,] _grid = new Tile[Rows, Cols];
        private readonly bool[,] _stoneBlockMap = new bool[Rows, Cols];

        public Tile this[int row, int col] => _grid[row, col];
        public Tile this[GridPosition pos] => _grid[pos.Row, pos.Col];

        public void Initialize(Func<TileType> randomTileProvider)
        {
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    var pos = new GridPosition(r, c);
                    _grid[r, c] = new Tile(randomTileProvider(), pos);
                }
            }
        }

        public bool IsInBounds(int row, int col) =>
            row >= 0 && row < Rows && col >= 0 && col < Cols;

        public bool IsInBounds(GridPosition pos) => IsInBounds(pos.Row, pos.Col);

        public bool IsStoneBlock(int row, int col) =>
            IsInBounds(row, col) && _stoneBlockMap[row, col];

        public void SetStoneBlock(int row, int col, bool value)
        {
            if (IsInBounds(row, col))
                _stoneBlockMap[row, col] = value;
        }

        public bool IsPlayable(int row, int col) =>
            IsInBounds(row, col) && !IsStoneBlock(row, col);

        public void SwapTiles(GridPosition a, GridPosition b)
        {
            var tileA = _grid[a.Row, a.Col];
            var tileB = _grid[b.Row, b.Col];

            _grid[a.Row, a.Col] = tileB;
            _grid[b.Row, b.Col] = tileA;

            tileB.Position = a;
            tileA.Position = b;
        }

        public void SetTile(GridPosition pos, Tile tile)
        {
            _grid[pos.Row, pos.Col] = tile;
            tile.Position = pos;
        }

        public void ClearTile(GridPosition pos)
        {
            _grid[pos.Row, pos.Col].Clear();
        }
    }
}
