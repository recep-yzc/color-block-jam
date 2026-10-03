using System;
using System.Numerics;
using ColorBlockJam.Gameplay.Logic;
using NUnit.Framework;

namespace ColorBlockJam.Tests
{
    public sealed class BlockPickerTests
    {
        private const float Padding = 0.3f;

        [Test]
        public void APressOnABlockPicksThatBlock()
        {
            var block = new BoardBlock(0, 0, new GridPoint(1, 1), TestShapes.Single);
            var board = new Board(3, 3, new[] { block }, Array.Empty<BoardDoor>());

            Assert.AreSame(block, BlockPicker.Pick(board, new Vector2(1.5f, 1.5f), Padding, out var cell));
            Assert.AreEqual(new GridPoint(1, 1), cell);
        }

        [Test]
        public void APressInsideThePaddingPicksTheNearestBlock()
        {
            var left = new BoardBlock(0, 0, new GridPoint(0, 0), TestShapes.Single);
            var right = new BoardBlock(1, 1, new GridPoint(2, 0), TestShapes.Single);
            var board = new Board(3, 1, new[] { left, right }, Array.Empty<BoardDoor>());

            Assert.AreSame(left, BlockPicker.Pick(board, new Vector2(1.2f, 0.5f), Padding, out var leftCell));
            Assert.AreEqual(new GridPoint(0, 0), leftCell, "The pick reports the nearest cell of the block.");
            Assert.AreSame(right, BlockPicker.Pick(board, new Vector2(1.75f, 0.5f), Padding, out _));
        }

        [Test]
        public void APressOutsideThePaddingPicksNothing()
        {
            var block = new BoardBlock(0, 0, new GridPoint(0, 0), TestShapes.Single);
            var board = new Board(3, 1, new[] { block }, Array.Empty<BoardDoor>());

            Assert.IsNull(BlockPicker.Pick(board, new Vector2(1.5f, 0.5f), Padding, out _));
        }

        [Test]
        public void ThePaddingReachesPastTheBoardEdge()
        {
            var block = new BoardBlock(0, 0, new GridPoint(0, 0), TestShapes.Single);
            var board = new Board(2, 1, new[] { block }, Array.Empty<BoardDoor>());

            Assert.AreSame(block, BlockPicker.Pick(board, new Vector2(-0.2f, 0.5f), Padding, out _));
        }

        [Test]
        public void ABlockThatLeftIsNeverPicked()
        {
            var gone = new BoardBlock(0, 0, new GridPoint(0, 0), TestShapes.Single);
            var board = new Board(2, 1, new[] { gone }, Array.Empty<BoardDoor>());
            board.Clear(gone);

            Assert.IsNull(BlockPicker.Pick(board, new Vector2(1.1f, 0.5f), Padding, out _));
        }
    }
}
