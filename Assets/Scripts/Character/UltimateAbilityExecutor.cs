using System;
using System.Collections.Generic;
using System.Linq;
using Matchmancer.Board;
using Matchmancer.Core;

namespace Matchmancer.Character
{
    /// <summary>
    /// Enum defining the three starter heroes' ultimate abilities.
    /// </summary>
    public enum UltimateType
    {
        /// <summary>
        /// Soulstream Pulse (Blue): Clears a 4x4 area centered on target position.
        /// </summary>
        SoulstreamPulse,

        /// <summary>
        /// Port Cross (Crux): Clears the entire row and column intersecting at target position.
        /// </summary>
        PortCross,

        /// <summary>
        /// Glamour Shift (Kaery): Converts up to 8 tiles of the most common type to the least common type.
        /// </summary>
        GlamourShift,
    }

    /// <summary>
    /// Executes ultimate abilities for the three starter heroes on the match-3 board.
    /// Pure C# class (no MonoBehaviour) for testability and flexibility.
    /// </summary>
    public class UltimateAbilityExecutor
    {
        private readonly Board.Board _board;
        private readonly System.Random _rng;

        /// <summary>
        /// Initializes a new instance of the UltimateAbilityExecutor.
        /// </summary>
        /// <param name="board">The game board instance.</param>
        /// <param name="rng">Random number generator for stochastic effects.</param>
        public UltimateAbilityExecutor(Board.Board board, System.Random rng)
        {
            _board = board ?? throw new ArgumentNullException(nameof(board));
            _rng = rng ?? throw new ArgumentNullException(nameof(rng));
        }

        /// <summary>
        /// Main execution method for ultimate abilities.
        /// </summary>
        /// <param name="type">The type of ultimate ability to execute.</param>
        /// <param name="target">The target position (required for SoulstreamPulse and PortCross, ignored for GlamourShift).</param>
        /// <param name="board">The game board to apply the effect to.</param>
        /// <returns>List of all affected positions for VFX and animation purposes.</returns>
        /// <exception cref="ArgumentNullException">Thrown if board is null.</exception>
        /// <exception cref="ArgumentException">Thrown if target-requiring abilities don't have a valid target.</exception>
        public List<GridPosition> Execute(UltimateType type, GridPosition target, Board.Board board)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));

            if (RequiresTarget(type) && !board.IsInBounds(target))
                throw new ArgumentException($"Target position {target} is out of bounds for ability {type}.", nameof(target));

            return type switch
            {
                UltimateType.SoulstreamPulse => ExecuteSoulstreamPulse(target, board),
                UltimateType.PortCross => ExecutePortCross(target, board),
                UltimateType.GlamourShift => ExecuteGlamourShift(board),
                _ => new List<GridPosition>(),
            };
        }

        /// <summary>
        /// Executes Soulstream Pulse: Clears tiles in a 4x4 area centered on target.
        /// Area is clamped to board bounds.
        /// </summary>
        /// <param name="target">The center position of the pulse.</param>
        /// <param name="board">The game board.</param>
        /// <returns>List of positions that were cleared.</returns>
        private List<GridPosition> ExecuteSoulstreamPulse(GridPosition target, Board.Board board)
        {
            var affectedPositions = new List<GridPosition>();

            // 4x4 area means 2 tiles in each direction from center
            int minRow = Math.Max(0, target.Row - 1);
            int maxRow = Math.Min(board.Rows - 1, target.Row + 2);
            int minCol = Math.Max(0, target.Col - 1);
            int maxCol = Math.Min(board.Cols - 1, target.Col + 2);

            for (int row = minRow; row <= maxRow; row++)
            {
                for (int col = minCol; col <= maxCol; col++)
                {
                    var position = new GridPosition(row, col);

                    // Only clear non-empty tiles
                    var tile = board[row, col];
                    if (tile != null && !tile.IsEmpty)
                    {
                        board.ClearTile(position);
                        affectedPositions.Add(position);
                    }
                }
            }

            return affectedPositions;
        }

        /// <summary>
        /// Executes Port Cross: Clears the entire row and entire column at target position.
        /// </summary>
        /// <param name="target">The intersection point of the cross.</param>
        /// <param name="board">The game board.</param>
        /// <returns>List of positions that were cleared.</returns>
        private List<GridPosition> ExecutePortCross(GridPosition target, Board.Board board)
        {
            var affectedPositions = new List<GridPosition>();

            // Clear entire row
            for (int col = 0; col < board.Cols; col++)
            {
                var pos = new GridPosition(target.Row, col);
                var tile = board[target.Row, col];
                if (tile != null && !tile.IsEmpty)
                {
                    board.ClearTile(pos);
                    affectedPositions.Add(pos);
                }
            }

            // Clear entire column
            for (int row = 0; row < board.Rows; row++)
            {
                var pos = new GridPosition(row, target.Col);
                var tile = board[row, target.Col];
                if (tile != null && !tile.IsEmpty)
                {
                    board.ClearTile(pos);
                    affectedPositions.Add(pos);
                }
            }

            return affectedPositions;
        }

        /// <summary>
        /// Executes Glamour Shift: Converts up to 8 random tiles of the most common type
        /// into the least common type. Does not clear tiles—only changes their type.
        /// </summary>
        /// <param name="board">The game board.</param>
        /// <returns>List of positions where tiles were converted.</returns>
        private List<GridPosition> ExecuteGlamourShift(Board.Board board)
        {
            var affectedPositions = new List<GridPosition>();

            // Count all tile types on the board (excluding empty tiles)
            var typeCounts = new Dictionary<TileType, int>();
            var tilesByType = new Dictionary<TileType, List<GridPosition>>();

            for (int row = 0; row < board.Rows; row++)
            {
                for (int col = 0; col < board.Cols; col++)
                {
                    var tile = board[row, col];
                    if (tile != null && !tile.IsEmpty)
                    {
                        if (!typeCounts.ContainsKey(tile.Type))
                        {
                            typeCounts[tile.Type] = 0;
                            tilesByType[tile.Type] = new List<GridPosition>();
                        }

                        typeCounts[tile.Type]++;
                        tilesByType[tile.Type].Add(new GridPosition(row, col));
                    }
                }
            }

            // If there are fewer than 2 distinct tile types, glamour shift cannot occur
            if (typeCounts.Count < 2)
                return affectedPositions;

            // Find most common and least common types
            var mostCommonType = typeCounts.OrderByDescending(kvp => kvp.Value).First().Key;
            var leastCommonType = typeCounts.OrderBy(kvp => kvp.Value).First().Key;

            // Convert up to 8 random tiles of the most common type to the least common type
            var tilesToConvert = tilesByType[mostCommonType];
            int convertCount = Math.Min(8, tilesToConvert.Count);

            // Shuffle and select random tiles
            for (int i = 0; i < convertCount; i++)
            {
                int randomIndex = _rng.Next(tilesToConvert.Count);
                var position = tilesToConvert[randomIndex];

                // Convert the tile type (Tile.Type has a public setter)
                var tile = board[position.Row, position.Col];
                tile.Type = leastCommonType;
                board.SetTile(position, tile);

                affectedPositions.Add(position);

                // Remove this position from the list to avoid duplicate conversions
                tilesToConvert.RemoveAt(randomIndex);
            }

            return affectedPositions;
        }

        /// <summary>
        /// Determines whether an ultimate ability requires a target position.
        /// </summary>
        /// <param name="type">The ultimate ability type.</param>
        /// <returns>True if the ability requires a target, false if it's board-wide.</returns>
        public bool RequiresTarget(UltimateType type)
        {
            return type switch
            {
                UltimateType.SoulstreamPulse => true,
                UltimateType.PortCross => true,
                UltimateType.GlamourShift => false,
                _ => false,
            };
        }

        /// <summary>
        /// Gets a preview of what positions would be affected by an ultimate ability
        /// without actually executing it. Useful for UI targeting previews.
        /// </summary>
        /// <param name="type">The ultimate ability type.</param>
        /// <param name="target">The target position (required for SoulstreamPulse and PortCross).</param>
        /// <param name="board">The game board.</param>
        /// <returns>List of positions that would be affected.</returns>
        public List<GridPosition> GetAffectedPreview(UltimateType type, GridPosition target, Board.Board board)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));

            if (RequiresTarget(type) && !board.IsInBounds(target))
                return new List<GridPosition>();

            return type switch
            {
                UltimateType.SoulstreamPulse => GetSoulstreamPulsePreview(target, board),
                UltimateType.PortCross => GetPortCrossPreview(target, board),
                UltimateType.GlamourShift => GetGlamourShiftPreview(board),
                _ => new List<GridPosition>(),
            };
        }

        /// <summary>
        /// Gets a preview of Soulstream Pulse affected positions without executing.
        /// </summary>
        private List<GridPosition> GetSoulstreamPulsePreview(GridPosition target, Board.Board board)
        {
            var affectedPositions = new List<GridPosition>();

            int minRow = Math.Max(0, target.Row - 1);
            int maxRow = Math.Min(board.Rows - 1, target.Row + 2);
            int minCol = Math.Max(0, target.Col - 1);
            int maxCol = Math.Min(board.Cols - 1, target.Col + 2);

            for (int row = minRow; row <= maxRow; row++)
            {
                for (int col = minCol; col <= maxCol; col++)
                {
                    var tile = board[row, col];
                    if (tile != null && !tile.IsEmpty)
                    {
                        affectedPositions.Add(new GridPosition(row, col));
                    }
                }
            }

            return affectedPositions;
        }

        /// <summary>
        /// Gets a preview of Port Cross affected positions without executing.
        /// </summary>
        private List<GridPosition> GetPortCrossPreview(GridPosition target, Board.Board board)
        {
            var affectedPositions = new List<GridPosition>();

            // Preview entire row
            for (int col = 0; col < board.Cols; col++)
            {
                var tile = board[target.Row, col];
                if (tile != null && !tile.IsEmpty)
                {
                    affectedPositions.Add(new GridPosition(target.Row, col));
                }
            }

            // Preview entire column
            for (int row = 0; row < board.Rows; row++)
            {
                var tile = board[row, target.Col];
                if (tile != null && !tile.IsEmpty)
                {
                    affectedPositions.Add(new GridPosition(row, target.Col));
                }
            }

            return affectedPositions;
        }

        /// <summary>
        /// Gets a preview of Glamour Shift affected positions without executing.
        /// Note: This shows which positions COULD be converted, based on current board state.
        /// </summary>
        private List<GridPosition> GetGlamourShiftPreview(Board.Board board)
        {
            var affectedPositions = new List<GridPosition>();

            var typeCounts = new Dictionary<TileType, int>();
            var tilesByType = new Dictionary<TileType, List<GridPosition>>();

            for (int row = 0; row < board.Rows; row++)
            {
                for (int col = 0; col < board.Cols; col++)
                {
                    var tile = board[row, col];
                    if (tile != null && !tile.IsEmpty)
                    {
                        if (!typeCounts.ContainsKey(tile.Type))
                        {
                            typeCounts[tile.Type] = 0;
                            tilesByType[tile.Type] = new List<GridPosition>();
                        }

                        typeCounts[tile.Type]++;
                        tilesByType[tile.Type].Add(new GridPosition(row, col));
                    }
                }
            }

            if (typeCounts.Count < 2)
                return affectedPositions;

            var mostCommonType = typeCounts.OrderByDescending(kvp => kvp.Value).First().Key;
            var tilesToConvert = tilesByType[mostCommonType];
            int previewCount = Math.Min(8, tilesToConvert.Count);

            for (int i = 0; i < previewCount; i++)
            {
                affectedPositions.Add(tilesToConvert[i]);
            }

            return affectedPositions;
        }
    }
}
