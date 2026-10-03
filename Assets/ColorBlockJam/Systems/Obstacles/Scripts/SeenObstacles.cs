using ColorBlockJam.Core.Persistence;

namespace ColorBlockJam.Obstacles
{
    public sealed class SeenObstacles : ISeenObstacles
    {
        private readonly IKeyValueStorage storage;

        public SeenObstacles(IKeyValueStorage storage)
        {
            this.storage = storage;
        }

        public bool IsSeen(ObstacleDefinition obstacle)
        {
            return storage.GetBool(SeenKey(obstacle), false);
        }

        public void MarkSeen(ObstacleDefinition obstacle)
        {
            storage.SetBool(SeenKey(obstacle), true);
        }

        private static string SeenKey(ObstacleDefinition obstacle)
        {
            return "obstacles." + obstacle.Id + ".seen";
        }
    }
}
