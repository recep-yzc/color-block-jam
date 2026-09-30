using System;
using System.Threading;
using ColorBlockJam.Gameplay.Logic;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// Knows whether the board can still be cleared. Moves can be undone and leaving only frees space, so the answer
    /// never changes during play: the solver is asked once, on a copy of the board on a worker thread, and asked again
    /// only while the answer is unknown, since a board with fewer blocks is a smaller search.
    /// </summary>
    public sealed class SolvabilityWatcher : IDisposable
    {
        private enum Answer
        {
            Unknown,
            Solvable,
            Unsolvable
        }

        private readonly BoardSolver solver;
        private readonly GameplayConfig config;
        private readonly CancellationTokenSource lifetime = new();
        private Answer answer;
        private bool isSearching;

        public SolvabilityWatcher(BoardSolver solver, GameplayConfig config)
        {
            this.solver = solver;
            this.config = config;
        }

        /// <summary>Raised when the solver finds that the board cannot be cleared.</summary>
        public event Action FoundUnsolvable;

        public bool IsUnsolvable => answer == Answer.Unsolvable;

        /// <summary>Starts a search of the board as it is now, unless the answer is known or a search is running.</summary>
        public void Check(Board board)
        {
            if (answer == Answer.Unknown && !isSearching)
            {
                SearchAsync(board.Clone()).Forget();
            }
        }

        public void Dispose()
        {
            lifetime.Cancel();
            lifetime.Dispose();
        }

        private async UniTaskVoid SearchAsync(Board snapshot)
        {
            isSearching = true;
            var token = lifetime.Token;
            var (isCanceled, result) = await UniTask.RunOnThreadPool(() => solver.Solve(snapshot, config.StuckSearchBudget, token),
                cancellationToken: token).SuppressCancellationThrow();
            isSearching = false;

            if (isCanceled)
            {
                return;
            }

            answer = result.IsSolved ? Answer.Solvable : result.IsStuck ? Answer.Unsolvable : Answer.Unknown;
            if (answer == Answer.Unsolvable)
            {
                FoundUnsolvable?.Invoke();
            }
        }
    }
}
