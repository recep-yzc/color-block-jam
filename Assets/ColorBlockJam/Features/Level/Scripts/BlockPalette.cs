using System;
using UnityEngine;

namespace ColorBlockJam.Level
{
    /// <summary>
    /// The block and door colors. Levels store a color as an index into this list.
    /// </summary>
    [CreateAssetMenu(menuName = "Color Block Jam/Level/Block Palette", fileName = "BlockPalette")]
    public sealed class BlockPalette : ScriptableObject
    {
        [Serializable]
        private struct Entry
        {
            public string name;
            public Color color;
        }

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
