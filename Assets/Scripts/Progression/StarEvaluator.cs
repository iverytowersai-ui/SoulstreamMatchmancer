using System;
using Matchmancer.Core;

namespace Matchmancer.Progression
{
    /// <summary>
    /// Pure C# star rating logic. Converts a <see cref="BattleResult"/> plus
    /// a <see cref="LevelConfig"/> into a <see cref="StarResult"/>.
    ///
    /// Rules:
    ///   1. Defeat → 0 stars no matter the score.
    ///   2. Victory guarantees a minimum of 1 star — you won, you get the
    ///      level completion credit even if you scraped in under the
    ///      OneStar threshold.
    ///   3. Higher tiers are additive score gates. Crossing a tier means
    ///      every lower tier is also "hit" for display purposes.
    ///   4. <see cref="StarResult.FiveStar"/> mirrors Stars == 5 and is the
    ///      single flag the achievement / title systems read.
    ///   5. Thresholds must be non-decreasing (one ≤ two ≤ three ≤ four ≤ five).
    ///      If a config violates that, the evaluator still works but logs
    ///      an <see cref="ArgumentException"/> via <see cref="ValidateThresholds"/>.
    /// </summary>
    public static class StarEvaluator
    {
        public static StarResult Evaluate(BattleResult result, LevelConfig level)
        {
            if (level == null) throw new ArgumentNullException(nameof(level));

            var output = new StarResult
            {
                Victory         = result.Victory,
                ScoreConsidered = result.FinalScore,
            };

            // Rule 1: defeat is zero, no matter what.
            if (!result.Victory)
            {
                output.Stars    = 0;
                output.FiveStar = false;
                return output;
            }

            // Work out how many thresholds were actually crossed.
            int tier = 0;
            if (result.FinalScore >= level.OneStar)   { tier = 1; output.OneStarHit   = true; }
            if (result.FinalScore >= level.TwoStar)   { tier = 2; output.TwoStarHit   = true; }
            if (result.FinalScore >= level.ThreeStar) { tier = 3; output.ThreeStarHit = true; }
            if (result.FinalScore >= level.FourStar)  { tier = 4; output.FourStarHit  = true; }
            if (result.FinalScore >= level.FiveStar)  { tier = 5; output.FiveStarHit  = true; }

            // Rule 2: victory floor.
            if (tier < 1) tier = 1;

            output.Stars    = tier;
            output.FiveStar = tier == 5;
            return output;
        }

        /// <summary>
        /// Optional sanity check for level authors. Returns true if the
        /// five thresholds are non-decreasing. Does not mutate anything.
        /// </summary>
        public static bool ValidateThresholds(LevelConfig level)
        {
            if (level == null) return false;
            return level.OneStar   <= level.TwoStar
                && level.TwoStar   <= level.ThreeStar
                && level.ThreeStar <= level.FourStar
                && level.FourStar  <= level.FiveStar;
        }
    }
}
