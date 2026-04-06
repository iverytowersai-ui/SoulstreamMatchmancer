using NUnit.Framework;
using Matchmancer.Objectives;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class ScoringTests
    {
        private Scoring _scoring;

        [SetUp]
        public void SetUp()
        {
            _scoring = new Scoring(
                oneStar: 500, twoStar: 1000, threeStar: 2000,
                fourStar: 3500, fiveStar: 5000);
        }

        [Test]
        public void InitialScore_IsZero()
        {
            Assert.AreEqual(0, _scoring.Score);
        }

        [Test]
        public void AddMatchScore_3Tiles_Adds30Points()
        {
            _scoring.AddMatchScore(3); // 3 tiles * 10 pts = 30
            Assert.AreEqual(30, _scoring.Score);
        }

        [Test]
        public void CascadeBonus_IncreasesPerDepth()
        {
            _scoring.AddMatchScore(3); // depth 0: 3 * 10 = 30
            _scoring.IncrementCascade();
            _scoring.AddMatchScore(3); // depth 1: 3 * (10+5) = 45
            Assert.AreEqual(75, _scoring.Score);
        }

        [Test]
        public void ResetCascade_ResetsDepthToZero()
        {
            _scoring.IncrementCascade();
            _scoring.IncrementCascade();
            _scoring.ResetCascade();
            _scoring.AddMatchScore(3); // depth 0: 3 * 10 = 30
            Assert.AreEqual(30, _scoring.Score);
        }

        [Test]
        public void SigilActivation_Adds50Points()
        {
            _scoring.AddSigilActivationScore();
            Assert.AreEqual(50, _scoring.Score);
        }

        [Test]
        public void RemainingMovesBonus_Adds50PerMove()
        {
            _scoring.AddRemainingMovesBonus(5);
            Assert.AreEqual(250, _scoring.Score);
        }

        [Test]
        public void CalculateStars_0Stars_BelowOneStar()
        {
            Assert.AreEqual(0, _scoring.CalculateStars());
        }

        [Test]
        public void CalculateStars_3Stars_At2000()
        {
            for (int i = 0; i < 200; i++) _scoring.AddMatchScore(1);
            Assert.AreEqual(3, _scoring.CalculateStars()); // 200 * 10 = 2000
        }

        [Test]
        public void CalculateStars_5Stars_At5000()
        {
            for (int i = 0; i < 100; i++) _scoring.AddSigilActivationScore(); // 100 * 50 = 5000
            Assert.AreEqual(5, _scoring.CalculateStars());
        }
    }
}
