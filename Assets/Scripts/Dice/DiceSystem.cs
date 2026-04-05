using System;
using System.Collections.Generic;
using Matchmancer.Core;

namespace Matchmancer.Dice
{
    /// <summary>
    /// Result of a dice roll — which school was selected and what board effect to apply.
    /// </summary>
    public class DiceRollResult
    {
        public MagickSchool School { get; }
        public List<GridPosition> AffectedPositions { get; set; }
        public string EffectDescription { get; }

        public DiceRollResult(MagickSchool school, string effectDescription)
        {
            School = school;
            EffectDescription = effectDescription;
            AffectedPositions = new List<GridPosition>();
        }
    }

    public class DiceSystem
    {
        private readonly Board.Board _board;
        private readonly Random _rng;

        private static readonly MagickSchool[] AllSchools = (MagickSchool[])Enum.GetValues(typeof(MagickSchool));

        public DiceSystem(Board.Board board, Random rng = null)
        {
            _board = board;
            _rng = rng ?? new Random();
        }

        /// <summary>
        /// Roll a random School of Magick and compute the board effect.
        /// Returns the result — caller applies it to the board.
        /// </summary>
        public DiceRollResult Roll()
        {
            var school = AllSchools[_rng.Next(AllSchools.Length)];
            var result = ResolveSchoolEffect(school);
            return result;
        }

        private DiceRollResult ResolveSchoolEffect(MagickSchool school)
        {
            // MVP: each school has a distinct board-clearing effect
            // These are hooks — visual/audio polish added later
            switch (school)
            {
                case MagickSchool.Evocation:
                    return CreateExplosionEffect(school, "Evocation: 3x3 explosion at random position");

                case MagickSchool.Necromancy:
                    return CreateClearTypeEffect(school, "Necromancy: Drain all tiles of one random type");

                case MagickSchool.Transmutation:
                    return CreateTransmuteEffect(school, "Transmutation: Convert random tiles to a single type");

                case MagickSchool.Divination:
                    return CreateRowClearEffect(school, "Divination: Reveal and clear a full row");

                case MagickSchool.Abjuration:
                    return CreateColumnClearEffect(school, "Abjuration: Shield column — clears full column");

                case MagickSchool.Illusion:
                    return CreateScatterClearEffect(school, "Illusion: Clear 5 random tiles");

                case MagickSchool.Conjuration:
                    return CreateExplosionEffect(school, "Conjuration: Summon 3x3 burst");

                case MagickSchool.Enchantment:
                    return CreateClearTypeEffect(school, "Enchantment: Charm all tiles of one type");

                case MagickSchool.Universal:
                    return CreateCrossClearEffect(school, "Universal: Clear cross pattern at center");

                case MagickSchool.Biomancy:
                    return CreateScatterClearEffect(school, "Biomancy: Organic spread — clear 5 random tiles");

                case MagickSchool.Technomancy:
                    return CreateRowClearEffect(school, "Technomancy: System scan — clear a full row");

                case MagickSchool.Alchemy:
                    return CreateTransmuteEffect(school, "Alchemy: Transmute random tiles");

                case MagickSchool.Fate:
                    return CreateScatterClearEffect(school, "Fate: Twist of destiny — clear 5 random tiles");

                default:
                    return new DiceRollResult(school, "Unknown school");
            }
        }

        private DiceRollResult CreateExplosionEffect(MagickSchool school, string desc)
        {
            var result = new DiceRollResult(school, desc);
            int centerR = _rng.Next(1, Board.Board.Rows - 1);
            int centerC = _rng.Next(1, Board.Board.Cols - 1);

            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    int r = centerR + dr;
                    int c = centerC + dc;
                    if (_board.IsPlayable(r, c))
                        result.AffectedPositions.Add(new GridPosition(r, c));
                }
            }
            return result;
        }

        private DiceRollResult CreateClearTypeEffect(MagickSchool school, string desc)
        {
            var result = new DiceRollResult(school, desc);
            var allTypes = (TileType[])Enum.GetValues(typeof(TileType));
            // Pick a random non-None tile type
            TileType target;
            do { target = allTypes[_rng.Next(allTypes.Length)]; }
            while (target == TileType.None);

            for (int r = 0; r < Board.Board.Rows; r++)
            {
                for (int c = 0; c < Board.Board.Cols; c++)
                {
                    if (_board.IsPlayable(r, c) && _board[r, c].Type == target)
                        result.AffectedPositions.Add(new GridPosition(r, c));
                }
            }
            return result;
        }

        private DiceRollResult CreateTransmuteEffect(MagickSchool school, string desc)
        {
            var result = new DiceRollResult(school, desc);
            // Convert up to 6 random tiles to a single type (clears via match cascade)
            var positions = GetRandomPlayablePositions(6);
            result.AffectedPositions.AddRange(positions);
            return result;
        }

        private DiceRollResult CreateRowClearEffect(MagickSchool school, string desc)
        {
            var result = new DiceRollResult(school, desc);
            int row = _rng.Next(Board.Board.Rows);
            for (int c = 0; c < Board.Board.Cols; c++)
            {
                if (_board.IsPlayable(row, c))
                    result.AffectedPositions.Add(new GridPosition(row, c));
            }
            return result;
        }

        private DiceRollResult CreateColumnClearEffect(MagickSchool school, string desc)
        {
            var result = new DiceRollResult(school, desc);
            int col = _rng.Next(Board.Board.Cols);
            for (int r = 0; r < Board.Board.Rows; r++)
            {
                if (_board.IsPlayable(r, col))
                    result.AffectedPositions.Add(new GridPosition(r, col));
            }
            return result;
        }

        private DiceRollResult CreateScatterClearEffect(MagickSchool school, string desc)
        {
            var result = new DiceRollResult(school, desc);
            result.AffectedPositions.AddRange(GetRandomPlayablePositions(5));
            return result;
        }

        private DiceRollResult CreateCrossClearEffect(MagickSchool school, string desc)
        {
            var result = new DiceRollResult(school, desc);
            int centerR = Board.Board.Rows / 2;
            int centerC = Board.Board.Cols / 2;

            for (int c = 0; c < Board.Board.Cols; c++)
            {
                if (_board.IsPlayable(centerR, c))
                    result.AffectedPositions.Add(new GridPosition(centerR, c));
            }
            for (int r = 0; r < Board.Board.Rows; r++)
            {
                if (r != centerR && _board.IsPlayable(r, centerC))
                    result.AffectedPositions.Add(new GridPosition(r, centerC));
            }
            return result;
        }

        private List<GridPosition> GetRandomPlayablePositions(int count)
        {
            var candidates = new List<GridPosition>();
            for (int r = 0; r < Board.Board.Rows; r++)
            {
                for (int c = 0; c < Board.Board.Cols; c++)
                {
                    if (_board.IsPlayable(r, c) && !_board[r, c].IsEmpty)
                        candidates.Add(new GridPosition(r, c));
                }
            }

            var selected = new List<GridPosition>();
            int toSelect = Math.Min(count, candidates.Count);
            for (int i = 0; i < toSelect; i++)
            {
                int idx = _rng.Next(candidates.Count);
                selected.Add(candidates[idx]);
                candidates.RemoveAt(idx);
            }
            return selected;
        }
    }
}
