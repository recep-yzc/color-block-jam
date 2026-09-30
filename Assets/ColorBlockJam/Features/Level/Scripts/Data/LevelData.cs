using System;

namespace ColorBlockJam.Level
{
    public enum LevelDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    /// <summary>Which way a block may move: freely, or only along one axis, which an arrow on it shows.</summary>
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

    /// <summary>
    /// A level as stored in its JSON file. The level editor writes it and the game reads it.
    /// The board is <see cref="width"/> by <see cref="height"/> cells; cell (0, 0) is the bottom left one.
    /// </summary>
    [Serializable]
    public sealed class LevelData
    {
        public const int DefaultWidth = 6;
        public const int DefaultHeight = 7;
        public const int DefaultTimeLimit = 90;

        public int width = DefaultWidth;
        public int height = DefaultHeight;
        /// <summary>Seconds the player has to clear the board.</summary>
        public int timeLimit = DefaultTimeLimit;
        public LevelDifficulty difficulty;
        public BlockData[] blocks = Array.Empty<BlockData>();
        public DoorData[] doors = Array.Empty<DoorData>();
    }

    /// <summary>
    /// A block: its color, the board cell of its origin and its cells relative to that origin.
    /// An arrow block moves only along <see cref="axis"/>. A block with <see cref="ice"/> is frozen until that many
    /// other blocks have left the board.
    /// </summary>
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

    /// <summary>
    /// A door on one side of the board, <see cref="length"/> cells long from <see cref="start"/>.
    /// A block of the same color leaves the board through it when the block fits in the door.
    /// </summary>
    [Serializable]
    public sealed class DoorData
    {
        public BoardSide side;
        public int start;
        public int length = 1;
        public int color;
    }
}
