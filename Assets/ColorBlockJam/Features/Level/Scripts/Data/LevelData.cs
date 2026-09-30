using System;

namespace ColorBlockJam.Level
{
    public enum LevelDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    public enum BlockAxis
    {
        Free,
        Horizontal,
        Vertical
    }

    public enum BoardSide
    {
        Bottom,
        Top,
        Left,
        Right
    }

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
    }

    [Serializable]
    public sealed class BlockData
    {
        public int color;
        public int x;
        public int y;
        public CellData[] cells = Array.Empty<CellData>();
        public BlockAxis axis;
        public int ice;
    }

    [Serializable]
    public struct CellData
    {
        public int x;
        public int y;

        public CellData(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    [Serializable]
    public sealed class DoorData
    {
        public BoardSide side;
        public int start;
        public int length = 1;
        public int color;
    }
}
