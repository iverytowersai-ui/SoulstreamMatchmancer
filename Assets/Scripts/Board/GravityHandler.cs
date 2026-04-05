using System;
using System.Collections.Generic;
using Matchmancer.Core;

namespace Matchmancer.Board
{
    public class GravityHandler
    {
        private readonly Board _board;
        private readonly Func<TileType> _randomTileProvider;

        public GravityHandler(Board board, Func<TileType> randomTileProvider)
        {
            _board = board;
            _randomTileProvider = randomTileProvider;
        }

        /// <summary>
        /// Drops tiles into empty spaces (gravity falls downward: higher row = lower on screen).
        /// Returns positions that changed for animation purposes.
        /// </summary>
        public List<(GridPosition from, GridPosition to)> ApplyGravity()
        {
            var moves = new List<(GridPosition from, GridPosition to)>();

            for (int col = 0; col < Board.Cols; col++)
            {
                int writeRow = Board.Rows - 1;

                // Scan from bottom to top
                for (int readRow = Board.Rows - 1; readRow >= 0; readRow--)
                {
                    if (_board.IsStoneBlock(readRow, col))
                    {
                        // Stone blocks are immovable — reset write cursor above the stone
                        writeRow = readRow - 1;
                        continue;
                    }

                    var tile = _board[readRow, col];
                    if (!tile.IsEmpty)
                    {
                        if (readRow != writeRow)
                        {
                            var from = new GridPosition(readRow, col);
                            var to = new GridPosition(writeRow, col);
                            _board.SetTile(to, tile);
                            _board.SetTile(from, new Tile(TileType.None, from));
                            moves.Add((from, to));
                        }
                        writeRow--;
                    }
                }
            }

            return moves;
        }

        /// <summary>
        /// Fills all remaining empty (non-stone) cells with new random tiles.
        /// Returns positions of newly created tiles for animation.
        /// </summary>
        public List<GridPosition> RefillBoard()
        {
            var filled = new List<GridPosition>();

            for (int col = 0; col < Board.Cols; col++)
            {
                for (int row = 0; row < Board.Rows; row++)
                {
                    if (_board.IsStoneBlock(row, col)) continue;

                    var tile = _board[row, col];
                    if (tile.IsEmpty)
                    {
                        var pos = new GridPosition(row, col);
                        _board.SetTile(pos, new Tile(_randomTileProvider(), pos));
                        filled.Add(pos);
                    }
                }
            }

            return filled;
        }
    }
}
