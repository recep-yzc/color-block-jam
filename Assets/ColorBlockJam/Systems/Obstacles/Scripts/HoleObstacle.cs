using ColorBlockJam.Level;
using UnityEngine;

namespace ColorBlockJam.Obstacles
{
    [CreateAssetMenu(menuName = "Color Block Jam/Obstacles/Hole", fileName = "Hole")]
    public sealed class HoleObstacle : ObstacleDefinition
    {
        public override bool AppearsIn(LevelData level)
        {
            return level.holes.Length > 0;
        }
    }
}
