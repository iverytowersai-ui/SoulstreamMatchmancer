using Matchmancer.StoneBlocks;

namespace Matchmancer.Objectives
{
    public enum ObjectiveType
    {
        ReachScore,       // Reach target score within move limit
        ClearAllStones,   // Destroy all stone blocks within move limit
        CollectTiles,     // Collect N tiles of a specific type
        Survive           // Boss level: survive N turns while stones spread
    }

    public enum LevelResult
    {
        InProgress,
        Victory,
        Defeat
    }

    public class ObjectiveConfig
    {
        public ObjectiveType Type { get; set; }
        public int TargetScore { get; set; }
        public int TargetStoneClears { get; set; }
        public int SurviveTurns { get; set; }
    }

    public class ObjectiveChecker
    {
        private readonly ObjectiveConfig _config;
        private readonly Scoring _scoring;
        private readonly MoveTracker _moveTracker;
        private readonly StoneBlockSystem _stoneBlocks;

        private int _turnsElapsed;

        public ObjectiveChecker(ObjectiveConfig config, Scoring scoring, MoveTracker moveTracker, StoneBlockSystem stoneBlocks)
        {
            _config = config;
            _scoring = scoring;
            _moveTracker = moveTracker;
            _stoneBlocks = stoneBlocks;
        }

        public void IncrementTurn()
        {
            _turnsElapsed++;
        }

        /// <summary>
        /// Check win/lose after each turn resolves.
        /// </summary>
        public LevelResult Evaluate()
        {
            switch (_config.Type)
            {
                case ObjectiveType.ReachScore:
                    if (_scoring.Score >= _config.TargetScore) return LevelResult.Victory;
                    if (_moveTracker.IsExhausted) return LevelResult.Defeat;
                    break;

                case ObjectiveType.ClearAllStones:
                    if (_stoneBlocks.RemainingStones <= 0) return LevelResult.Victory;
                    if (_moveTracker.IsExhausted) return LevelResult.Defeat;
                    break;

                case ObjectiveType.Survive:
                    if (_turnsElapsed >= _config.SurviveTurns) return LevelResult.Victory;
                    if (_moveTracker.IsExhausted) return LevelResult.Defeat;
                    break;

                default:
                    break;
            }

            return LevelResult.InProgress;
        }
    }
}
