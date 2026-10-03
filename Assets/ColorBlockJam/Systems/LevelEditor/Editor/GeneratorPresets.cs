using System;
using ColorBlockJam.Level;
using ColorBlockJam.LevelEditor.Authoring;
using UnityEngine;

namespace ColorBlockJam.LevelEditor
{
    [CreateAssetMenu(menuName = "Color Block Jam/Level/Generator Presets", fileName = "GeneratorPresets")]
    public sealed class GeneratorPresets : ScriptableObject
    {
        [Tooltip("Level Editor'daki Generate'in her zorluk için kullandığı ayarlar. Listede olmayan bir zorluk koddaki varsayılanları kullanır.")]
        [SerializeField] private GeneratorPreset[] presets = Defaults();

        public GeneratorSettings SettingsFor(LevelDifficulty difficulty)
        {
            foreach (var preset in presets)
            {
                if (preset.difficulty == difficulty)
                {
                    return preset.ToSettings();
                }
            }

            return GeneratorSettings.For(difficulty);
        }

        public bool Covers(LevelDifficulty difficulty)
        {
            foreach (var preset in presets)
            {
                if (preset.difficulty == difficulty)
                {
                    return true;
                }
            }

            return false;
        }

        private void Reset()
        {
            presets = Defaults();
        }

        private static GeneratorPreset[] Defaults()
        {
            var difficulties = (LevelDifficulty[])Enum.GetValues(typeof(LevelDifficulty));
            return Array.ConvertAll(difficulties, difficulty => GeneratorPreset.From(GeneratorSettings.For(difficulty)));
        }
    }
}
