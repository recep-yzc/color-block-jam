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
            var angle = (signX, signY) switch
            {
                ( > 0, > 0) => 0f,
                ( < 0, > 0) => 90f,
                ( < 0, _) => 180f,
                _ => 270f
            };
            return Quaternion.Euler(0f, -angle, 0f);
        }

        public static Quaternion TurnToSide(int signX, int signY)
        {
            var angle = (signX, signY) switch
            {
                (_, > 0) => 0f,
                ( < 0, _) => 90f,
                (_, < 0) => 180f,
                _ => 270f
            };
            return Quaternion.Euler(0f, -angle, 0f);
        }
    }
}
