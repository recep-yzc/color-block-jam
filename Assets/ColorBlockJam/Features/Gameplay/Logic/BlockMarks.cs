using System;
using ColorBlockJam.Level;

namespace ColorBlockJam.Gameplay.Logic
{
    public readonly struct ArrowRun
    {
        public ArrowRun(float centerX, float centerY, int length)
        {
            CenterX = centerX;
            CenterY = centerY;
            Length = length;
        }

        public float CenterX { get; }
        public float CenterY { get; }

        public int Length { get; }
    }

    public static class BlockMarks
    {
        public static ArrowRun FindArrow(ReadOnlySpan<GridPoint> cells, BlockAxis axis)
        {
            if (axis == BlockAxis.Free || cells.IsEmpty)
            {
                return default;
            }

            var isHorizontal = axis == BlockAxis.Horizontal;
            GetMiddle(cells, out var middleX, out var middleY);
            var best = default(ArrowRun);
            var bestWidth = 0;
            var bestDistance = float.MaxValue;

            foreach (var cell in cells)
            {
                var along = isHorizontal ? cell.X : cell.Y;
                var across = isHorizontal ? cell.Y : cell.X;
                if (Has(cells, along - 1, across, isHorizontal))
                {
                    continue;
                }

                var end = along;
                while (Has(cells, end + 1, across, isHorizontal))
                {
                    end++;
                }

                var low = across;
                while (Spans(cells, along, end, low - 1, isHorizontal))
                {
                    low--;
                }

                var high = across;
                while (Spans(cells, along, end, high + 1, isHorizontal))
                {
                    high++;
                }

                var alongCenter = (along + end + 1) * 0.5f;
                var acrossCenter = (low + high + 1) * 0.5f;
                var run = isHorizontal
                    ? new ArrowRun(alongCenter, acrossCenter, end - along + 1)
                    : new ArrowRun(acrossCenter, alongCenter, end - along + 1);
                var width = high - low + 1;
                var distance = Square(run.CenterX - middleX) + Square(run.CenterY - middleY);
                if (run.Length > best.Length ||
                    (run.Length == best.Length && (width > bestWidth || (width == bestWidth && distance < bestDistance))))
                {
                    best = run;
                    bestWidth = width;
                    bestDistance = distance;
                }
            }

            return best;
        }

        public static GridPoint FindIceCell(ReadOnlySpan<GridPoint> cells)
        {
            GetMiddle(cells, out var middleX, out var middleY);
            var nearest = default(GridPoint);
            var nearestDistance = float.MaxValue;
            foreach (var cell in cells)
            {
                var distance = Square(cell.X + 0.5f - middleX) + Square(cell.Y + 0.5f - middleY);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = cell;
                }
            }

            return nearest;
        }

        private static void GetMiddle(ReadOnlySpan<GridPoint> cells, out float x, out float y)
        {
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            foreach (var cell in cells)
            {
                minX = Math.Min(minX, cell.X);
                minY = Math.Min(minY, cell.Y);
                maxX = Math.Max(maxX, cell.X);
                maxY = Math.Max(maxY, cell.Y);
            }

            x = (minX + maxX + 1) * 0.5f;
            y = (minY + maxY + 1) * 0.5f;
        }

        private static bool Spans(ReadOnlySpan<GridPoint> cells, int from, int to, int across, bool isHorizontal)
        {
            for (var along = from; along <= to; along++)
            {
                if (!Has(cells, along, across, isHorizontal))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool Has(ReadOnlySpan<GridPoint> cells, int along, int across, bool isHorizontal)
        {
            var cell = isHorizontal ? new GridPoint(along, across) : new GridPoint(across, along);
            foreach (var other in cells)
            {
                if (other == cell)
                {
                    return true;
                }
            }

            return false;
        }

        private static float Square(float value) => value * value;
    }
}
