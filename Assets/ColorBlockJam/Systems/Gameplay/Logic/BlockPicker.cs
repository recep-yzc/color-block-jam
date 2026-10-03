using System;
using System.Numerics;

namespace ColorBlockJam.Gameplay.Logic
{
    public static class BlockPicker
    {
        public static BoardBlock Pick(Board board, Vector2 point, float padding, out GridPoint cell)
        {
            cell = new GridPoint((int)MathF.Floor(point.X), (int)MathF.Floor(point.Y));
            var pressed = board.BlockAt(cell);
            if (pressed != null)
            {
                return pressed;
            }

            BoardBlock nearest = null;
            var nearestDistance = padding * padding;
            var blocks = board.Blocks;
            for (var i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];
                foreach (var offset in block.Cells)
                {
                    var blockCell = new GridPoint(block.Position.X + offset.X, block.Position.Y + offset.Y);
                    if (board.BlockAt(blockCell) != block)
                    {
                        continue;
                    }

                    var distance = DistanceSquared(point, blockCell);
                    if (distance <= nearestDistance && (nearest == null || distance < nearestDistance))
                    {
                        nearest = block;
                        nearestDistance = distance;
                        cell = blockCell;
                    }
                }
            }

            return nearest;
        }

        private static float DistanceSquared(Vector2 point, GridPoint cell)
        {
            var dx = MathF.Max(MathF.Max(cell.X - point.X, point.X - (cell.X + 1)), 0f);
            var dy = MathF.Max(MathF.Max(cell.Y - point.Y, point.Y - (cell.Y + 1)), 0f);
            return dx * dx + dy * dy;
        }
    }
}
