using System.Collections.Generic;
using NUnit.Framework;
using Matchmancer.Core;
using Matchmancer.StoneBlocks;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class StoneBlockTests
    {
        private Board.Board _board;
        private StoneBlockSystem _system;

        [SetUp]
        public void SetUp()
        {
            _board = new Board.Board();
            _board.Initialize(() => TileType.PortRune);
            _system = new StoneBlockSystem(_board);
        }

        [Test]
        public void PlaceStone_MarksAsStoneBlock()
        {
            _system.PlaceStone(3, 3, 2);
            Assert.IsTrue(_board.IsStoneBlock(3, 3));
            Assert.AreEqual(1, _system.RemainingStones);
        }

        [Test]
        public void DamageAdjacentStones_1HP_DestroysOnFirstHit()
        {
            _system.PlaceStone(3, 3, 1);
            var matchPositions = new List<GridPosition> { new GridPosition(3, 2) };
            var destroyed = _system.DamageAdjacentStones(matchPositions);
            Assert.AreEqual(1, destroyed.Count);
            Assert.AreEqual(0, _system.RemainingStones);
            Assert.IsFalse(_board.IsStoneBlock(3, 3));
        }

        [Test]
        public void DamageAdjacentStones_2HP_SurvivesFirstHit()
        {
            _system.PlaceStone(3, 3, 2);
            var matchPositions = new List<GridPosition> { new GridPosition(3, 2) };
            var destroyed = _system.DamageAdjacentStones(matchPositions);
            Assert.AreEqual(0, destroyed.Count);
            Assert.AreEqual(1, _system.RemainingStones);
            Assert.AreEqual(1, _system.Stones[new GridPosition(3, 3)].CurrentHP);
        }

        [Test]
        public void DamageAdjacentStones_OneDamagePerMatchEvent_NotPerTile()
        {
            _system.PlaceStone(3, 3, 3);
            var matchPositions = new List<GridPosition>
            {
                new GridPosition(3, 2),
                new GridPosition(2, 3),
                new GridPosition(4, 3)
            };
            var destroyed = _system.DamageAdjacentStones(matchPositions);
            Assert.AreEqual(2, _system.Stones[new GridPosition(3, 3)].CurrentHP);
        }

        [Test]
        public void DamageAdjacentStones_NonAdjacentMatch_NoDamage()
        {
            _system.PlaceStone(3, 3, 1);
            var matchPositions = new List<GridPosition> { new GridPosition(0, 0) };
            var destroyed = _system.DamageAdjacentStones(matchPositions);
            Assert.AreEqual(0, destroyed.Count);
            Assert.AreEqual(1, _system.RemainingStones);
        }
    }
}
