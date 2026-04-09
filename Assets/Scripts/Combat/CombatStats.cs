using System;

namespace Matchmancer.Combat
{
    /// <summary>
    /// Live battle statistics tracker. One instance per battle.
    /// Consumed by the results screen, star rating, and achievements at
    /// level end. Pure C# — mirrors the style of <c>Scoring.cs</c> so
    /// tests can drive it without Unity.
    ///
    /// Ownership: <c>BoardController</c> creates one per <c>InitializeLevel</c>
    /// and passes it to <c>CombatResolver</c> (Skill 12) which records events
    /// as matches are resolved.
    /// </summary>
    public class CombatStats
    {
        #region Damage
        public float TotalDamageDealt  { get; private set; }
        public float MaxSingleHit      { get; private set; }
        public int   TotalCriticalHits { get; private set; }
        #endregion

        #region Combos
        public int CurrentCombo { get; private set; }
        public int MaxCombo     { get; private set; }
        #endregion

        #region Moves
        public int TotalMoves         { get; private set; }
        public int InventoryMovesUsed { get; private set; }
        public int ClutchMoves        { get; private set; }
        #endregion

        #region Defense & Energy
        public float TotalShieldGenerated { get; private set; }
        public float TotalEnergyGenerated { get; private set; }
        public int   UltimatesActivated   { get; private set; }
        #endregion

        #region Debuff / Break / Luck
        public int LuckTilesMatched  { get; private set; }
        public int DebuffsApplied    { get; private set; }
        public int BreaksTriggered   { get; private set; }
        #endregion

        #region Completion
        public bool  IsVictory      { get; private set; }
        public float BattleDuration { get; private set; }
        #endregion

        #region Events
        public event Action<float, bool> OnDamageDealt;  // (amount, isCrit)
        public event Action<int>         OnComboChanged;
        public event Action<float>       OnShieldGained;
        public event Action<float>       OnEnergyGained;
        #endregion

        /// <summary>Wipe all counters. Call at the start of every battle.</summary>
        public void ResetForNewBattle()
        {
            TotalDamageDealt     = 0f;
            MaxSingleHit         = 0f;
            TotalCriticalHits    = 0;
            CurrentCombo         = 0;
            MaxCombo             = 0;
            TotalMoves           = 0;
            InventoryMovesUsed   = 0;
            ClutchMoves          = 0;
            TotalShieldGenerated = 0f;
            TotalEnergyGenerated = 0f;
            UltimatesActivated   = 0;
            LuckTilesMatched     = 0;
            DebuffsApplied       = 0;
            BreaksTriggered      = 0;
            IsVictory            = false;
            BattleDuration       = 0f;
        }

        #region Recorders

        /// <summary>Record a damage event (post-defense-reduction). Updates max hit and crit count.</summary>
        public void RecordDamage(float amount, bool isCrit)
        {
            if (amount < 0f) amount = 0f;
            TotalDamageDealt += amount;
            if (amount > MaxSingleHit) MaxSingleHit = amount;
            if (isCrit) TotalCriticalHits++;
            OnDamageDealt?.Invoke(amount, isCrit);
        }

        /// <summary>Set the current cascade depth and update MaxCombo if a new record.</summary>
        public void RecordCombo(int comboCount)
        {
            if (comboCount < 0) comboCount = 0;
            CurrentCombo = comboCount;
            if (comboCount > MaxCombo) MaxCombo = comboCount;
            OnComboChanged?.Invoke(comboCount);
        }

        /// <summary>Cascade ended — reset the current combo to 0 without touching MaxCombo.</summary>
        public void ResetCombo()
        {
            CurrentCombo = 0;
            OnComboChanged?.Invoke(0);
        }

        public void RecordShield(float amount)
        {
            if (amount <= 0f) return;
            TotalShieldGenerated += amount;
            OnShieldGained?.Invoke(amount);
        }

        public void RecordEnergy(float amount)
        {
            if (amount <= 0f) return;
            TotalEnergyGenerated += amount;
            OnEnergyGained?.Invoke(amount);
        }

        public void RecordMove()             => TotalMoves++;
        public void RecordInventoryMove()    => InventoryMovesUsed++;
        public void RecordClutchMove()       => ClutchMoves++;
        public void RecordUltimate()         => UltimatesActivated++;
        public void RecordLuckTile(int n = 1)    => LuckTilesMatched += n;
        public void RecordDebuffApplied()    => DebuffsApplied++;
        public void RecordBreakTriggered()   => BreaksTriggered++;

        /// <summary>Seal the battle: set victory flag and final duration.</summary>
        public void FinalizeBattle(bool won, float durationSeconds)
        {
            IsVictory      = won;
            BattleDuration = durationSeconds;
        }

        #endregion
    }
}
