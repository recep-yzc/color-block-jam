using System;
using ColorBlockJam.Level;
using ColorBlockJam.LevelEditor;
using ColorBlockJam.LevelEditor.Authoring;
using NUnit.Framework;

namespace ColorBlockJam.Tests
{
    public sealed class GeneratorPresetsTests
    {
        [Test]
        public void ThePresetsCoverEveryDifficultySensibly()
        {
            var presets = TestAssets.LoadOnly<GeneratorPresets>();

            foreach (LevelDifficulty difficulty in Enum.GetValues(typeof(LevelDifficulty)))
            {
                Assert.IsTrue(presets.Covers(difficulty), $"No preset for {difficulty}.");
                var settings = presets.SettingsFor(difficulty);
                Assert.AreEqual(difficulty, settings.Difficulty);
                Assert.LessOrEqual(settings.MinBlocks, settings.MaxBlocks, $"{difficulty}: block range.");
                Assert.LessOrEqual(settings.MinRepositions, settings.MaxRepositions, $"{difficulty}: reposition range.");
                Assert.LessOrEqual(settings.MinMoves, settings.MaxMoves, $"{difficulty}: the moves cannot earn the badge.");
                Assert.IsNotEmpty(settings.ShapePools, $"{difficulty}: no shapes.");
            }
        }

        [Test]
        public void TheEasyPresetMakesALevel()
        {
            var settings = TestAssets.LoadOnly<GeneratorPresets>().SettingsFor(LevelDifficulty.Easy);

            Assert.IsNotNull(new LevelGenerator().Generate(settings, paletteSize: 10, seed: 42));
        }
    }
}
