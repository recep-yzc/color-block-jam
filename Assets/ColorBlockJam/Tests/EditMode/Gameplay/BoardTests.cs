using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using NUnit.Framework;

namespace ColorBlockJam.Tests
{
    public sealed class BoardTests
    {
        private static readonly GridPoint[] Horizontal2 = { new(0, 0), new(1, 0) };

        [Test]
        public void BlocksCannotOverlap()
        {
            var a = new BoardBlock(0, 0, new GridPoint(0, 0), Horizontal2);
            var b = new BoardBlock(1, 1, new GridPoint(2, 0), TestShapes.Single);
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
        public void RemovedCellsStopBlocksLikeWalls()
        {
            var block = new BoardBlock(0, 3, new GridPoint(0, 0), TestShapes.Single);
            var door = new BoardDoor(BoardSide.Right, 0, 1, 3);
            var board = new Board(3, 2, new[] { block }, new[] { door }, new[] { new GridPoint(1, 0) });

            Assert.IsFalse(board.IsFloor(1, 0));
            Assert.IsFalse(board.CanPlace(block, new GridPoint(1, 0)));
            Assert.IsTrue(board.CanPlace(block, new GridPoint(2, 0)));
            Assert.IsFalse(board.CanPassThrough(block, block.Position, Direction.Right), "The hole stands between it and its door.");
            Assert.IsFalse(board.Clone().CanPlace(block, new GridPoint(1, 0)), "A copy keeps the holes.");
        }

        [Test]
        public void NoPartOfABlockGoesIntoADoorTheBlockCannotLeaveThrough()
        {
            var shape = new[] { new GridPoint(0, 0), new GridPoint(1, 0), new GridPoint(0, 1) };
            var block = new BoardBlock(0, 3, new GridPoint(0, 1), shape);
            var narrow = new Board(3, 3, new[] { block }, new[] { new BoardDoor(BoardSide.Top, 0, 1, 3) });

            Assert.IsTrue(narrow.CanOccupy(block, 0, 1));
            Assert.IsFalse(narrow.CanOccupy(block, 0, 2), "Its stem would stick into a door its foot can never pass.");

            var wide = new Board(3, 3, new[] { block }, new[] { new BoardDoor(BoardSide.Top, 0, 2, 3) });
            Assert.IsTrue(wide.CanOccupy(block, 0, 2), "A door as wide as the block lets it in.");
        }

        [Test]
        public void ShapeMustPassTheDoorWhole()
        {
            var shape = new[] { new GridPoint(1, 0), new GridPoint(0, 1), new GridPoint(1, 1) };
            var block = new BoardBlock(0, 2, new GridPoint(0, 0), shape);
            var footOnly = new Board(4, 4, new[] { block }, new[] { new BoardDoor(BoardSide.Bottom, 1, 1, 2) });
            var both = new Board(4, 4, new[] { block }, new[] { new BoardDoor(BoardSide.Bottom, 0, 2, 2) });

            Assert.IsFalse(footOnly.CanPassThrough(block, block.Position, Direction.Down));
            Assert.IsTrue(both.CanPassThrough(block, block.Position, Direction.Down));
        }

        [Test]
        public void ABlockDroppedInFrontOfADoorItFitsGoesIn()
        {
            var door = new BoardDoor(BoardSide.Bottom, 1, 2, 3);
            var block = new BoardBlock(0, 3, new GridPoint(1, 0), Horizontal2);
            var board = new Board(4, 4, new[] { block }, new[] { door });

            Assert.AreSame(door, BlockPlacement.DoorToEnter(board, block, new GridPoint(1, 0)));
            Assert.IsNull(BlockPlacement.DoorToEnter(board, block, new GridPoint(1, 1)), "Not next to the door.");
            Assert.IsNull(BlockPlacement.DoorToEnter(board, block, new GridPoint(2, 0)), "Out of line with the door.");

            var otherColor = new BoardBlock(0, 1, new GridPoint(1, 0), Horizontal2);
            var mismatch = new Board(4, 4, new[] { otherColor }, new[] { door });
            Assert.IsNull(BlockPlacement.DoorToEnter(mismatch, otherColor, new GridPoint(1, 0)));
        }

        [Test]
        public void SolverClearsASolvableBoard()
        {
            var red = new BoardBlock(0, 0, new GridPoint(0, 0), TestShapes.Single);
            var blue = new BoardBlock(1, 1, new GridPoint(1, 0), TestShapes.Single);
            var doors = new[] { new BoardDoor(BoardSide.Left, 0, 1, 0), new BoardDoor(BoardSide.Right, 0, 1, 1) };
            var board = new Board(3, 1, new[] { red, blue }, doors);

            var result = new BoardSolver().Solve(board, 1000);

            Assert.IsTrue(result.IsSolved);
            Assert.AreEqual(3, result.Moves.Count);
            Assert.AreEqual(0, result.Repositions);
            Assert.AreEqual(2, board.RemainingBlocks, "The solver leaves the board as it was.");
        }

        [Test]
        public void SolverReportsStuckBoards()
        {
            var block = new BoardBlock(0, 0, new GridPoint(0, 0), TestShapes.Single);
            var board = new Board(2, 2, new[] { block }, new[] { new BoardDoor(BoardSide.Top, 0, 2, 5) });

            var result = new BoardSolver().Solve(board, 1000);

            Assert.IsTrue(result.IsStuck);
        }
    }
}
