using NUnit.Framework;
using Matchmancer.Achievements;
using Matchmancer.Core;
using Matchmancer.Progression;

namespace Matchmancer.Tests
{
    /// <summary>
    /// Integration tests that wire <see cref="AchievementTracker"/> +
    /// <see cref="TitleSystem"/> + <see cref="StarRatingSystem"/> +
    /// <see cref="ProgressionState"/> together and drive scripted battle
    /// results through the closed pipeline. Mirrors how
    /// <see cref="AchievementController"/> wires things in a real scene.
    /// </summary>
    [TestFixture]
    public class AchievementIntegrationTests
    {
        private ProgressionState  _progression;
        private StarRatingSystem  _stars;
        private AchievementTracker _tracker;
        private TitleSystem        _titles;
        private LevelConfig        _level;

        [SetUp]
        public void SetUp()
        {
            _progression = new ProgressionState();
            _stars       = new StarRatingSystem(_progression);
            _tracker     = new AchievementTracker();
            _titles      = new TitleSystem();
            _titles.WireToTracker(_tracker);

            _level = new LevelConfig
            {
                LevelNumber = 1,
                OneStar     = 500,
                TwoStar     = 1000,
                ThreeStar   = 1500,
                FourStar    = 2000,
                FiveStar    = 2500,
            };

            // Hook the same translation logic as AchievementController.
            _stars.OnBattleProcessed += (result, star, level) =>
            {
                if (result.DamageDealt > 0)
                    _tracker.IncrementStat(AchievementStatKey.TotalDamageDealt, result.DamageDealt);
                if (result.DamageTaken > 0)
                    _tracker.IncrementStat(AchievementStatKey.TotalDamageTaken, result.DamageTaken);
                if (result.MaxComboAchieved > 0)
                    _tracker.UpdateMaxStat(AchievementStatKey.MaxComboEver, result.MaxComboAchieved);

                if (!result.Victory) return;

                _tracker.IncrementStat(AchievementStatKey.LevelsCompleted);
                _tracker.IncrementStat(AchievementStatKey.EnemiesDefeated);
                if (star.Stars > 0)
                    _tracker.IncrementStat(AchievementStatKey.TotalStarsEarned, star.Stars);
                if (star.FiveStar)
                    _tracker.IncrementStat(AchievementStatKey.FiveStarsEarned);
                if (result.Flawless)
                    _tracker.IncrementStat(AchievementStatKey.FlawlessBattles);
            };
        }

        private static BattleResult Battle(
            int globalIndex, bool victory, int score,
            int combo = 0, int dmgDealt = 0, int dmgTaken = 0,
            int hp = 100, int maxHp = 100)
        {
            return new BattleResult
            {
                GlobalLevelIndex = globalIndex,
                Victory          = victory,
                FinalScore       = score,
                MaxComboAchieved = combo,
                DamageDealt      = dmgDealt,
                DamageTaken      = dmgTaken,
                HpRemaining      = hp,
                MaxHp            = maxHp,
            };
        }

        // ==================================================================
        // Cumulative stat translation
        // ==================================================================

        [Test]
        public void Victory_IncrementsLevelAndKillCounters()
        {
            _stars.ProcessBattleResult(Battle(1, true, 1500), _level);
            Assert.AreEqual(1L, _tracker.GetStat(AchievementStatKey.LevelsCompleted));
            Assert.AreEqual(1L, _tracker.GetStat(AchievementStatKey.EnemiesDefeated));
            Assert.AreEqual(3L, _tracker.GetStat(AchievementStatKey.TotalStarsEarned));
        }

        [Test]
        public void Defeat_IncrementsDamageStatsButNotLevelStats()
        {
            _stars.ProcessBattleResult(
                Battle(1, false, 100, dmgDealt: 200, dmgTaken: 999),
                _level);

            Assert.AreEqual(0L, _tracker.GetStat(AchievementStatKey.LevelsCompleted));
            Assert.AreEqual(0L, _tracker.GetStat(AchievementStatKey.TotalStarsEarned));
            Assert.AreEqual(200L, _tracker.GetStat(AchievementStatKey.TotalDamageDealt));
            Assert.AreEqual(999L, _tracker.GetStat(AchievementStatKey.TotalDamageTaken));
        }

        [Test]
        public void FiveStarVictory_IncrementsFiveStarCounter()
        {
            _stars.ProcessBattleResult(Battle(1, true, 2600), _level);
            Assert.AreEqual(1L, _tracker.GetStat(AchievementStatKey.FiveStarsEarned));
        }

        [Test]
        public void FlawlessVictory_IncrementsFlawlessCounter()
        {
            _stars.ProcessBattleResult(
                Battle(1, true, 1500, hp: 100, maxHp: 100, dmgTaken: 0),
                _level);
            Assert.AreEqual(1L, _tracker.GetStat(AchievementStatKey.FlawlessBattles));
        }

        [Test]
        public void NonFlawlessVictory_DoesNotIncrementFlawless()
        {
            _stars.ProcessBattleResult(
                Battle(1, true, 1500, hp: 100, maxHp: 100, dmgTaken: 5),
                _level);
            Assert.AreEqual(0L, _tracker.GetStat(AchievementStatKey.FlawlessBattles));
        }

        [Test]
        public void MaxCombo_TracksHighWaterAcrossBattles()
        {
            _stars.ProcessBattleResult(Battle(1, true, 1500, combo: 5), _level);
            _stars.ProcessBattleResult(Battle(2, true, 1500, combo: 12), _level);
            _stars.ProcessBattleResult(Battle(3, true, 1500, combo: 9), _level);

            Assert.AreEqual(12L, _tracker.GetStat(AchievementStatKey.MaxComboEver));
        }

        // ==================================================================
        // Achievement → title chain
        // ==================================================================

        [Test]
        public void TenLevelsAchievement_AutoUnlocksRewardTitle()
        {
            _titles.RegisterTitle(new TitleDefinition
            {
                Id = "veteran", DisplayName = "Veteran",
                SourceAchievementId = "ten_levels",
            });
            _tracker.RegisterAchievement(new AchievementDefinition
            {
                Id            = "ten_levels",
                StatKey       = AchievementStatKey.LevelsCompleted,
                TargetValue   = 10,
                TitleIdReward = "veteran",
            });

            for (int i = 1; i <= 10; i++)
                _stars.ProcessBattleResult(Battle(i, true, 1500), _level);

            Assert.IsTrue(_tracker.IsUnlocked("ten_levels"));
            Assert.IsTrue(_titles.HasTitle("veteran"));
        }

        [Test]
        public void FiveStarAchievement_GrantsTitleAfterFirstFiveStar()
        {
            _titles.RegisterTitle(new TitleDefinition
            {
                Id = "perfectionist", DisplayName = "Perfectionist",
                SourceAchievementId = "first_five_star",
            });
            _tracker.RegisterAchievement(new AchievementDefinition
            {
                Id            = "first_five_star",
                StatKey       = AchievementStatKey.FiveStarsEarned,
                TargetValue   = 1,
                TitleIdReward = "perfectionist",
            });

            // Win some 3-star levels first — should NOT unlock.
            _stars.ProcessBattleResult(Battle(1, true, 1500), _level);
            _stars.ProcessBattleResult(Battle(2, true, 1500), _level);
            Assert.IsFalse(_titles.HasTitle("perfectionist"));

            // 5-star — should unlock title via chain.
            _stars.ProcessBattleResult(Battle(3, true, 2600), _level);
            Assert.IsTrue(_titles.HasTitle("perfectionist"));
        }

        [Test]
        public void DamageMilestone_FiresFromLossesToo()
        {
            _tracker.RegisterAchievement(new AchievementDefinition
            {
                Id          = "thousand_damage",
                StatKey     = AchievementStatKey.TotalDamageDealt,
                TargetValue = 1000,
            });

            _stars.ProcessBattleResult(Battle(1, false, 0, dmgDealt: 600), _level);
            Assert.IsFalse(_tracker.IsUnlocked("thousand_damage"));

            _stars.ProcessBattleResult(Battle(1, true, 1500, dmgDealt: 500), _level);
            Assert.IsTrue(_tracker.IsUnlocked("thousand_damage"));
        }

        // ==================================================================
        // Equip flow over the integration
        // ==================================================================

        [Test]
        public void EquipUnlockedTitle_PersistsThroughMoreUnlocks()
        {
            _titles.RegisterTitle(new TitleDefinition { Id = "first_blood", SourceAchievementId = "kill_one" });
            _titles.RegisterTitle(new TitleDefinition { Id = "veteran",     SourceAchievementId = "kill_ten" });

            _tracker.RegisterAchievement(new AchievementDefinition
            {
                Id = "kill_one", StatKey = AchievementStatKey.EnemiesDefeated, TargetValue = 1, TitleIdReward = "first_blood",
            });
            _tracker.RegisterAchievement(new AchievementDefinition
            {
                Id = "kill_ten", StatKey = AchievementStatKey.EnemiesDefeated, TargetValue = 10, TitleIdReward = "veteran",
            });

            _stars.ProcessBattleResult(Battle(1, true, 1500), _level);
            _titles.Equip("first_blood");
            Assert.AreEqual("first_blood", _titles.EquippedId);

            for (int i = 2; i <= 10; i++)
                _stars.ProcessBattleResult(Battle(i, true, 1500), _level);

            Assert.IsTrue(_titles.HasTitle("veteran"));
            Assert.AreEqual("first_blood", _titles.EquippedId,
                "Equipped title should not change when a new title unlocks.");
        }
    }
}
