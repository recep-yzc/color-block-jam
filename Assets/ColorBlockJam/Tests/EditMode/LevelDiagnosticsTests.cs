using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using NUnit.Framework;
using UnityEditor;

namespace ColorBlockJam.Tests
{
    public sealed class LevelDiagnosticsTests
    {
        [Test]
        public void FindsColorsWithoutADoor()
        {
            var level = new LevelData
            {
                width = 4,
                height = 4,
                blocks = new[] { new BlockData { color = 2, x = 1, y = 1, cells = new[] { new CellData(0, 0) } } },
                doors = new[] { new DoorData { side = BoardSide.Top, start = 0, length = 1, color = 5 } }
            };

            var problems = LevelDiagnostics.Find(level);

            Assert.AreEqual(1, problems.Count);
            Assert.AreEqual(LevelProblemKind.ColorHasNoDoor, problems[0].Kind);
            Assert.AreEqual(2, problems[0].Color);
        }

        [Test]
        public void FindsBlocksTooWideForTheirDoor()
        {
            var level = new LevelData
            {
                width = 4,
                height = 4,
                blocks = new[] { new BlockData { color = 0, x = 0, y = 0, cells = new[] { new CellData(0, 0), new CellData(1, 0) } } },
                doors = new[] { new DoorData { side = BoardSide.Bottom, start = 1, length = 1, color = 0 } }
            };

            var problems = LevelDiagnostics.Find(level);

            Assert.AreEqual(1, problems.Count);
            Assert.AreEqual(LevelProblemKind.BlockFitsNoDoor, problems[0].Kind);
        }

        [Test]
        public void EveryCatalogLevelIsSound()
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(LevelCatalog)}");
            Assert.IsNotEmpty(guids);
            var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(AssetDatabase.GUIDToAssetPath(guids[0]));
            Assert.GreaterOrEqual(catalog.Count, 5);

            for (var number = 1; number <= catalog.Count; number++)
            {
                var level = catalog.Load(number);
                Assert.IsEmpty(LevelDiagnostics.Find(level), $"Level {number} has problems.");
                Assert.IsTrue(new BoardSolver().Solve(BoardFactory.Create(level), 20000).IsSolved, $"Level {number} is not solvable.");
            }
        }
    }
}
