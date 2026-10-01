using System;

namespace ColorBlockJam.Level
{
    [Serializable]
    public sealed class LevelData
    {
        public const int DefaultWidth = 6;
        public const int DefaultHeight = 7;
        public const int DefaultTimeLimit = 90;

        public int width = DefaultWidth;
        public int height = DefaultHeight;
        public int timeLimit = DefaultTimeLimit;
        public LevelDifficulty difficulty;
        public BlockData[] blocks = Array.Empty<BlockData>();
        public DoorData[] doors = Array.Empty<DoorData>();
        public CellData[] holes = Array.Empty<CellData>();
    }
}
