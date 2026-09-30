using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBlockJam.Level
{
    /// <summary>
    /// The playable levels in order. Each entry is a JSON file written by the level editor.
    /// After the last level the game starts again from the first one.
    /// </summary>
    [CreateAssetMenu(menuName = "Color Block Jam/Level/Level Catalog", fileName = "LevelCatalog")]
    public sealed class LevelCatalog : ScriptableObject
    {
        [Tooltip("Oynanış sırasıyla seviye JSON dosyaları. Son seviyeden sonra oyun baştan başlar.")]
        [SerializeField] private TextAsset[] levels = Array.Empty<TextAsset>();

        public int Count => levels.Length;
        public IReadOnlyList<TextAsset> Levels => levels;

        /// <param name="levelNumber">1-based level number; numbers past the last level wrap around.</param>
        public LevelData Load(int levelNumber)
        {
            var index = (levelNumber - 1) % levels.Length;
            return LevelSerializer.FromJson(levels[index].text);
        }
    }
}
