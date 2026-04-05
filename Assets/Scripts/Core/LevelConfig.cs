using System.Collections.Generic;
using Matchmancer.Objectives;

namespace Matchmancer.Core
{
    [System.Serializable]
    public class StoneBlockConfig
    {
        public int Row;
        public int Col;
        public int HP;

        public StoneBlockConfig(int row, int col, int hp)
        {
            Row = row;
            Col = col;
            HP = hp;
        }
    }

    [System.Serializable]
    public class LevelConfig
    {
        public int LevelNumber;
        public int TotalMoves;
        public int MeterCapacity;
        public ObjectiveConfig Objective;
        public List<StoneBlockConfig> StoneBlocks;

        // Star thresholds
        public int OneStar;
        public int TwoStar;
        public int ThreeStar;
        public int FourStar;
        public int FiveStar;
    }
}
