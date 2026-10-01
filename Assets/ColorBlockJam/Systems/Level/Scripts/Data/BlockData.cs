using System;

namespace ColorBlockJam.Level
{
    [Serializable]
    public sealed class BlockData
    {
        public int color;
        public int x;
        public int y;
        public CellData[] cells = Array.Empty<CellData>();
        public BlockAxis axis;
        public int ice;
    }
}
