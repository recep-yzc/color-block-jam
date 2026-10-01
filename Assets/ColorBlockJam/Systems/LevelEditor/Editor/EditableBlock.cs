using System.Collections.Generic;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;

namespace ColorBlockJam.LevelEditor
{
    internal sealed class EditableBlock
    {
        public int Color;
        public BlockAxis Axis;
        public int Ice;
        public readonly List<GridPoint> Cells = new();

        public EditableBlock(int color)
        {
            Color = color;
        }

        public bool Covers(GridPoint cell) => Cells.Contains(cell);
    }
}
