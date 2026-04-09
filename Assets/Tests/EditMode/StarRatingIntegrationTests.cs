using NUnit.Framework;
using Matchmancer.Core;
using Matchmancer.Progression;

namespace Matchmancer.Tests
{
    /// <summary>
    /// Wires <see cref="StarRatingSystem"/> to a live
    /// <see cref="ProgressionState"/> and asserts the full results-to-save
    /// pipeline works end to end without a scene.
    /// </summary>
    [TestFixture]
    public class StarRatingIntegrationTests
    {
        private ProgressionState   _state;
        private StarRatingSystem   _system;
        private LevelConfig        _level;

        [SetUp]
        public void SetUp()
        {
            _state  = new ProgressionState();
            _system = new StarRatingSystem(_state);
            _level  = new LevelConfig
            {
                LevelNumber = 1,
                OneStar     = 500,
                TwoStar     = 1000,
                ThreeStar   = 1500,
                FourStar    = 2000,
                FiveStar    = 2500,
            };
        }

        private static BattleResult Make(int global, bool victory, int score, int combo = 0) =>
            new BattleResult
            {
                GlobalLevelIndex = global,
                Victory          = victory,
                FinalScore       = score,
                MaxComboAchieved = combo,
                HpRemaining      = 100,
                MaxHp            = 100,
            };

        // ==================================================================
        // Victory recording
        // ==================================================================

        [Test]
        public void Victory_RecordsStarsIntoProgression()
        {
            var stars = _system.ProcessBattleResult(Make(1, true, 1500, combo: 7), _level);
            Assert.AreEqual(3, stars.Stars);
            Assert.AreEqual(3, _state.GetStars(1));
            Assert.AreEqual(7, _state.GetBestCombo(1), 0.0001f);
            Assert.IsTrue(_state.IsCompleted(1));
        }

        [Test]
        public void Defeat_DoesNotRecordProgression()
        {
            var stars = _system.ProcessBattleResult(Make(1, false, 9999), _level);
            Assert.AreEqual(0, stars.Stars);
            Assert.AreEqual(0, _state.GetStars(1));
            Assert.IsFalse(_state.IsCompleted(1));
        }

        [Test]
        public void ReplayBetterScore_UpgradesStars()
        {
            _system.ProcessBattleResult(Make(1, true, 600), _level);   // 1 star
            Assert.AreEqual(1, _state.GetStars(1));

            _system.ProcessBattleResult(Make(1, true, 2600), _level);  // 5 star
            Assert.AreEqual(5, _state.GetStars(1));
            Assert.IsTrue(_state.HasFiveStar(1));
        }

        [Test]
        public void ReplayWorseScore_DoesNotDowngradeStars()
        {
            _system.ProcessBattleResult(Make(1, true, 2600), _level); // 5 star
            _system.ProcessBattleResult(Make(1, true, 600),  _level); // 1 star, should not clobber
            Assert.AreEqual(5, _state.GetStars(1));
            Assert.IsTrue(_state.HasFiveStar(1));
        }

        [Test]
        public void FiveStarFlag_StaysTrueAcrossReplays()
        {
            _system.ProcessBattleResult(Make(1, true, 2600), _level); // 5 star
            Assert.IsTrue(_state.HasFiveStar(1));

            _system.ProcessBattleResult(Make(1, true, 1200), _level); // 2 star replay
            Assert.IsTrue(_state.HasFiveStar(1),
                "FiveStar is a sticky achievement — lesser replays should not unset it.");
        }

        // ==================================================================
        // Event firing
        // ==================================================================

        [Test]
        public void OnBattleProcessed_FiresWithData()
        {
            BattleResult seenResult = default;
            StarResult   seenStar   = default;
            LevelConfig  seenLevel  = null;
            int calls = 0;

            _system.OnBattleProcessed += (r, s, l) =>
            {
                seenResult = r;
                seenStar   = s;
                seenLevel  = l;
                calls++;
            };

            var input = Make(1, true, 1500, combo: 4);
            _system.ProcessBattleResult(input, _level);

            Assert.AreEqual(1, calls);
            Assert.AreEqual(input, seenResult);
            Assert.AreEqual(3, seenStar.Stars);
            Assert.AreSame(_level, seenLevel);
        }

        [Test]
        public void OnBattleProcessed_AlsoFiresOnDefeat()
        {
            int calls = 0;
            _system.OnBattleProcessed += (_, _2, _3) => calls++;

            _system.ProcessBattleResult(Make(1, false, 0), _level);
            Assert.AreEqual(1, calls, "The UI still needs to show the Defeat screen, so the event must fire.");
        }

        // ==================================================================
        // Preview mode
        // ==================================================================

        [Test]
        public void Preview_DoesNotTouchProgressionOrFireEvents()
        {
            int calls = 0;
            _system.OnBattleProcessed += (_, _2, _3) => calls++;

            var stars = _system.Preview(Make(1, true, 2600), _level);

            Assert.AreEqual(5, stars.Stars);
            Assert.AreEqual(0, _state.GetStars(1));
            Assert.IsFalse(_state.IsCompleted(1));
            Assert.AreEqual(0, calls);
        }

        // ==================================================================
        // Null progression (test harness / replay preview)
        // ==================================================================

        [Test]
        public void NullProgression_DoesNotThrow()
        {
            var standalone = new StarRatingSystem(null);
            var stars = standalone.ProcessBattleResult(Make(1, true, 2000), _level);
            Assert.AreEqual(4, stars.Stars);
        }

        [Test]
        public void ProcessBattleResult_NullLevel_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(() =>
                _system.ProcessBattleResult(Make(1, true, 1000), null));
        }

        // ==================================================================
        // HpFraction / Flawless helpers
        // ==================================================================

        [Test]
        public void BattleResult_HpFraction_Computed()
        {
            var r = new BattleResult { HpRemaining = 30, MaxHp = 120 };
            Assert.AreEqual(0.25f, r.HpFraction, 0.0001f);
        }

        [Test]
        public void BattleResult_HpFraction_ClampsAtOne()
        {
            var r = new BattleResult { HpRemaining = 500, MaxHp = 100 };
            Assert.AreEqual(1f, r.HpFraction, 0.0001f);
        }

        [Test]
        public void BattleResult_Flawless_OnlyWhenFullHpAndZeroDamage()
        {
            var flawless = new BattleResult { HpRemaining = 100, MaxHp = 100, DamageTaken = 0 };
            Assert.IsTrue(flawless.Flawless);

            var chipped  = new BattleResult { HpRemaining = 100, MaxHp = 100, DamageTaken = 5 };
            Assert.IsFalse(chipped.Flawless);

            var hurt     = new BattleResult { HpRemaining = 99,  MaxHp = 100, DamageTaken = 0 };
            Assert.IsFalse(hurt.Flawless);
        }

        // ==================================================================
        // Cross-level progression interactions
        // ==================================================================

        [Test]
        public void CompletingMultipleLevels_IncrementsCounters()
        {
            _system.ProcessBattleResult(Make(1, true, 2600), _level);
            _system.ProcessBattleResult(Make(2, true, 1600), _level);
            _system.ProcessBattleResult(Make(3, true, 600),  _level);

            Assert.AreEqual(3, _state.TotalCompletedLevels());
            Assert.AreEqual(1, _state.TotalFiveStars());
        }
    }
}
