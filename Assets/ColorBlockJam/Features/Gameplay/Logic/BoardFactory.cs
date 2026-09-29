using ColorBlockJam.Level;

namespace ColorBlockJam.Gameplay.Logic
{
    public static class BoardFactory
    {
        /// <summary>Builds a fresh board from a level. Block ids are their index in the level.</summary>
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

                blocks[i] = new BoardBlock(i, data.color, new GridPoint(data.x, data.y), cells);
            }

            var doors = new BoardDoor[level.doors.Length];
            for (var i = 0; i < doors.Length; i++)
            {
                var data = level.doors[i];
                doors[i] = new BoardDoor(data.side, data.start, data.length, data.color);
            }

            return new Board(level.width, level.height, blocks, doors);
        }
    }
}
