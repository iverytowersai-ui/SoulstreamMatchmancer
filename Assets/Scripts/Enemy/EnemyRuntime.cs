using System;
using Matchmancer.Combat;

namespace Matchmancer.Enemy
{
    /// <summary>
    /// Live enemy state for a single battle. Pure C# — no UnityEngine — so
    /// all combat logic is unit-testable.
    ///
    /// Damage pipeline (applied in <see cref="ApplyCombatEffect"/>):
    ///   1. <b>Break</b> effects chip away at armor.
    ///   2. <b>Damage</b> effects:
    ///      a. Multiplied by vulnerability (if active).
    ///      b. Reduced by enemy <see cref="Defense"/>.
    ///      c. Absorbed by remaining armor first, then HP.
    ///   3. <b>Debuff</b> effects add poison stacks and/or vulnerability duration.
    ///   4. Other roles (Energy, Defense, Luck) are ignored — they're player-side.
    ///
    /// Status ticks happen in <see cref="OnPlayerTurnEnd"/>, which should be
    /// called exactly once after every player move:
    ///   - Poison deals <c>tuning.PoisonDamagePerTurn × PoisonStacks</c> HP,
    ///     bypassing both armor and defense. Its duration then decrements;
    ///     when it hits zero, stacks clear.
    ///   - Vulnerability duration decrements.
    /// </summary>
    public class EnemyRuntime
    {
        private readonly CombatTuning _tuning;

        // ---- Identity ----
        public string DisplayName { get; }

        // ---- Vitals ----
        public int  MaxHp      { get; }
        public int  CurrentHp  { get; private set; }
        public int  MaxArmor   { get; }
        public int  CurrentArmor { get; private set; }
        public float Defense   { get; }

        // ---- Offensive ----
        public float BaseAttackPower { get; }
        public int   AttacksPerTurn  { get; }

        // ---- Status effects ----
        public int PoisonStacks              { get; private set; }
        public int PoisonTurnsRemaining      { get; private set; }
        public int VulnerabilityTurnsRemaining { get; private set; }

        public bool IsDefeated => CurrentHp <= 0;
        public bool IsVulnerable => VulnerabilityTurnsRemaining > 0;
        public bool IsPoisoned   => PoisonStacks > 0 && PoisonTurnsRemaining > 0;

        // ---- Events (for UI / VFX) ----
        /// <summary>(oldHp, newHp, amount) — amount &lt; 0 on damage, &gt; 0 on heal.</summary>
        public event Action<int, int, int> OnHpChanged;
        /// <summary>Fired once when HP first drops to 0.</summary>
        public event Action OnDefeated;
        /// <summary>(oldArmor, newArmor).</summary>
        public event Action<int, int> OnArmorChanged;
        /// <summary>Fired after poison ticks damage in OnPlayerTurnEnd.</summary>
        public event Action<int> OnPoisonTicked;
        /// <summary>Fired when the enemy uses an attack. Amount is the damage dealt.</summary>
        public event Action<float> OnAttack;

        public EnemyRuntime(
            string       displayName,
            int          maxHp,
            float        defense,
            int          maxArmor,
            float        baseAttackPower,
            int          attacksPerTurn,
            CombatTuning tuning)
        {
            if (tuning == null) throw new ArgumentNullException(nameof(tuning));
            if (maxHp < 1)      throw new ArgumentOutOfRangeException(nameof(maxHp));

            DisplayName     = displayName ?? "Unknown";
            MaxHp           = maxHp;
            CurrentHp       = maxHp;
            Defense         = Math.Max(0f, defense);
            MaxArmor        = Math.Max(0, maxArmor);
            CurrentArmor    = MaxArmor;
            BaseAttackPower = Math.Max(0f, baseAttackPower);
            AttacksPerTurn  = Math.Max(1, attacksPerTurn);
            _tuning         = tuning;
        }

        // ------------------------------------------------------------------
        // Incoming effects
        // ------------------------------------------------------------------

        /// <summary>
        /// Apply a single resolved combat effect. Enemy absorbs damage/break/debuff
        /// effects; player-side effects (Energy/Defense/Luck) are silently ignored.
        /// </summary>
        public void ApplyCombatEffect(CombatEffect effect)
        {
            if (IsDefeated) return;

            switch (effect.Role)
            {
                case CombatRole.Damage:
                    ApplyDamage(effect.DamageDealt);
                    break;
                case CombatRole.Break:
                    ApplyArmorDamage(effect.ArmorDamageDealt);
                    break;
                case CombatRole.Debuff:
                    if (effect.AppliesPoison)
                        AddPoisonStack(effect.DebuffDuration);
                    if (effect.AppliesVulnerability)
                        AddVulnerability(effect.DebuffDuration);
                    break;
                // Energy / Defense / Luck / None → player-side, ignore.
            }
        }

        /// <summary>
        /// Raw-HP damage entry point. Vulnerability and Defense are applied here.
        /// Armor absorbs first; any overflow reduces HP.
        /// </summary>
        public int ApplyDamage(float rawDamage)
        {
            if (rawDamage <= 0f || IsDefeated) return 0;

            float multiplied = IsVulnerable
                ? rawDamage * _tuning.VulnerabilityMult
                : rawDamage;

            float afterDefense = Math.Max(0f, multiplied - Defense);
            int   dmg          = (int)Math.Round(afterDefense);
            if (dmg <= 0) return 0;

            // Armor soak
            if (CurrentArmor > 0)
            {
                int absorbed = Math.Min(CurrentArmor, dmg);
                int oldArmor = CurrentArmor;
                CurrentArmor -= absorbed;
                dmg          -= absorbed;
                OnArmorChanged?.Invoke(oldArmor, CurrentArmor);
            }

            if (dmg <= 0) return 0;

            int oldHp = CurrentHp;
            CurrentHp = Math.Max(0, CurrentHp - dmg);
            OnHpChanged?.Invoke(oldHp, CurrentHp, -dmg);

            if (CurrentHp == 0 && oldHp > 0)
                OnDefeated?.Invoke();

            return dmg;
        }

        /// <summary>Damage the enemy's armor pool only. No HP spillover.</summary>
        public int ApplyArmorDamage(float rawArmorDmg)
        {
            if (rawArmorDmg <= 0f || CurrentArmor <= 0) return 0;

            int dmg      = (int)Math.Round(rawArmorDmg);
            int oldArmor = CurrentArmor;
            CurrentArmor = Math.Max(0, CurrentArmor - dmg);
            OnArmorChanged?.Invoke(oldArmor, CurrentArmor);
            return oldArmor - CurrentArmor;
        }

        // ------------------------------------------------------------------
        // Status effects
        // ------------------------------------------------------------------

        /// <summary>Add a poison stack and refresh duration if shorter.</summary>
        public void AddPoisonStack(int duration)
        {
            PoisonStacks++;
            if (duration > PoisonTurnsRemaining)
                PoisonTurnsRemaining = duration;
        }

        /// <summary>Apply or refresh a vulnerability window.</summary>
        public void AddVulnerability(int duration)
        {
            if (duration > VulnerabilityTurnsRemaining)
                VulnerabilityTurnsRemaining = duration;
        }

        // ------------------------------------------------------------------
        // Turn bookkeeping
        // ------------------------------------------------------------------

        /// <summary>
        /// Call exactly once after the player finishes their move (cascade done).
        /// Ticks poison DoT and decrements status durations.
        /// </summary>
        public void OnPlayerTurnEnd()
        {
            if (IsDefeated) return;

            // Poison DoT — bypasses defense/armor by design.
            if (IsPoisoned)
            {
                int dot = (int)(_tuning.PoisonDamagePerTurn * PoisonStacks);
                if (dot > 0)
                {
                    int oldHp = CurrentHp;
                    CurrentHp = Math.Max(0, CurrentHp - dot);
                    OnHpChanged?.Invoke(oldHp, CurrentHp, -dot);
                    OnPoisonTicked?.Invoke(dot);
                    if (CurrentHp == 0 && oldHp > 0)
                    {
                        OnDefeated?.Invoke();
                        return;
                    }
                }

                PoisonTurnsRemaining--;
                if (PoisonTurnsRemaining <= 0)
                {
                    PoisonTurnsRemaining = 0;
                    PoisonStacks         = 0;
                }
            }

            if (VulnerabilityTurnsRemaining > 0)
                VulnerabilityTurnsRemaining--;
        }

        /// <summary>
        /// Execute the enemy's attack phase. Returns total damage intended
        /// for the player (character-side defense/shield is applied later
        /// by CharacterRuntime in Skill 14).
        /// </summary>
        public float RollAttack()
        {
            if (IsDefeated) return 0f;
            float total = BaseAttackPower * AttacksPerTurn;
            OnAttack?.Invoke(total);
            return total;
        }
    }
}
