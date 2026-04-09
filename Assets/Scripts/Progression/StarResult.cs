using System;

namespace Matchmancer.Progression
{
    /// <summary>
    /// Output of <see cref="StarEvaluator.Evaluate"/>. Bundles the final
    /// star count, the 5-star flag used by achievements, and a per-tier
    /// breakdown so the results screen can show which thresholds were hit.
    /// </summary>
    [Serializable]
    public struct StarResult : IEquatable<StarResult>
    {
        /// <summary>0..5 stars. 0 means defeat.</summary>
        public int Stars;

        /// <summary>True when <see cref="Stars"/> == 5. Drives the 5-star achievement track.</summary>
        public bool FiveStar;

        /// <summary>True if the player won. Redundant with BattleResult but convenient.</summary>
        public bool Victory;

        // Per-tier flags so the results screen can render e.g.
        // "Score 1200 ★★★☆☆  (Four-Star 1500)"
        public bool OneStarHit;
        public bool TwoStarHit;
        public bool ThreeStarHit;
        public bool FourStarHit;
        public bool FiveStarHit;

        /// <summary>Score that was compared against the thresholds.</summary>
        public int ScoreConsidered;

        public bool Equals(StarResult other) =>
            Stars           == other.Stars
         && FiveStar        == other.FiveStar
         && Victory         == other.Victory
         && OneStarHit      == other.OneStarHit
         && TwoStarHit      == other.TwoStarHit
         && ThreeStarHit    == other.ThreeStarHit
         && FourStarHit     == other.FourStarHit
         && FiveStarHit     == other.FiveStarHit
         && ScoreConsidered == other.ScoreConsidered;

        public override bool Equals(object obj) => obj is StarResult r && Equals(r);

        public override int GetHashCode()
        {
            unchecked
            {
                int h = 17;
                h = h * 31 + Stars;
                h = h * 31 + (FiveStar        ? 1 : 0);
                h = h * 31 + (Victory         ? 1 : 0);
                h = h * 31 + (OneStarHit      ? 1 : 0);
                h = h * 31 + (TwoStarHit      ? 1 : 0);
                h = h * 31 + (ThreeStarHit    ? 1 : 0);
                h = h * 31 + (FourStarHit     ? 1 : 0);
                h = h * 31 + (FiveStarHit     ? 1 : 0);
                h = h * 31 + ScoreConsidered;
                return h;
            }
        }

        public override string ToString() =>
            $"StarResult(stars={Stars}, five={FiveStar}, win={Victory}, score={ScoreConsidered})";
    }
}
