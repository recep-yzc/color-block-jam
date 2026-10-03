using ColorBlockJam.Gameplay.Logic;
using NUnit.Framework;

namespace ColorBlockJam.Tests
{
    public sealed class LevelTimerTests
    {
        [Test]
        public void AFreezeHoldsTheTimer()
        {
            var timer = new LevelTimer(10f);
            timer.Freeze(2f);

            timer.Tick(1.5f);
            Assert.AreEqual(10f, timer.Remaining, "Frozen time does not count.");

            timer.Tick(1f);
            Assert.AreEqual(9.5f, timer.Remaining, 0.0001f, "The rest of the step after the freeze counts.");
            Assert.IsFalse(timer.IsFrozen);
        }

        [Test]
        public void TimerStopsAtZero()
        {
            var timer = new LevelTimer(1f);

            Assert.IsFalse(timer.Tick(0.6f));
            Assert.IsTrue(timer.Tick(0.6f));
            Assert.AreEqual(0f, timer.Remaining);
            Assert.IsFalse(timer.Tick(1f), "It only reports running out once.");
        }

        [Test]
        public void AddedTimeLetsARunOutTimerGoOn()
        {
            var timer = new LevelTimer(1f);
            timer.Tick(1f);

            timer.Add(20f);

            Assert.IsFalse(timer.IsExpired);
            Assert.IsFalse(timer.Tick(5f));
            Assert.AreEqual(15f, timer.Remaining, 0.0001f);
            Assert.IsTrue(timer.Tick(15f), "It reports running out again when the added time is used up.");
        }
    }
}
