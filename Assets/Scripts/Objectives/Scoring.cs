namespace Matchmancer.Objectives
{
    public class Scoring
    {
        public int Score { get; private set; }

        // Star thresholds (set per level)
        public int OneStar { get; }
        public int TwoStar { get; }
        public int ThreeStar { get; }
        public int FourStar { get; }
        public int FiveStar { get; }

        // Base points per tile matched
        private const int PointsPerTile = 10;
        private const int CascadeMultiplierStep = 5; // bonus per cascade depth
        private const int SigilActivationBonus = 50;

        private int _cascadeDepth;

        public Scoring(int oneStar, int twoStar, int threeStar, int fourStar, int fiveStar)
        {
            OneStar = oneStar;
            TwoStar = twoStar;
            ThreeStar = threeStar;
            FourStar = fourStar;
            FiveStar = fiveStar;
        }

        public void AddMatchScore(int tilesMatched)
        {
            int cascadeBonus = _cascadeDepth * CascadeMultiplierStep;
            Score += tilesMatched * (PointsPerTile + cascadeBonus);
        }

        public void AddSigilActivationScore()
        {
            Score += SigilActivationBonus;
        }

        public void IncrementCascade()
        {
            _cascadeDepth++;
        }

        public void ResetCascade()
        {
            _cascadeDepth = 0;
        }

        public int CalculateStars()
        {
            if (Score >= FiveStar) return 5;
            if (Score >= FourStar) return 4;
            if (Score >= ThreeStar) return 3;
            if (Score >= TwoStar) return 2;
            if (Score >= OneStar) return 1;
            return 0;
        }

        /// <summary>
        /// Bonus points for remaining moves (rewards efficiency for 5-star).
        /// Call at victory.
        /// </summary>
        public void AddRemainingMovesBonus(int movesRemaining)
        {
            Score += movesRemaining * 50;
        }
    }
}
