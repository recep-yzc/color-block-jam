using System;
using System.Threading;
using ColorBlockJam.Gameplay.Logic;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Gameplay
{
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

        public event Action FoundUnsolvable;

        public bool IsUnsolvable => answer == Answer.Unsolvable;

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
