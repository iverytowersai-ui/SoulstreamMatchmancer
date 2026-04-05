namespace Matchmancer.Objectives
{
    public class MoveTracker
    {
        public int TotalMoves { get; private set; }
        public int MovesRemaining { get; private set; }
        public int MovesUsed => TotalMoves - MovesRemaining;
        public bool IsExhausted => MovesRemaining <= 0;

        public MoveTracker(int totalMoves)
        {
            TotalMoves = totalMoves;
            MovesRemaining = totalMoves;
        }

        /// <summary>
        /// Deduct exactly 1 move. Called once per successful swap (never for cascades).
        /// </summary>
        public void DeductMove()
        {
            if (MovesRemaining > 0)
                MovesRemaining--;
        }
    }
}
