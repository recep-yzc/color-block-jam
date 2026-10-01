using System;
using System.Numerics;
using ColorBlockJam.Level;

namespace ColorBlockJam.Gameplay.Logic
{
    public sealed class BlockDragMover
    {
        private const int SlidePasses = 8;
        private const int DeflectSteps = 3;
        private const float MinProgress = 0.0005f;
        private const float Skin = 0.0001f;
        private const float EscapeDepth = 0.02f;
        private const float ParallelTolerance = 1e-6f;
        private const float Play = 0.005f;
        private const float HalfSize = 1f - Play;

        private readonly float radius;
        private readonly float inner;

        public BlockDragMover(float cornerRounding)
        {
            radius = Math.Clamp(cornerRounding, 0f, 0.45f);
            inner = HalfSize - radius;
        }

        public Vector2 Move(Board board, BoardBlock block, Vector2 from, Vector2 to)
        {
            var position = from;
            for (var pass = 0; pass < SlidePasses; pass++)
            {
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

        private float Sweep(Board board, BoardBlock block, Vector2 from, Vector2 motion, out Vector2 normal)
        {
            normal = Vector2.Zero;
            var time = 1f;

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

        private static Vector2 AlongAxis(BoardBlock block, Vector2 motion)
        {
            return block.Axis switch
            {
                BlockAxis.Horizontal => new Vector2(motion.X, 0f),
                BlockAxis.Vertical => new Vector2(0f, motion.Y),
                _ => motion
            };
        }

        private static bool IsObstacle(Board board, BoardBlock block, int x, int y)
        {
            return !board.CanOccupy(block, x, y);
        }

        private bool Cast(Vector2 origin, Vector2 motion, out float time, out Vector2 normal)
        {
            time = 0f;
            var distance = Distance(origin, out normal);
            if (distance <= Skin)
            {
                return distance >= -EscapeDepth && Vector2.Dot(motion, normal) < -ParallelTolerance;
            }

            if (!EnterSquare(origin, motion, out var enter, out normal))
            {
                return false;
            }

            var hit = origin + motion * enter;
            if (MathF.Abs(hit.X) > inner && MathF.Abs(hit.Y) > inner)
            {
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

        private static float WithSign(float magnitude, float sign)
        {
            return sign < 0f ? -magnitude : magnitude;
        }
    }
}
