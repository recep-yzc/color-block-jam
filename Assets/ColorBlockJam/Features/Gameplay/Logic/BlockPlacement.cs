using System;
using System.Numerics;
using ColorBlockJam.Level;

namespace ColorBlockJam.Gameplay.Logic
{
    public static class BlockPlacement
    {
        private static readonly GridPoint[] SnapCandidates =
        {
            new(0, 0), new(1, 0), new(-1, 0), new(0, 1), new(0, -1), new(1, 1), new(-1, 1), new(1, -1), new(-1, -1)
        };

        /// <summary>
        /// The board cell to settle the block on after a drag: the nearest one it fits, or its old cell.
        /// </summary>
        public static GridPoint Snap(Board board, BoardBlock block, Vector2 position)
        {
            var rounded = new GridPoint((int)MathF.Round(position.X), (int)MathF.Round(position.Y));
            var best = block.Position;
            var bestDistance = float.MaxValue;

            foreach (var offset in SnapCandidates)
            {
                var candidate = rounded + offset;
                if (!board.CanPlace(block, candidate))
                {
                    continue;
                }

                var distance = Vector2.DistanceSquared(position, new Vector2(candidate.X, candidate.Y));
                if (distance < bestDistance)
                {
                    best = candidate;
                    bestDistance = distance;
                }
            }

            return best;
        }

        /// <summary>
        /// How far, in cells, the block at <paramref name="position"/> has moved past the board edge
        /// through a door of its color. Zero when it is on the board.
        /// </summary>
        public static float DepthThroughDoor(Board board, BoardBlock block, Vector2 position, out BoardDoor door)
        {
            var column = (int)MathF.Round(position.X) + block.MinX;
            var row = (int)MathF.Round(position.Y) + block.MinY;
            var bottom = position.Y + block.MinY;
            var top = position.Y + block.MaxY + 1f;
            var left = position.X + block.MinX;
            var right = position.X + block.MaxX + 1f;

            door = null;
            var depth = 0f;
            if (bottom < 0f) { depth = -bottom; door = board.DoorBeyond(block, column, -1); }
            else if (top > board.Height) { depth = top - board.Height; door = board.DoorBeyond(block, column, board.Height); }
            else if (left < 0f) { depth = -left; door = board.DoorBeyond(block, -1, row); }
            else if (right > board.Width) { depth = right - board.Width; door = board.DoorBeyond(block, board.Width, row); }

            return door == null ? 0f : depth;
        }

        /// <summary>
        /// The board cell the block stands on when it touches <paramref name="side"/> from inside,
        /// in line with <paramref name="position"/>. A drag through a door leaves the board from here.
        /// </summary>
        public static GridPoint EdgePosition(Board board, BoardBlock block, Vector2 position, BoardSide side)
        {
            var x = (int)MathF.Round(position.X);
            var y = (int)MathF.Round(position.Y);
            return side switch
            {
                BoardSide.Bottom => new GridPoint(x, -block.MinY),
                BoardSide.Top => new GridPoint(x, board.Height - 1 - block.MaxY),
                BoardSide.Left => new GridPoint(-block.MinX, y),
                _ => new GridPoint(board.Width - 1 - block.MaxX, y)
            };
        }

        /// <summary>How many cells the block must travel from the board edge to be fully through the door.</summary>
        public static int LengthThroughDoor(BoardBlock block, BoardSide side)
        {
            return side is BoardSide.Bottom or BoardSide.Top ? block.Height : block.Width;
        }
    }
}
