using System.Collections.Generic;
using NUnit.Framework;
using Matchmancer.Core;
using Matchmancer.StoneBlocks;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class BossMechanicTests
    {
        private Board.Board _board;
        private StoneBlockSystem _stoneSystem;
        private BossMechanic _boss;

        [SetUp]
        public void SetUp()
        {
            _board = new Board.Board();
            _board.Initialize(() => TileType.PortRune);
            _stoneSystem = new StoneBlockSystem(_board);
            _stoneSystem.PlaceStone(3, 3, 2);
            _stoneSystem.PlaceStone(3, 4, 2);
            _boss = new BossMechanic(_board, _stoneSystem, spreadInterval: 3, spreadHP: 1);
        }

        [Test]
        public void SpreadStones_NotTriggered_BeforeInterval()
        {
            int stonesBefore = _stoneSystem.RemainingStones;
            _boss.OnTurnEnd(1);
            Assert.AreEqual(stonesBefore, _stoneSystem.RemainingStones);
        }

        [Test]
        public void SpreadStones_Triggered_AtInterval()
        {
            int stonesBefore = _stoneSystem.RemainingStones;
            _boss.OnTurnEnd(3);
            Assert.Greater(_stoneSystem.RemainingStones, stonesBefore);
        }

        [Test]
        public void SpreadStones_AddsAdjacentToExisting()
        {
            _boss.OnTurnEnd(3);
            bool foundAdjacent = false;
            foreach (var kvp in _stoneSystem.Stones)
            {
                var pos = kvp.Key;
                if (pos != new GridPosition(3, 3) && pos != new GridPosition(3, 4))
                {
                    bool isAdj = pos.IsAdjacentTo(new GridPosition(3, 3)) ||
                                 pos.IsAdjacentTo(new GridPosition(3, 4));
                    if (isAdj) foundAdjacent = true;
                }
            }
            Assert.IsTrue(foundAdjacent);
        }

        [Test]
        public void SpreadStones_TriggeredAgain_AtDoubleInterval()
        {
            _boss.OnTurnEnd(3);
            int afterFirst = _stoneSystem.RemainingStones;
            _boss.OnTurnEnd(6);
            Assert.Greater(_stoneSystem.RemainingStones, afterFirst);
        }
    }
}
