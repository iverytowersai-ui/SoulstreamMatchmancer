using System.Collections.Generic;
using Matchmancer.Core;
using Matchmancer.Match;

namespace Matchmancer.Sigils
{
    public class SigilSystem
    {
        private readonly Board.Board _board;

        public SigilSystem(Board.Board board)
        {
            _board = board;
        }

        /// <summary>
        /// Creates a Sigil at the designated spawn position based on match pattern.
        /// Called during match resolution (step 2b).
        /// </summary>
        public void CreateSigilFromMatch(MatchInfo match)
        {
            if (!match.SigilSpawnPosition.HasValue) return;

            var sigilType = match.Pattern switch
            {
                MatchPattern.FourInARow => SigilType.Line,
                MatchPattern.FiveInARow => SigilType.Star,
                MatchPattern.LShape => SigilType.Nova,
                MatchPattern.TShape => SigilType.Nova,
                _ => SigilType.None
            };

            if (sigilType == SigilType.None) return;

            var pos = match.SigilSpawnPosition.Value;
            var tile = _board[pos];
            tile.Type = match.TileType;
            tile.Sigil = sigilType;
        }

        /// <summary>
        /// Activates a Sigil at the given position. Returns positions of tiles cleared.
        /// The caller is responsible for actually clearing the tiles.
        /// </summary>
        public List<GridPosition> ActivateSigil(GridPosition pos)
        {
            var tile = _board[pos];
            if (!tile.IsSigil) return new List<GridPosition>();

            var cleared = new List<GridPosition>();

            switch (tile.Sigil)
            {
                case SigilType.Line:
                    cleared = ActivateLine(pos);
                    break;
                case SigilType.Star:
                    cleared = ActivateStar(pos, tile.Type);
                    break;
                case SigilType.Nova:
                    cleared = ActivateNova(pos);
                    break;
            }

            return cleared;
        }

        /// <summary>
        /// Line Sigil: Clears entire row OR column.
        /// Uses the direction of the original match (for MVP, clears row).
        /// </summary>
        private List<GridPosition> ActivateLine(GridPosition pos)
        {
            var cleared = new List<GridPosition>();

            // Clear the entire row
            for (int c = 0; c < Board.Board.Cols; c++)
            {
                var target = new GridPosition(pos.Row, c);
                if (!_board.IsStoneBlock(pos.Row, c) && target != pos)
                    cleared.Add(target);
            }

            return cleared;
        }

        /// <summary>
        /// Star Sigil: Clears ALL tiles of the Sigil's tile type from the board.
        /// </summary>
        private List<GridPosition> ActivateStar(GridPosition pos, TileType targetType)
        {
            var cleared = new List<GridPosition>();

            for (int r = 0; r < Board.Board.Rows; r++)
            {
                for (int c = 0; c < Board.Board.Cols; c++)
                {
                    if (_board.IsStoneBlock(r, c)) continue;
                    var p = new GridPosition(r, c);
                    if (p == pos) continue;
                    if (_board[r, c].Type == targetType)
                        cleared.Add(p);
                }
            }

            return cleared;
        }

        /// <summary>
        /// Nova Sigil: 3x3 explosion centered on the sigil position.
        /// </summary>
        private List<GridPosition> ActivateNova(GridPosition pos)
        {
            var cleared = new List<GridPosition>();

            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    int r = pos.Row + dr;
                    int c = pos.Col + dc;
                    if (!_board.IsInBounds(r, c)) continue;
                    if (_board.IsStoneBlock(r, c)) continue;
                    var target = new GridPosition(r, c);
                    if (target == pos) continue;
                    cleared.Add(target);
                }
            }

            return cleared;
        }
    }
}
