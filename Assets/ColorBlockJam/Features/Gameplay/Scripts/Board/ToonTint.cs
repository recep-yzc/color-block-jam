using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// Colors a renderer drawn with the toon shader through a property block, so every block and door shares one
    /// material instead of a copy per color. The shader reads the tint per instance, so GPU instancing keeps each
    /// renderer's own color.
    /// </summary>
    internal static class ToonTint
    {
        private static readonly int TintId = Shader.PropertyToID("_Tint");

        public static void Apply(Renderer renderer, Color color)
        {
            var block = new MaterialPropertyBlock();
            block.SetColor(TintId, color);
            renderer.SetPropertyBlock(block);
        }
    }
}
