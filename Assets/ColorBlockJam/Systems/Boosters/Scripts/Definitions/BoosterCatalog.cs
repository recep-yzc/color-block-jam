using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBlockJam.Boosters
{
    [CreateAssetMenu(menuName = "Color Block Jam/Boosters/Booster Catalog", fileName = "BoosterCatalog")]
    public sealed class BoosterCatalog : ScriptableObject
    {
        [Tooltip("Oyundaki booster'lar, bardaki sırasıyla. Yeni bir booster eklemek için tanım asset'ini buraya koymak yeter.")]
        [SerializeField] private BoosterDefinition[] boosters = Array.Empty<BoosterDefinition>();

        public IReadOnlyList<BoosterDefinition> Boosters => boosters;
    }
}
