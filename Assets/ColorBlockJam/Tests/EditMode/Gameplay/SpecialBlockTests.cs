using System;
using System.Numerics;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using ColorBlockJam.LevelEditor.Authoring;
using NUnit.Framework;

namespace ColorBlockJam.Tests
{
    public sealed class SpecialBlockTests
    {
        private static readonly GridPoint[] Single = { new(0, 0) };

        [Test]
        public void AnArrowBlockIsDraggedAlongItsAxisOnly()
        {
            var block = new BoardBlock(0, 0, new GridPoint(0, 1), Single, BlockAxis.Horizontal);
            var board = new Board(4, 4, new[] { block }, Array.Empty<BoardDoor>());

            var reached = new BlockDragMover(0.3f).Move(board, block, new Vector2(0f, 1f), new Vector2(3f, 3f));

            Assert.AreEqual(3f, reached.X, 0.01f);
            Assert.AreEqual(1f, reached.Y, 0.0001f, "It never leaves its row.");
            Assert.AreEqual(new GridPoint(3, 1), BlockPlacement.Snap(board, block, new Vector2(2.8f, 1.4f)));
        }

        [Test]
        public void AnArrowBlockLeavesOnlyThroughADoorOnItsAxis()
        {
            var block = new BoardBlock(0, 2, new GridPoint(1, 0), Single, BlockAxis.Horizontal);
            var doors = new[] { new BoardDoor(BoardSide.Bottom, 1, 1, 2), new BoardDoor(BoardSide.Right, 0, 1, 2) };
            var board = new Board(4, 4, new[] { block }, doors);

            Assert.IsFalse(board.CanPassThrough(block, block.Position, Direction.Down), "The bottom door is across its axis.");
            Assert.IsTrue(board.CanPassThrough(block, block.Position, Direction.Right));
        }

        [Test]
        public void TheSolverKeepsArrowBlocksOnTheirAxis()
        {
            var block = new BoardBlock(0, 0, new GridPoint(1, 1), Single, BlockAxis.Vertical);
            var board = new Board(4, 4, new[] { block }, new[] { new BoardDoor(BoardSide.Right, 0, 4, 0) });

            Assert.IsTrue(new BoardSolver().Solve(board, 1000).IsStuck);
        }

        [Test]
        public void IceThawsOnceEnoughBlocksHaveLeft()
        {
            var frozen = new BoardBlock(0, 0, new GridPoint(0, 0), Single, ice: 1);
            var other = new BoardBlock(1, 1, new GridPoint(2, 0), Single);
            var board = new Board(3, 1, new[] { frozen, other }, Array.Empty<BoardDoor>());

            Assert.IsTrue(board.IsFrozen(frozen));
            Assert.AreEqual(1, board.IceLeft(frozen));

            board.Clear(other);

            Assert.IsFalse(board.IsFrozen(frozen));
            Assert.AreEqual(0, board.IceLeft(frozen));
        }

        [Test]
        public void TheSolverWaitsForIceToMelt()
        {
            var frozen = new BoardBlock(0, 0, new GridPoint(0, 0), Single, ice: 1);
            var other = new BoardBlock(1, 1, new GridPoint(2, 0), Single);
            var doors = new[] { new BoardDoor(BoardSide.Left, 0, 1, 0), new BoardDoor(BoardSide.Right, 0, 1, 1) };
            var board = new Board(3, 1, new[] { frozen, other }, doors);

            var result = new BoardSolver().Solve(board, 1000);

            Assert.IsTrue(result.IsSolved);
            Assert.AreEqual(1, result.Moves[0].BlockId, "The free block has to leave first.");
            Assert.IsTrue(board.IsFrozen(frozen), "The solver leaves the board as it was.");
        }

        [Test]
        public void TheArrowLiesAlongTheLongestRunOnTheAxis()
        {
            var t = new[] { new GridPoint(0, 1), new GridPoint(1, 1), new GridPoint(2, 1), new GridPoint(1, 0) };
            var onTheBar = BlockMarks.FindArrow(t, BlockAxis.Horizontal);
            Assert.AreEqual(3, onTheBar.Length);
            Assert.AreEqual(1.5f, onTheBar.CenterX);
            Assert.AreEqual(1.5f, onTheBar.CenterY, "A T gets its arrow on its bar.");

            var square = new[] { new GridPoint(0, 0), new GridPoint(1, 0), new GridPoint(0, 1), new GridPoint(1, 1) };
            var inTheMiddle = BlockMarks.FindArrow(square, BlockAxis.Vertical);
            Assert.AreEqual(2, inTheMiddle.Length);
            Assert.AreEqual(1f, inTheMiddle.CenterX, "A square gets its arrow between its columns.");
            Assert.AreEqual(1f, inTheMiddle.CenterY);

            Assert.AreEqual(0, BlockMarks.FindArrow(square, BlockAxis.Free).Length, "A free block has no arrow.");
        }

        [Test]
        public void TheIceCountSitsOnTheBlock()
        {
            var l = new[] { new GridPoint(0, 0), new GridPoint(0, 1), new GridPoint(0, 2), new GridPoint(1, 0), new GridPoint(2, 0) };

            var cell = BlockMarks.FindIceCell(l);
            Assert.That(new[] { new GridPoint(0, 1), new GridPoint(1, 0) }, Does.Contain(cell));
        }

        [Test]
        public void DiagnosticsFindIceThatCanNeverMelt()
        {
            var level = new LevelData
            {
                width = 3,
                height = 3,
                blocks = new[] { new BlockData { color = 0, x = 1, y = 1, cells = new[] { new CellData(0, 0) }, ice = 1 } },
                doors = new[] { new DoorData { side = BoardSide.Left, start = 1, length = 1, color = 0 } }
            };

            var problems = LevelDiagnostics.Find(level);

            Assert.AreEqual(1, problems.Count);
            Assert.AreEqual(LevelProblemKind.IceNeverMelts, problems[0].Kind);
        }

        [Test]
        public void DiagnosticsFindArrowBlocksWithNoDoorOnTheirAxis()
        {
            var level = new LevelData
            {
                width = 4,
                height = 4,
                blocks = new[] { new BlockData { color = 0, x = 1, y = 1, cells = new[] { new CellData(0, 0) }, axis = BlockAxis.Vertical } },
                doors = new[] { new DoorData { side = BoardSide.Right, start = 0, length = 4, color = 0 } }
            };

            var problems = LevelDiagnostics.Find(level);

            Assert.AreEqual(1, problems.Count);
            Assert.AreEqual(LevelProblemKind.BlockFitsNoDoor, problems[0].Kind);
        }
    }
}
