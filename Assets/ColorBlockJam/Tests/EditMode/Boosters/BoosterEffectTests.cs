using ColorBlockJam.Boosters;
using ColorBlockJam.Gameplay.Logic;
using NUnit.Framework;

namespace ColorBlockJam.Tests
{
    public sealed class BoosterEffectTests
    {
        [Test]
        public void TheFreezeHoldsTheTimerAndRunsOneAtATime()
        {
            var timer = new LevelTimer(30f);
            var freeze = new FreezeEffect(timer, 5f);
            Assert.IsTrue(freeze.IsReady);

            freeze.Apply();

            Assert.IsFalse(freeze.IsReady, "A second freeze waits for the first one to end.");
            timer.Tick(5f);
            Assert.AreEqual(30f, timer.Remaining, 0.0001f);
            Assert.IsTrue(freeze.IsReady);
        }
    }
}
