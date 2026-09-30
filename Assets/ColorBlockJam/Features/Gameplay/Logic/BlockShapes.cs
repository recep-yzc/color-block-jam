using System.Collections.Generic;

namespace ColorBlockJam.Gameplay.Logic
{
    public static class BlockShapes
    {
        public static readonly IReadOnlyList<GridPoint[]> Small = new[]
        {
            Shape((0, 0)),
            Shape((0, 0), (1, 0)),
            Shape((0, 0), (0, 1)),
            Shape((0, 0), (1, 0), (0, 1), (1, 1)),
            Shape((0, 0), (1, 0), (0, 1)),
            Shape((0, 0), (1, 0), (1, 1)),
        };

        public static readonly IReadOnlyList<GridPoint[]> Long = new[]
        {
            Shape((0, 0), (1, 0), (2, 0)),
            Shape((0, 0), (0, 1), (0, 2)),
            Shape((0, 0), (0, 1), (1, 1)),
            Shape((1, 0), (0, 1), (1, 1)),
            Shape((0, 0), (0, 1), (0, 2), (1, 0)),
            Shape((0, 0), (1, 0), (2, 0), (2, 1)),
        };

        public static readonly IReadOnlyList<GridPoint[]> Complex = new[]
        {
            Shape((0, 0), (1, 0), (2, 0), (1, 1)),
            Shape((0, 0), (0, 1), (0, 2), (1, 1)),
            Shape((0, 0), (1, 0), (1, 1), (1, 2)),
            Shape((0, 1), (1, 1), (2, 1), (0, 0)),
            Shape((0, 0), (1, 0), (2, 0), (0, 1), (1, 1), (2, 1)),
        };

        private static GridPoint[] Shape(params (int x, int y)[] cells)
        {
            var points = new GridPoint[cells.Length];
            for (var i = 0; i < cells.Length; i++)
            {
                points[i] = new GridPoint(cells[i].x, cells[i].y);
            }

            return points;
        }
    }
}
