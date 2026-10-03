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
        private Answer answer;
        private CancellationTokenSource search;

        public SolvabilityWatcher(BoardSolver solver, GameplayConfig config)
        {
            this.solver = solver;
            this.config = config;
        }

        public event Action FoundUnsolvable;

        public bool IsUnsolvable => answer == Answer.Unsolvable;

        public void Check(Board board)
        {
            if (answer == Answer.Unknown && search == null)
            {
                SearchAsync(board.Clone()).Forget();
            }
        }

        public void Recheck(Board board)
        {
            if (answer == Answer.Solvable)
            {
                return;
            }

            CancelSearch();
            answer = Answer.Unknown;
            Check(board);
        }

        public void Dispose()
        {
            CancelSearch();
        }

        private async UniTaskVoid SearchAsync(Board snapshot)
        {
            var source = new CancellationTokenSource();
            search = source;
            var token = source.Token;
            var (isCanceled, result) = await UniTask.RunOnThreadPool(() => solver.Solve(snapshot, config.StuckSearchBudget, token),
                cancellationToken: token).SuppressCancellationThrow();
            if (search == source)
            {
                search = null;
            }

            source.Dispose();
            if (isCanceled || token.IsCancellationRequested)
            {
                return;
            }

            answer = result.IsSolved ? Answer.Solvable : result.IsStuck ? Answer.Unsolvable : Answer.Unknown;
            if (answer == Answer.Unsolvable)
            {
                FoundUnsolvable?.Invoke();
            }
        }

        private void CancelSearch()
        {
            if (search == null)
            {
                return;
            }

            search.Cancel();
            search = null;
        }
    }
}
