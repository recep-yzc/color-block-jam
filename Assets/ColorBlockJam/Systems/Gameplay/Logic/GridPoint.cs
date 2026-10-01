using System;

namespace ColorBlockJam.Gameplay.Logic
{
    public readonly struct GridPoint : IEquatable<GridPoint>
    {
        public readonly int X;
        public readonly int Y;

        public GridPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static GridPoint operator +(GridPoint a, GridPoint b) => new(a.X + b.X, a.Y + b.Y);
        public static GridPoint operator -(GridPoint a, GridPoint b) => new(a.X - b.X, a.Y - b.Y);
        public static bool operator ==(GridPoint a, GridPoint b) => a.Equals(b);
        public static bool operator !=(GridPoint a, GridPoint b) => !a.Equals(b);

        public bool Equals(GridPoint other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is GridPoint other && Equals(other);
        public override int GetHashCode() => (X * 397) ^ Y;
        public override string ToString() => $"({X}, {Y})";
    }
}
