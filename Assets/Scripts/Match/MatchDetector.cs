using System.Collections.Generic;
using System.Linq;
using Matchmancer.Core;

namespace Matchmancer.Match
{
    public class MatchDetector
    {
        private readonly Board.Board _board;

        public MatchDetector(Board.Board board)
        {
            _board = board;
        }

        public List<MatchInfo> FindAllMatches()
        {
            var horizontalRuns = FindRuns(isHorizontal: true);
            var verticalRuns = FindRuns(isHorizontal: false);
            var matches = new List<MatchInfo>();

            // Check for L/T shapes: intersections of horizontal and vertical runs
            var usedH = new HashSet<int>();
            var usedV = new HashSet<int>();

            for (int h = 0; h < horizontalRuns.Count; h++)
            {
                for (int v = 0; v < verticalRuns.Count; v++)
                {
                    if (usedH.Contains(h) || usedV.Contains(v)) continue;
                    if (horizontalRuns[h].type != verticalRuns[v].type) continue;

                    var intersection = FindIntersection(horizontalRuns[h].positions, verticalRuns[v].positions);
                    if (intersection.HasValue)
                    {
                        var combined = new HashSet<GridPosition>(horizontalRuns[h].positions);
                        combined.UnionWith(verticalRuns[v].positions);
                        var pattern = ClassifyLTShape(horizontalRuns[h].positions, verticalRuns[v].positions, intersection.Value);

                        var match = new MatchInfo(horizontalRuns[h].type, pattern, combined.ToList())
                        {
                            SigilSpawnPosition = intersection.Value
                        };
                        matches.Add(match);
                        usedH.Add(h);
                        usedV.Add(v);
                    }
                }
            }

            // Remaining horizontal runs (not part of L/T)
            for (int h = 0; h < horizontalRuns.Count; h++)
            {
                if (usedH.Contains(h)) continue;
                var run = horizontalRuns[h];
                var pattern = ClassifyLinePattern(run.positions.Count);
                var match = new MatchInfo(run.type, pattern, run.positions);
                if (pattern == MatchPattern.FourInARow || pattern == MatchPattern.FiveInARow)
                    match.SigilSpawnPosition = run.positions[run.positions.Count / 2];
                matches.Add(match);
            }

            // Remaining vertical runs
            for (int v = 0; v < verticalRuns.Count; v++)
            {
                if (usedV.Contains(v)) continue;
                var run = verticalRuns[v];
                var pattern = ClassifyLinePattern(run.positions.Count);
                var match = new MatchInfo(run.type, pattern, run.positions);
                if (pattern == MatchPattern.FourInARow || pattern == MatchPattern.FiveInARow)
                    match.SigilSpawnPosition = run.positions[run.positions.Count / 2];
                matches.Add(match);
            }

            return matches;
        }

        private List<(TileType type, List<GridPosition> positions)> FindRuns(bool isHorizontal)
        {
            var runs = new List<(TileType type, List<GridPosition> positions)>();
            int outerLimit = isHorizontal ? Board.Board.Rows : Board.Board.Cols;
            int innerLimit = isHorizontal ? Board.Board.Cols : Board.Board.Rows;

            for (int outer = 0; outer < outerLimit; outer++)
            {
                int runStart = 0;
                while (runStart < innerLimit)
                {
                    int r = isHorizontal ? outer : runStart;
                    int c = isHorizontal ? runStart : outer;

                    var tile = _board[r, c];
                    if (tile.IsEmpty || tile.Type == TileType.None || _board.IsStoneBlock(r, c))
                    {
                        runStart++;
                        continue;
                    }

                    var currentType = tile.Type;
                    var positions = new List<GridPosition> { new GridPosition(r, c) };
                    int next = runStart + 1;

                    while (next < innerLimit)
                    {
                        int nr = isHorizontal ? outer : next;
                        int nc = isHorizontal ? next : outer;

                        if (_board.IsStoneBlock(nr, nc)) break;
                        var nextTile = _board[nr, nc];
                        if (nextTile.Type != currentType || nextTile.IsEmpty) break;

                        positions.Add(new GridPosition(nr, nc));
                        next++;
                    }

                    if (positions.Count >= 3)
                        runs.Add((currentType, positions));

                    runStart = next;
                }
            }

            return runs;
        }

        private GridPosition? FindIntersection(List<GridPosition> horizontal, List<GridPosition> vertical)
        {
            var hSet = new HashSet<GridPosition>(horizontal);
            foreach (var pos in vertical)
            {
                if (hSet.Contains(pos))
                    return pos;
            }
            return null;
        }

        private MatchPattern ClassifyLTShape(List<GridPosition> horizontal, List<GridPosition> vertical, GridPosition intersection)
        {
            // T-shape: intersection is in the middle of one of the runs
            bool hMiddle = horizontal.Count >= 3 &&
                           horizontal.IndexOf(intersection) > 0 &&
                           horizontal.IndexOf(intersection) < horizontal.Count - 1;
            bool vMiddle = vertical.Count >= 3 &&
                           vertical.IndexOf(intersection) > 0 &&
                           vertical.IndexOf(intersection) < vertical.Count - 1;

            return (hMiddle || vMiddle) ? MatchPattern.TShape : MatchPattern.LShape;
        }

        private MatchPattern ClassifyLinePattern(int count)
        {
            return count switch
            {
                >= 5 => MatchPattern.FiveInARow,
                4 => MatchPattern.FourInARow,
                _ => MatchPattern.ThreeInARow
            };
        }
    }
}
