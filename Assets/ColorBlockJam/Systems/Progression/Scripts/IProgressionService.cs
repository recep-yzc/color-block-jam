namespace ColorBlockJam.Progression
{
    public interface IProgressionService
    {
        int CurrentLevel { get; }

        void CompleteCurrentLevel();
    }
}
