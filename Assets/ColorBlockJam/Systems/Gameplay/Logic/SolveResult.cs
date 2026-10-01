using System.Collections.Generic;

namespace ColorBlockJam.Gameplay.Logic
{
    public sealed class SolveResult
    {
        public SolveResult(bool isSolved, bool isExhausted, IReadOnlyList<SolverMove> moves, int repositions)
        {
            IsSolved = isSolved;
            IsExhausted = isExhausted;
            Moves = moves;
            Repositions = repositions;
        }

        public bool IsSolved { get; }

        public bool IsExhausted { get; }

        public IReadOnlyList<SolverMove> Moves { get; }

        public int Repositions { get; }

        public bool IsStuck => !IsSolved && IsExhausted;
    }
}
