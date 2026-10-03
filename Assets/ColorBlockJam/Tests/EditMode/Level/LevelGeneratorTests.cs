using System;
using System.Threading;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using ColorBlockJam.LevelEditor.Authoring;
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

            var generated = new LevelGenerator().Generate(settings, paletteSize: 10, seed: 42);

            Assert.IsNotNull(generated);
            Assert.That(generated.Solution.Repositions, Is.InRange(settings.MinRepositions, settings.MaxRepositions));
            Assert.AreEqual(difficulty, generated.Level.difficulty);
            Assert.AreEqual(difficulty, LevelRating.Rate(LevelRating.MovesToWin(generated.Level.blocks.Length, generated.Solution)),
                "The moves it takes earn the difficulty it was made for.");
            AssertPlaysBackToAClearBoard(generated);
        }

        [Test]
        public void GeneratedHolesAreAtLeastTwoByTwoAndKeepTheLevelSound()
        {
            var settings = GeneratorSettings.For(LevelDifficulty.Medium);
            settings.Width = 8;
            settings.Height = 9;
            settings.Holes = 2;

            var generated = new LevelGenerator().Generate(settings, paletteSize: 10, seed: 11);

            Assert.IsNotNull(generated);
            Assert.GreaterOrEqual(generated.Level.holes.Length, 2 * LevelDiagnostics.MinHoleSize * LevelDiagnostics.MinHoleSize);
            Assert.IsEmpty(LevelDiagnostics.Find(generated.Level), "Holes are big enough, off the edges and free of blocks.");
            AssertPlaysBackToAClearBoard(generated);
        }

        [Test]
        public void TheSameSeedGivesTheSameLevel()
        {
            var generator = new LevelGenerator();

            var first = generator.Generate(GeneratorSettings.For(LevelDifficulty.Medium), 10, seed: 7);
            var second = generator.Generate(GeneratorSettings.For(LevelDifficulty.Medium), 10, seed: 7);

            Assert.AreEqual(LevelSerializer.ToJson(first.Level), LevelSerializer.ToJson(second.Level));
        }

        [Test]
        public void CancellingStopsTheSearch()
        {
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            Assert.Throws<OperationCanceledException>(() =>
                new LevelGenerator().Generate(GeneratorSettings.For(LevelDifficulty.Hard), 10, seed: 3, cancellation.Token));
        }

        private static void AssertPlaysBackToAClearBoard(GeneratedLevel generated)
        {
            var board = BoardFactory.Create(generated.Level);
            foreach (var move in generated.Solution.Moves)
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
