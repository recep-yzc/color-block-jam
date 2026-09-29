using System.Numerics;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using NUnit.Framework;

namespace ColorBlockJam.Tests
{
    public sealed class BoardTests
    {
        private static readonly GridPoint[] Single = { new(0, 0) };
        private static readonly GridPoint[] Horizontal2 = { new(0, 0), new(1, 0) };

        [Test]
        public void BlocksCannotOverlap()
        {
            var a = new BoardBlock(0, 0, new GridPoint(0, 0), Horizontal2);
            var b = new BoardBlock(1, 1, new GridPoint(2, 0), Single);
            var board = new Board(4, 4, new[] { a, b }, new BoardDoor[0]);

            Assert.IsFalse(board.CanPlace(a, new GridPoint(1, 0)));
            Assert.IsTrue(board.CanPlace(a, new GridPoint(0, 1)));
        }

        [Test]
        public void BlockLeavesOnlyThroughAMatchingDoorItFits()
        {
            var block = new BoardBlock(0, 3, new GridPoint(1, 0), Horizontal2);
            var narrow = new BoardDoor(BoardSide.Bottom, 1, 1, 3);
            var board = new Board(4, 4, new[] { block }, new[] { narrow });

            Assert.IsFalse(board.CanPassThrough(block, block.Position, Direction.Down), "The door is narrower than the block.");

            var wide = new Board(4, 4, new[] { block }, new[] { new BoardDoor(BoardSide.Bottom, 1, 2, 3) });
            Assert.IsTrue(wide.CanPassThrough(block, block.Position, Direction.Down));

            var otherColor = new Board(4, 4, new[] { block }, new[] { new BoardDoor(BoardSide.Bottom, 1, 2, 4) });
            Assert.IsFalse(otherColor.CanPassThrough(block, block.Position, Direction.Down));
        }

        [Test]
        public void ShapeMustPassTheDoorWhole()
        {
            // An L whose foot fits the door but whose upright column has no door under it.
            var shape = new[] { new GridPoint(1, 0), new GridPoint(0, 1), new GridPoint(1, 1) };
            var block = new BoardBlock(0, 2, new GridPoint(0, 0), shape);
            var footOnly = new Board(4, 4, new[] { block }, new[] { new BoardDoor(BoardSide.Bottom, 1, 1, 2) });
            var both = new Board(4, 4, new[] { block }, new[] { new BoardDoor(BoardSide.Bottom, 0, 2, 2) });

            Assert.IsFalse(footOnly.CanPassThrough(block, block.Position, Direction.Down));
            Assert.IsTrue(both.CanPassThrough(block, block.Position, Direction.Down));
        }

        [Test]
        public void DragStopsAtObstacles()
        {
            var block = new BoardBlock(0, 0, new GridPoint(0, 0), Single);
            var wall = new BoardBlock(1, 1, new GridPoint(2, 0), Single);
            var board = new Board(4, 1, new[] { block, wall }, new BoardDoor[0]);
            var mover = new BlockDragMover(cornerAssist: 0.4f, assistRate: 1.5f);

            var reached = mover.Move(board, block, Vector2.Zero, new Vector2(3f, 0f));

            Assert.AreEqual(1f, reached.X, 0.02f);
        }

        [Test]
        public void DragRoundsCornersIntoGaps()
        {
            // A block slightly below a one-cell gap is eased up into it while moving right.
            var block = new BoardBlock(0, 0, new GridPoint(0, 1), Single);
            var below = new BoardBlock(1, 1, new GridPoint(2, 0), Single);
            var above = new BoardBlock(2, 1, new GridPoint(2, 2), Single);
            var board = new Board(4, 3, new[] { block, below, above }, new BoardDoor[0]);
            var mover = new BlockDragMover(cornerAssist: 0.4f, assistRate: 1.5f);

            var reached = mover.Move(board, block, new Vector2(0f, 0.75f), new Vector2(3f, 0.75f));

            // Without easing it would stop at x = 1 against the lower block.
            Assert.AreEqual(3f, reached.X, 0.02f);
            Assert.AreEqual(0.75f, reached.Y, 0.02f, "Past the gap it follows the finger again.");
        }

        [Test]
        public void SolverClearsASolvableBoard()
        {
            var red = new BoardBlock(0, 0, new GridPoint(0, 0), Single);
            var blue = new BoardBlock(1, 1, new GridPoint(1, 0), Single);
            var doors = new[] { new BoardDoor(BoardSide.Left, 0, 1, 0), new BoardDoor(BoardSide.Right, 0, 1, 1) };
            var board = new Board(3, 1, new[] { red, blue }, doors);

            var result = new BoardSolver().Solve(board, 1000);

            Assert.IsTrue(result.IsSolved);
            Assert.AreEqual(2, result.Moves.Count);
            Assert.AreEqual(2, board.RemainingBlocks, "The solver leaves the board as it was.");
        }

        [Test]
        public void SolverReportsStuckBoards()
        {
            // The only door is for a color that is not on the board.
            var block = new BoardBlock(0, 0, new GridPoint(0, 0), Single);
            var board = new Board(2, 2, new[] { block }, new[] { new BoardDoor(BoardSide.Top, 0, 2, 5) });

            var result = new BoardSolver().Solve(board, 1000);

            Assert.IsTrue(result.IsStuck);
        }

        [Test]
        public void TimerStopsAtZero()
        {
            var timer = new LevelTimer(1f);

            Assert.IsFalse(timer.Tick(0.6f));
            Assert.IsTrue(timer.Tick(0.6f));
            Assert.AreEqual(0f, timer.Remaining);
            Assert.IsFalse(timer.Tick(1f), "It only reports running out once.");
        }
    }
}
