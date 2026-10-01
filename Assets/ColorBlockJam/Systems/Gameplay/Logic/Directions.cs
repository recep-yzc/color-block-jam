namespace ColorBlockJam.Gameplay.Logic
{
    public static class Directions
    {
        public static readonly Direction[] All = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };

        public static GridPoint ToOffset(this Direction direction)
        {
            return direction switch
            {
                Direction.Up => new GridPoint(0, 1),
                Direction.Down => new GridPoint(0, -1),
                Direction.Left => new GridPoint(-1, 0),
                _ => new GridPoint(1, 0)
            };
        }
    }
}
