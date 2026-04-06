using NUnit.Framework;
using Matchmancer.Core;
using Matchmancer.Objectives;
using Matchmancer.StoneBlocks;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class ObjectiveCheckerTests
    {
        [Test]
        public void ReachScore_ScoreMet_Victory()
        {
            var scoring = new Scoring(100, 200, 300, 400, 500);
            var moveTracker = new MoveTracker(10);
            var board = new Board.Board();
            board.Initialize(() => TileType.PortRune);
            var stones = new StoneBlockSystem(board);
            var config = new ObjectiveConfig { Type = ObjectiveType.ReachScore, TargetScore = 100 };
            var checker = new ObjectiveChecker(config, scoring, moveTracker, stones);

            scoring.AddMatchScore(10); // 10 * 10 = 100

            Assert.AreEqual(LevelResult.Victory, checker.Evaluate());
        }

        [Test]
        public void ReachScore_MovesExhausted_Defeat()
        {
            var scoring = new Scoring(100, 200, 300, 400, 500);
            var moveTracker = new MoveTracker(1);
            var board = new Board.Board();
            board.Initialize(() => TileType.PortRune);
            var stones = new StoneBlockSystem(board);
            var config = new ObjectiveConfig { Type = ObjectiveType.ReachScore, TargetScore = 100 };
            var checker = new ObjectiveChecker(config, scoring, moveTracker, stones);

            moveTracker.DeductMove();
            Assert.AreEqual(LevelResult.Defeat, checker.Evaluate());
        }

        [Test]
        public void ClearAllStones_AllDestroyed_Victory()
        {
            var scoring = new Scoring(100, 200, 300, 400, 500);
            var moveTracker = new MoveTracker(10);
            var board = new Board.Board();
            board.Initialize(() => TileType.PortRune);
            var stones = new StoneBlockSystem(board);
            var config = new ObjectiveConfig { Type = ObjectiveType.ClearAllStones };
            var checker = new ObjectiveChecker(config, scoring, moveTracker, stones);

            Assert.AreEqual(LevelResult.Victory, checker.Evaluate());
        }

        [Test]
        public void ClearAllStones_StonesRemain_MovesLeft_InProgress()
        {
            var scoring = new Scoring(100, 200, 300, 400, 500);
            var moveTracker = new MoveTracker(10);
            var board = new Board.Board();
            board.Initialize(() => TileType.PortRune);
            var stones = new StoneBlockSystem(board);
            stones.PlaceStone(3, 3, 2);
            var config = new ObjectiveConfig { Type = ObjectiveType.ClearAllStones };
            var checker = new ObjectiveChecker(config, scoring, moveTracker, stones);

            Assert.AreEqual(LevelResult.InProgress, checker.Evaluate());
        }

        [Test]
        public void Survive_TurnsReached_Victory()
        {
            var scoring = new Scoring(100, 200, 300, 400, 500);
            var moveTracker = new MoveTracker(20);
            var board = new Board.Board();
            board.Initialize(() => TileType.PortRune);
            var stones = new StoneBlockSystem(board);
            var config = new ObjectiveConfig { Type = ObjectiveType.Survive, SurviveTurns = 3 };
            var checker = new ObjectiveChecker(config, scoring, moveTracker, stones);

            checker.IncrementTurn();
            checker.IncrementTurn();
            Assert.AreEqual(LevelResult.InProgress, checker.Evaluate());
            checker.IncrementTurn();
            Assert.AreEqual(LevelResult.Victory, checker.Evaluate());
        }
    }
}
