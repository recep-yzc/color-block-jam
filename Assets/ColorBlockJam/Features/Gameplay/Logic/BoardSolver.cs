using System.Collections.Generic;
using System.Threading;
using ColorBlockJam.Level;

namespace ColorBlockJam.Gameplay.Logic
{
    /// <summary>
    /// One straight slide of a block to <see cref="Target"/>, or, when <see cref="Exits"/> is set,
    /// a slide from <see cref="Target"/> in <see cref="Direction"/> out through its door.
    /// </summary>
    public readonly struct SolverMove
    {
        public readonly int BlockId;
        public readonly GridPoint Target;
        public readonly Direction Direction;
        public readonly bool Exits;

        public SolverMove(int blockId, GridPoint target, Direction direction, bool exits)
        {
            BlockId = blockId;
            Target = target;
            Direction = direction;
            Exits = exits;
        }
    }

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

        /// <summary>True when every reachable state was searched, so an unsolved result is certain.</summary>
        public bool IsExhausted { get; }

        /// <summary>The solution as straight slides, ready to be played back.</summary>
        public IReadOnlyList<SolverMove> Moves { get; }

        /// <summary>
        /// How many times a block has to be moved out of the way, not toward its door, in the best solution.
        /// Zero means every block can leave as it is, in some order. This is the level's difficulty.
        /// </summary>
        public int Repositions { get; }

        /// <summary>No sequence of moves clears the board from here.</summary>
        public bool IsStuck => !IsSolved && IsExhausted;
    }

    /// <summary>
    /// Finds the fewest block repositions that clear the board. Frozen blocks stay where they are until enough blocks
    /// have left, and arrow blocks only move along their axis.
    /// Leaving is never a bad move, because a removed block only frees space; so before every step all blocks that can
    /// reach their door leave. A step then moves one remaining block to any cell it can reach with the others still.
    /// The search is breadth-first over those steps, so the found solution needs the fewest repositions.
    /// Every move can be undone and leaving never hurts, so solvability never changes during play:
    /// a stuck result means the board was never solvable.
    /// The board is left in the state it had before the call. The solver keeps no state of its own, so one instance
    /// can search several boards at once, for example a <see cref="Board.Clone"/> on a worker thread.
    /// </summary>
    public sealed class BoardSolver
    {
        private sealed class Node
        {
            public GridPoint[] Positions;
            public bool[] Cleared;
            public int Parent;
            public int Depth;
            public List<SolverMove> Moves;
        }

        /// <param name="cancellationToken">Stops the search early, for example when the level it was for is gone;
        /// the result is then neither solved nor exhausted.</param>
        public SolveResult Solve(Board board, int maxStates, CancellationToken cancellationToken = default)
        {
            // Buffers live per call, so parallel calls share nothing.
            var expandReach = new ReachMap();
            var leaveReach = new ReachMap();
            var scratchMoves = new List<SolverMove>();
            var blocks = board.Blocks;
            var original = Capture(board, parent: -1, depth: 0, moves: null);

            var startMoves = new List<SolverMove>();
            LeaveAll(board, leaveReach, startMoves);
            var start = Capture(board, parent: -1, depth: 0, startMoves);

            var nodes = new List<Node> { start };
            var visited = new HashSet<string> { Key(board) };
            var queue = new Queue<int>();
            queue.Enqueue(0);
            var solvedIndex = board.IsCleared ? 0 : -1;
            var isOverBudget = false;

            while (solvedIndex < 0 && queue.Count > 0 && !isOverBudget && !cancellationToken.IsCancellationRequested)
            {
                var index = queue.Dequeue();
                var node = nodes[index];
                board.SetState(node.Positions, node.Cleared);

                for (var b = 0; b < blocks.Count && solvedIndex < 0; b++)
                {
                    var block = blocks[b];
                    if (block.IsCleared || board.IsFrozen(block))
                    {
                        continue;
                    }

                    expandReach.Fill(board, block);
                    for (var r = 1; r < expandReach.Count; r++)
                    {
                        var target = expandReach[r];
                        board.Move(block, target);
                        scratchMoves.Clear();
                        expandReach.AddPath(block.Id, target, scratchMoves);
                        LeaveAll(board, leaveReach, scratchMoves);

                        // Only states not seen before keep a copy of their moves.
                        if (visited.Add(Key(board)))
                        {
                            nodes.Add(Capture(board, index, node.Depth + 1, new List<SolverMove>(scratchMoves)));
                            if (board.IsCleared)
                            {
                                solvedIndex = nodes.Count - 1;
                                break;
                            }

                            queue.Enqueue(nodes.Count - 1);
                        }

                        board.SetState(node.Positions, node.Cleared);
                    }
                }

                isOverBudget = nodes.Count >= maxStates;
            }

            board.SetState(original.Positions, original.Cleared);

            var chain = new List<Node>();
            for (var i = solvedIndex; i >= 0; i = nodes[i].Parent)
            {
                chain.Add(nodes[i]);
            }

            var solution = new List<SolverMove>();
            for (var i = chain.Count - 1; i >= 0; i--)
            {
                solution.AddRange(chain[i].Moves);
            }

            var isSolved = solvedIndex >= 0;
            var isExhausted = !isSolved && !isOverBudget && !cancellationToken.IsCancellationRequested;
            var repositions = isSolved ? nodes[solvedIndex].Depth : 0;
            return new SolveResult(isSolved, isExhausted, solution, repositions);
        }

        /// <summary>Lets every block that can reach its door leave, until none can.</summary>
        private static void LeaveAll(Board board, ReachMap reach, List<SolverMove> moves)
        {
            bool hasLeft;
            var blocks = board.Blocks;
            do
            {
                hasLeft = false;
                for (var i = 0; i < blocks.Count; i++)
                {
                    // A block that leaves can thaw another, so the loop goes round until nothing more leaves.
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
            var doors = board.Doors;
            for (var r = 0; r < reach.Count; r++)
            {
                var from = reach[r];
                for (var d = 0; d < doors.Count; d++)
                {
                    var door = doors[d];
                    // Every way out passes the cell where the block touches the door's side, so only those are tried.
                    if (door.Color != block.Color || !BlockPlacement.TouchesSide(board, block, from, door.Side) ||
                        !board.CanPassThrough(block, from, door.ExitDirection))
                    {
                        continue;
                    }

                    reach.AddPath(block.Id, from, moves);
                    moves.Add(new SolverMove(block.Id, from, door.ExitDirection, exits: true));
                    board.Move(block, from);
                    board.Clear(block);
                    return true;
                }
            }

            return false;
        }

        private static Node Capture(Board board, int parent, int depth, List<SolverMove> moves)
        {
            var blocks = board.Blocks;
            var node = new Node
            {
                Positions = new GridPoint[blocks.Count],
                Cleared = new bool[blocks.Count],
                Parent = parent,
                Depth = depth,
                Moves = moves
            };

            for (var i = 0; i < blocks.Count; i++)
            {
                node.Positions[i] = blocks[i].Position;
                node.Cleared[i] = blocks[i].IsCleared;
            }

            return node;
        }

        private static string Key(Board board)
        {
            // Where a cleared block left does not matter, so all its states share one key.
            var blocks = board.Blocks;
            var chars = new char[blocks.Count * 2];
            for (var i = 0; i < blocks.Count; i++)
            {
                var isCleared = blocks[i].IsCleared;
                chars[i * 2] = isCleared ? '#' : (char)(blocks[i].Position.X + 64);
                chars[i * 2 + 1] = isCleared ? '#' : (char)(blocks[i].Position.Y + 64);
            }

            return new string(chars);
        }

        /// <summary>
        /// Every board cell one block can reach with the other blocks where they are, in breadth-first order
        /// (its own cell first), with the step that reached each one. Reused between blocks to avoid allocations.
        /// </summary>
        private sealed class ReachMap
        {
            private const int Unvisited = -2;
            private const int Start = -1;

            private readonly List<GridPoint> order = new();
            private readonly List<(GridPoint to, Direction direction)> path = new();
            private int[] previous = new int[64];
            private Direction[] step = new Direction[64];
            private int originX;
            private int originY;
            private int columns;
            private int rows;

            public int Count => order.Count;
            public GridPoint this[int index] => order[index];

            public void Fill(Board board, BoardBlock block)
            {
                // Only positions with the whole block on the board are indexed.
                originX = -block.MinX;
                originY = -block.MinY;
                columns = board.Width - block.Width + 1;
                rows = board.Height - block.Height + 1;
                var size = columns * rows;
                if (previous.Length < size)
                {
                    previous = new int[size];
                    step = new Direction[size];
                }

                for (var i = 0; i < size; i++)
                {
                    previous[i] = Unvisited;
                }

                order.Clear();
                order.Add(block.Position);
                previous[IndexOf(block.Position)] = Start;

                // Each hop is a whole straight slide, so paths have as few turns as possible and play back naturally.
                for (var head = 0; head < order.Count; head++)
                {
                    var position = order[head];
                    var positionIndex = IndexOf(position);
                    foreach (var direction in Directions.All)
                    {
                        if (!block.MovesAlong(direction))
                        {
                            continue;
                        }

                        var offset = direction.ToOffset();
                        for (var next = position + offset; ; next += offset)
                        {
                            var index = IndexOf(next);
                            if (index < 0 || !board.CanPlace(block, next))
                            {
                                break;
                            }

                            if (previous[index] != Unvisited)
                            {
                                continue;
                            }

                            previous[index] = positionIndex;
                            step[index] = direction;
                            order.Add(next);
                        }
                    }
                }
            }

            /// <summary>Adds the path from the block's cell to <paramref name="target"/> as straight slides.</summary>
            public void AddPath(int blockId, GridPoint target, List<SolverMove> moves)
            {
                path.Clear();
                for (var index = IndexOf(target); previous[index] != Start; index = previous[index])
                {
                    path.Add((PositionOf(index), step[index]));
                }

                for (var i = path.Count - 1; i >= 0; i--)
                {
                    moves.Add(new SolverMove(blockId, path[i].to, path[i].direction, exits: false));
                }
            }

            private int IndexOf(GridPoint position)
            {
                var column = position.X - originX;
                var row = position.Y - originY;
                return column < 0 || row < 0 || column >= columns || row >= rows ? -1 : row * columns + column;
            }

            private GridPoint PositionOf(int index)
            {
                return new GridPoint(index % columns + originX, index / columns + originY);
            }
        }
    }
}
