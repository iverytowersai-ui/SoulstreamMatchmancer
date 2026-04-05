using System.Collections.Generic;
using Matchmancer.Core;

namespace Matchmancer.StoneBlocks
{
    public class StoneBlockData
    {
        public GridPosition Position { get; }
        public int MaxHP { get; }
        public int CurrentHP { get; set; }
        public bool IsDestroyed => CurrentHP <= 0;

        public StoneBlockData(GridPosition position, int hp)
        {
            Position = position;
            MaxHP = hp;
            CurrentHP = hp;
        }
    }

    public class StoneBlockSystem
    {
        private readonly Board.Board _board;
        private readonly Dictionary<GridPosition, StoneBlockData> _stones = new();

        public int RemainingStones => _stones.Count;
        public IReadOnlyDictionary<GridPosition, StoneBlockData> Stones => _stones;

        public StoneBlockSystem(Board.Board board)
        {
            _board = board;
        }

        public void PlaceStone(int row, int col, int hp)
        {
            var pos = new GridPosition(row, col);
            _stones[pos] = new StoneBlockData(pos, hp);
            _board.SetStoneBlock(row, col, true);
        }

        /// <summary>
        /// Damage stone blocks adjacent to matched positions.
        /// Each stone takes 1 damage per adjacent match EVENT, not per tile.
        /// Call once per match, passing all matched positions.
        /// </summary>
        public List<GridPosition> DamageAdjacentStones(List<GridPosition> matchedPositions)
        {
            var destroyed = new List<GridPosition>();
            var damagedThisEvent = new HashSet<GridPosition>();

            foreach (var pos in matchedPositions)
            {
                var neighbors = GetAdjacentPositions(pos);
                foreach (var neighbor in neighbors)
                {
                    if (_stones.TryGetValue(neighbor, out var stone) && !damagedThisEvent.Contains(neighbor))
                    {
                        stone.CurrentHP -= 1;
                        damagedThisEvent.Add(neighbor);

                        if (stone.IsDestroyed)
                        {
                            _board.SetStoneBlock(neighbor.Row, neighbor.Col, false);
                            _stones.Remove(neighbor);
                            destroyed.Add(neighbor);
                        }
                    }
                }
            }

            return destroyed;
        }

        private List<GridPosition> GetAdjacentPositions(GridPosition pos)
        {
            var adjacent = new List<GridPosition>();
            int[] dr = { -1, 1, 0, 0 };
            int[] dc = { 0, 0, -1, 1 };

            for (int i = 0; i < 4; i++)
            {
                int r = pos.Row + dr[i];
                int c = pos.Col + dc[i];
                if (_board.IsInBounds(r, c))
                    adjacent.Add(new GridPosition(r, c));
            }

            return adjacent;
        }
    }
}
