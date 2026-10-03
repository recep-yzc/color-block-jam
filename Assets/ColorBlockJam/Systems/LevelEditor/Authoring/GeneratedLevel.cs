using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;

namespace ColorBlockJam.LevelEditor.Authoring
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
