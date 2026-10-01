namespace ColorBlockJam.Gameplay.Logic
{
    public readonly struct LevelProblem
    {
        public readonly LevelProblemKind Kind;
        public readonly int Block;
        public readonly int Color;

        public LevelProblem(LevelProblemKind kind, int block = -1, int color = -1)
        {
            Kind = kind;
            Block = block;
            Color = color;
        }
    }
}
