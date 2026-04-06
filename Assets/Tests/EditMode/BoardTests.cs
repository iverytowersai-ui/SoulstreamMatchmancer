using NUnit.Framework;
using Matchmancer.Core;
using Matchmancer.Board;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class GridPositionTests
    {
        [Test]
        public void IsAdjacentTo_HorizontalNeighbor_ReturnsTrue()
        {
            var a = new GridPosition(3, 3);
            var b = new GridPosition(3, 4);
            Assert.IsTrue(a.IsAdjacentTo(b));
        }

        [Test]
        public void IsAdjacentTo_VerticalNeighbor_ReturnsTrue()
        {
            var a = new GridPosition(3, 3);
            var b = new GridPosition(4, 3);
            Assert.IsTrue(a.IsAdjacentTo(b));
        }

        [Test]
        public void IsAdjacentTo_Diagonal_ReturnsFalse()
        {
            var a = new GridPosition(3, 3);
            var b = new GridPosition(4, 4);
            Assert.IsFalse(a.IsAdjacentTo(b));
        }

        [Test]
        public void IsAdjacentTo_SamePosition_ReturnsFalse()
        {
            var a = new GridPosition(3, 3);
            Assert.IsFalse(a.IsAdjacentTo(a));
        }

        [Test]
        public void IsAdjacentTo_FarApart_ReturnsFalse()
        {
            var a = new GridPosition(0, 0);
            var b = new GridPosition(7, 7);
            Assert.IsFalse(a.IsAdjacentTo(b));
        }

        [Test]
        public void Equality_SameRowCol_AreEqual()
        {
            var a = new GridPosition(2, 5);
            var b = new GridPosition(2, 5);
            Assert.AreEqual(a, b);
            Assert.IsTrue(a == b);
        }

        [Test]
        public void Equality_DifferentRowCol_AreNotEqual()
        {
            var a = new GridPosition(2, 5);
            var b = new GridPosition(5, 2);
            Assert.AreNotEqual(a, b);
            Assert.IsTrue(a != b);
        }
    }

    [TestFixture]
    public class BoardTests
    {
        private Board.Board _board;
        private int _tileIndex;
        private readonly TileType[] _types = {
            TileType.PortRune, TileType.OzoneMark, TileType.CovenSeal,
            TileType.WitchbreedThorn, TileType.SoulstreamShard, TileType.PetshaCharm
        };

        [SetUp]
        public void SetUp()
        {
            _board = new Board.Board();
            _tileIndex = 0;
            _board.Initialize(() => _types[_tileIndex++ % _types.Length]);
        }

        [Test]
        public void Initialize_AllCellsPopulated()
        {
            for (int r = 0; r < Board.Board.Rows; r++)
                for (int c = 0; c < Board.Board.Cols; c++)
                    Assert.AreNotEqual(TileType.None, _board[r, c].Type);
        }

        [Test]
        public void IsInBounds_ValidPosition_ReturnsTrue()
        {
            Assert.IsTrue(_board.IsInBounds(0, 0));
            Assert.IsTrue(_board.IsInBounds(7, 7));
        }

        [Test]
        public void IsInBounds_OutOfBounds_ReturnsFalse()
        {
            Assert.IsFalse(_board.IsInBounds(-1, 0));
            Assert.IsFalse(_board.IsInBounds(8, 0));
            Assert.IsFalse(_board.IsInBounds(0, 8));
        }

        [Test]
        public void SwapTiles_SwapsTypesAndPositions()
        {
            var posA = new GridPosition(0, 0);
            var posB = new GridPosition(0, 1);
            var typeA = _board[posA].Type;
            var typeB = _board[posB].Type;

            _board.SwapTiles(posA, posB);

            Assert.AreEqual(typeB, _board[posA].Type);
            Assert.AreEqual(typeA, _board[posB].Type);
            Assert.AreEqual(posA, _board[posA].Position);
            Assert.AreEqual(posB, _board[posB].Position);
        }

        [Test]
        public void StoneBlock_SetAndQuery()
        {
            _board.SetStoneBlock(3, 3, true);
            Assert.IsTrue(_board.IsStoneBlock(3, 3));
            Assert.IsFalse(_board.IsPlayable(3, 3));
        }

        [Test]
        public void ClearTile_MakesTileEmpty()
        {
            var pos = new GridPosition(2, 2);
            Assert.IsFalse(_board[pos].IsEmpty);
            _board.ClearTile(pos);
            Assert.IsTrue(_board[pos].IsEmpty);
        }
    }
}
