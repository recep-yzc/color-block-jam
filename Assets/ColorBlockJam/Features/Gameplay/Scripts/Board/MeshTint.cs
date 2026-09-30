using System;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// Writes a color into every vertex of a built mesh. Blocks and doors are each their own mesh anyway, so carrying
    /// the color in the mesh lets all of them share one material, which the SRP Batcher draws cheaply, and needs no
    /// material copy or property block per color.
    /// </summary>
    internal static class MeshTint
    {
        public static void Paint(Mesh mesh, Color color)
        {
            // Vertex colors reach the shader as they are, unlike material colors, so they are stored linear.
            var colors = new Color[mesh.vertexCount];
            Array.Fill(colors, color.linear);
            mesh.SetColors(colors);
        }
    }
}
