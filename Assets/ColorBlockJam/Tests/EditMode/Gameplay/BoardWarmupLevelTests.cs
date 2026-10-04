using System.Linq;
using ColorBlockJam.Gameplay;
using ColorBlockJam.Level;
using ColorBlockJam.LevelEditor.Authoring;
using NUnit.Framework;

namespace ColorBlockJam.Tests
{
    public sealed class BoardWarmupLevelTests
    {
        [Test]
        public void TheWarmupBoardIsSoundAndHasEveryPartTheBoardDraws()
        {
            var level = BoardWarmupLevel.Create();

            CollectionAssert.IsEmpty(LevelDiagnostics.Find(level));
            Assert.IsNotEmpty(level.doors, "No door to warm up.");
            Assert.IsTrue(level.blocks.Any(block => block.axis != BlockAxis.Free), "No arrow block to warm up.");
            Assert.IsTrue(level.blocks.Any(block => block.ice > 0), "No ice to warm up.");
        }
    }
}
