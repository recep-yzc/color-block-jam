using System;
using ColorBlockJam.Level;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// One material per palette color, made from a base material on first use.
    /// Separate materials, unlike property blocks, keep the SRP Batcher working.
    /// </summary>
    public sealed class ColorMaterials : IDisposable
    {
        private readonly Material baseMaterial;
        private readonly BlockPalette palette;
        private readonly Material[] materials;

        public ColorMaterials(Material baseMaterial, BlockPalette palette)
        {
            this.baseMaterial = baseMaterial;
            this.palette = palette;
            materials = new Material[palette.Count];
        }

        public Material Get(int color)
        {
            if (materials[color] == null)
            {
                materials[color] = new Material(baseMaterial)
                {
                    name = $"{baseMaterial.name} ({palette.GetName(color)})",
                    color = palette.GetColor(color)
                };
            }

            return materials[color];
        }

        public void Dispose()
        {
            foreach (var material in materials)
            {
                if (material != null)
                {
                    Object.Destroy(material);
                }
            }
        }
    }
}
