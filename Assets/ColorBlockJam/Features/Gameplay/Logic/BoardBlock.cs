using System;
using System.Collections.Generic;

namespace ColorBlockJam.Gameplay.Logic
{
    /// <summary>
    /// A block on the board: a set of cells relative to its origin, a color and the origin's board cell.
    /// </summary>
    public sealed class BoardBlock
    {
        private readonly GridPoint[] cells;

        public BoardBlock(int id, int color, GridPoint position, GridPoint[] cells)
        {
            if (cells.Length == 0)
            {
                throw new ArgumentException("A block needs at least one cell.", nameof(cells));
            }

            Id = id;
            Color = color;
            Position = position;
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
        public GridPoint Position { get; internal set; }
        public bool IsCleared { get; internal set; }
        public IReadOnlyList<GridPoint> Cells => cells;

        public int MinX { get; }
        public int MaxX { get; }
        public int MinY { get; }
        public int MaxY { get; }
        public int Width => MaxX - MinX + 1;
        public int Height => MaxY - MinY + 1;

        /// <summary>A copy in the same state, for a board copy that can be searched on another thread.</summary>
        internal BoardBlock Copy()
        {
            return new BoardBlock(Id, Color, Position, cells) { IsCleared = IsCleared };
        }

        public bool Contains(GridPoint relativeCell)
        {
            foreach (var cell in cells)
            {
                if (cell == relativeCell)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
