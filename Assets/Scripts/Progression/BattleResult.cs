using System;

namespace Matchmancer.Progression
{
    /// <summary>
    /// Pure C# snapshot of everything that matters at the moment a battle
    /// ends. The <see cref="StarEvaluator"/> converts one of these plus a
    /// <see cref="Matchmancer.Core.LevelConfig"/> into a <see cref="StarResult"/>.
    ///
    /// Built by the results pipeline from:
    ///   • ObjectiveChecker  → Victory flag
    ///   • Scoring           → FinalScore (after end-of-battle bonuses)
    ///   • MoveTracker       → MovesRemaining / MovesUsed
    ///   • CharacterRuntime  → HpRemaining / MaxHp
    ///   • CombatStats       → MaxComboAchieved / DamageDealt / DamageTaken
    /// </summary>
    [Serializable]
    public struct BattleResult : IEquatable<BattleResult>
    {
        /// <summary>Global level index (1..100). 0 means "unspecified".</summary>
        public int GlobalLevelIndex;

        /// <summary>True if the player met the level objective.</summary>
        public bool Victory;

        /// <summary>Final scoring value to compare to level star thresholds.</summary>
        public int FinalScore;

        // -------- Efficiency --------
        public int MovesUsed;
        public int MovesRemaining;
        public int TurnsTaken;

        // -------- Survival --------
        public int HpRemaining;
        public int MaxHp;

        // -------- Combat flair --------
        public int MaxComboAchieved;
        public int DamageDealt;
        public int DamageTaken;

        /// <summary>
        /// Convenience 0..1 fraction of HP left at battle end. Zero when
        /// <see cref="MaxHp"/> is not set.
        /// </summary>
        public float HpFraction
        {
            get
            {
                if (MaxHp <= 0) return 0f;
                float f = (float)HpRemaining / MaxHp;
                if (f < 0f) return 0f;
                if (f > 1f) return 1f;
                return f;
            }
        }

        /// <summary>True if the player ended the battle at full HP.</summary>
        public bool Flawless => MaxHp > 0 && HpRemaining >= MaxHp && DamageTaken == 0;

        public bool Equals(BattleResult other) =>
            GlobalLevelIndex  == other.GlobalLevelIndex
         && Victory           == other.Victory
         && FinalScore        == other.FinalScore
         && MovesUsed         == other.MovesUsed
         && MovesRemaining    == other.MovesRemaining
         && TurnsTaken        == other.TurnsTaken
         && HpRemaining       == other.HpRemaining
         && MaxHp             == other.MaxHp
         && MaxComboAchieved  == other.MaxComboAchieved
         && DamageDealt       == other.DamageDealt
         && DamageTaken       == other.DamageTaken;

        public override bool Equals(object obj) => obj is BattleResult r && Equals(r);

        public override int GetHashCode()
        {
            unchecked
            {
                int h = 17;
                h = h * 31 + GlobalLevelIndex;
                h = h * 31 + (Victory ? 1 : 0);
                h = h * 31 + FinalScore;
                h = h * 31 + MovesUsed;
                h = h * 31 + MovesRemaining;
                h = h * 31 + TurnsTaken;
                h = h * 31 + HpRemaining;
                h = h * 31 + MaxHp;
                h = h * 31 + MaxComboAchieved;
                h = h * 31 + DamageDealt;
                h = h * 31 + DamageTaken;
                return h;
            }
        }
    }
}
