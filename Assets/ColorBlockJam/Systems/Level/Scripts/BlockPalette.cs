using System;
using UnityEngine;

namespace ColorBlockJam.Level
{
    [CreateAssetMenu(menuName = "Color Block Jam/Level/Block Palette", fileName = "BlockPalette")]
    public sealed class BlockPalette : ScriptableObject
    {
        [Serializable]
        private struct Entry
        {
            [Tooltip("Rengin level editöründe görünen adı.")]
            public string name;
            [Tooltip("Bu renkteki blokların ve kapıların rengi.")]
            public Color color;
        }

        [Tooltip("Blok ve kapı renkleri. Seviyeler rengi bu listedeki sırasıyla tutar.")]
        [SerializeField] private Entry[] entries = Array.Empty<Entry>();

        public int Count => entries.Length;

        public Color GetColor(int index)
        {
            return entries[index].color;
        }

        public string GetName(int index)
        {
            return entries[index].name;
        }
    }
}
