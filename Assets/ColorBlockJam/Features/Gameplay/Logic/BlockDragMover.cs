using System;
using System.Numerics;

namespace ColorBlockJam.Gameplay.Logic
{
    /// <summary>
    /// Moves a dragged block continuously, in cell units, toward where the finger wants it,
    /// without ever overlapping a wall or another block.
    /// When the block is blocked on one axis but close to lining up with a gap, it is eased into line
    /// while it keeps moving, so it rounds the beveled corners of other blocks on a curved path instead of stopping.
    /// </summary>
    public sealed class BlockDragMover
    {
        // Longest sub-step, in cells, so fast drags cannot pass through a single cell.
        private const float MaxStep = 0.1f;
        private const float Epsilon = 0.001f;
        private const int ContactSearchSteps = 6;

        private readonly float cornerAssist;
        private readonly float assistRate;

        /// <param name="cornerAssist">How far out of line, in cells, the block may be to still be eased around a corner.</param>
        /// <param name="assistRate">Cells of easing per cell of movement; higher rounds corners more tightly.</param>
        public BlockDragMover(float cornerAssist, float assistRate)
        {
            this.cornerAssist = cornerAssist;
            this.assistRate = assistRate;
        }

        public Vector2 Move(Board board, BoardBlock block, Vector2 from, Vector2 to)
        {
            // Steps that only ease the block into line do not move it forward, so keep stepping
            // until the target is reached or nothing moves any more.
            var maxIterations = 4 * Math.Max(1, (int)MathF.Ceiling((to - from).Length() / MaxStep));
            var position = from;

            for (var i = 0; i < maxIterations; i++)
            {
                var remaining = to - position;
                if (remaining.LengthSquared() < Epsilon * Epsilon)
                {
                    break;
                }

                var scale = MathF.Min(1f, MaxStep / remaining.Length());
                var before = position;
                position = MoveAxis(board, block, position, remaining.X * scale, isHorizontal: true);
                position = MoveAxis(board, block, position, remaining.Y * scale, isHorizontal: false);

                if (Vector2.DistanceSquared(before, position) < 1e-8f)
                {
                    break;
                }
            }

            return position;
        }

        /// <summary>True when every cell the block covers at <paramref name="position"/> is open for it.</summary>
        public static bool Fits(Board board, BoardBlock block, Vector2 position)
        {
            foreach (var cell in block.Cells)
            {
                var left = position.X + cell.X;
                var bottom = position.Y + cell.Y;
                var x0 = (int)MathF.Floor(left + Epsilon);
                var x1 = (int)MathF.Floor(left + 1f - Epsilon);
                var y0 = (int)MathF.Floor(bottom + Epsilon);
                var y1 = (int)MathF.Floor(bottom + 1f - Epsilon);

                for (var x = x0; x <= x1; x++)
                {
                    for (var y = y0; y <= y1; y++)
                    {
                        if (!board.IsOpenFor(block, x, y))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private Vector2 MoveAxis(Board board, BoardBlock block, Vector2 position, float distance, bool isHorizontal)
        {
            if (MathF.Abs(distance) < 1e-6f)
            {
                return position;
            }

            var moved = Offset(position, distance, isHorizontal);
            if (Fits(board, block, moved))
            {
                return moved;
            }

            // Blocked: if the other axis is almost lined up with a gap, ease it into line while moving.
            var across = isHorizontal ? position.Y : position.X;
            var gap = MathF.Round(across) - across;
            if (MathF.Abs(gap) > Epsilon && MathF.Abs(gap) <= cornerAssist)
            {
                var ease = MathF.Sign(gap) * MathF.Min(MathF.Abs(gap), MathF.Abs(distance) * assistRate);

                var rounding = Offset(Offset(position, ease, !isHorizontal), distance, isHorizontal);
                if (Fits(board, block, rounding))
                {
                    return rounding;
                }

                var aligning = Offset(position, ease, !isHorizontal);
                if (Fits(board, block, aligning))
                {
                    return aligning;
                }
            }

            return SlideToContact(board, block, position, distance, isHorizontal);
        }

        private static Vector2 SlideToContact(Board board, BoardBlock block, Vector2 position, float distance, bool isHorizontal)
        {
            var reachable = 0f;
            var blocked = 1f;
            for (var i = 0; i < ContactSearchSteps; i++)
            {
                var mid = (reachable + blocked) * 0.5f;
                if (Fits(board, block, Offset(position, distance * mid, isHorizontal)))
                {
                    reachable = mid;
                }
                else
                {
                    blocked = mid;
                }
            }

            return Offset(position, distance * reachable, isHorizontal);
        }

        private static Vector2 Offset(Vector2 position, float distance, bool isHorizontal)
        {
            return isHorizontal
                ? new Vector2(position.X + distance, position.Y)
                : new Vector2(position.X, position.Y + distance);
        }
    }
}
