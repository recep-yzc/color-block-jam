using System;
using System.Collections.Generic;
using ColorBlockJam.Level;

namespace ColorBlockJam.Gameplay.Logic
{
    public sealed class Board
    {
        private const int Empty = -1;

        private readonly BoardBlock[] blocks;
        private readonly BoardDoor[] doors;
        private readonly GridPoint[] holes;
        private readonly bool[] isHole;
        private readonly int[] occupancy;

        public Board(int width, int height, BoardBlock[] blocks, BoardDoor[] doors, GridPoint[] holes = null)
        {
            for (var i = 0; i < blocks.Length; i++)
            {
                if (blocks[i].Id != i)
                {
                    throw new ArgumentException($"Block {i} has the id {blocks[i].Id}; a block's id must be its index.", nameof(blocks));
                }
            }

            Width = width;
            Height = height;
            this.blocks = blocks;
            this.doors = doors;
            this.holes = holes ?? Array.Empty<GridPoint>();
            isHole = new bool[width * height];
            foreach (var hole in this.holes)
            {
                if (IsInside(hole.X, hole.Y))
                {
                    isHole[hole.Y * width + hole.X] = true;
                }
            }

            occupancy = new int[width * height];
            Rebuild();
        }

        public int Width { get; }
        public int Height { get; }
        public IReadOnlyList<BoardBlock> Blocks => blocks;
        public IReadOnlyList<BoardDoor> Doors => doors;
        public IReadOnlyList<GridPoint> Holes => holes;
        public int RemainingBlocks { get; private set; }
        public bool IsCleared => RemainingBlocks == 0;

        private int ClearedCount => blocks.Length - RemainingBlocks;

        public bool IsFrozen(BoardBlock block) => block.Ice > ClearedCount;

        public int IceLeft(BoardBlock block) => Math.Max(0, block.Ice - ClearedCount);

        public Board Clone()
        {
            var copies = new BoardBlock[blocks.Length];
            for (var i = 0; i < blocks.Length; i++)
            {
                copies[i] = blocks[i].Copy();
            }

            return new Board(Width, Height, copies, doors, holes);
        }

        private bool IsInside(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Width && y < Height;
        }

        public bool IsHole(int x, int y)
        {
            return IsInside(x, y) && isHole[y * Width + x];
        }

        public bool IsFloor(int x, int y)
        {
            return IsInside(x, y) && !isHole[y * Width + x];
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

        public bool IsOpenFor(BoardBlock block, int x, int y)
        {
            if (IsInside(x, y))
            {
                var index = y * Width + x;
                return !isHole[index] && (occupancy[index] == Empty || occupancy[index] == block.Id);
            }

            return DoorBeyond(block, x, y) != null;
        }

        public bool CanOccupy(BoardBlock block, int x, int y)
        {
            foreach (var cell in block.Cells)
            {
                var cellX = x + cell.X;
                var cellY = y + cell.Y;
                if (!IsOpenFor(block, cellX, cellY))
                {
                    return false;
                }

                if (!IsInside(cellX, cellY) && !DoorTakesWhole(block, x, y, DoorBeyond(block, cellX, cellY).Side))
                {
                    return false;
                }
            }

            return true;
        }

        public BoardDoor DoorBeyond(BoardBlock block, int x, int y)
        {
            BoardSide side;
            int alongEdge;

            if (y < 0 && x >= 0 && x < Width) { side = BoardSide.Bottom; alongEdge = x; }
            else if (y >= Height && x >= 0 && x < Width) { side = BoardSide.Top; alongEdge = x; }
            else if (x < 0 && y >= 0 && y < Height) { side = BoardSide.Left; alongEdge = y; }
            else if (x >= Width && y >= 0 && y < Height) { side = BoardSide.Right; alongEdge = y; }
            else { return null; }

            return DoorFor(block, side, alongEdge);
        }

        public BoardDoor DoorAt(BoardSide side, int alongEdge)
        {
            foreach (var door in doors)
            {
                if (door.Side == side && door.Covers(alongEdge))
                {
                    return door;
                }
            }

            return null;
        }

        private bool DoorTakesWhole(BoardBlock block, int x, int y, BoardSide side)
        {
            var isAcrossColumns = side is BoardSide.Top or BoardSide.Bottom;
            foreach (var cell in block.Cells)
            {
                if (DoorFor(block, side, isAcrossColumns ? x + cell.X : y + cell.Y) == null)
                {
                    return false;
                }
            }

            return true;
        }

        private BoardDoor DoorFor(BoardBlock block, BoardSide side, int alongEdge)
        {
            foreach (var door in doors)
            {
                if (door.Side == side && door.Color == block.Color && door.Covers(alongEdge))
                {
                    return door;
                }
            }

            return null;
        }

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

        public bool CanPassThrough(BoardBlock block, GridPoint from, Direction direction)
        {
            if (!block.MovesAlong(direction))
            {
                return false;
            }

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

        internal void SetState(GridPoint[] positions, bool[] cleared)
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
