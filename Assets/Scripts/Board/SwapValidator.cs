using Matchmancer.Core;
using Matchmancer.Match;

namespace Matchmancer.Board
{
    public class SwapValidator
    {
        private readonly Board _board;
        private readonly MatchDetector _matchDetector;

        public SwapValidator(Board board, MatchDetector matchDetector)
        {
            _board = board;
            _matchDetector = matchDetector;
        }

        public bool IsValidSwap(GridPosition a, GridPosition b)
        {
            if (!a.IsAdjacentTo(b)) return false;
            if (!_board.IsPlayable(a.Row, a.Col)) return false;
            if (!_board.IsPlayable(b.Row, b.Col)) return false;

            // Swapping a Sigil with any adjacent tile is always valid (activates it)
            var tileA = _board[a];
            var tileB = _board[b];
            if (tileA.IsSigil || tileB.IsSigil) return true;

            // Try the swap, check for matches, then swap back
            _board.SwapTiles(a, b);
            var matches = _matchDetector.FindAllMatches();
            _board.SwapTiles(a, b);

            return matches.Count > 0;
        }
    }
}
