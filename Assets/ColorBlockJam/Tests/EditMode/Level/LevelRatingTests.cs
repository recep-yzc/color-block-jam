using System;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using NUnit.Framework;

namespace ColorBlockJam.Tests
{
    public sealed class LevelRatingTests
    {
        [Test]
        public void MovesToWinAreTheBlocksToSendOutPlusThoseInTheWay()
        {
            var solution = new SolveResult(true, false, Array.Empty<SolverMove>(), 3);

            Assert.AreEqual(8, LevelRating.MovesToWin(5, solution));
        }

        [TestCase(1, LevelDifficulty.Easy)]
        [TestCase(8, LevelDifficulty.Easy)]
        [TestCase(9, LevelDifficulty.Medium)]
        [TestCase(12, LevelDifficulty.Medium)]
        [TestCase(13, LevelDifficulty.Hard)]
        [TestCase(16, LevelDifficulty.Hard)]
        [TestCase(17, LevelDifficulty.SuperHard)]
        [TestCase(30, LevelDifficulty.SuperHard)]
        public void TheBadgeFollowsTheMovesToWin(int moves, LevelDifficulty badge)
        {
            Assert.AreEqual(badge, LevelRating.Rate(moves));
        }
    }
}
