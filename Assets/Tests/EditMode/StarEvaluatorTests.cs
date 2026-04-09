using NUnit.Framework;
using Matchmancer.Core;
using Matchmancer.Progression;

namespace Matchmancer.Tests
{
    /// <summary>
    /// Pure C# tests for <see cref="StarEvaluator"/>. No Unity, no scene.
    /// Exercises every star rule: defeat floor, victory floor, score tiers,
    /// and the 5-star achievement flag.
    /// </summary>
    [TestFixture]
    public class StarEvaluatorTests
    {
        private LevelConfig _level;

        [SetUp]
        public void SetUp()
        {
            _level = new LevelConfig
            {
                LevelNumber   = 1,
                TotalMoves    = 30,
                MeterCapacity = 100,
                OneStar       = 500,
                TwoStar       = 1000,
                ThreeStar     = 1500,
                FourStar      = 2000,
                FiveStar      = 2500,
            };
        }

        private static BattleResult Win(int score, int combo = 0, int hp = 100, int maxHp = 100) =>
            new BattleResult
            {
                GlobalLevelIndex = 1,
                Victory          = true,
                FinalScore       = score,
                HpRemaining      = hp,
                MaxHp            = maxHp,
                MaxComboAchieved = combo,
            };

        private static BattleResult Loss(int score) =>
            new BattleResult
            {
                GlobalLevelIndex = 1,
                Victory          = false,
                FinalScore       = score,
            };

        // ==================================================================
        // Rule 1: Defeat
        // ==================================================================

        [Test]
        public void Defeat_Returns_ZeroStars()
        {
            var r = StarEvaluator.Evaluate(Loss(9999), _level);
            Assert.AreEqual(0, r.Stars);
            Assert.IsFalse(r.FiveStar);
            Assert.IsFalse(r.Victory);
        }

        [Test]
        public void Defeat_IgnoresHighScore_NoTierFlags()
        {
            var r = StarEvaluator.Evaluate(Loss(9999), _level);
            Assert.IsFalse(r.OneStarHit);
            Assert.IsFalse(r.TwoStarHit);
            Assert.IsFalse(r.ThreeStarHit);
            Assert.IsFalse(r.FourStarHit);
            Assert.IsFalse(r.FiveStarHit);
        }

        // ==================================================================
        // Rule 2: Victory floor
        // ==================================================================

        [Test]
        public void Victory_BelowOneStar_FloorsToOneStar()
        {
            var r = StarEvaluator.Evaluate(Win(0), _level);
            Assert.AreEqual(1, r.Stars);
            Assert.IsTrue(r.Victory);
            Assert.IsFalse(r.OneStarHit, "Floor does not fake the OneStarHit flag — the score really didn't cross it.");
        }

        [Test]
        public void Victory_ExactlyOneStar_OneStarHit()
        {
            var r = StarEvaluator.Evaluate(Win(500), _level);
            Assert.AreEqual(1, r.Stars);
            Assert.IsTrue(r.OneStarHit);
        }

        // ==================================================================
        // Tier ladder
        // ==================================================================

        [Test]
        public void Victory_AtTwoStarThreshold_TwoStars()
        {
            var r = StarEvaluator.Evaluate(Win(1000), _level);
            Assert.AreEqual(2, r.Stars);
            Assert.IsTrue(r.OneStarHit);
            Assert.IsTrue(r.TwoStarHit);
            Assert.IsFalse(r.ThreeStarHit);
        }

        [Test]
        public void Victory_BetweenTiers_RoundsDown()
        {
            var r = StarEvaluator.Evaluate(Win(1999), _level);
            Assert.AreEqual(3, r.Stars);
            Assert.IsTrue(r.ThreeStarHit);
            Assert.IsFalse(r.FourStarHit);
        }

        [Test]
        public void Victory_AtFourStar_FourStars()
        {
            var r = StarEvaluator.Evaluate(Win(2000), _level);
            Assert.AreEqual(4, r.Stars);
            Assert.IsTrue(r.FourStarHit);
            Assert.IsFalse(r.FiveStarHit);
            Assert.IsFalse(r.FiveStar, "4-star result is not a 5-star.");
        }

        [Test]
        public void Victory_AtFiveStar_AllFlagsSet()
        {
            var r = StarEvaluator.Evaluate(Win(2500), _level);
            Assert.AreEqual(5, r.Stars);
            Assert.IsTrue(r.OneStarHit);
            Assert.IsTrue(r.TwoStarHit);
            Assert.IsTrue(r.ThreeStarHit);
            Assert.IsTrue(r.FourStarHit);
            Assert.IsTrue(r.FiveStarHit);
            Assert.IsTrue(r.FiveStar);
        }

        [Test]
        public void Victory_WayOverFiveStar_CapsAtFive()
        {
            var r = StarEvaluator.Evaluate(Win(999999), _level);
            Assert.AreEqual(5, r.Stars);
            Assert.IsTrue(r.FiveStar);
        }

        // ==================================================================
        // ScoreConsidered + Victory flag
        // ==================================================================

        [Test]
        public void Result_CarriesScoreAndVictoryFlag()
        {
            var r = StarEvaluator.Evaluate(Win(1750), _level);
            Assert.AreEqual(1750, r.ScoreConsidered);
            Assert.IsTrue(r.Victory);
        }

        // ==================================================================
        // Validation helper
        // ==================================================================

        [Test]
        public void ValidateThresholds_WellOrdered_ReturnsTrue()
        {
            Assert.IsTrue(StarEvaluator.ValidateThresholds(_level));
        }

        [Test]
        public void ValidateThresholds_OutOfOrder_ReturnsFalse()
        {
            _level.ThreeStar = 100; // below TwoStar = 1000
            Assert.IsFalse(StarEvaluator.ValidateThresholds(_level));
        }

        [Test]
        public void ValidateThresholds_NullLevel_ReturnsFalse()
        {
            Assert.IsFalse(StarEvaluator.ValidateThresholds(null));
        }

        // ==================================================================
        // Guard rails
        // ==================================================================

        [Test]
        public void Evaluate_NullLevel_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(() =>
                StarEvaluator.Evaluate(Win(100), null));
        }
    }
}
