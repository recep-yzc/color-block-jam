using System.Collections.Generic;

namespace ColorBlockJam.Gameplay.Logic
{
    internal sealed class ReachMap
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
            var startIndex = IndexOf(block.Position);
            if (startIndex < 0)
            {
                return;
            }

            order.Add(block.Position);
            previous[startIndex] = Start;

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
