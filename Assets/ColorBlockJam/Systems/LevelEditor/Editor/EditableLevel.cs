using System;
using System.Collections.Generic;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;

namespace ColorBlockJam.LevelEditor
{
    internal sealed class EditableLevel
    {
        public const int MinSize = 3;
        public const int MaxSize = 10;
        public const int NoDoor = -1;

        private readonly Dictionary<BoardSide, int[]> doorSlots = new();

        public EditableLevel(int width, int height)
        {
            Width = width;
            Height = height;
            foreach (BoardSide side in Enum.GetValues(typeof(BoardSide)))
            {
                doorSlots[side] = NewSlots(SlotCount(side));
            }
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int TimeLimit = LevelData.DefaultTimeLimit;
        public LevelDifficulty Difficulty;
        public readonly List<EditableBlock> Blocks = new();

        public static EditableLevel From(LevelData data)
        {
            var level = new EditableLevel(Math.Clamp(data.width, MinSize, MaxSize), Math.Clamp(data.height, MinSize, MaxSize))
            {
                TimeLimit = data.timeLimit,
                Difficulty = data.difficulty
            };

            foreach (var blockData in data.blocks)
            {
                var block = new EditableBlock(blockData.color) { Axis = blockData.axis, Ice = blockData.ice };
                foreach (var cell in blockData.cells)
                {
                    block.Cells.Add(new GridPoint(blockData.x + cell.x, blockData.y + cell.y));
                }

                level.Blocks.Add(block);
            }

            foreach (var door in data.doors)
            {
                var slots = level.doorSlots[door.side];
                for (var i = door.start; i < door.start + door.length; i++)
                {
                    if (i >= 0 && i < slots.Length)
                    {
                        slots[i] = door.color;
                    }
                }
            }

            return level;
        }

        public LevelData ToData()
        {
            var blocks = new List<BlockData>();
            foreach (var block in Blocks)
            {
                if (block.Cells.Count == 0)
                {
                    continue;
                }

                var minX = int.MaxValue;
                var minY = int.MaxValue;
                foreach (var cell in block.Cells)
                {
                    minX = Math.Min(minX, cell.X);
                    minY = Math.Min(minY, cell.Y);
                }

                var cells = new CellData[block.Cells.Count];
                for (var i = 0; i < cells.Length; i++)
                {
                    cells[i] = new CellData(block.Cells[i].X - minX, block.Cells[i].Y - minY);
                }

                blocks.Add(new BlockData { color = block.Color, x = minX, y = minY, cells = cells, axis = block.Axis, ice = block.Ice });
            }

            var doors = new List<DoorData>();
            foreach (var pair in doorSlots)
            {
                var slots = pair.Value;
                for (var i = 0; i < slots.Length;)
                {
                    if (slots[i] == NoDoor)
                    {
                        i++;
                        continue;
                    }

                    var start = i;
                    while (i < slots.Length && slots[i] == slots[start])
                    {
                        i++;
                    }

                    doors.Add(new DoorData { side = pair.Key, start = start, length = i - start, color = slots[start] });
                }
            }

            return new LevelData
            {
                width = Width,
                height = Height,
                timeLimit = TimeLimit,
                difficulty = Difficulty,
                blocks = blocks.ToArray(),
                doors = doors.ToArray()
            };
        }

        public int SlotCount(BoardSide side) => side is BoardSide.Bottom or BoardSide.Top ? Width : Height;

        public int GetDoor(BoardSide side, int slot) => doorSlots[side][slot];

        public void SetDoor(BoardSide side, int slot, int color) => doorSlots[side][slot] = color;

        public bool IsInside(GridPoint cell) => cell.X >= 0 && cell.Y >= 0 && cell.X < Width && cell.Y < Height;

        public int BlockAt(GridPoint cell)
        {
            for (var i = 0; i < Blocks.Count; i++)
            {
                if (Blocks[i].Covers(cell))
                {
                    return i;
                }
            }

            return -1;
        }

        public bool Fits(IEnumerable<GridPoint> cells, int ignored = -1)
        {
            foreach (var cell in cells)
            {
                if (!IsInside(cell))
                {
                    return false;
                }

                var owner = BlockAt(cell);
                if (owner >= 0 && owner != ignored)
                {
                    return false;
                }
            }

            return true;
        }

        public void Resize(int width, int height)
        {
            Width = Math.Clamp(width, MinSize, MaxSize);
            Height = Math.Clamp(height, MinSize, MaxSize);
            Blocks.RemoveAll(block => !block.Cells.TrueForAll(IsInside));

            foreach (BoardSide side in Enum.GetValues(typeof(BoardSide)))
            {
                var old = doorSlots[side];
                var slots = NewSlots(SlotCount(side));
                Array.Copy(old, slots, Math.Min(old.Length, slots.Length));
                doorSlots[side] = slots;
            }
        }

        private static int[] NewSlots(int count)
        {
            var slots = new int[count];
            Array.Fill(slots, NoDoor);
            return slots;
        }
    }
}
