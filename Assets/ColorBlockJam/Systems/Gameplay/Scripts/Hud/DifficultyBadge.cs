using System;
using ColorBlockJam.Level;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    [Serializable]
    public struct DifficultyBadge
    {
        [Tooltip("Rozetin gösterildiği zorluk.")]
        public LevelDifficulty difficulty;
        [Tooltip("Rozette yazan ad.")]
        public string label;
        [Tooltip("Rozetin rengi.")]
        public Color color;
    }
}
