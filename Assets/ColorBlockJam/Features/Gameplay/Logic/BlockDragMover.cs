using System;
using System.Numerics;
using ColorBlockJam.Level;

namespace ColorBlockJam.Gameplay.Logic
{
    /// <summary>
    /// Moves a dragged block continuously, in cell units, the way a real object pushed across a table moves:
    /// it never overlaps a wall or another block, it slides along whatever it rubs against, and it rolls around
    /// corners on a curve instead of catching on them.
    ///
    /// The math works on the block's position. Every whole-cell offset where the block would not fit is an obstacle
    /// for that position: a 2×2 square around the offset with rounded corners, which is what two touching cells with
    /// slightly rounded corners look like to each other. Neighboring obstacles overlap by a whole cell, so a flat wall
    /// stays perfectly flat, with no seams to snag on, and only real corners are round. Blocks keep half a percent of a
    /// cell of play, so one always slides into a gap exactly its own size.
    ///
    /// A move is swept: its whole path is tested at once and stops at the first contact, so a fast drag never passes
    /// through anything. The rest of the move is then turned along the touched surface and swept again. Repeated a few
    /// times, that is sliding along a wall, and rolling around a rounded corner, within one frame.
    /// </summary>
    public sealed class BlockDragMover
    {
        // Each pass either moves the block or ends the move; eight are enough to round a few corners in one frame.
        private const int SlidePasses = 8;

        // Times one sweep is turned along a surface. Two surfaces use up both directions in 2D; the third is slack.
        private const int DeflectSteps = 3;

        // Less movement than this, in cells, counts as standing still.
        private const float MinProgress = 0.0005f;

        // A block stops this far short of a surface, so float error never leaves it inside.
        private const float Skin = 0.0001f;

        // Deeper than this inside an obstacle can only come from float error on another path: the obstacle is then
        // ignored so the block can get out instead of locking up.
        private const float EscapeDepth = 0.02f;

        // A motion this close to a touched surface's direction slides along it rather than pushing into it.
        private const float ParallelTolerance = 1e-6f;

        // Two cells overlap while their positions are less than one cell apart on both axes. A sliver of play, far
        // too small to see, keeps a gap exactly one block wide a real channel rather than a line float error cannot hit.
        private const float Play = 0.005f;
        private const float HalfSize = 1f - Play;

        private readonly float radius;
        private readonly float inner;

        /// <param name="cornerRounding">
        /// Radius, in cells, of the rounded corners between the block and what it touches, up to 0.45.
        /// Bigger rolls around corners more widely; 0 keeps sharp corners.
        /// </param>
        public BlockDragMover(float cornerRounding)
        {
            radius = Math.Clamp(cornerRounding, 0f, 0.45f);
            inner = HalfSize - radius;
        }

        /// <summary>
        /// Moves the block from <paramref name="from"/> as far toward <paramref name="to"/> as it can go,
        /// sliding along what it touches. Returns where it ends up.
        /// </summary>
        public Vector2 Move(Board board, BoardBlock block, Vector2 from, Vector2 to)
        {
            var position = from;
            for (var pass = 0; pass < SlidePasses; pass++)
            {
                // Each pass aims straight at the target again, so a corner that was rounded is left behind at once.
                var remaining = AlongAxis(block, to - position);
                if (remaining.LengthSquared() < MinProgress * MinProgress)
                {
                    break;
                }

                var landed = SweepAndSlide(board, block, position, remaining);
                if (Vector2.DistanceSquared(landed, position) < MinProgress * MinProgress)
                {
                    break;
                }

                position = landed;
            }

            return position;
        }

        /// <summary>
        /// Sweeps the motion; when it is stopped at once, the part pushing into the surface is dropped and the rest
        /// swept again. On a flat face that slides along the face; on a rounded corner the surface faces diagonally,
        /// so the block moves around the corner.
        /// </summary>
        private Vector2 SweepAndSlide(Board board, BoardBlock block, Vector2 from, Vector2 motion)
        {
            for (var step = 0; step < DeflectSteps; step++)
            {
                var time = Sweep(board, block, from, motion, out var normal);
                var landed = from + motion * time;
                if (Vector2.DistanceSquared(landed, from) >= MinProgress * MinProgress)
                {
                    return landed;
                }

                if (normal == Vector2.Zero)
                {
                    break;
                }

                motion = AlongAxis(block, motion - normal * Vector2.Dot(motion, normal));
                if (motion.LengthSquared() < MinProgress * MinProgress)
                {
                    break;
                }
            }

            return from;
        }

        /// <summary>
        /// How much of <paramref name="motion"/>, from 0 to 1, the block travels before it touches something,
        /// and the normal of the surface it touches (zero when nothing is in the way).
        /// </summary>
        private float Sweep(Board board, BoardBlock block, Vector2 from, Vector2 motion, out Vector2 normal)
        {
            normal = Vector2.Zero;
            var time = 1f;

            // Only obstacles within one cell of the path can be touched.
            var minX = (int)MathF.Floor(MathF.Min(from.X, from.X + motion.X)) - 1;
            var maxX = (int)MathF.Ceiling(MathF.Max(from.X, from.X + motion.X)) + 1;
            var minY = (int)MathF.Floor(MathF.Min(from.Y, from.Y + motion.Y)) - 1;
            var maxY = (int)MathF.Ceiling(MathF.Max(from.Y, from.Y + motion.Y)) + 1;

            for (var x = minX; x <= maxX; x++)
            {
                for (var y = minY; y <= maxY; y++)
                {
                    if (!IsObstacle(board, block, x, y) ||
                        !Cast(from - new Vector2(x, y), motion, out var hitTime, out var hitNormal) || hitTime >= time)
                    {
                        continue;
                    }

                    time = hitTime;
                    normal = hitNormal;
                }
            }

            if (time < 1f)
            {
                time = MathF.Max(0f, time - Skin / motion.Length());
            }

            return time;
        }

        /// <summary>The part of a motion an arrow block may make: only along its axis. Other blocks keep all of it.</summary>
        private static Vector2 AlongAxis(BoardBlock block, Vector2 motion)
        {
            return block.Axis switch
            {
                BlockAxis.Horizontal => new Vector2(motion.X, 0f),
                BlockAxis.Vertical => new Vector2(0f, motion.Y),
                _ => motion
            };
        }

        /// <summary>True when the block placed at the whole cell (x, y) would cover a wall or another block.</summary>
        private static bool IsObstacle(Board board, BoardBlock block, int x, int y)
        {
            foreach (var cell in block.Cells)
            {
                if (!board.IsOpenFor(block, x + cell.X, y + cell.Y))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// When a point starting at <paramref name="origin"/>, relative to an obstacle's center, first enters the
        /// obstacle while moving by <paramref name="motion"/>; the obstacle is a rounded square, so it is convex.
        /// </summary>
        private bool Cast(Vector2 origin, Vector2 motion, out float time, out Vector2 normal)
        {
            time = 0f;
            var distance = Distance(origin, out normal);
            if (distance <= Skin)
            {
                // Touching: pushing into the surface stops at once, moving along it or away is free. The small
                // allowance keeps float error in a slide along the surface from reading as a push into it.
                return distance >= -EscapeDepth && Vector2.Dot(motion, normal) < -ParallelTolerance;
            }

            // The rounded square sits inside the plain one; enter that first.
            if (!EnterSquare(origin, motion, out var enter, out normal))
            {
                return false;
            }

            var hit = origin + motion * enter;
            if (MathF.Abs(hit.X) > inner && MathF.Abs(hit.Y) > inner)
            {
                // A corner: the rounded square's surface there is a quarter circle.
                var center = new Vector2(WithSign(inner, hit.X), WithSign(inner, hit.Y));
                if (!EnterCircle(origin - center, motion, out enter))
                {
                    return false;
                }

                normal = Vector2.Normalize(origin + motion * enter - center);
            }

            time = enter;
            return true;
        }

        /// <summary>
        /// Signed distance from a point, relative to an obstacle's center, to the obstacle's surface (negative inside),
        /// and the surface normal nearest to it.
        /// </summary>
        private float Distance(Vector2 point, out Vector2 normal)
        {
            var overX = MathF.Abs(point.X) - inner;
            var overY = MathF.Abs(point.Y) - inner;
            if (overX > 0f && overY > 0f)
            {
                var length = MathF.Sqrt(overX * overX + overY * overY);
                normal = new Vector2(WithSign(overX, point.X), WithSign(overY, point.Y)) / length;
                return length - radius;
            }

            if (overX > overY)
            {
                normal = new Vector2(WithSign(1f, point.X), 0f);
                return overX - radius;
            }

            normal = new Vector2(0f, WithSign(1f, point.Y));
            return overY - radius;
        }

        /// <summary>Where a moving point enters the plain square around the obstacle, and through which side.</summary>
        private static bool EnterSquare(Vector2 origin, Vector2 motion, out float enter, out Vector2 normal)
        {
            enter = 0f;
            normal = Vector2.Zero;
            var exit = 1f;

            for (var axis = 0; axis < 2; axis++)
            {
                var start = axis == 0 ? origin.X : origin.Y;
                var speed = axis == 0 ? motion.X : motion.Y;
                if (MathF.Abs(speed) < 1e-9f)
                {
                    // Moving along this side: on or outside it, the square is never entered.
                    if (MathF.Abs(start) >= HalfSize)
                    {
                        return false;
                    }

                    continue;
                }

                var near = (-WithSign(HalfSize, speed) - start) / speed;
                var far = (WithSign(HalfSize, speed) - start) / speed;
                if (near > enter)
                {
                    enter = near;
                    normal = axis == 0 ? new Vector2(-MathF.Sign(speed), 0f) : new Vector2(0f, -MathF.Sign(speed));
                }

                exit = MathF.Min(exit, far);
                if (enter > exit)
                {
                    return false;
                }
            }

            return exit > 0f && enter <= 1f;
        }

        /// <summary>When a moving point, relative to a corner circle's center, first enters the circle.</summary>
        private bool EnterCircle(Vector2 origin, Vector2 motion, out float enter)
        {
            enter = 0f;
            var a = Vector2.Dot(motion, motion);
            var b = Vector2.Dot(origin, motion);
            var c = Vector2.Dot(origin, origin) - radius * radius;
            if (c <= 0f)
            {
                return b < 0f;
            }

            var discriminant = b * b - a * c;
            if (discriminant < 0f || b >= 0f)
            {
                return false;
            }

            enter = (-b - MathF.Sqrt(discriminant)) / a;
            return enter <= 1f;
        }

        /// <summary><paramref name="magnitude"/>, which is not negative, with the sign of <paramref name="sign"/>.</summary>
        private static float WithSign(float magnitude, float sign)
        {
            return sign < 0f ? -magnitude : magnitude;
        }
    }
}
