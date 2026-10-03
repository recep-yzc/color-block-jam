using System.Collections.Generic;

namespace ColorBlockJam.Gameplay.Logic
{
    public sealed class SolveResult
    {
        public SolveResult(bool isSolved, bool isExhausted, IReadOnlyList<SolverMove> moves, int repositions)
        {
            IsSolved = isSolved;
            IsStuck = !isSolved && isExhausted;
            Moves = moves;
            Repositions = repositions;
        }

        public bool IsSolved { get; }

        public IReadOnlyList<SolverMove> Moves { get; }

        public int Repositions { get; }

        public bool IsStuck { get; }
    }
}
