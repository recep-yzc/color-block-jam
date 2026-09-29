using ColorBlockJam.Level;

namespace ColorBlockJam.Gameplay.Logic
{
    /// <summary>
    /// A door on one side of the board. The cells beyond it are open for blocks of its color,
    /// so a block that fits the door can slide out of the board through it.
    /// </summary>
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

        /// <summary>The direction a block moves to leave the board through this door.</summary>
        public Direction ExitDirection => Side switch
        {
            BoardSide.Bottom => Direction.Down,
            BoardSide.Top => Direction.Up,
            BoardSide.Left => Direction.Left,
            _ => Direction.Right
        };

        /// <summary>True when <paramref name="alongEdge"/> (x for top and bottom doors, y for side doors) is inside the door.</summary>
        public bool Covers(int alongEdge)
        {
            return alongEdge >= Start && alongEdge < Start + Length;
        }
    }
}
