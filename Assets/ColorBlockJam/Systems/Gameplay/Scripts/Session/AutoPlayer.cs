using System;
using System.Threading;
using ColorBlockJam.Gameplay.Logic;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Gameplay
{
    public sealed class AutoPlayer
    {
        private readonly BoardSolver solver;
        private readonly GameplayConfig config;
        private readonly LevelBoard levelBoard;

        public AutoPlayer(BoardSolver solver, GameplayConfig config, LevelBoard levelBoard)
        {
            this.solver = solver;
            this.config = config;
            this.levelBoard = levelBoard;
        }

        public async UniTask<bool> PlayAsync(CancellationToken cancellationToken)
        {
            var board = levelBoard.Board;
            var snapshot = board.Clone();
            var result = await UniTask.RunOnThreadPool(() => solver.Solve(snapshot, config.AutoPlaySearchBudget, cancellationToken),
                cancellationToken: cancellationToken);
            if (!result.IsSolved)
            {
                return false;
            }

            var pause = TimeSpan.FromSeconds(config.AutoPlayPause);
            foreach (var move in result.Moves)
            {
                var block = board.Blocks[move.BlockId];
                if (move.Exits)
                {
                    levelBoard.LeaveFrom(block, move.Target, move.Direction);
                }
                else
                {
                    await levelBoard.SlideAsync(block, move.Target, cancellationToken);
                }

                await UniTask.Delay(pause, cancellationToken: cancellationToken);
            }

            return true;
        }
    }
}
