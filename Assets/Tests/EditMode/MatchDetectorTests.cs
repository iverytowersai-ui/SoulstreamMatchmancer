using System.Linq;
using NUnit.Framework;
using Matchmancer.Core;
using Matchmancer.Board;
using Matchmancer.Match;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class MatchDetectorTests
    {
        private Board.Board _board;
        private MatchDetector _detector;

        [SetUp]
        public void SetUp()
        {
            _board = new Board.Board();
            int idx = 0;
            TileType[] types = {
                TileType.PortRune, TileType.OzoneMark, TileType.CovenSeal,
                TileType.WitchbreedThorn, TileType.SoulstreamShard, TileType.PetshaCharm
            };
            _board.Initialize(() => types[idx++ % types.Length]);
            _detector = new MatchDetector(_board);
        }

        private void SetRow(int row, int startCol, int count, TileType type)
        {
            for (int c = startCol; c < startCol + count; c++)
            {
                var pos = new GridPosition(row, c);
                _board.SetTile(pos, new Tile(type, pos));
            }
        }

        private void SetCol(int col, int startRow, int count, TileType type)
        {
            for (int r = startRow; r < startRow + count; r++)
            {
                var pos = new GridPosition(r, col);
                _board.SetTile(pos, new Tile(type, pos));
            }
        }

        [Test]
        public void FindAllMatches_NoMatches_ReturnsEmpty()
        {
            var matches = _detector.FindAllMatches();
            Assert.AreEqual(0, matches.Count);
        }

        [Test]
        public void FindAllMatches_HorizontalThree_FindsMatch()
        {
            SetRow(0, 0, 3, TileType.PortRune);
            var matches = _detector.FindAllMatches();
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(MatchPattern.ThreeInARow, matches[0].Pattern);
            Assert.AreEqual(3, matches[0].TileCount);
            Assert.AreEqual(TileType.PortRune, matches[0].TileType);
        }

        [Test]
        public void FindAllMatches_HorizontalFour_ClassifiesAsFour()
        {
            SetRow(0, 0, 4, TileType.OzoneMark);
            var matches = _detector.FindAllMatches();
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(MatchPattern.FourInARow, matches[0].Pattern);
            Assert.AreEqual(4, matches[0].TileCount);
            Assert.IsNotNull(matches[0].SigilSpawnPosition);
        }

        [Test]
        public void FindAllMatches_HorizontalFive_ClassifiesAsFive()
        {
            SetRow(0, 0, 5, TileType.CovenSeal);
            var matches = _detector.FindAllMatches();
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(MatchPattern.FiveInARow, matches[0].Pattern);
            Assert.AreEqual(5, matches[0].TileCount);
        }

        [Test]
        public void FindAllMatches_VerticalThree_FindsMatch()
        {
            SetCol(0, 0, 3, TileType.WitchbreedThorn);
            var matches = _detector.FindAllMatches();
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(MatchPattern.ThreeInARow, matches[0].Pattern);
        }

        [Test]
        public void FindAllMatches_LShape_ClassifiesAsLShape()
        {
            SetRow(0, 0, 3, TileType.SoulstreamShard);
            SetCol(0, 1, 3, TileType.SoulstreamShard);
            var matches = _detector.FindAllMatches();
            var lMatches = matches.Where(m =>
                m.Pattern == MatchPattern.LShape || m.Pattern == MatchPattern.TShape).ToList();
            Assert.GreaterOrEqual(lMatches.Count, 1);
        }

        [Test]
        public void FindAllMatches_StoneBlockBreaksRun()
        {
            // Overwrite the single cell at (0,5) first: the SetUp's cycling
            // init pattern happens to place PetshaCharm at that column, which
            // would accidentally extend a 5-long PetshaCharm run to 6 cells,
            // and the stone at col 2 would then leave a valid 3-match at
            // cols 3-5. Breaking that collision lets us test *just* the
            // stone-block behavior. We intentionally pick CovenSeal: col 6
            // is already PortRune and col 4 will be PetshaCharm, so using
            // CovenSeal at col 5 avoids creating any accidental new match.
            var colFive = new GridPosition(0, 5);
            _board.SetTile(colFive, new Tile(TileType.CovenSeal, colFive));

            SetRow(0, 0, 5, TileType.PetshaCharm);
            _board.SetStoneBlock(0, 2, true);
            var matches = _detector.FindAllMatches();
            Assert.AreEqual(0, matches.Count);
        }

        [Test]
        public void MeterCharge_ThreeMatch_Returns1()
        {
            SetRow(0, 0, 3, TileType.PortRune);
            var matches = _detector.FindAllMatches();
            Assert.AreEqual(1, matches[0].MeterCharge);
        }

        [Test]
        public void MeterCharge_FourMatch_Returns2()
        {
            SetRow(0, 0, 4, TileType.PortRune);
            var matches = _detector.FindAllMatches();
            Assert.AreEqual(2, matches[0].MeterCharge);
        }

        [Test]
        public void MeterCharge_FiveMatch_Returns3()
        {
            SetRow(0, 0, 5, TileType.PortRune);
            var matches = _detector.FindAllMatches();
            Assert.AreEqual(3, matches[0].MeterCharge);
        }
    }
}
