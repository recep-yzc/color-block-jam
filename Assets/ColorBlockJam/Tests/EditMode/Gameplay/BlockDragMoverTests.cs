using System;
using System.Numerics;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using NUnit.Framework;
using UnityEngine.TestTools.Constraints;

namespace ColorBlockJam.Tests
{
    public sealed class BlockDragMoverTests
    {
        private const float Rounding = 0.3f;
        private const float FrameTime = 1f / 60f;
        private const float FollowSharpness = 26f;
        private const float MaxSpeed = 40f;

        private static readonly GridPoint[] Single = { new(0, 0) };

        [Test]
        public void StopsAtTheFirstObstacleEvenInOneBigMove()
        {
            var block = new BoardBlock(0, 0, new GridPoint(0, 0), Single);
            var wall = new BoardBlock(1, 1, new GridPoint(2, 0), Single);
            var board = new Board(4, 1, new[] { block, wall }, Array.Empty<BoardDoor>());

            var reached = new BlockDragMover(Rounding).Move(board, block, Vector2.Zero, new Vector2(3f, 0f));

            Assert.AreEqual(1f, reached.X, 0.01f);
        }

        [Test]
        public void ADragFrameAllocatesNothing()
        {
            var shape = new[] { new GridPoint(0, 0), new GridPoint(1, 0), new GridPoint(0, 1) };
            var block = new BoardBlock(0, 0, new GridPoint(0, 0), shape);
            var obstacle = new BoardBlock(1, 1, new GridPoint(3, 1), Single);
            var board = new Board(6, 6, new[] { block, obstacle }, new[] { new BoardDoor(BoardSide.Right, 0, 2, 0) });
            var mover = new BlockDragMover(Rounding);
            TestDelegate frame = () => mover.Move(board, block, Vector2.Zero, new Vector2(2.6f, 1.4f));

            frame();
            frame();
            var isClean = false;
            for (var attempt = 0; attempt < 3 && !isClean; attempt++)
            {
                isClean = !UnityEngine.TestTools.Constraints.Is.AllocatingGCMemory().ApplyTo(frame).IsSuccess;
            }

            Assert.IsTrue(isClean, "A drag frame must not allocate.");
        }

        [Test]
        public void SlidesAlongAWallWhenPushedIntoIt()
        {
            var block = new BoardBlock(0, 0, new GridPoint(0, 1), Single);
            var board = new Board(5, 3, new[] { block }, Array.Empty<BoardDoor>());

            var reached = Drag(board, block, new Vector2(0f, 1f), new Vector2(4f, -3f), frames: 120);

            Assert.AreEqual(4f, reached.X, 0.02f, "It rubs along the floor all the way.");
            Assert.AreEqual(0f, reached.Y, 0.01f);
        }

        [Test]
        public void RollsIntoAGapItIsSlightlyOutOfLineWith()
        {
            var block = new BoardBlock(0, 0, new GridPoint(0, 1), Single);
            var below = new BoardBlock(1, 1, new GridPoint(2, 0), Single);
            var above = new BoardBlock(2, 1, new GridPoint(2, 2), Single);
            var board = new Board(4, 3, new[] { block, below, above }, Array.Empty<BoardDoor>());

            var reached = Drag(board, block, new Vector2(0f, 0.75f), new Vector2(3f, 0.75f), frames: 120);

            Assert.AreEqual(3f, reached.X, 0.02f);
            Assert.AreEqual(0.75f, reached.Y, 0.02f, "Past the gap it follows the finger again.");
        }

        [Test]
        public void RollsAroundACornerWithoutStopping()
        {
            var block = new BoardBlock(0, 0, new GridPoint(1, 1), Single);
            var obstacle = new BoardBlock(1, 1, new GridPoint(2, 1), Single);
            var board = new Board(4, 4, new[] { block, obstacle }, Array.Empty<BoardDoor>());
            var finger = new Vector2(3f, 2.2f);
            var stalls = 0;
            var previous = new Vector2(1f, 1f);

            var reached = Drag(board, block, previous, finger, frames: 120, position =>
            {
                if (Vector2.Distance(position, finger) > 0.05f && Vector2.Distance(position, previous) < 1e-4f)
                {
                    stalls++;
                }

                previous = position;
            });

            Assert.AreEqual(0, stalls, "The block never catches on the corner.");
            Assert.AreEqual(finger.X, reached.X, 0.02f);
            Assert.AreEqual(finger.Y, reached.Y, 0.02f);
        }

        [Test]
        public void APushIntoARoundedCornerRollsOverIt()
        {
            var block = new BoardBlock(0, 0, new GridPoint(1, 1), Single);
            var obstacle = new BoardBlock(1, 1, new GridPoint(2, 1), Single);
            var board = new Board(4, 4, new[] { block, obstacle }, Array.Empty<BoardDoor>());

            var reached = new BlockDragMover(Rounding).Move(board, block, new Vector2(1f, 1.8f), new Vector2(1.8f, 1.8f));

            Assert.AreEqual(1.8f, reached.X, 0.02f);
            Assert.AreEqual(2f, reached.Y, 0.02f);
        }

        [Test]
        public void EntersOnlyADoorOfItsOwnColor()
        {
            var door = new BoardDoor(BoardSide.Bottom, 1, 1, color: 0);
            var matching = new BoardBlock(0, 0, new GridPoint(1, 1), Single);
            var other = new BoardBlock(0, 3, new GridPoint(1, 1), Single);

            var through = Drag(new Board(3, 3, new[] { matching }, new[] { door }), matching, new Vector2(1f, 1f), new Vector2(1f, -2f), 60);
            var stopped = Drag(new Board(3, 3, new[] { other }, new[] { door }), other, new Vector2(1f, 1f), new Vector2(1f, -2f), 60);

            Assert.Less(through.Y, -0.5f);
            Assert.AreEqual(0f, stopped.Y, 0.01f);
        }

        [Test]
        public void NeverOverlapsAnythingWhileDraggedAround()
        {
            var shape = new[] { new GridPoint(0, 0), new GridPoint(1, 0), new GridPoint(0, 1) };
            var dragged = new BoardBlock(0, 2, new GridPoint(0, 0), shape);
            var blocks = new[]
            {
                dragged,
                new BoardBlock(1, 1, new GridPoint(3, 1), Single),
                new BoardBlock(2, 3, new GridPoint(1, 3), new[] { new GridPoint(0, 0), new GridPoint(1, 0) }),
                new BoardBlock(3, 4, new GridPoint(4, 3), new[] { new GridPoint(0, 0), new GridPoint(0, 1) })
            };
            var board = new Board(6, 6, blocks, new[] { new BoardDoor(BoardSide.Right, 0, 2, color: 2) });
            var mover = new BlockDragMover(Rounding);
            var random = new Random(7);
            var position = new Vector2(0f, 0f);
            var finger = position;

            for (var frame = 0; frame < 3000; frame++)
            {
                if (frame % 20 == 0)
                {
                    finger = new Vector2((float)random.NextDouble() * 8f - 1f, (float)random.NextDouble() * 8f - 1f);
                }

                position = Chase(mover, board, dragged, position, finger);
                AssertTouchesOnlyAtCorners(board, dragged, position);
            }
        }

        private static Vector2 Drag(Board board, BoardBlock block, Vector2 from, Vector2 finger, int frames,
            Action<Vector2> onFrame = null)
        {
            var mover = new BlockDragMover(Rounding);
            var position = from;
            for (var frame = 0; frame < frames; frame++)
            {
                position = Chase(mover, board, block, position, finger);
                onFrame?.Invoke(position);
            }

            return position;
        }

        private static Vector2 Chase(BlockDragMover mover, Board board, BoardBlock block, Vector2 position, Vector2 finger)
        {
            var toFinger = finger - position;
            var distance = toFinger.Length();
            if (distance < 0.0005f)
            {
                return position;
            }

            var step = MathF.Min(distance * (1f - MathF.Exp(-FollowSharpness * FrameTime)), MaxSpeed * FrameTime);
            return mover.Move(board, block, position, position + toFinger * (step / distance));
        }

        private static void AssertTouchesOnlyAtCorners(Board board, BoardBlock block, Vector2 position)
        {
            const float tolerance = 0.001f;
            const float play = 0.005f + tolerance;
            foreach (var cell in block.Cells)
            {
                var left = position.X + cell.X;
                var bottom = position.Y + cell.Y;
                for (var x = (int)MathF.Floor(left); x <= (int)MathF.Floor(left + 1f); x++)
                {
                    for (var y = (int)MathF.Floor(bottom); y <= (int)MathF.Floor(bottom + 1f); y++)
                    {
                        if (board.IsOpenFor(block, x, y))
                        {
                            continue;
                        }

                        var overlapX = MathF.Min(left + 1f, x + 1f) - MathF.Max(left, x);
                        var overlapY = MathF.Min(bottom + 1f, y + 1f) - MathF.Max(bottom, y);
                        if (MathF.Min(overlapX, overlapY) > play)
                        {
                            Assert.That(overlapX <= Rounding + tolerance && overlapY <= Rounding + tolerance,
                                $"The block at {position} overlaps cell ({x}, {y}) by {overlapX} × {overlapY}.");
                        }
                    }
                }
            }
        }
    }
}
