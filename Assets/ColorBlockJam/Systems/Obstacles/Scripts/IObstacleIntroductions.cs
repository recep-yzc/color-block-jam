namespace ColorBlockJam.Obstacles
{
    public interface IObstacleIntroductions
    {
        bool IsIntroduced(ObstacleDefinition obstacle);

        void MarkIntroduced(ObstacleDefinition obstacle);
    }
}
