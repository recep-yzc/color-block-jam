using System;

namespace ColorBlockJam.Level
{
    [Serializable]
    public sealed class DoorData
    {
        public BoardSide side;
        public int start;
        public int length = 1;
        public int color;
    }
}
