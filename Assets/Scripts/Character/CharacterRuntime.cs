using System;
using Matchmancer.Combat;

namespace Matchmancer.Character
{
    /// <summary>
    /// Live player state during a battle. Pure C# — no UnityEngine — so tests
    /// can exercise the full loop without a scene.
    ///
    /// Responsibilities:
    ///   • Hold HP, shield, energy, live attack/luck.
    ///   • Receive enemy attacks via <see cref="TakeEnemyDamage"/>.
    ///   • Consume player-facing <see cref="CombatEffect"/> (Defense → shield,
    ///     Energy → meter; Damage/Break/Debuff/Luck are enemy-facing and ignored).
    ///   • Track XP and level up, recalculating stats on each level.
    ///   • Drive the ultimate: fills on energy, queues on player input,
    ///     returns a damage multiplier when consumed.
    ///
    /// Damage pipeline on incoming hits:
    ///   shield absorbs first  →  defense flat subtract  →  clamp to min 1
    ///   → HP reduction (min 0)  → OnDefeated when HP hits zero.
    ///
    /// Events fire after state has mutated so listeners always see the new
    /// values. Defeated runtimes no-op on further mutation.
    /// </summary>
    public class CharacterRuntime
    {
        private readonly CharacterTuning _tuning;

        // ------------------------------------------------------------------
        // Identity
        // ------------------------------------------------------------------
        public string DisplayName => _tuning.DisplayName;

        // ------------------------------------------------------------------
        // Level & XP
        // ------------------------------------------------------------------
        public int Level     { get; private set; }
        public int CurrentXp { get; private set; }
        public int MaxLevel  => (_tuning.XpPerLevel?.Length ?? 0) + 1;
        public bool IsMaxLevel => Level >= MaxLevel;

        // ------------------------------------------------------------------
        // Core stats (recalculated on level up)
        // ------------------------------------------------------------------
        public int   MaxHp          { get; private set; }
        public int   CurrentHp      { get; private set; }
        public float CurrentAttack  { get; private set; }
        public float CurrentDefense { get; private set; }
        public float CurrentLuck    { get; private set; }

        // ------------------------------------------------------------------
        // Battle resources
        // ------------------------------------------------------------------
        public float CurrentShield { get; private set; }
        public float CurrentEnergy { get; private set; }
        public float MaxEnergy     => _tuning.MaxEnergy;

        // ------------------------------------------------------------------
        // Ultimate
        // ------------------------------------------------------------------
        public bool UltimateReady   { get; private set; }
        public bool UltimateQueued  { get; private set; }
        public string UltimateName  => _tuning.UltimateName;

        // ------------------------------------------------------------------
        // Defeat
        // ------------------------------------------------------------------
        public bool IsDefeated { get; private set; }

        // ------------------------------------------------------------------
        // Events
        // ------------------------------------------------------------------
        public event Action<int, int, int>   OnHpChanged;       // oldHp, newHp, delta (negative = damage)
        public event Action<float>           OnShieldChanged;   // current shield
        public event Action<float, float>    OnEnergyChanged;   // current, max
        public event Action<bool>            OnUltimateReadyChanged; // true = filled, false = consumed
        public event Action<float>           OnDamageTaken;     // damage dealt after reduction
        public event Action                  OnDefeated;
        public event Action<int, int>        OnXpChanged;       // current xp, xp to next level
        public event Action<int>             OnLevelUp;         // new level

        // ------------------------------------------------------------------
        // Construction
        // ------------------------------------------------------------------
        public CharacterRuntime(CharacterTuning tuning, int startingLevel = 1)
        {
            _tuning = tuning ?? throw new ArgumentNullException(nameof(tuning));
            if (startingLevel < 1)
                throw new ArgumentOutOfRangeException(nameof(startingLevel),
                    "Starting level must be ≥ 1.");
            if (_tuning.BaseMaxHp <= 0)
                throw new ArgumentOutOfRangeException(nameof(tuning),
                    "BaseMaxHp must be > 0.");

            Level          = Math.Min(startingLevel, MaxLevel);
            CurrentXp      = 0;
            IsDefeated     = false;
            CurrentShield  = 0f;
            CurrentEnergy  = 0f;
            UltimateReady  = false;
            UltimateQueued = false;

            RecalculateStatsForLevel(fullHeal: true);
        }

        private void RecalculateStatsForLevel(bool fullHeal)
        {
            int stepsAbove1 = Math.Max(0, Level - 1);

            MaxHp          = _tuning.BaseMaxHp  + _tuning.HpPerLevel      * stepsAbove1;
            CurrentAttack  = _tuning.BaseAttack + _tuning.AttackPerLevel  * stepsAbove1;
            CurrentDefense = _tuning.BaseDefense+ _tuning.DefensePerLevel * stepsAbove1;
            CurrentLuck    = _tuning.BaseLuck   + _tuning.LuckPerLevel    * stepsAbove1;

            if (fullHeal) CurrentHp = MaxHp;
            else          CurrentHp = Math.Min(CurrentHp, MaxHp);
        }

        // ------------------------------------------------------------------
        // Incoming damage from the enemy
        // ------------------------------------------------------------------

        /// <summary>
        /// Player absorbs an enemy attack. Shield eats first, then flat
        /// defense subtracts, then HP. A hit that would deal positive damage
        /// always deals at least 1 (chip damage).
        /// Returns the HP damage actually dealt.
        /// </summary>
        public int TakeEnemyDamage(float rawDamage)
        {
            if (IsDefeated || rawDamage <= 0f) return 0;

            float remaining = rawDamage;

            // 1. Shield absorbs first
            if (CurrentShield > 0f)
            {
                float absorbed = Math.Min(CurrentShield, remaining);
                CurrentShield -= absorbed;
                remaining     -= absorbed;
                OnShieldChanged?.Invoke(CurrentShield);
            }

            if (remaining <= 0f) return 0;

            // 2. Flat defense — chip damage guaranteed at 1
            float afterDefense = remaining - CurrentDefense;
            if (afterDefense < 1f) afterDefense = 1f;

            // 3. Apply to HP
            int dmg = (int)Math.Round(afterDefense);
            int oldHp = CurrentHp;
            CurrentHp = Math.Max(0, CurrentHp - dmg);
            int delta = CurrentHp - oldHp; // negative

            OnHpChanged?.Invoke(oldHp, CurrentHp, delta);
            OnDamageTaken?.Invoke(dmg);

            if (CurrentHp == 0 && oldHp > 0)
            {
                IsDefeated = true;
                OnDefeated?.Invoke();
            }

            return -delta;
        }

        // ------------------------------------------------------------------
        // Player-facing combat effects from tile matches
        // ------------------------------------------------------------------

        /// <summary>
        /// Apply a single <see cref="CombatEffect"/> to the player.
        /// Only <see cref="CombatRole.Defense"/> (shield) and
        /// <see cref="CombatRole.Energy"/> (ultimate meter) are player-facing.
        /// Damage/Break/Debuff/Luck are enemy-facing and ignored here.
        /// </summary>
        public void ApplyCombatEffect(CombatEffect effect)
        {
            if (IsDefeated) return;

            switch (effect.Role)
            {
                case CombatRole.Defense:
                    AddShield(effect.ShieldGenerated);
                    break;

                case CombatRole.Energy:
                    AddEnergy(effect.EnergyGenerated);
                    break;

                // Damage / Break / Debuff / Luck → enemy-facing, ignored.
                default:
                    break;
            }
        }

        // ------------------------------------------------------------------
        // Shield
        // ------------------------------------------------------------------

        public void AddShield(float amount)
        {
            if (IsDefeated || amount <= 0f) return;
            CurrentShield += amount;
            OnShieldChanged?.Invoke(CurrentShield);
        }

        // ------------------------------------------------------------------
        // Energy / Ultimate
        // ------------------------------------------------------------------

        public void AddEnergy(float amount)
        {
            if (IsDefeated || amount <= 0f) return;

            float before = CurrentEnergy;
            CurrentEnergy = Math.Min(MaxEnergy, CurrentEnergy + amount);

            if (CurrentEnergy != before)
                OnEnergyChanged?.Invoke(CurrentEnergy, MaxEnergy);

            if (!UltimateReady && CurrentEnergy >= MaxEnergy)
            {
                UltimateReady = true;
                OnUltimateReadyChanged?.Invoke(true);
            }
        }

        /// <summary>
        /// Player taps the Ultimate button. Queues the ultimate to fire on
        /// the next Damage wave. No-op if not ready or defeated.
        /// Returns true if the queue request succeeded.
        /// </summary>
        public bool QueueUltimate()
        {
            if (IsDefeated || !UltimateReady) return false;
            UltimateQueued = true;
            return true;
        }

        /// <summary>
        /// Called by combat code right before a Damage wave is computed.
        /// If the ultimate is queued, consumes it (resets energy/ready flags)
        /// and returns the configured multiplier. Otherwise returns 1.
        /// </summary>
        public float ConsumeUltimateMultiplier()
        {
            if (!UltimateQueued) return 1f;

            UltimateQueued = false;
            UltimateReady  = false;
            CurrentEnergy  = 0f;

            OnEnergyChanged?.Invoke(CurrentEnergy, MaxEnergy);
            OnUltimateReadyChanged?.Invoke(false);

            return _tuning.UltimateDamageMultiplier;
        }

        // ------------------------------------------------------------------
        // Healing
        // ------------------------------------------------------------------

        public void Heal(int amount)
        {
            if (IsDefeated || amount <= 0) return;
            int oldHp = CurrentHp;
            CurrentHp = Math.Min(MaxHp, CurrentHp + amount);
            int delta = CurrentHp - oldHp;
            if (delta != 0)
                OnHpChanged?.Invoke(oldHp, CurrentHp, delta);
        }

        // ------------------------------------------------------------------
        // XP & Level Up
        // ------------------------------------------------------------------

        /// <summary>
        /// Award XP. Rolls over into as many level-ups as earned. Each level
        /// up recalculates stats and grants a small heal (hpPerLevel).
        /// </summary>
        public void AwardXp(int amount)
        {
            if (amount <= 0 || IsMaxLevel) return;

            CurrentXp += amount;

            int toNext = XpToNextLevel();
            while (!IsMaxLevel && toNext > 0 && CurrentXp >= toNext)
            {
                CurrentXp -= toNext;
                Level++;

                int oldMaxHp = MaxHp;
                RecalculateStatsForLevel(fullHeal: false);
                int healChunk = MaxHp - oldMaxHp;
                if (healChunk > 0)
                {
                    int oldHp = CurrentHp;
                    CurrentHp = Math.Min(MaxHp, CurrentHp + healChunk);
                    int delta = CurrentHp - oldHp;
                    if (delta != 0)
                        OnHpChanged?.Invoke(oldHp, CurrentHp, delta);
                }

                OnLevelUp?.Invoke(Level);
                toNext = XpToNextLevel();
            }

            if (IsMaxLevel) CurrentXp = 0;
            OnXpChanged?.Invoke(CurrentXp, XpToNextLevel());
        }

        /// <summary>
        /// XP required to reach the next level from the current level.
        /// Zero when at the hard cap.
        /// </summary>
        public int XpToNextLevel()
        {
            if (_tuning.XpPerLevel == null || _tuning.XpPerLevel.Length == 0) return 0;
            int idx = Level - 1;
            if (idx < 0 || idx >= _tuning.XpPerLevel.Length) return 0;
            return _tuning.XpPerLevel[idx];
        }
    }
}
