using System;
using System.Collections.Generic;
using System.Threading;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;

namespace ColorBlockJam.LevelEditor.Authoring
{
    public sealed class LevelGenerator
    {
        private const int DoorPlacementTries = 30;
        private const int BlockPlacementTries = 400;
        private const int HolePlacementTries = 30;

        private static readonly BoardSide[] Sides = { BoardSide.Bottom, BoardSide.Top, BoardSide.Left, BoardSide.Right };

        private readonly BoardSolver solver = new();

        public const int MaxAttempts = 300;

        public GeneratedLevel Generate(GeneratorSettings settings, int paletteSize, int seed,
            CancellationToken cancellationToken = default, Action<int> onAttempt = null)
        {
            var random = new Random(seed);

            for (var attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                onAttempt?.Invoke(attempt);

                var level = TryBuild(settings, paletteSize, random);
                if (level == null)
                {
                    continue;
                }

                var result = solver.Solve(BoardFactory.Create(level), settings.SolveBudget, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                if (!result.IsSolved)
                {
                    continue;
                }

                var moves = LevelRating.MovesToWin(level.blocks.Length, result);
                if (result.Repositions >= settings.MinRepositions && result.Repositions <= settings.MaxRepositions &&
                    moves >= settings.MinMoves && moves <= settings.MaxMoves)
                {
                    return new GeneratedLevel(level, result);
                }
            }

            return null;
        }

        private static LevelData TryBuild(GeneratorSettings settings, int paletteSize, Random random)
        {
            var holes = PlaceHoles(settings, random);
            if (holes == null)
            {
                return null;
            }

            var colors = PickColors(Math.Min(settings.Colors, paletteSize), paletteSize, random);
            var doors = PlaceDoors(settings, colors, random);
            if (doors == null)
            {
                return null;
            }

            var blockCount = random.Next(settings.MinBlocks, settings.MaxBlocks + 1);
            var blocks = PlaceBlocks(settings, colors, doors, holes, blockCount, random);
            if (blocks.Count < settings.MinBlocks)
            {
                return null;
            }

            AddIce(settings, blocks, random);

            return new LevelData
            {
                width = settings.Width,
                height = settings.Height,
                difficulty = settings.Difficulty,
                timeLimit = RoundUpToFive(settings.BaseSeconds + settings.SecondsPerBlock * blocks.Count),
                blocks = blocks.ToArray(),
                doors = doors.ToArray(),
                holes = Array.ConvertAll(holes, hole => new CellData(hole.X, hole.Y))
            };
        }

        private static GridPoint[] PlaceHoles(GeneratorSettings settings, Random random)
        {
            var holes = new List<GridPoint>();
            for (var hole = 0; hole < settings.Holes; hole++)
            {
                var isPlaced = false;
                for (var tries = 0; tries < HolePlacementTries && !isPlaced; tries++)
                {
                    var width = random.Next(LevelDiagnostics.MinHoleSize, Math.Max(LevelDiagnostics.MinHoleSize, settings.MaxHoleSize) + 1);
                    var height = random.Next(LevelDiagnostics.MinHoleSize, Math.Max(LevelDiagnostics.MinHoleSize, settings.MaxHoleSize) + 1);
                    if (width + 2 > settings.Width || height + 2 > settings.Height)
                    {
                        continue;
                    }

                    var x = random.Next(1, settings.Width - width);
                    var y = random.Next(1, settings.Height - height);
                    if (TouchesHole(holes, x - 1, y - 1, x + width, y + height))
                    {
                        continue;
                    }

                    for (var dx = 0; dx < width; dx++)
                    {
                        for (var dy = 0; dy < height; dy++)
                        {
                            holes.Add(new GridPoint(x + dx, y + dy));
                        }
                    }

                    isPlaced = true;
                }

                if (!isPlaced)
                {
                    return null;
                }
            }

            return holes.ToArray();
        }

        private static bool TouchesHole(List<GridPoint> holes, int minX, int minY, int maxX, int maxY)
        {
            foreach (var hole in holes)
            {
                if (hole.X >= minX && hole.X <= maxX && hole.Y >= minY && hole.Y <= maxY)
                {
                    return true;
                }
            }

            return false;
        }

        private static int[] PickColors(int count, int paletteSize, Random random)
        {
            var all = new int[paletteSize];
            for (var i = 0; i < all.Length; i++)
            {
                all[i] = i;
            }

            Shuffle(all, random);
            var colors = new int[count];
            Array.Copy(all, colors, count);
            return colors;
        }

        private static List<DoorData> PlaceDoors(GeneratorSettings settings, int[] colors, Random random)
        {
            var doors = new List<DoorData>();
            foreach (var color in colors)
            {
                var isPlaced = false;
                for (var tries = 0; tries < DoorPlacementTries && !isPlaced; tries++)
                {
                    var side = Sides[random.Next(Sides.Length)];
                    var edge = side is BoardSide.Bottom or BoardSide.Top ? settings.Width : settings.Height;
                    var length = Math.Min(edge, random.Next(1, settings.MaxDoorLength + 1));
                    var start = random.Next(0, edge - length + 1);
                    if (Overlaps(doors, side, start, length))
                    {
                        continue;
                    }

                    doors.Add(new DoorData { side = side, start = start, length = length, color = color });
                    isPlaced = true;
                }

                if (!isPlaced)
                {
                    return null;
                }
            }

            return doors;
        }

        private static bool Overlaps(List<DoorData> doors, BoardSide side, int start, int length)
        {
            foreach (var door in doors)
            {
                if (door.side == side && start < door.start + door.length && door.start < start + length)
                {
                    return true;
                }
            }

            return false;
        }

        private static List<BlockData> PlaceBlocks(GeneratorSettings settings, int[] colors, List<DoorData> doors, GridPoint[] holes,
            int count, Random random)
        {
            var taken = new bool[settings.Width, settings.Height];
            foreach (var hole in holes)
            {
                taken[hole.X, hole.Y] = true;
            }

            var blocks = new List<BlockData>();

            for (var tries = 0; tries < BlockPlacementTries && blocks.Count < count; tries++)
            {
                var color = blocks.Count < colors.Length ? colors[blocks.Count] : colors[random.Next(colors.Length)];
                var pool = settings.ShapePools[random.Next(settings.ShapePools.Length)];
                var shape = pool[random.Next(pool.Count)];
                var width = 0;
                var height = 0;
                foreach (var cell in shape)
                {
                    width = Math.Max(width, cell.X + 1);
                    height = Math.Max(height, cell.Y + 1);
                }

                if (width > settings.Width || height > settings.Height)
                {
                    continue;
                }

                var x = random.Next(0, settings.Width - width + 1);
                var y = random.Next(0, settings.Height - height + 1);
                var axis = random.NextDouble() < settings.ArrowShare
                    ? random.Next(2) == 0 ? BlockAxis.Horizontal : BlockAxis.Vertical
                    : BlockAxis.Free;
                if (!IsFree(taken, shape, x, y) ||
                    !LevelDiagnostics.CanEverLeave(settings.Width, settings.Height, shape, color, doors, axis, new GridPoint(x, y), holes))
                {
                    continue;
                }

                var cells = new CellData[shape.Length];
                for (var i = 0; i < shape.Length; i++)
                {
                    cells[i] = new CellData(shape[i].X, shape[i].Y);
                    taken[x + shape[i].X, y + shape[i].Y] = true;
                }

                blocks.Add(new BlockData { color = color, x = x, y = y, cells = cells, axis = axis });
            }

            return blocks;
        }

        private static void AddIce(GeneratorSettings settings, List<BlockData> blocks, Random random)
        {
            var order = new int[blocks.Count];
            for (var i = 0; i < order.Length; i++)
            {
                order[i] = i;
            }

            var count = Math.Min(settings.IceBlocks, order.Length);
            for (var i = 0; i < count; i++)
            {
                var pick = random.Next(i, order.Length);
                (order[i], order[pick]) = (order[pick], order[i]);
                blocks[order[i]].ice = Math.Min(blocks.Count - 1, random.Next(1, settings.MaxIce + 1));
            }
        }

        private static bool IsFree(bool[,] taken, GridPoint[] shape, int x, int y)
        {
            foreach (var cell in shape)
            {
                if (taken[x + cell.X, y + cell.Y])
                {
                    return false;
                }
            }

            return true;
        }

        private static int RoundUpToFive(int seconds)
        {
            return (seconds + 4) / 5 * 5;
        }

        private static void Shuffle(int[] values, Random random)
        {
            for (var i = values.Length - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                (values[i], values[j]) = (values[j], values[i]);
            }
        }
    }
}
