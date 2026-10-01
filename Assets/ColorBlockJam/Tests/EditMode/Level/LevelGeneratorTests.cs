using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using NUnit.Framework;

namespace ColorBlockJam.Tests
{
    public sealed class LevelGeneratorTests
    {
        [TestCase(LevelDifficulty.Easy)]
        [TestCase(LevelDifficulty.Medium)]
        [TestCase(LevelDifficulty.Hard)]
        [TestCase(LevelDifficulty.SuperHard)]
        public void GeneratedLevelsAreSolvableWithinTheirDifficulty(LevelDifficulty difficulty)
        {
            var settings = GeneratorSettings.For(difficulty);

            var generated = new LevelGenerator().Generate(difficulty, paletteSize: 10, seed: 42);

            Assert.IsNotNull(generated);
            var solution = new BoardSolver().Solve(BoardFactory.Create(generated.Level), settings.SolveBudget);
            Assert.IsTrue(solution.IsSolved);
            Assert.That(solution.Repositions, Is.InRange(settings.MinRepositions, settings.MaxRepositions));
            Assert.AreEqual(difficulty, generated.Level.difficulty);
            Assert.AreEqual(difficulty, LevelRating.Rate(LevelRating.MovesToWin(generated.Level.blocks.Length, solution)),
                "The moves it takes earn the difficulty it was made for.");
        }

        [Test]
        public void TheSameSeedGivesTheSameLevel()
        {
            var generator = new LevelGenerator();

            var first = generator.Generate(LevelDifficulty.Medium, 10, seed: 7);
            var second = generator.Generate(LevelDifficulty.Medium, 10, seed: 7);

            Assert.AreEqual(LevelSerializer.ToJson(first.Level), LevelSerializer.ToJson(second.Level));
        }

        [Test]
        public void SolutionPlaysBackToAClearBoard()
        {
            var level = new LevelGenerator().Generate(LevelDifficulty.Medium, 10, seed: 3).Level;
            var board = BoardFactory.Create(level);
            var solution = new BoardSolver().Solve(board, 20000);

            foreach (var move in solution.Moves)
            {
                var block = board.Blocks[move.BlockId];
                if (move.Exits)
                {
                    Assert.AreEqual(move.Target, block.Position);
                    Assert.IsTrue(board.CanPassThrough(block, move.Target, move.Direction));
                    board.Clear(block);
                }
                else
                {
                    Assert.IsTrue(board.CanPlace(block, move.Target));
                    board.Move(block, move.Target);
                }
            }

            Assert.IsTrue(board.IsCleared);
        }
    }
}
