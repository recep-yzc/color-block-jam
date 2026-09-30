using ColorBlockJam.Level;

namespace ColorBlockJam.Gameplay.Logic
{
    public sealed class GeneratedLevel
    {
        public GeneratedLevel(LevelData level, SolveResult solution)
        {
            Level = level;
            Solution = solution;
        }

        public LevelData Level { get; }
        public SolveResult Solution { get; }
    }
}
