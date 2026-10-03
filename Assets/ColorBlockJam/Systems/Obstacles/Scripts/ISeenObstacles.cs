namespace ColorBlockJam.Obstacles
{
    public interface ISeenObstacles
    {
        bool IsSeen(ObstacleDefinition obstacle);

        void MarkSeen(ObstacleDefinition obstacle);
    }
}
