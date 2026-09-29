using System;
using System.Collections.Generic;
using ColorBlockJam.Level;

namespace ColorBlockJam.Gameplay.Logic
{
    /// <summary>
    /// The board rules: which cells are taken, where a block may be and which door lets it out.
    /// It knows nothing about input, time or views.
    /// </summary>
    public sealed class Board
    {
        private const int Empty = -1;

        private readonly BoardBlock[] blocks;
        private readonly BoardDoor[] doors;
        private readonly int[] occupancy;

        public Board(int width, int height, BoardBlock[] blocks, BoardDoor[] doors)
        {
            Width = width;
            Height = height;
            this.blocks = blocks;
            this.doors = doors;
            occupancy = new int[width * height];
            Rebuild();
        }

        public int Width { get; }
        public int Height { get; }
        public IReadOnlyList<BoardBlock> Blocks => blocks;
        public IReadOnlyList<BoardDoor> Doors => doors;
        public int RemainingBlocks { get; private set; }
        public bool IsCleared => RemainingBlocks == 0;

        public bool IsInside(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Width && y < Height;
        }

        public BoardBlock BlockAt(GridPoint cell)
        {
            if (!IsInside(cell.X, cell.Y))
            {
                return null;
            }

            var id = occupancy[cell.Y * Width + cell.X];
            return id == Empty ? null : blocks[id];
        }

        /// <summary>
        /// True when <paramref name="block"/> may cover the cell: an empty or own cell on the board,
        /// or a cell beyond a door of its color.
        /// </summary>
        public bool IsOpenFor(BoardBlock block, int x, int y)
        {
            if (IsInside(x, y))
            {
                var id = occupancy[y * Width + x];
                return id == Empty || id == block.Id;
            }

            return DoorBeyond(block, x, y) != null;
        }

        /// <summary>The door of the block's color in front of an outside cell, or null.</summary>
        public BoardDoor DoorBeyond(BoardBlock block, int x, int y)
        {
            BoardSide side;
            int alongEdge;

            if (y < 0 && x >= 0 && x < Width) { side = BoardSide.Bottom; alongEdge = x; }
            else if (y >= Height && x >= 0 && x < Width) { side = BoardSide.Top; alongEdge = x; }
            else if (x < 0 && y >= 0 && y < Height) { side = BoardSide.Left; alongEdge = y; }
            else if (x >= Width && y >= 0 && y < Height) { side = BoardSide.Right; alongEdge = y; }
            else { return null; }

            foreach (var door in doors)
            {
                if (door.Side == side && door.Color == block.Color && door.Covers(alongEdge))
                {
                    return door;
                }
            }

            return null;
        }

        /// <summary>True when every cell of the block at <paramref name="position"/> is on the board and open.</summary>
        public bool CanPlace(BoardBlock block, GridPoint position)
        {
            foreach (var cell in block.Cells)
            {
                var x = position.X + cell.X;
                var y = position.Y + cell.Y;
                if (!IsInside(x, y) || !IsOpenFor(block, x, y))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// True when the block, standing on the board at <paramref name="from"/>, can slide in
        /// <paramref name="direction"/> until it is fully off the board: every cell it sweeps on the way is open for it.
        /// A shape wider than its door, or one that would scrape a wall on the way out, cannot leave.
        /// </summary>
        public bool CanPassThrough(BoardBlock block, GridPoint from, Direction direction)
        {
            var offset = direction.ToOffset();
            for (var position = from + offset; ; position += offset)
            {
                var isOutside = true;
                foreach (var cell in block.Cells)
                {
                    var x = position.X + cell.X;
                    var y = position.Y + cell.Y;
                    if (!IsOpenFor(block, x, y))
                    {
                        return false;
                    }

                    isOutside &= !IsInside(x, y);
                }

                if (isOutside)
                {
                    return true;
                }
            }
        }

        public void Move(BoardBlock block, GridPoint position)
        {
            Stamp(block, Empty);
            block.Position = position;
            Stamp(block, block.Id);
        }

        public void Clear(BoardBlock block)
        {
            if (block.IsCleared)
            {
                return;
            }

            Stamp(block, Empty);
            block.IsCleared = true;
            RemainingBlocks--;
        }

        /// <summary>Puts every block back to the given state. Used by the solver to visit states.</summary>
        public void SetState(IReadOnlyList<GridPoint> positions, IReadOnlyList<bool> cleared)
        {
            for (var i = 0; i < blocks.Length; i++)
            {
                blocks[i].Position = positions[i];
                blocks[i].IsCleared = cleared[i];
            }

            Rebuild();
        }

        private void Rebuild()
        {
            Array.Fill(occupancy, Empty);
            RemainingBlocks = 0;

            foreach (var block in blocks)
            {
                if (block.IsCleared)
                {
                    continue;
                }

                RemainingBlocks++;
                Stamp(block, block.Id);
            }
        }

        private void Stamp(BoardBlock block, int value)
        {
            foreach (var cell in block.Cells)
            {
                var x = block.Position.X + cell.X;
                var y = block.Position.Y + cell.Y;
                if (IsInside(x, y))
                {
                    occupancy[y * Width + x] = value;
                }
            }
        }
    }
}
