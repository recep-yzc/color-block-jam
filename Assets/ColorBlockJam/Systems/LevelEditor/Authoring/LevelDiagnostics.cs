using System;
using System.Collections.Generic;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;

namespace ColorBlockJam.LevelEditor.Authoring
{
    public static class LevelDiagnostics
    {
        public const int MinHoleSize = 2;

        public static List<LevelProblem> Find(LevelData level)
        {
            var problems = new List<LevelProblem>();
            if (level.blocks.Length == 0)
            {
                problems.Add(new LevelProblem(LevelProblemKind.NoBlocks));
            }

            var holes = BoardFactory.HolesOf(level);
            var isHole = HoleMap(level, holes);
            CheckHoles(level, holes, isHole, problems);
            CheckCells(level, isHole, problems);
            CheckDoors(level, isHole, problems);
            CheckExits(level, holes, problems);
            return problems;
        }

        private static bool[] HoleMap(LevelData level, GridPoint[] holes)
        {
            var isHole = new bool[level.width * level.height];
            foreach (var hole in holes)
            {
                if (IsInside(level, hole.X, hole.Y))
                {
                    isHole[hole.Y * level.width + hole.X] = true;
                }
            }

            return isHole;
        }

        private static void CheckHoles(LevelData level, GridPoint[] holes, bool[] isHole, List<LevelProblem> problems)
        {
            foreach (var hole in holes)
            {
                if (IsInside(level, hole.X, hole.Y) && !IsInLargeEnoughHole(level, isHole, hole))
                {
                    problems.Add(new LevelProblem(LevelProblemKind.HoleTooSmall));
                    return;
                }
            }
        }

        private static void CheckCells(LevelData level, bool[] isHole, List<LevelProblem> problems)
        {
            var owner = new int[level.width * level.height];
            Array.Fill(owner, -1);
            for (var b = 0; b < level.blocks.Length; b++)
            {
                var block = level.blocks[b];
                foreach (var cell in block.cells)
                {
                    var x = block.x + cell.x;
                    var y = block.y + cell.y;
                    if (!IsInside(level, x, y))
                    {
                        problems.Add(new LevelProblem(LevelProblemKind.BlockOutsideBoard, b, block.color));
                        break;
                    }

                    var index = y * level.width + x;
                    if (isHole[index])
                    {
                        problems.Add(new LevelProblem(LevelProblemKind.BlockOnHole, b, block.color));
                        break;
                    }

                    if (owner[index] >= 0 && owner[index] != b)
                    {
                        problems.Add(new LevelProblem(LevelProblemKind.BlocksOverlap, b, block.color));
                        break;
                    }

                    owner[index] = b;
                }
            }
        }

        private static void CheckDoors(LevelData level, bool[] isHole, List<LevelProblem> problems)
        {
            for (var d = 0; d < level.doors.Length; d++)
            {
                var door = level.doors[d];
                var edge = door.side is BoardSide.Bottom or BoardSide.Top ? level.width : level.height;
                if (door.start < 0 || door.length < 1 || door.start + door.length > edge)
                {
                    problems.Add(new LevelProblem(LevelProblemKind.DoorOutsideBoard, color: door.color));
                }
                else if (FacesHole(level, door, isHole))
                {
                    problems.Add(new LevelProblem(LevelProblemKind.DoorFacesHole, color: door.color));
                }

                for (var other = 0; other < d; other++)
                {
                    var earlier = level.doors[other];
                    if (earlier.side == door.side && door.start < earlier.start + earlier.length && earlier.start < door.start + door.length)
                    {
                        problems.Add(new LevelProblem(LevelProblemKind.DoorsOverlap, color: door.color));
                    }
                }
            }
        }

        private static void CheckExits(LevelData level, GridPoint[] holes, List<LevelProblem> problems)
        {
            var reported = new HashSet<int>();
            for (var b = 0; b < level.blocks.Length; b++)
            {
                var block = level.blocks[b];
                if (block.cells.Length == 0)
                {
                    problems.Add(new LevelProblem(LevelProblemKind.EmptyBlock, b, block.color));
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
                        new GridPoint(block.x, block.y), holes))
                {
                    problems.Add(new LevelProblem(LevelProblemKind.BlockFitsNoDoor, b, block.color));
                }

                if (block.ice > level.blocks.Length - 1)
                {
                    problems.Add(new LevelProblem(LevelProblemKind.IceNeverMelts, b, block.color));
                }
            }
        }

        private static bool IsInside(LevelData level, int x, int y)
        {
            return x >= 0 && y >= 0 && x < level.width && y < level.height;
        }

        public static bool CanEverLeave(int width, int height, GridPoint[] shape, int color, IReadOnlyList<DoorData> doors,
            BlockAxis axis, GridPoint origin, GridPoint[] holes)
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

                var board = new Board(width, height, new[] { probe }, new[] { new BoardDoor(door.side, door.start, door.length, door.color) },
                    holes);
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

                    if (!BlockPlacement.StaysOnLane(axis, origin, position) || !board.CanPlace(probe, position))
                    {
                        continue;
                    }

                    board.Move(probe, position);
                    if (BlockPlacement.DoorToEnter(board, probe, position) != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsInLargeEnoughHole(LevelData level, bool[] isHole, GridPoint cell)
        {
            for (var left = cell.X - MinHoleSize + 1; left <= cell.X; left++)
            {
                for (var bottom = cell.Y - MinHoleSize + 1; bottom <= cell.Y; bottom++)
                {
                    if (IsHoleSquare(level, isHole, left, bottom))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsHoleSquare(LevelData level, bool[] isHole, int left, int bottom)
        {
            for (var x = left; x < left + MinHoleSize; x++)
            {
                for (var y = bottom; y < bottom + MinHoleSize; y++)
                {
                    if (!IsInside(level, x, y) || !isHole[y * level.width + x])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool FacesHole(LevelData level, DoorData door, bool[] isHole)
        {
            for (var along = door.start; along < door.start + door.length; along++)
            {
                var cell = door.side switch
                {
                    BoardSide.Bottom => new GridPoint(along, 0),
                    BoardSide.Top => new GridPoint(along, level.height - 1),
                    BoardSide.Left => new GridPoint(0, along),
                    _ => new GridPoint(level.width - 1, along)
                };

                if (isHole[cell.Y * level.width + cell.X])
                {
                    return true;
                }
            }

            return false;
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
