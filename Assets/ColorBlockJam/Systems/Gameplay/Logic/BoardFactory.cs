using System;
using ColorBlockJam.Level;

namespace ColorBlockJam.Gameplay.Logic
{
    public static class BoardFactory
    {
        public static Board Create(LevelData level)
        {
            var blocks = new BoardBlock[level.blocks.Length];
            for (var i = 0; i < blocks.Length; i++)
            {
                var data = level.blocks[i];
                var cells = new GridPoint[data.cells.Length];
                for (var c = 0; c < cells.Length; c++)
                {
                    cells[c] = new GridPoint(data.cells[c].x, data.cells[c].y);
                }

                blocks[i] = new BoardBlock(i, data.color, new GridPoint(data.x, data.y), cells, data.axis, data.ice);
            }

            var doors = new BoardDoor[level.doors.Length];
            for (var i = 0; i < doors.Length; i++)
            {
                var data = level.doors[i];
                doors[i] = new BoardDoor(data.side, data.start, data.length, data.color);
            }

            return new Board(level.width, level.height, blocks, doors, HolesOf(level));
        }

        public static GridPoint[] HolesOf(LevelData level)
        {
            var data = level.holes ?? Array.Empty<CellData>();
            var holes = new GridPoint[data.Length];
            for (var i = 0; i < holes.Length; i++)
            {
                holes[i] = new GridPoint(data[i].x, data[i].y);
            }

            return holes;
        }
    }
}
