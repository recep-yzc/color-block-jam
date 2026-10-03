using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using ColorBlockJam.LevelEditor.Authoring;
using NUnit.Framework;

namespace ColorBlockJam.Tests
{
    public sealed class LevelDiagnosticsTests
    {
        private const int Tutorials = 3;

        private LevelCatalog catalog;
        private SolveResult[] solutions;

        [OneTimeSetUp]
        public void LoadCatalog()
        {
            catalog = TestAssets.LoadOnly<LevelCatalog>();
            solutions = new SolveResult[catalog.Count + 1];
        }

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
                holes = new[] { new CellData(1, 1), new CellData(2, 1), new CellData(1, 2), new CellData(2, 2) }
            };

            var problems = LevelDiagnostics.Find(level);

            Assert.AreEqual(1, problems.Count);
            Assert.AreEqual(LevelProblemKind.BlockOnHole, problems[0].Kind);
        }

        [Test]
        public void FindsHolesSmallerThanTwoByTwo()
        {
            var level = new LevelData
            {
                width = 5,
                height = 5,
                blocks = new[] { new BlockData { color = 0, x = 0, y = 0, cells = new[] { new CellData(0, 0) } } },
                doors = new[] { new DoorData { side = BoardSide.Bottom, start = 0, length = 1, color = 0 } },
                holes = new[] { new CellData(2, 2), new CellData(3, 2) }
            };

            var problems = LevelDiagnostics.Find(level);

            Assert.AreEqual(1, problems.Count);
            Assert.AreEqual(LevelProblemKind.HoleTooSmall, problems[0].Kind);

            level.holes = new[] { new CellData(2, 2), new CellData(3, 2), new CellData(2, 3), new CellData(3, 3) };
            Assert.IsEmpty(LevelDiagnostics.Find(level), "A 2x2 hole is fine.");
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
                holes = new[] { new CellData(1, 0), new CellData(2, 0), new CellData(1, 1), new CellData(2, 1) }
            };

            var kinds = LevelDiagnostics.Find(level).ConvertAll(problem => problem.Kind);

            Assert.Contains(LevelProblemKind.DoorFacesHole, kinds);
            Assert.Contains(LevelProblemKind.BlockFitsNoDoor, kinds, "The only door is behind the hole.");
        }

        [Test]
        public void EveryCatalogLevelIsSoundAndEarnsItsBadge()
        {
            Assert.GreaterOrEqual(catalog.Count, 50);

            for (var number = 1; number <= catalog.Count; number++)
            {
                var level = catalog.Load(number);
                Assert.IsEmpty(LevelDiagnostics.Find(level), $"Level {number} has problems.");
                var solution = SolutionOf(number);
                Assert.IsTrue(solution.IsSolved, $"Level {number} is not solved within the budget the game checks it with.");
                Assert.AreEqual(level.difficulty, LevelRating.Rate(LevelRating.MovesToWin(level.blocks.Length, solution)),
                    $"Level {number} wears a badge its moves do not earn.");
            }
        }

        [Test]
        public void TheFirstLevelsAreTutorials()
        {
            for (var number = 1; number <= Tutorials; number++)
            {
                var level = catalog.Load(number);
                var solution = SolutionOf(number);
                Assert.LessOrEqual(level.blocks.Length, number, $"Tutorial {number} has too many blocks.");
                Assert.AreEqual(0, solution.Repositions, $"Tutorial {number} asks to move a block out of the way.");
            }
        }

        [Test]
        public void TheLevelsGetHarderOverTime()
        {
            var middle = AverageMoves(18, 32);

            Assert.Less(AverageMoves(1, 15), middle);
            Assert.Less(middle, AverageMoves(36, 50));
        }

        [Test]
        public void ManyLevelsHaveHoles()
        {
            var withHoles = 0;
            for (var number = 1; number <= catalog.Count; number++)
            {
                if (catalog.Load(number).holes.Length > 0)
                {
                    withHoles++;
                }
            }

            Assert.GreaterOrEqual(withHoles, catalog.Count / 5);
        }

        private double AverageMoves(int first, int last)
        {
            var total = 0;
            for (var number = first; number <= last; number++)
            {
                total += LevelRating.MovesToWin(catalog.Load(number).blocks.Length, SolutionOf(number));
            }

            return total / (double)(last - first + 1);
        }

        private SolveResult SolutionOf(int number)
        {
            return solutions[number] ??= new BoardSolver().Solve(BoardFactory.Create(catalog.Load(number)), BoardSolver.DefaultBudget);
        }
    }
}
