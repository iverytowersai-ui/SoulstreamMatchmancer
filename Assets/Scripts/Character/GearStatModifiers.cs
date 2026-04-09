using System;

namespace Matchmancer.Character
{
    /// <summary>
    /// Pure C# bucket of additive stat modifiers from one or more pieces of
    /// gear. Two layers per stat:
    ///   • Flat   — added AFTER percent scaling  (e.g. +25 Attack)
    ///   • Pct    — added BEFORE flat           (e.g. +0.15 = +15% Attack)
    ///
    /// Final formula (applied by <see cref="CharacterRuntime"/>):
    ///     final = (base + perLevel * (level-1)) * (1 + pct) + flat
    ///
    /// Modifiers compose via operator+ so the inventory can sum every
    /// equipped slot into a single value to hand to the runtime.
    /// </summary>
    [Serializable]
    public struct GearStatModifiers : IEquatable<GearStatModifiers>
    {
        // -------- Flat bonuses ----------
        public int   FlatMaxHp;
        public float FlatAttack;
        public float FlatDefense;
        public float FlatLuck;

        // -------- Percent bonuses (0.15f == +15%) ----------
        public float PctMaxHp;
        public float PctAttack;
        public float PctDefense;
        public float PctLuck;

        public static GearStatModifiers Zero => default;

        public bool IsZero =>
            FlatMaxHp == 0 && FlatAttack == 0f && FlatDefense == 0f && FlatLuck == 0f &&
            PctMaxHp  == 0f && PctAttack == 0f && PctDefense == 0f && PctLuck  == 0f;

        // ------------------------------------------------------------------
        // Composition
        // ------------------------------------------------------------------
        public static GearStatModifiers operator +(GearStatModifiers a, GearStatModifiers b)
        {
            return new GearStatModifiers
            {
                FlatMaxHp   = a.FlatMaxHp   + b.FlatMaxHp,
                FlatAttack  = a.FlatAttack  + b.FlatAttack,
                FlatDefense = a.FlatDefense + b.FlatDefense,
                FlatLuck    = a.FlatLuck    + b.FlatLuck,
                PctMaxHp    = a.PctMaxHp    + b.PctMaxHp,
                PctAttack   = a.PctAttack   + b.PctAttack,
                PctDefense  = a.PctDefense  + b.PctDefense,
                PctLuck     = a.PctLuck     + b.PctLuck,
            };
        }

        // ------------------------------------------------------------------
        // Apply helpers — used by CharacterRuntime stat recalc
        // ------------------------------------------------------------------

        /// <summary>
        /// Scale an int base stat (MaxHp) by percent and add flat. Clamped
        /// to a minimum of 1 so gear can never brick a stat.
        /// </summary>
        public int ApplyToMaxHp(int rawMaxHp)
        {
            float scaled = rawMaxHp * (1f + PctMaxHp) + FlatMaxHp;
            int final = (int)Math.Round(scaled);
            return final < 1 ? 1 : final;
        }

        public float ApplyToAttack (float raw) => Math.Max(0f, raw * (1f + PctAttack)  + FlatAttack);
        public float ApplyToDefense(float raw) => Math.Max(0f, raw * (1f + PctDefense) + FlatDefense);
        public float ApplyToLuck   (float raw) => Math.Max(0f, raw * (1f + PctLuck)    + FlatLuck);

        // ------------------------------------------------------------------
        // Equality
        // ------------------------------------------------------------------
        public bool Equals(GearStatModifiers other)
        {
            return FlatMaxHp   == other.FlatMaxHp
                && FlatAttack  == other.FlatAttack
                && FlatDefense == other.FlatDefense
                && FlatLuck    == other.FlatLuck
                && PctMaxHp    == other.PctMaxHp
                && PctAttack   == other.PctAttack
                && PctDefense  == other.PctDefense
                && PctLuck     == other.PctLuck;
        }

        public override bool Equals(object obj) => obj is GearStatModifiers m && Equals(m);

        public override int GetHashCode()
        {
            unchecked
            {
                int h = 17;
                h = h * 31 + FlatMaxHp;
                h = h * 31 + FlatAttack.GetHashCode();
                h = h * 31 + FlatDefense.GetHashCode();
                h = h * 31 + FlatLuck.GetHashCode();
                h = h * 31 + PctMaxHp.GetHashCode();
                h = h * 31 + PctAttack.GetHashCode();
                h = h * 31 + PctDefense.GetHashCode();
                h = h * 31 + PctLuck.GetHashCode();
                return h;
            }
        }

        public static bool operator ==(GearStatModifiers a, GearStatModifiers b) => a.Equals(b);
        public static bool operator !=(GearStatModifiers a, GearStatModifiers b) => !a.Equals(b);
    }
}
