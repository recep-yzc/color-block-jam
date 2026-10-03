using ColorBlockJam.Level;
using UnityEngine;

namespace ColorBlockJam.Obstacles
{
    [CreateAssetMenu(menuName = "Color Block Jam/Obstacles/Ice", fileName = "Ice")]
    public sealed class IceObstacle : ObstacleDefinition
    {
        public override bool AppearsIn(LevelData level)
        {
            foreach (var block in level.blocks)
            {
                if (block.ice > 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
