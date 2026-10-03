using ColorBlockJam.Level;
using UnityEngine;

namespace ColorBlockJam.Obstacles
{
    [CreateAssetMenu(menuName = "Color Block Jam/Obstacles/Arrow Block", fileName = "ArrowBlock")]
    public sealed class ArrowBlockObstacle : ObstacleDefinition
    {
        public override bool AppearsIn(LevelData level)
        {
            foreach (var block in level.blocks)
            {
                if (block.axis != BlockAxis.Free)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
