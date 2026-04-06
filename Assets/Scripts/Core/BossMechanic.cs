using System.Collections.Generic;
using System.Linq;
using Matchmancer.StoneBlocks;

namespace Matchmancer.Core
{
    /// <summary>
    /// Level 10 boss mechanic: every N turns, new 1-HP stone blocks spread
    /// adjacent to existing stones.
    /// </summary>
    public class BossMechanic
    {
        private readonly Board.Board _board;
        private readonly StoneBlockSystem _stoneSystem;
        private readonly int _spreadInterval;
        private readonly int _spreadHP;

        public BossMechanic(Board.Board board, StoneBlockSystem stoneSystem, int spreadInterval = 3, int spreadHP = 1)
        {
            _board = board;
            _stoneSystem = stoneSystem;
            _spreadInterval = spreadInterval;
            _spreadHP = spreadHP;
        }

        /// <summary>
        /// Call at end of each turn with the current turn number (1-based).
        /// Returns positions of newly spawned stones (for animation).
        /// </summary>
        public List<GridPosition> OnTurnEnd(int turnNumber)
        {
            if (turnNumber % _spreadInterval != 0)
                return new List<GridPosition>();

            return SpreadStones();
        }

        private List<GridPosition> SpreadStones()
        {
            var candidates = new HashSet<GridPosition>();
            int[] dr = { -1, 1, 0, 0 };
            int[] dc = { 0, 0, -1, 1 };

            // Collect all empty positions adjacent to existing stones
            foreach (var kvp in _stoneSystem.Stones.ToList())
            {
                var stonePos = kvp.Key;
                for (int i = 0; i < 4; i++)
                {
                    int r = stonePos.Row + dr[i];
                    int c = stonePos.Col + dc[i];
                    if (!_board.IsInBounds(r, c)) continue;
                    if (_board.IsStoneBlock(r, c)) continue;

                    candidates.Add(new GridPosition(r, c));
                }
            }

            // Spawn 1-2 new stones from candidates
            var spawned = new List<GridPosition>();
            int toSpawn = System.Math.Min(2, candidates.Count);
            var candidateList = candidates.ToList();

            for (int i = 0; i < toSpawn && candidateList.Count > 0; i++)
            {
                var pos = candidateList[i];
                _stoneSystem.PlaceStone(pos.Row, pos.Col, _spreadHP);
                spawned.Add(pos);
            }

            return spawned;
        }
    }
}
