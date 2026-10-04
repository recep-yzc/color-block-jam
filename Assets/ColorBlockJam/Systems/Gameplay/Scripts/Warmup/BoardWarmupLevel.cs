using ColorBlockJam.Level;

namespace ColorBlockJam.Gameplay
{
    public static class BoardWarmupLevel
    {
        public static LevelData Create()
        {
            return new LevelData
            {
                width = 3,
                height = 2,
                blocks = new[]
                {
                    new BlockData { color = 0, cells = new[] { new CellData(0, 0) } },
                    new BlockData { color = 1, y = 1, cells = new[] { new CellData(0, 0) }, axis = BlockAxis.Horizontal, ice = 1 }
                },
                doors = new[]
                {
                    new DoorData { side = BoardSide.Left, start = 0, color = 0 },
                    new DoorData { side = BoardSide.Right, start = 1, color = 1 }
                }
            };
        }
    }
}
