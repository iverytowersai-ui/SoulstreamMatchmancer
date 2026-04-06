using NUnit.Framework;
using Matchmancer.Core;
using Matchmancer.Board;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class GravityHandlerTests
    {
        private Board.Board _board;
        private GravityHandler _gravity;
        private int _refillIndex;
        private readonly TileType[] _refillTypes = {
            TileType.PetshaCharm, TileType.CovenSeal, TileType.OzoneMark,
            TileType.PortRune, TileType.WitchbreedThorn, TileType.SoulstreamShard
        };

        [SetUp]
        public void SetUp()
        {
            _board = new Board.Board();
            _board.Initialize(() => TileType.PortRune);
            _refillIndex = 0;
            _gravity = new GravityHandler(_board, () => _refillTypes[_refillIndex++ % _refillTypes.Length]);
        }

        [Test]
        public void ApplyGravity_EmptyCell_TileDropsDown()
        {
            var bottomPos = new GridPosition(7, 0);
            var aboveType = _board[6, 0].Type;
            _board.ClearTile(bottomPos);

            var moves = _gravity.ApplyGravity();

            Assert.Greater(moves.Count, 0);
            Assert.AreEqual(aboveType, _board[7, 0].Type);
        }

        [Test]
        public void ApplyGravity_StoneBlock_TilesStopAboveStone()
        {
            _board.SetStoneBlock(5, 0, true);
            _board.ClearTile(new GridPosition(4, 0));

            var moves = _gravity.ApplyGravity();

            Assert.IsFalse(_board[4, 0].IsEmpty);
        }

        [Test]
        public void RefillBoard_FillsAllEmpties()
        {
            for (int r = 0; r < 3; r++)
                _board.ClearTile(new GridPosition(r, 0));

            var filled = _gravity.RefillBoard();

            Assert.AreEqual(3, filled.Count);
            for (int r = 0; r < 3; r++)
                Assert.IsFalse(_board[r, 0].IsEmpty);
        }

        [Test]
        public void RefillBoard_SkipsStoneBlocks()
        {
            _board.SetStoneBlock(2, 0, true);
            _board.ClearTile(new GridPosition(1, 0));

            var filled = _gravity.RefillBoard();

            Assert.IsTrue(_board.IsStoneBlock(2, 0));
            Assert.IsFalse(_board[1, 0].IsEmpty);
        }

        [Test]
        public void GravityThenRefill_FullCycle_NoCellsEmpty()
        {
            _board.ClearTile(new GridPosition(7, 3));
            _board.ClearTile(new GridPosition(6, 3));
            _board.ClearTile(new GridPosition(5, 3));

            _gravity.ApplyGravity();
            _gravity.RefillBoard();

            for (int r = 0; r < Board.Board.Rows; r++)
                Assert.IsFalse(_board[r, 3].IsEmpty, $"Cell ({r},3) is still empty after gravity+refill");
        }
    }
}
