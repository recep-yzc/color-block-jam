using System.Collections.Generic;
using ColorBlockJam.Level;

namespace ColorBlockJam.Gameplay.Logic
{
    public enum LevelProblemKind
    {
        NoBlocks,
        BlockOutsideBoard,
        BlocksOverlap,
        DoorOutsideBoard,
        DoorsOverlap,
        ColorHasNoDoor,
        BlockFitsNoDoor,
        IceNeverMelts
    }

    /// <summary>A mistake in a level found without solving it. <see cref="Block"/> and <see cref="Color"/> are -1 when unused.</summary>
    public readonly struct LevelProblem
    {
        public readonly LevelProblemKind Kind;
        public readonly int Block;
        public readonly int Color;

        public LevelProblem(LevelProblemKind kind, int block = -1, int color = -1)
        {
            Kind = kind;
            Block = block;
            Color = color;
        }
    }

    /// <summary>
    /// Quick checks that catch most broken levels at once, before the slower solver runs:
    /// every block on the board and apart, every door on an edge, a door each block fits through (along its axis for
    /// an arrow block), and ice that enough other blocks can melt.
    /// </summary>
    public static class LevelDiagnostics
    {
        public static List<LevelProblem> Find(LevelData level)
        {
            var problems = new List<LevelProblem>();
            if (level.blocks.Length == 0)
            {
                problems.Add(new LevelProblem(LevelProblemKind.NoBlocks));
            }

            var owner = new int[level.width * level.height];
            for (var i = 0; i < owner.Length; i++)
            {
                owner[i] = -1;
            }

            for (var b = 0; b < level.blocks.Length; b++)
            {
                var block = level.blocks[b];
                foreach (var cell in block.cells)
                {
                    var x = block.x + cell.x;
                    var y = block.y + cell.y;
                    if (x < 0 || y < 0 || x >= level.width || y >= level.height)
                    {
                        problems.Add(new LevelProblem(LevelProblemKind.BlockOutsideBoard, b, block.color));
                        break;
                    }

                    var index = y * level.width + x;
                    if (owner[index] >= 0 && owner[index] != b)
                    {
                        problems.Add(new LevelProblem(LevelProblemKind.BlocksOverlap, b, block.color));
                        break;
                    }

                    owner[index] = b;
                }
            }

            for (var d = 0; d < level.doors.Length; d++)
            {
                var door = level.doors[d];
                var edge = door.side is BoardSide.Bottom or BoardSide.Top ? level.width : level.height;
                if (door.start < 0 || door.length < 1 || door.start + door.length > edge)
                {
                    problems.Add(new LevelProblem(LevelProblemKind.DoorOutsideBoard, color: door.color));
                }

                for (var other = 0; other < d; other++)
                {
                    var next = level.doors[other];
                    if (next.side == door.side && door.start < next.start + next.length && next.start < door.start + door.length)
                    {
                        problems.Add(new LevelProblem(LevelProblemKind.DoorsOverlap, color: door.color));
                    }
                }
            }

            var reported = new HashSet<int>();
            for (var b = 0; b < level.blocks.Length; b++)
            {
                var block = level.blocks[b];
                if (block.cells.Length == 0)
                {
                    continue;
                }

                if (!HasDoor(level, block.color))
                {
                    if (reported.Add(block.color))
                    {
                        problems.Add(new LevelProblem(LevelProblemKind.ColorHasNoDoor, b, block.color));
                    }

                    continue;
                }

                if (!CanEverLeave(level.width, level.height, ToShape(block), block.color, level.doors, block.axis,
                        new GridPoint(block.x, block.y)))
                {
                    problems.Add(new LevelProblem(LevelProblemKind.BlockFitsNoDoor, b, block.color));
                }

                // Ice melts one step for every other block that leaves, so it cannot ask for more than there are.
                if (block.ice > level.blocks.Length - 1)
                {
                    problems.Add(new LevelProblem(LevelProblemKind.IceNeverMelts, b, block.color));
                }
            }

            return problems;
        }

        /// <summary>
        /// True when the shape, alone on an empty board, fits through some door of its color. An arrow block also has to
        /// reach the door along its axis, so it stays on the row or column of <paramref name="origin"/>.
        /// </summary>
        public static bool CanEverLeave(int width, int height, GridPoint[] shape, int color, IReadOnlyList<DoorData> doors,
            BlockAxis axis = BlockAxis.Free, GridPoint origin = default)
        {
            var probe = new BoardBlock(0, color, origin, shape, axis);
            if (probe.Width > width || probe.Height > height)
            {
                return false;
            }

            foreach (var door in doors)
            {
                if (door.color != color)
                {
                    continue;
                }

                var direction = BoardDoor.ExitDirectionOf(door.side);
                if (!probe.MovesAlong(direction))
                {
                    continue;
                }

                var board = new Board(width, height, new[] { probe }, new[] { new BoardDoor(door.side, door.start, door.length, door.color) });
                var isAlongX = door.side is BoardSide.Bottom or BoardSide.Top;
                var steps = isAlongX ? width - probe.Width : height - probe.Height;

                for (var along = 0; along <= steps; along++)
                {
                    var position = door.side switch
                    {
                        BoardSide.Bottom => new GridPoint(along - probe.MinX, -probe.MinY),
                        BoardSide.Top => new GridPoint(along - probe.MinX, height - 1 - probe.MaxY),
                        BoardSide.Left => new GridPoint(-probe.MinX, along - probe.MinY),
                        _ => new GridPoint(width - 1 - probe.MaxX, along - probe.MinY)
                    };

                    if (!IsOnLane(axis, position, origin))
                    {
                        continue;
                    }

                    board.Move(probe, position);
                    if (board.CanPassThrough(probe, position, direction))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsOnLane(BlockAxis axis, GridPoint position, GridPoint origin)
        {
            return axis switch
            {
                BlockAxis.Horizontal => position.Y == origin.Y,
                BlockAxis.Vertical => position.X == origin.X,
                _ => true
            };
        }

        private static bool HasDoor(LevelData level, int color)
        {
            foreach (var door in level.doors)
            {
                if (door.color == color)
                {
                    return true;
                }
            }

            return false;
        }

        private static GridPoint[] ToShape(BlockData block)
        {
            var shape = new GridPoint[block.cells.Length];
            for (var i = 0; i < shape.Length; i++)
            {
                shape[i] = new GridPoint(block.cells[i].x, block.cells[i].y);
            }

            return shape;
        }
    }
}
