using System;
using NUnit.Framework;
using Matchmancer.Combat;
using Matchmancer.Core;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class CombatRoleMapTests
    {
        [Test]
        public void SoulstreamShard_MapsToDamage()
        {
            Assert.AreEqual(CombatRole.Damage, CombatRoleMap.GetRole(TileType.SoulstreamShard));
        }

        [Test]
        public void PortRune_MapsToEnergy()
        {
            Assert.AreEqual(CombatRole.Energy, CombatRoleMap.GetRole(TileType.PortRune));
        }

        [Test]
        public void CovenSeal_MapsToDefense()
        {
            Assert.AreEqual(CombatRole.Defense, CombatRoleMap.GetRole(TileType.CovenSeal));
        }

        [Test]
        public void OzoneMark_MapsToDebuff()
        {
            Assert.AreEqual(CombatRole.Debuff, CombatRoleMap.GetRole(TileType.OzoneMark));
        }

        [Test]
        public void WitchbreedThorn_MapsToBreak()
        {
            Assert.AreEqual(CombatRole.Break, CombatRoleMap.GetRole(TileType.WitchbreedThorn));
        }

        [Test]
        public void PetshaCharm_MapsToLuck()
        {
            Assert.AreEqual(CombatRole.Luck, CombatRoleMap.GetRole(TileType.PetshaCharm));
        }

        [Test]
        public void GetRole_None_Throws()
        {
            Assert.Throws<ArgumentException>(() => CombatRoleMap.GetRole(TileType.None));
        }

        [Test]
        public void TryGetRole_None_ReturnsFalse()
        {
            bool ok = CombatRoleMap.TryGetRole(TileType.None, out CombatRole role);
            Assert.IsFalse(ok);
        }

        [Test]
        public void TryGetRole_AllTiles_ReturnTrue()
        {
            foreach (TileType t in Enum.GetValues(typeof(TileType)))
            {
                if (t == TileType.None) continue;
                Assert.IsTrue(
                    CombatRoleMap.TryGetRole(t, out _),
                    $"TryGetRole returned false for {t}");
            }
        }
    }
}
