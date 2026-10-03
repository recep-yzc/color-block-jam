using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBlockJam.Obstacles
{
    [CreateAssetMenu(menuName = "Color Block Jam/Obstacles/Obstacle Catalog", fileName = "ObstacleCatalog")]
    public sealed class ObstacleCatalog : ScriptableObject
    {
        [Tooltip("Tanıtılan engeller. Bir seviyede birden fazla yeni engel varsa popup'lar bu sırayla çıkar.")]
        [SerializeField] private ObstacleDefinition[] obstacles = Array.Empty<ObstacleDefinition>();

        public IReadOnlyList<ObstacleDefinition> Obstacles => obstacles;
    }
}
