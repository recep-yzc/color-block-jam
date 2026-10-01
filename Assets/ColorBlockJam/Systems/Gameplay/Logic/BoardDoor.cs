using ColorBlockJam.Level;

namespace ColorBlockJam.Gameplay.Logic
{
    public sealed class BoardDoor
    {
        public BoardDoor(BoardSide side, int start, int length, int color)
        {
            Side = side;
            Start = start;
            Length = length;
            Color = color;
        }

        public BoardSide Side { get; }
        public int Start { get; }
        public int Length { get; }
        public int Color { get; }

        public Direction ExitDirection => ExitDirectionOf(Side);

        public static Direction ExitDirectionOf(BoardSide side) => side switch
        {
            BoardSide.Bottom => Direction.Down,
            BoardSide.Top => Direction.Up,
            BoardSide.Left => Direction.Left,
            _ => Direction.Right
        };

        public bool Covers(int alongEdge)
        {
            return alongEdge >= Start && alongEdge < Start + Length;
        }
    }
}
