using System;
using ColorBlockJam.Level;

namespace ColorBlockJam.Gameplay.Logic
{
    public sealed class BoardBlock
    {
        private readonly GridPoint[] cells;

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

        public int Ice { get; }
        public GridPoint Position { get; internal set; }
        public bool IsCleared { get; internal set; }
        public ReadOnlySpan<GridPoint> Cells => cells;

        public int MinX { get; }
        public int MaxX { get; }
        public int MinY { get; }
        public int MaxY { get; }
        public int Width => MaxX - MinX + 1;
        public int Height => MaxY - MinY + 1;

        internal BoardBlock Copy()
        {
            return new BoardBlock(Id, Color, Position, cells, Axis, Ice) { IsCleared = IsCleared };
        }

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
