using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBlockJam.Level
{
    [CreateAssetMenu(menuName = "Color Block Jam/Level/Level Catalog", fileName = "LevelCatalog")]
    public sealed class LevelCatalog : ScriptableObject
    {
        [Tooltip("Oynanış sırasıyla seviye JSON dosyaları. Son seviyeden sonra oyun baştan başlar.")]
        [SerializeField] private TextAsset[] levels = Array.Empty<TextAsset>();

        public int Count => levels.Length;
        public IReadOnlyList<TextAsset> Levels => levels;

        public LevelData Load(int levelNumber)
        {
            var index = (levelNumber - 1) % levels.Length;
            return LevelSerializer.FromJson(levels[index].text);
        }
    }
}
