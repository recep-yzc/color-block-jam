using ColorBlockJam.Core.Persistence;

namespace ColorBlockJam.Obstacles
{
    public sealed class ObstacleIntroductions : IObstacleIntroductions
    {
        private readonly IKeyValueStorage storage;

        public ObstacleIntroductions(IKeyValueStorage storage)
        {
            this.storage = storage;
        }

        public bool IsIntroduced(ObstacleDefinition obstacle)
        {
            return storage.GetBool(IntroducedKey(obstacle), false);
        }

        public void MarkIntroduced(ObstacleDefinition obstacle)
        {
            storage.SetBool(IntroducedKey(obstacle), true);
        }

        private static string IntroducedKey(ObstacleDefinition obstacle)
        {
            return $"obstacles.{obstacle.Id}.introduced";
        }
    }
}
