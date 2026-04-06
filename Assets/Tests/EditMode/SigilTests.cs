using System.Collections.Generic;
using NUnit.Framework;
using Matchmancer.Core;
using Matchmancer.Match;
using Matchmancer.Sigils;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class SigilSystemTests
    {
        private Board.Board _board;
        private SigilSystem _sigilSystem;

        [SetUp]
        public void SetUp()
        {
            _board = new Board.Board();
            _board.Initialize(() => TileType.PortRune);
            _sigilSystem = new SigilSystem(_board);
        }

        [Test]
        public void CreateSigilFromMatch_FourInARow_CreatesLineSigil()
        {
            var positions = new List<GridPosition>
            {
                new(0, 0), new(0, 1), new(0, 2), new(0, 3)
            };
            var match = new MatchInfo(TileType.PortRune, MatchPattern.FourInARow, positions)
            {
                SigilSpawnPosition = new GridPosition(0, 2)
            };

            _sigilSystem.CreateSigilFromMatch(match);

            Assert.AreEqual(SigilType.Line, _board[0, 2].Sigil);
            Assert.AreEqual(TileType.PortRune, _board[0, 2].Type);
        }

        [Test]
        public void CreateSigilFromMatch_FiveInARow_CreatesStarSigil()
        {
            var positions = new List<GridPosition>
            {
                new(0, 0), new(0, 1), new(0, 2), new(0, 3), new(0, 4)
            };
            var match = new MatchInfo(TileType.CovenSeal, MatchPattern.FiveInARow, positions)
            {
                SigilSpawnPosition = new GridPosition(0, 2)
            };

            _sigilSystem.CreateSigilFromMatch(match);

            Assert.AreEqual(SigilType.Star, _board[0, 2].Sigil);
        }

        [Test]
        public void CreateSigilFromMatch_LShape_CreatesNovaSigil()
        {
            var positions = new List<GridPosition>
            {
                new(0, 0), new(0, 1), new(0, 2), new(1, 0), new(2, 0)
            };
            var match = new MatchInfo(TileType.OzoneMark, MatchPattern.LShape, positions)
            {
                SigilSpawnPosition = new GridPosition(0, 0)
            };

            _sigilSystem.CreateSigilFromMatch(match);

            Assert.AreEqual(SigilType.Nova, _board[0, 0].Sigil);
        }

        [Test]
        public void CreateSigilFromMatch_ThreeInARow_NoSigil()
        {
            var positions = new List<GridPosition> { new(0, 0), new(0, 1), new(0, 2) };
            var match = new MatchInfo(TileType.PortRune, MatchPattern.ThreeInARow, positions);

            _sigilSystem.CreateSigilFromMatch(match);

            Assert.AreEqual(SigilType.None, _board[0, 0].Sigil);
            Assert.AreEqual(SigilType.None, _board[0, 1].Sigil);
            Assert.AreEqual(SigilType.None, _board[0, 2].Sigil);
        }

        [Test]
        public void ActivateSigil_Line_ClearsEntireRow()
        {
            _board[3, 3].Sigil = SigilType.Line;
            _board[3, 3].Type = TileType.PortRune;

            var cleared = _sigilSystem.ActivateSigil(new GridPosition(3, 3));

            Assert.AreEqual(7, cleared.Count);
            foreach (var pos in cleared)
                Assert.AreEqual(3, pos.Row);
        }

        [Test]
        public void ActivateSigil_Star_ClearsAllOfType()
        {
            for (int r = 0; r < Board.Board.Rows; r++)
                for (int c = 0; c < Board.Board.Cols; c++)
                {
                    var pos = new GridPosition(r, c);
                    var type = (r + c) % 2 == 0 ? TileType.PortRune : TileType.OzoneMark;
                    _board.SetTile(pos, new Tile(type, pos));
                }

            _board[0, 0].Sigil = SigilType.Star;
            _board[0, 0].Type = TileType.PortRune;

            var cleared = _sigilSystem.ActivateSigil(new GridPosition(0, 0));

            foreach (var pos in cleared)
                Assert.AreEqual(TileType.PortRune, _board[pos].Type);
        }

        [Test]
        public void ActivateSigil_Nova_Clears3x3()
        {
            _board[3, 3].Sigil = SigilType.Nova;

            var cleared = _sigilSystem.ActivateSigil(new GridPosition(3, 3));

            Assert.AreEqual(8, cleared.Count);
            foreach (var pos in cleared)
            {
                Assert.GreaterOrEqual(pos.Row, 2);
                Assert.LessOrEqual(pos.Row, 4);
                Assert.GreaterOrEqual(pos.Col, 2);
                Assert.LessOrEqual(pos.Col, 4);
            }
        }

        [Test]
        public void ActivateSigil_Nova_AtCorner_ClearsPartial()
        {
            _board[0, 0].Sigil = SigilType.Nova;

            var cleared = _sigilSystem.ActivateSigil(new GridPosition(0, 0));

            Assert.AreEqual(3, cleared.Count);
        }

        [Test]
        public void ActivateSigil_Line_SkipsStoneBlocks()
        {
            _board[3, 3].Sigil = SigilType.Line;
            _board.SetStoneBlock(3, 5, true);

            var cleared = _sigilSystem.ActivateSigil(new GridPosition(3, 3));

            Assert.AreEqual(6, cleared.Count);
            Assert.IsFalse(cleared.Exists(p => p.Row == 3 && p.Col == 5));
        }
    }
}
