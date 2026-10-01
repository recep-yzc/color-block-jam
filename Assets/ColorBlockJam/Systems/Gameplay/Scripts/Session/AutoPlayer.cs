using System;
using System.Collections.Generic;
using System.Threading;
using ColorBlockJam.Gameplay.Logic;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Gameplay
{
    public sealed class AutoPlayer
    {
        private readonly BoardSolver solver;
        private readonly GameplayConfig config;

        public AutoPlayer(BoardSolver solver, GameplayConfig config)
        {
            this.solver = solver;
            this.config = config;
        }

        public async UniTask<bool> PlayAsync(Board board, IReadOnlyList<BlockView> views, Action<BoardBlock, BoardDoor> onLeft,
            CancellationToken cancellationToken)
        {
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
                var view = views[move.BlockId];

                if (move.Exits)
                {
                    Leave(board, block, view, move, onLeft, cancellationToken);
                }
                else
                {
                    await view.SlideAsync(move.Target, cancellationToken);
                    board.Move(block, move.Target);
                }

                await UniTask.Delay(pause, cancellationToken: cancellationToken);
            }

            return true;
        }

        private static void Leave(Board board, BoardBlock block, BlockView view, SolverMove move,
            Action<BoardBlock, BoardDoor> onLeft, CancellationToken cancellationToken)
        {
            var door = BlockPlacement.ExitDoor(board, block, move.Target, move.Direction);
            var steps = BlockPlacement.StepsToLeave(board, block, move.Target, move.Direction);
            board.Clear(block);
            view.LeaveFromAsync(move.Target, move.Direction, steps, cancellationToken).Forget();
            onLeft(block, door);
        }
    }
}
