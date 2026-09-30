namespace ColorBlockJam.Gameplay.Logic
{
    public readonly struct SolverMove
    {
        public readonly int BlockId;
        public readonly GridPoint Target;
        public readonly Direction Direction;
        public readonly bool Exits;

        public SolverMove(int blockId, GridPoint target, Direction direction, bool exits)
        {
            BlockId = blockId;
            Target = target;
            Direction = direction;
            Exits = exits;
        }
    }
}
