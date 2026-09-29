using System;
using System.Collections.Generic;
using System.Threading;
using ColorBlockJam.Gameplay.Logic;
using Cysharp.Threading.Tasks;
using NVector2 = System.Numerics.Vector2;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// Plays the level by itself: asks the solver for a solution and performs its moves one by one.
    /// </summary>
    public sealed class AutoPlayer
    {
        private readonly BoardSolver solver;
        private readonly GameplayConfig config;

        public AutoPlayer(BoardSolver solver, GameplayConfig config)
        {
            this.solver = solver;
            this.config = config;
        }

        /// <returns>False when the solver finds no way to clear the board from its current state.</returns>
        public async UniTask<bool> PlayAsync(Board board, IReadOnlyList<BlockView> views, Action<BoardBlock, BoardDoor> onLeft,
            CancellationToken cancellationToken)
        {
            var result = solver.Solve(board, config.AutoPlaySearchBudget);
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
                    await LeaveAsync(board, block, view, move, onLeft, cancellationToken);
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

        private static async UniTask LeaveAsync(Board board, BoardBlock block, BlockView view, SolverMove move,
            Action<BoardBlock, BoardDoor> onLeft, CancellationToken cancellationToken)
        {
            // The exit target is one step past the last cell on the board; slide there first.
            var lastInside = move.Target - move.Direction.ToOffset();
            if (lastInside != block.Position)
            {
                await view.SlideAsync(lastInside, cancellationToken);
                board.Move(block, lastInside);
            }

            BlockPlacement.DepthThroughDoor(board, block, new NVector2(move.Target.X, move.Target.Y), out var door);
            board.Clear(block);
            var distance = BlockPlacement.LengthThroughDoor(block, door.Side) + 0.5f;
            view.ExitAsync(move.Direction, distance, cancellationToken).Forget();
            onLeft(block, door);
        }
    }
}
