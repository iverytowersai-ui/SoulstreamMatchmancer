using System;

namespace Matchmancer.Shop
{
    /// <summary>
    /// Pure C# gold-tracking wallet. One instance per save profile.
    /// MVP economy is single soft-currency ("Gold") — add a
    /// <c>CurrencyType</c> enum if premium gems are introduced later.
    ///
    /// Thread-safe? No — single-thread Unity.
    ///
    /// Persistence: snapshot / restore via <see cref="CreateSnapshot"/>
    /// and <see cref="LoadFromSnapshot"/>; wired into <see cref="Save.SaveData"/>.
    /// </summary>
    public class Wallet
    {
        public long Gold { get; private set; }

        /// <summary>Total gold earned across the lifetime of this save.</summary>
        public long LifetimeGoldEarned { get; private set; }

        /// <summary>Total gold spent across the lifetime of this save.</summary>
        public long LifetimeGoldSpent  { get; private set; }

        // ------------------------------------------------------------------
        // Events
        // ------------------------------------------------------------------

        /// <summary>(oldBalance, newBalance, delta) — positive delta = earn, negative = spend.</summary>
        public event Action<long, long, long> OnGoldChanged;

        // ------------------------------------------------------------------
        // Queries
        // ------------------------------------------------------------------

        public bool CanAfford(long price) => price >= 0 && Gold >= price;

        // ------------------------------------------------------------------
        // Mutations
        // ------------------------------------------------------------------

        /// <summary>
        /// Add gold to the wallet (level rewards, achievement payouts,
        /// gear sales). Clamps negative amounts to 0.
        /// </summary>
        public void Earn(long amount)
        {
            if (amount <= 0) return;

            long old = Gold;
            Gold += amount;
            if (Gold < old) Gold = long.MaxValue; // overflow guard
            LifetimeGoldEarned += amount;

            OnGoldChanged?.Invoke(old, Gold, amount);
        }

        /// <summary>
        /// Remove gold. Returns <c>true</c> on success. If the player
        /// can't afford it, the wallet is left unchanged and returns
        /// <c>false</c>. Price must be non-negative.
        /// </summary>
        public bool Spend(long price)
        {
            if (price < 0) return false;
            if (price == 0) return true; // free item
            if (Gold < price) return false;

            long old = Gold;
            Gold -= price;
            LifetimeGoldSpent += price;

            OnGoldChanged?.Invoke(old, Gold, -price);
            return true;
        }

        /// <summary>
        /// Force-set balance. Save/load and debug only — does not fire
        /// events and does not touch lifetime counters.
        /// </summary>
        public void SetBalanceRaw(long balance)
        {
            Gold = balance < 0 ? 0 : balance;
        }

        // ------------------------------------------------------------------
        // Snapshot / restore
        // ------------------------------------------------------------------

        public WalletSnapshot CreateSnapshot()
        {
            return new WalletSnapshot
            {
                Gold               = Gold,
                LifetimeGoldEarned = LifetimeGoldEarned,
                LifetimeGoldSpent  = LifetimeGoldSpent,
            };
        }

        public void LoadFromSnapshot(WalletSnapshot snapshot)
        {
            if (snapshot == null) return;
            Gold               = snapshot.Gold < 0 ? 0 : snapshot.Gold;
            LifetimeGoldEarned = snapshot.LifetimeGoldEarned;
            LifetimeGoldSpent  = snapshot.LifetimeGoldSpent;
        }
    }

    [Serializable]
    public class WalletSnapshot
    {
        public long Gold;
        public long LifetimeGoldEarned;
        public long LifetimeGoldSpent;
    }
}
