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
        public int MinMoves;
        public int MaxMoves = int.MaxValue;
        public int SolveBudget = 5000;

        public int BaseSeconds;
        public int SecondsPerBlock;

        public double ArrowShare;

        public int IceBlocks;
        public int MaxIce;

        public int Holes;
        public int MaxHoleSize = LevelDiagnostics.MinHoleSize;
        public IReadOnlyList<GridPoint[]>[] ShapePools;

        public static GeneratorSettings For(LevelDifficulty difficulty)
        {
            return difficulty switch
            {
                LevelDifficulty.Easy => new GeneratorSettings
                {
                    Width = 5, Height = 6, Colors = 3, MinBlocks = 4, MaxBlocks = 6, MaxDoorLength = 2,
                    MinRepositions = 0, MaxRepositions = 1, MinMoves = 4, MaxMoves = LevelRating.MostEasyMoves,
                    BaseSeconds = 20, SecondsPerBlock = 10,
                    ShapePools = new[] { BlockShapes.Small }
                },
                LevelDifficulty.Medium => new GeneratorSettings
                {
                    Width = 6, Height = 7, Colors = 5, MinBlocks = 7, MaxBlocks = 9, MaxDoorLength = 3,
                    MinRepositions = 1, MaxRepositions = 3, MinMoves = LevelRating.MostEasyMoves + 1,
                    MaxMoves = LevelRating.MostMediumMoves, SolveBudget = 10000,
                    BaseSeconds = 20, SecondsPerBlock = 9, ArrowShare = 0.15,
                    ShapePools = new[] { BlockShapes.Small, BlockShapes.Long }
                },
                LevelDifficulty.Hard => new GeneratorSettings
                {
                    Width = 7, Height = 8, Colors = 6, MinBlocks = 10, MaxBlocks = 12, MaxDoorLength = 3,
                    MinRepositions = 2, MaxRepositions = 5, MinMoves = LevelRating.MostMediumMoves + 1,
                    MaxMoves = LevelRating.MostHardMoves, SolveBudget = 20000,
                    BaseSeconds = 25, SecondsPerBlock = 9, ArrowShare = 0.2, IceBlocks = 1, MaxIce = 3,
                    ShapePools = new[] { BlockShapes.Small, BlockShapes.Long, BlockShapes.Complex }
                },
                LevelDifficulty.SuperHard => new GeneratorSettings
                {
                    Width = 7, Height = 8, Colors = 7, MinBlocks = 12, MaxBlocks = 14, MaxDoorLength = 3,
                    MinRepositions = 3, MaxRepositions = 8, MinMoves = LevelRating.MostHardMoves + 1, SolveBudget = 30000,
                    BaseSeconds = 30, SecondsPerBlock = 9, ArrowShare = 0.25, IceBlocks = 2, MaxIce = 4,
                    ShapePools = new[] { BlockShapes.Small, BlockShapes.Long, BlockShapes.Complex }
                },
                _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null)
            };
        }
    }
}
