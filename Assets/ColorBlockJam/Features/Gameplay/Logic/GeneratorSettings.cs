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
}
