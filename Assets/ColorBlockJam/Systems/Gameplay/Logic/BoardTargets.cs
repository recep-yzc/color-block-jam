using System.Collections.Generic;

namespace ColorBlockJam.Gameplay.Logic
{
    public static class BoardTargets
    {
        public static List<BoardBlock> OfColor(Board board, int color)
        {
            var found = new List<BoardBlock>();
            foreach (var block in board.Blocks)
            {
                if (!block.IsCleared && block.Color == color)
                {
                    found.Add(block);
                }
            }

            return found;
        }

        public static List<BoardBlock> InRow(Board board, int row)
        {
            var found = new List<BoardBlock>();
            foreach (var block in board.Blocks)
            {
                if (!block.IsCleared && CoversRow(block, row))
                {
                    found.Add(block);
                }
            }

            return found;
        }

        private static bool CoversRow(BoardBlock block, int row)
        {
            foreach (var cell in block.Cells)
            {
                if (block.Position.Y + cell.Y == row)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
