using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    internal static class ArtSpace
    {
        public const float CellSize = 2f;
        public const float QuarterSize = CellSize / 2f;
        public const float BlockHalfHeight = 0.4f;

        public static readonly Quaternion LayFlat = Quaternion.Euler(90f, 0f, 0f);

        public static Quaternion TurnToDiagonal(int signX, int signY)
        {
            float angle;
            if (signX > 0 && signY > 0) angle = 0f;
            else if (signX < 0 && signY > 0) angle = 90f;
            else if (signX < 0) angle = 180f;
            else angle = 270f;

            return Quaternion.Euler(0f, -angle, 0f);
        }

        public static Quaternion TurnToSide(int signX, int signY)
        {
            float angle;
            if (signY > 0) angle = 0f;
            else if (signX < 0) angle = 90f;
            else if (signY < 0) angle = 180f;
            else angle = 270f;

            return Quaternion.Euler(0f, -angle, 0f);
        }
    }
}
