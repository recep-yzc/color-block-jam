using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using NUnit.Framework;
using UnityEditor;

namespace ColorBlockJam.Tests
{
    public sealed class LevelDiagnosticsTests
    {
        private const int StuckBudget = 30000;
        private const int Tutorials = 3;

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
        public void FindsBlocksOnARemovedCell()
        {
            var level = new LevelData
            {
                width = 4,
                height = 4,
                blocks = new[] { new BlockData { color = 0, x = 1, y = 1, cells = new[] { new CellData(0, 0) } } },
                doors = new[] { new DoorData { side = BoardSide.Top, start = 1, length = 1, color = 0 } },
                holes = new[] { new CellData(1, 1) }
            };

            var problems = LevelDiagnostics.Find(level);

            Assert.AreEqual(1, problems.Count);
            Assert.AreEqual(LevelProblemKind.BlockOnHole, problems[0].Kind);
        }

        [Test]
        public void FindsDoorsThatOpenOntoARemovedCell()
        {
            var level = new LevelData
            {
                width = 4,
                height = 4,
                blocks = new[] { new BlockData { color = 0, x = 0, y = 0, cells = new[] { new CellData(0, 0) } } },
                doors = new[] { new DoorData { side = BoardSide.Bottom, start = 1, length = 1, color = 0 } },
                holes = new[] { new CellData(1, 0) }
            };

            var kinds = LevelDiagnostics.Find(level).ConvertAll(problem => problem.Kind);

            Assert.Contains(LevelProblemKind.DoorFacesHole, kinds);
            Assert.Contains(LevelProblemKind.BlockFitsNoDoor, kinds, "The only door is behind the hole.");
        }

        [Test]
        public void EveryCatalogLevelIsSoundAndEarnsItsBadge()
        {
            var catalog = LoadCatalog();
            Assert.GreaterOrEqual(catalog.Count, 100);

            for (var number = 1; number <= catalog.Count; number++)
            {
                var level = catalog.Load(number);
                Assert.IsEmpty(LevelDiagnostics.Find(level), $"Level {number} has problems.");
                var solution = new BoardSolver().Solve(BoardFactory.Create(level), StuckBudget);
                Assert.IsTrue(solution.IsSolved, $"Level {number} is not solved within the budget the game checks it with.");
                Assert.AreEqual(level.difficulty, LevelRating.Rate(LevelRating.MovesToWin(level.blocks.Length, solution)),
                    $"Level {number} wears a badge its moves do not earn.");
            }
        }

        [Test]
        public void TheFirstLevelsAreTutorials()
        {
            var catalog = LoadCatalog();

            for (var number = 1; number <= Tutorials; number++)
            {
                var level = catalog.Load(number);
                var solution = new BoardSolver().Solve(BoardFactory.Create(level), StuckBudget);
                Assert.LessOrEqual(level.blocks.Length, number, $"Tutorial {number} has too many blocks.");
                Assert.AreEqual(0, solution.Repositions, $"Tutorial {number} asks to move a block out of the way.");
            }
        }

        [Test]
        public void TheLevelsGetHarderOverTime()
        {
            var catalog = LoadCatalog();

            Assert.Less(AverageMoves(catalog, 1, 20), AverageMoves(catalog, 41, 60));
            Assert.Less(AverageMoves(catalog, 41, 60), AverageMoves(catalog, 81, 100));
        }

        private static double AverageMoves(LevelCatalog catalog, int first, int last)
        {
            var total = 0;
            for (var number = first; number <= last; number++)
            {
                var level = catalog.Load(number);
                total += LevelRating.MovesToWin(level.blocks.Length, new BoardSolver().Solve(BoardFactory.Create(level), StuckBudget));
            }

            return total / (double)(last - first + 1);
        }

        private static LevelCatalog LoadCatalog()
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(LevelCatalog)}");
            Assert.AreEqual(1, guids.Length);
            return AssetDatabase.LoadAssetAtPath<LevelCatalog>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
    }
}
