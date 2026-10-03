using System;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    internal static class MeshTint
    {
        public static void Paint(Mesh mesh, Color color, int count, Color restColor)
        {
            var colors = new Color[mesh.vertexCount];
            Array.Fill(colors, color.linear, 0, count);
            Array.Fill(colors, restColor.linear, count, colors.Length - count);
            mesh.SetColors(colors);
        }
    }
}
