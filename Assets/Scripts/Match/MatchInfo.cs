using System.Collections.Generic;
using Matchmancer.Core;

namespace Matchmancer.Match
{
    public enum MatchPattern
    {
        ThreeInARow,    // Standard match-3
        FourInARow,     // Creates Line Sigil
        FiveInARow,     // Creates Star Sigil
        LShape,         // Creates Nova Sigil
        TShape          // Creates Nova Sigil
    }

    public class MatchInfo
    {
        public TileType TileType { get; }
        public MatchPattern Pattern { get; }
        public List<GridPosition> Positions { get; }

        /// <summary>
        /// The position where a Sigil should be created (if applicable).
        /// Typically the intersection point for L/T or the center for lines.
        /// </summary>
        public GridPosition? SigilSpawnPosition { get; set; }

        public int TileCount => Positions.Count;

        public int MeterCharge
        {
            get
            {
                return TileCount switch
                {
                    3 => 1,
                    4 => 2,
                    _ => 3 // 5+ = 3 charge
                };
            }
        }

        public MatchInfo(TileType tileType, MatchPattern pattern, List<GridPosition> positions)
        {
            TileType = tileType;
            Pattern = pattern;
            Positions = positions;
        }
    }
}
