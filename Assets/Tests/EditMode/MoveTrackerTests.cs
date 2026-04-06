using NUnit.Framework;
using Matchmancer.Objectives;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class MoveTrackerTests
    {
        [Test]
        public void InitialState_FullMoves()
        {
            var tracker = new MoveTracker(20);
            Assert.AreEqual(20, tracker.TotalMoves);
            Assert.AreEqual(20, tracker.MovesRemaining);
            Assert.AreEqual(0, tracker.MovesUsed);
            Assert.IsFalse(tracker.IsExhausted);
        }

        [Test]
        public void DeductMove_DecrementsBy1()
        {
            var tracker = new MoveTracker(20);
            tracker.DeductMove();
            Assert.AreEqual(19, tracker.MovesRemaining);
            Assert.AreEqual(1, tracker.MovesUsed);
        }

        [Test]
        public void DeductMove_AtZero_StaysAtZero()
        {
            var tracker = new MoveTracker(1);
            tracker.DeductMove();
            tracker.DeductMove(); // should not go negative
            Assert.AreEqual(0, tracker.MovesRemaining);
            Assert.IsTrue(tracker.IsExhausted);
        }

        [Test]
        public void IsExhausted_WhenAllMovesUsed()
        {
            var tracker = new MoveTracker(3);
            tracker.DeductMove();
            tracker.DeductMove();
            Assert.IsFalse(tracker.IsExhausted);
            tracker.DeductMove();
            Assert.IsTrue(tracker.IsExhausted);
        }
    }
}
