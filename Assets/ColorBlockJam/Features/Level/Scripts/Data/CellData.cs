using System;

namespace ColorBlockJam.Level
{
    [Serializable]
    public struct CellData
    {
        public int x;
        public int y;

        public CellData(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
