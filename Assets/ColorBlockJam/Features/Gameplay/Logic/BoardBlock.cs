using System;
using ColorBlockJam.Level;

namespace ColorBlockJam.Gameplay.Logic
{
    /// <summary>
    /// A block on the board: a set of cells relative to its origin, a color and the origin's board cell.
    /// It may be an arrow block that moves along one axis only, and it may start frozen under ice.
    /// </summary>
    public sealed class BoardBlock
    {
        private readonly GridPoint[] cells;

        /// <param name="ice">How many other blocks must leave the board before this one thaws and can move.</param>
        public BoardBlock(int id, int color, GridPoint position, GridPoint[] cells, BlockAxis axis = BlockAxis.Free, int ice = 0)
        {
            if (cells.Length == 0)
            {
                throw new ArgumentException("A block needs at least one cell.", nameof(cells));
            }

            Id = id;
            Color = color;
            Position = position;
            Axis = axis;
            Ice = Math.Max(0, ice);
            this.cells = cells;

            MinX = MinY = int.MaxValue;
            MaxX = MaxY = int.MinValue;
            foreach (var cell in cells)
            {
                MinX = Math.Min(MinX, cell.X);
                MaxX = Math.Max(MaxX, cell.X);
                MinY = Math.Min(MinY, cell.Y);
                MaxY = Math.Max(MaxY, cell.Y);
            }
        }

        public int Id { get; }
        public int Color { get; }
        public BlockAxis Axis { get; }

        /// <summary>How many other blocks must leave before this one thaws; see <see cref="Board.IsFrozen"/>.</summary>
        public int Ice { get; }
        public GridPoint Position { get; internal set; }
        public bool IsCleared { get; internal set; }
        /// <summary>The cells relative to <see cref="Position"/>. A span, so looping over them allocates nothing.</summary>
        public ReadOnlySpan<GridPoint> Cells => cells;

        public int MinX { get; }
        public int MaxX { get; }
        public int MinY { get; }
        public int MaxY { get; }
        public int Width => MaxX - MinX + 1;
        public int Height => MaxY - MinY + 1;

        /// <summary>A copy in the same state, for a board copy that can be searched on another thread.</summary>
        internal BoardBlock Copy()
        {
            return new BoardBlock(Id, Color, Position, cells, Axis, Ice) { IsCleared = IsCleared };
        }

        /// <summary>True when the block may move in <paramref name="direction"/>: always, unless it is an arrow block.</summary>
        public bool MovesAlong(Direction direction)
        {
            return Axis switch
            {
                BlockAxis.Horizontal => direction is Direction.Left or Direction.Right,
                BlockAxis.Vertical => direction is Direction.Up or Direction.Down,
                _ => true
            };
        }
    }
}
