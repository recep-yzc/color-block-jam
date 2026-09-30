using System;
using System.Collections.Generic;
using ColorBlockJam.Level;

namespace ColorBlockJam.Gameplay.Logic
{
    public sealed class GeneratorSettings
    {
        public int Width;
        public int Height;
        public int Colors;
        public int MinBlocks;
        public int MaxBlocks;
        public int MaxDoorLength;

        public int MinRepositions;
        public int MaxRepositions;

        public int BaseSeconds;
        public int SecondsPerBlock;

        public double ArrowShare;

        public int IceBlocks;
        public int MaxIce;
        public IReadOnlyList<GridPoint[]>[] ShapePools;

        public static GeneratorSettings For(LevelDifficulty difficulty)
        {
            return difficulty switch
            {
                LevelDifficulty.Easy => new GeneratorSettings
                {
                    Width = 5, Height = 6, Colors = 3, MinBlocks = 5, MaxBlocks = 6, MaxDoorLength = 2,
                    MinRepositions = 0, MaxRepositions = 1, BaseSeconds = 20, SecondsPerBlock = 10,
                    ShapePools = new[] { BlockShapes.Small }
                },
                LevelDifficulty.Medium => new GeneratorSettings
                {
                    Width = 6, Height = 7, Colors = 5, MinBlocks = 8, MaxBlocks = 9, MaxDoorLength = 3,
                    MinRepositions = 1, MaxRepositions = 2, BaseSeconds = 20, SecondsPerBlock = 9, ArrowShare = 0.15,
                    ShapePools = new[] { BlockShapes.Small, BlockShapes.Long }
                },
                _ => new GeneratorSettings
                {
                    Width = 7, Height = 8, Colors = 7, MinBlocks = 11, MaxBlocks = 13, MaxDoorLength = 3,
                    MinRepositions = 2, MaxRepositions = 6, BaseSeconds = 20, SecondsPerBlock = 8, ArrowShare = 0.2,
                    IceBlocks = 1, MaxIce = 3,
                    ShapePools = new[] { BlockShapes.Small, BlockShapes.Long, BlockShapes.Complex }
                }
            };
        }
    }

    public sealed class GeneratedLevel
    {
        public GeneratedLevel(LevelData level, SolveResult solution)
        {
            Level = level;
            Solution = solution;
        }

        public LevelData Level { get; }
        public SolveResult Solution { get; }
    }

    public sealed class LevelGenerator
    {
        private const int DoorPlacementTries = 30;
        private const int BlockPlacementTries = 400;

        private static readonly BoardSide[] Sides = { BoardSide.Bottom, BoardSide.Top, BoardSide.Left, BoardSide.Right };

        private readonly BoardSolver solver = new();

        public int SolveBudget { get; } = 5000;
        public int MaxAttempts { get; } = 300;

        public GeneratedLevel Generate(LevelDifficulty difficulty, int paletteSize, int seed, Func<int, bool> onAttempt = null)
        {
            var settings = GeneratorSettings.For(difficulty);
            var random = new Random(seed);

            for (var attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                if (onAttempt != null && !onAttempt(attempt))
                {
                    return null;
                }

                var level = TryBuild(settings, difficulty, paletteSize, random);
                if (level == null)
                {
                    continue;
                }

                var result = solver.Solve(BoardFactory.Create(level), SolveBudget);
                if (!result.IsSolved)
                {
                    continue;
                }

                if (result.Repositions >= settings.MinRepositions && result.Repositions <= settings.MaxRepositions)
                {
                    return new GeneratedLevel(level, result);
                }
            }

            return null;
        }

        private static LevelData TryBuild(GeneratorSettings settings, LevelDifficulty difficulty, int paletteSize, Random random)
        {
            var colors = PickColors(Math.Min(settings.Colors, paletteSize), paletteSize, random);
            var doors = PlaceDoors(settings, colors, random);
            if (doors == null)
            {
                return null;
            }

            var blockCount = random.Next(settings.MinBlocks, settings.MaxBlocks + 1);
            var blocks = PlaceBlocks(settings, colors, doors, blockCount, random);
            if (blocks.Count < settings.MinBlocks)
            {
                return null;
            }

            AddIce(settings, blocks, random);

            return new LevelData
            {
                width = settings.Width,
                height = settings.Height,
                difficulty = difficulty,
                timeLimit = RoundUpToFive(settings.BaseSeconds + settings.SecondsPerBlock * blocks.Count),
                blocks = blocks.ToArray(),
                doors = doors.ToArray()
            };
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

        private static List<BlockData> PlaceBlocks(GeneratorSettings settings, int[] colors, List<DoorData> doors, int count, Random random)
        {
            var taken = new bool[settings.Width, settings.Height];
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
                    !LevelDiagnostics.CanEverLeave(settings.Width, settings.Height, shape, color, doors, axis, new GridPoint(x, y)))
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
            for (var i = 0; i < settings.IceBlocks; i++)
            {
                var block = blocks[random.Next(blocks.Count)];
                block.ice = Math.Min(blocks.Count - 1, random.Next(1, settings.MaxIce + 1));
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
