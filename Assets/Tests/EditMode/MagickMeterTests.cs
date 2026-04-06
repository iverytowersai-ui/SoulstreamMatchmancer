using NUnit.Framework;
using Matchmancer.Meter;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class MagickMeterTests
    {
        private MagickMeter _meter;

        [SetUp]
        public void SetUp()
        {
            _meter = new MagickMeter(10);
        }

        [Test]
        public void InitialState_ZeroCharge_NotFull()
        {
            Assert.AreEqual(0, _meter.CurrentCharge);
            Assert.AreEqual(10, _meter.MaxCapacity);
            Assert.IsFalse(_meter.IsFull);
        }

        [Test]
        public void AddCharge_IncreasesCharge()
        {
            _meter.AddCharge(3);
            Assert.AreEqual(3, _meter.CurrentCharge);
        }

        [Test]
        public void AddCharge_ClampsAtMax_NoOverflow()
        {
            _meter.AddCharge(7);
            _meter.AddCharge(5);
            Assert.AreEqual(10, _meter.CurrentCharge);
        }

        [Test]
        public void IsFull_AtCapacity_ReturnsTrue()
        {
            _meter.AddCharge(10);
            Assert.IsTrue(_meter.IsFull);
        }

        [Test]
        public void AddSigilActivationCharge_Adds3()
        {
            _meter.AddSigilActivationCharge();
            Assert.AreEqual(3, _meter.CurrentCharge);
        }

        [Test]
        public void Reset_SetsToZero()
        {
            _meter.AddCharge(8);
            _meter.Reset();
            Assert.AreEqual(0, _meter.CurrentCharge);
            Assert.IsFalse(_meter.IsFull);
        }

        [Test]
        public void FullCycle_ChargeToFull_Reset_ChargeAgain()
        {
            _meter.AddCharge(10);
            Assert.IsTrue(_meter.IsFull);
            _meter.Reset();
            Assert.AreEqual(0, _meter.CurrentCharge);
            _meter.AddCharge(5);
            Assert.AreEqual(5, _meter.CurrentCharge);
        }
    }
}
