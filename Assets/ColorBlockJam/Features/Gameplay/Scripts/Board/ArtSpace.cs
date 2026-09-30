using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// Rules shared by the builders: the art is made for a 2-unit cell, and block modules are modeled
    /// on the XY plane with their top toward -Z.
    /// </summary>
    internal static class ArtSpace
    {
        public const float CellSize = 2f;
        public const float QuarterSize = CellSize / 2f;

        /// <summary>Half a block's height, in cells: a block stands about 0.8 of a cell tall.</summary>
        public const float BlockHalfHeight = 0.4f;

        /// <summary>Turns a module modeled on the XY plane so it lies on the ground with its top up.</summary>
        public static readonly Quaternion LayFlat = Quaternion.Euler(90f, 0f, 0f);

        /// <summary>Turn around the vertical axis that maps +X +Y of the model to the given diagonal.</summary>
        public static Quaternion TurnToDiagonal(int signX, int signY)
        {
            float angle;
            if (signX > 0 && signY > 0) angle = 0f;
            else if (signX < 0 && signY > 0) angle = 90f;
            else if (signX < 0) angle = 180f;
            else angle = 270f;

            // Counterclockwise on the board is a negative turn around world up.
            return Quaternion.Euler(0f, -angle, 0f);
        }

        /// <summary>Turn around the vertical axis that maps the model's +Y side to the given side.</summary>
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
