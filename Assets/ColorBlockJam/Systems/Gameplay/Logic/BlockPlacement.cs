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

        public static GridPoint Snap(Board board, BoardBlock block, Vector2 position)
        {
            var rounded = new GridPoint((int)MathF.Round(position.X), (int)MathF.Round(position.Y));
            var best = block.Position;
            var bestDistance = float.MaxValue;

            foreach (var offset in SnapCandidates)
            {
                var candidate = rounded + offset;
                if (!board.CanPlace(block, candidate) || !StaysOnLane(block.Axis, block.Position, candidate))
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

        public static bool StaysOnLane(BlockAxis axis, GridPoint lane, GridPoint cell)
        {
            return axis switch
            {
                BlockAxis.Horizontal => cell.Y == lane.Y,
                BlockAxis.Vertical => cell.X == lane.X,
                _ => true
            };
        }

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

        public static int StepsToLeave(Board board, BoardBlock block, GridPoint from, Direction direction)
        {
            return direction switch
            {
                Direction.Down => from.Y + block.MaxY + 1,
                Direction.Up => board.Height - from.Y - block.MinY,
                Direction.Left => from.X + block.MaxX + 1,
                _ => board.Width - from.X - block.MinX
            };
        }

        public static BoardDoor ExitDoor(Board board, BoardBlock block, GridPoint from, Direction direction)
        {
            foreach (var cell in block.Cells)
            {
                var x = from.X + cell.X;
                var y = from.Y + cell.Y;
                var door = direction switch
                {
                    Direction.Down => board.DoorBeyond(block, x, -1),
                    Direction.Up => board.DoorBeyond(block, x, board.Height),
                    Direction.Left => board.DoorBeyond(block, -1, y),
                    _ => board.DoorBeyond(block, board.Width, y)
                };

                if (door != null)
                {
                    return door;
                }
            }

            return null;
        }

        public static bool TouchesSide(Board board, BoardBlock block, GridPoint position, BoardSide side)
        {
            return side switch
            {
                BoardSide.Bottom => position.Y + block.MinY == 0,
                BoardSide.Top => position.Y + block.MaxY == board.Height - 1,
                BoardSide.Left => position.X + block.MinX == 0,
                _ => position.X + block.MaxX == board.Width - 1
            };
        }

        public static BoardDoor DoorToEnter(Board board, BoardBlock block, GridPoint cell)
        {
            var doors = board.Doors;
            for (var i = 0; i < doors.Count; i++)
            {
                var door = doors[i];
                if (door.Color == block.Color && !board.IsFrozen(block) && TouchesSide(board, block, cell, door.Side) &&
                    board.CanPassThrough(block, cell, door.ExitDirection))
                {
                    return ExitDoor(board, block, cell, door.ExitDirection);
                }
            }

            return null;
        }

        public static int LengthThroughDoor(BoardBlock block, BoardSide side)
        {
            return side is BoardSide.Bottom or BoardSide.Top ? block.Height : block.Width;
        }
    }
}
