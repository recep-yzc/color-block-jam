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
    }
}
