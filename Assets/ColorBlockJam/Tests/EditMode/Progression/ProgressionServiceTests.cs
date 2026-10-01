using ColorBlockJam.Progression;
using NUnit.Framework;

namespace ColorBlockJam.Tests
{
    public sealed class ProgressionServiceTests
    {
        [Test]
        public void NewPlayerStartsAtLevelOne()
        {
            Assert.AreEqual(1, new ProgressionService(new InMemoryStorage()).CurrentLevel);
        }

        [Test]
        public void ASetLevelIsSavedAndNeverBelowOne()
        {
            var storage = new InMemoryStorage();
            var progression = new ProgressionService(storage);

            progression.SetCurrentLevel(7);
            Assert.AreEqual(7, new ProgressionService(storage).CurrentLevel);

            progression.SetCurrentLevel(0);
            Assert.AreEqual(1, progression.CurrentLevel);
        }
    }
}
