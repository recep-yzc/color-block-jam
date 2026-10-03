using System.Collections.Generic;
using ColorBlockJam.Level;

namespace ColorBlockJam.Obstacles
{
    public static class ObstacleRules
    {
        public static List<ObstacleDefinition> Pending(IReadOnlyList<ObstacleDefinition> obstacles, LevelData level,
            IObstacleIntroductions introductions)
        {
            var pending = new List<ObstacleDefinition>();
            foreach (var obstacle in obstacles)
            {
                if (Appears(obstacle.Kind, level) && !introductions.IsIntroduced(obstacle))
                {
                    pending.Add(obstacle);
                }
            }

            return pending;
        }

        public static bool Appears(ObstacleKind kind, LevelData level)
        {
            if (kind == ObstacleKind.Hole)
            {
                return level.holes.Length > 0;
            }

            foreach (var block in level.blocks)
            {
                if (kind == ObstacleKind.ArrowBlock ? block.axis != BlockAxis.Free : block.ice > 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
