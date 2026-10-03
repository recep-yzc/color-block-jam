using System.Collections.Generic;
using System.Threading;

namespace ColorBlockJam.Gameplay.Logic
{
    public sealed class BoardSolver
    {
        private const int InitialStates = 256;

        public SolveResult Solve(Board board, int maxStates, CancellationToken cancellationToken = default)
        {
            var blocks = board.Blocks;
            var expandReach = new ReachMap();
            var leaveReach = new ReachMap();
            var scratchMoves = new List<SolverMove>();
            var allMoves = new List<SolverMove>();
            var parents = new List<int>();
            var depths = new List<int>();
            var moveStarts = new List<int>();
            var positions = new GridPoint[blocks.Count];
            var cleared = new bool[blocks.Count];
            var originalPositions = new GridPoint[blocks.Count];
            var originalCleared = new bool[blocks.Count];
            for (var i = 0; i < blocks.Count; i++)
            {
                originalPositions[i] = blocks[i].Position;
                originalCleared[i] = blocks[i].IsCleared;
            }

            var states = new SolverStates(blocks.Count, InitialStates);
            var visited = new HashSet<int>(states);

            LeaveAll(board, leaveReach, scratchMoves);
            states.Write(board);
            visited.Add(states.Commit());
            parents.Add(-1);
            depths.Add(0);
            moveStarts.Add(0);
            allMoves.AddRange(scratchMoves);

            var queue = new Queue<int>();
            queue.Enqueue(0);
            var solvedIndex = board.IsCleared ? 0 : -1;
            var isOverBudget = false;

            while (solvedIndex < 0 && queue.Count > 0 && !isOverBudget && !cancellationToken.IsCancellationRequested)
            {
                var index = queue.Dequeue();
                states.Restore(index, board, positions, cleared);

                for (var b = 0; b < blocks.Count && solvedIndex < 0; b++)
                {
                    var block = blocks[b];
                    if (block.IsCleared || board.IsFrozen(block))
                    {
                        continue;
                    }

                    var origin = block.Position;
                    expandReach.Fill(board, block);
                    for (var r = 1; r < expandReach.Count; r++)
                    {
                        var target = expandReach[r];
                        board.Move(block, target);

                        states.Write(board);
                        if (visited.Contains(states.Next))
                        {
                            board.Move(block, origin);
                            continue;
                        }

                        var remaining = board.RemainingBlocks;
                        scratchMoves.Clear();
                        expandReach.AddPath(block.Id, target, scratchMoves);
                        LeaveAll(board, leaveReach, scratchMoves);

                        states.Write(board);
                        if (visited.Add(states.Next))
                        {
                            queue.Enqueue(states.Commit());
                            parents.Add(index);
                            depths.Add(depths[index] + 1);
                            moveStarts.Add(allMoves.Count);
                            allMoves.AddRange(scratchMoves);
                            if (board.IsCleared)
                            {
                                solvedIndex = states.Count - 1;
                                break;
                            }
                        }

                        if (board.RemainingBlocks == remaining)
                        {
                            board.Move(block, origin);
                        }
                        else
                        {
                            states.Restore(index, board, positions, cleared);
                        }
                    }
                }

                isOverBudget = states.Count >= maxStates;
            }

            board.SetState(originalPositions, originalCleared);

            var isSolved = solvedIndex >= 0;
            var isExhausted = !isSolved && !isOverBudget && !cancellationToken.IsCancellationRequested;
            var repositions = isSolved ? depths[solvedIndex] : 0;
            return new SolveResult(isSolved, isExhausted, Solution(solvedIndex, parents, moveStarts, allMoves), repositions);
        }

        private static List<SolverMove> Solution(int solvedIndex, List<int> parents, List<int> moveStarts, List<SolverMove> allMoves)
        {
            var chain = new List<int>();
            for (var i = solvedIndex; i >= 0; i = parents[i])
            {
                chain.Add(i);
            }

            var solution = new List<SolverMove>();
            for (var c = chain.Count - 1; c >= 0; c--)
            {
                var node = chain[c];
                var end = node + 1 < moveStarts.Count ? moveStarts[node + 1] : allMoves.Count;
                for (var m = moveStarts[node]; m < end; m++)
                {
                    solution.Add(allMoves[m]);
                }
            }

            return solution;
        }

        private static void LeaveAll(Board board, ReachMap reach, List<SolverMove> moves)
        {
            bool hasLeft;
            var blocks = board.Blocks;
            do
            {
                hasLeft = false;
                for (var i = 0; i < blocks.Count; i++)
                {
                    if (!blocks[i].IsCleared && !board.IsFrozen(blocks[i]) && TryLeave(board, blocks[i], reach, moves))
                    {
                        hasLeft = true;
                    }
                }
            }
            while (hasLeft && !board.IsCleared);
        }

        private static bool TryLeave(Board board, BoardBlock block, ReachMap reach, List<SolverMove> moves)
        {
            reach.Fill(board, block);
            for (var r = 0; r < reach.Count; r++)
            {
                var from = reach[r];
                var door = BlockPlacement.DoorToEnter(board, block, from);
                if (door == null)
                {
                    continue;
                }

                reach.AddPath(block.Id, from, moves);
                moves.Add(new SolverMove(block.Id, from, door.ExitDirection, exits: true));
                board.Move(block, from);
                board.Clear(block);
                return true;
            }

            return false;
        }
    }
}
