using NUnit.Framework;
using Matchmancer.Core;
using Matchmancer.Dice;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class DiceSystemTests
    {
        private Board.Board _board;
        private DiceSystem _dice;

        [SetUp]
        public void SetUp()
        {
            _board = new Board.Board();
            _board.Initialize(() => TileType.PortRune);
            _dice = new DiceSystem(_board, new System.Random(42));
        }

        [Test]
        public void Roll_ReturnsValidSchool()
        {
            var result = _dice.Roll();
            Assert.IsTrue(System.Enum.IsDefined(typeof(MagickSchool), result.School));
        }

        [Test]
        public void Roll_ReturnsNonEmptyAffectedPositions()
        {
            var result = _dice.Roll();
            Assert.Greater(result.AffectedPositions.Count, 0);
        }

        [Test]
        public void Roll_AllPositionsAreInBounds()
        {
            for (int i = 0; i < 20; i++)
            {
                var result = _dice.Roll();
                foreach (var pos in result.AffectedPositions)
                {
                    Assert.IsTrue(_board.IsInBounds(pos),
                        $"Position {pos} is out of bounds on roll {i}");
                }
            }
        }

        [Test]
        public void Roll_HasEffectDescription()
        {
            var result = _dice.Roll();
            Assert.IsFalse(string.IsNullOrEmpty(result.EffectDescription));
        }

        [Test]
        public void Roll_AvoidsStoneBlocks()
        {
            _board.SetStoneBlock(3, 3, true);
            _board.SetStoneBlock(3, 4, true);
            _board.SetStoneBlock(4, 3, true);
            _board.SetStoneBlock(4, 4, true);

            for (int i = 0; i < 50; i++)
            {
                var result = _dice.Roll();
                foreach (var pos in result.AffectedPositions)
                {
                    Assert.IsTrue(_board.IsPlayable(pos.Row, pos.Col),
                        $"Dice affected stone block at {pos}");
                }
            }
        }
    }
}
