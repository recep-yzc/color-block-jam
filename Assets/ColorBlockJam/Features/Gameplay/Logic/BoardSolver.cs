using System.Collections.Generic;

namespace ColorBlockJam.Gameplay.Logic
{
    /// <summary>One straight slide of a block, or a slide out through its door.</summary>
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
        public SolveResult(bool isSolved, bool isExhausted, IReadOnlyList<SolverMove> moves, int exploredStates)
        {
            IsSolved = isSolved;
            IsExhausted = isExhausted;
            Moves = moves;
            ExploredStates = exploredStates;
        }

        public bool IsSolved { get; }

        /// <summary>True when every reachable state was searched, so an unsolved result is certain.</summary>
        public bool IsExhausted { get; }

        public IReadOnlyList<SolverMove> Moves { get; }
        public int ExploredStates { get; }

        /// <summary>No sequence of moves clears the board from here.</summary>
        public bool IsStuck => !IsSolved && IsExhausted;
    }

    /// <summary>
    /// Breadth-first search over block positions. A move slides one block in a straight line; any drag path
    /// is a chain of such slides. Leaving through a door is always taken as soon as it is possible, because
    /// removing a block can only free space, which keeps the search small.
    /// Used by the level editor to validate and generate levels, by stuck detection and by auto play.
    /// The board is left in the state it had before the call.
    /// </summary>
    public sealed class BoardSolver
    {
        private sealed class Node
        {
            public GridPoint[] Positions;
            public bool[] Cleared;
            public int Parent;
            public SolverMove Move;
        }

        public SolveResult Solve(Board board, int maxStates)
        {
            var blocks = board.Blocks;
            var count = blocks.Count;
            var start = new Node { Positions = new GridPoint[count], Cleared = new bool[count], Parent = -1 };
            for (var i = 0; i < count; i++)
            {
                start.Positions[i] = blocks[i].Position;
                start.Cleared[i] = blocks[i].IsCleared;
            }

            var nodes = new List<Node> { start };
            var visited = new HashSet<string> { Key(start) };
            var queue = new Queue<int>();
            queue.Enqueue(0);
            var isSolved = false;
            var solvedIndex = -1;
            var isOverBudget = false;
            var children = new List<Node>();

            while (queue.Count > 0)
            {
                var index = queue.Dequeue();
                var node = nodes[index];
                board.SetState(node.Positions, node.Cleared);

                if (board.IsCleared)
                {
                    isSolved = true;
                    solvedIndex = index;
                    break;
                }

                Expand(board, node, index, children);
                foreach (var child in children)
                {
                    if (!visited.Add(Key(child)))
                    {
                        continue;
                    }

                    nodes.Add(child);
                    queue.Enqueue(nodes.Count - 1);
                }

                if (nodes.Count >= maxStates)
                {
                    isOverBudget = true;
                    break;
                }
            }

            board.SetState(start.Positions, start.Cleared);

            var moves = new List<SolverMove>();
            for (var i = solvedIndex; i > 0; i = nodes[i].Parent)
            {
                moves.Add(nodes[i].Move);
            }

            moves.Reverse();
            var isExhausted = !isSolved && !isOverBudget;
            return new SolveResult(isSolved, isExhausted, moves, nodes.Count);
        }

        private static void Expand(Board board, Node node, int index, List<Node> children)
        {
            children.Clear();

            foreach (var block in board.Blocks)
            {
                if (block.IsCleared)
                {
                    continue;
                }

                foreach (var direction in Directions.All)
                {
                    var offset = direction.ToOffset();
                    for (var step = 1; ; step++)
                    {
                        var target = block.Position + offset * step;
                        if (board.CanPlace(block, target))
                        {
                            children.Add(Child(node, index, block.Id, target, direction, exits: false));
                            continue;
                        }

                        if (board.CanPassThrough(block, target - offset, direction))
                        {
                            // Leaving is never a bad move, so it is the only one taken from this state.
                            children.Clear();
                            children.Add(Child(node, index, block.Id, target, direction, exits: true));
                            return;
                        }

                        break;
                    }
                }
            }
        }

        private static Node Child(Node parent, int parentIndex, int blockId, GridPoint target, Direction direction, bool exits)
        {
            var positions = (GridPoint[])parent.Positions.Clone();
            var cleared = (bool[])parent.Cleared.Clone();
            positions[blockId] = target;
            cleared[blockId] = exits;

            return new Node
            {
                Positions = positions,
                Cleared = cleared,
                Parent = parentIndex,
                Move = new SolverMove(blockId, target, direction, exits)
            };
        }

        private static string Key(Node node)
        {
            var chars = new char[node.Positions.Length * 3];
            for (var i = 0; i < node.Positions.Length; i++)
            {
                chars[i * 3] = (char)(node.Positions[i].X + 64);
                chars[i * 3 + 1] = (char)(node.Positions[i].Y + 64);
                chars[i * 3 + 2] = node.Cleared[i] ? '1' : '0';
            }

            return new string(chars);
        }
    }
}
