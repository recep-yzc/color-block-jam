using System.Collections.Generic;
using ColorBlockJam.Level;

namespace ColorBlockJam.Obstacles
{
    public static class ObstacleRules
    {
        public static List<ObstacleDefinition> Pending(IReadOnlyList<ObstacleDefinition> obstacles, LevelData level, ISeenObstacles seen)
        {
            var pending = new List<ObstacleDefinition>();
            foreach (var obstacle in obstacles)
            {
                if (obstacle.AppearsIn(level) && !seen.IsSeen(obstacle))
                {
                    pending.Add(obstacle);
                }
            }

            return pending;
        }
    }
}
