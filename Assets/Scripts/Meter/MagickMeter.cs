namespace Matchmancer.Meter
{
    public class MagickMeter
    {
        public int CurrentCharge { get; private set; }
        public int MaxCapacity { get; private set; }
        public bool IsFull => CurrentCharge >= MaxCapacity;

        public MagickMeter(int maxCapacity = 10)
        {
            MaxCapacity = maxCapacity;
            CurrentCharge = 0;
        }

        /// <summary>
        /// Add charge from a match. Clamped at max — no overflow for MVP.
        /// </summary>
        public void AddCharge(int amount)
        {
            CurrentCharge = System.Math.Min(CurrentCharge + amount, MaxCapacity);
        }

        /// <summary>
        /// Add flat +3 charge for a Sigil activation event.
        /// </summary>
        public void AddSigilActivationCharge()
        {
            AddCharge(3);
        }

        /// <summary>
        /// Reset meter to 0 after Dice Roll phase completes.
        /// </summary>
        public void Reset()
        {
            CurrentCharge = 0;
        }
    }
}
