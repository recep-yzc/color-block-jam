using System;
using ColorBlockJam.Gameplay.Logic;
using NUnit.Framework;

namespace ColorBlockJam.Tests
{
    public sealed class BoardTargetsTests
    {
        [Test]
        public void TheVacuumTakesEveryBlockOfTheColorStillOnTheBoard()
        {
            var first = new BoardBlock(0, 1, new GridPoint(0, 0), TestShapes.Single);
            var other = new BoardBlock(1, 2, new GridPoint(1, 0), TestShapes.Single);
            var frozen = new BoardBlock(2, 1, new GridPoint(2, 0), TestShapes.Single, ice: 1);
            var gone = new BoardBlock(3, 1, new GridPoint(0, 1), TestShapes.Single);
            var board = new Board(3, 2, new[] { first, other, frozen, gone }, Array.Empty<BoardDoor>());
            board.Clear(gone);

            CollectionAssert.AreEquivalent(new[] { first, frozen }, BoardTargets.OfColor(board, 1),
                "Frozen blocks go too, blocks already gone do not.");
        }

        [Test]
        public void TheRocketTakesEveryBlockWithACellInTheRow()
        {
            var tall = new BoardBlock(0, 0, new GridPoint(0, 0), new[] { new GridPoint(0, 0), new GridPoint(0, 1) });
            var low = new BoardBlock(1, 1, new GridPoint(1, 0), TestShapes.Single);
            var high = new BoardBlock(2, 2, new GridPoint(2, 1), TestShapes.Single);
            var board = new Board(3, 2, new[] { tall, low, high }, Array.Empty<BoardDoor>());

            CollectionAssert.AreEquivalent(new[] { tall, high }, BoardTargets.InRow(board, 1));
            CollectionAssert.AreEquivalent(new[] { tall, low }, BoardTargets.InRow(board, 0));
        }
    }
}
